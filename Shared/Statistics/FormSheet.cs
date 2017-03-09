using System.Collections.Generic;
using SportRadar.Common.Enums;

namespace Shared.Statistics
{
    public class FormSheet
    {
        public long Id { get; set; }
        public int Rank { get; set; }
        public int Points { get; set; }
        public string Team { get; set; }
        public List<MatchResult> Results { get; set; }
    }
}