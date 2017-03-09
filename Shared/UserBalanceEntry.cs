using System;

namespace Shared
{
    [Serializable]
    public class UserBalanceEntry
    {
        public decimal Amount { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Type { get; set; }

        public string TicketNumber { get; set; }
    }
}