namespace SportBetting.WPF.Prism.Shared
{
    public class BusinessProps
    {
        public BusinessProps(long number, long creditnumber, long transactionnumber)
        {
            LastCreditNoteNumber = creditnumber;
            LastTicketNumber = number;
            LastTransactionId = transactionnumber;
        }

        public long LastTransactionId { get; set; }

        public long LastTicketNumber { get; set; }

        public long LastCreditNoteNumber { get; set; }
    }
}