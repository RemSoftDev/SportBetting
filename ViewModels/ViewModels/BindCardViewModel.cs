using BaseObjects;
using BaseObjects.ViewModels;
using System;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Question Dialog view model.
    /// </summary>
    [ServiceAspect]
    public class BindCardViewModel : BaseViewModel
    {

        private static ILog Log = LogFactory.CreateLog(typeof(BindCardViewModel));

        #region Constructors

        public BindCardViewModel()
        {
            AcceptCommand = new Command(AcceptOnClick);
            CancelCommand = new Command(CancelOnClick);
            LogoutCommand = new Command(OnLogout);
            Status = CardStatus.NotReady;
            Text = TranslationProvider.Translate(MultistringTags.PLEASE_INSERT_CARD).ToString();
            IsBindingCard = true;
            Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.CloseCurrentWindow);
            Mediator.Register<string>(this, CardInserted, MsgTag.CardInserted);
            Mediator.Register<long>(this, CardRemoved, MsgTag.CardRemoved);

        }


        #endregion

        public event EventHandler AcceptClick;
        public event EventHandler CancelClick;

        #region Properties

        public string CardNumber
        {
            get { return ChangeTracker.CardNumber; }
            set { ChangeTracker.CardNumber = value; }
        }

        public enum CardStatus { Ready = 0, NotReady = 1, AlreadyInUse = 2 };
        public CardStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public bool IsVisibleLogoutButton
        {
            get { return _isVisibleLogoutButton; }
            set
            {
                _isVisibleLogoutButton = value;
                OnPropertyChanged("IsVisibleLogoutButton");
            }
        }

        public bool IsCardReady
        {
            get { return _isCardReady; }
            set
            {
                _isCardReady = value;
                OnPropertyChanged("IsCardReady");
            }
        }

        private string _text;
        private bool _acceptButton = true;
        private bool _cancelButton = true;
        private CardStatus _status;
        private bool _isCardReady;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public bool CancelButton
        {
            get { return _cancelButton; }
            set
            {
                _cancelButton = value;
                OnPropertyChanged("CancelButton");
            }
        }

        public bool AcceptButton
        {
            get { return _acceptButton; }
            set
            {
                _acceptButton = value;
                OnPropertyChanged("AcceptButton");
            }
        }

        #endregion

        #region Commands

        public Command AcceptCommand { get; set; }
        public Command CancelCommand { get; set; }
        public Command LogoutCommand { get; set; }
        #endregion

        public void CloseCurrentWindow(bool state)
        {
            Close();
        }

        private void OnLogout()
        {
            StationRepository.DisableCashIn();
            CardNumber = "";
            CardRemovedMessage();
        }

        private void CardRemovedMessage()
        {
            Mediator.SendMessage<long>(0, "CardRemoved");
            Close();
        }



        public override void Close()
        {
            IsBindingCard = false;
            base.Close();
        }

        public string CurrentCardNumber
        {
            get { return ChangeTracker.CardNumber; }
            set { ChangeTracker.CardNumber = value; }
        }

        private bool _isVisibleLogoutButton = false;

        private void CardInserted(string number)
        {
            CurrentCardNumber = number;

            UpdateCardStatus(number);

        }

        private void UpdateCardStatus(string number)
        {
            if (IsVisibleLogoutButton && number == ChangeTracker.CurrentUser.CardNumber)
            {
                Close();
                return;
            }

            Status = CardStatus.Ready;
            Text = TranslationProvider.Translate(MultistringTags.READY).ToString();
            if (!number.Contains("?"))
            {
                Status = CardStatus.AlreadyInUse;
                Text = TranslationProvider.Translate(MultistringTags.CARD_ALREADY_REGISTERED).ToString();
            }
            IsCardReady = true;
            if (number.Equals(ChangeTracker.CurrentUser.CardNumber))
            {
                Text = TranslationProvider.Translate(MultistringTags.PLEASE_INSERT_CARD).ToString();
                Status = CardStatus.NotReady;
                IsCardReady = false;
            }
        }

        private void CardRemoved(long obj)
        {
            CurrentCardNumber = "";
            Text = TranslationProvider.Translate(MultistringTags.PLEASE_INSERT_CARD).ToString();
            Status = CardStatus.NotReady;
            IsCardReady = false;

        }

        private void CancelOnClick()
        {
            if (CancelClick != null)
                CancelClick(null, null);
            ChangeTracker.BindingCardCancelled = true;
            if (ChangeTracker.CurrentUser.IsLoggedInWithIDCard)
                CloseWindow();
            else
                Close();

        }

        [AsyncMethod]
        private void AcceptOnClick()
        {
            PleaseWaitAcceptOnClick();
        }

        private void PleaseWaitAcceptOnClick()
        {
            Blur();
            try
            {
                if (AcceptClick != null)
                    AcceptClick(null, null);
                CloseWindow();

            }
            catch (Exception e)
            {
                Log.Error("", e);
            }
            UnBlur();
        }

        private void CloseWindow()
        {
            if (String.IsNullOrEmpty(ChangeTracker.CurrentUser.CardNumber) ||
                ChangeTracker.CurrentUser.CardNumber == CurrentCardNumber ||
                !ChangeTracker.CurrentUser.IsLoggedInWithIDCard)
            {
                Close();
            }
            else if (CurrentCardNumber != ChangeTracker.CurrentUser.CardNumber)
            {
                AcceptButton = false;
                CancelButton = false;
                IsVisibleLogoutButton = true;
                Text = TranslationProvider.Translate(MultistringTags.REMOVE_CARD_AND_INSERT_YOUR_BINDED_CARD).ToString();
                Status = CardStatus.NotReady;
            }
        }

    }
}