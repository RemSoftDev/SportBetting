using System;
using System.Windows.Controls;
using System.Collections.Generic;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class PinInsertingViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinInsertingViewModel"/> class.
        /// </summary>
        public PinInsertingViewModel ()
        {
            Mediator.Register<string>(this, OnClearExecute, "ClearTicketNumber");
            Mediator.Register<string>(this, OnBackSpaceExecute, "PinBackspace");
            Mediator.Register<string>(this, OnPinButtonExecute, "PinButton");
            SelectionChanged = new Command<object>(OnSelectionChanged);
            this.SavePinCommand = new Command(this.OnSavePinCommand);
            NewPin.Validate += NewPin_Validate;
            NewPinConfirmed.Validate += NewPin_Validate;
        }

        List<string> NewPin_Validate(object sender, string property)
        {
            return ValidateFields();
        }



        #region Properties   
        

        private int _UserType = 0;
        private bool _isEnabledSave;
        public bool IsEnabledSave
        {
            get { return _isEnabledSave; }
            set
            {
                _isEnabledSave = value;
                OnPropertyChanged("IsEnabledSave");
            }
        }

        private bool _isNewPinFocused;
        public bool IsNewPinFocused
        {
            get { return _isNewPinFocused; }
            set
            {
                _isNewPinFocused = value;
               
            }
        }

        private bool _isNewPinConfirmedFocused;
        private MyModelBase _newPin = new MyModelBase();
        private MyModelBase _newPinConfirmed = new MyModelBase();

        public bool IsNewPinConfirmedFocused
        {
            get { return _isNewPinConfirmedFocused; }
            set
            {
                _isNewPinConfirmedFocused = value;
            }
        }

        public MyModelBase NewPin
        {
            get { return _newPin; }
            set { _newPin = value; }
        }

        public MyModelBase NewPinConfirmed
        {
            get { return _newPinConfirmed; }
            set { _newPinConfirmed = value; }
        }

        #endregion

        #region Commands  
        public Command<object> SelectionChanged { get; private set; }
        public Command SavePinCommand { get; private set; }
        #endregion

        #region Methods  
        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<PinKeyboardViewModel>(RegionNames.PinKeyboardRegion);
            base.OnNavigationCompleted();
        }
        public override void Close()
        {
            base.Close();
        }
        private void OnPinButtonExecute(string obj)
        {
            int number;

           
            if (int.TryParse(obj, out number))
            {
                if (IsNewPinFocused)
                {
                    if (String.IsNullOrEmpty(NewPin.ValueMasked) || NewPin.ValueMasked.Length < 4)
                    {
                        NewPin.ValueMasked += number.ToString();
                    }
                }
                else if (IsNewPinConfirmedFocused)
                {
                    if (String.IsNullOrEmpty(NewPinConfirmed.ValueMasked) || NewPinConfirmed.ValueMasked.Length < 4)
                    {
                        NewPinConfirmed.ValueMasked += number.ToString();
                    }
                }
               
            }
        }
        private void OnBackSpaceExecute(string obj)
        {
            if (IsNewPinFocused)
            {
                var newPinMaskedLength = (this.NewPin.Value ?? string.Empty).Length - 1;
                if (newPinMaskedLength < 0)
                {
                    newPinMaskedLength = 0;
                }
                this.NewPin.ValueMasked = (this.NewPin.ValueMasked ?? string.Empty).Substring(0, newPinMaskedLength);
            }
            else if (IsNewPinConfirmedFocused)
            {
       
                var newPinConfirmedMasked = (this.NewPinConfirmed.Value ?? string.Empty).Length - 1;
                if (newPinConfirmedMasked < 0)
                {
                    newPinConfirmedMasked = 0;
                }
                this.NewPinConfirmed.ValueMasked = (this.NewPinConfirmed.ValueMasked ?? string.Empty).Substring(0, newPinConfirmedMasked);
            }

        }

        private void OnClearExecute(string obj)
        {
            if (IsNewPinFocused)
            {
                NewPin.ValueMasked = string.Empty;
            }
            else if (IsNewPinConfirmedFocused)
            {
                NewPinConfirmed.ValueMasked = string.Empty;
            }
        }



        protected List<string> ValidateFields()
        {
            List<string> validationResults = new List<string>();
            bool result = true;
            NewPin.Error = null;
            if (this.NewPin.Value.Length != 4)
            {
                result = false;
                this.NewPin.Error = TranslationProvider.Translate(MultistringTags.TERMINAL_PIN_4_SYMBOLS) as string;
                validationResults.Add(TranslationProvider.Translate(MultistringTags.TERMINAL_PIN_4_SYMBOLS));
            }
            if (NewPin.Value != NewPinConfirmed.Value && !String.IsNullOrEmpty(NewPinConfirmed.Value))
            {
                NewPin.Error = TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT);
                NewPinConfirmed.Error = TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT);
                validationResults.Add(TranslationProvider.Translate(MultistringTags.PASSWORDS_ARE_DIFFERENT));                                                                                                                                             
            }
            IsEnabledSave = validationResults.Count == 0 && NewPin.Value == NewPinConfirmed.Value;
            return validationResults;
        }

        private void OnSelectionChanged(object args)
        {
            var textBox = args as TextBox;
            if (textBox != null)
            {
                if (textBox.SelectionStart != textBox.Text.Length)
                {
                    textBox.Select(textBox.Text.Length, 0);
                }
            }
        }

        [WsdlServiceSyncAspect]
        private void OnSavePinCommand()
        {

            string pin = NewPin.Value.ToString();

            try
            {
                if (ChangeTracker.CurrentUser is OperatorUser)
                {
                    WsdlRepository.ChangeOperatorIDCardPin((int)ChangeTracker.CurrentUser.AccountId, ref pin);
                }
                else
                {
                    WsdlRepository.ChangeIDCardPin(StationRepository.GetUid(ChangeTracker.CurrentUser), ref pin);
                }
                PrinterHandler.PrintPinNote(pin);
                NewPin.Value = NewPinConfirmed.Value = string.Empty;
                NewPin.ValueMasked = NewPinConfirmed.ValueMasked = string.Empty;

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
        #endregion
    }
}
