using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using static System.Windows.Forms.LinkLabel;

namespace regexFinder
{
    internal class RegexFinder
    {
        public List<string> Lines { get; set; }
        public List<string> Patterns { get;  set; }

        public List<string> SplitText(string text)
        {
            List<string> result = new List<string>();
            using (StringReader reader = new StringReader(text))
            {
                string line;
                try
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        result.Add(line);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading lines: {ex.Message}");
                    result.Add(string.Empty);
                }
            }
            return result;
        }

        public Dictionary<string, List<string>> FindAllMatches(NotificationProgress nf)
        {
            var results = new Dictionary<string, List<string>>();

            int totalProgress = Patterns.Count * Lines.Count;
            int progress = 0;

            int patternNumber = 0;
            foreach (var pattern in Patterns)
            {
                patternNumber++;

                var matches = new List<string>();
                var regex = new Regex(pattern);

                int lineNumber = 0;
                foreach (var line in Lines)
                {
                    Debug.WriteLine($"Processing line {++lineNumber} {patternNumber}");
                    nf.SetProgress(++progress, totalProgress);
                    var matchCollection = regex.Matches(line);
                    if (matchCollection.Count > 1) { 
                        var matchValues = new List<string>();
                        var resultsValues = new List<string>();
                        foreach (Match match in matchCollection)
                        {
                            matchValues.Add(match.Value.Trim().Replace('.', ','));
                        }
                        resultsValues.Add("=" + string.Join("+", matchValues));
                        matches.Add(string.Join(" ", resultsValues));
                        continue;
                    }
                    else if (matchCollection.Count == 0)
                    {
                        matches.Add(string.Empty);
                        continue;
                    } else if (matchCollection.Count == 1)
                    {
                        matches.Add(matchCollection[0].Value.Trim().Replace('.', ','));
                        continue;
                    }
                }
            
                results[pattern] = matches;
            }

            return results;
        }


    }
}