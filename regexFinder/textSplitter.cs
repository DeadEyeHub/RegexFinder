using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace regexFinder
{
    internal class textSplitter
    {
        public List<string> Lines { get; private set; }

        List<string> SplitText(string text)
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
    }
}

