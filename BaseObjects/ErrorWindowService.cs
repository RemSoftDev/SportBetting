using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportRadar.Common;
using SportRadar.Common.Enums;
using SportRadar.Common.Logs;

namespace BaseObjects
{
    [ServiceAspect]
    public class ErrorWindowService : IErrorWindowService
    {

        private static ILog Log = LogFactory.CreateLog(typeof(ErrorWindowService));

        private IMediator _mediator;
        public IMediator Mediator
        {
            get { return _mediator ?? (_mediator = IoCContainer.Kernel.Get<IMediator>()); }
        }

        private IMyRegionManager _myRegionManager;
        public IMyRegionManager MyRegionManager
        {
            get
            {
                return _myRegionManager ?? (_myRegionManager = IoCContainer.Kernel.Get<IMyRegionManager>());
            }
        }
        public IList<ErrorMessageViewModel> Models = new List<ErrorMessageViewModel>();


        public static void MaximizeWindow(Window window)
        {
            window.Left = 0;
            window.Top = 0;
            var screens = Screen.AllScreens.Where(s => s.Primary).FirstOrDefault();
            window.Width = screens.WorkingArea.Right;
            window.Height = screens.WorkingArea.Bottom;
        }

        public void ShowError(string obj, ErrorSettings errorSettings)
        {
            Log.Error(obj, new Exception(obj));

            IoCContainer.Kernel.Get<IWaitOverlayProvider>().DisposeAll();
            IoCContainer.Kernel.Get<IQuestionWindowService>().Close();
            IoCContainer.Kernel.Get<IEnterPinWindowService>().Close();
            try
            {


                Thread newWindowThread = new Thread(() =>
                {
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

                    var window = MyRegionManager.FindWindowByViewModel<ErrorMessageViewModel>(false);
                    var Model = (ErrorMessageViewModel)window.DataContext;
                    Model.Dispatcher = new MyDispatcher(Dispatcher.CurrentDispatcher);
                    Models.Add(Model);
                    MaximizeWindow(window);
                    Model.Text = obj;
                    Model.ErrorLevel = errorSettings.ErrorLevel;

                    Model.TextAligment = errorSettings.TextAligment;
                    Model.WarningVisibility = errorSettings.WarningVisibility;
                    Model.YesButtonTime = errorSettings.AddCounterSeconds;
                    Model.ShowButtons = errorSettings.HideButtons ? Visibility.Collapsed : Visibility.Visible;
                    Model.OkClick += errorSettings.OkClick;
                    if (errorSettings.CreateButtonEvent)
                    {
                        Mediator.SendMessage<bool>(true, "PrinterErrorValue");
                        window.Closed += window_Closed;
                    }
                    window.Closed -= window_Closed2;
                    window.ShowDialog();
                    Models.Remove(Model);

                });

                newWindowThread.SetApartmentState(ApartmentState.STA);
                newWindowThread.IsBackground = true;

                newWindowThread.Start();
                var dispatcher = Dispatcher.FromThread(newWindowThread);
                do
                {
                    Thread.Sleep(10);
                    dispatcher = Dispatcher.FromThread(newWindowThread);
                }
                while (dispatcher == null);


            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }


        public void ShowError(string obj, EventHandler okClick = null, bool bCreateButtonEvent = false, int iAddCounterSeconds = 0, ErrorLevel errorLevel = ErrorLevel.Normal)
        {
            var errorSettings = new ErrorSettings();
            errorSettings.ErrorLevel = errorLevel;
            errorSettings.OkClick = okClick;
            errorSettings.CreateButtonEvent = bCreateButtonEvent;
            errorSettings.AddCounterSeconds = iAddCounterSeconds;
            if (errorSettings.ErrorLevel == ErrorLevel.Normal)
                errorSettings.WarningVisibility = Visibility.Collapsed;
            if (errorSettings.ErrorLevel == ErrorLevel.ModalWindow)
            {
                errorSettings.ErrorLevel = ErrorLevel.Critical;
                errorSettings.HideButtons = true;
            }

            ShowError(obj, errorSettings);

        }

        public void Close()
        {
            if (Models.Count > 0)

                foreach (var errorMessageViewModel in Models)
                {
                    errorMessageViewModel.Close();
                }

        }


        void window_Closed(object sender, EventArgs e)
        {
            Mediator.SendMessage<bool>(false, "PrinterErrorValue");
            Close();
        }
        void window_Closed2(object sender, EventArgs e)
        {
            Close();
        }

    }
}
