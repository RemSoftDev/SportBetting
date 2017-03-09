namespace SportBetting.WPF.Prism.Shared
{
    public interface IBusinessPropsHelper
    {
        string GetNextTransactionId();
        string GenerateNextCreditNoteNumber();
        void Initialize(string stationNumber, BusinessProps bp);
        string GenerateNextTicketNumber();
    }
}