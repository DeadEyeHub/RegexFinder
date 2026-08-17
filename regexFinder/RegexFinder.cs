using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace regexFinder
{
    public class RegexFinder
    {
        public List<string> Lines { get; set; } = new();
        public List<PatternDefinition> Patterns { get; set; } = new();
        public List<BlockDefinition> Blocks { get; set; } = new();
        private readonly System.Threading.CancellationToken _token;

        public RegexFinder(System.Threading.CancellationToken token)
        {
            _token = token;
        }

        public List<List<string>> FindAlChecks(NotificationProgress nf = null)
        {
            var results = new List<List<string>>();

            var splitterP = Patterns.FirstOrDefault(p =>
                    string.Equals(p?.Name, "Splitter", StringComparison.OrdinalIgnoreCase))
                ?? throw new Exception("Splitter pattern not found");
            var splitterRx = splitterP.CompiledRegex
                ?? throw new Exception("Splitter regex not compiled or empty");

            var patternsToApply = Patterns
                .Where(p => !string.Equals(p?.Name, "Splitter", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var headers = patternsToApply.Select(p => p.Name).ToList();
            results.Add(headers);

            int checkBegin = -1;
            int total = Lines.Count;

            for (int i = 0; i < Lines.Count; i++)
            {
                nf?.SetProgress(i + 1, total);
                if (_token.IsCancellationRequested) break;

                bool isLast = (i == Lines.Count - 1);
                bool isSplit = splitterRx.IsMatch(Lines[i]) || isLast;

                if (checkBegin < 0)
                {
                    if (isSplit)
                    {
                        checkBegin = i;
                        if (isLast)
                        {
                            var values1 = ProcessCheckByBlocks(checkBegin, i, patternsToApply);
                            var row1 = headers.Select(h => values1.TryGetValue(h, out var v) ? v : string.Empty).ToList();
                            results.Add(row1);
                            checkBegin = -1;
                        }
                    }
                    continue;
                }

                if (!isSplit) continue;

                int checkEnd = isLast ? i : i - 1;
                if (checkEnd >= checkBegin)
                {
                    var values = ProcessCheckByBlocks(checkBegin, checkEnd, patternsToApply);
                    var row = headers.Select(h => values.TryGetValue(h, out var v) ? v : string.Empty).ToList();
                    results.Add(row);
                }

                checkBegin = isLast ? -1 : i;
            }

            return results;
        }

        private Dictionary<string, string> ProcessCheckByBlocks(
            int checkStart, int checkEnd,
            List<PatternDefinition> patternsToApply)
        {
            var blockSpans = FindBlocksInRange(Lines, checkStart, checkEnd, Blocks);
            var wholeCheck = (Name: "__whole__", Start: checkStart, End: checkEnd);
            var values = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var p in patternsToApply)
            {
                if (p == null) continue;

                var spans = string.IsNullOrWhiteSpace(p.BlockName)
                    ? new List<(string Name, int Start, int End)> { wholeCheck }
                    : blockSpans
                        .Where(span => string.Equals(span.Name, p.BlockName.Trim(), StringComparison.OrdinalIgnoreCase))
                        .ToList();

                var found = new List<string>();
                foreach (var span in spans)
                    found.AddRange(FindPatternValues(Lines, span.Start, span.End, p));

                var keyName = p.Name ?? $"field_{values.Count + 1}";
                values[keyName] = CombinePatternValues(p, found);
            }

            ValidateComparisons(values, patternsToApply);

            return values;
        }

        private List<(string Name, int Start, int End)> FindBlocksInRange(
            List<string> lines, int s, int e, List<BlockDefinition> blocks)
        {
            var result = new List<(string Name, int Start, int End)>();
            if (blocks == null || blocks.Count == 0) return result;

            foreach (var b in blocks)
            {
                if (b == null || b.StartsRegex == null || b.EndsRegex == null) continue;

                var cursor = s;
                while (cursor <= e)
                {
                    var start = -1;
                    for (var i = cursor; i <= e; i++)
                    {
                        if (b.StartsRegex.IsMatch(lines[i]))
                        {
                            start = i;
                            break;
                        }
                    }

                    if (start < 0) break;

                    var end = -1;
                    for (var i = start; i <= e; i++)
                    {
                        if (b.EndsRegex.IsMatch(lines[i]))
                        {
                            end = i;
                            break;
                        }
                    }

                    if (end < 0)
                    {
                        // A malformed receipt should still be processed up to its boundary.
                        end = e;
                        result.Add((b.Name ?? $"block_{result.Count + 1}", start, end));
                        break;
                    }

                    result.Add((b.Name ?? $"block_{result.Count + 1}", start, end));
                    cursor = Math.Max(start + 1, end + 1);
                }
            }

            return result
                .OrderBy(span => span.Start)
                .ThenBy(span => span.End)
                .ThenBy(span => span.Name, StringComparer.Ordinal)
                .ToList();
        }

        private List<string> FindPatternValues(
            List<string> allLines, int startIdx, int endIdx, PatternDefinition p)
        {
            var found = new List<string>();
            if (p == null || p.CompiledRegex == null) return found;

            int s = Math.Max(0, startIdx);
            int e = Math.Min(allLines.Count - 1, endIdx);
            if (e < s) return found;

            var rx = p.CompiledRegex;
            if (p.Multiline && p.LinesCount > 1)
            {
                int lineCount = p.LinesCount;
                for (int i = s; i + lineCount - 1 <= e; i++)
                {
                    string combined = string.Join(" ",
                        Enumerable.Range(0, lineCount)
                                  .Select(k => allLines[i + k].Trim()));

                    AddMatches(found, rx, combined);
                }
            }
            else
            {
                for (int i = s; i <= e; i++)
                {
                    AddMatches(found, rx, allLines[i].Trim());
                }
            }

            return found;
        }

        private static void AddMatches(List<string> found, Regex rx, string text)
        {
            foreach (Match m in rx.Matches(text))
            {
                var value = m.Groups.Count > 1 ? m.Groups[1].Value : m.Value;
                value = (value ?? string.Empty).Trim().Replace(',', '.');
                if (value.Length > 0) found.Add(value);
            }
        }

        private static string CombinePatternValues(PatternDefinition p, List<string> found)
        {
            var keyName = p.Name ?? "field";
            switch ((p.CombineMethod ?? "first").Trim().ToLowerInvariant())
            {
                case "merge":
                    return string.Join("; ", found.Distinct());

                case "sum":
                    var numericValues = found
                        .Select(ParseDouble)
                        .Where(d => d.HasValue)
                        .Select(d => d!.Value)
                        .ToList();
                    if (p.DistinctValues)
                        numericValues = numericValues.Distinct().ToList();

                    var sum = numericValues.Sum();
                    return numericValues.Count == 0
                        ? string.Empty
                        : sum.ToString("0.00", CultureInfo.InvariantCulture);

                case "none":
                case "first":
                    return found.FirstOrDefault() ?? string.Empty;

                case "last":
                    return found.LastOrDefault() ?? string.Empty;

                default:
                    throw new InvalidOperationException(
                        $"Unknown combineMethod '{p.CombineMethod}' for pattern '{keyName}'.");
            }
        }
        private void ValidateComparisons(Dictionary<string, string> values, List<PatternDefinition> patterns)
        {
            const double TOL = 0.02;

            foreach (var p in patterns)
            {
                if (string.IsNullOrWhiteSpace(p?.CompareTo)) continue;
                var name = p.Name;
                if (!values.TryGetValue(name, out var curStr)) continue;

                var cur = ParseDouble(curStr);
                if (cur is null) continue;

                var targets = p.CompareTo.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (targets.Length == 0) continue;

                double sum = 0;
                bool any = false;
                foreach (var t in targets)
                {
                    if (!values.TryGetValue(t, out var vstr)) continue;
                    var v = ParseDouble(vstr);
                    if (v.HasValue) { sum += v.Value; any = true; }
                }
                if (!any) continue;

                double denom = Math.Max(1.0, Math.Abs(sum));
                double relDiff = Math.Abs(cur.Value - sum) / denom;

                if (relDiff > TOL)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"CompareTo mismatch: {name}={cur.Value} vs sum({string.Join("+", targets)})={sum} (diff={relDiff:P1})");
                }
            }
        }

        private static double? ParseDouble(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Replace('\u00A0', ' ').Replace(',', '.').Trim();
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (double?)null;
        }
    }
}
