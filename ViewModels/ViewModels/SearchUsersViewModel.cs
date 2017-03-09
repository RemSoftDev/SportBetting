using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportRadar.Common.Collections;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Categories view model.
    /// </summary>
    [ServiceAspect]
    public class SearchUsersViewModel : BaseViewModel
    {

        #region Constructors

        private static Dictionary<string, FieldType> AccountingToFormFieldsMap = new Dictionary<string, FieldType>
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

        public SearchUsersViewModel()
        {

            OpenUserProfileCommand = new Command<FoundUser>(OnOpenUserProfile);
            SearchCommand = new Command(OnSearchExecute);
            ClearCommand = new Command(OnClearCommand);
            UnfocusComand = new Command(OnUnfocus);
            StationRepository.BarcodeScannerTempActive = true;
            ScrollChangedCommand = new Command<double>(ScrollChanged);

            Mediator.Register<valueField[]>(this, OpenUserProfile, MsgTag.OpenUserProfile);

            if (ChangeTracker.Is34Mode)
                ChangeTracker.NeedVerticalRegistrationFields = true;

            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);

            GetRegistrationForm();
        }

        private void LanguageChosen(string obj)
        {
            //GetRegistrationForm();

            foreach (var field in RegistrationFields)
            {
                if (field.Name == "password2")
                    field.Label = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CONFIRM_PASSWORD) + "*";
                else
                    field.Label = TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + field.Name.ToUpperInvariant(), field.Name.ToUpperInvariant())).ToString().Replace("{0}", "");
            }
        }

        private void OnUnfocus()
        {
            IsFocused = false;
        }

        #endregion

        #region Properties

        [WsdlServiceSyncAspectSilent]
        private void RegistrationForm()
        {
            formField[] form = StationRepository.GetRegistrationForm();
            RegistrationFields = InitFields(form);
        }

        private List<global::ViewModels.Registration> registrationFields;
        public List<global::ViewModels.Registration> RegistrationFields
        {
            get { return registrationFields; }
            set
            {
                registrationFields = value;
                OnPropertyChanged("RegistrationFields");
            }
        }

        private bool _isFocused;

        protected FoundUser EditUserId
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }

        public ObservableCollection<FoundUser> FoundUsers
        {
            get { return ChangeTracker.FoundUsers; }
            set
            {
                ChangeTracker.FoundUsers = value;
                OnPropertyChanged("FoundUsers");
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
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
            }
        }

        #endregion

        #region Commands

        public Command<FoundUser> OpenUserProfileCommand { get; private set; }
        public Command SearchCommand { get; private set; }
        public Command ClearCommand { get; private set; }
        public Command UnfocusComand { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }

        #endregion

        #region Methods

        private void GetRegistrationForm()
        {
            RegistrationForm();
            if (RegistrationFields != null)
            {
                var firstOrDefault = RegistrationFields.FirstOrDefault();
                if (firstOrDefault != null) firstOrDefault.IsFocused = true;
            }
            else
                HideWindow();

        }

        private void HideWindow()
        {
            Mediator.SendMessage("", MsgTag.HideKeyboard);
            if (ChangeTracker.CurrentUser is OperatorUser)
            {
                MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
            }
            else
            {
                MyRegionManager.NavigatBack(RegionNames.AuthContentRegion);
            }
        }

        public override void Close()
        {
           
            StationRepository.BarcodeScannerTempActive = false;
            ChangeTracker.OperatorSearchUserViewOpen = false;
            if (ChangeTracker.Is34Mode)
                ChangeTracker.NeedVerticalRegistrationFields = false;
            base.Close();
        }
        public override void OnNavigationCompleted()
        {
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_FORM_USER;
            ChangeTracker.AdminTitle2 = MultistringTags.SEARCH_USERS;
            ChangeTracker.SearchUsersChecked = true;
            ChangeTracker.OperatorSearchUserViewOpen = true;
            //OnSearchExecute();

            base.OnNavigationCompleted();
        }

        private void OpenUserProfile(valueField[] foundUser)
        {
          
            try
            {
                valueField[][] vf = new[] { foundUser };           
                FoundUsers = new ObservableCollection<FoundUser>(ConvertUsers(vf));
                if (FoundUsers.Count == 1)
                {
                    MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);  
                    OnOpenUserProfile (FoundUsers[0]);
                }
            }
            catch
            {
            }
        }

        protected void OnOpenUserProfile (FoundUser foundUser)
        {
            WaitOverlayProvider.ShowWaitOverlay();
            EditUserId = foundUser;
            MyRegionManager.NavigateUsingViewModel<FoundUserProfileViewModel>(RegionNames.UsermanagementContentRegion);
        }


        private void OnClearCommand()
        {
            FoundUsers = new ObservableCollection<FoundUser>();
            foreach (var field in RegistrationFields)
            {
                if (field.Name.ToLower() == "currency" || (field.Name.ToLower() == "tax_number" && !field.Visible))
                {
                    continue;
                }
                if (field.Type == FieldType.Date)
                {
                    DateTime selDate = DateTime.Today;
                    field.Value = " ";
                    continue;
                }
                if (field.Type == FieldType.DropDown || field.Type == FieldType.Selector)
                {
                    field.Value = field.Options.First().Value;
                    continue;
                }
                field.Value = "";
                field.ValueMasked = "";
                field.ValueBool = false;
            }
            ChangeTracker.SearchRequest = new List<criteria>();
        }

        private void OnSearchExecute()
        {
            SearchExecute();
        }

        //private List<criteria> _request = new List<criteria>();

        [WsdlServiceSyncAspect]
        private void SearchExecute()
        {
            FoundUsers = new ObservableCollection<FoundUser>();
            ChangeTracker.SearchRequest = new List<criteria>();

            foreach (var registrationField in RegistrationFields)
            {
                if (registrationField.Value != null && !string.IsNullOrEmpty(registrationField.Value.Trim()))
                    ChangeTracker.SearchRequest.Add(new criteria { name = registrationField.Name, value = registrationField.Value.Trim() });
            }

            if (ChangeTracker.SearchRequest.Count == 0)
                return;

            var result = WsdlRepository.AccountSearch(ChangeTracker.SearchRequest.ToArray(), StationRepository.GetUid(ChangeTracker.CurrentUser));
            FoundUsers = new ObservableCollection<FoundUser>(ConvertUsers(result));
            if (FoundUsers.Count < 1)
                ShowError(TranslationProvider.Translate(MultistringTags.USER_NOT_FOUND).ToString());
        }
        
        private IList<FoundUser> ConvertUsers(valueField[][] result)
        {
            var list = new List<FoundUser>();
            if (result != null)
                foreach (var account in result)
                {
                    var acc = new SportBetting.WPF.Prism.Shared.Models.FoundUser();
                    acc.AccountId = Convert.ToInt32(account.Where(x => x.name.Contains("account_id")).Select(x => x.value).FirstOrDefault());
                    acc.Address = account.Where(x => x.name.Contains("address")).Select(x => x.value).FirstOrDefault();
                    DateTime date = DateTime.Today;

                    DateTime.TryParse(account.Where(x => x.name.Contains("date_of_birth")).Select(x => x.value).FirstOrDefault(), new DateTimeFormatInfo { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out date);

                    acc.DateOfBirth = date;

                    acc.DocumentNumber = account.Where(x => x.name.Contains("document_number")).Select(x => x.value).FirstOrDefault();

                    string accDocumentNumber = account.Where(x => x.name.Contains("document_type")).Select(x => x.value).FirstOrDefault();
                    if (!String.IsNullOrEmpty(accDocumentNumber))
                        acc.DocumentType = TranslationProvider.Translate(MultistringTag.Assign(accDocumentNumber,accDocumentNumber)) as string;
                    else
                        acc.DocumentType = "";

                    acc.EMail = account.Where(x => x.name.Contains("email")).Select(x => x.value).FirstOrDefault();
                    acc.Firstname = account.Where(x => x.name.Contains("firstname")).Select(x => x.value).FirstOrDefault();
                    acc.Lastname = account.Where(x => x.name.Contains("lastname")).Select(x => x.value).FirstOrDefault();
                    acc.Username = account.Where(x => x.name.Contains("username")).Select(x => x.value).FirstOrDefault();
                    acc.Active = account.Where(x => x.name.Contains("active")).Select(x => x.value).FirstOrDefault() == "true";
                    acc.IsVerified = account.Where(x => x.name.Contains("verified")).Select(x => x.value).FirstOrDefault() == "1";
                    acc.Phone = account.Where(x => x.name.Contains("phone")).Select(x => x.value).FirstOrDefault();
                    acc.HasCard = Convert.ToInt32(account.Where(x => x.name.Contains("card_status")).Select(x => x.value).FirstOrDefault());
                    list.Add(acc);
                }
            return list;
        }

        private void OnBackCommand()
        {
            FoundUsers.Clear();
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
        }

        private List<global::ViewModels.Registration> InitFields(formField[] fields)
        {
            List<global::ViewModels.Registration> _fields = new List<global::ViewModels.Registration>();

            foreach (formField accountingField in fields)
            {
                if (!accountingField.searchable)
                {
                    continue;
                }

                global::ViewModels.Registration field = new global::ViewModels.Registration();
                //field.isValidatedEvent += new isValidatedDelegate(field_isValidatedEvent);
                field.ReadOnly = false;
                field.IsEnabled = true;

                field.Visible = !accountingField.hidden;
                field.Name = accountingField.name;
                field.Value = accountingField.value;

                field.Label = TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + field.Name.ToUpperInvariant(),field.Name.ToUpperInvariant())).ToString().Replace("{0}", "");

                if (accountingField.type == null)
                    continue;
                field.Type = AccountingToFormFieldsMap[accountingField.type];
                
                if (accountingField.field_options != null)
                {
                    foreach (fieldOption option in accountingField.field_options)
                    {
                        field.Options.Add(new SelectorValue(option.value, option.id));
                    }

                    if (!field.Options.Any(sv => sv.Name == field.Value))
                    {
                        var svTmp = field.Options.FirstOrDefault();
                        if (svTmp != null)
                        {
                            field.Value = svTmp.Value;
                        }
                    }
                }
                if (accountingField.type == "DROPDOWN")
                {
                    string[] ids = accountingField.default_value.Split(';');
                    foreach (var id in ids)
                    {
                        string mltStr = TranslationProvider.Translate(MultistringTag.Assign(id.ToUpperInvariant(),id.ToUpperInvariant())).ToString();
                        field.Options.Add(new SelectorValue(mltStr, id));
                    }
                }
                else
                {
                    //field.Value = accountingField.default_value;
                    field.Value = "";
                }

                if (accountingField.type == "CURRENCY")
                {
                    field.IsEnabled = false;
                    field.EmptyValidation = field.IsEnabled;

                }

                if (accountingField.type == "PHONE")
                {
                    field.ValueInt = accountingField.value;
                }


                if (accountingField.type == "LANGUAGE")
                {
                    var languages = new SyncObservableCollection<Language>();
                    //LanguageRepository.GetAllLanguages(languages);
                    var lr = new LanguageRepository();
                    lr.GetAllLanguages(languages);
                    foreach (var lang in languages)
                    {
                        field.Options.Add(new SelectorValue(lang.ShortName, lang.ShortName));
                    }
                    field.Value = accountingField.default_value;
                }

                if (field.Type == FieldType.Password)
                {
                    _fields.Add(field);
                    global::ViewModels.Registration secondPasswordField = new global::ViewModels.Registration();
                    field.PasswordConfirmation = secondPasswordField;
                    secondPasswordField.PasswordConfirmation = field;
                    //secondPasswordField.isValidatedEvent += new isValidatedDelegate(field_isValidatedEvent);

                    secondPasswordField.Name = "password2";
                    secondPasswordField.Label = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CONFIRM_PASSWORD) + "*";
                    secondPasswordField.Type = FieldType.Password2;
                    secondPasswordField.IsEnabled = true;
                    secondPasswordField.Mandatory = true;
                    secondPasswordField.Value = "";
                    _fields.Add(secondPasswordField);
                }
                else if (field.Type == FieldType.Date)
                {
                    //if (string.IsNullOrWhiteSpace(field.Value))
                    //{
                    //    field.Value = DateTime.Today.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    //}
                    //DateTime selDate = new DateTime();
                    //DateTime.TryParse(field.Value, new DateTimeFormatInfo() { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out selDate);
                    //field.Value = selDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    field.Value = " ";
                    _fields.Add(field);
                }
                else
                {
                    _fields.Add(field);
                }
                
                if (accountingField.name == "tax_number" && !StationRepository.DisplayTaxNumber)
                {
                    //field.Visible = false;
                    _fields.Remove(field);
                }

            }
            return _fields;
        }

        #endregion
    }
}