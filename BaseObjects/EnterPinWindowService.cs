using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Aspects.WaitOverlayProvider;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Logs;

namespace BaseObjects
{
    [ServiceAspect]
    public class EnterPinWindowService : IEnterPinWindowService
    {

        private static ILog Log = LogFactory.CreateLog(typeof(EnterPinWindowService));
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

        private IMessageStorage _mediator;
        public IMessageStorage Mediator
        {
            get
            {
                return _mediator ?? (_mediator = IoCContainer.Kernel.Get<IMessageStorage>());
            }
        }

        public EnterPinViewModel Model = null;

        public static void MaximizeWindow(Window window)
        {
            window.Left = 0;
            window.Top = 0;
            var screens = Screen.AllScreens.Where(s => s.Primary).FirstOrDefault();
            window.Width = screens.WorkingArea.Right;
            window.Height = screens.WorkingArea.Bottom;

        }





        public void AskPin(EventHandler<EventArgs<string>> enterpinviewModelOkClick1, EventHandler<EventArgs<string>> viewModelOkClick)
        {

            IoCContainer.Kernel.Get<IWaitOverlayProvider>().DisposeAll();
            IoCContainer.Kernel.Get<IQuestionWindowService>().Close();
            IoCContainer.Kernel.Get<IEnterPinWindowService>().Close();
            IoCContainer.Kernel.Get<IErrorWindowService>().Close();
            Log.Error("ask Pin", new Exception());
            try
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {

                    var window = MyRegionManager.FindWindowByViewModel<EnterPinViewModel>(false);
                    Model = (EnterPinViewModel)window.DataContext;
                    MaximizeWindow(window);

                    if (viewModelOkClick != null)
                        Model.OkClick += enterpinviewModelOkClick1;
                    if (viewModelOkClick != null)
                        Model.CloseClick += viewModelOkClick;
                    window.Closed -= window_Closed;


                    window.ShowDialog();

                    if (viewModelOkClick != null)
                        Model.OkClick -= enterpinviewModelOkClick1;
                    if (viewModelOkClick != null)
                        Model.CloseClick -= viewModelOkClick;

                }));
            }
            catch (Exception ex)
            {

                Log.Error(ex.Message, ex);
            }



        }

        public void Close()
        {
            Mediator.SendMessage(true, MsgTag.ClosePinWindow);
        }

        void window_Closed(object sender, EventArgs e)
        {
            Model = null;
        }




    }
}
