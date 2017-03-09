using System;
using System.Text;
using System.Threading;
using NLogger;
using Nbt.Services.Scf.CashIn.Validator;
using Preferences.Services.Preference;
using SportRadar.Common.Logs;
using System.Collections.Generic;

namespace Nbt.Services.Scf.CashIn.Validator.CCTalk
{
   
    public sealed class CCTNote : AValidator
    {
        private static ILog Log = LogFactory.CreateLog(typeof(CCTNote));
        private byte _address;
        private int _channelCount;
        private decimal[] _channelValues;
        private decimal _credit;
        private byte _defaultAddress;
        private bool _enabled;
        private bool _encrypted;
        private byte[] _encrytionKey;
        private decimal _maxLimit;
        private Thread _pollAcceptor;
        private bool _useCRCChecksum;
        private object enableLock;
        private IPrefSupplier _pref;
        private string _prefKey;
        private static bool _datasetIsValid = true;
        private volatile bool _datasetIsReadout = false;
        // private int timeOut;

        private static volatile bool _checkDeviceStatus = false;
      

        public CCTNote(IPrefSupplier pref, string prefKey, string mode)
        {
            ValFound = false;
            Log.Debug("Creating CCTalk-NoteAcceptor...");
            _pref = pref;
            _prefKey = prefKey;
            _defaultAddress = 40;
            _encrytionKey = new byte[6];
            _encrypted = true;
            _channelCount = 16;
            enableLock = new Object();
            string s = "123456";
            string defaultKey = "123456";
            bool flag = false;
            // timeOut = 1000 * 60 * 5;
            ASCIIEncoding asciiencoding = new ASCIIEncoding();
            _channelValues = new decimal[_channelCount];
            CCTCommunication.SetMode(mode);

            int adr = Convert.ToInt32(pref.GetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorAddress));
            
            if (adr == 0)
            {
                _address = _defaultAddress;
            }
            else {
                _address = (byte)adr;
            }

            s = Convert.ToString(pref.GetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorEncryptionKey));

            if(s != null && s.Length<=0) {
                s = defaultKey;
            }
            
            if (s.Length != 6)
            {
                Log.Debug("No Encryption set. Try to find Noteacceptor.");
                _encrypted = false;
                if (CCTCommunication.Instance.CheckAcceptorPresent("Bill Validator", _address, ref _useCRCChecksum, _encrytionKey))
                {
                    flag = true;
                }
                else if (CCTCommunication.Instance.FindAcceptor("Bill Validator", ref _address, ref _useCRCChecksum, _encrytionKey))
                {
                    pref.SetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorAddress, Convert.ToInt32(_address));
                    flag = true;                    
                }
            }
            else
            {
                Log.Debug("Encryptionkey set to:" + s + ". Try to find Noteacceptor.");
                _encrypted = true;
                _encrytionKey = asciiencoding.GetBytes(s);
                for (int i1 = 0; i1 < _encrytionKey.Length; i1++)
                {
                    _encrytionKey[i1] = (byte)(_encrytionKey[i1] - 48);
                }
                CCTCommunication.Instance.secArray = _encrytionKey;
                //if (CCTCommunication.Instance.CheckAcceptorPresentEncrypted("Bill Validator", _address, ref _encrypted, ref _useCRCChecksum))
                if (CCTCommunication.Instance.CheckAcceptorPresentEncrypted("Bill Validator", _address, ref _encrypted, ref _useCRCChecksum, _encrytionKey))
                {
                    flag = true;
                }
                else
                {
                    _address = _defaultAddress;
                    if (CCTCommunication.Instance.FindAcceptor("Bill Validator", ref _address, ref _useCRCChecksum, _encrytionKey))
                    {
                        pref.SetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorAddress, Convert.ToInt32(_address));
                        flag = true;
                    }
                    if (CCTCommunication.Instance.CheckAcceptorPresentEncrypted("Bill Validator", _address, ref _encrypted, ref _useCRCChecksum, _encrytionKey))
                    {
                        pref.SetIntegerEntry(prefKey + CashInSettings.Default.PrefValidatorAddress, Convert.ToInt32(_address));
                        flag = true;
                    }
                }
            }

            if (flag)
            {
              
               base.Disable();
#if BETCENTER  
                
#else
               if (!CheckCurrencyDataSet("EUR05"))
               {
                   _datasetIsValid = false;
                   Log.Error("DATASET is FALSE",new Exception(""));
                   //return;
               }
 #endif
                SetBillOperationMode();
               /* int useAcceptorValues = Convert.ToInt32(pref.GetIntegerEntry(prefKey + CashInSettings.Default.UseAcceptorValues));
                if (useAcceptorValues == 1)
                    GetChannelValues();
                else
                    loadChannelSettings();*/
                GetChannelValues();
                _pollAcceptor = new Thread(new ThreadStart(readData));
                _pollAcceptor.Start();
                base.Enable(0);
                Thread.Sleep(800);
                base.Disable();
                ValFound = true;
                return;
            }
            Log.Error("No CCTalk Noteacceptor found on address:" + _address,new Exception());
            // throw new ApplicationException("No CCTalk-NoteAcceptor found");
        }

        private void GetChannelValues()
        {
            bool flag;
            bool result = true;
            byte[] bArr2;

            byte[] bArr3 = new byte[1];
            byte[] bArr1 = bArr3;
            string s = null;
            string[] sArr1 = new string[(_channelCount + 1)];
            ASCIIEncoding asciiencoding = new ASCIIEncoding();
            for (byte b = 1; b <= (byte)_channelCount; b++)
            {
                bArr1[0] = b;
                result &= CCTCommunication.Instance.SendCCTalkCommand(1, _address, 157, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey);
               
                if (result)
                {
                    //if (!flag)
                    //{
                        try
                        {
                            sArr1[b] = asciiencoding.GetString(bArr2);
                            _channelValues[b - 1] = Convert.ToDecimal(sArr1[b].Substring(2, 4));
                        }
                        catch 
                        {
                            _channelValues[b - 1] = 0;
                        }
                    //}
                    //else
                    //{
                    //    _channelValues[b - 1] = 0.0;
                    //}
                        Log.Debug("CCTalk NoteAcceptor GetChannelValues    " + b.ToString().PadLeft(2) + " " + sArr1[b] + " " + _channelValues[b - 1].ToString().PadLeft(5));
                }
                else
                {
                    Log.Debug("CCTalk NoteAcceptor GetChannelValues failed    " + s);
                    break;
                }
            }
            _datasetIsReadout = result;
        }

        private void handleNoteAcceptorEventCodes(byte b1, byte b2)
        {
            if ((b1 != 0) && (b2 == 0))
            {
                if (!this.IsEnabled())
                {
                    OnCashInAfterDisable(new CashInEventArgs(_channelValues[b1 - 1], false));
                    Log.Debug(String.Concat("Note [", _channelValues[b1 - 1], "] accepted after disable!"));
                }
                else
                {
                    _credit += _channelValues[b1 - 1];
                    OnCashIn(new CashInEventArgs(_channelValues[b1 - 1], false));
                    Log.Debug(String.Concat("Note [", _channelValues[b1 - 1], "] accepted!"));
                }
                if (_maxLimit - _channelValues [b1 - 1] < _channelValues [0])
                {
                    OnCashLimitExceeded(new ValidatorEventArgs<string>("Cash Limit is reached. Bill Validator will be disabled!"));
                }
            }
            else if ((b1 == 0) && (b2 == 4))
            {
                OnCashLimitExceeded (new ValidatorEventArgs<string>("Cash Limit is exedeed. Cash was rejected!"));
                Log.Debug("Bill Validator ccTalk: Cash Limit is exedeed. Cash was rejected!");
            }
        }

        private void loadChannelSettings()
        {

            for (int i = 0; _channelValues != null && i < _channelValues.Length; i++)
            {
                _channelValues[i] = (decimal) (_pref.GetDoubleEntry(_prefKey + CashInSettings.Default.PrefValidatorChannel + String.Format("{0:##00}", i + 1)) ?? 0);
                Log.Debug("CCTalk Note Channel " + (i + 1) + ": [" + (_channelValues[i] == 0 ? "not set" : _channelValues[i].ToString()) + "]");
            }
        }

        private void readData()
        {
            bool flag;
            byte[] bArr2;

            byte[] bArr3 = new byte[0];
            byte[] bArr1 = null;
            string s = null;
            int i1 = -1, i2 = 0;
            Log.Debug("NoteAcceptor READ started");
            while (true)
            {
                try
                {
                label_2:
                  /*  if (_checkDeviceStatus)
                    {   
                        GetDeviceInventoryA();
                        _checkDeviceStatus = false;
                    }*/
                if (!_datasetIsReadout)
                {
                    GetChannelValues ();
                    continue;
                }

                //if (CCTCommunication.Instance.SendCCTalkCommand(_address, 159, bArr1, timeOut, out bArr2, CCTMessageType.EncryptedCRC16, _encrytionKey))
                // if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 229, true, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 159, true, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
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
                        handleNoteAcceptorEventCodes(bArr2[1 + (i3 * 2)], bArr2[2 + (i3 * 2)]);
                    }
                }
                if (!string.IsNullOrEmpty(s))
                    Log.Debug("CCTalk NoteAcceptor Error " + s);
                Thread.Sleep(150);
                goto label_2;
                }
                catch (Exception Ex)
                {
                    Log.Error("In CCTNote readData.Closing CCTNote", Ex);
                    Disable();
                    return;
                }
            }
        }

        //public bool SetBillOperationMode(bool stacker, bool escrow)
        //{
        //    byte[] bArr1;

        //    byte b = 0;
        //    if (stacker)
        //        b |= 1;
        //    if (escrow)
        //        b |= 2;
        //    byte[] bArr2 = new byte[] { b };
        //    return ExecuteCommand(153, bArr2, out bArr1, 1000);
        //}

        private void SetBillOperationMode()
        {
            bool stacker = false;
            bool escrow = false;

            byte b = 0;
            if (stacker)
                b |= 1;
            if (escrow)
                b |= 2;
            byte[] bArr2 = new byte[] { b };

            bool flag;
            byte[] bArr1;

            //byte[] bArr3 = new byte[1];
            //byte[] bArr2 = bArr3;
            string s = null;
            // CCTCommunication.Instance.SendCCTalkCommand(1, _address, 159, false, bArr2, out flag, out bArr1, out s, _encrypted, _useCRCChecksum, _encrytionKey);
            CCTCommunication.Instance.SendCCTalkCommand(1, _address, 153, false, bArr2, out flag, out bArr1, out s, _encrypted, _useCRCChecksum, _encrytionKey);
            
            // string s = null; for commnted implmentation
            // CCTCommunication.Instance.SendCCTalkCommand(_address, 159, bArr1, timeOut, out bArr2, CCTMessageType.EncryptedCRC16, _encrytionKey);
            if (!string.IsNullOrEmpty(s))
            {
                Log.Debug("CCTalk NoteAcceptor Error: " + s);
                return;
            }
            Log.Debug("CCTalk NoteAcceptor Escrow-mode disabled.");
        }

        private bool setChannels (decimal ActValue)
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
                    Log.Error("NoteAcceptor Limit set to 0. All Channels not set to value 0.0.",new Exception());
                }

                bool flag3 = ((ActValue + _channelValues[i]) <= _maxLimit) && (_channelValues[i] != 0);
                if (flag3)
                {
                    int shift = 0;
                    if (i < 8)
                    {
                        shift = (i & 31);
                        b1 |= (byte)(b3 << shift);
                        continue;
                    }
                    //b2 |= b3 << ((i - 8) & 31);
                    shift = ((i - 8) & 31);
                    b2 |= (byte)(b3 << shift);
                }
            }
            bArr1[0] = b1;
            bArr1[1] = b2;
            // return CCTCommunication.Instance.SendCCTalkCommand(_address, 231, bArr1, timeOut, out bArr2, CCTMessageType.EncryptedCRC16, _encrytionKey);
            return CCTCommunication.Instance.SendCCTalkCommand(1, _address, 231, false, bArr1, out flag1, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey);
        }

        public override bool Disable()
        {
            bool flag;
            byte[] bArr2;

            byte[] bArr3 = new byte[1];
            byte[] bArr1 = bArr3;
            string s = null;
            base.Disable();
            _credit = 0;
            // _logger.Debug("CCTalk NoteAcceptor closed.");
            lock (enableLock)
            {
                if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 228, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                //if (CCTCommunication.Instance.SendCCTalkCommand(_address, 228, bArr1, timeOut, out bArr2, CCTMessageType.EncryptedCRC16, _encrytionKey))
                {
                    _enabled = false;
                    return true;
                }
                else
                {
                    Log.Debug("CCTalk NoteAcceptor Disable failed    " + s);
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
            if (!_datasetIsValid || !_datasetIsReadout)
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
                // if (CCTCommunication.Instance.SendCCTalkCommand(_address, 228, bArr1, timeOut, out bArr2, CCTMessageType.EncryptedCRC16, _encrytionKey))
                if (CCTCommunication.Instance.SendCCTalkCommand(1, _address, 228, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                {
                    _enabled = true;
                }
                else
                {
                    Log.Debug("CCTalk NoteAcceptor MasterEnable failed    " + s);
                    _enabled = false;
                }
                if (setChannels(0))
                {
                    _enabled = true;
                    setChannels(this._credit);
                }
                else
                {
                    Log.Debug("CCTalk NoteAcceptor Channel Enable failed    " + s);
                    _enabled = false;
                }
                if (_enabled)
                {
                    if (_pollAcceptor.ThreadState == ThreadState.Unstarted)
                        _pollAcceptor.Start();
                }
                else
                {
                    Log.Debug("CCTalk NoteAcceptor Channel Enable failed.");
                }
            }
            // _logger.Debug("CCTalk NoteAcceptor opend.");
            return _enabled;
        }

      
        // s.voronin 13.062013
        private bool CheckCurrencyDataSet (string ValidDataSet)
        {
         
            bool flag;
            byte[] bArr2;
            byte[] bArr1 = new byte[0];
            string s = null;
            bool result = false;
            const int ATTEMPT_MAX = 3;
            const string DEFAULT_DATA_SET = "EUR05";
            const string NV_10_DATASET = "EUR02";
            const string NV_10_DATASET_2 = "EUR45";

            System.Diagnostics.Debug.Assert (ValidDataSet != null && ValidDataSet.Length == 5, "The Length of 'ValidDataSet' is not 5 chars!"); 

            for (int attempt = 0; attempt < ATTEMPT_MAX; attempt++)
            {
                try
                {
                  
                    // Request Currensy Revision 
                    if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 145, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                    {
                        string dataset = System.Text.Encoding.Default.GetString(bArr2);

                        if (dataset == null || dataset.Length < 5)
                        {
                            continue;
                        }
                        dataset = dataset.Substring(0, 5);

                        if (dataset == DEFAULT_DATA_SET)
                        {
                            result = true;
                            break;
                        }
                        if (dataset == ValidDataSet)
                        {
                            result = true;
                            break;
                        }
                        if (dataset == NV_10_DATASET) // for Gregor only
                        {
                            result = true;
                            break;
                        }
                        if (dataset == NV_10_DATASET_2) // for Gregor only
                        {
                            result = true;
                            break;
                        }
                    }

                }
                catch
                {
                }
            }
            return result;
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
               
                if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 244, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                {
                    di.device_model = System.Text.Encoding.Default.GetString(bArr2);
                    di.device_type = DeviceType.BILL_VALIDATOR;
                   
                    result = true;
                    list.Add(di);
                }          

            }
            catch
            {
            }

            return list;
        }

        public override List<DeviceInfo> GetDeviceInventory ()
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

                if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 244, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                {
                    di.device_model = System.Text.Encoding.Default.GetString(bArr2);

                    if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 246, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                    {
                        di.device_producer = System.Text.Encoding.Default.GetString(bArr2);

                        if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 241, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                        {
                            di.device_firmware_version = System.Text.Encoding.Default.GetString(bArr2);

                            if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 242, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
                            {
                                if (bArr2.Length > 3)
                                {
                                    di.device_serial_number = (bArr2[1] * 65536 + bArr2[2] * 256 + bArr2[3]).ToString();
                                    di.device_type = DeviceType.BILL_VALIDATOR;
                                    result = true;
                                    list.Add(di);
                                }
                            }
                           
                        }
                    }
                }             

            }
            catch
            {
            }

            return list;
        }


        public bool IsFirmwareUpdateSupported()
        {
            bool flag;
            byte[] bArr2;
            byte[] bArr1 = new byte[0];
            string s = null;

            if ( CCTCommunication.Instance.SendCCTalkCommand (1, _address, 139, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
            {
                return true;
            }

            return false;
        }

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

        public override bool SetEnabledChannels (decimal ActValue)
        {
            lock (enableLock)
            {
                base.SetEnabledChannels(ActValue);
                return setChannels(ActValue);
            }
        }

        public override bool IsDataSetValid()
        {
            return _datasetIsValid;
        }

        public override bool CheckBillValidator ()
        {
            bool flag;
            byte[] bArr2;
            byte[] bArr1 = new byte[0];
            string s = null;

            if (CCTCommunication.Instance.SendCCTalkCommand (1, _address, 246, false, bArr1, out flag, out bArr2, out s, _encrypted, _useCRCChecksum, _encrytionKey))
            {
                return true;
            }

            return false;
        }


    } // class CCTNote

}

