using System;

namespace Shared.Statistics
{
    public class TournamentSheet
    {
        public long Id { get; set; }
        public string Rank { get; set; }
        public string Team { get; set; }

        public string Played { get; set; }
        public string Won { get; set; }
        public string Draw { get; set; }
        public string Lost { get; set; }

        public Tuple<int, int> Goals { get; set; }
        public int Difference { get; set; }
        public string Score { get; set; }
        
    }
}
