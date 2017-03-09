using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SportBetting.WPF.Prism.Shared
{
    public static class MsgTag
    {
        public const string LiveLanguageChosen      = "LiveLanguageChosen";
        public const string LanguageChosen          = "LanguageChosen";
        public const string UseCreditNote           = "UseCreditNote";
        public const string OpenStoredTicket        = "OpenStoredTicket";
        public const string SportChosen             = "SportChosen";
        public const string ShowKeyboard            = "ShowKeyboard";
        public const string HideKeyboard            = "HideKeyboard";
        public const string WriteNumber             = "WriteNumber";
        public const string OpenLogin               = "OpenLogin";
        public const string Refresh                 = "Refresh";
        public const string LiveRefresh             = "LiveRefresh";
        public const string BetDomainViewLiveRefresh = "BetDomainViewLiveRefresh";
        public const string RefreshLockOffers       = "RefreshLockOffers";
        public const string SetFocus         = "SetFocus";
        public const string CloseCurrentWindow      = "CloseCurrentWindow";
        public const string HideUserProfile         = "HideUserProfile";
        public const string ShowUserManagement      = "ShowUserManagement";
        public const string LoadTicket              = "LoadTicket";
        public const string AutoLogoutWaitWindow    = "AutoLogoutWaitWindow";
        public const string Error                   = "Error";
        public const string AskAboutCashPool        = "AskAboutCashPool";
        public const string ServiceRestart          = "ServiceRestart";
        public const string VerifyUser              = "VerifyUser";
        public const string FillSports              = "FillSports";
        public const string PrinterNotReady         = "PrinterNotReady";
        public const string RestartApplication      = "RestartApplication";
        public const string RestartInTestMode       = "RestartInTestMode";
        public const string RestartStation          = "RestartStation";
        public const string RemoveMatch             = "RemoveMatch";
        public const string CreatedCheckpoint       = "CreatedCheckpoint";
        public const string LockStation             = "LockStation";
        public const string RefreshCashOperations   = "RefreshCashOperations";
        public const string SelectedOddsMatchRemoved = "SelectedOddsMatchRemoved";
        public const string DoBasketUIRefresh       = "DoBasketUIRefresh";
        public const string RefreshResults          = "RefreshResults";
        public const string ShowResultFilters       = "ShowResultFilters";
        public const string FillResultSports        = "FillResultSports";
        public const string UpdateHistory           = "UpdateHistory";
        public const string LanguageChosenHeader    = "LanguageChosenHeader";
        public const string SessionClosed           = "SessionClosed";
        public const string UserBlocked             = "UserBlocked";
        public const string CloseLogin              = "CloseLogin";
        public const string BindUserCard            = "BindUserCard";
        public const string BindOperatorCard        = "BindOperatorCard";
        public const string HideLogin               = "HideLogin";
        public const string EnterCommand            = "EnterCommand";
        public const string UpdateCurrentUser       = "UpdateCurrentUser";
        public const string UpdateNamedOdds         = "UpdateNamedOdds";
        public const string UpdateProgres           = "UpdateProgres";
        public const string ActivateShowSelected    = "ActivateShowSelected";
        public const string ActivateForwardSelected = "ActivateForwardSelected";
        public const string ShowSelectedTournaments = "ShowSelectedTournaments";
        public const string LoadBetDomains          = "LoadBetDomains";
        public const string BlockSportFilter        = "BlockSportFilter";
        public const string BlockTimeFilter         = "BlockTimeFilter";
        public const string LoadAccCheckpoints      = "LoadAccCheckpoints";
        public const string LoadPaymentNote         = "LoadPaymentNote";
        public const string OpenSearchUserView      = "LoadRegistrationNote";
        public const string OpenUserProfile         = "OpenUserProfile";
        public const string SetCreditNoteButton     = "SetCreditNoteButton";
        public const string AddMoneyFromCreditNote  = "AddMoneyFromCreditNote";
        public const string AddMoneyFromTicket      = "AddMoneyFromTicket";
        public const string ReloadTicket            = "ReloadTicket";
        public const string LoadOperShiftReports    = "LoadOperShiftReports";
        public const string LoadOperators           = "LoadOperators";
        public const string RefreshSelectedOdds     = "RefreshSelectedOdds";
        public const string OpenOperatorPaymentView = "OpenOperatorPaymentView";
        public const string ShowFirstViewAndResetFilters = "ShowFirstViewAndResetFilters";
        public const string HideOtherWindows        = "HideOtherWindows";
        public const string ShowTermsAndConditions  = "ShowTermsAndConditions";
        public const string ShowResponibleGamng     = "ShowResponibleGamng";
        public const string ZippedLogsUploaded      = "ZippedLogsUploaded";
        public const string AskLoginAnonymous       = "AskLoginAnonymous";
        public const string HideVerification        = "HideVerification";
        public const string CloseWarningWindow = "CloseWarningWindow";
        public const string ShowNotificationBar = "ShowNotificationBar";
        public const string HideNotificationBar = "HideNotificationBar";
        public const string TestWriteNumber = "TestWriteNumber";
        public const string EmulateBarcode = "EmulateBarcode";
        public const string NavigateBack = "NavigateBack";
        public const string ShowCalibration = "ShowCalibration";
        
        public const string CardInserted = "CardInserted";
        public const string StartedCardReading = "StartedCardReading";
        public const string CardRemoved = "CardRemoved";
        public const string IdCardError = "IdCardError";
        public const string EnterPinClearTicketNumber = "EnterPinClearTicketNumber";
        public const string EnterPinBackspace = "EnterPinBackspace";
        public const string EnterPinButton = "EnterPinButton";
        public const string EnableEdit = "EnableEdit";
        public const string SaveUserProfile = "SaveUserProfile";
        public const string RefreshCashpool = "RefreshCashpool";
        public const string PrinterErrorValue = "PrinterErrorValue";
        public const string ClearTicketNumber = "ClearTicketNumber";
        public const string PinBackspace = "PinBackspace";
        public const string PinButton = "PinButton";
        public const string GetSearchResults = "GetSearchResults";
        public const string CloseEnterPinWindow = "CloseEnterPinWindow";
        public const string CheckRestart = "CheckRestart";
        public const string ShowWarning = "ShowWarning";
        public const string ResetFilters = "ResetFilters";
        public const string ShowBalanceCheckpoints = "ShowBalanceCheckpoints";
        public const string RefreshLiveMonitor = "RefreshLiveMonitor";
        public const string UpdateBalance = "UpdateBalance";
        public const string ShowCashAcceptorLockedLabel = "ShowCashAcceptorLockedLabel";


        public const string LoadPrevPage = "LoadPrevPage";
        public const string LoadNextPage = "LoadNextPage";

        public const string RefreshTicketDetails = "RefreshTicketDetails";
        public const string ShowNotification = "ShowNotification";
        public const string HideNotification = "HideNotification";

        public const string ShowMenu = "ShowMenu";
        public const string ShowStream = "ShowStream";
        public const string HideStream = "HideStream";
        public const string OddPlaced = "OddPlaced";
        public const string BarcodeScannerTest = "BarcodeScannerTest";

        public const string CloseRegistration = "CloseRegistration";
        public const string BasketRebindWheel = "BasketRebindWheel";

        public const string UpdateLiveMonitorTemplates = "UpdateLiveMonitorTemplates";
        public const string OffsetChanged = "OffsetChanged";
        public const string RefreshStation = "RefreshStation";

        public const string FillTaxNumber = "FillTaxNumber";

        public const string ShowVFL = "ShowVFL";
        public const string ShowVHC = "ShowVHC";

        public static string Unblur = "Unblur";

        public static string OpenVHCtAC = "OpenVHCtAC";

        public static string ShowSystemMessage = "ShowSystemMessage";
        public static string SetUpBrowser = "SetUpBrowser";
        public static string GetFocus = "GetFocus";
        public static string ChangeVisibility = "ChangeVisibility";

        public static string ClearSelectedSports = "ClearSelectedSports";
        public static string ClosePinWindow = "ClosePinWindow";
        public static string AcceptNewTermsVersion = "AcceptNewTermsVersion";
    }   
}
