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

        public void ExportDictionaryToCsv(Dictionary<string, List<string>> data, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write header
                writer.WriteLine(string.Join(",", data.Keys));

                // Find the max number of rows
                int maxRows = 0;
                foreach (var list in data.Values)
                    if (list.Count > maxRows) maxRows = list.Count;

                // Write rows
                for (int i = 0; i < maxRows; i++)
                {
                    var row = new List<string>();
                    foreach (var key in data.Keys)
                    {
                        if (i < data[key].Count)
                            row.Add($"\"{data[key][i].Replace("\"", "\"\"")}\"");
                        else
                            row.Add("");
                    }
                    writer.WriteLine(string.Join(",", row));
                }
            }
        }
    }
}
