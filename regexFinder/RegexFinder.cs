using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static System.Windows.Forms.LinkLabel;

namespace regexFinder
{
    internal class RegexFinder
    {
        private CancellationToken _token;

        public RegexFinder(CancellationToken token)
        {
            _token = token;
        }

        public List<string> Lines { get; set; }
        public List<PatternDefinition> Patterns { get; set; }



        public List<List<string>> FindAllMatches(NotificationProgress nf)
        {
            var results = new List<List<string>>();
            var splitterPattern = Patterns.FirstOrDefault(p => p.Name == "Splitter");
            if (splitterPattern == null)
                throw new Exception("Splitter pattern not found");

            var splitter = new Regex(splitterPattern.RegexCommand);
            var patternsToApply = Patterns.Where(p => p.Name != "Splitter").ToList();

            // Заголовки — это .Name из всех паттернов, кроме Splitter
            var headers = patternsToApply.Select(p => p.Name).ToList();
            results.Add(headers);

            int checkBegin = -1;
            int total = Lines.Count;
            for (int i = 0; i < Lines.Count; i++)
            {
                nf.SetProgress(i + 1, total);
                if (_token.IsCancellationRequested)
                    break;

                var line = Lines[i];
                bool isLast = i == Lines.Count - 1;

                if (splitter.IsMatch(line) || isLast)
                {
                    if (checkBegin < 0)
                    {
                        checkBegin = i;
                        continue;
                    }

                    int checkEnd = isLast ? i : i - 1;
                    var checkLines = Lines.GetRange(checkBegin, checkEnd - checkBegin + 1);
                    var values = ProcessCheck(checkLines, patternsToApply);
                    var row = headers.Select(h => values.ContainsKey(h) ? values[h] : string.Empty).ToList();
                    results.Add(row);
                    checkBegin = isLast ? -1 : i;
                }
            }

            return results;
        }
        private Dictionary<string, string> ProcessCheck(List<string> checkLines, List<PatternDefinition> patterns)
        {
            var values = new Dictionary<string, string>();

            // Группируем строки по возможным StartsWith
            var groupedLines = new Dictionary<string, List<string>>();

            foreach (var line in checkLines)
            {
                var trimmedLine = line.TrimStart();
                bool matched = false;

                foreach (var pattern in patterns)
                {
                    if (pattern.StartsWith == null) continue;

                    foreach (var start in pattern.StartsWith)
                    {
                        if (!string.IsNullOrWhiteSpace(start) && trimmedLine.StartsWith(start))
                        {
                            if (!groupedLines.ContainsKey(start))
                                groupedLines[start] = new List<string>();

                            groupedLines[start].Add(line);
                            matched = true;
                            break;
                        }
                    }

                    if (matched) break;
                }

                if (!matched)
                {
                    if (!groupedLines.ContainsKey("*"))
                        groupedLines["*"] = new List<string>();

                    groupedLines["*"].Add(line);
                }
            }

            // Обработка каждого паттерна
            foreach (var pattern in patterns)
            {
                var regex = new Regex(pattern.RegexCommand);
                var foundValues = new List<string>();

                // Получаем релевантные строки
                List<string> relevantLines;

                if (pattern.StartsWith == null || pattern.StartsWith.Count == 0)
                {
                    relevantLines = checkLines;
                }
                else if (pattern.StartsWith.Count == 1)
                {
                    var key = pattern.StartsWith[0];
                    relevantLines = groupedLines.TryGetValue(key, out var lines) ? lines : new List<string>();
                }
                else
                {
                    relevantLines = new List<string>();
                    foreach (var start in pattern.StartsWith)
                    {
                        if (!string.IsNullOrWhiteSpace(start) && groupedLines.TryGetValue(start, out var lines))
                            relevantLines.AddRange(lines);
                    }
                }

                // Применяем паттерн к строкам
                int i = 0;
                while (i < relevantLines.Count)
                {
                    if (_token.IsCancellationRequested)
                        break;

                    string combinedText = relevantLines[i];

                    if (pattern.Multiline && pattern.LinesCount > 1 && i + pattern.LinesCount <= relevantLines.Count)
                    {
                        combinedText = string.Join(" ", relevantLines.GetRange(i, pattern.LinesCount));
                    }

                    combinedText = combinedText.Replace(',', '.');

                    foreach (Match match in regex.Matches(combinedText))
                    {
                        var value = match.Groups.Count > 1
                            ? match.Groups[1].Value.Trim().Replace(',', '.')
                            : match.Value.Trim().Replace(',', '.');

                        if (!foundValues.Contains(value))
                            foundValues.Add(value);
                    }

                    i++;
                }

                // Объединение/обработка найденных значений
                if (pattern.CombineMethod == "merge")
                {
                    values[pattern.Name] = string.Join("", foundValues.Distinct());
                }
                else if (pattern.CombineMethod == "sum")
                {
                    var floatValues = foundValues
                        .Select(v => v.Replace(',', '.'))
                        .Select(v => double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var f) ? f : (double?)null)
                        .Where(v => v.HasValue)
                        .Select(v => v.Value)
                        .ToList();

                    double? compareTarget = null;
                    if (!string.IsNullOrWhiteSpace(pattern.CompareTo))
                    {
                        foreach (var refName in pattern.CompareTo.Split(',').Select(n => n.Trim()))
                        {
                            if (values.TryGetValue(refName, out string refVal) &&
                                double.TryParse(refVal.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                            {
                                compareTarget = val;
                                break;
                            }
                        }
                    }

                    var sum = floatValues.Sum();
                    values[pattern.Name] = sum.ToString("0.00", CultureInfo.InvariantCulture);
                }
                else // none
                {
                    values[pattern.Name] = foundValues.FirstOrDefault();
                }
            }

            return values;
        }




    }
}

