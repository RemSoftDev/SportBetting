using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportRadar.Common.Collections;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Windows;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class CreateOperatorViewModel : BaseViewModel
    {
        #region CardStatus enum

        public enum CardStatus
        {
            Ready = 0,
            NotReady = 1,
            AlreadyInUse = 2
        };

        #endregion



        private bool _isFieldsReady;
        private bool _isFocused;
        private CardStatus _status;
        private string _text;
        private string pin = "";
        private List<string> registrationCheck = new List<string>();


        public CreateOperatorViewModel()
        {

            RegisterCommand = new Command(OnRegisterExecute);
            ClearCommand = new Command(OnClearExecute);
            BackCommand = new Command(OnBack);
            BindCardCommand = new Command(BindCard);

            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);

            if (ChangeTracker.Is34Mode)
                ChangeTracker.NeedVerticalRegistrationFields = true;


            Status = CardStatus.NotReady;
        }

        public global::ViewModels.Registration ConfirmPassword { get; set; }

        public global::ViewModels.Registration Password { get; set; }
        public global::ViewModels.Registration Language { get; set; }

        public global::ViewModels.Registration Username { get; set; }
        public global::ViewModels.Registration FirstName { get; set; }
        public global::ViewModels.Registration LastName { get; set; }
        public global::ViewModels.Registration Email { get; set; }
        public global::ViewModels.Registration OperatorType { get; set; }

        public CardStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public bool IsEnabledBindCard
        {
            get { return _isEnabledBindCard; }
            set
            {
                _isEnabledBindCard = value;
                OnPropertyChanged("IsEnabledBindCard");
            }
        }

        private bool _isEnabledBindCard;

        public bool IsReadyToRegister
        {
            get { return IsFieldsReady && IsEnabledRegister; }
        }

        public bool IsFieldsReady
        {
            get { return _isFieldsReady; }
            set
            {
                _isFieldsReady = value;
                OnPropertyChanged("IsFieldsReady");
                OnPropertyChanged("IsReadyToRegister");
            }
        }

        private Visibility _areFieldsVisible = Visibility.Visible;
        public Visibility AreFieldsVisible
        {
            get { return _areFieldsVisible; }
            set
            {
                _areFieldsVisible = value;
                OnPropertyChanged("AreFieldsVisible");
            }
        }

        private Visibility _isWarningVisible = Visibility.Collapsed;
        public Visibility IsWarningVisible
        {
            get { return _isWarningVisible; }
            set
            {
                _isWarningVisible = value;
                OnPropertyChanged("IsWarningVisible");
            }
        }

        public string CardNumber
        {
            get { return ChangeTracker.CardNumber; }
            set { ChangeTracker.CardNumber = value; }
        }

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

       
        public bool _isEnabledRegister = true;
        public bool IsEnabledRegister
        {
            get { return _isEnabledRegister; }
            set
            {
                _isEnabledRegister = value;
                OnPropertyChanged("IsReadyToRegister");
                OnPropertyChanged("IsEnabledRegister");
            }
        }
        /// <summary>
        /// Gets or sets the DocNumber.
        /// </summary>
        public List<global::ViewModels.Registration> FormFields
        {
            get { return _formFields; }
            set
            {
                _formFields = value;
                OnPropertyChanged();
            }
        }

        public Command RegisterCommand { get; private set; }
        public Command ClearCommand { get; private set; }
        public Command BackCommand { get; private set; }
        public Command BindCardCommand { get; private set; }
        private static ILanguageRepository LanguageRepository
        {
            get { return IoCContainer.Kernel.Get<ILanguageRepository>(); }
        }

        private void LanguageChosen(string obj)
        {
            //Initfields();

            foreach (var registration in FormFields)
            {
                var tag = MultistringTag.Assign("TERMINAL_FORM_" + registration.Name.ToUpperInvariant(), registration.Name.ToUpperInvariant());
                registration.Label = TranslationProvider.Translate(tag) as string;

                registration.isValidatedEvent += field_isValidatedEvent;
                if (registration.Mandatory)
                    registration.Label += "*";

            }
        }

        public override void OnNavigationCompleted()
        {
            Initfields();

            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ADMINISTRATION;
            ChangeTracker.AdminTitle2 = MultistringTags.REGISTER_OPERATOR;
            ChangeTracker.RegisterOperatorChecked = true;

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            if(ChangeTracker.Is34Mode)
                ChangeTracker.NeedVerticalRegistrationFields = false;
            base.Close();
        }

        private void Initfields()
        {
            var fields = new List<global::ViewModels.Registration>();

            global::ViewModels.Registration fieldUsername = new global::ViewModels.Registration();
            fieldUsername.Mandatory = true;
            fieldUsername.Name = "username";
            fieldUsername.Type = FieldType.Text;
            fieldUsername.Rules = new List<fieldValidationRule> { new fieldValidationRule { name = "MAX", value = "255" }, new fieldValidationRule { name = "MIN", value = "3" } };
            fields.Add(fieldUsername);
            Username = fieldUsername;
            Username.IsFocused = true;

            global::ViewModels.Registration fieldPassword = new global::ViewModels.Registration();
            fieldPassword.Mandatory = true;
            fieldPassword.Name = "password";
            fieldPassword.Type = FieldType.Password;
            fieldPassword.Rules = new List<fieldValidationRule> { new fieldValidationRule { name = "MAX", value = "35" }, new fieldValidationRule { name = "MIN", value = "3" } };
            fields.Add(fieldPassword);
            Password = fieldPassword;



            global::ViewModels.Registration fieldConfirmPassword = new global::ViewModels.Registration();
            fieldConfirmPassword.Mandatory = true;
            fieldConfirmPassword.Name = "confirm_password";
            fieldConfirmPassword.Type = FieldType.Password2;
            fieldConfirmPassword.Rules = new List<fieldValidationRule> { new fieldValidationRule { name = "MAX", value = "35" }, new fieldValidationRule { name = "MIN", value = "3" }, new fieldValidationRule() };
            fieldConfirmPassword.PasswordConfirmation = fieldPassword;
            fieldPassword.PasswordConfirmation = fieldConfirmPassword;
            fields.Add(fieldConfirmPassword);
            ConfirmPassword = fieldConfirmPassword;


            global::ViewModels.Registration fieldFirstName = new global::ViewModels.Registration();
            fieldFirstName.Name = "first_name";
            fieldFirstName.Mandatory = true;
            fieldFirstName.Type = FieldType.Text;
            fieldFirstName.Rules = new List<fieldValidationRule> { new fieldValidationRule { name = "MAX", value = "35" }, new fieldValidationRule { name = "MIN", value = "3" } };
            fields.Add(fieldFirstName);
            FirstName = fieldFirstName;


            global::ViewModels.Registration fieldLastName = new global::ViewModels.Registration();
            fieldLastName.Name = "last_name";
            fieldLastName.Mandatory = true;
            fieldLastName.Type = FieldType.Text;
            fieldLastName.Rules = new List<fieldValidationRule> { new fieldValidationRule { name = "MAX", value = "35" }, new fieldValidationRule { name = "MIN", value = "3" } };
            fields.Add(fieldLastName);
            LastName = fieldLastName;

            global::ViewModels.Registration fieldEmail = new global::ViewModels.Registration();
            fieldEmail.Name = "email";
            fieldEmail.Type = FieldType.EMail;
            fields.Add(fieldEmail);
            Email = fieldEmail;

            global::ViewModels.Registration fieldOperatorType = new global::ViewModels.Registration();
            OperatorType = fieldOperatorType;
            fieldOperatorType.Name = "operator_type";
            fieldOperatorType.Mandatory = true;
            fieldOperatorType.Type = FieldType.Selector;

            bool isEmpty = true;
            if (ChangeTracker.CurrentUser != null && ChangeTracker.CurrentUser.CreateOperator)
            {
                var opList = WsdlRepository.GetAllRoles(ChangeTracker.CurrentUser.AccountId.ToString());

                if (opList == null || opList.Count() <= 0)
                {
                    fieldOperatorType.Visible = false;
                    fieldOperatorType.IsEnabled = false;
                }

                foreach (UserRole userRole in opList)
                {
                    if (userRole.login_to_terminal == 1)
                    {
                        isEmpty = false;
                        fieldOperatorType.Options.Add(new SelectorValue(userRole.name, userRole.id.ToString())); 
                    }
                }
            }

            //if (ChangeTracker.CurrentUser != null && ChangeTracker.CurrentUser.CreateOperator)
            //    fieldOperatorType.Options.Add(new SelectorValue(TranslationProvider.Translate(MultistringTags.Location_owner).ToString(), "3"));
            //if (ChangeTracker.CurrentUser != null && ChangeTracker.CurrentUser.CreateOperator)
            //    fieldOperatorType.Options.Add(new SelectorValue(TranslationProvider.Translate(MultistringTags.OPERATOR).ToString(), "4"));

            if (!isEmpty)
                fields.Add(fieldOperatorType);
            else
            {
                IsWarningVisible = Visibility.Visible;
                AreFieldsVisible = Visibility.Collapsed;
                Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);                
            }

            fieldOperatorType.Rules = new List<fieldValidationRule>();
            fieldOperatorType.Rules.Add(new fieldValidationRule() { name = "MIN", value = "1" });
            fieldOperatorType.Value = fieldOperatorType.Options.Select(x=>x.Value).FirstOrDefault();
            fieldOperatorType.EmptyValidation = false;


            global::ViewModels.Registration fieldLanguage = new global::ViewModels.Registration();
            Language = fieldLanguage;
            fieldLanguage.Mandatory = true;
            fieldLanguage.Name = "language";
            fieldLanguage.Type = FieldType.DropDown;
            fieldLanguage.Options = new ObservableCollection<SelectorValue>();
            var languages = new SyncObservableCollection<Language>();
            LanguageRepository.GetAllLanguages(languages);
            foreach (var language in languages)
            {
                fieldLanguage.Options.Add(new SelectorValue(language.ShortName, language.ShortName));
            }
            fieldLanguage.Value = fieldLanguage.Options.Select(x => x.Value).FirstOrDefault();
            fieldLanguage.EmptyValidation = false;
            fields.Add(fieldLanguage);

            foreach (var registration in fields)
            {
                registration.IsEnabled = true;
                registration.Value = string.Empty;
                var tag = MultistringTag.Assign("TERMINAL_FORM_" + registration.Name.ToUpperInvariant(), registration.Name.ToUpperInvariant());
                registration.Label = TranslationProvider.Translate(tag) as string;

                registration.isValidatedEvent += field_isValidatedEvent;
                if (registration.Mandatory)
                    registration.Label += "*";

            }
            FormFields = fields;
        }

        private void field_isValidatedEvent(Registration item, bool val)
        {
            if (!val && !registrationCheck.Contains(item.Name))
                registrationCheck.Add(item.Name);
            else if (val && registrationCheck.Contains(item.Name))
                registrationCheck.Remove(item.Name);
            IsFieldsReady = registrationCheck.Count == 0;

            if (FormFields != null)
            {
                if (item.Name == "password")
                {
                    foreach (var registrationField in FormFields)
                    {
                        if (registrationField.Type == FieldType.Password2)
                        {
                            registrationField.ValidateFields();
                        }
                    }
                }
            }
        }

        private int operatorId;
        private List<global::ViewModels.Registration> _formFields;

        [WsdlServiceSyncAspect]
        private void OnRegisterExecute()
        {
            if (!ValidateMyField())
                return;

            pin = "";
            try
            {
                operatorId = WsdlRepository.RegisterOperator(StationRepository.GetUid(ChangeTracker.CurrentUser), FirstName.Value, LastName.Value, Username.Value, Password.Value, Language.Value, Convert.ToInt32(OperatorType.Value), Email.Value);
                if (operatorId > 0)
                {
                    IsEnabledBindCard = true;
                    IsEnabledRegister = false;
                    ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
                }
                else
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.SERVER_ERROR));
                }


            }
            catch (FaultException<HubServiceException> ex)
            {
                switch (ex.Detail.code)
                {
                    case 101:
                        Username.ErrorText = TranslationProvider.Translate(MultistringTags.USERNAME_ALREADY_REGISTERED).ToString();
                        return;
                    default:
                        ShowError(ex.Detail.message);
                        return;

                }
            }


        }
        private void BindCard()
        {
            OnBindOperatorCard();
        }

        private void OnBindOperatorCard()
        {
            Mediator.SendMessage<long>(operatorId, MsgTag.BindOperatorCard);
            OnPropertyChanged("IsCardButtonsActive");
            OnPropertyChanged("BlockCardButtonsActive");
            OnPropertyChanged("IsEnabledBindCard");


        }

        private bool ValidateMyField()
        {
            if (String.IsNullOrEmpty(FirstName.Value.Trim()))
            {
                FirstName.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                return false;
            }
            if (String.IsNullOrEmpty(LastName.Value.Trim()))
            {
                LastName.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                return false;
            }
            if (String.IsNullOrEmpty(Username.Value.Trim()))
            {
                Username.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                return false;
            }
            if (String.IsNullOrEmpty(Password.Value.Trim()))
            {
                Password.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                return false;
            }
            if (String.IsNullOrEmpty(ConfirmPassword.Value.Trim()))
            {
                ConfirmPassword.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED).ToString();
                return false;
            }

            return true;
        }


        private void OnClearExecute()
        {
            foreach (var field in FormFields)
            {
                field.Value = "";
                field.ValueMasked = "";
            }
            pin = "";
            IsEnabledRegister = true;
            IsEnabledBindCard = false;
        }


        private void OnBack()
        {
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }


    }
}