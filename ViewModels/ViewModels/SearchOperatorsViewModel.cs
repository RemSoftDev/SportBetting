using System.Collections.Generic;
using System.Collections.ObjectModel;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Categories view model.
    /// </summary>
    [ServiceAspect]
    public class SearchOperatorsViewModel : BaseViewModel
    {

        #region Constructors

        public SearchOperatorsViewModel()
        {
            OpenProfileCommand = new Command<FoundOperator>(OnOpenProfile);
            SearchCommand = new Command(OnSearchExecute);
            ClearCommand = new Command(OnClearCommand);
            UnfocusComand = new Command(OnUnfocus);
            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);
            IsFirstFocused = true;
        }

        private void OnUnfocus()
        {
            IsFocused = false;
            IsFirstFocused = false;
        }

        #endregion

        #region Properties

        private string _eMail = "";
        private string _firstname = "";
        private bool _isFocused;
        private bool _isFirstFocused;
        private string _lastname = "";
        private string _username = "";





        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        public string Firstname
        {
            get { return _firstname; }
            set
            {
                _firstname = value;
                OnPropertyChanged("Firstname");
            }
        }

        public string Lastname
        {
            get { return _lastname; }
            set
            {
                _lastname = value;
                OnPropertyChanged("Lastname");
            }
        }

        public string EMail
        {
            get { return _eMail; }
            set
            {
                _eMail = value;
                OnPropertyChanged("EMail");
            }
        }


        public ObservableCollection<FoundOperator> FoundOperators
        {
            get { return ChangeTracker.FoundOperators; }
            set
            {
                ChangeTracker.FoundOperators = value;
                OnPropertyChanged("FoundOperators");
            }
        }

        public bool IsFirstFocused
        {
            get { return _isFirstFocused; }
            set
            {
                _isFirstFocused = value;
                OpenKeyboard(value);
            }
        }

        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                OpenKeyboard(value);
            }
        }

        #endregion

        #region Commands

        public Command SearchCommand { get; set; }
        public Command ClearCommand { get; set; }
        public Command<FoundOperator> OpenProfileCommand { get; private set; }
        public Command UnfocusComand { get; private set; }

        #endregion

        #region Methods

        private void LanguageChosen(string obj)
        {
            OnPropertyChanged("Username");
            OnPropertyChanged("Firstname");
            OnPropertyChanged("Lastname");
            OnPropertyChanged("EMail");
        }

        public override void OnNavigationCompleted()
        {
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ADMINISTRATION;
            ChangeTracker.AdminTitle2 = MultistringTags.SEARCH_OPERATORS;
            ChangeTracker.SearchOperatorUsersChecked = true;

            base.OnNavigationCompleted();
        }
        private void OnClearCommand()
        {
            FoundOperators = new ObservableCollection<FoundOperator>();
            Username = "";
            Firstname = "";
            Lastname = "";
            EMail = "";
        }

        [AsyncMethod]
        protected void OnOpenProfile(FoundOperator foundOperator)
        {
            WaitOverlayProvider.ShowWaitOverlay();
            PleaseWaitOnOpenProfile(foundOperator);
        }

        protected void PleaseWaitOnOpenProfile(FoundOperator foundOperator)
        {
            ChangeTracker.FoundOperator = foundOperator;
            MyRegionManager.NavigateUsingViewModel<OperatorProfileViewModel>(RegionNames.UsermanagementContentRegion);

        }



        [AsyncMethod]
        private void OnSearchExecute()
        {
            SearchExecute();
        }

        [WsdlServiceSyncAspect]
        private void SearchExecute()
        {
            FoundOperators = new ObservableCollection<FoundOperator>();
            var request = new OperatorCriterias();

            if (!string.IsNullOrEmpty(Username.Trim()))
            {
                request.username = Username.Trim();
            }
            else
            {
                request.username = "";
            }
            if (!string.IsNullOrEmpty(Firstname.Trim()))
            {
                request.name = Firstname.Trim();
            }
            else
            {
                request.name = "";
            }
            if (!string.IsNullOrEmpty(Lastname.Trim()))
            {
                request.surname = Lastname.Trim();
            }
            else
            {
                request.surname = "";
            }
            if (!string.IsNullOrEmpty(EMail.Trim()))
            {
                request.email = EMail.Trim();
            }
            else
            {
                request.email = "";
            }

            try
            {
                var result = WsdlRepository.SearchForOperators(request, StationRepository.GetUid(ChangeTracker.CurrentUser));
                FoundOperators = new ObservableCollection<FoundOperator>(ConvertUsers(result));
            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                switch (exception.Detail.code)
                {
                    case 178:
                        ShowError(TranslationProvider.Translate(MultistringTags.OPERATOR_NOT_FOUND).ToString());
                        return;
                    default: // 113, 114, 172
                        ShowError(exception.Detail.message);
                        return;
                }
            }
            if (FoundOperators.Count < 1)
                ShowError(TranslationProvider.Translate(MultistringTags.OPERATOR_NOT_FOUND).ToString());
        }

        private IEnumerable<FoundOperator> ConvertUsers(Operator[] result)
        {
            var list = new List<FoundOperator>();
            if (result != null)
                foreach (var account in result)
                {
                    if (account.franchisor == StationRepository.FranchiserName)
                    {
                        var acc = new SportBetting.WPF.Prism.Shared.Models.FoundOperator();
                        acc.AccountId = account.id;
                        acc.EMail = account.email;
                        acc.Firstname = account.name;
                        acc.Lastname = account.surname;
                        acc.Username = account.username;
                        acc.Active = account.active;
                        acc.Role = account.role;
                        acc.Location = account.location;
                        acc.Franchisor = account.franchisor;
                        acc.ActiveCard = account.hasActiveCard ?? false;

                        if (acc.ActiveCard)
                        {
                            if (account.card_pin_enabled == 1)
                            {
                                acc.CardPin = TranslationProvider.Translate(MultistringTags.ENABLED_PIN).ToString();
                                if (!StationRepository.IsIdCardEnabled)
                                    acc.CardPin = TranslationProvider.Translate(MultistringTags.BARCODE_ENABLED_PIN).ToString();
                            }
                            else
                            {
                                acc.CardPin = TranslationProvider.Translate(MultistringTags.DISABLED_PIN).ToString();
                                if (!StationRepository.IsIdCardEnabled)
                                    acc.CardPin = TranslationProvider.Translate(MultistringTags.BARCODE_DISABLED_PIN).ToString();
                            }

                        }

                        list.Add(acc);
                    }
                }
            return list;
        }

        private void OpenKeyboard(bool value)
        {
            if (value)
            {
                Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
            }
            else
            {
                Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            }
        }

        #endregion
    }
}