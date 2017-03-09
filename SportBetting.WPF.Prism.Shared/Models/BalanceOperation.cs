using System;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class BalanceOperation
    {
        private bool _hidden;
        public String HistoryRecordType { get; set; }
        public string TicketNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
        public String Comment { get; set; }
        public String Currency { get; set; }

        public bool Hidden
        {
            get { return _hidden; }
            set { _hidden = value; }
        }

        public BalanceOperation(string historyRecordType, string ticketNumber, decimal amount, DateTime dateTime, string comment, string currency)
        {
            HistoryRecordType = historyRecordType;
            TicketNumber = ticketNumber;
            Amount = amount;
            DateTime = dateTime;
            Comment = comment;
            Currency = currency;
        }
    }
}
