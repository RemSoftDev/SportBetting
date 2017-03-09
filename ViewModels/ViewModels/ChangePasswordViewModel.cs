using System;
using System.Windows;
using System.Windows.Controls;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Collections.Generic;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class ChangePasswordViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordViewModel"/> class.
        /// </summary>
        public ChangePasswordViewModel()
        {
            this.ChangePasswordCommand = new Command(this.OnChangePasswordExecute);
            GeneratePinCommand = new Command(this.OnGeneratePinCommand);
            SelectionChanged = new Command<object>(OnSelectionChanged);
            NewPinCommand = new Command(OnNewPin);
            PinStatusChange = new Command(OnPinStatusChange);
            NewPassword.Validate += NewPassword_Validate;
            NewPasswordConfirmed.Validate += NewPassword_Validate;
            OldPassword.Validate += NewPassword_Validate;
        }

        List<string> NewPassword_Validate(object sender, string property)
        {
            return ValidateFields();
        }



        #region Properties

        private bool _isFocused;

        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                if (_isFocused)
                {
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
            }
        }

        public MyModelBase OldPassword
        {
            get { return _oldPassword; }
            set { _oldPassword = value; }
        }

        public bool IsLoggedInUser
        {
            get { return ChangeTracker.CurrentUser is LoggedInUser; }
        }

        public MyModelBase NewPassword
        {
            get { return _newPassword; }
            set { _newPassword = value; }
        }


        public MyModelBase NewPasswordConfirmed
        {
            get { return _newPasswordConfirmed; }
            set { _newPasswordConfirmed = value; }
        }


        private bool _isEnabledChange;
        private MyModelBase _oldPassword = new MyModelBase();
        private MyModelBase _newPassword = new MyModelBase();
        private MyModelBase _newPasswordConfirmed = new MyModelBase();

        public bool IsEnabledChange
        {
            get { return _isEnabledChange; }
            set
            {
                _isEnabledChange = value;
                OnPropertyChanged("IsEnabledChange");
            }
        }

        protected FoundUser EditUserId
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }

        #endregion

        #region Commands

        public Command ChangePasswordCommand { get; private set; }
        public Command<object> SelectionChanged { get; private set; }
        public Command NewPinCommand { get; private set; }
        public Command GeneratePinCommand { get; private set; }
        public Command PinStatusChange { get; private set; }

        #endregion

        #region Methods

        private void OnPinStatusChange()
        {
            string text = null;
            if (ChangeTracker.UserPinSetting == 0)
            {
                // set to enabled
                text = TranslationProvider.Translate(MultistringTags.TERMINAL_PIN_ENABLED).ToString();
            }
            else
            {
                // set to disabled
                text = TranslationProvider.Translate(MultistringTags.TERMINAL_PIN_DISABLED).ToString();
                if (!StationRepository.IsIdCardEnabled)
                    text = TranslationProvider.Translate(MultistringTags.TERMINAL_BARCODECARD_PIN_DISABLED).ToString();
            }

            var yesButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_OK) as string;
            var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL) as string;
            QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText, ChangePinStatus, null);
        }

        [WsdlServiceSyncAspect]
        private void ChangePinStatus(object sender, EventArgs e)
        {
            valueForm valueForm = new valueForm();
            var fields = new List<valueField>();

            valueField field = new valueField();
            field.name = "card_pin_enabled";
            field.value = IoCContainer.Kernel.Get<IChangeTracker>().UserPinSetting == 0 ? "1" : "0";
            fields.Add(field);
            valueForm.fields = fields.ToArray();

            string result = WsdlRepository.UpdateProfile((int)ChangeTracker.CurrentUser.AccountId, StationRepository.GetUid(ChangeTracker.CurrentUser), valueForm);

            if (result == "true")
            {
                // change button captions / visibility
                IoCContainer.Kernel.Get<IChangeTracker>().UserPinSetting =
                    IoCContainer.Kernel.Get<IChangeTracker>().UserPinSetting == 0 ? 1 : 0;
            }

            OnPropertyChanged("IsPinButtonEnabled");
            OnPropertyChanged("IsPinButtonVisible");
            OnPropertyChanged("PinButtonCaption");
        }

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
            MyRegionManager.NavigateUsingViewModel<PinInsertingViewModel>(RegionNames.UserProfileContentRegion);

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
                ShowPrinterErrorMessage();
                return;
            }
            Random RandNum = new Random();
            int RandomNumber = RandNum.Next(1000, 9999);
            string pin = RandomNumber.ToString();

            try
            {
                WsdlRepository.ChangeIDCardPin(StationRepository.GetUid(ChangeTracker.CurrentUser), ref pin);
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


        protected List<string> ValidateFields()
        {
            List<string> validationResults = new List<string>();
            if (string.IsNullOrEmpty(OldPassword.Value) && IsLoggedInUser)
                validationResults.Add("field is required");
            if (string.IsNullOrEmpty(NewPassword.Value))
                validationResults.Add("field is required");
            if (string.IsNullOrEmpty(NewPasswordConfirmed.Value))
                validationResults.Add("field is required");
            if (NewPassword.Value != NewPasswordConfirmed.Value && !String.IsNullOrEmpty(NewPasswordConfirmed.Value))
            {
                NewPassword.Error = TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT);
                NewPasswordConfirmed.Error = TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT);
                validationResults.Add(TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT));
                validationResults.Add(TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT));

            }
            if (NewPassword.Value == NewPasswordConfirmed.Value)
            {
                NewPassword.Error = null;
                NewPasswordConfirmed.Error = null;

            }

            IsEnabledChange = validationResults.Count == 0;
            return validationResults;
        }


        [WsdlServiceSyncAspect]
        private void OnChangePasswordExecute()
        {

            if (this.ValidateViewModel())
            {
                try
                {
                    if (IsLoggedInUser)
                    {
                        bool result = WsdlRepository.ChangePasswordFromTerminal(StationRepository.GetUid(ChangeTracker.CurrentUser), OldPassword.Value, NewPassword.Value);

                        if (result)
                        {
                            ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
                            //MyRegionManager.NavigateUsingViewModel<ChangeUserProfileViewModel>(RegionNames.UserProfileContentRegion);
                            //Mediator.SendMessage<long>(0, MsgTag.UserProfileInitFields);
                            //ChangeTracker.CheckedProfile = true;
                            OldPassword.ValueMasked = "";
                            NewPassword.ValueMasked = "";
                            NewPasswordConfirmed.ValueMasked = "";
                            IsEnabledChange = false;
                        }
                    }
                    else
                    {
                        var result = WsdlRepository.ChangePasswordFromShop((int)ChangeTracker.CurrentUser.AccountId, StationRepository.GetUid(ChangeTracker.CurrentUser), NewPassword.Value);
                        if (result)
                            ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
                    }
                }
                catch (System.ServiceModel.FaultException<HubServiceException> exception)
                {
                    switch (exception.Detail.code)
                    {
                        case 107:
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALID_OLD_PASSWORD).ToString());
                            break;
                        case 1007:
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PASSWORD_WAS_ALREADY_USED).ToString());
                            break;
                        default:
                            ShowError(exception.Detail.message);
                            break;
                    }
                }
            }

        }

        protected bool ValidateViewModel()
        {
            ValidateFields();
            return IsEnabledChange;
        }

        public void OnSelectionChanged(object args)
        {
            var textBox = args as TextBox;
            if (textBox != null)
            {
                if (textBox.SelectionStart != textBox.Text.Length)
                    textBox.Select(textBox.Text.Length, 0);
            }
        }



        public Visibility IsPinButtonVisible
        {
            get
            {
                if (ChangeTracker.CurrentUser.CardNumber != null && StationRepository.UserCardPinSetting == 3 && ChangeTracker.CurrentUser.CardNumber != "")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public string PinButtonCaption
        {
            get
            {
                if (ChangeTracker.UserPinSetting == 0)
                {
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_PIN_ENABLE).ToString();
                }
                else
                {
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_PIN_DISABLE).ToString();
                }
            }
        }

        public Visibility IsPinChangeButtonVisible
        {
            get { return ChangeTracker.CurrentUser.CardNumber == null || ChangeTracker.CurrentUser.CardNumber == "" ? Visibility.Collapsed : Visibility.Visible; }
        }

        #endregion
    }
}
