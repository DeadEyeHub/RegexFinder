using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace regexFinder
{
    public static class CsvInjector
    {
        public static int Inject(string masterPath, string correctedPath, string outputPath)
        {
            var master = CsvValidator.LoadDocument(masterPath);
            var corrected = CsvValidator.LoadDocument(correctedPath);
            const string keyField = "Ceka numurs";

            ValidateHeaders(master.Headers, corrected.Headers);
            var masterRows = IndexRows(master.Rows, keyField, "master CSV");
            var correctedRows = IndexRows(corrected.Rows, keyField, "corrected CSV");
            var missing = correctedRows.Keys.Where(key => !masterRows.ContainsKey(key)).ToList();
            if (missing.Count > 0)
                throw new InvalidDataException($"Keys not found in master CSV: {string.Join(", ", missing)}.");

            var output = new List<List<string>> { master.Headers };
            foreach (var row in master.Rows)
            {
                var key = row[keyField];
                var replacement = correctedRows.TryGetValue(key, out var correctedRow) ? correctedRow : row;
                output.Add(master.Headers.Select(header => replacement[header]).ToList());
            }

            new CsvExporter().ExportResultToCsv(output, outputPath);
            return correctedRows.Count;
        }

        private static Dictionary<string, Dictionary<string, string>> IndexRows(
            IReadOnlyList<Dictionary<string, string>> rows, string keyField, string source)
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var key = row.TryGetValue(keyField, out var value) ? value?.Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidDataException($"{source} contains an empty '{keyField}'.");
                if (!result.TryAdd(key, row))
                    throw new InvalidDataException($"{source} contains duplicate key '{key}'.");
            }
            return result;
        }

        private static void ValidateHeaders(IReadOnlyList<string> master, IReadOnlyList<string> corrected)
        {
            if (!master.SequenceEqual(corrected, StringComparer.Ordinal))
                throw new InvalidDataException("Master and corrected CSV headers do not match.");
        }
    }
}
