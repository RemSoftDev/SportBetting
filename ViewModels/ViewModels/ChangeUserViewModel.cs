using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportRadar.Common.Collections;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using FieldType = SportBetting.WPF.Prism.Models.Interfaces.FieldType;
using Language = SportBetting.WPF.Prism.Shared.Models.Language;
using SelectorValue = SportBetting.WPF.Prism.Models.Interfaces.SelectorValue;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ChangeUserProfile view model.
    /// </summary>
    [ServiceAspect]
    public class ChangeUserViewModel : BaseViewModel
    {
        private static readonly Dictionary<string, FieldType> AccountingToFormFieldsMap = new Dictionary<string, FieldType>
        {
            { "EMAIL", FieldType.EMail }, 
            { "STRING", FieldType.Text }, 
            { "PHONE", FieldType.Numeric }, 
            { "NUMBER", FieldType.Numeric }, 
            { "DATE", FieldType.Date },
            { "PASSWORD", FieldType.Password },
            { "CURRENCY", FieldType.Selector }, 
            { "LANGUAGE", FieldType.Selector }, 
            { "DROPDOWN", FieldType.DropDown },
        };


        private readonly List<string> registrationCheck = new List<string>();

        protected FoundUser EditUser
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeUserViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public ChangeUserViewModel()
        {
            if (ChangeTracker.Is34Mode)
                ChangeTracker.NeedVerticalRegistrationFields = true;

            HidePleaseWait = false;
            ChangeUserProfileCommand = new Command(OnChangeUserProfile);
            CancelEditingProfileCommand = new Command(OnCancelEditingProfile);
            SaveUserProfileCommand = new Command(OnSave);
            Mediator.Register<long>(this, CancelEditingProfile, "DisableEdit");
            Mediator.Register<long>(this, EnableEdit, MsgTag.EnableEdit);
            Mediator.Register<long>(this, SaveUserProfile, MsgTag.SaveUserProfile);

        }

        #endregion

        #region Properties

        public bool _isEnabledSave;
        private bool _isFocused;

        private static ILanguageRepository LanguageRepository
        {
            get { return IoCContainer.Kernel.Get<ILanguageRepository>(); }
        }

        public bool IsMouseOver
        {
            get { return _isMouseOver; }
            set
            {
                if (value.Equals(_isMouseOver)) return;
                _isMouseOver = value;
            }
        }

        public bool IsEnabledForEditing
        {
            get { return _isEnabledForEditing; }
            set
            {
                _isEnabledForEditing = value;
                OnPropertyChanged("IsEnabledForEditing");
            }
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
                else if (!IsMouseOver)
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
            }
        }

        public bool IsEnabledSave
        {
            get { return _isEnabledSave; }
            set
            {
                _isEnabledSave = value;
                OnPropertyChanged("IsEnabledSave");
            }
        }

        public List<global::ViewModels.Registration> ChangeUserProfileFields
        {
            get { return _changeUserProfileFields; }
            set
            {
                _changeUserProfileFields = value;
                OnPropertyChanged("ChangeUserProfileFields");
            }
        }

        protected FoundUser EditUserId
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }

        #endregion

        #region Commands

        public Command SaveUserProfileCommand { get; private set; }
        public Command ChangeUserProfileCommand { get; private set; }
        public Command CancelEditingProfileCommand { get; private set; }

        #endregion

        #region Methods

        private bool _isEnabledForEditing;
        private bool _isMouseOver;
        private List<global::ViewModels.Registration> _changeUserProfileFields;


        private void OnChangeUserProfile()
        {
            IsEnabledForEditing = true;

            List<global::ViewModels.Registration> temp = new List<global::ViewModels.Registration>(ChangeUserProfileFields);
            foreach (var field in temp)
            {
                field.IsEnabled = true;
                field.EmptyValidation = field.IsEnabled;

            }

            ChangeUserProfileFields = new List<global::ViewModels.Registration>(temp);
        }

        [AsyncMethod]
        private void OnCancelEditingProfile()
        {
            PleaseWaitCancelEditingProfile();
        }

        [WsdlServiceSyncAspect]
        private void PleaseWaitCancelEditingProfile()
        {
            IsEnabledForEditing = false;
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            foreach (var field in ChangeUserProfileFields)
            {
                field.IsEnabled = false;
                field.EmptyValidation = field.IsEnabled;
            }
            profileForm form = WsdlRepository.LoadProfile(StationRepository.GetUid(new LoggedInUser(EditUserId.AccountId, null, 0,0,0,0)));
            ChangeUserProfileFields = InitFields(form.fields);
        }

        [AsyncMethod]
        private void OnSave()
        {
            PleaseWaitOnSave();
        }
        [WsdlServiceSyncAspect]
        private void PleaseWaitOnSave()
        {
            string result = "";
            try
            {
                valueForm form = Values();
                result = WsdlRepository.UpdateProfile((int)ChangeTracker.CurrentUser.AccountId, StationRepository.GetUid(new LoggedInUser(EditUserId.AccountId, null, 0, 0, 0, 0)), form);
            }
            catch (FaultException<HubServiceException> ex)
            {
                switch (ex.Detail.code)
                {
                    case 101:
                        foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields)
                        {
                            if (registrationField.Name == "username")
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.USERNAME_ALREADY_REGISTERED).ToString();
                                break;
                            }
                        }
                        break;
                    case 102:
                        foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields)
                        {
                            if (registrationField.Name == "email")
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.MAIL_ALREADY_REGISTERED).ToString();
                                break;
                            }
                        }
                        break;
                    case 103:
                        if (ex.Message.Contains("date_of_birth"))
                        {
                            foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "date_of_birth"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("firstname"))
                        {
                            foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "firstname"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("lastname"))
                        {
                            foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "lastname"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("password"))
                        {
                            foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "password"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("email"))
                        {
                            foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "email"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_EMAIL_WRONGFORMAT).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("address"))
                        {
                            foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "address"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_ADRESS).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("document_number"))
                        {
                            foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "document_number"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_DOCUMENT_NUMBER).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("phone"))
                        {
                            foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "phone"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_PHONE_NUMBER).ToString();
                                break;
                            }
                        }
                        break;
                    case 104:
                        foreach (global::ViewModels.Registration registrationField in ChangeUserProfileFields)
                        {
                            if (registrationField.Name == "username")
                            {
                                registrationField.ErrorText = ex.Reason.Translations.Where(x => x.XmlLang.ToLowerInvariant() == SelectedLanguage.ToLowerInvariant()).Select(x => x.Text).FirstOrDefault();
                                break;
                            }
                        }
                        break;
                    default:
                        ShowError(ex.Detail.message);
                        break;
                }
            }
            if (result == "true")
            {
                IsEnabledForEditing = false;
                Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);

                List<global::ViewModels.Registration> temp = new List<global::ViewModels.Registration>(ChangeUserProfileFields);
                foreach (var field in temp)
                {
                    field.IsEnabled = false;
                    field.IsFocused = false;
                    field.EmptyValidation = field.IsEnabled;
                }

                ChangeUserProfileFields = new List<global::ViewModels.Registration>(temp);
            }
        }


        private void EnableEdit(long obj)
        {
            IsEnabledForEditing = true;
            foreach (var field in ChangeUserProfileFields)
            {
                field.IsEnabled = true;
                field.EmptyValidation = field.IsEnabled;
            }
        }

        private void SaveUserProfile(long obj)
        {
            IsEnabledForEditing = false;
            OnSave();
        }


        [WsdlServiceSyncAspect]
        public void CancelEditingProfile(long obj)
        {
            IsEnabledForEditing = false;
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            foreach (var field in ChangeUserProfileFields)
            {
                field.IsEnabled = false;
                field.EmptyValidation = field.IsEnabled;
            }
            profileForm form = WsdlRepository.LoadProfile(StationRepository.GetUid(new LoggedInUser(EditUserId.AccountId, null, 0, 0, 0, 0)));
            ChangeUserProfileFields = InitFields(form.fields);
        }

        private void GetChangeUserProfileForm(long obj)
        {
            ChangeUserProfileForm();
        }

        [WsdlServiceSyncAspect]
        private void ChangeUserProfileForm()
        {
            if (EditUserId != null)
            {
                profileForm form = WsdlRepository.LoadProfile(StationRepository.GetUid(new LoggedInUser(EditUserId.AccountId, null, 0, 0, 0, 0)));
                ChangeUserProfileFields = InitFields(form.fields);
                if (EditUser != null) EditUser.IsVerified = form.fields.Where(x => x.name == "verified").Select(x => x.value).FirstOrDefault() == "1";
            }
        }

        private List<global::ViewModels.Registration> InitFields(IEnumerable<formField> fields)
        {
            List<global::ViewModels.Registration> _fields = new List<global::ViewModels.Registration>();
            List<global::ViewModels.Registration> tempFields = new List<global::ViewModels.Registration>(); 

            foreach (formField accountingField in fields)
            {
                if (accountingField.type == null)
                    continue;

                global::ViewModels.Registration field = new global::ViewModels.Registration();
                field.isValidatedEvent += field_isValidatedEvent;
                field.ReadOnly = accountingField.@readonly;
                field.IsEnabled = false;
                field.EmptyValidation = field.IsEnabled;
                field.Visible = !accountingField.hidden;
                field.Name = accountingField.name;
                field.Value = accountingField.value;

                field.Label = TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + field.Name.ToUpperInvariant(), field.Name.ToUpperInvariant())).ToString().Replace("{0}", "");

                field.Rules = new List<fieldValidationRule>();
                field.Type = AccountingToFormFieldsMap[accountingField.type];
                if (accountingField.validation_rules != null)
                {
                    foreach (fieldValidationRule rule in accountingField.validation_rules)
                    {
                        field.Rules.Add(rule);
                    }
                }

                if (accountingField.field_options != null)
                {
                    foreach (fieldOption option in accountingField.field_options)
                    {
                        field.Options.Add(new SelectorValue(option.value, option.id));
                    }
                    if (accountingField.field_options.Length == 0)
                    {
                        field.Options.Add(new SelectorValue(accountingField.value, accountingField.value));
                    }

                    if (field.Options.All(sv => sv.Name != field.Value))
                    {
                        var svTmp = field.Options.FirstOrDefault();
                        if (svTmp != null)
                        {
                            field.Value = svTmp.Name;
                        }
                    }
                }
                else
                {
                    field.Options.Add(new SelectorValue(accountingField.value, accountingField.value));
                    if (field.Options.All(sv => sv.Name != field.Value))
                    {
                        var svTmp = field.Options.FirstOrDefault();
                        if (svTmp != null)
                        {
                            field.Value = svTmp.Name;
                        }
                    }
                }

                if (accountingField.type == "DROPDOWN")
                {
                    if (!String.IsNullOrEmpty(accountingField.value))
                        field.Value = TranslationProvider.Translate(MultistringTag.Assign(accountingField.value.ToUpperInvariant(), accountingField.value.ToUpperInvariant())).ToString();
                    else
                        field.Value = "";
                }
                if (accountingField.type == "PHONE")
                {
                    field.ValueInt = accountingField.value;
                }

                if (accountingField.type == "CURRENCY")
                {
                    field.Value = StationRepository.Currency;
                }

                if (accountingField.type == "LANGUAGE")
                {
                    var languages = new SyncObservableCollection<Language>();
                    LanguageRepository.GetAllLanguages(languages);
                    foreach (var lang in languages)
                    {
                        field.Options.Add(new SelectorValue(lang.ShortName, lang.ShortName));
                    }
                    field.Value = accountingField.value;
                }
                else
                {
                    _fields.Add(field);
                }
                field.Mandatory = accountingField.mandatory;
                if (field.Mandatory)
                    field.Label += "*";
                if (field.Type == FieldType.Date)
                {
                    DateTime selDate = new DateTime();
                    DateTime.TryParse(field.Value, new DateTimeFormatInfo { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out selDate);
                    field.Value = selDate.ToString("dd.MM.yyyy");
                }
                if (accountingField.name == "tax_number" && !StationRepository.DisplayTaxNumber)
                {
                    field.Visible = false;
                }
                if (accountingField.name != "bet_acceptance_checked" && accountingField.name != "terms_and_cond_version")
                {
                    tempFields.Add(field);
                }
            }

            return tempFields;
        }

        private void field_isValidatedEvent(Registration item, bool val)
        {
            if (!val && !registrationCheck.Contains(item.Name))
                registrationCheck.Add(item.Name);
            else if (val && registrationCheck.Contains(item.Name))
                registrationCheck.Remove(item.Name);
            IsEnabledSave = registrationCheck.Count == 0;
            if (ChangeUserProfileFields != null)
            {
                if (item.Name == "password")
                {
                    foreach (var registrationField in ChangeUserProfileFields)
                    {
                        if (registrationField.Type == FieldType.Password2)
                        {
                            registrationField.Value = registrationField.Value + " ";
                            registrationField.Value = registrationField.Value.Remove(registrationField.Value.Count() - 1);
                        }
                    }
                }
            }
        }

        public valueForm Values()
        {
            valueForm valueForm = new valueForm();
            List<valueField> temp = new List<valueField>();
            //valueForm.fields = new valueFields();
            foreach (global::ViewModels.Registration entry in ChangeUserProfileFields)
            {
                if (entry.Type == FieldType.Date)
                {
                    valueField field = new valueField();
                    field.name = entry.Name;
                    DateTime selDate;
                    DateTime.TryParse(entry.Value, new DateTimeFormatInfo { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out selDate);
                    field.value = selDate.ToString("dd.MM.yyyy");
                    if (string.IsNullOrEmpty(field.value))
                        field.value = "";
                    temp.Add(field);
                }
                else if (entry.Name != "password2")
                {
                    valueField field = new valueField();
                    field.name = entry.Name;
                    if (entry.Name == "card_pin_enabled" && entry.Value == "")
                    {
                        field.value = "1";
                    }
                    else
                    {
                        field.value = entry.Value;
                    }
                    if (string.IsNullOrEmpty(field.value))
                        field.value = "";
                    temp.Add(field);
                }
            }

            valueForm.fields = temp.ToArray();
            return valueForm;
        }

        public override void OnNavigationCompleted()
        {
            GetChangeUserProfileForm(0);

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            if (ChangeTracker.Is34Mode)
                ChangeTracker.NeedVerticalRegistrationFields = false;
            base.Close();
        }

        #endregion
    }
}