using System;

namespace ViewModels
{
    public class Tip
    {
        private int _state;
        private long _id;
        private string _homeTeam;

        private string _eventName ;
        public string EventName
        {
            get { return _eventName; }
            set { _eventName = value; }
        }

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

        public bool IsBanker { get; set; }

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

        public string CurrentCurrency { get; set; }

        public string CorrectTip { get; set; }

        public bool Calculated { get; set; }

        public bool PendingApproval { get; set; }

        public bool Canceled { get; set; }

        public string OpenLostWonColor
        {
            get
            {
                if (!this.Calculated) return "#FF9FA7AF"; // state 0 == open
                else if (this.PendingApproval) return "#FF9933";
                else if (this.Won) return "#ff22b613"; // state 1 == won
                else if (this.Canceled) return "#61217C"; //canceled
                else return "#FFFF1313"; // state 2 == lost
            }
        }
    }
}