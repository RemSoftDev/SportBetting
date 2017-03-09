using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Shared;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using WsdlRepository.WsdlServiceReference;

//using Nbt.IDCardReader;

namespace WsdlRepository
{
    public interface IStationRepository
    {
        List<string> SystemLanguages { get; set; }
        bool CheckDevicesChange();
        decimal GetMaxOdd(Ticket ticket);
        decimal GetMinStakeCombiBet(Ticket ticket);
        decimal GetMinStakeSystemBet(Ticket ticket);
        decimal GetMinStakeSingleBet(Ticket ticket);
        decimal GetMinStakePerRow(Ticket ticket);
        decimal GetMaxWinSystemBet(Ticket ticket);
        decimal GetMaxStakeSingleBet(Ticket ticket);
        decimal GetMaxWinSingleBet(Ticket ticket);
        decimal GetMaxWinMultiBet(Ticket ticket);
        decimal GetMaxSystemBet(Ticket ticket);
        decimal GetMaxStakeSystemBet(Ticket ticket);
        decimal GetMaxStakeCombi(Ticket ticket);
        int GetMinCombination(Ticket ticket);
        int GetMaxCombination(Ticket ticket);
        bool GetAllowMultiBet(Ticket ticket);
        StationAppConfigSr GetStationAppConfigValue(string name);
        void SetStationAppConfigValue<T>(string name, T value);
        decimal GetManipulationFeePercentage(Ticket ticket);
        decimal GetMaxStake(Ticket ticket);
        decimal GetBonusValueForBets(Ticket ticket);
        decimal UserConfidenceRaiting { get; }

        //From terminal
        void AddTestMoNeyFromKeyboard(decimal money);
        bool AllowAnonymousBetting { get; }
        int AutoLogoutWindowLiveTimeInSec { get; set; }
        bool BarcodeScannerAlwaysActive { get; set; }
        bool BarcodeScannerTempActive { get; set; }
        string Currency { get; set; }
        void EnableCashIn();
        void DisableCashIn();
        void SetCashInDefaultState(decimal minLimit);
        int PrinterStatus { get; set; }
        string StationNumber { get; }
        int StoreTicketExpirationHours { get; }
        bool DisableTransferToTerminal { get; }
        int PaymentNoteLength { get; }
        DateTime? Created_At { get; set; }
        CultureInfo Culture { get; }
        Dictionary<string, string> CultureInfos { get; }
        bool Refresh();
        string ServerVersion { get; set; }
        string TeamViewerID { get; set; }
        int CheckSumLength { get; }
        int TaxNumberLength { get; set; }
        decimal PrintingLanguageSetting { get; set; }
        string DefaultDisplayLanguage { get; set; }
        int FranchisorID { get; set; }
        Dictionary<string, string> HubSettings { get; }
        void Init();
        int LocationID { get; }
        bool IsPrematchEnabled { get; }
        bool IsLiveMatchEnabled { get; }
        bool ResultsVisible { get; }
        int SyncInterval { get; }
        bool IsAutoLogoutEnabled { get; set; }
        bool IsIdCardEnabled { get; set; }
        int Active { get; }
        int TicketNumberLength { get; }
        bool UsePrinter { get; set; }
        string StationName { get; set; }
        string FranchiserName { get; set; }
        string LocationName { get; set; }
        string FooterLine1 { get; }
        string FooterLine2 { get; }
        string BetTerms { get; }
        string HeaderLine1 { get; }
        string HeaderLine2 { get; }
        string HeaderLine3 { get; }
        string HeaderLine4 { get; }
        bool PrintLogo { get; }
        int SngLiveBetTicketAcceptTime { get; }
        int CombiLiveBetTicketAcceptTime { get; }
        bool AuthorizedTicketScanning { get; set; }
        int CreditNoteExpirationDays { get; set; }
        decimal GetBonusFromOdd(Ticket newTicket);
        decimal BonusFromOdd { get; set; }
        bool PayoutExpiredPaymentCreditNotes { get; set; }
        bool TurnOffCashInInit { get; set; }
        bool IsLocked { get; }
        bool IsStatisticsEnabled { get; }
        bool IsCashDatasetValid();
        bool IsReady { get; set; }
        int PathCount(IList<ITipItemVw> ticketX, out int PathExtra);
        bool AllowMixedStakes { get; }
        bool AllowFutureMatches { get; set; }
        int UserCardPinSetting { get; set; }
        int OperatorCardPinSetting { get; set; }
        bool IsStoreTicketEnabled { get; }
        bool DisplayTaxNumber { get; set; }
        string StationTyp { get; }
        string LiveStreamFeedUrl { get; }
        //bool EnableOddsChangeIndication { get; set; }
        formField[] GetRegistrationForm();
        bool EnableLiveStreaming { get; set; }
        int VFLSource { get; set; }
        bool IsTestMode { get; set; }
        bool BarcodeScannerTestMode { get; set; }
        bool IsImmediateWithdrawEnabled { get; set; }
        bool IsCreditNoteImmediatelyPaid { get; set; }
        bool AllowVfl { get; set; }
        bool AllowVhc { get; set; }
        bool AllowLive { get; set; }
        int TimeZoneOffset { get; set; }
        string LastPrintedObjects { get; set; }
        string LayoutName { get; set; }
        uid GetUid(User user);
        uid GetBasicUid();
        bool ResponsibleGaming { get; set; }
        int CardBarcodeLen { get; set; }
        bool IsCashier { get; }
        event PropertyChangedEventHandler PropertyChanged;
    }
}
