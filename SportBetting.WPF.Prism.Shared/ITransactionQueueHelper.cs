using System;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Shared
{
    public interface ITransactionQueueHelper
    {
        int GetCountTransactionQueue();
        int TrySaveTicketOnHub(string sTransactionId, uid uid, TicketWS ticketDataWs, string sStationNumber, bool bIsOffLineTicket);
        int TryStoreTicketOnHub(string sTransactionId, uid uidSession, TicketWS ticketDataWs, string sStationNumber, bool bIsOffLineTicket, string pin);
        void TryToSaveTransactionObjectsOnHub();
        decimal TryRegisterMoneyOnHub(uid userUID, string sTransactionId, bool cashIn, string operationType, int operatorId, bool checkpoint, out DateTime startdate, out DateTime enddate);
        bool TryDepositByTicketMoneyOnHub(string sTransactionId, uid User, string number, string code, string creditNumber, string creditCode, ref string error);
        bool TryDepositByCreditNoteMoneyOnHub(string getNextTransactionId, uid getUid, string number, string code, ref string error);
        accountBalance TryDepositMoneyOnHub(string sTransactionId, uid User, decimal money, bool realMoney, ref Exception error, CashAcceptorType? type);
    }
}