using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace regexFinder
{
    public sealed class CheckResult
    {
        public string CheckName { get; init; }
        public int Row { get; init; }
        public string Key { get; init; }
        public string Message { get; init; }
        public bool Passed { get; init; }
    }

    public static class CsvValidator
    {
        public static List<Dictionary<string, string>> Load(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length == 0) return new();

            var rows = lines.Select(ParseLine).ToList();
            var headers = rows[0];
            if (headers.Count != headers.Distinct(StringComparer.Ordinal).Count())
                throw new InvalidDataException("CSV contains duplicate column names.");

            return rows.Skip(1)
                .Select((values, index) =>
                {
                    if (values.Count != headers.Count)
                        throw new InvalidDataException($"CSV row {index + 2} has {values.Count} columns; expected {headers.Count}.");
                    return headers.Select((header, i) => (header, value: values[i]))
                        .ToDictionary(x => x.header, x => x.value, StringComparer.Ordinal);
                })
                .ToList();
        }

        public static List<CheckResult> Run(
            IReadOnlyList<Dictionary<string, string>> rows,
            IReadOnlyList<CheckDefinition> checks)
        {
            var results = new List<CheckResult>();
            foreach (var check in checks)
            {
                switch ((check.Type ?? string.Empty).Trim().ToLowerInvariant())
                {
                    case "required":
                        RunRequired(rows, check, results);
                        break;
                    case "comparison":
                        RunComparison(rows, check, results);
                        break;
                    case "hashsequence":
                        RunHashSequence(rows, check, results);
                        break;
                    case "sequence":
                        RunSequence(rows, check, results);
                        break;
                    default:
                        results.Add(Fail(check, 0, "", $"Unknown test type: {check.Type}"));
                        break;
                }
            }
            return results;
        }

        private static void RunRequired(
            IReadOnlyList<Dictionary<string, string>> rows,
            CheckDefinition check,
            List<CheckResult> results)
        {
            for (var i = 0; i < rows.Count; i++)
            {
                var value = Get(rows[i], check.Field);
                if (string.IsNullOrWhiteSpace(value))
                    results.Add(Fail(check, i + 2, GetKey(rows[i]), $"'{check.Field}' is empty."));
            }
        }

        private static void RunComparison(
            IReadOnlyList<Dictionary<string, string>> rows,
            CheckDefinition check,
            List<CheckResult> results)
        {
            for (var i = 0; i < rows.Count; i++)
            {
                var actualText = Get(rows[i], check.Left);
                if (!TryNumber(actualText, out var actual))
                {
                    results.Add(Fail(check, i + 2, GetKey(rows[i]), $"'{check.Left}' is not a number."));
                    continue;
                }

                var expected = 0d;
                var missing = new List<string>();
                foreach (var field in check.Right ?? new())
                {
                    var text = Get(rows[i], field);
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    if (!TryNumber(text, out var value))
                        missing.Add(field);
                    else
                        expected += value;
                }

                if (missing.Count > 0)
                {
                    results.Add(Fail(check, i + 2, GetKey(rows[i]), $"Non-numeric fields: {string.Join(", ", missing)}."));
                    continue;
                }

                if (Math.Abs(actual - expected) > check.Tolerance)
                    results.Add(Fail(check, i + 2, GetKey(rows[i]), $"{check.Left}={actual:0.00}; expected {expected:0.00}."));
            }
        }

        private static void RunHashSequence(
            IReadOnlyList<Dictionary<string, string>> rows,
            CheckDefinition check,
            List<CheckResult> results)
        {
            var ordered = OrderRows(rows, check.OrderBy);
            for (var i = 1; i < ordered.Count; i++)
            {
                var previous = Get(ordered[i - 1].row, check.PreviousField);
                var current = Get(ordered[i].row, check.CurrentField);
                if (!string.Equals(previous, current, StringComparison.OrdinalIgnoreCase))
                    results.Add(Fail(check, ordered[i].index + 2, GetKey(ordered[i].row),
                        $"'{check.CurrentField}' does not match previous '{check.PreviousField}'."));
            }
        }

        private static void RunSequence(
            IReadOnlyList<Dictionary<string, string>> rows,
            CheckDefinition check,
            List<CheckResult> results)
        {
            var ordered = OrderRows(rows, check.OrderBy);
            for (var i = 1; i < ordered.Count; i++)
            {
                if (!TryNumber(Get(ordered[i - 1].row, check.Field), out var previous) ||
                    !TryNumber(Get(ordered[i].row, check.Field), out var current))
                {
                    results.Add(Fail(check, ordered[i].index + 2, GetKey(ordered[i].row), $"'{check.Field}' is not numeric."));
                    continue;
                }

                if (Math.Abs((current - previous) - check.Step) > check.Tolerance)
                    results.Add(Fail(check, ordered[i].index + 2, GetKey(ordered[i].row),
                        $"Sequence break: {previous:0.##} -> {current:0.##}."));
            }
        }

        private static List<(Dictionary<string, string> row, int index)> OrderRows(
            IReadOnlyList<Dictionary<string, string>> rows, string orderBy)
        {
            return rows.Select((row, index) => (row, index))
                .OrderBy(x => TryNumber(Get(x.row, orderBy), out var number) ? number : double.MaxValue)
                .ThenBy(x => Get(x.row, orderBy), StringComparer.Ordinal)
                .ToList();
        }

        private static List<string> ParseLine(string line)
        {
            var result = new List<string>();
            var value = new StringBuilder();
            var quoted = false;
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (quoted && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        value.Append('"');
                        i++;
                    }
                    else quoted = !quoted;
                }
                else if (c == ',' && !quoted)
                {
                    result.Add(value.ToString());
                    value.Clear();
                }
                else value.Append(c);
            }
            result.Add(value.ToString());
            return result;
        }

        private static bool TryNumber(string value, out double number) =>
            double.TryParse(value?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

        private static string Get(Dictionary<string, string> row, string field) =>
            field != null && row.TryGetValue(field, out var value) ? value : string.Empty;

        private static string GetKey(Dictionary<string, string> row) =>
            Get(row, "Ceka numurs");

        private static CheckResult Fail(CheckDefinition check, int row, string key, string message) => new()
        {
            CheckName = check.Name,
            Row = row,
            Key = key,
            Message = message,
            Passed = false
        };
    }
}
