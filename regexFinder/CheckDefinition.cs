using System.Collections.Generic;

namespace regexFinder
{
    public class CheckDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Field { get; set; }
        public string Left { get; set; }
        public List<string> Right { get; set; } = new();
        public double Tolerance { get; set; } = 0.01;
        public string OrderBy { get; set; }
        public string PreviousField { get; set; }
        public string CurrentField { get; set; }
        public double Step { get; set; } = 1;
    }
}
