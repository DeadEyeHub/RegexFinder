using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace regexFinder
{
    public class PatternDefinition
    {
        public string Name { get; set; }
        public string RegexCommand { get; set; }
        public string BlockName { get; set; }
        public string CombineMethod { get; set; }
        public string CompareTo { get; set; }
        public string ValueType { get; set; }
        public bool Multiline { get; set; }
        public int LinesCount { get; set; }

        [JsonIgnore] public Regex CompiledRegex { get; private set; }

        public void BuildRegex(RegexOptions extraOptions = RegexOptions.None, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(RegexCommand))
            {
                CompiledRegex = null;
                return;
            }

            var opts = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | extraOptions;
            CompiledRegex = timeout.HasValue
                ? new Regex(RegexCommand, opts, timeout.Value)
                : new Regex(RegexCommand, opts, TimeSpan.FromSeconds(2));
        }
    }
}
