using System;
using System.Linq;
using System.ServiceModel;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserProfile view model.
    /// </summary>
    [ServiceAspect]
    public class FoundUserProfileViewModel : BaseViewModel
    {
        private static ILog Log = LogFactory.CreateLog(typeof(FoundUserProfileViewModel));

        #region Constructors

        public FoundUserProfileViewModel()
        {
            HidePleaseWait = false;

            BackCommand = new Command(OnBackCommand);
            ActivateCommand = new Command(OnActivate);
            HistoryCommand = new Command(OnHistory);
            ChangePasswordCommand = new Command(OnChangePassword);
            BlockCardCommand = new Command(OnBlockCard);
            NewPINCommand = new Command(OnNewPin);
            BindCardCommand = new Command(OnBindCard);
        }

        #endregion

        #region Properties

        public FoundUser EditUser
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }

        public string UserActiveColor
        {
            get
            {
                if (EditUser == null) return "#FF9FA7AF";
                if (!EditUser.IsVerified) return "#FF9FA7AF";
                if (!EditUser.Active) return "#FFFF1313";
                return "#ff22b613";
            }
        }

        public bool Activate
        {
            get
            {
                if (EditUser != null)
                {
                    if (!EditUser.IsVerified) return false;
                    else if (!EditUser.Active) return false;
                    else return true;
                }
                return false;
            }
            set
            {
                return;
            }
        }

        public bool EnableActivationButton
        {
            get
            {
                if (ActivateText == TranslationProvider.Translate(MultistringTags.BLOCK).ToString() && !ChangeTracker.CurrentUser.BlockUser)
                    return false;
                else if (ActivateText == TranslationProvider.Translate(MultistringTags.ACTIVATE).ToString() && EditUser.IsVerified && !ChangeTracker.CurrentUser.BlockUser)
                    return false;
                else if (ActivateText == TranslationProvider.Translate(MultistringTags.ACTIVATE).ToString() && !EditUser.IsVerified && !ChangeTracker.CurrentUser.ActivateUser)
                    return false;
                else
                    return true;
            }
        }

        public string ActivateText
        {
            get
            {
                OnPropertyChanged("Activate");
                return Activate ?
                TranslationProvider.Translate(MultistringTags.BLOCK).ToString() :
                TranslationProvider.Translate(MultistringTags.ACTIVATE).ToString();
            }
        }

        public string ActiveUserText
        {
            get
            {
                OnPropertyChanged("UserActiveColor");
                if (!EditUser.IsVerified) return TranslationProvider.Translate(MultistringTags.TERMINAL_ACCOUNT_INACTIVE).ToString();
                else if (!EditUser.Active) return TranslationProvider.Translate(MultistringTags.TERMINAL_ACCOUNT_BLOCK).ToString();
                else return TranslationProvider.Translate(MultistringTags.TERMINAL_ACCOUNT_ACTIVE).ToString();
            }
        }

        public string ActiveCardText
        {
            get
            {
                if (EditUser.HasCard == 169)
                {
                    if (!StationRepository.IsIdCardEnabled)
                        return TranslationProvider.Translate(MultistringTags.TERMINAL_BARCODECARD_INACTIVE).ToString();
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_IDCARD_INACTIVE).ToString();
                }
                else if (EditUser.HasCard == 1)
                {
                    if (!StationRepository.IsIdCardEnabled)
                        return TranslationProvider.Translate(MultistringTags.TERMINAL_BARCODECARD_ACTIVE).ToString();
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_IDCARD_ACTIVE).ToString();
                }
                else
                {
                    if (!StationRepository.IsIdCardEnabled)
                        return TranslationProvider.Translate(MultistringTags.TERMINAL_BARCODECARD_BLOCKED).ToString();

                    return TranslationProvider.Translate(MultistringTags.TERMINAL_IDCARD_BLOCKED).ToString();
                }

            }
        }

        public string CardNumber
        {
            get { return ChangeTracker.CardNumber; }
            set { ChangeTracker.CardNumber = value; }
        }

        public bool VerifiedUser
        {
            get { return EditUser != null && !EditUser.IsVerified; }
        }

        public bool IsEnabledBindCard
        {
            get
            {
                if (ChangeTracker.CurrentUser is OperatorUser)
                {
                    if (ChangeTracker.CurrentUser.BindUserCard && EditUser.IsVerified)
                        return true;
                    else return false;
                }
                else return EditUser.IsVerified;
            }
        }

        public bool IsCardButtonsActive
        {
            get
            {
                if (EditUser != null)
                {
                    return EditUser.HasCard == 1;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool BlockCardButtonsActive
        {
            get
            {
                if (EditUser != null)
                {
                    return EditUser.HasCard == 1 && ChangeTracker.CurrentUser.BlockUserCard;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Commands

        public Command BackCommand { get; private set; }
        public Command ActivateCommand { get; private set; }
        public Command HistoryCommand { get; private set; }
        public Command ChangePasswordCommand { get; private set; }
        public Command BlockCardCommand { get; private set; }
        public Command NewPINCommand { get; private set; }
        public Command BindCardCommand { get; private set; }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<ChangeUserViewModel>(RegionNames.UserManagementProfileRegion);

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UserManagementProfileRegion);

            base.Close();
        }



        [WsdlServiceSyncAspect]
        private void OnBindCard()
        {
            try
            {
                var cards = WsdlRepository.GetIdCardInfo(EditUser.AccountId, Role.User);
                if (cards != null && cards.Any(card => card.active == "1"))
                {
                    var text = TranslationProvider.Translate(MultistringTags.USER_HAVE_ACTIVE_CARD_IT_WILL_BE_BLOCKED);
                    if (!StationRepository.IsIdCardEnabled)
                        text = TranslationProvider.Translate(MultistringTags.USER_HAVE_ACTIVE_BARCODECARD_IT_WILL_BE_BLOCKED);
                    QuestionWindowService.ShowMessage(text, null, null, model_YesClick, null);
                }
                else
                {
                    model_YesClick(null, null);
                }
            }
            catch (FaultException<HubServiceException> exception)
            {
                if (exception.Detail.code == 169)
                {
                    model_YesClick(null, null);
                }
            }
        }

        [AsyncMethod]
        private void model_YesClick(object sender, EventArgs e)
        {
            if (EditUser == null)
                return;

            Mediator.SendMessage<long>(EditUser.AccountId, MsgTag.BindUserCard);
            OnPropertyChanged("IsCardButtonsActive");
            OnPropertyChanged("BlockCardButtonsActive");
            var cards = new IdCardInfoItem[0];
            cards = null;
            try
            {
                cards = WsdlRepository.GetIdCardInfo(EditUser.AccountId, Role.User);
            }
            catch (Exception ex)
            {
            }
            if (cards != null && cards.Any(card => card.active == "1") && EditUser != null)
            {
                EditUser.HasCard = 1;
            }
            else if (cards != null && cards.Length > 0 && EditUser != null)
                EditUser.HasCard = 0;
            else if (EditUser != null)
                EditUser.HasCard = 169;
            OnPropertyChanged("IsCardButtonsActive");
            OnPropertyChanged("ActiveCardText");
            OnPropertyChanged("EditUser");
        }


        [WsdlServiceSyncAspect]
        private void OnNewPin()
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage();
                return;
            }
            Random RandNum = new Random();
            int RandomNumber = RandNum.Next(1000, 9999);
            string pin = RandomNumber.ToString();

            try
            {
                WsdlRepository.ChangeIDCardPin(StationRepository.GetUid(new LoggedInUser(EditUser.AccountId, null, 0, 0, 0, 0)), ref pin);
                PrinterHandler.PrintPinNote(pin);

            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                switch (exception.Detail.code)
                {
                    case 162:
                        if (StationRepository.IsIdCardEnabled)
                            ShowError(TranslationProvider.Translate(MultistringTags.USER_DONT_HAVE_ACTIVE_CARD));
                        else
                            ShowError(TranslationProvider.Translate(MultistringTags.USER_DONT_HAVE_ACTIVE_BARCODECARD));
                        break;
                }
            }
        }

        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;

            string errorMessage = TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_PINCODE).ToString() + ", ";

            switch (status)
            {
                case 0:
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), null, true);
                    return;
                case 4:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_PAPER).ToString();
                    break;
                case 6:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_TONER).ToString();
                    break;
                case 7:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OPEN).ToString();
                    break;
                case 8:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_JAMMED).ToString();
                    break;
                case 9:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OFFLINE).ToString();
                    break;
            }

            ShowError(errorMessage, null, true);
        }

        private void OnBlockCard()
        {
            if (StationRepository.IsIdCardEnabled)
                QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.BLOCK_CARD_QUESTION).ToString(), null, null, BlockClick, null);
            else
                QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.BLOCK_BARCODECARD_QUESTION).ToString(), null, null, BlockClick, null);
        }

        [WsdlServiceSyncAspect]
        private void BlockClick(object sender, EventArgs ev)
        {
            try
            {
                var cards = WsdlRepository.GetIdCardInfo(EditUser.AccountId, Role.User);

                var result = "";
                bool isDone = true;
                if (cards != null)
                {
                    foreach (var card in cards.Where(x => x.active == "1"))
                    {
                        result = WsdlRepository.UpdateIdCard(card.number, "1", false, null);
                        if (result != null && Boolean.Parse(result))
                        {
                            isDone = Boolean.Parse(result) && isDone;
                        }
                    }
                }
                if (isDone)
                {
                    EditUser.HasCard = 0;
                    QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString(), null, null, null, null, false);
                }
            }
            catch (FaultException<HubServiceException> exception)
            {
                ShowError(exception.Detail.message);
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }

            OnPropertyChanged("IsCardButtonsActive");
            OnPropertyChanged("BlockCardButtonsActive");
            OnPropertyChanged("IsEnabledBindCard");
            OnPropertyChanged("ActiveCardText");
            OnPropertyChanged("EditUser");

        }


        private void OnChangePassword()
        {
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            MyRegionManager.NavigateUsingViewModel<UserChangePasswordViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnHistory()
        {
            WaitOverlayProvider.ShowWaitOverlay();

            MyRegionManager.NavigateUsingViewModel<UserHistoryViewModel>(RegionNames.UsermanagementContentRegion);

        }

        private void OnActivate()
        {
            if (!EditUser.IsVerified)
            {
                var text = TranslationProvider.Translate(MultistringTags.CHECK_DOCUMENTS_OF_USER).ToString();
                QuestionWindowService.ShowMessage(text, null, null, verifyUserClick, null);
            }
            else
            {
                ToggleAccountState();
            }

        }

        [WsdlServiceSyncAspect]
        private void ToggleAccountState()
        {
            EditUser.Active = WsdlRepository.ToggleAccountState(EditUser.AccountId, (int)ChangeTracker.CurrentUser.AccountId);
            OnPropertyChanged("ActivateText");
            OnPropertyChanged("ActiveUserText");
            OnPropertyChanged("IsUserVerified");
            OnPropertyChanged("IsEnabledBindCard");
            OnPropertyChanged("IsCardButtonsActive");
            OnPropertyChanged("BlockCardButtonsActive");
        }

        [WsdlServiceSyncAspect]
        private void verifyUserClick(object sender, EventArgs e)
        {
            WsdlRepository.EndUserVerification(StationRepository.GetUid(new LoggedInUser(EditUser.AccountId, null, 0, 0, 0, 0)));
            EditUser.IsVerified = true;
            OnPropertyChanged("VerifiedUser");
            OnPropertyChanged("ActivateText");
            OnPropertyChanged("ActiveUserText");
            OnPropertyChanged("IsUserVerified");
            OnPropertyChanged("IsEnabledBindCard");
            OnPropertyChanged("IsCardButtonsActive");
            OnPropertyChanged("BlockCardButtonsActive");
            OnPropertyChanged("EnableActivationButton");
        }



        private void OnBackCommand()
        {
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }



        #endregion
    }
}