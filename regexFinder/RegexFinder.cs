using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace regexFinder
{
    internal class RegexFinder
    {
        public List<string> Lines { get; private set; }
        public List<string> Patterns { get; private set; }

        public RegexFinder()
        {
            Lines = new List<string>();
            Patterns = new List<string>();
        }

        public List<string> SplitText(string text)
        {
            Lines = new List<string>();
            using (StringReader reader = new StringReader(text))
            {
                string line;
                try
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        Lines.Add(line);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading lines: {ex.Message}");
                    Lines.Add(string.Empty);
                }
                return Lines;
            }
        }
        public Dictionary<string, List<string>> FindAllMatches()
        {
            var results = new Dictionary<string, List<string>>();

            foreach (var pattern in Patterns)
            {
                var matches = new List<string>();
                var regex = new Regex(pattern);

                foreach (var line in Lines)
                {
                    foreach (Match match in regex.Matches(line))
                    {
                        matches.Add(match.Value.Trim());
                    }
                }
                results[pattern] = matches;
            }

            return results;
        }

        public List<string> FindAndFormatNumbers(string pattern)
        {
            var formattedResults = new List<string>();
            var regex = new Regex(pattern);

            foreach (var line in Lines)
            {
                var matches = regex.Matches(line);
                var formattedNumbers = new List<string>();

                foreach (Match match in matches)
                {
                    // Trim and replace '.' with ',')
                    string number = match.Value.Trim().Replace('.', ',');
                    formattedNumbers.Add(number);
                }

                if (formattedNumbers.Count > 0)
                {
                    // Join all numbers with '+')
                    formattedResults.Add("=" + string.Join("+", formattedNumbers));
                }
                else
                {
                    formattedResults.Add(string.Empty);
                }
            }

            return formattedResults;
        }
    }
}