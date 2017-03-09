using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows.Controls;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Authorization Login view model.
    /// </summary>
    [ServiceAspect]
    public class LoginViewModel : BaseViewModel
    {
        private bool PasswordLoginError = false;
        private bool UsernameLoginError = false;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public LoginViewModel()
        {
            _isEnabled = false;
            OpenRegistrationCommand = new Command(OpenRegistrationWindow);
            HideWindowCommand = new Command(HideWindow);
            DoLoginCommand = new Command(Login);
            SelectionChanged = new Command<object>(OnSelectionChanged);
            UnfocusComand = new Command(OnUnfocus);
            Mediator.Register<Tuple<string, string>>(this, DoRaisePropertyChanged, "RaisePropertyChanged");
            Mediator.Register<bool>(this, CloseLogin, MsgTag.CloseLogin);

            Mediator.Register<string>(this, SetFocus, MsgTag.SetFocus);

            UserName.Validate += UserName_Validate;
            Password.Validate += UserName_Validate;

        }

        List<string> UserName_Validate(object sender, string property)
        {
            return ValidateFields();
        }

        private void OnUnfocus()
        {
            IsFocusedLogin = false;
            IsFocusedPassword = false;
        }

        private void SetFocus(string obj)
        {
            if (lastFocus == "login")
                IsFocusedLogin = true;
            else
            {
                IsFocusedPassword = true;
            }
        }

        public override void OnNavigationCompleted()
        {
            //SelectUsername(true);
            IsFocusedLogin = true;

            base.OnNavigationCompleted();
        }

        #endregion

        #region Properties


        private bool _isFocusedPassword;
        private bool _isFocusedLogin;
        

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        public MyModelBase UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        private string lastFocus = "login";
        private bool _isFocusedPanel;
        private MyModelBase _userName = new MyModelBase();
        private MyModelBase _password = new MyModelBase();

        public bool IsFocusedLogin
        {
            get { return _isFocusedLogin; }
            set
            {
                _isFocusedLogin = value;
                OnPropertyChanged();
                if (_isFocusedLogin)
                {
                    lastFocus = "login";
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
            }
        }
        public bool IsFocusedPassword
        {
            get { return _isFocusedPassword; }
            set
            {

                _isFocusedPassword = value;
                OnPropertyChanged();
                if (_isFocusedPassword)
                {
                    lastFocus = "password";
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
            }
        }
        private bool _isEnabled;
        public bool isEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("isEnabled");
            }
        }

        /// <summary>
        /// Gets or sets the Password.
        /// </summary>
        public MyModelBase Password
        {
            get { return _password; }
            set { _password = value; }
        }

        #endregion

        #region Commands

        public Command OpenRegistrationCommand { get; private set; }
        public Command HideWindowCommand { get; private set; }
        public Command DoLoginCommand { get; private set; }
        public Command<object> SelectionChanged { get; private set; }

        public bool IsFocusedPanel
        {
            get { return _isFocusedPanel; }
            set
            {
                _isFocusedPanel = value;
                OnPropertyChanged();
            }
        }

        public Command UnfocusComand { get; private set; }

        #endregion

        #region Methods

        private void OnSelectionChanged(object args)
        {
            var textBox = args as TextBox;
            if (textBox != null)
            {
                if (textBox.SelectionStart != textBox.Text.Length)
                    textBox.Select(textBox.Text.Length, 0);
            }
        }

        private void DoRaisePropertyChanged(Tuple<string, string> tuplePropnameErrormsg)
        {
            //TODO refactoring
            switch (tuplePropnameErrormsg.Item1)
            {
                case "UserName":
                    {
                        UsernameLoginError = true;
                        UserName.Error = tuplePropnameErrormsg.Item2;
                    }
                    break;
                case "PasswordMasked":
                    {
                        PasswordLoginError = true;
                        Password.Error = tuplePropnameErrormsg.Item2;
                    }
                    break;
            }
            Validate(true);
            OnPropertyChanged(tuplePropnameErrormsg.Item1);
        }

        private void OpenRegistrationWindow()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            PleaseWaitOpenRegistrationWindow();
        }

        private void PleaseWaitOpenRegistrationWindow()
        {
            MyRegionManager.NavigateUsingViewModel<RegistrationViewModel>(RegionNames.AuthContentRegion);
        }

        private void HideWindow()
        {
            ClearEnteredData();
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            Dispatcher.Invoke((Action)(() =>
            {
                if (ChangeTracker.LoginWindow == null || !ChangeTracker.LoginWindow.IsVisible)
                    return;
                ChangeTracker.LoginWindow.Close();
            }));
            Mediator.SendMessage(true, MsgTag.AskLoginAnonymous);
        }

        private void Login()
        {
            if (StationRepository.HubSettings.ContainsKey("username_password_auth") && StationRepository.HubSettings["username_password_auth"].Equals("false"))
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_LOGIN_FORBIDDEN).ToString(), null, false, 5);
                return;
            }

            if (UserName.Value == "" || Password.Value == "")
            {
                ShowError(UserName.Value == ""
                    ? TranslationProvider.Translate(MultistringTags.SHOP_FORM_CHECK_USERNAME).ToString()
                    : TranslationProvider.Translate(MultistringTags.INVALID_PASSWORD).ToString()
                    , null, false, 5);
                return;
            }
            DoLogin();
        }


        private int PleaseWaitLogin()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            return DoLogin(UserName.Value, Password.Value);
        }


        public int DoLogin(string UserName, string Password)
        {

            decimal reserved = 0;
            decimal factor;
            decimal cashpool = 0;
            if (ChangeTracker.CurrentUser is AnonymousUser)
            {
                cashpool = WsdlRepository.GetBalance(StationRepository.GetUid(ChangeTracker.CurrentUser), out reserved, out factor) - reserved;
            }

            ClearEverythingAfterUser();

            SessionWS sessionId = WsdlRepository.OpenSession(StationRepository.StationNumber, false, UserName, Password, false);
            string username = sessionId.username;

            var lang = sessionId.default_language;
            string[] permissions = sessionId.permissions;
            string role = sessionId.roleName, roleColor = sessionId.highlight_color;
            int id = sessionId.account_id;

            if (TranslationProvider.CurrentLanguage != lang && lang != null)
            {
                TranslationProvider.CurrentLanguage = lang;
                Mediator.SendMessage(lang, MsgTag.LanguageChosenHeader);
                Mediator.SendMessage(lang, MsgTag.LanguageChosen);
            }

            if (sessionId.session_id == InvalidSessionID || sessionId.session_id == null)
            {
                var previousUser = ChangeTracker.CurrentUser;
                ClearEverythingAfterUser();
                OpenAnonymousSession(false, previousUser);
                return 0;
            }
            else if (permissions != null)
            {
                var user = new OperatorUser(sessionId.session_id) { Username = UserName };
                user.Username = user.Username.Trim(new Char[] { ' ', '@', '.' });
                user.AccountId = id;
                user.RoleID = GetRoleId(sessionId.role_id);
                user.Role = role;
                user.RoleColor = roleColor;
                user.Permissions = permissions;

                IdCardInfoItem[] cards = null;
                try
                {
                    cards = WsdlRepository.GetIdCardInfo(sessionId.account_id, Role.Operator);
                }
                catch (Exception)
                { }

                if (cards != null && cards.SingleOrDefault(c => c.active == "1") != null)
                {
                    user.CardNumber = cards.Single(c => c.active == "1").number;
                    user.HasActiveCard = true;
                }
                else
                {
                    user.CardNumber = null;
                    user.HasActiveCard = false;
                }

                user.PinEnabled = sessionId.card_pin_enabled == 1;
                ChangeTracker.CurrentUser = user;
                return 1;
            }
            else
            {
                if (StationRepository.Active == 0)
                {
                    Mediator.SendMessage(new Tuple<string, string, bool, int>(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN).ToString(), "", false, 3), MsgTag.Error);
                    var previousUser = ChangeTracker.CurrentUser;
                    ClearEverythingAfterUser();
                    OpenAnonymousSession(false, previousUser);
                    return 1;
                }
                foreach (var ticket in TicketHandler.TicketsInBasket.ToSyncList().Where(x => x.TipItems.ToSyncList().Any(c => c.IsChecked)))
                {
                    TicketHandler.OnChangeStake("clear", ticket, ChangeTracker.CurrentUser.Cashpool);
                }
                var user = new LoggedInUser(id, sessionId.session_id, sessionId.balance.amount - sessionId.balance.reserved, sessionId.accountSystemSettings.user_deposit_limit_daily, sessionId.accountSystemSettings.user_deposit_limit_weekly, sessionId.accountSystemSettings.user_deposit_limit_monthly) { Username = UserName };
                user.Permissions = permissions;
                if (!String.IsNullOrEmpty(sessionId.session_id)) user.CardNumber = sessionId.cardNumber;
                ChangeTracker.CurrentUser = user;
                ChangeTracker.CurrentUser.Currency = StationRepository.Currency;
                // user.RoleID = roleId;
                user.Role = role;
                user.RoleColor = roleColor;
                if (cashpool > 0)
                {
                    Mediator.SendMessage<decimal>(cashpool, MsgTag.AskAboutCashPool);
                }
                GetUserPinSettingFromProfile();
                var minLimit = ChangeTracker.CurrentUser.DailyLimit;
                if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                    minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
                if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                    minLimit = ChangeTracker.CurrentUser.MonthlyLimit;
               var notificationText =TranslationProvider.Translate(MultistringTags.USER_LIMIT, minLimit,StationRepository.Currency);
                Mediator.SendMessage(notificationText, MsgTag.ShowNotification);
                Mediator.SendMessage(true, MsgTag.RefreshTicketDetails);

                if (sessionId.force_change_password)
                    return 3;
                else if (sessionId.password_reminder)
                    return 2;

                return 1;
            }
        }




        private void DoChangePassword(object sender, EventArgs e)
        {
            MyRegionManager.NavigateUsingViewModel<ChangePasswordReminderViewModel>(RegionNames.AuthContentRegion);
        }

        private void DoChangePasswordForced(object sender, EventArgs e)
        {
            MyRegionManager.NavigateUsingViewModel<ChangePasswordReminderViewModel>(RegionNames.AuthContentRegion, true);
        }

        private void CancelPasswordChange(object sender, EventArgs e)
        {
            HideWindow();
            WaitOverlayProvider.DisposeAll();
        }

        private void CancelForcedPassChange(object sender, EventArgs e)
        {
            var previousUser = ChangeTracker.CurrentUser;
            ClearEverythingAfterUser();
            OpenAnonymousSession(false, previousUser);
            HideWindow();
        }

        [WsdlServiceSyncAspect]
        private void DoLogin()
        {
            Validate(true);
            try
            {
                int sucess = PleaseWaitLogin();

                if (sucess == 1)
                {
                    HideWindow();
                    if (ChangeTracker.CurrentUser is OperatorUser)
                    {
                        WaitOverlayProvider.ShowWaitOverlay();

                        ChangeTracker.SelectedLive = false;
                        ChangeTracker.SelectedResults = false;
                        ChangeTracker.SelectedTicket = false;
                        ChangeTracker.SelectedSports = false;
                        ChangeTracker.SelectedVirtualSports = false;
                        MyRegionManager.NavigateUsingViewModel<UserManagementViewModel>(RegionNames.ContentRegion);

                    }
                    else if (ChangeTracker.CurrentUser is LoggedInUser)
                    {
                        WaitOverlayProvider.DisposeAll();

                        //check terms and conditions
                        profileForm form = WsdlRepository.LoadProfile(StationRepository.GetUid(ChangeTracker.CurrentUser));
                        if (form != null)
                        {
                            foreach (formField accountingField in form.fields)
                            {
                                if (accountingField.name == "terms_and_cond_version")
                                {
                                    string value = "";
                                    if (accountingField.value == null)
                                        value = "1.0";
                                    else
                                        value = accountingField.value;

                                    if (value != ChangeTracker.TermsAndConditionsVersion)
                                    {
                                        ChangeTracker.NewTermsAccepted = false;
                                        Mediator.SendMessage<bool>(false, MsgTag.AcceptNewTermsVersion);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        WaitOverlayProvider.DisposeAll();
                    }
                }
                else if (sucess == 0)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.SERVER_ERROR) as string);
                }
                else if (sucess == 2) //passwordreminder
                {
                    QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_PASSWORD_NOT_CHANGED).ToString(), null, null, DoChangePassword, CancelPasswordChange);
                }
                else if (sucess == 3) // forced reminder
                {
                    QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_PASSWORD_NOT_CHANGED).ToString(), null, null, DoChangePasswordForced, CancelForcedPassChange);
                }
            }
            catch (FaultException<HubServiceException> exception)
            {
                WaitOverlayProvider.DisposeAll();
                switch (exception.Detail.code)
                {
                    case 111:
                        OnPropertyChanged("PasswordMasked");
                        DoRaisePropertyChanged(new Tuple<string, string>("UserName", TranslationProvider.Translate(MultistringTags.SHOP_FORM_CHECK_USERNAME).ToString()));
                        UsernameLoginError = false;
                        break;
                    case 112:
                        DoRaisePropertyChanged(new Tuple<string, string>("PasswordMasked", TranslationProvider.Translate(MultistringTags.INVALID_PASSWORD).ToString()));
                        PasswordLoginError = false;
                        break;
                    case 113:
                        ShowError(TranslationProvider.Translate(MultistringTags.LOGIN_FRANCHISOR_INCORRECT).ToString());
                        PasswordLoginError = false;
                        break;
                    case 114:
                        DoRaisePropertyChanged(new Tuple<string, string>("UserName", TranslationProvider.Translate(MultistringTags.USER_IS_NOT_VERIFIED).ToString()));
                        PasswordLoginError = false;
                        break;
                    case 115:
                        ShowError(TranslationProvider.Translate(MultistringTags.CANNOT_LOGIN_TO_THIS_STATION).ToString());
                        PasswordLoginError = false;
                        break;
                    case 116:
                        ShowError(TranslationProvider.Translate(MultistringTags.LOGIN_RESTRICTED).ToString());
                        PasswordLoginError = false;
                        break;
                    case 117:
                        ShowError(TranslationProvider.Translate(MultistringTags.USER_INACTIVE).ToString());
                        PasswordLoginError = false;
                        break;
                    case 118:
                        ShowError(TranslationProvider.Translate(MultistringTags.INVALIDLOCATION).ToString());
                        PasswordLoginError = false;
                        break;
                    case 172:
                        PasswordLoginError = false;

                        string sTime = "";
                        string sUsername = "";
                        if (exception.Detail.parameters != null)
                        {
                            //if (exception.Detail.parameters[2].name == "blockedUntil")
                            //{
                            //    sTime = exception.Detail.parameters[2].value;
                            //    long milliSeconds = Int64.Parse(sTime);
                            //    DateTime UTCBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            //    DateTime dt = UTCBaseTime.Add(new TimeSpan(milliSeconds * TimeSpan.TicksPerMillisecond)).ToLocalTime();
                            //    dt = LocalDateTime.Get(dt);
                            //    sTime = dt.ToString("dd.MM.yyyy HH:mm");
                            //    sUsername = exception.Detail.parameters[1].value;
                            //}
                            //else
                            //{
                            //    sTime = exception.Detail.parameters[1].value;
                            //    sUsername = exception.Detail.parameters[1].value;
                            //}

                            for (int i = 0; i < exception.Detail.parameters.Length; i++)
                            {
                                if (exception.Detail.parameters[i].name == "blockedUntil")
                                {
                                    sTime = exception.Detail.parameters[i].value;
                                    long milliSeconds = Int64.Parse(sTime);
                                    DateTime UTCBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                    DateTime dt = UTCBaseTime.Add(new TimeSpan(milliSeconds * TimeSpan.TicksPerMillisecond)).ToLocalTime();
                                    sTime = dt.ToString("dd.MM.yyyy HH:mm");
                                }

                                if (exception.Detail.parameters[i].name == "username")
                                    sUsername = exception.Detail.parameters[i].value;
                            }
                        }
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_USERNAME_BLOCKED, sUsername, sTime));

                        break;
                    case 124:
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_ADMIN_LOGIN_PROHIBITED).ToString());
                        PasswordLoginError = false;
                        break;
                    default: // 113, 114
                        ShowError(exception.Detail.message);
                        break;
                }
            }
        }

        private void Validate(bool b)
        {
            ValidateFields();
        }


        protected List<string> ValidateFields()
        {
            var list = new List<string>();
            // TODO refactoring
            if (string.IsNullOrEmpty(UserName.Value) || UsernameLoginError)
            {
                if (string.IsNullOrEmpty(UserName.Value))
                {
                    UserName.Error = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED);
                    UsernameLoginError = false;
                    list.Add(UserName.Error);
                }

            }
            if (string.IsNullOrEmpty(Password.Value) || PasswordLoginError)
            {
                if (string.IsNullOrEmpty(Password.Value))
                {
                    Password.Error = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_REQUIRED);
                    PasswordLoginError = false;
                    list.Add(Password.Error);
                }
                //validationResults.Add(PasswordErrorMessage);
            }
            if(!string.IsNullOrEmpty(Password.Value) && !string.IsNullOrEmpty(UserName.Value))
            {
                isEnabled = true;
            }
            else
            {
                isEnabled = false;
            }
            return list;
        }


        private void ClearEnteredData()
        {
            IsFocusedLogin = false;
            IsFocusedPassword = false;
            UserName.Value = string.Empty;
            Password.ValueMasked = string.Empty;
        }
        public override void Close()
        {
            UserName.Validate -= UserName_Validate;
            Password.Validate -= UserName_Validate;

            base.Close();
        }
        private void CloseLogin(bool logged)
        {
            //if user logged in by card, close this window
            HideWindow();
        }

        private void SelectUsername(bool select)
        {
            IsFocusedLogin = true;
            OnPropertyChanged("IsFocusedLogin");
        }

        #endregion
    }
}