using System;
using Shared;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using System.Collections.Generic;
using System.Linq;
using TranslationByMarkupExtension;

namespace BaseObjects.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class EnterPinViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnterPinViewModel"/> class.
        /// </summary>

        public EnterPinViewModel()
        {

            if (ChangeTracker.MainWindow != null)
                ChangeTracker.MainWindow.Activate();
            this.CloseCommand = new Command(this.OnClose);
            this.ProceedCommand = new Command(this.OnProceed);
            Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.CloseCurrentWindow);
            Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.ClosePinWindow);

            Mediator.Register<long>(this, CardRemoved, MsgTag.CardRemoved);
            Mediator.Register<string>(this, EnterPinClearTicketNumber, MsgTag.EnterPinClearTicketNumber);
            Mediator.Register<string>(this, EnterPinBackspace, MsgTag.EnterPinBackspace);
            Mediator.Register<string>(this, EnterPinButton, MsgTag.EnterPinButton);


            _pin.Validate += _pin_Validate;
        }

        List<string> _pin_Validate(object sender, string property)
        {
            return ValidateFields();
        }

        #region Properties

        public event EventHandler<EventArgs<string>> OkClick;
        public event EventHandler<EventArgs<string>> CloseClick;

        private MyModelBase _pin = new MyModelBase();


        /// <summary>
        /// Gets or sets the Pin.
        /// </summary>
        public MyModelBase Pin
        {
            get { return _pin; }
            set { _pin = value; }
        }

        #endregion

        #region Commands

        public Command CloseCommand { get; private set; }
        public Command ProceedCommand { get; private set; }

        #endregion

        private void OnClose()
        {
            if (CloseClick != null)
                CloseClick(this, new EventArgs<string>(Pin.Value));

            Close();
        }

        private void OnProceed()
        {
            if (!ValidateViewModel())
                return;

            if (OkClick != null)
                OkClick(this, new EventArgs<string>(Pin.Value));
            Close();
            //Mediator.SendMessage<string>(this.Pin, "PinEntered");
            //Mediator.SendMessage<string>(this.Pin, "LoadStoredTicketWindow");

        }

        private bool ValidateViewModel()
        {
            ValidateFields();
            return string.IsNullOrEmpty(Pin.Error);
        }

        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<PinKeyboardViewModel>(RegionNames.EnterPinKeyboardRegion);

            base.OnNavigationCompleted();
        }
        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.EnterPinKeyboardRegion);
            _pin.Validate -= _pin_Validate;

            Mediator.UnregisterRecipientAndIgnoreTags(this);
            IsClosed = true;
            if (Dispatcher != null)
                Dispatcher.Invoke((Action)(() =>
                {
                    if (ViewWindow != null && ViewWindow.IsVisible)
                        ViewWindow.Close();
                }));

        }

        private void CardRemoved(long obj)
        {
            OnClose();
        }


        private void EnterPinClearTicketNumber(string input)
        {
            this.Pin.ValueMasked = string.Empty;
        }

        private void EnterPinBackspace(string input)
        {

            var newPinMaskedLength = (this.Pin.ValueMasked ?? string.Empty).Length - 1;
            if (newPinMaskedLength < 0)
            {
                newPinMaskedLength = 0;
            }
            this.Pin.ValueMasked = (this.Pin.ValueMasked ?? string.Empty).Substring(0, newPinMaskedLength);
        }

        private void EnterPinButton(string input)
        {
            // do not allow to enter more then 4 numbers
            if (this.Pin.ValueMasked.Length >= 4)
                return;
            this.Pin.ValueMasked += this.RemoveInvalidChars(input);
        }



        private string RemoveInvalidChars(string input)
        {
            List<char> chars = new List<char>();

            foreach (var ch in input)
            {
                if (char.IsNumber(ch))
                {
                    chars.Add(ch);
                }
            }

            return new string(chars.ToArray());
        }
        public void CloseCurrentWindow(bool state)
        {
            Close();
        }
        protected List<string> ValidateFields()
        {
            var list = new List<string>();
            if (this.Pin.Value.Length != 4)
            {
                Pin.Error = TranslationProvider.Translate(MultistringTags.TERMINAL_PIN_4_SYMBOLS) as string;
                list.Add(Pin.Error);
            }
            return list;

        }

    }
}
