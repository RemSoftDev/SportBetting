using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Nbt.Services.Scf.CashIn.Validator;
using Preferences.Services.Preference;
using SerialPortManager;
using NLogger;


namespace Nbt.Services.Scf.CashIn.Validator.ID003// Nbt.Services.Scf.CashIn.src.Validator.ID003
{
    public class ID003 : AValidator
    {
        private decimal _credit;
        private decimal _maxLimit;
        private byte _escrowData;

        private const int CHANNEL_COUNT =32;
        private decimal[] _channelValues = new decimal [CHANNEL_COUNT];

        private ComPortMT serialPort;
        private static SerialPortManager.SerialPortManager spm = SerialPortManager.SerialPortManager.Instance;

        private static object enableLock = new Object();
        private Thread _pollAcceptor;

        ushort[] crc_table = 
        {
            0x0000,0x1189,0x2312,0x329B,0x4624,0x57AD,0x6536,0x74BF,
            0x8C48,0x9DC1,0xAF5A,0xBED3,0xCA6C,0xDBE5,0xE97E,0xF8F7,
            0x1081,0x0108,0x3393,0x221A,0x56A5,0x472C,0x75B7,0x643E,
            0x9CC9,0x8D40,0xBFDB,0xAE52,0xDAED,0xCB64,0xF9FF,0xE876,
            0x2102,0x308B,0x0210,0x1399,0x6726,0x76AF,0x4434,0x55BD,
            0xAD4A,0xBCC3,0x8E58,0x9FD1,0xEB6E,0xFAE7,0xC87C,0xD9F5,
            0x3183,0x200A,0x1291,0x0318,0x77A7,0x662E,0x54B5,0x453C,
            0xBDCB,0xAC42,0x9ED9,0x8F50,0xFBEF,0xEA66,0xD8FD,0xC974,
            0x4204,0x538D,0x6116,0x709F,0x0420,0x15A9,0x2732,0x36BB,
            0xCE4C,0xDFC5,0xED5E,0xFCD7,0x8868,0x99E1,0xAB7A,0xBAF3,
            0x5285,0x430C,0x7197,0x601E,0x14A1,0x0528,0x37B3,0x263A,
            0xDECD,0xCF44,0xFDDF,0xEC56,0x98E9,0x8960,0xBBFB,0xAA72,
            0x6306,0x728F,0x4014,0x519D,0x2522,0x34AB,0x0630,0x17B9,
            0xEF4E,0xFEC7,0xCC5C,0xDDD5,0xA96A,0xB8E3,0x8A78,0x9BF1,
            0x7387,0x620E,0x5095,0x411C,0x35A3,0x242A,0x16B1,0x0738,
            0xFFCF,0xEE46,0xDCDD,0xCD54,0xB9EB,0xA862,0x9AF9,0x8B70,
            0x8408,0x9581,0xA71A,0xB693,0xC22C,0xD3A5,0xE13E,0xF0B7,
            0x0840,0x19C9,0x2B52,0x3ADB,0x4E64,0x5FED,0x6D76,0x7CFF,
            0x9489,0x8500,0xB79B,0xA612,0xD2AD,0xC324,0xF1BF,0xE036,
            0x18C1,0x0948,0x3BD3,0x2A5A,0x5EE5,0x4F6C,0x7DF7,0x6C7E,
            0xA50A,0xB483,0x8618,0x9791,0xE32E,0xF2A7,0xC03C,0xD1B5,
            0x2942,0x38CB,0x0A50,0x1BD9,0x6F66,0x7EEF,0x4C74,0x5DFD,
            0xB58B,0xA402,0x9699,0x8710,0xF3AF,0xE226,0xD0BD,0xC134,
            0x39C3,0x284A,0x1AD1,0x0B58,0x7FE7,0x6E6E,0x5CF5,0x4D7C,
            0xC60C,0xD785,0xE51E,0xF497,0x8028,0x91A1,0xA33A,0xB2B3,
            0x4A44,0x5BCD,0x6956,0x78DF,0x0C60,0x1DE9,0x2F72,0x3EFB,
            0xD68D,0xC704,0xF59F,0xE416,0x90A9,0x8120,0xB3BB,0xA232,
            0x5AC5,0x4B4C,0x79D7,0x685E,0x1CE1,0x0D68,0x3FF3,0x2E7A,
            0xE70E,0xF687,0xC41C,0xD595,0xA12A,0xB0A3,0x8238,0x93B1,
            0x6B46,0x7ACF,0x4854,0x59DD,0x2D62,0x3CEB,0x0E70,0x1FF9,
            0xF78F,0xE606,0xD49D,0xC514,0xB1AB,0xA022,0x92B9,0x8330,
            0x7BC7,0x6A4E,0x58D5,0x495C,0x3DE3,0x2C6A,0x1EF1,0x0F78
        };

        public ID003 (IPrefSupplier pref, string prefKey, string mode)
        {
            bool result = false;
            List<CommunicationResource> sp = spm.GetSafeSerialPortsMap();

            foreach (CommunicationResource cr in sp)
            {
                if (cr.PortType == ResourceType.BILL_VALIDATOR_SERIAL_PORT &&
                    cr.ProtocolType == DeviceProtocol.PROTOCOL_ID003)
                {
                    serialPort = new ComPortMT (cr.PortName, 9600, System.IO.Ports.Parity.Even);
                    result = serialPort.TryOpen();
                    break;

                }
            }

            if (result)
            {
                result = InitializeBillValidator();
            }

            if (result)
            {
                Disable();
                CreatePollingThread();
            }

            ValFound = result;        
        }

      

        private bool InitializeBillValidator()
        {
            bool result = false;
            const int RETRY_ATTEMPT_MAX = 3;

            for (int retry = 0; retry < RETRY_ATTEMPT_MAX; retry++)
            {
                result = SetDefaultParameters ();
                if (!result)
                {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }
                 result = GetChannelValues();
                 if (!result)
                 {
                     System.Threading.Thread.Sleep(500);
                     continue;
                 }

                break;
            }

            
            return result;
        }

        private void CreatePollingThread ()
        {
            _pollAcceptor = new Thread(new ThreadStart (CheckStatus));
            _pollAcceptor.Start();
            base.Enable(0);
            Thread.Sleep(800);
            base.Disable();
        }

        private void CheckStatus()
        {
            byte[] reply = null;
           
            while (true)
            {
                ExecuteCommand (PackMsg (ID003_Commands.StatusRequest), out reply);
                CheckAbnormalState (reply);
                Thread.Sleep(100);
            }
        }

        private void CheckAbnormalState (byte[] reply)
        {
            if (reply != null && reply.Length > 0)
            {
                switch (reply[0])
                {
                    case  ID003_Status_Response.PowerUp:
                          SetDefaultParameters();
                          Log.Debug("ID003 NoteAcceptor: ABNORMAL DEVICE STATE -> ABNORMAL POWER UP!");
                          break;
                    case ID003_Status_Response.PowerUpBillInAcceptor:
                         SetDefaultParameters();
                         Log.Debug("ID003 NoteAcceptor: ABNORMAL DEVICE STATE -> ABNORMAL POWER UP WITH BILL IN ACCEPTOR!");
                         break;
                    case ID003_Status_Response.PowerUpBillInStacker:
                         SetDefaultParameters();
                         Log.Debug("ID003 NoteAcceptor: ABNORMAL DEVICE STATE -> ABNORMAL POWER UP WITH BILL IN STACKER!");
                         break;
                }
               
            }
        }

        private bool GetChannelValues()
        {
           
            byte[] reply = null;
            int pos = 0;

            if (!ExecuteCommand(PackMsg (ID003_Commands.GetBillTable), out reply))
            {
                return false;
            }

            for (int i = 1; i < reply.Length; i+= 4)
            {
                try
                {
                    _channelValues[pos] = Convert.ToDecimal (reply[i + 2] * Math.Pow (10, reply[i + 3]));
                    Log.Debug("ID003 NoteAcceptor GetChannelValues()    " + pos.ToString().PadLeft(2) + " " + _channelValues[pos].ToString().PadLeft(5));
                }
                catch
                {
                    _channelValues[pos] = 0;
                    Log.Debug("ID003 NoteAcceptor  EXCEPTION IN: GetChannelValues()");
                }

                if (pos++ >= CHANNEL_COUNT)
                {
                    Log.Debug("ID003 NoteAcceptor  ERROR IN: GetChannelValues():  _channelValues[pos] >= CHANNEL_COUNT");
                    return false;
                }
            }
            return true;
        }

        private bool ExecuteCommand (byte[] msg, out byte []rep)
        {
            bool result = false;
            byte cmd = 0;
            rep = null;

            if (serialPort != null)
            {
                AnswerType answer_type = GetAnswerType(msg);
                byte[] reply = serialPort.SendCmd(msg);

                if (reply != null)
                {
                    if (UnpackMsg (reply, ref cmd))
                    {
                        result = true;
                        try
                        {
                            rep = new byte[reply.Length - 2 - 2];
                            Array.Copy(reply, 2, rep, 0, rep.Length);
                        }
                        catch
                        {
                        }

                        switch (answer_type)
                        {
                            case AnswerType.ACK:
                                if (cmd != ID003_Constants.ACK)
                                {
                                    return false;
                                }
                                break;
                            case AnswerType.Echo:
                                if (!ByteArrayCompare(msg, reply))
                                {
                                    return false;
                                }
                                break;
                            case AnswerType.Answer:
                                ParseStatusResponse(cmd, reply);
                                break;
                        }
                    }
                }
            }
       
            return result;
        }


        private bool HandleNoteAcceptorEventCodes (byte code)
        {

            try
            {
                _credit += _channelValues[code - 0x61];
                OnCashIn(new CashInEventArgs(_channelValues[code - 0x61], false));
                Log.Debug(String.Concat("Note [", _channelValues[code - 0x61], "] accepted!"));
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool CheckIfCashLimitExceeded (byte code)
        {
            bool result = false;

            try
            {
                decimal cash =  _channelValues [code - 0x61];
                if (cash > _maxLimit)
                {
                  
                    result = true;
                }
            }
            catch
            {
            }

            return result;
        }

        private void ParseStatusResponse (byte cmd, byte[] response)
        { 

            switch (cmd)
            {
                case ID003_Status_Response.VendValid:
                    // Response to [VEND VALID] ->  Send ACK
                    if (HandleNoteAcceptorEventCodes(_escrowData))
                    {
                        serialPort.SendCmd(PackMsg(ID003_Constants.ACK));
                    }
                    _escrowData = 0;
                    break;
                case ID003_Status_Response.Escrow:
                    _escrowData = response [3];
                    if (CheckIfCashLimitExceeded (response[3]))
                    {
                        serialPort.SendCmd (PackMsg(ID003_Commands.Return));
                        _escrowData = 0;

                        OnCashLimitExceeded (new ValidatorEventArgs<string>("Cash Limit is exedeed. Cash was rejected!"));
                        Log.Debug ("ID003: Cash Limit is exedeed. Cash was rejected!");
                    }
                    else
                    {
                        serialPort.SendCmd (PackMsg(ID003_Commands.Stack2));
                    }

                    break;
                
            }
        }

        private AnswerType GetAnswerType (byte[] msg)
        {
            if (msg != null)
            {
                if (msg.Length >= ID003_Constants.MinAnswerSize)
                {
                    byte cmd = msg[2];

                    switch (cmd)
                    {
                        case ID003_Commands.Reset:
                        case ID003_Commands.Return:
                        case ID003_Commands.Stack1:
                        case ID003_Commands.Stack2: return AnswerType.ACK;

                        case ID003_Commands.SetInhibits:
                        case ID003_Commands.SetDirections:
                        case ID003_Commands.SetSecurities:
                        case ID003_Commands.SetEnables:
                        case ID003_Commands.SetCommMode:
                        case ID003_Commands.OptionalFunction:
                        case ID003_Commands.BarcodeFunction:
                        case ID003_Commands.BarInhibit: return AnswerType.Echo;

                        case ID003_Commands.StatusRequest:
                        case ID003_Commands.GetBillTable:
                        case ID003_Commands.VersionRequest: return AnswerType.Answer;

                    }
                }
            }

            return AnswerType.Default;
        }

        private bool SetDefaultParameters ()
        {
            byte[] reply = null;
           // Reset
            if (!ExecuteCommand (PackMsg (ID003_Commands.Reset), out reply))
            {
                return false;
            }

            // Communication Mode
            byte[] com_mode = new byte[] { ID003_Commands.SetCommMode, 0x00 };

            if (!ExecuteCommand(PackMsg(com_mode), out reply))
            {
                return false;
            }

            // Optional function
            byte[] optional = new byte[] { ID003_Commands.OptionalFunction, 3, 0 };

            if (!ExecuteCommand(PackMsg(optional), out reply))
            {
                return false;
            }

            // Barcode function
            byte[] barcode = new byte[] { ID003_Commands.BarcodeFunction, 1, 0x12};

            if (!ExecuteCommand(PackMsg(barcode), out reply))
            {
                return false;
            }

            //Barcode Inhibit
            byte[] b_inhibit = new byte[] { ID003_Commands.BarInhibit, 0xFC };

            if (!ExecuteCommand(PackMsg(b_inhibit), out  reply))
            {
                return false;
            }

            return true;
        }

        private bool ByteArrayCompare (byte[] a1, byte[] a2)
        {

            if (a1.Length != a2.Length)
            {
                return false;
            }

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }
              
            return true;
        }

        public override List<DeviceInfo> GetShortDeviceInventory ()
        {
            return GetDeviceInventory();
        }
        public override List <DeviceInfo> GetDeviceInventory()
        {
            bool result = false;
            byte[] reply = null;

            List<DeviceInfo> list = new List<DeviceInfo>(1);
            DeviceInfo di = new DeviceInfo();

            try 
            {
                if (ExecuteCommand (PackMsg(ID003_Commands.VersionRequest), out  reply))
                {
                    string line = System.Text.Encoding.ASCII.GetString(reply, 1, reply.Length - 1);
                    string[] inventory = line.Split(' ');

                    if (inventory != null)
                    {
                        if (inventory.Length > 0)
                        {
                            if (inventory[0].ToUpperInvariant()[0] == 'U')
                            {
                                di.device_producer = "JCM";
                                di.device_type = DeviceType.BILL_VALIDATOR;
                                di.device_serial_number = "N/A";
                            }
                            di.device_model = inventory[0];

                        }
                        if (inventory.Length >= 1)
                        {
                            di.device_firmware_version = inventory[1];
                        }

                    }
                    result = true;
                    list.Add(di);
                }
                else
                {
                    di.device_type = DeviceType.UNKNOWN;
                }
               
            }
            catch
            {
            }        

            return list;
        }


        public override bool IsDataSetValid ()
        {
            return true;
        }

      
        public override bool SetEnabledChannels (decimal ActValue)
        {
            return true;
        }

        public override bool Disable ()
        {
            bool result = false;
            byte[] reply = null;
            byte[] disable = new byte[] { ID003_Commands.SetInhibits,1 };

            lock (enableLock)
            {
                base.Disable();
                _credit = 0;
                result = ExecuteCommand (PackMsg (disable), out reply);
            }

            return result;
        }

        public override bool Enable (decimal MaxLimit)
        {

            bool result = false;
            byte[] reply = null;
            byte[] enable = new byte[] { ID003_Commands.SetInhibits, 0 };

            lock (enableLock)
            {
                base.Enable(MaxLimit);
                _maxLimit = MaxLimit < 0 ? 0 : MaxLimit;
                result = ExecuteCommand (PackMsg(enable), out reply);
 
            }

            return result;
        }

        public override decimal GetCredit ()
        {
            base.GetCredit();
            return _credit;
        }

        public override void SetCredit (decimal credit)
        {
            base.SetCredit(credit);
            _credit = credit;
        }
        public override bool ResetCredit ()
        {
            base.ResetCredit();
            _credit = 0;
            return true;
        }

        public override bool CheckBillValidator ()
        {
            bool result = false;

            byte[] reply = null;

            result = ExecuteCommand(PackMsg(ID003_Commands.StatusRequest), out reply);

            return result;
        }

        ushort CalcCRC16 (byte[] msg)
        {
            ushort crc = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                crc = (ushort)((crc >> 8) ^ crc_table[msg[i] ^ (crc & 0x00FF)]);
            }

            return crc;
        }

        ushort CalcCRC16 (byte[] msg, int len)
        {
            ushort crc = 0;

            if (len <= msg.Length)
            {
                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)((crc >> 8) ^ crc_table[msg[i] ^ (crc & 0x00FF)]);
                }
            }

            return crc;
        }

        public byte[] PackMsg (byte[] cmd)
        {
            int len = 1 + 1 + cmd.Length + 2;

            byte[] msg = new byte[len];
            msg[0] = ID003_Constants.Prefix;
            msg[1] = (byte)len;
            int pos = 2;
            for (int i = 0; i < cmd.Length; i++)
            {
                msg[pos] = cmd[i];
                pos++;
            }

            ushort crc = CalcCRC16 (msg, pos);
            msg[pos++] = (byte)crc;
            msg[pos] = (byte)((crc >> 8) & 0x00FF);

            return msg;

        }

        public byte[] PackMsg (byte cmd)
        {
            int len = 1 + 1 + 1 + 2;

            byte[] msg = new byte[len];
            msg[0] = ID003_Constants.Prefix;
            msg[1] = (byte)len;
            msg[2] = cmd;

            ushort crc = CalcCRC16(msg, 3);
            msg[3] = (byte)crc;
            msg[4] = (byte)((crc >> 8) & 0x00FF);

            return msg;

        }

        public bool UnpackMsg (byte[] msg, ref byte cmd)
        {
            bool result = false;

            if (msg.Length >= ID003_Constants.MinAnswerSize)
            {
                if (msg[0] == ID003_Constants.Prefix)
                {
                    int len = msg[1];
                    if (len == msg.Length)
                    {
                        ushort crc = CalcCRC16 (msg, len - 2);
                        ushort lw = msg[len - 2];
                        ushort hw = msg[len - 1];
                        ushort msg_crc = (ushort)(hw * 256 + lw);

                        if (msg_crc == crc)
                        {
                            result = true;
                            cmd = msg[2];
                        }
                    }
                }
            }

            return result;
        }

    }
}
