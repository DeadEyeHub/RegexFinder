using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace regexFinder
{
    public sealed class CsvDocument
    {
        public List<string> Headers { get; init; } = new();
        public List<Dictionary<string, string>> Rows { get; init; } = new();
    }

    public sealed class CheckResult
    {
        public string CheckName { get; init; }
        public int Row { get; init; }
        public string Key { get; init; }
        public string Message { get; init; }
        public bool Passed { get; init; }
        public List<string> RelatedKeys { get; init; } = new();
    }

    public static class CsvValidator
    {
        public static List<Dictionary<string, string>> Load(string path)
            => LoadDocument(path).Rows;

        public static CsvDocument LoadDocument(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length == 0) return new();

            var rows = lines.Select(ParseLine).ToList();
            var headers = rows[0];
            if (headers.Count != headers.Distinct(StringComparer.Ordinal).Count())
                throw new InvalidDataException("CSV contains duplicate column names.");

            var data = rows.Skip(1)
                .Select((values, index) =>
                {
                    if (values.Count != headers.Count)
                        throw new InvalidDataException($"CSV row {index + 2} has {values.Count} columns; expected {headers.Count}.");
                    return headers.Select((header, i) => (header, value: values[i]))
                        .ToDictionary(x => x.header, x => x.value, StringComparer.Ordinal);
                })
                .ToList();
            return new CsvDocument { Headers = headers, Rows = data };
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
                    case "grandtotalreconciliation":
                        RunGrandTotal(rows, check, results);
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
                if (IsCancelled(rows[i], check))
                    continue;

                if ((check.IgnoreReceiptTypes ?? new()).Contains(Get(rows[i], "Ceka tips"), StringComparer.OrdinalIgnoreCase))
                    continue;

                var selectedFields = new[] { check.Left }
                    .Concat(check.Right ?? new())
                    .Concat(check.Subtract ?? new())
                    .Where(field => !string.IsNullOrWhiteSpace(field))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                var emptyFields = selectedFields.Where(field => string.IsNullOrWhiteSpace(Get(rows[i], field))).ToList();
                var actualText = Get(rows[i], check.Left);
                if (string.IsNullOrWhiteSpace(actualText))
                    continue;

                if (!TryNumber(actualText, out var actual))
                {
                    var details = emptyFields.Count > 0
                        ? $" Empty fields: {string.Join(", ", emptyFields)}."
                        : string.Empty;
                    results.Add(Fail(check, i + 2, GetKey(rows[i]), $"'{check.Left}' is not a number.{details}"));
                    continue;
                }

                var expected = 0d;
                var addendCount = 0;
                var missing = new List<string>();
                foreach (var field in check.Right ?? new())
                {
                    var text = Get(rows[i], field);
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    if (!TryNumber(text, out var value))
                        missing.Add(field);
                    else
                    {
                        expected += value;
                        addendCount++;
                    }
                }

                foreach (var field in check.Subtract ?? new())
                {
                    var text = Get(rows[i], field);
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    if (!TryNumber(text, out var value))
                        missing.Add(field);
                    else
                        expected -= value;
                }

                if (missing.Count > 0)
                {
                    var emptyDetails = emptyFields.Count > 0
                        ? $" Empty fields: {string.Join(", ", emptyFields)}."
                        : string.Empty;
                    results.Add(Fail(check, i + 2, GetKey(rows[i]),
                        $"Non-numeric fields: {string.Join(", ", missing)}.{emptyDetails}"));
                    continue;
                }

                if (addendCount == 0)
                    continue;

                if (Math.Abs(actual - expected) > check.Tolerance)
                {
                    var emptyDetails = emptyFields.Count > 0
                        ? $" Empty fields: {string.Join(", ", emptyFields)}."
                        : string.Empty;
                    results.Add(Fail(check, i + 2, GetKey(rows[i]),
                        $"{check.Left}={actual:0.00}; expected {expected:0.00}.{emptyDetails}"));
                }
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

                var expected = previous + check.Step;
                if (Math.Abs(current - expected) > check.Tolerance)
                    results.Add(Fail(check, ordered[i].index + 2, GetKey(ordered[i].row),
                        $"Current {check.Field}={current:0.##}; previous {previous:0.##}, expected {expected:0.##}."));
            }
        }

        private static void RunGrandTotal(
            IReadOnlyList<Dictionary<string, string>> rows,
            CheckDefinition check,
            List<CheckResult> results)
        {
            if (string.IsNullOrWhiteSpace(check.ReceiptTypeField))
            {
                results.Add(Fail(check, 0, "", "Receipt type field is not configured."));
                return;
            }
            if (rows.Count > 0 && !rows[0].ContainsKey(check.ReceiptTypeField))
            {
                results.Add(Fail(check, 0, "", $"Receipt type field '{check.ReceiptTypeField}' does not exist in CSV."));
                return;
            }
            if (string.IsNullOrWhiteSpace(check.AmountField) ||
                (rows.Count > 0 && !rows[0].ContainsKey(check.AmountField)))
            {
                results.Add(Fail(check, 0, "", $"Amount field '{check.AmountField}' does not exist in CSV."));
                return;
            }

            var checkpointStart = 0;
            var accumulated = 0d;
            var checkpoints = 0;

            for (var i = 0; i < rows.Count; i++)
            {
                if (IsCancelled(rows[i], check))
                    continue;

                var receiptType = Get(rows[i], check.ReceiptTypeField);
                var checkpointTypes = check.CheckpointReceiptTypes ?? new();
                var isCheckpoint = checkpointTypes.Count > 0
                    ? checkpointTypes.Contains(receiptType, StringComparer.OrdinalIgnoreCase)
                    : TryNumber(Get(rows[i], check.TotalField), out _);
                if (!isCheckpoint) continue;

                checkpoints++;
                var hasTotal = TryNumber(Get(rows[i], check.AmountField), out var total);

                var rangeRows = rows.Skip(checkpointStart).Take(i - checkpointStart).ToList();
                var rangeKeys = rangeRows.Select(GetKey).Where(key => !string.IsNullOrWhiteSpace(key)).ToList();
                var values = new List<double>();
                var invalidRows = new List<string>();

                foreach (var row in rangeRows)
                {
                    if (IsCancelled(row, check)) continue;
                    if (!IsIncludedReceipt(row, check)) continue;
                    if (IsExcludedReceipt(row, check)) continue;
                    var key = GetKey(row);
                    var amount = Get(row, check.AmountField);
                    if (!TryNumber(amount, out var value))
                    {
                        if (!string.IsNullOrWhiteSpace(key)) invalidRows.Add(key);
                        continue;
                    }
                    values.Add(value);
                }

                if (invalidRows.Count > 0)
                {
                    results.Add(Fail(check, i + 2, GetKey(rows[i]),
                        $"Non-numeric {check.AmountField} in checks: {string.Join(", ", invalidRows)}.", rangeKeys));
                }

                var rangeTotal = values.Sum();
                accumulated += rangeTotal;
                var expected = check.Cumulative ? accumulated : rangeTotal;
                if (!hasTotal)
                {
                    results.Add(Fail(check, i + 2, GetKey(rows[i]),
                        $"Checkpoint '{check.AmountField}' is empty or non-numeric.", rangeKeys));
                }
                else if (Math.Abs(total - expected) > check.Tolerance)
                {
                    results.Add(Fail(check, i + 2, GetKey(rows[i]),
                        $"{check.AmountField}={total:0.00}; expected {expected:0.00}; range {rangeTotal:0.00}.", rangeKeys));
                }

                checkpointStart = i + 1;
            }

            if (checkpoints == 0)
            {
                var expectedTypes = (check.CheckpointReceiptTypes ?? new()).Count > 0
                    ? string.Join(", ", check.CheckpointReceiptTypes)
                    : $"numeric values in '{check.TotalField}'";
                results.Add(Fail(check, 0, "",
                    $"No checkpoint rows found in receipt type field '{check.ReceiptTypeField}' for: {expectedTypes}."));
            }
        }

        private static bool IsIncludedReceipt(Dictionary<string, string> row, CheckDefinition check)
        {
            if (check.IncludedReceiptTypes == null || check.IncludedReceiptTypes.Count == 0) return true;
            return check.IncludedReceiptTypes.Contains(Get(row, check.ReceiptTypeField), StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsExcludedReceipt(Dictionary<string, string> row, CheckDefinition check)
        {
            if (!check.ExcludeIfNonZero || string.IsNullOrWhiteSpace(check.ExcludeIfField)) return false;
            var value = Get(row, check.ExcludeIfField);
            return TryNumber(value, out var number) ? Math.Abs(number) > check.Tolerance : !string.IsNullOrWhiteSpace(value);
        }

        private static bool IsCancelled(Dictionary<string, string> row, CheckDefinition check)
        {
            if (check == null || !check.SkipCancelledReceipts ||
                string.IsNullOrWhiteSpace(check.CancelledField)) return false;

            var value = Get(row, check.CancelledField);
            if (string.IsNullOrWhiteSpace(value)) return false;
            return TryNumber(value, out var number)
                ? Math.Abs(number) > 0.000001
                : true;
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

        private static CheckResult Fail(CheckDefinition check, int row, string key, string message, List<string> relatedKeys = null) => new()
        {
            CheckName = check.Name,
            Row = row,
            Key = key,
            Message = message,
            Passed = false,
            RelatedKeys = relatedKeys ?? (string.IsNullOrWhiteSpace(key) ? new() : new() { key })
        };
    }
}
