using System;
using System.Collections.Generic;

namespace Shared
{
    public class TicketDetails
    {
        private IList<TipDetails> _tipItems = new List<TipDetails>();
        public string CheckSum { get; set; }

        public string TicketNumber { get; set; }

        public DateTime AcceptedTime { get; set; }

        public string BetType { get; set; }

        public decimal CurrentTicketPossibleWin { get; set; }

        public bool Paid { get; set; }

        public DateTime? PaidAt { get; set; }

        public decimal WonAmount { get; set; }

        public IList<TipDetails> TipItems
        {
            get { return _tipItems; }
            set { _tipItems = value; }
        }
    }
}