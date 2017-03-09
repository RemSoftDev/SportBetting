using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportRadar.Common.Logs;

namespace BaseObjects
{
    public class QuestionWindowService : IQuestionWindowService
    {

        private static ILog Log = LogFactory.CreateLog(typeof(ErrorWindowService));


        private IMyRegionManager _myRegionManager;
        public IMyRegionManager MyRegionManager
        {
            get
            {
                return _myRegionManager ?? (_myRegionManager = IoCContainer.Kernel.Get<IMyRegionManager>());
            }
        }
        private IDispatcher _dispatcher;
        public IDispatcher Dispatcher
        {
            get
            {
                return _dispatcher ?? (_dispatcher = IoCContainer.Kernel.Get<IDispatcher>());
            }
        }
        public QuestionViewModel Model { get; set; }


        public static void MaximizeWindow(Window window)
        {
            window.Left = 0;
            window.Top = 0;
            var screens = Screen.AllScreens.Where(s => s.Primary).FirstOrDefault();
            window.Width = screens.WorkingArea.Right;
            window.Height = screens.WorkingArea.Bottom;

        }




        public void ShowMessage(string text)
        {
            ShowMessage(text, null, null, null, null, false);
        }


        public void ShowMessageSync(string text, string yesButtonText, string noButtonText, EventHandler yesClick, EventHandler noClick,
           bool IsVisibleNoButton = true, int yesbuttonTimer = 0, bool clearCashToTransfer = false, bool warning = false)
        {

            Log.Debug(text);


            IoCContainer.Kernel.Get<IWaitOverlayProvider>().DisposeAll();
            IoCContainer.Kernel.Get<IEnterPinWindowService>().Close();
            try
            {

                Dispatcher.Invoke((Action)(() =>
                {
                    var window = MyRegionManager.FindWindowByViewModel<QuestionViewModel>(false);
                    Model = (QuestionViewModel)window.DataContext;
                    MaximizeWindow(window);

                    Model.Text = text;
                    if (yesClick != null)
                        Model.YesClick += yesClick;
                    if (noClick != null)
                        Model.NoClick += noClick;
                    if (yesButtonText != null)
                        Model.YesButtonText = yesButtonText;
                    if (noButtonText != null)
                        Model.NoButtonText = noButtonText;
                    Model.IsVisibleNoButton = IsVisibleNoButton;
                    Model.IYesButtonTime = yesbuttonTimer;
                    Model.ClearCashToTransfer = clearCashToTransfer;
                    Model.WarningSign = warning;
                    window.Closed -= window_Closed;

                    window.ShowDialog();
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }



        }


        public void ShowMessage(string text, string yesButtonText, string noButtonText, EventHandler yesClick, EventHandler noClick,
            bool IsVisibleNoButton = true, int yesbuttonTimer = 0, bool clearCashToTransfer = false, bool warning = false)
        {

            Log.Debug(text);

           
            IoCContainer.Kernel.Get<IWaitOverlayProvider>().DisposeAll();
            IoCContainer.Kernel.Get<IEnterPinWindowService>().Close();
            try
            {

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    var window = MyRegionManager.FindWindowByViewModel<QuestionViewModel>(false);
                    Model = (QuestionViewModel)window.DataContext;
                    MaximizeWindow(window);

                    Model.Text = text;
                    if (yesClick != null)
                        Model.YesClick += yesClick;
                    if (noClick != null)
                        Model.NoClick += noClick;
                    if (yesButtonText != null)
                        Model.YesButtonText = yesButtonText;
                    if (noButtonText != null)
                        Model.NoButtonText = noButtonText;
                    Model.IsVisibleNoButton = IsVisibleNoButton;
                    Model.IYesButtonTime = yesbuttonTimer;
                    Model.ClearCashToTransfer = clearCashToTransfer;
                    Model.WarningSign = warning;
                    window.Closed -= window_Closed;

                    window.ShowDialog();
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }



        }
        public void Close()
        {
            if (Model != null)
                Dispatcher.BeginInvoke(((Action)(() =>
                { Model.Close(); })));
        }


        void window_Closed(object sender, EventArgs e)
        {
            Model = null;
        }


    }
}
