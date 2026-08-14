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

        public List<List<string>> FindAlChecks(NotificationProgress nf)
        {
            var results = new List<List<string>>();

            var splitterP = Patterns.FirstOrDefault(p => p.Name == "Splitter")
                ?? throw new Exception("Splitter pattern not found");
            var splitterRx = splitterP.CompiledRegex
                ?? throw new Exception("Splitter regex not compiled or empty");

            var patternsToApply = Patterns.Where(p => p.Name != "Splitter").ToList();

            var headers = patternsToApply.Select(p => p.Name).ToList();
            results.Add(headers);

            int checkBegin = -1;
            int total = Lines.Count;

            for (int i = 0; i < Lines.Count; i++)
            {
                nf.SetProgress(i + 1, total);
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
            var spans = FindBlocksInRange(Lines, checkStart, checkEnd, Blocks);

            if (spans.Count == 0)
                spans.Add(("__whole__", checkStart, checkEnd));

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (blockName, start, end) in spans)
            {
                var pats = patternsToApply;

                var blockValues = ProcessPatternsOnRange(Lines, start, end, pats);

                foreach (var kv in blockValues)
                    values[kv.Key] = kv.Value;
            }

            ValidateComparisons(values, patternsToApply);

            return values;
        }

        private List<(string Name, int Start, int End)> FindBlocksInRange(
            List<string> lines, int s, int e, List<BlockDefinition> blocks)
        {
            var result = new List<(string, int, int)>();
            if (blocks == null || blocks.Count == 0) return result;

            foreach (var b in blocks)
            {
                if (b == null || b.StartsRegex == null || b.EndsRegex == null) continue;

                int start = -1, end = -1;

                for (int i = s; i <= e; i++)
                {
                    if (b.StartsRegex.IsMatch(lines[i])) { start = i; break; }
                }
                if (start < 0) continue;

                for (int i = start; i <= e; i++)
                {
                    if (b.EndsRegex.IsMatch(lines[i])) { end = i; break; }
                }
                if (end < 0) continue;

                result.Add((b.Name ?? $"block_{result.Count + 1}", start, end));
            }

            return result;
        }

        private Dictionary<string, string> ProcessPatternsOnRange(
            List<string> allLines, int startIdx, int endIdx, List<PatternDefinition> patterns)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (patterns == null || patterns.Count == 0) return values;

            int s = Math.Max(0, startIdx);
            int e = Math.Min(allLines.Count - 1, endIdx);
            if (e < s) return values;

            foreach (var p in patterns)
            {
                var rx = p?.CompiledRegex;
                if (rx == null) { values[p?.Name ?? $"field_{values.Count + 1}"] = string.Empty; continue; }

                var found = new List<string>();

                if (p.Multiline && p.LinesCount > 1)
                {
                    int L = p.LinesCount;
                    for (int i = s; i + L - 1 <= e; i++)
                    {
                        string combined = string.Join(" ",
                            Enumerable.Range(0, L)
                                      .Select(k => allLines[i + k].Replace(',', '.').Trim()));

                        foreach (Match m in rx.Matches(combined))
                        {
                            var v = m.Groups.Count > 1 ? m.Groups[1].Value : m.Value;
                            v = (v ?? "").Trim().Replace(',', '.');
                            if (v.Length > 0) found.Add(v);
                        }
                    }
                }
                else
                {
                    for (int i = s; i <= e; i++)
                    {
                        var text = allLines[i].Replace(',', '.').Trim();
                        foreach (Match m in rx.Matches(text))
                        {
                            var v = m.Groups.Count > 1 ? m.Groups[1].Value : m.Value;
                            v = (v ?? "").Trim().Replace(',', '.');
                            if (v.Length > 0) found.Add(v);
                        }
                    }
                }

                var keyName = p.Name ?? $"field_{values.Count + 1}";
                switch ((p.CombineMethod ?? "first").Trim().ToLowerInvariant())
                {
                    case "merge":
                        values[keyName] = string.Join("; ", found.Distinct());
                        break;

                    case "sum":
                        var sum = found
                            .Select(ParseDouble)
                            .Where(d => d.HasValue)
                            .Sum(d => d!.Value);
                        values[keyName] = sum.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                        break;

                    case "none":
                    case "first":
                        values[keyName] = found.FirstOrDefault() ?? string.Empty;
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unknown combineMethod '{p.CombineMethod}' for pattern '{keyName}'.");
                }
            }

            return values;
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
