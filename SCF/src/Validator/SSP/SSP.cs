using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Nbt.Services.Scf.CashIn.Validator;
using Preferences.Services.Preference;
using SerialPortManager;
using SportRadar.Common.Logs;



namespace Nbt.Services.Scf.CashIn.Validator.SSP //Nbt.Services.Scf.CashIn.src.Validator.SSP
{
    public class SSP: AValidator
    {
        private decimal _credit;
        private decimal _maxLimit;

        private static SerialPortManager.SerialPortManager spm = SerialPortManager.SerialPortManager.Instance;
        private SSP_Validator Validator = null;
        private static object validatorLock = new Object();
        private volatile bool datasetIsValid = true;
        private string port = null;
        private Thread _pollAcceptor;
        private static ILog Log = LogFactory.CreateLog(typeof(SSP_Validator));

        public SSP (IPrefSupplier pref, string prefKey, string mode)
        {
            bool result = false;
            List<CommunicationResource> sp = spm.GetSafeSerialPortsMap();

            foreach (CommunicationResource cr in sp)
            {
                if (cr.PortType == ResourceType.BILL_VALIDATOR_SERIAL_PORT &&
                    cr.ProtocolType == DeviceProtocol.PROTOCOL_SSP)
                {
                    port = cr.PortName;
                    Validator = new SSP_Validator ();
                    result = ConnectToValidator ();
                    break;

                }
            }

            if (result)
            {
                result = InitializeBillValidator ();
            }

            if (result)
            {
                CreatePollingThread ();
            }

            ValFound = result;        
        }

        private void CreatePollingThread()
        {
            _pollAcceptor = new Thread (new ThreadStart (RunPollingThread));
            _pollAcceptor.Start();
            base.Enable(0);
            Thread.Sleep(800);
            base.Disable();
        }

        private void RunPollingThread ()
        {
            int err_counter = 0;
            const int ERR_CNTR_MAX = 5;
            decimal amount = 0;
            string currency = string.Empty;
            bool result = false;
            while (true)
            {
                System.Threading.Thread.Sleep (250);
                lock (validatorLock)
                {
                    result = Validator.DoPool (out amount, out currency);
                }
                if (result)
                {
                    if (amount > 0)
                    {
                        OnCashIn(new CashInEventArgs(amount, false));
                        Log.Debug(String.Concat("Note [", amount, " ", currency, "] accepted!"));
                    }
                    err_counter = 0;
                }
                else if (err_counter++ > ERR_CNTR_MAX)
                {
                    TryReconnect ();
                }
            }
        }

        private bool TryReconnect()
        {
            while (!ConnectToValidator())
            {
                Thread.Sleep (1000);
            }
            return InitializeBillValidator ();
        }

        private bool ConnectToValidator ()
        {
            bool result = false;

            if (Validator != null && port!= null)
            {
                Validator.SSPComms.CloseComPort ();
                Validator.CommandStructure.EncryptionStatus = false;
                Validator.CommandStructure.ComPort = port;
                Validator.CommandStructure.SSPAddress = 0;
                Validator.CommandStructure.Timeout = 3000;

                result = Validator.OpenComPort() && Validator.NegotiateKeys();
            }

            return result;
        }

        private byte FindMaxProtocolVersion ()
        {
            byte b = 0x06;

            while (true)
            {
                Validator.SetProtocolVersion (b);
                if (Validator.CommandStructure.ResponseData[0] == CCommands.SSP_RESPONSE_CMD_FAIL)
                {
                    return --b;
                }
               
                if (++b > 20)
                {
                    return 0x06; // return default if protocol 'runs away'
                }
            }
        }

        private bool InitializeBillValidator()
        {
            bool result = false;

            Validator.CommandStructure.EncryptionStatus = true;
            byte maxPVersion = FindMaxProtocolVersion ();

            if (maxPVersion > 6)
            {
                Validator.SetProtocolVersion (maxPVersion);
                result = true;
            }

            Validator.Setup();
            Validator.SetInhibits(0, _maxLimit);
            Validator.EnableValidator();
/*
#if BETCENTER  
                
#else
            if (!CheckValidatorDataset())
            {
                Log.Error("Bill Validator has wrong dataset!");
                datasetIsValid = false;
            }
#endif
 * */
            
            return result;
        }


        private bool CheckValidatorDataset ()
        {
            bool result = false;
            const string DEFAULT_DATA_SET = "EUR05";
            const string NV_10_DATASET = "EUR02";
            const string NV_10_DATASET_2 = "EUR45";
            try
            {
                string dataset = Validator.GetDatasetVersion();
                if (dataset != null && dataset.Length > 5)
                {
                    dataset = dataset.Substring (0, 5);
                    if (dataset == DEFAULT_DATA_SET ||
                        dataset == NV_10_DATASET ||
                        dataset == NV_10_DATASET_2)
                    {
                        result = true;
                    }
                }
               
            }
            catch
            {
               
            }

            return result;
        }


        public override bool SetEnabledChannels (decimal ActValue)
        {
            return Validator.SetInhibits (ActValue, _maxLimit);
        }

        public override bool Disable()
        {
            bool result = false;
           

            lock (validatorLock)
            {
                base.Disable();
                _credit = 0;
                result = Validator.DisableValidator ();
            }

            return result;
        }

        public override bool Enable (decimal MaxLimit)
        {

            bool result = false;

            lock (validatorLock)
            {
                base.Enable(MaxLimit);
                _maxLimit = MaxLimit < 0 ? 0 : MaxLimit;
                result = Validator.EnableValidator();
                if (result)
                {
                    result = Validator.SetInhibits(this._credit, _maxLimit);
                }
            }
           
            return result;
        }

        public override void SetCredit (decimal credit)
        {
            base.SetCredit(credit);
            _credit = credit;
        }

        public override decimal GetCredit()
        {
            base.GetCredit();
            return _credit;
        }

        public override bool ResetCredit()
        {
            base.ResetCredit();
            _credit = 0;
            return true;
        }

        public override bool IsDataSetValid()
        {
            return datasetIsValid;
        }

        public override List<DeviceInfo> GetDeviceInventory()
        {
            List<DeviceInfo> list = null;

            lock (validatorLock)
            {
                list = Validator.GetDeviceInventory ();
            }

            return list;
        }

        public override List<DeviceInfo> GetShortDeviceInventory()
        {
            List<DeviceInfo> list = null;

            lock (validatorLock)
            {
                list = Validator.GetShortDeviceInventory();
            }

            return list;
        }
        
        public override bool CheckBillValidator()
        {
            bool result = false;
            string dataset = null;

            lock (validatorLock)
            {
                dataset = Validator.GetDatasetVersion ();
            }
            if (dataset != null && dataset != string.Empty)
            {
                result = true;
            }

            return result;
        }
    }
}
