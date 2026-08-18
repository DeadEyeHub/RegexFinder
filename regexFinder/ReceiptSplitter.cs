using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace regexFinder
{
    internal static class ReceiptSplitter
    {
        public static List<(int Start, int End)> FindRanges(
            IReadOnlyList<string> lines,
            Regex splitter)
        {
            var ranges = new List<(int Start, int End)>();
            if (lines == null || splitter == null) return ranges;

            var start = -1;
            for (var i = 0; i < lines.Count; i++)
            {
                if (!splitter.IsMatch(lines[i])) continue;
                if (start >= 0) ranges.Add((start, i - 1));
                start = i;
            }

            if (start >= 0) ranges.Add((start, lines.Count - 1));
            return ranges;
        }
    }
}
