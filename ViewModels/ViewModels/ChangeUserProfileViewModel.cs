using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using BaseObjects;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportRadar.Common.Collections;
using TranslationByMarkupExtension;
using System.Linq;
using WsdlRepository.WsdlServiceReference;
using SportBetting.WPF.Prism.Models;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// ChangeUserProfile view model.
    /// </summary>
    [ServiceAspect]
    public class ChangeUserProfileViewModel : BaseViewModel
    {
        private static readonly Dictionary<string, FieldType> AccountingToFormFieldsMap = new Dictionary<string, FieldType>
		{
			{"EMAIL", FieldType.EMail},
			{"STRING", FieldType.Text},
			{"PHONE", FieldType.Numeric},
			{"NUMBER", FieldType.Numeric},
			{"DATE", FieldType.Date},
			{"PASSWORD", FieldType.Password},
			{"CURRENCY", FieldType.Text},
            {"LANGUAGE", FieldType.Selector},
			{"DROPDOWN", FieldType.DropDown},
		};

        private readonly ScrollViewerModule _scrollViewerModule;

        private readonly List<string> _registrationCheck = new List<string>();

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeUserProfileViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public ChangeUserProfileViewModel()
        {
            _scrollViewerModule = new ScrollViewerModule(Dispatcher);
            WaitOverlayProvider.ShowWaitOverlay();
            HidePleaseWait = false;

            SaveUserProfileCommand = new Command(OnSave);
            ChangeUserProfileCommand = new Command(OnChangeUserProfile);
            CancelEditingProfileCommand = new Command(OnCancelEditingProfile);
            ScrollDownStart = new Command(OnScrollDownStartExecute);
            ScrollDownStop = new Command(OnScrollDownStopExecute);
            ScrollUpStart = new Command(OnScrollUpStartExecute);
            ScrollUpStop = new Command(OnScrollUpStopExecute);

            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LanguageChosenHeader);

            var scroller = GetScrollviewer();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }

            ChangeUserProfileForm();
        }

        protected new ScrollViewer GetScrollviewer()
        {

            ScrollViewer scrollViewerTmp = null;

            var mainWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.ToString().Contains("UserProfileWindow"));

            if (mainWindow != null)
            {
                scrollViewerTmp = AppVisualTree.FindChild<ScrollViewer>(mainWindow, "Scroller");
            }


            return scrollViewerTmp;
        }


        #endregion

        #region Properties

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
                OnPropertyChanged();
            }
        }

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
                if (value.Equals(_isEnabledSave)) return;
                _isEnabledSave = value;
                OnPropertyChanged("IsEnabledSave");
            }
        }

        /// <summary>
        /// Gets or sets the DocNumber.
        /// </summary>
        public List<global::ViewModels.Registration> ChangeUserProfileFields
        {
            get { return _changeUserProfileFields; }
            set
            {
                _changeUserProfileFields = value;
                OnPropertyChanged("ChangeUserProfileFields");
            }
        }


        /// <summary>
        /// Register the DocNumber property so it is known in the class.
        /// </summary>

        #endregion

        #region Commands

        public Command ChangeUserProfileCommand { get; private set; }
        public Command CancelEditingProfileCommand { get; private set; }
        public Command SaveUserProfileCommand { get; private set; }

        public Command ScrollDownStart { get; private set; }
        public Command ScrollDownStop { get; private set; }
        public Command ScrollUpStart { get; private set; }
        public Command ScrollUpStop { get; private set; }

        #endregion

        #region Methods

        private void OnLanguageChosenExecute(string lang)
        {
            foreach (Registration reg in ChangeUserProfileFields)
            {
                reg.Label = TranslationProvider.Translate(reg.Multistring);
            }
        }

        public override void OnNavigationCompleted()
        {
            DoRefresh(0);
            if (ChangeTracker.IsLandscapeMode) ChangeTracker.FooterArrowsVisible = true;

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            if (ChangeTracker.IsLandscapeMode) ChangeTracker.FooterArrowsVisible = false;
            base.Close();
        }

        [WsdlServiceSyncAspect]
        private void OnSave()
        {
            if (!this.ValidateViewModel())
                return;

            string result = "";
            try
            {
                valueForm form = Values();
                result = WsdlRepository.UpdateProfile(null, StationRepository.GetUid(ChangeTracker.CurrentUser), form);
            }
            catch (FaultException<HubServiceException> ex)
            {
                switch (ex.Detail.code)
                {
                    case 101:
                        foreach (var registrationField in ChangeUserProfileFields)
                        {
                            if (registrationField.Name == "username")
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.USERNAME_ALREADY_REGISTERED).ToString();
                                break;
                            }
                        }
                        break;
                    case 102:
                        foreach (var registrationField in ChangeUserProfileFields)
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
                            foreach (var registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "date_of_birth"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("firstname"))
                        {
                            foreach (var registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "firstname"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("lastname"))
                        {
                            foreach (var registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "lastname"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("password"))
                        {
                            foreach (var registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "password"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("email"))
                        {
                            foreach (var registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "email"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_EMAIL_WRONGFORMAT).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("address"))
                        {
                            foreach (var registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "address"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_ADRESS).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("document_number"))
                        {
                            foreach (var registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "document_number"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_DOCUMENT_NUMBER).ToString();
                                break;
                            }
                        }
                        if (ex.Message.Contains("phone"))
                        {
                            foreach (var registrationField in ChangeUserProfileFields.Where(registrationField => registrationField.Name == "phone"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_PHONE_NUMBER).ToString();
                                break;
                            }
                        }
                        break;
                    case 104:
                        foreach (var registrationField in ChangeUserProfileFields)
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

                var temp = new List<global::ViewModels.Registration>(ChangeUserProfileFields);
                foreach (var field in temp)
                {
                    field.IsEnabled = false;
                    field.IsFocused = false;
                    field.EmptyValidation = field.IsEnabled;

                    if (field.Name == "username")
                    {
                        var loggedInUser = ChangeTracker.CurrentUser as LoggedInUser;
                        if (loggedInUser != null)
                            loggedInUser.Username = field.Value;
                    }
                }

                ChangeUserProfileFields = new List<global::ViewModels.Registration>(temp);
            }
        }

        private bool ValidateViewModel()
        {
            foreach (var registrationField in ChangeUserProfileFields)
            {
                registrationField.ValidateFields();
                if (!string.IsNullOrEmpty(registrationField.Error))
                    return false;
            }
            return true;
        }


        private void OnChangeUserProfile()
        {
            if (ChangeUserProfileFields == null || ChangeUserProfileFields.Count == 0)
            {
                // in case if internet connection lost
                return;
            }
            IsEnabledForEditing = true;
            var temp = new List<global::ViewModels.Registration>(ChangeUserProfileFields);
            foreach (var field in temp)
            {
                if (field.Name != "currency")
                {
                    field.IsEnabled = !field.ReadOnly;
                }
                else
                {
                    field.IsEnabled = false;
                }
                field.EmptyValidation = field.IsEnabled;
            }

            ChangeUserProfileFields = new List<global::ViewModels.Registration>(temp);
        }
        [WsdlServiceSyncAspect]
        private void OnCancelEditingProfile()
        {
            IsEnabledForEditing = false;
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            foreach (var field in ChangeUserProfileFields)
            {
                field.IsEnabled = false;
                field.EmptyValidation = field.IsEnabled;

            }
            profileForm form = WsdlRepository.LoadProfile(StationRepository.GetUid(ChangeTracker.CurrentUser));
            ChangeUserProfileFields = InitFields(form.fields);

        }

        [WsdlServiceSyncAspect]
        private void ChangeUserProfileForm()
        {
            if (ChangeTracker.CurrentUser is LoggedInUser)
            {
                profileForm form = WsdlRepository.LoadProfile(StationRepository.GetUid(ChangeTracker.CurrentUser));
                if (form != null)
                {
                    var fields = InitFields(form.fields);
                    ChangeUserProfileFields = fields;
                    OnChangeUserProfile();
                }

            }
        }

        private void DoRefresh(long obj)
        {
            ChangeUserProfileForm();
        }

        private List<global::ViewModels.Registration> InitFields(IEnumerable<formField> fields)
        {
            List<global::ViewModels.Registration> _fields = new List<global::ViewModels.Registration>();

            foreach (formField accountingField in fields)
            {
                if (accountingField.type == null)
                    continue;

                var field = new global::ViewModels.Registration();
                field.isValidatedEvent += field_isValidatedEvent;
                field.ReadOnly = accountingField.@readonly;
                field.IsEnabled = false;
                field.EmptyValidation = field.IsEnabled;

                field.Name = accountingField.name;
                field.Value = accountingField.value;


                field.Multistring = MultistringTag.Assign("TERMINAL_FORM_" + field.Name.ToUpperInvariant(), field.Name.ToUpperInvariant());
                if (!StationRepository.IsIdCardEnabled && field.Multistring.Value == MultistringTags.TERMINAL_FORM_CARD_PIN_ENABLED.Value)
                    field.Multistring = MultistringTags.TERMINAL_FORM_BARCODECARD_PIN_ENABLED;
                field.Label = TranslationProvider.Translate(field.Multistring);

                field.Rules = new List<fieldValidationRule>();
                field.Type = AccountingToFormFieldsMap[accountingField.type];
                foreach (fieldValidationRule rule in accountingField.validation_rules)
                {
                    field.Rules.Add(rule);
                }

                if (accountingField.type == "DATE")
                {
                    int minAge = 0;
                    try
                    {
                        int.TryParse(StationRepository.HubSettings["min_play_age"], out minAge);
                    }
                    catch
                    {
                    }

                    var rule = new fieldValidationRule() { name = "MIN", value = minAge.ToString() };

                    field.Rules.Add(rule);
                }

                if (accountingField.field_options != null)
                {
                    foreach (fieldOption option in accountingField.field_options)
                    {
                        field.Options.Add(new SelectorValue(option.value, option.id));
                    }


                }
                field.Options.Add(new SelectorValue(accountingField.value, accountingField.value));
                if (field.Options.All(sv => sv.Name != field.Value))
                {
                    var svTmp = field.Options.FirstOrDefault();
                    if (svTmp != null)
                    {
                        field.Value = svTmp.Name;
                    }
                }

                if (accountingField.type == "CURRENCY")
                {
                    field.Value = StationRepository.Currency;
                }

                if (accountingField.type == "DROPDOWN")
                {
                    if (!String.IsNullOrEmpty(accountingField.value))
                        field.Value = TranslationProvider.Translate(MultistringTag.Assign(accountingField.value.ToUpperInvariant(), accountingField.value.ToUpperInvariant())).ToString();
                    else
                        field.Value = "";
                    //field.ReadOnly = false;
                }

                if (accountingField.type == "PHONE")
                {
                    field.ValueInt = accountingField.value;
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
                    DateTime.TryParse(field.Value, new DateTimeFormatInfo() { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out selDate);
                    field.Value = selDate.ToString("dd.MM.yyyy");
                }

                if (field.Name == "card_pin_enabled")
                {
                    if (field.Value == "1")
                    {
                        if (StationRepository.IsIdCardEnabled)
                            field.Value = TranslationProvider.Translate(MultistringTags.ENABLED_PIN).ToString();
                        else
                            field.Value = TranslationProvider.Translate(MultistringTags.BARCODE_ENABLED_PIN).ToString();

                    }
                    else
                    {
                        if (StationRepository.IsIdCardEnabled)
                            field.Value = TranslationProvider.Translate(MultistringTags.DISABLED_PIN).ToString();
                        else
                            field.Value = TranslationProvider.Translate(MultistringTags.BARCODE_DISABLED_PIN).ToString();

                    }
                }
                if (accountingField.name == "tax_number" && !StationRepository.DisplayTaxNumber)
                {
                    field.Visible = false;
                }
                if (accountingField.name == "bet_acceptance_checked")
                {
                    field.Visible = false;
                }
                if (accountingField.name == "terms_and_cond_version")
                {
                    field.Visible = false;
                }

            }
            return _fields;
        }

        void field_isValidatedEvent(Registration item, bool val)
        {
            if (!val && !_registrationCheck.Contains(item.Name))
                _registrationCheck.Add(item.Name);
            else if (val && _registrationCheck.Contains(item.Name))
                _registrationCheck.Remove(item.Name);
            IsEnabledSave = _registrationCheck.Count == 0;
            if (ChangeUserProfileFields != null)
            {
                if (item.Name == "password")
                {
                    for (int index = 0; index < ChangeUserProfileFields.Count; index++)
                    {
                        var registrationField = ChangeUserProfileFields[index];
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
            var fields = new List<valueField>();
            foreach (var entry in ChangeUserProfileFields)
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
                    fields.Add(field);
                }
                else if (entry.Name != "password2" && entry.Name != "card_pin_enabled")
                {
                    valueField field = new valueField();
                    field.name = entry.Name;
                    field.value = entry.Value;
                    if (string.IsNullOrEmpty(field.value))
                        field.value = "";
                    fields.Add(field);
                }
                if (entry.Name == "card_pin_enabled")
                {
                    valueField field = new valueField();
                    field.name = entry.Name;
                    if (entry.Value == TranslationProvider.Translate(MultistringTags.TERMINAL_PIN_ENABLE).ToString()) field.value = "1";
                    else field.value = "0";
                    fields.Add(field);
                }
            }
            valueForm.fields = fields.ToArray();
            return valueForm;
        }

        private void OnScrollDownStartExecute()
        {
            this._scrollViewerModule.OnScrollDownStartExecute(this.GetScrollviewer());
        }
        private void OnScrollDownStopExecute()
        {
            this._scrollViewerModule.OnScrollDownStopExecute();
        }
        private void OnScrollUpStartExecute()
        {
            this._scrollViewerModule.OnScrollUpStartExecute(this.GetScrollviewer());
        }
        private void OnScrollUpStopExecute()
        {
            this._scrollViewerModule.OnScrollUpStopExecute();
        }

        private bool _isMouseOver;
        private bool _isEnabledForEditing;
        private bool _isEnabledSave = true;
        private List<global::ViewModels.Registration> _changeUserProfileFields;

        #endregion
    }
}