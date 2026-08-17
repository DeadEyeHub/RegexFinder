using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace regexFinder
{
    public class BlockDefinition
    {
        public string Name { get; set; }
        public string StartsWith { get; set; }
        public string EndsWith { get; set; }
        public bool StartsIsRegex { get; set; } = true;
        public bool EndsIsRegex { get; set; } = true;

        [JsonIgnore] public Regex StartsRegex { get; private set; }
        [JsonIgnore] public Regex EndsRegex { get; private set; }

        public void BuildRegexes(RegexOptions extraOptions = RegexOptions.None,
                                 TimeSpan? timeout = null,
                                 bool ignoreCase = true,
                                 bool autoDetectRegex = false)
        {
            var opts = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | extraOptions;
            if (ignoreCase) opts |= RegexOptions.IgnoreCase;

            StartsRegex = Make(StartsWith,
                               autoDetectRegex ? IsProbablyRegex(StartsWith) : StartsIsRegex,
                               opts, timeout);
            EndsRegex = Make(EndsWith,
                               autoDetectRegex ? IsProbablyRegex(EndsWith) : EndsIsRegex,
                               opts, timeout);
        }

        private static Regex Make(string spec, bool treatAsRegex, RegexOptions options, TimeSpan? timeout)
        {
            if (string.IsNullOrWhiteSpace(spec)) return null;

            string pattern = treatAsRegex
                ? spec
                : "^" + Regex.Escape(spec.Trim()) + "$";

            return timeout.HasValue
                ? new Regex(pattern, options, timeout.Value)
                : new Regex(pattern, options, TimeSpan.FromSeconds(2));
        }

        private static bool IsProbablyRegex(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            return s.IndexOfAny(new[] { '^', '$', '.', '*', '+', '?', '|', '(', ')', '[', ']', '{', '}', '\\' }) >= 0;
        }
    }
}
