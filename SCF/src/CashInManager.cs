using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NLogger;
using Nbt.Services.Scf.CashIn.Validator;
using Nbt.Services.Serios;
using Nbt.Services.Key;
using Preferences.Services.Preference;

//using NBT.Services.Scf.CashIn.Validator.InnovativeTechnology;


namespace Nbt.Services.Scf.CashIn
{

    public enum DeviceType
    {
        UNKNOWN = 0,
        BILL_VALIDATOR,
        COIN_ACCEPTOR
    }

    public class DeviceInfo
    {
        public DeviceType device_type;
        public string device_producer;
        public string device_model;
        public string device_serial_number;
        public string device_firmware_version;
    }

    public class CashInManager : IValidator
    {

        //private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(CashInManager).Name);

        //private static volatile bool done = false;
        private ValidatorComposite validator = null;

        public ValidatorComposite Validator
        {
            get { return validator; }
        }

        private SeriosMK1Keys keySystem = null;

        public decimal MaxCredit = 0;		//0 is unlimited
        public bool AceptorsFound = false;
        private readonly object eventLock = new object();

        private EventHandler<CashInEventArgs> m_CashIn;
       
        public event EventHandler<CashInEventArgs> CashIn
        {
            add
            {
                lock (eventLock)
                {
                    m_CashIn -= value;
                    m_CashIn += value;
                }
            }
            remove
            {
                lock (eventLock) { m_CashIn -= value; }
            }
        }

        public event EventHandler<KeyEventArgs> KeyEventHandler;
        public EventHandler<ValidatorEventArgs<string>> CashLimitExceededEventHandler = null;

        #region event handling
        public void Handler1(object sender, CashInEventArgs e)
        {
            Console.WriteLine("Event1:{0}. CashInvalue:money:{1}, credit:{2}", sender, e.MoneyIn, e.Credit);
            Console.WriteLine("sum=" + this.validator.GetCredit());

        }

        public void Handler2(object sender, CashInEventArgs e)
        {
            //Console.WriteLine("Event2:{0}. CashInvalue:{1}", sender, e.Credit);
            //Console.WriteLine("sum="+this.validator.GetCredit());
            //if (!validator.IsEnabled())

            /*if (MaxCredit>0 && validator.GetCredit() >= MaxCredit)
                done=false;*/
            //TODO  some checks..
            Update(e);

        }

        public void OnCashLimitExeeded (object sender, ValidatorEventArgs<string> e)
        {
            EventHandler<ValidatorEventArgs<string>> handler = CashLimitExceededEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void KeyHandlerMethod(object sender, KeyEventArgs e)
        {
            Console.WriteLine("Event:{0}. Keyvalue:{1}", sender, e.KeyID);
            UpdateKey(e);

        }

        private void Update(CashInEventArgs e)
        {
            EventHandler<CashInEventArgs> tmpCI = m_CashIn;	//for thread safety
            if (tmpCI != null)
                tmpCI(this, e);
        }

        private void UpdateKey(KeyEventArgs e)
        {
            EventHandler<KeyEventArgs> tmpCI = KeyEventHandler;	//for thread safety
            if (tmpCI != null)
                tmpCI(this, e);
        }
        #endregion

        public IList<Delegate> GetInvocationList()
        {
            if (m_CashIn != null) return m_CashIn.GetInvocationList();
            return new List<Delegate>();
        }


        public CashInManager(string prefFileName, string mode)
        {
            /*ValidatorComposite validator = new ValidatorComposite();
            validator.CashInComposite += Handler1;
            validator.CashInComposite += Handler2;
            validator.Enable(0);*/
            //new TextFilePref("CashIn", "CashIn", false, false);
            validator = new ValidatorComposite(new TextFilePref(prefFileName, prefFileName, false, false), mode);

            //keySystem = new SeriosMK1Keys();
            Initialize();
        }

        public CashInManager (IPrefSupplier prefFile, string mode)
        {
            Log.DebugFormat ("CashInManager init....");
           
            if (prefFile != null)
            {
                foreach (var p in prefFile.GetAllList())
                {
                    Log.DebugFormat ("{0} = {1}", p.Key, p.Value);
                }
                validator = new ValidatorComposite (prefFile, mode);
            }
            if (validator != null)
            {
                AceptorsFound = validator.ValFound;
                Initialize();
            }
            //keySystem = new SeriosMK1Keys();
        }

        private void Initialize()
        {
            validator.CashIn += Handler1;
            validator.CashIn += Handler2;
            validator.CashLimitExceededEventHandler += OnCashLimitExeeded;

            if (keySystem != null)
                keySystem.KeyEvent += KeyHandlerMethod;
        }

        public bool Enable(decimal creditLimit)
        {
            Log.DebugFormat("Enable cashin {0}", creditLimit);
            MaxCredit = (creditLimit >= 0) ? creditLimit : 0;
            return validator != null && validator.Enable(creditLimit);
        }

        public bool Disable ()
        {
            Log.Debug("Disable cashin");

            return validator != null && validator.Disable();
        }

        public decimal GetCredit ()
        {
            if (validator == null)
            {
                return 0;
            }
            return validator.GetCredit();
        }

        public bool ResetCredit ()
        {
            MaxCredit = 0;
            return validator != null && validator.ResetCredit();
        }

        public bool IsEnabled()
        {
            return validator != null && validator.IsEnabled();
        }

        public bool SetEnabledChannels(decimal actcredit)
        {
            return validator != null && validator.SetEnabledChannels(actcredit);
        }

        public void SetCredit(decimal credit)
        {
            Log.DebugFormat("Set Credit cashin {0}", credit);
            if (validator != null)
                validator.SetCredit(credit);
        }

        public bool IsDataSetValid()
        {
            return validator != null && validator.IsDataSetValid();
        }

        public List<DeviceInfo> GetDeviceInventory ()
        {
            if (validator != null)
            {
                return validator.GetDeviceInventory();
            }
            return null;
        }

        public List<DeviceInfo> GetShortDeviceInventory ()
        {
            if (validator != null)
            {
                return validator.GetShortDeviceInventory();
            }
            return null;
        }
        public bool CheckBillValidator ()
        {
            return validator != null && validator.CheckBillValidator();
        }

        public bool CheckCoinAcceptor ()
        {
            return validator != null && validator.CheckCoinAcceptor();
        }
    }
}
