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
        public List<string> Subtract { get; set; } = new();
        public List<string> IgnoreReceiptTypes { get; set; } = new();
        public double Tolerance { get; set; } = 0.01;
        public string OrderBy { get; set; }
        public string PreviousField { get; set; }
        public string CurrentField { get; set; }
        public double Step { get; set; } = 1;
        public string AmountField { get; set; }
        public string TotalField { get; set; }
        public string ReceiptTypeField { get; set; }
        public List<string> IncludedReceiptTypes { get; set; } = new();
        public List<string> CheckpointReceiptTypes { get; set; } = new();
        public string ExcludeIfField { get; set; }
        public bool ExcludeIfNonZero { get; set; }
        public string CancelledField { get; set; } = "IsCancelled";
        public bool SkipCancelledReceipts { get; set; } = true;
        public bool Cumulative { get; set; } = true;
    }
}
