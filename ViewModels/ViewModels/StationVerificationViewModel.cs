using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Configuration;
using System.Text;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Keyboard.ViewModels;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;
using System.Windows.Threading;
using System.Windows.Input;
using SportBetting.WPF.Prism.OldCode;
using Command = BaseObjects.Command;
using SmartCardReader;

namespace ViewModels.ViewModels
{


    [ServiceAspect]
    public class StationVerificationViewModel : BaseViewModel
    {
        private static ILog Log = LogFactory.CreateLog(typeof(StationVerificationViewModel));

        //private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(StationVerificationViewModel));
        private string _cashAcceptorStatus;
        private string _coinAcceptorStatus;
        private string _idReaderStatus;
        private string _barcodeScannerStatus;
        private string _serverConnectionStatus;

        private DeviceStatus _isCashAcceptorFound = DeviceStatus.STATUS_UNKNOWN;
        private DeviceStatus _isCoinAcceptorFound = DeviceStatus.STATUS_UNKNOWN;
        private DeviceStatus _isIdReaderFound = DeviceStatus.STATUS_UNKNOWN;
        private DeviceStatus _isBarcodeScannerFound = DeviceStatus.STATUS_UNKNOWN;
        private DeviceStatus _serverConnectionEstablished = DeviceStatus.STATUS_UNKNOWN;

        DispatcherTimer dispatcherTimerCash = new DispatcherTimer();
        DispatcherTimer dispatcherTimerCoin = new DispatcherTimer();
        DispatcherTimer dispatcherTimerIdReader = new DispatcherTimer();
        DispatcherTimer dispatcherTimerBarcodeScanner = new DispatcherTimer();

        private bool KeyboardRegionInitialized = false;
        private const int BARCODE_SCANNER_DETECT_TIME = 15;
        private const string BARCODE_SCANNER_PLEASE_SCAN_MSG = "Please scan\r\nany note or ticket! ({0})";
        private int bc_counter = 0;

        SmartCardManager scm = SmartCardReader.SmartCardManager.GetManager ();
            
        public StationVerificationViewModel()
        {


            AcceptCommand = new Command(OnAcceptCommand);
            TestModeCommand = new Command(OnTestModeCommand);
          
            CheckBarcodeScannerCommand = new Command(OnCheckBarcodeScanner);
            CheckCashAcceptorCommand = new Command(OnCheckCashAcceptor);
            CheckCoinAcceptorCommand = new Command(OnCheckCoinAcceptor);
            CheckIDCardCommand = new Command(OnCheckIDCard);
            CheckServerConnectionCommand = new Command(OnCheckServerConnection);
            UnfocusComand = new Command(OnUnfocus);

            dispatcherTimerCash.Tick += new EventHandler(dispatcherTimerCash_Tick);
            dispatcherTimerCash.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerCoin.Tick += new EventHandler(dispatcherTimerCoin_Tick);
            dispatcherTimerCoin.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerIdReader.Tick += new EventHandler(dispatcherTimerIdReader_Tick);
            dispatcherTimerIdReader.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerBarcodeScanner.Tick += new EventHandler(dispatcherBarcodeScanner_Tick);
            dispatcherTimerBarcodeScanner.Interval = new TimeSpan(0, 0, 1);
            KeydownCommand = new Command<KeyEventArgs>(OnKeyDown);
           
        }

        public Command CheckBarcodeScannerCommand { get; private set; }
        public Command CheckCoinAcceptorCommand { get; private set; }
        public Command CheckCashAcceptorCommand { get; private set; }
        public Command CheckIDCardCommand { get; private set; }
        public Command CheckServerConnectionCommand { get; private set; }
        public Command AcceptCommand { get; private set; }
        public Command UnfocusComand { get; private set; }
        public Command TestModeCommand { get; private set; }
        public Command<KeyEventArgs> KeydownCommand { get; set; }

        public void OnKeyDown(KeyEventArgs e) 
        {
            if (StationRepository.BarcodeScannerTestMode)
            {
                var charinput = KeyInterop.VirtualKeyFromKey(e.Key);
                BarCodeConverter.ProcessBarcode((char)charinput);
                e.Handled = true;
                if (BarCodeConverter.IsComplete())
                {
                    BarcodeScannerTestResult("Barcode Scanner\r\n Status: OK", DeviceStatus.STATUS_OK);
                }
            }
        }

        private void InitializeKeyboardRgn()
        {
            try
            {
                MyRegionManager.NavigateUsingViewModel<KeyboardViewModel>(RegionNames.VerificationKeyboardRegion);
                KeyboardRegionInitialized = true;
            }
            catch
            {
            }
        }

        private void BarcodeScannerTestResult(string msg, DeviceStatus status)
        {
            dispatcherTimerBarcodeScanner.Stop();
            bc_counter = 0;
            IsBarcodeScannerFound =status;
            BarcodeScannerStatus = msg;
            StationRepository.BarcodeScannerTestMode = false;

        }

        private void dispatcherBarcodeScanner_Tick (object sender, EventArgs e)
        {
            if (++bc_counter > BARCODE_SCANNER_DETECT_TIME)
            {
                BarcodeScannerTestResult ("Barcode Scanner\r\n Status: Not Found!", DeviceStatus.STATUS_NOK);
            }
            else
            {
                BarcodeScannerStatus = String.Format(BARCODE_SCANNER_PLEASE_SCAN_MSG, BARCODE_SCANNER_DETECT_TIME - bc_counter);
            }
        }

        private void dispatcherTimerCash_Tick(object sender, EventArgs e)
        {
            bool result = false;

            dispatcherTimerCash.Stop();

            result = StationSettings.CheckBillValidator ();
            if (result == true)
            {
                IsCashAcceptorFound = DeviceStatus.STATUS_OK;
                CashAcceptorStatus = "Cash Acceptor\r\n Status: OK";
            }
            else
            {
                IsCashAcceptorFound = DeviceStatus.STATUS_NOK;
                CashAcceptorStatus = "Cash Acceptor\r\n Status: Not Found!";
            }
        }

        private void dispatcherTimerCoin_Tick(object sender, EventArgs e)
        {
            bool result = false;

            dispatcherTimerCoin.Stop();

            result = StationSettings.CheckCoinAcceptor ();

            if (result == true)
            {
                IsCoinAcceptorFound = DeviceStatus.STATUS_OK;
                CoinAcceptorStatus = "Coin Acceptor\r\n Status: OK";
            }
            else
            {
                IsCoinAcceptorFound = DeviceStatus.STATUS_NOK;
                CoinAcceptorStatus = "Coin Acceptor\r\n Status: Not Found!";
            }

        }

        private void dispatcherTimerIdReader_Tick(object sender, EventArgs e)
        {
            bool result = false;
            dispatcherTimerIdReader.Stop();
           
             CardReaderInfo cri = scm.GetCardReaderInventory();

            if (cri != null)
            {
                result = true;
            }

            if (result)
            {
                IsIdReaderFound = DeviceStatus.STATUS_OK;
                IdReaderStatus = "ID Card Reader\r\n Status: OK";
            }
            else
            {
                IsIdReaderFound = DeviceStatus.STATUS_NOK;
                IdReaderStatus = "ID Card Reader\r\n Status: Not Found!";
            }

        }

        private void CheckCashAcceptorStatus()
        {

            IsCashAcceptorFound = DeviceStatus.STATUS_UNKNOWN;
            
            CashAcceptorStatus = "Status:\r\nVerifying...";
            dispatcherTimerCash.Start();
        }

        private void CheckCoinAcceptorStatus()
        {
            IsCoinAcceptorFound = DeviceStatus.STATUS_UNKNOWN;
          
            CoinAcceptorStatus = "Status:\r\nVerifying...";
            dispatcherTimerCoin.Start();

        }

        private void CheckIdReaderStatus()
        {

            IsIdReaderFound = DeviceStatus.STATUS_UNKNOWN;
           
            IdReaderStatus = "Status:\r\nVerifying...";
            dispatcherTimerIdReader.Start();

        }

        private void CheckServerConnection()
        {
            IsServerConnectionEstablished = DeviceStatus.STATUS_UNKNOWN;
            ServerConnectionStatus = "Status:\r\nVerifying...";
            try
            {
                var config = ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;
                var uri = config.Endpoints[0].Address;
                new TcpClient(uri.Host, uri.Port);
                ServerConnectionStatus = "Server connection\r\nStatus: OK";
                IsServerConnectionEstablished = DeviceStatus.STATUS_OK;
            }
            catch (Exception ex)
            {
                ServerConnectionStatus = "Server connection\r\nStatus: Failure!";
                IsServerConnectionEstablished = DeviceStatus.STATUS_NOK;
            }
        }

        private void OnCheckBarcodeScanner()
        {
            StationRepository.BarcodeScannerTestMode = true;
            bc_counter = 0;
            IsBarcodeScannerFound = DeviceStatus.STATUS_UNKNOWN;
            BarcodeScannerStatus = String.Format(BARCODE_SCANNER_PLEASE_SCAN_MSG, BARCODE_SCANNER_DETECT_TIME - bc_counter);
            dispatcherTimerBarcodeScanner.Start();
        }

        private void OnCheckCashAcceptor()
        {
            CheckCashAcceptorStatus();
        }

        private void OnCheckCoinAcceptor()
        {
            CheckCoinAcceptorStatus();
        }

        private void OnCheckIDCard()
        {
            if (StationRepository.IsIdCardEnabled || StationRepository.StationNumber == "0000")
            {
                CheckIdReaderStatus();
            }
            else
            {
                IdReaderStatus = "ID Reader is disabled!";
            }

        }

        private void OnCheckServerConnection()
        {
            CheckServerConnection();
        }

        public DeviceStatus IsCashAcceptorFound
        {
            get { return _isCashAcceptorFound; }
            set
            {
                _isCashAcceptorFound = value;
                OnPropertyChanged("IsCashAcceptorFound");
            }
        }
        public string CashAcceptorStatus
        {
            get { return _cashAcceptorStatus; }
            set { _cashAcceptorStatus = value; OnPropertyChanged("CashAcceptorStatus"); }
        }

        public DeviceStatus IsCoinAcceptorFound
        {
            get { return _isCoinAcceptorFound; }
            set
            {
                _isCoinAcceptorFound = value;
                OnPropertyChanged("IsCoinAcceptorFound");
            }
        }
        public string CoinAcceptorStatus
        {
            get { return _coinAcceptorStatus; }
            set { _coinAcceptorStatus = value; OnPropertyChanged("CoinAcceptorStatus"); }
        }

        public DeviceStatus IsIdReaderFound
        {
            get { return _isIdReaderFound; }
            set
            {
                _isIdReaderFound = value;
                OnPropertyChanged("IsIdReaderFound");
            }
        }
        public string IdReaderStatus
        {
            get { return _idReaderStatus; }
            set { _idReaderStatus = value; OnPropertyChanged("IdReaderStatus"); }
        }

        public DeviceStatus IsBarcodeScannerFound
        {
            get { return _isBarcodeScannerFound; }
            set
            {
                _isBarcodeScannerFound = value;
                OnPropertyChanged("IsBarcodeScannerFound");
            }
        }
        public string BarcodeScannerStatus
        {
            get { return _barcodeScannerStatus; }
            set { _barcodeScannerStatus = value; OnPropertyChanged("BarcodeScannerStatus"); }
        }

        public string ServerConnectionStatus
        {
            get { return _serverConnectionStatus; }
            set { _serverConnectionStatus = value; OnPropertyChanged("ServerConnectionStatus"); }
        }
        public DeviceStatus IsServerConnectionEstablished
        {
            get { return _serverConnectionEstablished; }
            set
            {
                _serverConnectionEstablished = value;
                OnPropertyChanged("IsServerConnectionEstablished");
            }
        }

        private void OnUnfocus()
        {
            IsFocused = false;
        }


        private void OnTestModeCommand()
        {
            //StationRepository.SetTestModeSetting(true);
            Mediator.SendMessage<string>("testmode", MsgTag.RestartInTestMode);
        }


        [WsdlServiceSyncAspectSilent]
        private void OnAcceptCommand()
        {

            if (_sVerificationCode == "")
            {
                return;
            }

            Log.DebugFormat("Sending verification code {0}", _sVerificationCode);



            // Create a collection object and populate it using the PFX file

            bool bResult = true;
            string sStationNumber = null;
#if STAGINGUITEST
            sStationNumber = _sVerificationCode.Substring(0, 4);
#else
            if (System.Diagnostics.Debugger.IsAttached && string.IsNullOrEmpty(ConfigurationManager.AppSettings["CertificateUrl"]))
            {
                sStationNumber = _sVerificationCode.Substring(0, 4);
            }
            else if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CertificateUrl"]))
            {

                string url = ConfigurationManager.AppSettings["CertificateUrl"];

                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(url);
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                ASCIIEncoding encoding = new ASCIIEncoding();
                string postData = "c=" + _sVerificationCode;
                byte[] data = encoding.GetBytes(postData);

                httpWReq.Method = "POST";
                httpWReq.ContentType = "application/x-www-form-urlencoded";
                httpWReq.ContentLength = data.Length;

                using (Stream stream = httpWReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();
                System.IO.Stream dataStream = response.GetResponseStream();

                System.IO.BinaryReader streamReader = new System.IO.BinaryReader(dataStream);

                IList<byte[]> array = new List<byte[]>();
                do
                {
                    byte[] tmpfile = streamReader.ReadBytes(1024);
                    array.Add(tmpfile);
                } while (array.Last().Length == 1024);

                byte[] file = new byte[array.Sum(x => x.Length)];

                int i = 0;
                foreach (var bytese in array)
                {
                    foreach (var b in bytese)
                    {
                        file[i++] = b;
                    }
                }


                dataStream.Close();
                streamReader.Close();

                string certFilename = DateTime.Now.ToFileTime() + "cert.p12";

                FileStream fs = new FileStream(certFilename, FileMode.CreateNew);
                BinaryWriter bw = new BinaryWriter(fs);

                for (int j = 0; j < file.Length; j++)
                    bw.Write(file[j]);

                streamReader.Close();
                response.Close();
                fs.Flush();
                fs.Close();

                try
                {
                    X509Certificate2 cert = new X509Certificate2(file, _sVerificationCode);
                    var serviceRuntimeUserCertificateStore = new X509Store(StoreName.My);
                    serviceRuntimeUserCertificateStore.Open(OpenFlags.ReadWrite);
                    foreach (var certificate in serviceRuntimeUserCertificateStore.Certificates)
                    {
                        if (certificate.Subject.Contains("Sportradar AG"))
                        {
                            serviceRuntimeUserCertificateStore.Remove(certificate);
                            break;
                        }

                    }

                    var process = new Process();
                    var startInfo = new ProcessStartInfo();
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "certutil.exe";
                    startInfo.Arguments = " -f -user -p " + _sVerificationCode + " -importpfx " + certFilename;
                    process.StartInfo = startInfo;
                    process.Start();

                    serviceRuntimeUserCertificateStore.Close();
                    sStationNumber = cert.Subject.Substring(cert.Subject.IndexOf("CN=") + 3, cert.Subject.IndexOf(", ", cert.Subject.IndexOf("CN=")) - cert.Subject.IndexOf("CN=") - 3);
                    sStationNumber = sStationNumber.Replace("terminal", "");

                }
                catch (Exception e)
                {
                    Log.Error(e.Message, e);
                    ShowError(TranslationProvider.Translate(MultistringTags.CERTIFICATE_ERROR).ToString());
                    return;
                }

            }
            else
            {
                try
                {
                    sStationNumber = WsdlRepository.GetStationByVerificationNumber(_sVerificationCode);
                }
                catch (Exception ex)
                {
                    Log.Error("", ex);
                    ShowError(ex.Message);
                    return;
                }

            }
#endif
            try
            {
                if (bResult)
                {
                    Log.InfoFormat("Saving station number {0} to database", sStationNumber);
                    StationRepository.SetStationAppConfigValue("StationNumber", sStationNumber);
                }
            }
            catch (Exception Ex)
            {
                Log.Error("", Ex);
                ShowError("Can't connect to database " + Ex.Message);
                return;
            }

            _sVerificationCode = "";
            Log.Debug("Closing verification window");

            ChangeTracker.VerifivationCancelled = false;
            ChangeTracker.VerificationRestart = true;
            Log.Debug("Exiting verification window");

            Mediator.SendMessage<long>(123, MsgTag.RestartApplication);
        }


        private string _sVerificationCode = "";
        public string VerificationCode
        {
            get { return _sVerificationCode; }
            set
            {
                _sVerificationCode = value;
            }
        }


        private bool _isFocused;
        public bool IsFocused
        {
            get
            {
                //  Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                return _isFocused;
            }
            set
            {
                if (!KeyboardRegionInitialized)
                {
                    InitializeKeyboardRgn();
                }
                _isFocused = value;
                if (_isFocused)
                {
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
            }
        }


    }

}
