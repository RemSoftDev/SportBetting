using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class CardPinViewModel : BaseViewModel
    {
        #region Constructors
        public CardPinViewModel()
        {
            BindCardCommand = new Command(OnBindCard);
            NewPinCommand = new Command(OnNewPin);
            EnablePinCommand = new Command(OnEnablePin);
            GeneratePinCommand = new Command(this.OnGeneratePinCommand);
        }
        #endregion

        #region Properties

        public Visibility IsEnablePinButtonVisible
        {
            get
            {
                if (ChangeTracker.CurrentUser.HasActiveCard && StationRepository.OperatorCardPinSetting == 3 && ChangeTracker.CurrentUser.CardNumber != "")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility IsNewPinButtonVisible
        {
            get
            {
                //if (ChangeTracker.CurrentUser.HasActiveCard && (StationRepository.OperatorCardPinSetting == 3 || StationRepository.OperatorCardPinSetting == 1) && ChangeTracker.CurrentUser.CardNumber != "")
                if (ChangeTracker.CurrentUser.HasActiveCard && (StationRepository.OperatorCardPinSetting == 3 || StationRepository.OperatorCardPinSetting == 1))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public bool ShowBindCard { get { return string.IsNullOrEmpty(ChangeTracker.CurrentUser.CardNumber) || !ChangeTracker.CurrentUser.IsLoggedInWithIDCard; } }

        #endregion

        #region Commands
        public Command BindCardCommand { get; set; }
        public Command NewPinCommand { get; set; }
        public Command EnablePinCommand { get; set; }
        public Command GeneratePinCommand { get; private set; }
        #endregion

        #region Methods


        [WsdlServiceSyncAspect]
        private void OnBindCard()
        {
            try
            {
                Blur();
                var cards = WsdlRepository.GetIdCardInfo((int)ChangeTracker.CurrentUser.AccountId, Role.Operator);

                UnBlur();
                if (cards != null && cards.Any(card => card.active == "1"))
                {
                    var text = TranslationProvider.Translate(MultistringTags.USER_HAVE_ACTIVE_CARD_IT_WILL_BE_BLOCKED).ToString();
                    if (!StationRepository.IsIdCardEnabled)
                        text = TranslationProvider.Translate(MultistringTags.USER_HAVE_ACTIVE_BARCODECARD_IT_WILL_BE_BLOCKED).ToString();
                    QuestionWindowService.ShowMessage(text, null, null, model_YesClick, null);
                }
                else
                {
                    Mediator.SendMessage<long>(ChangeTracker.CurrentUser.AccountId, MsgTag.BindOperatorCard);
                    OnPropertyChanged("IsCardButtonsActive");
                    OnPropertyChanged("BlockCardButtonsActive");
                }

            }
            catch (FaultException<HubServiceException> error)
            {
                UnBlur();
                if (error.Detail.code == 169)
                {
                    Mediator.SendMessage<long>(ChangeTracker.CurrentUser.AccountId, MsgTag.BindOperatorCard);
                    OnPropertyChanged("IsCardButtonsActive");
                    OnPropertyChanged("BlockCardButtonsActive");
                }
                else
                    ShowError(error.Reason.Translations.Where(x => x.XmlLang.ToLowerInvariant() == SelectedLanguage.ToLowerInvariant()).Select(x => x.Text).FirstOrDefault());
            }
            OnPropertyChanged("IsEnablePinButtonVisible");
            OnPropertyChanged("IsNewPinButtonVisible");

            //Mediator.SendMessage<long>(ChangeTracker.CurrentUser.AccountId, MsgTag.BindOperatorCard);
        }



        [AsyncMethod]
        private void model_YesClick(object sender, EventArgs e)
        {
            Mediator.SendMessage<long>(ChangeTracker.CurrentUser.AccountId, MsgTag.BindOperatorCard);
            OnPropertyChanged("IsCardButtonsActive");
            OnPropertyChanged("BlockCardButtonsActive");
            OnPropertyChanged("IsEnablePinButtonVisible");
            OnPropertyChanged("IsNewPinButtonVisible");

        }
        //
        private void OnNewPin()
        {
            var text = TranslationProvider.Translate(MultistringTags.TERMINAL_NEW_PIN).ToString();
            if (!StationRepository.IsIdCardEnabled)
                text = TranslationProvider.Translate(MultistringTags.TERMINAL_NEW_BARCODE_PIN).ToString();

            var yesButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_OK) as string;
            var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL) as string;
            QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText, OnDefinedPIN, null);
        }

        private void OnDefinedPIN(object sender, EventArgs e)
        {
            // MyRegionManager.CloseAllViewsInRegion(RegionNames.UserProfileContentRegion);
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage();
                return;
            }
            MyRegionManager.NavigateUsingViewModel<PinInsertingViewModel>(RegionNames.UsermanagementContentRegion);

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

        [WsdlServiceSyncAspect]
        private void OnRandomPIN(object sender, EventArgs e)
        {
            DoRandomPin();
        }

        [WsdlServiceSyncAspect]
        private void OnGeneratePinCommand()
        {
            DoRandomPin();
        }

        private void DoRandomPin()
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                //ShowPrinterErrorMessage();
                return;
            }
            Random RandNum = new Random();
            int RandomNumber = RandNum.Next(1000, 9999);
            string pin = RandomNumber.ToString();

            try
            {
                WsdlRepository.ChangeOperatorIDCardPin((int)ChangeTracker.CurrentUser.AccountId, ref pin);
                PrinterHandler.PrintPinNote(pin);

            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                switch (exception.Detail.code)
                {
                    case 162:
                        if (StationRepository.IsIdCardEnabled)
                            ShowError(TranslationProvider.Translate(MultistringTags.USER_DONT_HAVE_ACTIVE_CARD) as string);
                        else
                            ShowError(TranslationProvider.Translate(MultistringTags.USER_DONT_HAVE_ACTIVE_BARCODECARD) as string);
                        break;
                }
            }
        }

        private void OnEnablePin()
        {
            var text = ChangeTracker.CurrentUser.PinEnabled
                                         ? TranslationProvider.Translate(
                                             MultistringTags.TERMINAL_FORM_PIN_WILL_BE_DISABLED) as string
                                         : TranslationProvider.Translate(
                                             MultistringTags.TERMINAL_FORM_PIN_WILL_BE_ENABLED) as string;
            if (StationRepository.IsIdCardEnabled)
            {
                text = ChangeTracker.CurrentUser.PinEnabled
                                         ? TranslationProvider.Translate(
                                             MultistringTags.TERMINAL_FORM_BARCODE_PIN_WILL_BE_DISABLED) as string
                                         : TranslationProvider.Translate(
                                             MultistringTags.TERMINAL_FORM_BARCODE_PIN_WILL_BE_ENABLED) as string;
            }

            var yesButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_OK) as string;
            var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL) as string;
            QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText, onEnablePinYesClicked, null);
        }

        [WsdlServiceSyncAspect]
        private void onEnablePinYesClicked(object sender, System.EventArgs e)
        {
            var pinEnbl = ChangeTracker.CurrentUser.PinEnabled;

            ChangeTracker.CurrentUser.PinEnabled = !pinEnbl;

            try
            {
                //WsdlRepository.UpdateOperator(FoundOperator.AccountId, new OperatorCriterias() { active = !FoundOperator.Active, activeSpecified = true });
                WsdlRepository.UpdateOperator((int)ChangeTracker.CurrentUser.AccountId, new OperatorCriterias() { pinEnabled = (sbyte)(ChangeTracker.CurrentUser.PinEnabled ? 1 : 0), pinEnabledSpecified = true });
            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                switch (exception.Detail.code)
                {
                    default: // 113, 114, 172
                        ShowError(exception.Detail.message);
                        return;
                }
            }
        }

        /*
         * Might be needed when User will be ablo not to only generate, but also provide new pin.
         * 
         */
        //private void pleaseWaitOnNewPin()
        //{
        //    Blur();
        //    var cards = WsdlRepository.GetIdCardInfo((int)ChangeTracker.CurrentUser.AccountId, Role.Operator);
        //    UnBlur();
        //    if (cards != null && cards.Any(card => card.active == "1"))
        //    {
        //        Dispatcher.Invoke((Action)(() =>
        //        {
        //            QuestionViewModel model = new QuestionViewModel();
        //            model.YesClick += new EventHandler(newPin_YesClick);
        //            model.Text = TranslationProvider.Translate(MultistringTags.DEFINE_OWN_ID_CARD_PIN).ToString();
        //            var uiVisualizerService = GetService<IUIVisualizerService>();
        //            uiVisualizerService.ShowDialog(model);
        //        }));
        //    }
        //}

        public override void OnNavigationCompleted()
        {

            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ADMINISTRATION;
            ChangeTracker.AdminTitle2 = MultistringTags.CARD_AND_PIN;
            if (!StationRepository.IsIdCardEnabled)
                ChangeTracker.AdminTitle2 = MultistringTags.BARCODECARD_AND_PIN;
            ChangeTracker.CardAndPinChecked = true;
            base.OnNavigationCompleted();
        }

        #endregion
    }
}