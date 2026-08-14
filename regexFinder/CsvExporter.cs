using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    string safeRow = row.Replace("\"", "\"\"");
                    writer.WriteLine($"\"{safeRow}\"");
                }
            }
        }

        public void ExportResultToCsv(List<List<string>> results, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var row in results)
                    writer.WriteLine(string.Join(",", row.Select(EscapeCsvField)));
            }
        }

        private static string EscapeCsvField(string value)
        {
            value ??= string.Empty;
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
