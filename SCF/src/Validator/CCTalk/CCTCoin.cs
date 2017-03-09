using System;
using System.Text;
using System.Threading;
using NLogger;
using Nbt.Services.Scf.CashIn.Validator;
using Preferences.Services.Preference;
using System.Collections.Generic;
using SerialPortManager;

namespace Nbt.Services.Scf.CashIn.Validator.CCTalk
{

    public sealed class CCTCoin : AValidator
    {
        //private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(CCTCoin).Name);
        private byte _address;
        private decimal[] _channelValues;
        private decimal _credit;
        private bool _enabled;
        private decimal _maxLimit;
        private Thread _pollAcceptor;
        private bool _useCRCChecksum;
        private int CHANNEL_COUNT;
        private object enableLock;
        private IPrefSupplier _pref;
        private string _prefKey;
        private byte[] _encrytionKey;
        private byte _defaultAddress;
        private volatile bool _datasetIsReadout = false;

        private static volatile bool _checkDeviceStatus = false;
       
        private static SerialPortManager.SerialPortManager spm = SerialPortManager.SerialPortManager.Instance;
        private static bool create_spacial_coin_class= false;
        private volatile bool strange_device = false;

        private bool CheckIfCashDevicesUseDiffPorts ()
        {
            bool result = false;
            bool CoinAcceptorFound = false;
            bool BillAcceptorFound = false;
            string CoinAcceptorPort = null;
            string BillAcceptorPort = null;

            try
            {
                List<SerialPortManager.CommunicationResource> sp = spm.GetSafeSerialPortsMap();
                foreach (CommunicationResource cr in sp)
                {
                    if (cr.ProtocolType == DeviceProtocol.PROTOCOL_CCTALK)
                    {
                        if (cr.PortType == ResourceType.BILL_VALIDATOR_SERIAL_PORT)
                        {
                            BillAcceptorFound = true;
                            BillAcceptorPort = cr.PortName;
                        }
                        else if (cr.PortType == ResourceType.COIN_ACCEPTOR_SERIAL_PORT)
                        {
                            CoinAcceptorFound = true;
                            CoinAcceptorPort = cr.PortName;
                        }
                    }
                }

                if (BillAcceptorFound && CoinAcceptorFound)
                {
                    if (BillAcceptorPort != CoinAcceptorPort)
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

      
        public CCTCoin(IPrefSupplier pref, string prefKey, string mode)
        {
            ValFound = false;
            Log.Debug("Creating CCTalk-CoinAcceptor...");
            _pref = pref;
            _prefKey = prefKey;
            _defaultAddress = 2;
            CHANNEL_COUNT = 16;
            _encrytionKey = new byte[6];
            enableLock = new Object();
            bool flag = false;
            _channelValues = new decimal[CHANNEL_COUNT];

           create_spacial_coin_class = CheckIfCashDevicesUseDiffPorts ();
           int adr = Convert.ToInt32(pref.GetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorAddress));

           if (adr == 0)
           {
               _address = _defaultAddress;
           }
           else
           {
               _address = (byte)adr;
           }


           if (create_spacial_coin_class)
           {
               CCTCommunicationA.SetMode(mode);


               if (CCTCommunicationA.Instance.CheckAcceptorPresent("Coin Acceptor", _address, ref _useCRCChecksum, _encrytionKey))
               {
                   flag = true;
               }
               else if (CCTCommunicationA.Instance.FindAcceptor("Coin Acceptor", ref _address, ref _useCRCChecksum, _encrytionKey))
               {
                   pref.SetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorAddress, Convert.ToInt32(_address));
                   flag = true;
               }
           }
           else
           {
               CCTCommunication.SetMode(mode);
               

               if (CCTCommunication.Instance.CheckAcceptorPresent("Coin Acceptor", _address, ref _useCRCChecksum, _encrytionKey))
               {
                   flag = true;
               }
               else if (CCTCommunication.Instance.FindAcceptor("Coin Acceptor", ref _address, ref _useCRCChecksum, _encrytionKey))
               {
                   pref.SetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorAddress, Convert.ToInt32(_address));
                   flag = true;
               }
           }
            if (flag)
            {
               
                ResetCoinAccseptor();
                base.Disable();
                GetChannelValues();
                _pollAcceptor = new Thread (new ThreadStart(readData));
                _pollAcceptor.Start();
                ValFound = true;
                return;
            }
            Log.Debug("No CCTalk CoinAcceptor found on address:" + _address);
            // throw new ApplicationException("no CCTalk-CoinAcceptor found");
        }

        private bool ResetCoinAccseptor()
        {
            bool result = false;

            bool flag;
            byte[] bArr3;
            byte[] bArr1 = new byte[1];

            string s = null;

            try
            {
                List<DeviceInfo> list = GetShortDeviceInventory();
                {
                    if (list != null)
                    {

                        foreach (DeviceInfo di in list)
                        {
                            if (di.device_type == Nbt.Services.Scf.CashIn.DeviceType.COIN_ACCEPTOR)
                            {
                                if (di.device_model == "EMP")
                                {
                                    strange_device = true;
                                }
                                else
                                {
                                    strange_device = false;
                                }
                                break;
                            }
                        }

                    }

                }
                if (create_spacial_coin_class)
                {
                    if (CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 1, false, bArr1, out flag, out bArr3, out s, false, _useCRCChecksum, _encrytionKey))
                    {
                        result = true;
                    }
                }
                else
                {
                    if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 1, false, bArr1, out flag, out bArr3, out s, false, _useCRCChecksum, _encrytionKey))
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

        private void GetChannelValues()
        {
            bool flag;
            bool result = true;
            byte[] bArr2;

            byte[] bArr3 = new byte[1];
            byte[] bArr1 = bArr3;
            string s = null;
            string[] sArr1 = new string[(CHANNEL_COUNT + 1)];
            ASCIIEncoding asciiencoding = new ASCIIEncoding();

            //from prefs
            //int multiplicator = Convert.ToInt32(_pref.GetIntegerEntry(_prefKey + CashInSettings.Default.PrefValidatorMultiplicator));

            //if (multiplicator == 0)  //0 would cause error
            int multiplicator = 100;  // has to be 100, because CCTCoin sets 50cent to 50, 1euro to 100 ...

            for (byte b = 1; b <= (byte)CHANNEL_COUNT; b++)
            {
                bArr1[0] = b;
                if (create_spacial_coin_class)
                {
                    result &= CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 184, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                }
                else
                {
                    result &= CCTCommunication.Instance.SendCCTalkCommand(1, _address, 184, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                }
                if (result)
                {
                    if (!flag)
                    {
                        try
                        {
                            sArr1[b] = asciiencoding.GetString(bArr2);
                            _channelValues[b - 1] = Convert.ToDecimal(sArr1[b].Substring(2, 3)) / multiplicator;
                        }
                        catch 
                        {
                            _channelValues[b - 1] = 0;
                        }
                    }
                    else
                    {
                        _channelValues[b - 1] = 0;
                    }
                    Log.Debug("CCTalk CoinAcceptor GetChannelValues    " + b.ToString().PadLeft(2) + " " + sArr1[b] + " " + _channelValues[b - 1].ToString().PadLeft(5));
                }
                else
                {
                    Log.Debug("CCTalk CoinAcceptor GetChannelValues failed    " + s);
                    break;
                }
            }
            _datasetIsReadout = result;
        }

        private void handleCoinAcceptorEventCodes (byte b1, byte b2)
        {
            if (b1 != 0)
            {
                if (!this.IsEnabled())
                {
                    OnCashInAfterDisable(new CashInEventArgs(_channelValues[b1 - 1], true));
                    Log.Debug(String.Concat("Coin [", _channelValues[b1 - 1], "] accepted after disable!"));
                    return;
                }
                _credit += _channelValues[b1 - 1];
                OnCashIn(new CashInEventArgs(_channelValues[b1 - 1], true));
                Log.Debug(String.Concat("Coin [", _channelValues[b1 - 1], "] accepted!"));
                if (_maxLimit - _channelValues[b1 - 1] < _channelValues[0])
                {
                    OnCashLimitExceeded(new ValidatorEventArgs<string>("Cash Limit is reached. Coin Acceptor will be disabled!"));
                }
            }
            else
            {
                if (b2 == 2)
                {
                    OnCashLimitExceeded (new ValidatorEventArgs<string>("Cash Limit is exedeed. Coin was rejected!"));
                    Log.Debug("Bill Validator ccTalk: Cash Limit is exedeed. Cash was rejected!");
                }
                else if (_enabled && strange_device)
                {
                    ResetCoinAccseptor ();
                    Enable (_maxLimit);                 
                }
                
            }
        }

        private void loadChannelSettings()
        {
            //from prefs
            //int multiplicator = Convert.ToInt32(_pref.GetIntegerEntry(_prefKey + CashInSettings.Default.PrefValidatorMultiplicator));

            //if (multiplicator == 0)  //0 would cause error
            int multiplicator = 100; // has to be 100, because CCTCoin sets 50cent to 50, 1euro to 100 ...


            for (int i = 0; _channelValues != null && i < _channelValues.Length; i++)
            {
                _channelValues[i] = (decimal) ((_pref.GetDoubleEntry(_prefKey + CashInSettings.Default.PrefValidatorChannel + String.Format("{0:##00}", i + 1)) ?? 0) / multiplicator);
                Log.Info("CCTalk Coin Channel " + (i + 1) + ": [" + (_channelValues[i] == 0 ? "not set" : _channelValues[i].ToString()) + "]");
            }

            Log.Info("CCTalk CoinAcceptor Multiplicator == " + multiplicator);
        }

        private void readData()
        {
            bool flag;
            byte[] bArr2;

            byte[] bArr3 = new byte[1];
            byte[] bArr1 = bArr3;
            string s = null;
            int i1 = -1, i2 = 0;


            Log.Debug("CCTalk CoinAcceptor read started");

            while (true)
            {
                try
                {
                    if (!_datasetIsReadout)
                    {
                        GetChannelValues();
                        Thread.Sleep(150);
                        continue;
                    }
                label_2:
                    Thread.Sleep(150);
                    
                    if (create_spacial_coin_class)
                    {
                        if (CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 229, true, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                        {
                            if (bArr2[0] == 0)
                                i1 = 0;    // pwr up
                            if (i1 == -1)
                                i1 = bArr2[0]; // first syncro
                            if (i1 > bArr2[0])
                                i2 = 255 - i1 + bArr2[0];
                            else
                                i2 = bArr2[0] - i1;
                            i1 = bArr2[0];
                            if (i2 > 5)
                                i2 = 5;
                            for (int i3 = 0; i3 < i2; i3++)
                            {
                                handleCoinAcceptorEventCodes(bArr2[1 + (i3 * 2)], bArr2[2 + (i3 * 2)]);
                            }
                        }
                    }
                    else
                    {
                        if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 229, true, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                        {
                            if (bArr2[0] == 0)
                                i1 = 0;
                            if (i1 == -1)
                                i1 = bArr2[0];
                            if (i1 > bArr2[0])
                                i2 = 255 - i1 + bArr2[0];
                            else
                                i2 = bArr2[0] - i1;
                            i1 = bArr2[0];
                            if (i2 > 5)
                                i2 = 5;
                            for (int i3 = 0; i3 < i2; i3++)
                            {
                                handleCoinAcceptorEventCodes(bArr2[1 + (i3 * 2)], bArr2[2 + (i3 * 2)]);
                            }
                        }
                    }
                    if ((s != null) && (s.Length > 0))
                        Log.Debug("CCTalk CoinAcceptor Error " + s);
                  
                    goto label_2;
                }
                catch (Exception Ex)
                {
                    Log.Error("In CCTCoin readData.Closing CoinAcceptor", Ex);
                    Disable();
                    return;
                }
            }
        }

        private bool setChannels(decimal ActValue)
        {
            bool flag1;
            byte[] bArr2;

            byte[] bArr1 = new byte[2];
            string s = null;
            byte b1 = 0, b2 = 0, b3 = 1;
            for (int i = 0; i < _channelValues.Length; i++)
            {
                if (_maxLimit == 0)
                {
                    Log.Error("CoinAcceptor Limit set to 0. All Channels not set to value 0.0.");
                }

                bool flag3 = ((ActValue + _channelValues[i]) <= _maxLimit) && (_channelValues[i] != 0m/*> 0.05m*/);
                if (flag3)
                {
                    int shift = 0;
                    if (i < 8)
                    {
                        shift = (i & 31);
                        b1 |= (byte) (b3 << shift);
                        continue;
                    }
                    //b2 |= b3 << ((i - 8) & 31);
                    shift = ((i - 8) & 31);
                    b2 |= (byte)(b3 << shift);
                }
            }
            bArr1[0] = b1;
            bArr1[1] = b2;
            if (create_spacial_coin_class)
                return CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 231, false, bArr1, out flag1, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
            return CCTCommunication.Instance.SendCCTalkCommand(1, _address, 231, false, bArr1, out flag1, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
        }

        public override bool Disable()
        {
            bool flag;
            byte[] bArr3;

            byte[] bArr4 = new byte[2];
            byte[] bArr1 = bArr4;
            byte[] bArr5 = new byte[1];
            byte[] bArr2 = bArr5;
            string s = null;
            base.Disable();
            _credit = 0;

            // _logger.Debug("CCTalk CoinAcceptor closed.");
            lock (enableLock)
            {
                if (create_spacial_coin_class)
                {
                    if (CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 231, false, bArr1, out flag, out bArr3, out s, false, _useCRCChecksum, _encrytionKey)
                    && CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 228, false, bArr2, out flag, out bArr3, out s, false, _useCRCChecksum, _encrytionKey))
                    {
                        _enabled = false;
                        return true;
                    }
                    else
                    {
                        Log.Debug("CCTalk CoinAcceptor Disable failed    " + s);
                        return false;
                    }
                }

                if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 231, false, bArr1, out flag, out bArr3, out s, false, _useCRCChecksum, _encrytionKey)
                    && CCTCommunication.Instance.SendCCTalkCommand(1, _address, 228, false, bArr2, out flag, out bArr3, out s, false, _useCRCChecksum, _encrytionKey))
                {
                    _enabled = false;
                    return true;
                }
                else
                {
                    Log.Debug("CCTalk CoinAcceptor Disable failed    " + s);
                    return false;
                }
            }
        }

        public override bool Enable(decimal MaxLimit)
        {
            bool flag;
            byte[] bArr2;

            byte[] bArr3 = new byte[] { 1 };
            byte[] bArr1 = bArr3;
            string s = null;
            //_credit = 0;

            if (!_datasetIsReadout)
            {
                if (_enabled)
                {
                    Disable();
                }
                return false;
            }
            lock (enableLock)
            {
                base.Enable(MaxLimit);
                _maxLimit = MaxLimit < 0 ? 0 : MaxLimit;

                if (create_spacial_coin_class)
                {
                    if (CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 228, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                    {
                        _enabled = true;
                    }
                    else
                    {
                        Log.Debug("CCTalk CoinAcceptor MasterEnable failed    " + s);
                        _enabled = false;
                    }
                }
                else
                {
                    if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 228, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                    {
                        _enabled = true;
                    }
                    else
                    {
                        Log.Debug("CCTalk CoinAcceptor MasterEnable failed    " + s);
                        _enabled = false;
                    }
                }
                if (setChannels(0))
                {
                    _enabled = true;
                    setChannels(this._credit);
                }
                else
                {
                    Log.Debug("CCTalk CoinAcceptor Channel Enable failed    " + s);
                    _enabled = false;
                }
                if (_pollAcceptor != null && _pollAcceptor.ThreadState == ThreadState.Unstarted)
                    _pollAcceptor.Start();
            }
            // _logger.Debug("CCTalk CoinAcceptor opened.");
            return _enabled;
        }

       

        public override List<DeviceInfo> GetShortDeviceInventory ()
        {
            bool flag;
            byte[] bArr2;
            byte[] bArr1 = new byte[0];
            string s = null;
            bool result = false;


            List<DeviceInfo> list = new List<DeviceInfo>(1);
            DeviceInfo di = new DeviceInfo();
            di.device_type = DeviceType.UNKNOWN;

            try
            {
                if (create_spacial_coin_class)
                {
                    result = CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 244, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                }
                else
                {
                    result = CCTCommunication.Instance.SendCCTalkCommand (1, _address, 244, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                }
                if (result)
                {
                    di.device_model = System.Text.Encoding.Default.GetString(bArr2);
                    di.device_type = DeviceType.COIN_ACCEPTOR;

                   
                    list.Add(di);
                }

            }
            catch
            {
            }

            return list;
        }

        public override List<DeviceInfo> GetDeviceInventory()
        {
            bool flag;
            byte[] bArr2;
            byte[] bArr1 = new byte[0];
            string s = null;
            bool result = false;


            List<DeviceInfo> list = new List<DeviceInfo>(1);
            DeviceInfo di = new DeviceInfo();
            di.device_type = DeviceType.UNKNOWN;

            try
            {
                if (create_spacial_coin_class)
                {
                    if (CCTCommunicationA.Instance.SendCCTalkCommand (1, _address, 244, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                    {
                        di.device_model = System.Text.Encoding.Default.GetString(bArr2);
                        if (CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 246, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                        {
                            di.device_producer = System.Text.Encoding.Default.GetString(bArr2);
                            if (CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 241, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                            {
                                di.device_firmware_version = System.Text.Encoding.Default.GetString(bArr2);
                                result = CCTCommunicationA.Instance.SendCCTalkCommand (1, _address, 242, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                                if (result && bArr2.Length > 2)
                                {
                                    di.device_serial_number = (bArr2[2] * 65536 + bArr2[1] * 256 + bArr2[0]).ToString();
                                   
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 244, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                    {
                        di.device_model = System.Text.Encoding.Default.GetString (bArr2);
                        if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 246, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                        {
                            di.device_producer = System.Text.Encoding.Default.GetString(bArr2);
                            if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 241, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                            {
                                di.device_firmware_version = System.Text.Encoding.Default.GetString(bArr2);
                                result = CCTCommunication.Instance.SendCCTalkCommand(1, _address, 242, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                                if (result && bArr2.Length > 2)
                                {
                                    di.device_serial_number = (bArr2[2] * 65536 + bArr2[1] * 256 + bArr2[0]).ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }

            if (result)
            {
                di.device_type = DeviceType.COIN_ACCEPTOR;
                list.Add(di);
            }

            return list;
        }

    /*    private bool GetDeviceInventoryA (int stage, ref DeviceInventory invent)
        {       
            bool flag;
            byte[] bArr2;
            byte[] bArr1 = new byte[0];
            string s = null;
            bool result = false;  
           // System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch ();
            
           // sw.Start();
            try
            {
                if (create_spacial_coin_class)
                {
                    switch (stage)
                    {
                        case 0:
                            // Request Manufacturer ID
                            result = CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 246, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            invent.ManufacturerID = System.Text.Encoding.Default.GetString(bArr2);
                            break;
                        case 1:
                            // Request Equipment category ID
                            result = CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 245, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            invent.EquipmentCategoryID = System.Text.Encoding.Default.GetString(bArr2);
                            break;
                        case 2:
                            // Request product code
                            result = CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 244, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            invent.ProductCode = System.Text.Encoding.Default.GetString(bArr2);
                            break;
                        case 3:
                            // Request sw version
                            result = CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 241, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            invent.SW_Version = System.Text.Encoding.Default.GetString(bArr2);
                            break;
                        case 4:
                            // Request Serial Number
                            result = CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 242, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            if (result && bArr2.Length > 2)
                            {
                                invent.SerialNumber = (bArr2[2] * 65536 + bArr2[1] * 256 + bArr2[0]).ToString();
                                invent.IsInitialized = true;
                                Inventory.Clone(invent);
                            }
                            else
                            {
                                result = false;
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                     switch (stage)
                    {
                        case 0:
                            // Request Manufacturer ID
                            result = CCTCommunication.Instance.SendCCTalkCommand(1, _address, 246, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            invent.ManufacturerID = System.Text.Encoding.Default.GetString(bArr2);
                            break;
                        case 1:
                            // Request Equipment category ID
                            result = CCTCommunication.Instance.SendCCTalkCommand(1, _address, 245, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            invent.EquipmentCategoryID = System.Text.Encoding.Default.GetString(bArr2);
                            break;
                        case 2:
                            // Request product code
                            result = CCTCommunication.Instance.SendCCTalkCommand(1, _address, 244, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            invent.ProductCode = System.Text.Encoding.Default.GetString(bArr2);
                            break;
                        case 3:
                            // Request sw version
                            result = CCTCommunication.Instance.SendCCTalkCommand(1, _address, 241, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            invent.SW_Version = System.Text.Encoding.Default.GetString(bArr2);
                            break;
                        case 4:
                            // Request Serial Number
                            result = CCTCommunication.Instance.SendCCTalkCommand(1, _address, 242, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey);
                            if (result && bArr2.Length > 2)
                            {
                                invent.SerialNumber = (bArr2[2] * 65536 + bArr2[1] * 256 + bArr2[0]).ToString();
                                invent.IsInitialized = true;
                                Inventory.Clone(invent);
                            }
                            else
                            {
                                result = false;
                            }
                            break;

                        default:
                            break;
                    }
                }
               
                if (!result)
                {
                    Inventory.DropData();
                }                            
            }
            catch
            {
                Inventory.DropData();
            }
           // sw.Stop();

          //  if (sw.ElapsedMilliseconds > 300)
          //  {
          //      int i = 0;
          //  }
          //  return result;
        }
        */

        public override decimal GetCredit()
        {
            base.GetCredit();
            return _credit;
        }

        public override void SetCredit(decimal credit)
        {
            base.SetCredit(credit);
            _credit = credit;
        }

        public override bool IsEnabled()
        {
            bool flag;

            lock (enableLock)
            {
                base.IsEnabled();
                flag = _enabled;
            }
            return flag;
        }

        public override bool ResetCredit()
        {
            base.ResetCredit();
            _credit = 0;
            return true;
        }

        public override bool SetEnabledChannels(decimal ActValue)
        {
            lock (enableLock)
            {
                base.SetEnabledChannels(ActValue);
                return setChannels(ActValue);
            }
        }

        public override bool IsDataSetValid()
        {
            return true;
        }

        public override bool CheckCoinAcceptor ()
        {
            bool flag;
            byte[] bArr2;
            byte[] bArr1 = new byte[0];
            string s = null;

            if (create_spacial_coin_class)
            {
                if (CCTCommunicationA.Instance.SendCCTalkCommand(1, _address, 246, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
                {
                    return true;
                }
                return false;
            }

            if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 246, false, bArr1, out flag, out bArr2, out s, false, _useCRCChecksum, _encrytionKey))
            {
                return true;
            }

            return false;
        }

    } // class CCTCoin

}

