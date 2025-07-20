using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public List<List<string>> FindAllMatches(List<Regex> regexList,  NotificationProgress nf)
        {
            var results = new List<List<string>>();



            int totalProgress = Lines.Count;
            int progress = 0;
            int patternNumber = 0;

            var splitter = regexList[0];

            var regexList2 = regexList.GetRange(1, regexList.Count - 1);
            var headers = new List<string>();

            foreach (var regex in regexList2)
            {
                headers.Add(regex.ToString());
            }
            results.Add(headers);

            int checkBegin = -1;
            int finalLine = Lines.Count - 1;
            for (int lineNumber = 0; lineNumber<Lines.Count; lineNumber++)
            {
                if (_token.IsCancellationRequested)
                {
            
                    break;
                }

                var line = Lines[lineNumber];

                nf.SetProgress(++progress, totalProgress);

                int lastCheckLine = 0;
                if (lineNumber == Lines.Count - 1) 
                {
                    lastCheckLine = lineNumber;
                }
                else if(splitter.IsMatch(line))
                {
                    if (checkBegin < 0)
                    {
                        checkBegin = lineNumber;
                        continue;
                    }

                    lastCheckLine = lineNumber - 1;
                }
                else
                {
                    continue;
                }

                var checkLines = Lines.GetRange(checkBegin, lastCheckLine - checkBegin+1);
                checkBegin = lastCheckLine + 1;

                var columms = proceessCheck(checkLines, regexList2);

                var list = new List<string>();
                foreach (var regex in regexList2)
                {
                    columms.TryGetValue(regex, out string value);
                    list.Add(value ?? string.Empty);
                }

                results.Add(list);


            }

            return results;


            foreach (var regex in regexList)
            {
                patternNumber++;

                var matches = new List<string>();

                foreach (var line in Lines)
                {
                    //Debug.WriteLine($"Processing line {++lineNumber}");
                    nf.SetProgress(++progress, totalProgress);

                    //int lineNumber = 0;
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
            
               // results[pattern] = matches;
            }

            return results;
        }

        private Dictionary<Regex, string> proceessCheck(List<string> checkLines, List<Regex> regexList)
        {
            Dictionary<Regex, string> columms = new Dictionary<Regex, string>();

            foreach (var line in checkLines)
            {
                foreach (var regex in regexList)
                {
                    var matches = new List<string>();
                    var matchCollection = regex.Matches(line);
                    var count = matchCollection.Count;
                    if ( count == 0)
                        continue;


                    columms.TryGetValue(regex, out string s);
                    for (int i = 0; i<count;  i++)
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            s+="_";
                        }
                        s+=matchCollection[i].Value.Trim().Replace('.', ',');
                    }

                    columms[regex] = s;
                    break; 
                }
            }

            return columms;
        }
    }
}

