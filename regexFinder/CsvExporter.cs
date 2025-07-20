using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace regexFinder
{
    internal class CsvExporter
    {
        public void ExportToCsv(List<string> rows, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var row in rows)
                {
                    // Escape quotes and commas if needed
                    string safeRow = row.Replace("\"", "\"\"");
                    writer.WriteLine($"\"{safeRow}\"");
                }
            }
        }

        public void ExportResultToCsv(List<List<string>> results, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write header
                //writer.WriteLine(string.Join(",", data.Keys));

                // Find the max number of rows

                foreach (var list in results) 
                { 
                    writer.WriteLine(string.Join(",", list));
                }
            }
        }
    }
}
