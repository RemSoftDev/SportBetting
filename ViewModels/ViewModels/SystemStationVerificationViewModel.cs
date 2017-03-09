using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;

using System.Windows.Threading;
using Command = BaseObjects.Command;
using SportBetting.WPF.Prism.OldCode;
using SmartCardReader;


namespace ViewModels.ViewModels
{

    [ServiceAspect]
    public class SystemStationVerificationViewModel : BaseViewModel
    {
        private static ILog Log = LogFactory.CreateLog(typeof(SystemStationVerificationViewModel));

        private string _cashAcceptorStatus;
        private string _coinAcceptorStatus;
        private string _idReaderStatus;
        private string _barcodeScannerStatus;

        private DeviceStatus _isCashAcceptorFound = DeviceStatus.STATUS_UNKNOWN;
        private DeviceStatus _isCoinAcceptorFound = DeviceStatus.STATUS_UNKNOWN;
        private DeviceStatus _isIdReaderFound = DeviceStatus.STATUS_UNKNOWN;
        private DeviceStatus _isBarcodeScannerFound = DeviceStatus.STATUS_UNKNOWN;

        DispatcherTimer dispatcherTimerCash = new DispatcherTimer();
        DispatcherTimer dispatcherTimerCoin = new DispatcherTimer();
        DispatcherTimer dispatcherTimerIdReader = new DispatcherTimer();
        DispatcherTimer dispatcherTimerBarcodeScanner = new DispatcherTimer();
      
        
        private const int BARCODE_SCANNER_DETECT_TIME = 15;
        private int bc_counter = 0;

        SmartCardManager scm = SmartCardReader.SmartCardManager.GetManager();

        public SystemStationVerificationViewModel()
        {
            AcceptCommand = new Command(OnVerifyStation);
            CheckBarcodeScannerCommand = new Command(OnCheckBarcodeScanner);
            CheckCashAcceptorCommand = new Command(OnCheckCashAcceptor);
            CheckCoinAcceptorCommand = new Command(OnCheckCoinAcceptor);
            CheckIDCardCommand = new Command(OnCheckIDCard);

            dispatcherTimerCash.Tick += new EventHandler(dispatcherTimerCash_Tick);
            dispatcherTimerCash.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerCoin.Tick += new EventHandler(dispatcherTimerCoin_Tick);
            dispatcherTimerCoin.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerIdReader.Tick += new EventHandler(dispatcherTimerIdReader_Tick);
            dispatcherTimerIdReader.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimerBarcodeScanner.Tick += new EventHandler(dispatcherBarcodeScanner_Tick);
            dispatcherTimerBarcodeScanner.Interval = new TimeSpan(0, 0, 1);
            Mediator.Register<bool>(this, OnBarcodeScannerTest, MsgTag.BarcodeScannerTest);
            
        }

        public Command CheckBarcodeScannerCommand { get; private set; }
        public Command CheckCoinAcceptorCommand { get; private set; }
        public Command CheckCashAcceptorCommand { get; private set; }
        public Command CheckIDCardCommand { get; private set; }
        public Command AcceptCommand { get; private set; }




        public override void OnNavigationCompleted()
        {
            ChangeTracker.VerifyStationChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_PRINT_SYSTEM;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_VERIFY_STATION;

            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            StationRepository.BarcodeScannerTestMode = false;
            dispatcherTimerCash.Tick -= new EventHandler(dispatcherTimerCash_Tick);
            dispatcherTimerCoin.Tick -= new EventHandler(dispatcherTimerCoin_Tick);
            dispatcherTimerIdReader.Tick -= new EventHandler(dispatcherTimerIdReader_Tick);
            dispatcherTimerBarcodeScanner.Tick -= new EventHandler(dispatcherBarcodeScanner_Tick);
            dispatcherTimerCash.Stop();
            dispatcherTimerCoin.Stop();
            dispatcherTimerIdReader.Stop();
            dispatcherTimerBarcodeScanner.Stop();
            base.Close();
        }


        private void OnBarcodeScannerTest(bool res)
        {
            if (res)
            {
                //BarcodeScannerTestResult ("Barcode Scanner\r\n Status: OK",  DeviceStatus.STATUS_OK);
                BarcodeScannerTestResult(
                    String.Format("{0}\r\n {1}",
                        TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_BARCODE_SCANNER),
                        TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_OK)
                    ), DeviceStatus.STATUS_OK);
            }
        }

        private void BarcodeScannerTestResult(string msg, DeviceStatus status)
        {
            dispatcherTimerBarcodeScanner.Stop();
            bc_counter = 0;
            IsBarcodeScannerFound = status;
            BarcodeScannerStatus = msg;
            StationRepository.BarcodeScannerTestMode = false;

        }

        private void dispatcherBarcodeScanner_Tick(object sender, EventArgs e)
        {
            if (++bc_counter > BARCODE_SCANNER_DETECT_TIME)
            {
                //BarcodeScannerTestResult("Barcode Scanner\r\n Status: Not Found!", DeviceStatus.STATUS_NOK);
                BarcodeScannerTestResult(
                    String.Format("{0}\r\n {1}",
                        TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_BARCODE_SCANNER),
                        TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_NOT_FOUND)
                    ), DeviceStatus.STATUS_NOK);
            }
            else
            {
                BarcodeScannerStatus = string.Format("{0}\r\n{1} ({2})",
                TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_PLEASE_SCAN_FIRST_LINE),
                TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_PLEASE_SCAN_SECOND_LINE),
                BARCODE_SCANNER_DETECT_TIME - bc_counter);
            }
        }

        private void dispatcherTimerCash_Tick(object sender, EventArgs e)
        {
            bool result = false;

            dispatcherTimerCash.Stop();
           

            result = StationSettings.CheckBillValidator();
            if (result == true)
            {
                IsCashAcceptorFound = DeviceStatus.STATUS_OK;
                //CashAcceptorStatus = "Cash Acceptor\r\n Status: OK";
                CashAcceptorStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_CASH_ACCEPTOR),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_OK)
                    );
            }
            else
            {
                IsCashAcceptorFound = DeviceStatus.STATUS_NOK;
                //CashAcceptorStatus = "Cash Acceptor\r\n Status: Not Found!";
                CashAcceptorStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_CASH_ACCEPTOR),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_NOT_FOUND)
                    );
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
                //CoinAcceptorStatus = "Coin Acceptor\r\n Status: OK";
                CoinAcceptorStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_COIN_ACCEPTOR),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_OK)
                    );
            }
            else
            {
                IsCoinAcceptorFound = DeviceStatus.STATUS_NOK;
                //CoinAcceptorStatus = "Coin Acceptor\r\n Status: Not Found!";
                CoinAcceptorStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_COIN_ACCEPTOR),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_NOT_FOUND)
                    );
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
                IdReaderStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_ID_CARD_READER),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_OK)
                    );
                //"ID Card Reader\r\n Status: OK");
            }
            else
            {
                IsIdReaderFound = DeviceStatus.STATUS_NOK;
                IdReaderStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_ID_CARD_READER),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_NOT_FOUND)
                    );
                //IdReaderStatus = "ID Card Reader\r\n Status: Not Found!";
            }

        }

        private void CheckCashAcceptorStatus()
        {

            IsCashAcceptorFound = DeviceStatus.STATUS_UNKNOWN;
           
            //CashAcceptorStatus = "Status:\r\nVerifying...";
            CashAcceptorStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_VERIFYING)
                    );
            dispatcherTimerCash.Start();
        }

        private void CheckCoinAcceptorStatus()
        {
            IsCoinAcceptorFound = DeviceStatus.STATUS_UNKNOWN;
           
            //CoinAcceptorStatus = "Status:\r\nVerifying...";
            CoinAcceptorStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_VERIFYING)
                    );
            dispatcherTimerCoin.Start();

        }

        private void CheckIdReaderStatus()
        {
          
            //IdReaderStatus = "Status:\r\nVerifying...";
            IdReaderStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_VERIFYING)
                    );
            dispatcherTimerIdReader.Start();

        }

        private void OnCheckBarcodeScanner()
        {
            StationRepository.BarcodeScannerTestMode = true;
            bc_counter = 0;
            IsBarcodeScannerFound = DeviceStatus.STATUS_UNKNOWN;
            BarcodeScannerStatus = string.Format("{0}\r\n{1} ({2})",
                TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_PLEASE_SCAN_FIRST_LINE),
                TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_PLEASE_SCAN_SECOND_LINE),
                BARCODE_SCANNER_DETECT_TIME - bc_counter);
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
            IsIdReaderFound = DeviceStatus.STATUS_UNKNOWN;
            if (StationRepository.IsIdCardEnabled || StationRepository.StationNumber == "0000")
            {
                CheckIdReaderStatus();
            }
            else
            {
                //IdReaderStatus = "ID Card Reader\r\n Status: Disabled!";
                IdReaderStatus = String.Format(
                    "{0}\r\n {1}",
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_ID_CARD_READER),
                    TranslationProvider.Translate(MultistringTags.TERMINAL_VERIFICATION_STATUS_DISABLED)
                    );
            }

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

        private void OnCloseBalance(object sender, EventArgs e)
        {
            OnAcceptCommand();
        }

        private void OnVerifyStation()
        {
            decimal billsAmount;
            decimal coinsamount;
            int billscount;
            int coinscount;
            var amount = WsdlRepository.GetStationCashInfo(StationRepository.StationNumber, out billsAmount, out billscount, out coinsamount, out coinscount);
            if (amount > 0)
            {
                var text = TranslationProvider.Translate(MultistringTags.TERMINAL_EMPTY_BOX_BEFORE_VALIDATION).ToString();
                var yesButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_OK) as string;
                var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL) as string;
                QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText, OnCloseBalance, null);
                return;
            }

            OnAcceptCommand();

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
                    Log.Error(e.Message,e);
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

            /* ChangeTracker.VerifivationCancelled = false;
             ChangeTracker.VerificationRestart = true;*/
            Log.Debug("Exiting verification window");
            Mediator.SendMessage<long>(0, MsgTag.RestartApplication);
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


        


    }

}
