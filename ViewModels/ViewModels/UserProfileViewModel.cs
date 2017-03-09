using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;

namespace ViewModels.ViewModels
{

    [ServiceAspect]
    public class UserProfileViewModel : BaseViewModel
    {

        #region Constructor & destructor

        public UserProfileViewModel()
        {
            CloseCommand = new Command(CloseCurrentWindow);
            BackCommand = new Command(OnBack);
        }


        #endregion

        #region Properties

        public string CurrentUsername
        {
            get
            {
                var user = ChangeTracker.CurrentUser as LoggedInUser;
                if (user != null)
                    return user.Username;
                return "";
            }
        }
        #endregion

        #region Commands
        public Command CloseCommand { get; set; }
        public Command BackCommand { get; set; }
        #endregion

        #region Methods
        public void CloseCurrentWindow()
        {
            Mediator.SendMessage<long>(0, MsgTag.HideUserProfile);
        }
        private void OnBack()
        {
            MyRegionManager.NavigatBack(RegionNames.ContentRegion);
        }



        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<UserProfileMenuViewModel>(RegionNames.UserProfileMenuRegion);
            Mediator.SendMessage<long>(-1, MsgTag.ShowSystemMessage);
            base.OnNavigationCompleted();
        }
        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UserProfileMenuRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UserProfileContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.ProfileKeyboardRegion);
            ChangeTracker.HeaderVisible = true;
            ChangeTracker.FooterVisible = true;
            ChangeTracker.FooterArrowsVisible = true;
            ChangeTracker.IsUserProfile = false;
            Mediator.SendMessage<long>(-1, MsgTag.ShowSystemMessage);
            base.Close();
        }
        #endregion
    }
}