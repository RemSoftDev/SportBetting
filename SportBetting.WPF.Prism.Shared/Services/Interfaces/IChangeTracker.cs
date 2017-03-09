using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Collections;
using SportRadar.Common.Enums;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using SportBetting.WPF.Prism.OldCode;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using BalanceCheckpoint = SportBetting.WPF.Prism.Shared.Models.BalanceCheckpoint;

namespace SportBetting.WPF.Prism.Shared.Services.Interfaces
{
    /// <summary>
    /// Keeps track of last changes.
    /// </summary>
    public interface IChangeTracker
    {
        bool NewTermsAccepted { get; set; }
        string TermsAndConditionsVersion { get; }
        bool NeedVerticalRegistrationFields { get; set; }
        User CurrentUser { get; set; }
        System.Windows.Window MainWindow { get; set; }
        DateTime MouseClickLastTime { get; set; }
        string ErrorLabel { get; set; }
        void OnPropertyChanged(string propertyName);
        decimal TruncateDecimal(decimal valueToTruncate);
        BarCodeConverter.BarcodeType LoadedTicketType { get; set; }
        string LoadedTicket { get; set; }
        string LoadedTicketcheckSum { get; set; }
        TicketWS CurrentTicket { get; set; }
        DateTime StartDateAccounting { get; set; }
        DateTime EndDateAccounting { get; set; }
        bool CashInAccounting { get; set; }
        bool CashOutAccounting { get; set; }
        bool FromCheckPointsAccounting { get; set; }
        DateTime CalendarStartDateAccounting { get; set; }
        DateTime CalendarEndDateAccounting { get; set; }
        DateTime? BirthDate { get; set; }
        DateTime? minDate { get; set; }
        DateTime? maxDate { get; set; }
        DateTime? initDate { get; set; }
        DateTime LastCashoutDate { get; set; }
        Window UserProfileWindow { get; set; }
        FoundUser EditableUser { get; set; }
        ObservableCollection<FoundUser> FoundUsers { get; set; }
        ObservableCollection<FoundOperator> FoundOperators { get; set; }
        string CardNumber { get; set; }
        string SearchString { get; set; }
        ComboBoxItemStringId SelectedSportFilter { get; set; }
        bool MaxOddFactorExceeded { get; set; }
        ComboBoxItem SelectedTimeFilter { get; set; }
        bool VerifivationCancelled { get; set; }
        bool VerificationRestart { get; set; }
        TicketWS Ticket { get; set; }
        int RestartPending { get; set; }
        ObservableCollection<BalanceCheckpoint> Balance { get; set; }
        FoundOperator FoundOperator { get; set; }
        bool IsBindingCard { get; set; }
        bool PrinterErrorChecked { get; set; }
        SyncList<LiveWindowEntry> LiveMonitors { get; set; }
        double HeaderHeight { get; set; }
        double MatchHeight { get; set; }
        ComboBoxItem ResultsSelectedTime { get; set; }
        int ResultsSelectedDay { get; set; }
        int LiveSelectedMode { get; set; }
        bool LiveSelectedAllSports { get; set; }

        int PreMatchSelectedMode { get; set; }
        ObservableCollection<TicketView> Tickets { get; set; }
        ComboBoxItem SelectedType { get; set; }
        int SelectedTycketType { get; set; }
        long AllPages { get; set; }
        long CurrentPageIndex { get; set; }
        bool AutoLogoutActive { get; set; }
        bool DoLogout { get; set; }
        Window LoginWindow { get; set; }
        SortableObservableCollection<IMatchVw> AllResults { get; set; }
        ObservableCollection<ComboBoxItem> SearchSports { get; set; }
        ObservableCollection<ComboBoxItem> SearchTournaments { get; set; }
        bool LockTournamentAgainstAll { get; set; }
        HashSet<string> SelectedTournaments { get; set; }
        double HistoryrowHeight { get; set; }
        double HistorygridHeight { get; set; }
        bool ErrorWindowActive { get; set; }
        string PaymentFlowOperationType { get; set; }
        ComboBoxItem SearchSelectedTournament { get; set; }
        ComboBoxItem SearchSelectedSport { get; set; }

        int UserPinSetting { get; set; }
        bool LockCashAcceptors { get; set; }
        bool OperatorPaymentViewOpen { get; set; }

        int BasketWheelPosition { get; set; }
        bool BetDomainViewFromBasket { get; set; }
        ObservableCollection<ComboBoxItemStringId> SportFilters { get; set; }
        bool IsBetdomainViewOpen { get; set; }
        bool IsBasketOpen { get; set; }
        bool IsUserProfile { get; set; }
        bool IsCashAcceporDatasetValid { get; }
        bool RedirectToTicketDetails { get; set; }
        bool SelectedSports { get; set; }
        bool SelectedTicket { get; set; }
        bool SelectedLive { get; set; }
        bool SelectedResults { get; set; }
        //bool IsAdminWindowOpened { get; set; }
        MultistringTag LastNotificationTag { get; set; }
        MultistringTag AdminTitle2 { get; set; }
        MultistringTag AdminTitle1 { get; set; }
        bool IsUserManagementWindowVisible { get; set; }
        bool TicketChecked { get; set; }
        bool UserChecked { get; set; }
        bool CashHistoryChecked { get; set; }
        bool SearchOperatorUsersChecked { get; set; }
        bool RestartChecked { get; set; }
        bool SystemInfoNetworkChecked { get; set; }
        bool SystemInfoMonitorsChecked { get; set; }
        bool SystemInfoChecked { get; set; }
        bool VerifyStationChecked { get; set; }
        bool RegisterUserChecked { get; set; }
        bool SearchUsersChecked { get; set; }
        bool ShopPaymentsChecked { get; set; }
        bool ProfitAccountingChecked { get; set; }
        bool CashOperationsChecked { get; set; }
        bool OperatorsShiftsReporChecked { get; set; }
        bool OpenShiftsChecked { get; set; }
        bool RegisterOperatorChecked { get; set; }
        bool CardAndPinChecked { get; set; }
        bool BalanceOperationsChecked { get; set; }
        bool TerminalAccountingChecked { get; set; }
        bool AdministrationHiddenChecked { get; set; }
        bool EmptyBoxChecked { get; set; }
        bool PrintTestPageChecked { get; set; }
        decimal TerminalBalance { get; set; }
        decimal LocationBalance { get; set; }
        decimal TotalLocationPaymentBalance { get; set; }
        decimal TotalStationCash { get; set; }
        decimal LocationCashPosition { get; set; }
        DateTime ProfitReportStartDate { get; set; }
        DateTime ProfitReportEndDate { get; set; }
        bool CreateFromLastCheckpoint { get; set; }
        SortableObservableCollection<IMatchVw> SearchMatches { get; set; }
        bool SelectedVirtualSports { get; set; }
        List<criteria> SearchRequest { get; set; }
        int ItemsAmmountPerPage { get; set; }
        bool BindingCardCancelled { get; set; }
        IMatchVw CurrentMatch { get; set; }
        int TicketsStartPage { get; set; }
        bool OperatorSearchUserViewOpen { get; set; }
        int VideoTimePeriodMin { get; set; }
        int VideoTimePeriodMax { get; set; }
        int VideoWarningBefore { get; set; }
        ObservableCollection<ComboBoxItem> TimeFilters { get; set; }
        bool HeaderVisible { get; set; }
        bool FooterVisible { get; set; }
        bool FooterArrowsVisible { get; set; }
        string TicketLogo { get; set; }
        bool AdministratorWindowLoading { get; set; }
        bool BetSelected { get; set; }
        bool IsForecastOpen { get; set; }
        bool IsLandscapeMode { get; set; }
        bool Is34Mode { get; set; }
        double Screen2WindowScale { get; set; }
        bool IsSearchOpen { get; set; }
        String CurrentMatchOrRaceDay { get; set; }
        String CurrentSeasonOrRace { get; set; }
        eServerSourceType? SourceType { get; set; }
        VHCType? VhcSelectedType { get; set; }
        bool IsCalibration { get; set; }
        bool CanScanTaxNumber { get; set; }
        bool PrintTicketChecked { get; set; }
        bool TicketBuyActive { get; set; }
        List<string> SelectedDescriptorsLive { get; set; }
        List<string> SelectedDescriptorsPreMatch { get; set; }
        bool TestInputActive { get; set; }
        IList<string> SelectedDescriptors { get; set; }
    }
}