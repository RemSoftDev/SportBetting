using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing.Printing;
using System.Management;

using SmartCardReader;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using SportBetting.WPF.Prism.OldCode;

namespace SportBetting.WPF.Prism
{
    public class PeripheralChecker
    {
        private bool billValidatorState;
        private string billValidatorInfo;
        private bool coinAcceptorState;
        private string coinAcceptorInfo;
        private bool printerState;
        private string printerInfo;
        private bool cardReaderState;
        private string cardReaderInfo;

        
        private static readonly object locker = new object();
        private static volatile PeripheralChecker _instance = null;
        PeripheralInfo pi = new PeripheralInfo();
        SmartCardManager scm = SmartCardReader.SmartCardManager.GetManager();

        public volatile bool StopProcess = false;
        public static PeripheralChecker Instance()
        {
            if (_instance == null)
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new PeripheralChecker();
                    }
                }
            }
            return _instance;
        }

        public PeripheralInfo  GetPeripheralInfo ()
        {
            return pi;
        }

        private void Run ()
        {
            while (true)
            {
                if (!StopProcess)
                {
                  
                    Thread.Sleep(5000);
                    CheckPeripheralInfo ();
                }
                 Thread.Sleep (55000);
            }
        }

      
        private PeripheralChecker()
        {
            SetInitialState();
            new Thread(Run).Start();
        }

        private void SetInitialState()
        {
            billValidatorState = false;
            billValidatorInfo = null;
            coinAcceptorState = false;
            coinAcceptorInfo = null;
            printerState = false;
            printerInfo = null;
            cardReaderState = false;
            cardReaderInfo = null;
        }

        private void CheckPrinter()
        {
            try
            {
                PrinterSettings settings = new PrinterSettings();
                printerInfo = settings.PrinterName;

                string query = string.Format("SELECT * from Win32_Printer WHERE Name LIKE '%{0}'", printerInfo.Replace("\\", "\\\\"));
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection coll = searcher.Get();


                foreach (ManagementObject printer in coll)
                {
                    printerState = !printer["WorkOffline"].ToString().ToLowerInvariant().Equals("true");
                }
            }
            catch
            {
                printerState = false;
            }
        }


        private void CheckCashValidators ()
        {
            List<Nbt.Services.Scf.CashIn.DeviceInfo> list = StationSettings.GetShortDeviceInventoryList ();

            if (list != null)
            {
                foreach (Nbt.Services.Scf.CashIn.DeviceInfo di in list)
                {
                    if (di.device_type == Nbt.Services.Scf.CashIn.DeviceType.COIN_ACCEPTOR)
                    {
                        coinAcceptorState = true;                      
                        coinAcceptorInfo = di.device_model;                   
                    }
                    else if (di.device_type == Nbt.Services.Scf.CashIn.DeviceType.BILL_VALIDATOR)
                    {
                        billValidatorState = true;
                        billValidatorInfo = di.device_model; 
                    }
                }

            }
        }

        private void CheckCardReader()
        {
            bool result = false;

              CardReaderInfo cri = scm.GetCardReaderInventory();

              if (cri != null)
              {
                  cardReaderInfo = cri.type;
                  result = true;
              }
            
            cardReaderState = result;
        }

        private void CheckPeripheralInfo()
        {
            try
            {
                SetInitialState();
                CheckPrinter();
                CheckCardReader();
             
                CheckCashValidators();

                lock (pi)
                {
                    pi.printerState = printerState;
                    pi.printerVersion = printerInfo;
                    pi.coinAcceptorState = coinAcceptorState;
                    pi.coinAcceptorVersion = coinAcceptorInfo;
                    pi.billValidatorState = billValidatorState;
                    pi.billValidatorVersion = billValidatorInfo;
                    pi.cardReaderState = cardReaderState;
                    pi.cardReaderVersion = cardReaderInfo;
                }
            }
            catch { }
        }
    }
}
