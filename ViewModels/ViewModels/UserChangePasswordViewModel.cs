using System.Collections.Generic;
using System.Windows.Controls;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class UserChangePasswordViewModel : BaseViewModel
    {
        public UserChangePasswordViewModel()
        {
            ChangePasswordCommand = new Command(OnChangePasswordExecute);
            BackCommand = new Command(OnBack);
            SelectionChanged = new Command<object>(OnSelectionChanged);
            NewPassword.Validate += NewPassword_Validate;
            NewPasswordConfirmed.Validate += NewPassword_Validate;
        }

        List<string> NewPassword_Validate(object sender, string property)
        {
           return  ValidateFields();
        }

        #region Properties

        public bool _isEnabledChange;
        private bool _isFocused;
        private MyModelBase _oldPassword = new MyModelBase();
        private MyModelBase _newPassword = new MyModelBase();
        private MyModelBase _newPasswordConfirmed = new MyModelBase();

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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
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
        public Command BackCommand { get; private set; }
        public Command<object> SelectionChanged { get; private set; }

        #endregion

        #region Methods

        protected List<string> ValidateFields()
        {
            NewPassword.Error = null;
            List<string> validationResults = new List<string>();
            if (string.IsNullOrEmpty(NewPassword.Value))
                validationResults.Add("field is required");
            if (string.IsNullOrEmpty(NewPasswordConfirmed.Value))
                validationResults.Add("field is required");
            if (NewPassword.Value != NewPasswordConfirmed.Value)
            {
                NewPassword.Error = TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT);
                NewPasswordConfirmed.Error = TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT);

                validationResults.Add(TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT));
                validationResults.Add(TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT));
            }
            IsEnabledChange = validationResults.Count == 0;
            return validationResults;
        }

        [WsdlServiceSyncAspect]
        private void OnChangePasswordExecute()
        {
            if (ValidateViewModel())
            {
                var result = WsdlRepository.ChangePasswordFromShop((int)ChangeTracker.CurrentUser.AccountId, StationRepository.GetUid(new LoggedInUser(EditUserId.AccountId, null, 0,0,0,0)), NewPassword.Value);
                if (result)
                    ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
            }
        }

        public override void Close()
        {
            NewPassword.Validate -= NewPassword_Validate;
            NewPasswordConfirmed.Validate -= NewPassword_Validate;

            base.Close();
        }

        private bool ValidateViewModel()
        {
            return ValidateFields().Count == 0;
        }

        private void OnSelectionChanged(object args)
        {
            var textBox = args as TextBox;
            if (textBox != null)
            {
                if (textBox.SelectionStart != textBox.Text.Length)
                    textBox.Select(textBox.Text.Length, 0);
            }
        }


        [AsyncMethod]
        private void OnBack()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            PleaseWaitOnBack();
        }

        private void PleaseWaitOnBack()
        {
            Mediator.SendMessage("", MsgTag.HideKeyboard);
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);

        }


        #endregion

        public string this[string columnName]
        {
            get
            {
                if (string.IsNullOrEmpty(NewPassword.Value))
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                if (string.IsNullOrEmpty(OldPassword.Value))
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                if (OldPassword != NewPassword)
                    return TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT).ToString();

                return string.Empty;
            }
        }

        public string Error { get; private set; }
    }
}