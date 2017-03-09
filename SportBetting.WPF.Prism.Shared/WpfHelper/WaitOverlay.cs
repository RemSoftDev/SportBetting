using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using SportBetting.WPF.Prism.Shared.Controls;
using SportRadar.Common.Logs;
using Timer = System.Timers.Timer;
using IocContainer;
using TranslationByMarkupExtension;
using Ninject;

namespace SportBetting.WPF.Prism.Shared.WpfHelper
{


    public class WaitOverlayProvider : Modules.Aspects.WaitOverlayProvider.IWaitOverlayProvider
    {
        public void ShowWaitOverlay(bool isTicketPrinting = false)
        {
            WaitOverlay.Show(isTicketPrinting);
        }

        public void ShowWaitOverlay()
        {
            WaitOverlay.Show();
        }

        public void DisposeAll()
        {
            WaitOverlay.Dispose();
        }
    }

    internal class WaitOverlay
    {
        protected static readonly ILog Log = LogFactory.CreateLog(typeof(WaitOverlay));
        private static Loader _pleaseWaitLoader;
        private static Timer _pleaseWaitTimer = new Timer(60000);
        private static Dispatcher dispatcher;
        private static object _locker = new object();
        static WaitOverlay()
        {
            _pleaseWaitTimer.Elapsed += _pleaseWaitTimer_Elapsed;
            _pleaseWaitTimer.AutoReset = true;
        }

        static void _pleaseWaitTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_locker)
            {
                Dispose();
            }

        }

        private static ITranslationProvider _translationProvider;
        public static ITranslationProvider TranslationProvider
        {
            get
            {
                return _translationProvider ?? (_translationProvider = IoCContainer.Kernel.Get<ITranslationProvider>());
            }
        }

        public static void Show(bool isTicketPrinting = false)
        {
            lock (_locker)
            {
                if (_pleaseWaitTimer.Enabled)
                {
                    _pleaseWaitTimer.Stop();
                    _pleaseWaitTimer.Start();
                    Log.DebugFormat("extend overlay time");

                    return;

                }
                Log.Debug("show first overlay");
                string overlayText = isTicketPrinting ? TranslationProvider.Translate(MultistringTags.TERMINAL_TICKET_IN_PROCESS) as string : TranslationProvider.Translate(MultistringTags.TERMINAL_PLEASE_WAIT) as string;
                if (dispatcher == null)
                {
                    Thread newWindowThread = new Thread(() =>
                        {
                            
                            _pleaseWaitLoader = new Loader(overlayText);
                            if (!Debugger.IsAttached)
                            {
                                MaximizeWindow(_pleaseWaitLoader);
                            }
                            if (Debugger.IsAttached)
                            {
                                _pleaseWaitLoader.Height = 100;
                                _pleaseWaitLoader.Width = 150;
                            }
                            _pleaseWaitLoader.Show();

                            Dispatcher.Run();
                        });
                    _pleaseWaitTimer.Start();

                    newWindowThread.SetApartmentState(ApartmentState.STA);
                    newWindowThread.IsBackground = true;

                    newWindowThread.Start();

                    dispatcher = Dispatcher.FromThread(newWindowThread);

                    while (dispatcher == null)
                    {
                        Thread.Sleep(10);
                        dispatcher = Dispatcher.FromThread(newWindowThread);

                    }
                }
                else
                {
                    dispatcher.Invoke(() =>
                        {
                            _pleaseWaitLoader.PleaseWaitText = overlayText;
                            _pleaseWaitLoader.Show();
                            _pleaseWaitTimer.Start();

                        });
                }
            }

        }

        public static void MaximizeWindow(Window window)
        {
            window.Left = 0;
            window.Top = 0;
            var screens = Screen.AllScreens.Where(s => s.Primary).FirstOrDefault();
            window.Width = screens.WorkingArea.Right;
            window.Height = screens.WorkingArea.Bottom;

        }


        public static void Dispose()
        {

            lock (_locker)
            {
                try
                {
                    if (!_pleaseWaitTimer.Enabled)
                    {
                        Log.Debug("overlay is already hidden");
                        return;
                    }

                    Log.Debug("hide overlay");
                    if (dispatcher != null)
                    {
                        dispatcher.Invoke(() =>
                            {
                                if (_pleaseWaitLoader != null)
                                    _pleaseWaitLoader.Hide();
                            });
                        // dispatcher.InvokeShutdown();

                    }
                    // dispatcher = null;
                    _pleaseWaitTimer.Stop();

                }
                catch (Exception e)
                {

                    Log.Error(e.Message, e);
                }

            }
        }

    }

}
