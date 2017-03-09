using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using BaseObjects;
using BaseObjects.ViewModels;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Models.Interfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;
using System.Linq;
using WsdlRepository.WsdlServiceReference;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Shared.WpfHelper;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Authorization User Registration view model.
    /// </summary>
    [ServiceAspect]
    public class RegistrationViewModel : BaseViewModel
    {
        private static ILog Log = LogFactory.CreateLog(typeof(RegistrationViewModel));

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
			{"TERMSCONDITIONS", FieldType.TermsConditions},
		};

        private readonly ScrollViewerModule _ScrollViewerModule;

        private List<string> registrationCheck = new List<string>();



        #region Constructors

        public RegistrationViewModel()
        {
            //Mediator.Register<DateTime?>(this, SetDate, MsgTag.RegistrationBirthDate);

            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);
            HideWindowCommand = new Command(HideWindow);
            RegisterCommand = new Command(CheckRegistration);
            ScrollDownStart = new Command(OnScrollDownStartExecute);
            ScrollDownStop = new Command(OnScrollDownStopExecute);
            ScrollUpStart = new Command(OnScrollUpStartExecute);
            ScrollUpStop = new Command(OnScrollUpStopExecute);
            BindCardCommand = new Command(OnBindCard);
            ClearCommand = new Command(OnClearExecute);
            UnfocusComand = new Command(OnUnfocus);
            NextStepCommand = new Command(OnNextStep);
            BackStepCommand = new Command(OnBackStep);
            StepCommand = new Command<StepButton>(OnStepClick);
            IsEnabledRegister = false;
            SelectionChanged = new Command<object>(OnSelectionChanged);
            OpenTermsAndConditions = new Command(OnOpenTermsAndConditions);

            Mediator.Register<bool>(this, CloseView, MsgTag.CloseRegistration);
            Mediator.Register<string>(this, FillTaxNumber, MsgTag.FillTaxNumber);

            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);

            var scroller = this.GetScrollviewer();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }
        }

        private void LanguageChosen(string obj)
        {
            foreach (var field in RegistrationFields)
            {
                var a = field.Type;
                if (field.Name == "password2")
                    field.Label = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CONFIRM_PASSWORD) + "*";
                
                else
                    field.Label = TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + field.Name.ToUpperInvariant(), "")).Replace("{0}", "");
            }
        }

        private void OnStepClick(StepButton obj)
        {
            obj.IsSelected = true;
            RegistrationStep = obj.StepNumber;
            SetVisibleFields(RegistrationStep);

        }

        [PleaseWaitAspect]
        private void OnOpenTermsAndConditions()
        {
            Mediator.SendMessage("", MsgTag.ShowTermsAndConditions);
        }

        private void FillTaxNumber(string number)
        {
            foreach (var registrationField in RegistrationFields)
            {
                if (registrationField.Name == "tax_number")
                {
                    registrationField.Value = number;
                    break;
                }
            }
        }

        private void CloseView(bool logged)
        {
            //if user logged in by card, close this window
            HideWindow();
        }

        private void OnUnfocus()
        {
            foreach (var registrationField in RegistrationFields)
            {
                registrationField.IsFocused = false;
            }

        }

        #endregion

        //private void SetDate(DateTime? dt)
        //{
        //    //foreach (Registration entry in RegistrationFields)
        //    //{
        //    //    if (entry.Type == FieldType.Date)
        //    //    {
        //    //        valueField field = new valueField();
        //    //        field.value = dt.ToString();
        //    //    }
        //    //}
        //    List<Registration> RF=new List<Registration>();

        //    for (int i = 0; i < RegistrationFields.Count; i++)
        //    {
        //        if (RegistrationFields[i].Type == FieldType.Date)
        //        {
        //            RegistrationFields[i].Value = dt.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        //        }
        //    }
        //        OnPropertyChanged("RegistrationFields");
        //}
        #region Properties


        private static ILanguageRepository LanguageRepository
        {
            get { return IoCContainer.Kernel.Get<ILanguageRepository>(); }
        }




        public bool _isEnabledRegister;
        public bool IsEnabledRegister
        {
            get { return _isEnabledRegister; }
            set
            {
                _isEnabledRegister = value;
                OnPropertyChanged("IsEnabledRegister");
            }
        }

        public bool IsEnabledBindCard
        {
            get { return _isEnabledBindCard && ChangeTracker.CurrentUser.BindUserCard; }
            set
            {
                _isEnabledBindCard = value;
                OnPropertyChanged("IsEnabledBindCard");
            }
        }

        private int registrationStep = 1;
        public int RegistrationStep
        {
            get
            {
                if (registrationStep == 0) return 1;
                return registrationStep;
            }
            set { registrationStep = value; OnPropertyChanged("RegistrationStep"); }
        }

        private bool _isEnabledBindCard;

        private ObservableCollection<global::ViewModels.Registration> registrationFields;
        public ObservableCollection<global::ViewModels.Registration> RegistrationFields
        {
            get { return registrationFields; }
            set
            {
                registrationFields = value;
                OnPropertyChanged("RegistrationFields");
            }
        }

        private ObservableCollection<global::ViewModels.Registration> visibleFields = new ObservableCollection<Registration>();
        public ObservableCollection<global::ViewModels.Registration> VisibleFields
        {
            get { return visibleFields; }
            set
            {
                visibleFields = value;
                OnPropertyChanged("VisibleFields");
            }
        }

        private List<global::ViewModels.Registration> activeFields;
        public List<global::ViewModels.Registration> ActiveFields
        {
            get { return activeFields; }
            set
            {
                activeFields = value;
                OnPropertyChanged("ActiveFields");
            }
        }


        protected FoundUser EditUser
        {
            get { return ChangeTracker.EditableUser; }
            set { ChangeTracker.EditableUser = value; }
        }
        public string CardNumber
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>().CardNumber; }
            set { IoCContainer.Kernel.Get<IChangeTracker>().CardNumber = value; }
        }

        private bool _isFocusedTaxNumber = false;

        public bool IsFocusedTaxNumber
        {
            get { return _isFocusedTaxNumber; }
            set
            {
                _isFocusedTaxNumber = value;
                if (_isFocusedTaxNumber)
                {
                    ChangeTracker.CanScanTaxNumber = true;
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                    ChangeTracker.CanScanTaxNumber = false;
            }
        }
        #endregion

        #region Commands

        public Command HideWindowCommand { get; private set; }
        public Command RegisterCommand { get; private set; }
        public Command OpenTermsAndConditions { get; private set; }
        public Command BindCardCommand { get; private set; }
        public Command ClearCommand { get; private set; }
        public Command UnfocusComand { get; private set; }
        public Command NextStepCommand { get; private set; }
        public Command<StepButton> StepCommand { get; private set; }
        public Command BackStepCommand { get; private set; }
        public Command<object> SelectionChanged { get; private set; }

        /// <summary>
        /// Gets the ScrollDownStart command.
        /// </summary>
        public Command ScrollDownStart { get; private set; }
        /// <summary>
        /// Gets the ScrollDownStop command.
        /// </summary>
        public Command ScrollDownStop { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStart command.
        /// </summary>
        public Command ScrollUpStart { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStop command.
        /// </summary>
        public Command ScrollUpStop { get; private set; }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            GetRegistrationForm(0);

            base.OnNavigationCompleted();
            ValidateViewModel();
        }

        [WsdlServiceSyncAspect]
        private void OnBindCard()
        {

            ChangeTracker.BindingCardCancelled = false;
            Mediator.SendMessage<long>(EditUser.AccountId, MsgTag.BindUserCard);
            if (ChangeTracker.BindingCardCancelled == true)
            {
                return;
            }
            try
            {
                var cards = WsdlRepository.GetIdCardInfo(EditUser.AccountId, Role.User);
                if (cards != null && cards.Any(card => card.active == "1"))
                {
                    IsEnabledBindCard = false;
                }
            }
            catch (FaultException<HubServiceException> exception)
            {

                if (exception.Detail.code == 169)
                {
                    if (StationRepository.IsIdCardEnabled)
                        ShowError(TranslationProvider.Translate(MultistringTags.CARD_NOT_FOUND).ToString());
                    else
                        ShowError(TranslationProvider.Translate(MultistringTags.BARCODECARD_NOT_FOUND).ToString());
                }
                else if (exception.Detail.code == 164)
                {
                    if (StationRepository.IsIdCardEnabled)
                        ShowError(TranslationProvider.Translate(MultistringTags.CARD_BLOCKED).ToString());
                    else
                        ShowError(TranslationProvider.Translate(MultistringTags.BARCODECARD_BLOCKED).ToString());
                }
                else if (exception.Detail.code == 172)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.USERNAME_BLOCKED).ToString());
                }
                //else if (exception.Detail.code == 1620)
                //{
                //    // if card is OK, location requires PIN, but user has PIN disabled, 
                //    // then don't show error, only lead to PIN asking window
                //}
                else
                    ShowError(exception.Message);
            }
        }

        private void OnNextStep()
        {
            if (RegistrationStep > 0 && RegistrationStep < RegistrationSteps.Count)
                RegistrationStep++;
            OnStepClick(RegistrationSteps[RegistrationStep - 1]);
        }

        private void OnBackStep()
        {
            if (RegistrationStep > 1 && RegistrationStep <= RegistrationSteps.Count)
                RegistrationStep--;
            OnStepClick(RegistrationSteps[RegistrationStep - 1]);
        }

        private void SetVisibleFields(int step)
        {
            Dispatcher.Invoke(() =>
                {
                    VisibleFields.Clear();
                    for (int i = (step - 1) * 5; i < ActiveFields.Count; i++)
                    {
                        if (VisibleFields.Count > 4)
                            break;
                        VisibleFields.Add(ActiveFields[i]);
                    }
                });

        }


        private void OnClearExecute()
        {
            foreach (var field in RegistrationFields)
            {
                if (field.Name.ToLower() == "currency" || (field.Name.ToLower() == "tax_number" && !field.Visible))
                {
                    continue;
                }
                if (field.Type == FieldType.Date)
                {
                    DateTime selDate = DateTime.Today;
                    field.Value = selDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    continue;
                }
                if (field.Type == FieldType.DropDown || field.Type == FieldType.Selector)
                {

                    if (string.IsNullOrEmpty(field.default_value))
                        field.Value = "";
                    if (field.Name == "document_type")
                    {
                        field.Value = "";
                    }
                    else
                    {
                        field.Value = field.default_value;
                    }
                    continue;
                }
                field.Value = null;
                field.ValueMasked = null;
                field.ValueBool = false;                
            }
        }


        private void model_NoClick(object sender, EventArgs e)
        {
        }


        public override ScrollViewer GetScrollviewer()
        {
            System.Windows.Controls.ScrollViewer scrollViewerTmp = null;

            //foreach (var win in System.Windows.Application.Current.Windows.OfType<DataWindow>())
            //{
            //    System.Diagnostics.Debug.WriteLine(win.ToString());
            //}

            var mainWindow = System.Windows.Application.Current.Windows.OfType<Window>().Where(x => x.ToString().Contains("AuthWindow")).FirstOrDefault();
            if (ChangeTracker.CurrentUser is OperatorUser)
                mainWindow = System.Windows.Application.Current.Windows.OfType<Window>().Where(x => x.ToString().Contains("UsermanagementWindow")).FirstOrDefault();

            if (mainWindow != null)
            {
                scrollViewerTmp = AppVisualTree.FindChild<System.Windows.Controls.ScrollViewer>(
                    mainWindow,
                    "scroller");
            }

            return scrollViewerTmp;
        }

        private void OnSelectionChanged(object args)
        {
            var textBox = ((RoutedEventArgs)args).Source as TextBox;
            if (textBox != null)
            {
                if (textBox.SelectionStart != textBox.Text.Length)
                    textBox.Select(textBox.Text.Length, 0);
            }
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
        [AsyncMethod]
        private void CheckRegistration()
        {
            IsEnabledRegister = false;
            PleaseWaitCheckRegistration();
        }


        [WsdlServiceSyncAspect]
        private void PleaseWaitCheckRegistration()
        {
            string registration_note_number;
            string user = "username";

            if (!this.ValidateViewModel())
                return;
            try
            {
                valueForm form = Values();
                EditUser = new FoundUser();

                bool bVerified = (ChangeTracker.CurrentUser is OperatorUser) ? true : false;
                long? iOperatorId = (ChangeTracker.CurrentUser is OperatorUser) ? (int)ChangeTracker.CurrentUser.AccountId : 0;
                iOperatorId = iOperatorId != 0 ? iOperatorId : null;

                EditUser.AccountId = WsdlRepository.RegisterAccount(iOperatorId, form, StationRepository.StationNumber, bVerified, out registration_note_number);
                if (EditUser.AccountId == 0)
                {
                    ShowError(TranslationProvider.Translate(MultistringTags.SERVER_ERROR).ToString());
                    return;
                }
                else
                {

                    if (bVerified)
                    {
                        ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_DONE).ToString());
                    }
                    else
                    {
                        ShowError(TranslationProvider.Translate(MultistringTags.REGISTRATION_DONE_VERIFY_BEFORE_LOGIN).ToString());
                        foreach (var formField in form.fields)
                        {
                            if (formField.name == "username")
                            {
                                user = formField.value;
                                break;
                            }
                        }

                        bool isPrinted = PrinterHandler.PrintRegistrationNote(user, registration_note_number);
                        if (!isPrinted)
                        {
                            ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), null, true);
                        }
                    }
                }
            }
            catch (FaultException<HubServiceException> ex)
            {
                switch (ex.Detail.code)
                {
                    case 1000:
                        ShowError(TranslationProvider.Translate(MultistringTags.USER_ALREADY_REGISTERED));
                        Mediator.SendMessage("", MsgTag.ShowKeyboard);
                        break;
                    case 101:
                        if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                        {
                            foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "username"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.USERNAME_ALREADY_REGISTERED).ToString();

                                //var i =
                                //    ActiveFields.Where(field => field.Name == "username")
                                //                .Select((field, index) => index);
                                //RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                //SetVisibleFields(RegistrationStep);
                                Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                break;
                            }
                        }
                        else
                        {
                            foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "username"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.USERNAME_ALREADY_REGISTERED).ToString();
                                Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                break;
                            }
                        }

                        break;
                    case 102:
                        if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                        {
                            foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "email"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.MAIL_ALREADY_REGISTERED).ToString();
                                //var i =
                                //    ActiveFields.Where(field => field.Name == "email")
                                //                .Select((field, index) => index);
                                //RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                //SetVisibleFields(RegistrationStep);
                                Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                break;
                            }
                        }
                        else
                        {
                            foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "email"))
                            {
                                registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.MAIL_ALREADY_REGISTERED).ToString();
                                Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                break;
                            }
                        }

                        break;
                    case 103:
                        if (ex.Message.Contains("date_of_birth"))
                        {
                            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                            {
                                foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "date_of_birth"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                    //var i =
                                    //ActiveFields.Where(field => field.Name == "date_of_birth")
                                    //            .Select((field, index) => index);
                                    //RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                    //SetVisibleFields(RegistrationStep);
                                    break;
                                }
                            }
                            else
                            {
                                foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "date_of_birth"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                    break;
                                }
                            }
                        }
                        if (ex.Message.Contains("firstname"))
                        {
                            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                            {
                                foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "firstname"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                    //var i =
                                    //ActiveFields.Where(field => field.Name == "firstname")
                                    //            .Select((field, index) => index);
                                    //RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                    //SetVisibleFields(RegistrationStep);
                                    Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                    break;
                                }
                            }
                            else
                            {
                                foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "firstname"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                    Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                    break;
                                }
                            }
                        }
                        if (ex.Message.Contains("lastname"))
                        {
                            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                            {
                                foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "lastname"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                    //var i =
                                    //ActiveFields.Where(field => field.Name == "lastname")
                                    //            .Select((field, index) => index);
                                    //RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                    //SetVisibleFields(RegistrationStep);
                                    Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                    break;
                                }
                            }
                            else
                            {
                                foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "lastname"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                    Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                    break;
                                }
                            }
                        }
                        if (ex.Message.Contains("password"))
                        {
                            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                            {
                                foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "password"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                    //var i =
                                    //ActiveFields.Where(field => field.Name == "password")
                                    //            .Select((field, index) => index);
                                    //RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                    //SetVisibleFields(RegistrationStep);
                                    Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                    break;
                                }
                            }
                            else
                            {
                                foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "password"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_INVALIDVALUE).ToString();
                                    Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                    break;
                                }
                            }

                        }
                        if (ex.Message.Contains("email"))
                        {
                            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                            {
                                foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "email"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_EMAIL_WRONGFORMAT).ToString();
                                    //var i =
                                    //ActiveFields.Where(field => field.Name == "email")
                                    //            .Select((field, index) => index);
                                    //RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                    //SetVisibleFields(RegistrationStep);
                                    Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                    break;
                                }
                            }
                            else
                            {
                                foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "email"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_EMAIL_WRONGFORMAT).ToString();
                                    Mediator.SendMessage("", MsgTag.ShowKeyboard);
                                    break;
                                }
                            }

                        }
                        if (ex.Message.Contains("address"))
                        {
                            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                            {
                                foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "address"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_ADRESS).ToString();
                                    //var i =
                                    //ActiveFields.Where(field => field.Name == "address")
                                    //            .Select((field, index) => index);
                                    //RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                    //SetVisibleFields(RegistrationStep);
                                    break;
                                }
                            }
                            else
                            {
                                foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "address"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_ADRESS).ToString();
                                    break;
                                }
                            }

                        }
                        if (ex.Message.Contains("document_number"))
                        {
                            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                            {
                                foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "document_number"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_DOCUMENT_NUMBER).ToString();
                                    var i =
                                    ActiveFields.Where(field => field.Name == "document_number")
                                                .Select((field, index) => index);
                                    RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                    SetVisibleFields(RegistrationStep);
                                    break;
                                }
                            }
                            else
                            {
                                foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "document_number"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_DOCUMENT_NUMBER).ToString();
                                    break;
                                }
                            }

                        }
                        if (ex.Message.Contains("phone"))
                        {
                            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
                            {
                                foreach (global::ViewModels.Registration registrationField in ActiveFields.Where(registrationField => registrationField.Name == "phone"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_PHONE_NUMBER).ToString();
                                    var i =
                                    ActiveFields.Where(field => field.Name == "phone")
                                                .Select((field, index) => index);
                                    RegistrationStep = (i.FirstOrDefault() % 5 != 0) ? (i.FirstOrDefault() / 5) + 1 : (i.FirstOrDefault() / 5);
                                    SetVisibleFields(RegistrationStep);
                                    break;
                                }
                            }
                            else
                            {
                                foreach (global::ViewModels.Registration registrationField in RegistrationFields.Where(registrationField => registrationField.Name == "phone"))
                                {
                                    registrationField.ErrorText = TranslationProvider.Translate(MultistringTags.TERMINAL_INVALID_PHONE_NUMBER).ToString();
                                    break;
                                }
                            }

                        }
                        break;
                    default:
                        ShowError(ex.Detail.message);
                        break;
                }
                if (ActiveFields != null)
                {
                    var step = 1;
                    for (int i = 0; i < ActiveFields.Count; i++)
                    {
                        var registration = ActiveFields[i];
                        if (!string.IsNullOrEmpty(registration.Error))
                        {
                            step = i / 5 + (i % 5 > 0 ? 1 : 0);
                            RegistrationStep = step;
                            SetVisibleFields(RegistrationStep);
                            registration.IsFocused = true;
                            break;
                        }
                    }
                }

                return;
            }
            if (IsOperatorUser)
            {
                IsEnabledRegister = false;
                IsEnabledBindCard = true;
            }
            else
                CloseWindow();

        }

        private bool ValidateViewModel()
        {
            foreach (var registrationField in RegistrationFields)
            {
                registrationField.ValidateFields();
                if (!string.IsNullOrEmpty(registrationField.Error))
                    return false;
            }
            return true;
        }

        public bool IsOperatorUser
        {
            get { return ChangeTracker.CurrentUser is OperatorUser; }
        }

        public ObservableCollection<StepButton> RegistrationSteps
        {
            get { return _registrationSteps; }
            set { _registrationSteps = value; }
        }


        private void CloseWindow()
        {
            Mediator.SendMessage("", MsgTag.HideKeyboard);
            if (ChangeTracker.CurrentUser is OperatorUser)
            {
                MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
            }
            Mediator.SendMessage<long>(0, MsgTag.HideLogin);

        }

        private void GetRegistrationForm(int obj)
        {
            RegistrationForm();
            if (RegistrationFields != null)
            {
                var visibleFidelds = RegistrationFields.Where(x => x.Visible).ToList();
                var firstOrDefault = visibleFidelds.FirstOrDefault();
                if (firstOrDefault != null) firstOrDefault.IsFocused = true;

                RegistrationSteps.Add(new StepButton() { IsFirst = true, IsSelected = true, StepNumber = 1 });
                var amount = visibleFidelds.Count / 5 - 1;
                for (int i = 1; i <= amount; i++)
                {
                    RegistrationSteps.Add(new StepButton() { IsMiddle = true, StepNumber = i + 1 });
                }
                if (visibleFidelds.Count % 5 == 0)
                {
                    RegistrationSteps.RemoveAt(RegistrationSteps.Count - 1);
                }
                RegistrationSteps.Add(new StepButton() { IsLast = true, StepNumber = RegistrationSteps.Count + 1 });


            }
            else
                HideWindow();

        }

        [WsdlServiceSyncAspectSilent]
        private void RegistrationForm()
        {
            formField[] form = StationRepository.GetRegistrationForm();

            RegistrationFields = InitFields(new List<formField>(form));
            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
            {
                ActiveFields = RegistrationFields.Where(i => i.Visible).ToList();
                SetVisibleFields(registrationStep);
            }
        }

        private Registration TermsConditionsField;
        private ObservableCollection<StepButton> _registrationSteps = new ObservableCollection<StepButton>();

        private ObservableCollection<Registration> InitFields(IList<formField> fields)
        {
            var _fields = new ObservableCollection<Registration>();

            fields.Add(new formField() { name = "termsConditions", type = "TERMSCONDITIONS" });
            foreach (formField accountingField in fields)
            {
                var field = new Registration();
                field.ReadOnly = false;
                field.IsEnabled = true;
                field.isValidatedEvent += field_isValidatedEvent;
                field.Visible = !accountingField.hidden;
                field.Name = accountingField.name;
                field.Value = accountingField.value;
                field.Mandatory = accountingField.mandatory;
                field.default_value = accountingField.default_value;
                field.Label = TranslationProvider.Translate(MultistringTag.Assign("TERMINAL_FORM_" + field.Name.ToUpperInvariant(), "")).Replace("{0}", "");                                

                field.Rules = new List<fieldValidationRule>();
                if (accountingField.type == null)
                    continue;
                if (AccountingToFormFieldsMap.ContainsKey(accountingField.type))
                    field.Type = AccountingToFormFieldsMap[accountingField.type];
                else
                    field.Type = FieldType.Text;
                if (accountingField.validation_rules != null)
                    foreach (var rule in accountingField.validation_rules)
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

                    if (!field.Options.Any(sv => sv.Name == field.Value))
                    {
                        var svTmp = field.Options.FirstOrDefault();
                        if (svTmp != null)
                        {
                            field.Value = svTmp.Value;
                        }
                    }
                }
                if (field.Type == FieldType.TermsConditions)
                {
                    TermsConditionsField = field;
                }
                if (accountingField.type == "DROPDOWN")
                {
                    string[] ids = accountingField.default_value.Split(';');
                    foreach (var id in ids)
                    {
                        string mltStr = TranslationProvider.Translate(MultistringTag.Assign(id.ToUpperInvariant(), ""));
                        field.Options.Add(new SelectorValue(mltStr, id));
                    }
                    field.Value = "";
                    if (string.IsNullOrEmpty(accountingField.default_value))
                        field.Value = "";
                }
                else
                {
                    field.Value = accountingField.default_value;
                }

                if (accountingField.type == "CURRENCY")
                {
                    field.IsEnabled = false;
                    field.EmptyValidation = field.IsEnabled;
                    field.Value = StationRepository.Currency;
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
                    field.Value = accountingField.default_value;
                    if (string.IsNullOrEmpty(accountingField.default_value))
                        field.Value = "";

                }

                if (field.Type == FieldType.Password)
                {
                    _fields.Add(field);
                    global::ViewModels.Registration secondPasswordField = new global::ViewModels.Registration();
                    field.PasswordConfirmation = secondPasswordField;
                    secondPasswordField.PasswordConfirmation = field;
                    secondPasswordField.isValidatedEvent += new isValidatedDelegate(field_isValidatedEvent);

                    secondPasswordField.Rules = new List<fieldValidationRule>();
                    foreach (fieldValidationRule rule in accountingField.validation_rules)
                    {
                        secondPasswordField.Rules.Add(rule);
                    }

                    secondPasswordField.Rules.Add(new fieldValidationRule() { });

                    secondPasswordField.Name = "password2";
                    secondPasswordField.Label = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CONFIRM_PASSWORD) + "*";
                    secondPasswordField.Type = FieldType.Password2;
                    secondPasswordField.IsEnabled = true;
                    secondPasswordField.Mandatory = true;
                    secondPasswordField.Value = null;
                    _fields.Add(secondPasswordField);
                }
                else if (field.Type == FieldType.Date)
                {
                    if (string.IsNullOrWhiteSpace(field.Value))
                    {
                        field.Value = DateTime.Today.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    }
                    DateTime selDate = new DateTime();
                    DateTime.TryParse(field.Value, new DateTimeFormatInfo() { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out selDate);
                    field.Value = selDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    _fields.Add(field);
                }
                else
                {
                    _fields.Add(field);
                }
                if (field.Name == "email")
                {
                    int a = 0;
                    a++;
                }
                if (field.Mandatory)
                    field.Label += "*";
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

        void field_isValidatedEvent(Registration item, bool valid)
        {
            try
            {
                var fields = new List<Registration>();

                if ((ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode) && RegistrationStep == RegistrationSteps.Count)
                    fields = RegistrationFields.Union(VisibleFields).ToList();
                else
                {
                    if (RegistrationFields != null)
                        fields = RegistrationFields.ToList();
                }
                if (!valid && !registrationCheck.Contains(item.Name))
                    registrationCheck.Add(item.Name);
                else if (valid)
                {
                    registrationCheck.Remove(item.Name);
                }
                if (TermsConditionsField != null && !TermsConditionsField.ValueBool && !registrationCheck.Contains("terms"))
                    registrationCheck.Add("terms");
                if (TermsConditionsField != null && TermsConditionsField.ValueBool)
                    registrationCheck.Remove("terms");

                //else if (!val && registrationCheck.Contains(regName))
                //    registrationCheck.Remove(regName);
                if (fields != null)
                {
                    if (item.Name == "password")
                    {
                        foreach (var registrationField in fields)
                        {
                            if (registrationField.Type == FieldType.Password2)
                            {
                                registrationField.ValidateFields();
                            }
                        }
                    }
                }

                IsEnabledRegister = registrationCheck.Count == 0;



            }
            catch (Exception e)
            {
                Log.Error("", e);
            }
        }

        public valueForm Values()
        {
            valueForm valueForm = new valueForm();
            var fields = new List<valueField>();
            foreach (global::ViewModels.Registration entry in RegistrationFields)
            {
                if (entry.Name.ToLower() == "currency")
                {
                    valueField field = new valueField();
                    field.name = entry.Name;
                    field.value = "EUR";
                    fields.Add(field);
                }
                else if (entry.Type == FieldType.Date)
                {
                    valueField field = new valueField();
                    field.name = entry.Name;
                    DateTime selDate = new DateTime();
                    DateTime.TryParse(entry.Value, new DateTimeFormatInfo() { DateSeparator = ".", ShortDatePattern = "ddMMyyyy" }, DateTimeStyles.AllowWhiteSpaces, out selDate);
                    field.value = selDate.ToString("dd.MM.yyyy");
                    if (string.IsNullOrEmpty(field.value))
                        field.value = "";
                    fields.Add(field);
                }
                else if (entry.Name != "password2")
                {
                    valueField field = new valueField();
                    field.name = entry.Name;
                    if (entry.Name == "card_pin_enabled")
                    {
                        field.value = "1"; // set PIN enabled for card by default
                    }
                    else
                    {
                        field.value = entry.Value;
                    }
                    if (string.IsNullOrEmpty(field.value))
                        field.value = "";
                    fields.Add(field);
                }
            }
            valueForm.fields = fields.ToArray();

            return valueForm;
        }

        /// <summary>
        /// Method to invoke when the ScrollDownStart command is executed.
        /// </summary>
        private void OnScrollDownStartExecute()
        {
            this._ScrollViewerModule.OnScrollDownStartExecute(this.GetScrollviewer());
        }
        /// <summary>
        /// Method to invoke when the ScrollDownStop command is executed.
        /// </summary>
        private void OnScrollDownStopExecute()
        {
            this._ScrollViewerModule.OnScrollDownStopExecute();
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStart command is executed.
        /// </summary>
        private void OnScrollUpStartExecute()
        {
            this._ScrollViewerModule.OnScrollUpStartExecute(this.GetScrollviewer());
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStop command is executed.
        /// </summary>
        private void OnScrollUpStopExecute()
        {
            this._ScrollViewerModule.OnScrollUpStopExecute();
        }


        #endregion
    }

    public class StepButton : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _name = "step";
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public bool IsMiddle { get; set; }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public int StepNumber { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}