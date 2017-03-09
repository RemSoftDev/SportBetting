using System;

namespace Shared
{
    public class TipDetails
    {

        public const int TICKET_WONSTATUS_INVALID = -1;
        public const int TICKET_WONSTATUS_OPEN = 0;
        public const int TICKET_WONSTATUS_WON = 1;
        public const int TICKET_WONSTATUS_LOST = 2;
        public const int TICKET_WONSTATUS_PAID = 3;
        public const int TICKET_WONSTATUS_CANCELED = 4;

        private int _state;
        private long _id;
        private string _homeTeam;

        public int State
        {
            get { return _state; }
            set { _state = value; }
        }

        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string ResultName { get; set; }

        public string Text { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string BetDomainNameFromTicket { get; set; }

        public string HomeTeam
        {
            get { return _homeTeam; }
            set { _homeTeam = value; }
        }

        public string AwayTeam { get; set; }

        public string Score { get; set; }

        public bool Won { get; set; }

        public decimal Stake { get; set; }

        public string Competitors { get; set; }

        public string StateString { get; set; }

        public string CorrectTip { get; set; }

        public bool Calculated { get; set; }

        public string OpenLostWonColor
        {
            get
            {
                if (!this.Calculated) return "#FF9FA7AF"; // state 0 == open
                else if (this.Won) return "#ff22b613"; // state 1 == won
                else return "#FFFF1313"; // state 2 == lost
            }
        }
    }
}