using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Nbt.Services.Spf;
using Nbt.Services.Spf.Printer;
using Shared;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Shared.Services
{
    public interface IPrinterHandler
    {
        IPrinter InitPrinter(bool b);
        bool PrintTicket(TicketWS ticket, bool isDuplicate);
        bool PrintStoredTicket(TicketWS ticket, string pin, string stationNumber, DateTime expireDate, string toString);
        bool PrintCreditNote(decimal amount, string number, string checkSum, bool b, DateTime minValue, DateTime dateTime);
        bool PrintPaymentNote(decimal amount, string code, DateTime now);
        int currentStatus { get; set; }
        int NotPrintedItemsCount { get; set; }
        bool PrintTestString();
        bool PrintCashBalance(Dictionary<Decimal, int> cashinNotes, DateTime startDate, DateTime endDate, decimal cashin, decimal cashout, decimal collected, bool isDuplicate, bool isBallanceInfo, string operatorName, int id);
        bool PrintShopPaymentReciept(string type, decimal amount, long id, string username);
        bool PrintChechpointForLocation(ProfitAccountingCheckpoint checkpoint, bool shopPaymentsReadLocationOwner);
        bool PrintChechpointForTerminal(ProfitAccountingCheckpoint checkpoint, bool shopPaymentsReadLocationOwner);
        bool PrintAccountReceipt(string type, AccountingRecieptWS reciept, DateTime start, DateTime end);
        bool PrintDepositLostMessage(decimal credit, string sUsername);
        void PrintPinNote(string pin);
        bool PrintOperatorShiftReport(OperatorShiftCheckpoint checkpoint, decimal balance);
        bool PrintOperatorSettlementResponce(ProduceOperatorSettlementResponse responce);
        bool PrintPaymentRecept(string number, string code, decimal amount, bool isCreditNote);
        bool PrintRegistrationNote(string user, string registration_note_number);
        void PrintChangeOrientationBarcode();
        bool PrintLastObject();
        bool PrintLastObject(int amount);
        event EventHandler RefreshNotPrintedCount;
        bool PrintLastTickets(int amount);
        int PrintedTicketsCount { get; }
        XmlPreprocess CreateXmlAndSaveFile(TicketWS rTicketWs, bool isDuplicate);
        void DeleteAllPrinterObjects();
        void WriteBarcodeCardNumber(string s);
    }
}