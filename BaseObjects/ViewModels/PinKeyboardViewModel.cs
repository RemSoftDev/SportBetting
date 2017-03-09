using SportBetting.WPF.Prism.Modules.Aspects;

namespace BaseObjects.ViewModels
{


    /// <summary>
    /// Categories view model.
    /// </summary>
    [ServiceAspect]
    public class PinKeyboardViewModel : BaseViewModel
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PinKeyboardViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public PinKeyboardViewModel()
        {
            ButtonBackCommand = new Command(OnBack);
            ButtonCommand = new Command<string>(OnNumberButton);
            ButtonClearCommand = new Command(OnClear);
        }


        #endregion

        #region Properties
        #endregion

        #region Commands

        public Command<string> ButtonCommand { get; set; }
        public Command ButtonClearCommand { get; set; }
        public Command ButtonBackCommand { get; set; }
        #endregion

        #region Methods
        private void OnClear()
        {
            Mediator.SendMessage<string>("", "EnterPinClearTicketNumber");
            Mediator.SendMessage<string>("", "EnterPinClear");
            Mediator.SendMessage<string>("", "WithdrawClearTicketNumber");
            Mediator.SendMessage<string>("", "ClearTicketNumber");
        }

        private void OnBack()
        {
            Mediator.SendMessage<string>("", "EnterPinBackspace");
            Mediator.SendMessage<string>("", "EnterBackspace");
            Mediator.SendMessage<string>("", "WithdrawPinBackspace");
            Mediator.SendMessage<string>("", "PinBackspace");
        }
        private void OnNumberButton(string number)
        {
            Mediator.SendMessage<string>(number, "EnterPinButton");
            Mediator.SendMessage<string>(number, "EnterButton");
            Mediator.SendMessage<string>(number, "WithdrawPinButton");
            Mediator.SendMessage<string>(number, "PinButton");


        }
        public override void Close()
        {
            base.Close();
        }

        #endregion
    }
}