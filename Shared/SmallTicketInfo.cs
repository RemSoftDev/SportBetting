using System;

namespace Shared
{
    [System.Serializable]
    public class SmallTicketInfo
    {
        private DateTime _acceptedTime;
        private string _checkSum;
        private string _ticketNumber;
        private TicketCategory _status;

        public DateTime AcceptedTime
        {
            get { return _acceptedTime; }
            set { _acceptedTime = value; }
        }

        public string CheckSum
        {
            get { return _checkSum; }
            set { _checkSum = value; }
        }

        public string TicketNumber
        {
            get { return _ticketNumber; }
            set { _ticketNumber = value; }
        }

        public TicketCategory Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public string BetType { get; set; }

        public decimal Stake { get; set; }

        public decimal TotalOdd { get; set; }

        public decimal Payout { get; set; }

        public decimal PossibleWin { get; set; }

        public decimal TicketShopProfit
        {
            get
            {
                decimal result = 0;
                if (this.Status != TicketCategory.Open)
                {
                    result = (this.Stake - this.Payout);
                }
                return result;
            }
        }
    }
    public enum TicketCategory
    {
        Won,

        Lost,

        Canceled,

        Open,
    }
}