using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Keyboard.ViewModels;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// MainWindow view model.
    /// </summary>
    [ServiceAspect]
    public class AuthViewModel : BaseViewModel
    {
        #region Variables
        #endregion

        #region Constructor & destructor
        public AuthViewModel()
            : base()
        {
            HidePleaseWait = false;
        }



        #endregion

        #region Properties


        #endregion

        #region Commands

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<LoginViewModel>(RegionNames.AuthContentRegion);
            MyRegionManager.NavigateUsingViewModel<KeyboardViewModel>(RegionNames.LoginKeyboardRegion);

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.AuthContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.LoginKeyboardRegion);
            base.Close();
        }

        #endregion

    }
}