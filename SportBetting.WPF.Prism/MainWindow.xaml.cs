using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BaseObjects;
using IocContainer;
using Ninject;
using SportBetting.WPF.Prism;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportRadar.Common.Logs;
using WsdlRepository;

namespace SportBetting.Wpf.Prism
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static ILog Log = LogFactory.CreateLog(typeof(App));
        static AutoResetEvent _autoEvent = new AutoResetEvent(false);

        public MainWindow()
        {
            IoCContainer.Kernel = new StandardKernel();
            IoCContainer.Kernel.Bind<IStationRepository>().To<StationRepository>().InSingletonScope();

            InitializeComponent();
            this.Loaded += MainWindow_Activated;
            SendLogsCommand = new Command(OnSendLogsCommand);
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            this.Loaded -= MainWindow_Activated;
            Log.Debug("starting terminal starter");
            SetMessage("Initializing...");

            SetMessage("Starting Station. Please wait...");



            new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var processes = Process.GetProcesses();
                            foreach (var process in processes)
                            {
                                if (process.ProcessName.Contains("MainWpfWindow"))
                                {
                                    process.Kill();
                                    Log.Debug("kill old process");

                                    while (!process.HasExited)
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                            }

                            new Thread(() =>
                                {
                                    Process proc = new Process();
                                    proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\MainWpfWindow.exe";
                                    proc.StartInfo.Arguments = Process.GetCurrentProcess().Id.ToString();
                                    Log.Debug("starting terminal");
                                    
                                    proc.Start();
                                    Log.Debug("terminal process started");

                                }).Start();
                            
                            Thread.Sleep(60000);
                            Log.Debug("terminal not responcing trying to restart it");

                            
                        }
                        catch (Exception ex)
                        {

                            Log.Error(ex.Message, ex);
                        }

                    }

                }).Start();

            // proc.Dispose();
        }



        public void SetMessage(string sValue)
        {
            txtMessage.Text = sValue;
            bMessages.UpdateLayout();
            Thread.Sleep(500);
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        //public string Message;
        public void SetMessageControlledSleep(string sValue, int sleep)
        {
            txtMessage.Text = sValue;
            bMessages.UpdateLayout();
            Thread.Sleep(sleep);
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }


        public Command SendLogsCommand { get; set; }

        public object EnabledSendLogs
        {
            get { return true; }
        }
        public IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        private void OnSendLogsCommand()
        {
            LogSending.stationNumber = StationRepository.StationNumber;
            LogSending.SendLogs();
        }
    }
}
