using System;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Keyboard.ViewModels;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class UserProfileMenuViewModel : BaseViewModel
    {


        #region Constructors
        public UserProfileMenuViewModel()
        {

            ShowAccountHistoryCommand = new Command(OnShowHistory);
            ShowUserProfileCommand = new Command(OnShowProfile);
            ShowChangePasswordCommand = new Command(OnChangePassword);
            ShowDepositMoneyCommand = new Command(OnShowDepositMoney);
            ShowCashMoneyCommand = new Command(OnShowCashMoney);
            ShowTicketsCommand = new Command(OnTickets);
            ShowBalanceCommand = new Command(OnBalance);
            LogoutCommand = new Command(OnLogout);

            ChangeTracker.HeaderVisible = false;
            ChangeTracker.FooterArrowsVisible = false;

            var user = ChangeTracker.CurrentUser as LoggedInUser;
            if (user != null)
                Hello_string = TranslationProvider.Translate(MultistringTags.HELLO_MSG, user.Username);


        }

        #endregion

        #region Properties


        public string Hello_string { get; set; }


        #endregion

        #region Commands

        public Command ShowAccountHistoryCommand { get; set; }
        public Command ShowUserProfileCommand { get; set; }
        public Command ShowChangePasswordCommand { get; set; }
        public Command ShowDepositMoneyCommand { get; set; }
        public Command ShowCashMoneyCommand { get; set; }
        public Command ShowTicketsCommand { get; set; }
        public Command ShowBalanceCommand { get; set; }
        public Command LogoutCommand { get; set; }

        #endregion

        #region Method

        public override void OnNavigationCompleted()
        {
            if (StationRepository.IsTestMode)
            {
                OnTickets();
            }
            else
            {
                MyRegionManager.ClearHistory(RegionNames.UserProfileContentRegion);
                MyRegionManager.NavigateUsingViewModel<KeyboardViewModel>(RegionNames.ProfileKeyboardRegion);
                MyRegionManager.NavigateUsingViewModel<WithdrawMoneyViewModel>(RegionNames.UserProfileContentRegion);
            }
          
            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.ProfileKeyboardRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UserProfileContentRegion);
            base.Close();
        }

        private void OnBalance()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            PleaseWaitOnBalance();
        }


        private void PleaseWaitOnBalance()
        {
            MyRegionManager.ClearHistory(RegionNames.UserProfileContentRegion);
            MyRegionManager.NavigateUsingViewModel<UserHistoryViewModel>(RegionNames.UserProfileContentRegion);

        }


        [PleaseWaitAspect]
        private void OnLogout()
        {
            if (!string.IsNullOrEmpty(ChangeTracker.CurrentUser.CardNumber) && StationRepository.IsIdCardEnabled)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.REMOVE_CARD) as string);
                return;
            }
            //hack go to different view to make this model update next time load
            Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
            Mediator.SendMessage<long>(0, MsgTag.HideUserProfile);
            ClearAndOpenAnonymousSession();
        }
        private void OnTickets()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            ChangeTracker.SelectedType = null;
            ChangeTracker.SelectedTycketType = 0;
            ChangeTracker.TicketsStartPage = 1;
            MyRegionManager.ClearHistory(RegionNames.UserProfileContentRegion);
            MyRegionManager.NavigateUsingViewModel<UserTicketsViewModel>(RegionNames.UserProfileContentRegion);
        }

        private void OnShowHistory()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UserProfileContentRegion);
            MyRegionManager.NavigateUsingViewModel<UserHistoryViewModel>(RegionNames.UserProfileContentRegion);
        }


        private void OnShowCashMoney()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UserProfileContentRegion);
            MyRegionManager.NavigateUsingViewModel<WithdrawMoneyViewModel>(RegionNames.UserProfileContentRegion);
        }


        private void OnShowDepositMoney()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UserProfileContentRegion);
            MyRegionManager.NavigateUsingViewModel<DepositMoneyViewModel>(RegionNames.UserProfileContentRegion);
        }


        private void OnChangePassword()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UserProfileContentRegion);
            MyRegionManager.NavigateUsingViewModel<ChangePasswordViewModel>(RegionNames.UserProfileContentRegion);
        }

        private void OnShowProfile()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            PleaseWaitOnShowProfile();
        }

        private void PleaseWaitOnShowProfile()
        {
            MyRegionManager.ClearHistory(RegionNames.UserProfileContentRegion);
            MyRegionManager.NavigateUsingViewModel<ChangeUserProfileViewModel>(RegionNames.UserProfileContentRegion);
        }

        public string PasswordButtonCaption
        {
            get
            {
                if (ChangeTracker.CurrentUser.CardNumber == null || ChangeTracker.CurrentUser.CardNumber == "")
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_CHANGE_PASSWORD).ToString();

                if (StationRepository.UserCardPinSetting == 1 || StationRepository.UserCardPinSetting == 3)
                {
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_CHANGE_PASSWORD_AND_PIN).ToString();
                }
                else
                {
                    return TranslationProvider.Translate(MultistringTags.TERMINAL_CHANGE_PASSWORD).ToString();
                }
            }
        }

        #endregion
    }
}