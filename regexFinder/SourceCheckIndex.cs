using System;
using System.Collections.Generic;
using System.Linq;

namespace regexFinder
{
    public sealed class SourceCheckIndex
    {
        private readonly Dictionary<string, List<string[]>> _blocks = new(StringComparer.OrdinalIgnoreCase);

        public SourceCheckIndex(IReadOnlyList<string> lines, IReadOnlyList<PatternDefinition> patterns)
        {
            var splitter = patterns.FirstOrDefault(p => string.Equals(p.Name, "Splitter", StringComparison.OrdinalIgnoreCase))?.CompiledRegex;
            var keyPattern = patterns.FirstOrDefault(p => string.Equals(p.Name, "Ceka numurs", StringComparison.OrdinalIgnoreCase))?.CompiledRegex;
            if (splitter == null || keyPattern == null) return;

            var current = new List<string>();
            foreach (var line in lines)
            {
                if (splitter.IsMatch(line) && current.Count > 0)
                {
                    AddBlock(current, keyPattern);
                    current = new List<string>();
                }
                current.Add(line);
            }
            if (current.Count > 0) AddBlock(current, keyPattern);
        }

        public IEnumerable<string[]> GetBlocks(IEnumerable<string> keys)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in keys ?? Enumerable.Empty<string>())
            {
                if (!seen.Add(key) || !_blocks.TryGetValue(key, out var blocks)) continue;
                foreach (var block in blocks) yield return block;
            }
        }

        private void AddBlock(List<string> lines, System.Text.RegularExpressions.Regex keyPattern)
        {
            var key = lines.SelectMany(line => keyPattern.Matches(line).Cast<System.Text.RegularExpressions.Match>())
                .Select(match => match.Groups.Count > 1 ? match.Groups[1].Value : match.Value)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            if (string.IsNullOrWhiteSpace(key)) return;
            if (!_blocks.TryGetValue(key, out var blocks))
                _blocks[key] = blocks = new List<string[]>();
            blocks.Add(lines.ToArray());
        }
    }
}
