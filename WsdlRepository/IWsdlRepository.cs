using System;
using System.Collections.Generic;
using WsdlRepository.WsdlServiceReference;

namespace WsdlRepository
{
    public interface IWsdlRepository
    {
        string SecureKey { get; set; }
        UpdateRecord[] UpdateLine(string stationNumber, long lastUpdateFileId, System.DateTime lastUpdate, out System.Nullable<int> total);
        UpdateRecord[] UpdateLocalization(string stationNumber, long lastUpdateFileId);
        long GetBusinessProps(string stationNumber, out long lastTicketNumber, out long lastTransactionId);
        string SaveTicket(string transactionId, uid uid, TicketWS ticketData, bool isOffLineTicket, string station_id, out long[] tipLock, out long[] tournamentLock);
        bool ConnectionProblem { get; set; }
        TicketWS LoadTicket(string number, string checksum, string stationNumber, string lang, string culture, bool showDetails);
        TicketWS[] LoadStoredTicket(uid userUid, string number, string checksum, string stationNumber, string pin);
        bool? CancelTicket(string number, string checksum, string stationNumber);
        SessionWS OpenSession(string station_id, bool anonymous, string username, string password, bool isShop);

        SessionWS LoginWithIdCard(string station_id, string cardNumber, string pin, bool isShopSession = false);
        UserTicket[] GetUserTickets(string accountId, ticketCategory ticketCategory, AccountTicketSorting sorting, int startIndex, int pageSize, out string totalNumber);
        UpdateRecord[] GetLatestConfidenceFactorsUpdate(string stationNumber);
        bool CloseSession(string sessionid);
        profileForm LoadProfile(uid uid);
        string UpdateProfile(long? operatorId, uid uidFromUser, valueForm valueForm);
        int RegisterAccount(long? operatorId, valueForm valueForm, string stationNumber, bool isVerified, out string registration_note_number);
        bool ChangePasswordFromTerminal(uid uid, string oldPassword, string newPassword);
        string EndUserVerification(uid uid);
        bool ToggleAccountState(long accountId, long operatorId);
        string UpdateIdCard(string cardnumber, string status, bool active, long? accountId);
        string RegisterIdCard(string number, bool active, string franchizorId, out string pin);
        void ChangeIDCardPin(uid uid, ref string newPin);
        void ChangeOperatorIDCardPin(long operatorId, ref string newPin);
        valueField[][] AccountSearch(criteria[] criterias, uid uid);
        Operator[] SearchForOperators(OperatorCriterias criterias, uid uid);
        int RegisterOperator(uid uidFromUser, string firstName, string lastName, string username, string password, string language, int roleId, string email);
        bool UpdateOperator(long accountId, OperatorCriterias criterias);
        string BindOperatorIdCard(long accountId, string cardNumber, out string pin);
        string CreateCheckpoint(string stationNumber, long accountId, out decimal amount);
        AccountingRecieptWS GetAccountingRecieptData(DateTime startDate, DateTime endDate, string stationNumber, int? locationId);
        string GetAccountByTicket(string ticketNumber);
        bool ChangePasswordFromShop(int operatorId, uid getUid, string newPassword);
        string History(uid uid, string type, string operationgroup, string startIndex, string limit, DateTime? startDate, DateTime? endDate, out historyData[] history, bool hideSystemRecalculation);
        ArrayOfProfitAccountingCheckpoints GetProfitAccountingCheckpoint(int objectId, string stationNumber, int currentPosition, int itemsAmountPerPage);
        ProfitAccountingCheckpoint CreateProfitAccountingCheckpoint(int locationId, string stationNumber);
        IEnumerable<StationBalanceCheckpoint> GetStationBalanceCheckpoints(string stationNumber, int startIndex, int pageSize);
        decimal GetStationPaymentFlowData(string stationNumber, int currentPosition, int itemsAmountPerPage, out decimal paymkentBalance, out decimal locationCashPosition, out decimal totalLocationBalance, out PaymentFlowData[] list, out long itemsTotal);
        bool AddPaymentFlow(PaymentFlowData request);
        string RegisterPaymentNote(uid uid, ref decimal amountRef, out DateTime expiration, out bool moneyWithdraw, out string number, out bool withdrawFrombalance);
        bool SaveCreditNote(uid iud, string number, string checkSum, decimal amount, string stationNumber,out PayoutType payoutType);
        CreditNoteWS LoadCreditNote(string number, string checksum, string stationNumber);
        IdCardInfoItem[] GetIdCardInfo(int accountId, Role user);
        TicketWS[] LoadStoredTickets(uid uid, string stationNumber, string ticketnumber, string pin, string checkSum);
        decimal GetBalance(uid getUid, out decimal reserved, out decimal factor);
        bool ValidateIdCard(string cardNumber, string stationNumber, bool isShop, out SessionWS sessionId);
        string StoreTicket(string sTransactionId, uid uid, TicketWS ticketData, string pin, bool bIsOffLineTicket, string sStationNumber);
        accountBalance Deposit(string sTransactionId, uid uId, decimal decMoneyAmount, bool realMoney, CashAcceptorType? type, bool depositFromCashpool);
        decimal CashOut(uid userUID, string stationNumber, out DateTime lastCashout, string comment, out DateTime enddate);
        bool DepositByTicket(string sTransactionId, uid uId, string ticketNumber, string ticketCode, string creditNoteNumber, string creditNoteCode);
        bool? WriteRemoteError2Log(string message, int criticality, string objectId, string msgTerminal);
        string GetStationByVerificationNumber(string sVerificationCode);
        StationWS GetStationProperties(string stationNumber, string sIpAddress, string teamViewerId, string serverVersion, PeripheralInfo pi, out valueForm hubResponse, out BsmHubConfigurationResponse hubConfigurationResponse, DriverInfo[] drivers);
        registrationForm GetRegistrationForm();
        IEnumerable<UserRole> GetAllRoles(string operatorId);
        bool DepositByCreditNote(string sTransactionId, uid uId, string noteNumber, string checkSum);
        string IssueCreditToStationBalance(uid uid, decimal amount);
        SettlementHistory[] GetSettlementHistory(uid accountUid);
        OperatorShiftCheckpoint[] GetOperatorShiftCheckpoints(int locationId, short operatorId, out decimal tempBalance, bool isFilterByOperator);
        OperatorShiftData CreateOperatorShiftCheckpoint(int locationId, short operatorId);
        OperatorWithShift[] GetAllOperatorsWithShifts(uid uid);
        SettlementHistoryDetail[] GetSettlementHistoryDetails(int id);
        bool CheckForEmptyBoxAndPayments(int locationId, int id);
        profileForm LoadPaymentNote(string paymentNoteNumber, uid getUid, out decimal amount);
        bool PayCreditNote(uid uid, string number, string code, string accountId, string stationNumber);
        string WithdrawByPaymentNote(string paymentNoteNumber, uid getUid, out decimal amount, out bool withFrombalance);
        DateTime ProduceOperatorSettlement(ref int operatorId, int accountId, out string opName, out string opLName, out string frName, out string locName, out string locOwnerName, out DateTime stDate, out DateTime enDate, out CheckpointSlip[] cpArray, out TotalSettlementSection totSection);
        long[] GetLockedOffer(string sStationNumber, out long[] arrLockedTournamentIds);
        UserComment[] LoadUserComments(long uid);
        bool AddUserComment(long uid, uid operatorId, string comment);
        bool DeleteUserComment(int commentId);
        bool EditUserComments(long uid, uid operatorId, int commentId, string comment);
        UpdateRecord[] UpdateFlags(string stationNumber, long id);
        UpdateRecord[] UpdateStatistics(string stationNumber, long id);
        bool StationEvent(string stationNumber, StationEventType eventType);
        bool RegisterCash(uid uid, decimal amount);
        UpdateRecord[] GetMetainfo(string stationNumber, long id);
        decimal GetBettingBalance(DateTime startDate, DateTime endDate, string stationNumber,out decimal amountWon);
        bool UnbindIdCard(string number);
        ProfitAccountingCheckpoint GetProfitAccountingReport(int id, string station, DateTime? startDate, DateTime? endDate);
        valueField[] GetAccountByRegistrationNote(string registration_note_number, int franchisorId);
        string Matchsorting(string matches);
        string GetLiveStreamFeed();
        OperatorShiftData GetOperatorShiftReport(int locationId, short operatorId);
        string GetStationResource(int resourceId);
        decimal GetCashInfo(string station, out decimal totalStationCash, out decimal locationCashPosition,
                                   out decimal totalLocationCash, out decimal totalLocationPaymentBalance);
        string GetContentManagementData(string stationNr);
        CashOperationtData[] GetStationCashHistory(string stationId, DateTime startDate, DateTime endDate);
        decimal GetStationCashInfo(string station, out decimal billsAmount, out int billscount, out decimal coinsamount, out int coinscount);
        CashOutItem[] GetCasheOuts(string stationnumber);
    }
}
