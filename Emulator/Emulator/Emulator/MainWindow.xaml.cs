using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Diagnostics;
using System.ComponentModel;

namespace Emulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
       
        public const string DETECTED = "DETECTED!";
        public const string NOT_DETECTED = "NOT DETECTED!";
        public const string CONNECTED = "Connected";
        public const string NO_CONNECTION = "No Connection";
        public const string DISABLE = "DISABLE";
        public const string ENABLE = "ENABLE";

        private string _Da2DetectionStatus = DETECTED;
        private string _TerminalStatus = DETECTED;
        private string _ConnectionStatus = NO_CONNECTION;
        private string _BillValidatorStatus = DISABLE;
        private string _CoinAcceptorStatus = DISABLE;
        private string _VCP_Status = "0";

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        EmulatorA emulator = new EmulatorA();
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _SystemIsInitialized = false;

        public string DA2DetectionStatus
        {
            get { return _Da2DetectionStatus; }
            set
            {
                if (value != _Da2DetectionStatus)
                { _Da2DetectionStatus = value; RaisePropertyChanged("DA2DetectionStatus"); }
            }
        }
        public string TerminalStatus
        {
            get { return _TerminalStatus; }
            set
            {
                if (value != _TerminalStatus)
                { _TerminalStatus = value; RaisePropertyChanged("TerminalStatus"); }
            }
        }
        public string ConnectionStatus
        {
            get { return _ConnectionStatus; }
            set
            {
                if (value!=_ConnectionStatus)
                { _ConnectionStatus = value; RaisePropertyChanged("ConnectionStatus"); }
            }
        }
        public string BillValidatorStatus
        {
            get { return _BillValidatorStatus; }
            set
            {
                if (value != _BillValidatorStatus)
                { _BillValidatorStatus = value; RaisePropertyChanged("BillValidatorStatus"); }
            }
        }
        public string CoinAcceptorStatus
        {
            get { return _CoinAcceptorStatus; }
            set
            {
                if (value != _CoinAcceptorStatus)
                { _CoinAcceptorStatus = value; RaisePropertyChanged("CoinAcceptorStatus"); }
            }
        }
        public string VCP_Status
        {
            get { return _VCP_Status; }
            set
            {
                if (value != _VCP_Status)
                { _VCP_Status = value; RaisePropertyChanged("VCP_Status"); }
            }
        }


        public string CautionMsg
        {
            get { return string.Format("Before you start, you need to shut down the Terminal application!\nDA2 adapter must be disconnected from computer!"); }
        }

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            DisableAll();
            CheckInitialSystemState();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!_SystemIsInitialized)
                {
                    dispatcherTimer.Stop();
                    CheckInitialSystemState();
                    dispatcherTimer.Start();
                }
                else if (!emulator.IsConnectionAlive())
                {
                    ConnectionStatus = NO_CONNECTION;
                    DisableAll();
                }
                else
                {
                    ConnectionStatus = CONNECTED;
                    if (emulator.BillValidator.IsDeviceEnabled())
                    {
                        EnableBillValidator();
                    }
                    else
                    {
                        DisableBillValidator();
                    }

                    if (emulator.CoinAcceptor.IsDeviceEnabled())
                    {
                        EnableCoinAcceptor();
                    }
                    else
                    {
                        DisableCoinAcceptor();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void CheckInitialSystemState()
        {
           
            if (!CheckTerminalStatus())
            {
                TabPanel.SelectedIndex = 0;
            }
            else if (!CheckVirtualSerialPorts())
            {
                TabPanel.SelectedIndex = 1;
            }
            else
            {
                TabPanel.SelectedIndex = 2;

                if (!emulator.Run())
                {
                    string port = emulator.GetComPortName();
                    if (string.IsNullOrEmpty(port))
                    {
                        port = "<invalide value>";
                    }
                    string msg = "Can't open " + port + " Port! Application will be terminated!";
                    MessageBox.Show(msg, "error");
                    Environment.Exit(0);
                }
                _SystemIsInitialized = true;
            }
          
        }

        private bool CheckTerminalStatus()
        {
            bool result = true;

            if (emulator.IsTerminalRunning())
            {
                TerminalStatus = DETECTED;               
                result = false;
            }
            else
            {
                TerminalStatus = NOT_DETECTED;
            }

            if (emulator.IsDA2AdapterInstalled())
            {
                DA2DetectionStatus = DETECTED;
                result = false;
            }
            else
            {
                DA2DetectionStatus = NOT_DETECTED;
            }

            return result;
        }

        private bool CheckVirtualSerialPorts()
        {
            bool result = false;
            int count = 0;
            List<string> ports = emulator.GetInstalledVirtualPorts();

            try
            {
                count = ports.Count;
                if (count == 2)
                {
                   result = true;
                }
            }
            catch
            {

            }

            VCP_Status = count.ToString()+ " PORTS DETECTED";

            return result;
        }

        private void DisableBillValidator ()
        {
            Button_5_Euro.IsEnabled = false;
            Button_10_Euro.IsEnabled = false;
            Button_20_Euro.IsEnabled = false;
            Button_50_Euro.IsEnabled = false;
            Button_100_Euro.IsEnabled = false;
            Button_200_Euro.IsEnabled = false;

            BillValidatorStatus = DISABLE;
        }
        private void EnableBillValidator()
        {
            Button_5_Euro.IsEnabled = true;
            Button_10_Euro.IsEnabled = true;
            Button_20_Euro.IsEnabled = true;
            Button_50_Euro.IsEnabled = true;
            Button_100_Euro.IsEnabled = true;
            Button_200_Euro.IsEnabled = true;

            BillValidatorStatus = ENABLE;
        }
        private void DisableCoinAcceptor()
        {
            Button_5_Cent.IsEnabled = false;
            Button_10_Cent.IsEnabled = false;
            Button_20_Cent.IsEnabled = false;
            Button_50_Cent.IsEnabled = false;
            Button_1_Euro.IsEnabled = false;
            Button_2_Euro.IsEnabled = false;

            CoinAcceptorStatus = DISABLE;
        }
        private void EnableCoinAcceptor()
        {
            Button_5_Cent.IsEnabled = false;
            Button_10_Cent.IsEnabled = true;
            Button_20_Cent.IsEnabled = true;
            Button_50_Cent.IsEnabled = true;
            Button_1_Euro.IsEnabled = true;
            Button_2_Euro.IsEnabled = true;

            CoinAcceptorStatus = ENABLE;
        }

        private void DisableAll()
        {
            DisableBillValidator();
            DisableCoinAcceptor();
        }

        private void EnableAll()
        {
            EnableBillValidator();
            EnableCoinAcceptor();
        }

        private void OnCheckVCP(object sender, RoutedEventArgs e)
        {
            CheckVirtualSerialPorts();
        }
          
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void OnIsertMoney (object sender, RoutedEventArgs e)
        {
            string buttonText = ((Button)sender).Name;

            switch (buttonText)
            {
                case "Button_5_Euro":
                    emulator.BillValidator.InjectCashEvent("EU0005A");
                    break;
                case "Button_10_Euro":
                    emulator.BillValidator.InjectCashEvent("EU0010A");
                    break;
                case "Button_20_Euro":
                    emulator.BillValidator.InjectCashEvent("EU0020A");
                    break;
                case "Button_50_Euro":
                    emulator.BillValidator.InjectCashEvent("EU0050A");
                    break;
                case "Button_100_Euro":
                    emulator.BillValidator.InjectCashEvent("EU0100A");
                    break;
                case "Button_200_Euro":
                    emulator.BillValidator.InjectCashEvent("EU0200A");
                    break;

                case "Button_10_Cent":
                    emulator.CoinAcceptor.InjectCashEvent("EU010A");
                    break;
                case "Button_20_Cent":
                    emulator.CoinAcceptor.InjectCashEvent("EU020A");
                    break;
                case "Button_50_Cent":
                    emulator.CoinAcceptor.InjectCashEvent("EU050A");
                    break;
                case "Button_1_Euro":
                    emulator.CoinAcceptor.InjectCashEvent("EU100A");
                    break;
                case "Button_2_Euro":
                    emulator.CoinAcceptor.InjectCashEvent("EU200A");
                    break;

                default:
                    break;

            }
            
        }
    }
}
