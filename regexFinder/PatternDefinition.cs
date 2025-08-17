using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace regexFinder
{
    public class PatternDefinition
    {
        public string Name { get; set; }
        public string RegexCommand { get; set; }
        public List <string> StartsWith { get; set; }
        public string CombineMethod { get; set; }  // "merge", "sum", "none"
        public string CompareTo { get; set; }      // null или список имён через запятую
        public string ValueType { get; set; }      // "string", "float", "int", ...
        public bool Multiline { get; set; }        // если true — объединять несколько строк
        public int LinesCount { get; set; }        // количество строк для объединения
    }
}