using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITLlib;
using SportRadar.Common.Logs;

namespace Nbt.Services.Scf.CashIn.Validator.SSP
{
    public class ChannelData
    {
        public int Value;
        public byte Channel;
        public char[] Currency;
        public int Level;
        public bool Recycling;
        public ChannelData()
        {
            Value = 0;
            Channel = 0;
            Currency = new char[3];
            Level = 0;
            Recycling = false;
        }
    };

    public class SSP_Validator
    {
        // ssp library variables
        SSPComms eSSP;
        SSP_COMMAND cmd, storedCmd;
        SSP_KEYS keys;
        SSP_FULL_KEY sspKey;
        SSP_COMMAND_INFO info;

        private static ILog Log = LogFactory.CreateLog (typeof (SSP_Validator));

        char m_UnitType;    
        int m_NumStackedNotes;    
        int m_NumberOfChannels;       
        int m_ValueMultiplier;      
        List<ChannelData> m_UnitDataList;

        /* Variable Access */

        public SSPComms SSPComms
        {
            get { return eSSP; }
            set { eSSP = value; }
        }

      
        public SSP_COMMAND CommandStructure
        {
            get { return cmd; }
            set { cmd = value; }
        }

      
        public SSP_COMMAND_INFO InfoStructure
        {
            get { return info; }
            set { info = value; }
        }


       
        public char UnitType
        {
            get { return m_UnitType; }
        }

       
        public int NumberOfChannels
        {
            get { return m_NumberOfChannels; }
            set { m_NumberOfChannels = value; }
        }

      
        public int NumberOfNotesStacked
        {
            get { return m_NumStackedNotes; }
            set { m_NumStackedNotes = value; }
        }

       
        public int Multiplier
        {
            get { return m_ValueMultiplier; }
            set { m_ValueMultiplier = value; }
        }


        public SSP_Validator()
        {
            eSSP = new SSPComms();
            cmd = new SSP_COMMAND();
            storedCmd = new SSP_COMMAND();
            keys = new SSP_KEYS();
            sspKey = new SSP_FULL_KEY();
            info = new SSP_COMMAND_INFO();
            m_UnitDataList = new List<ChannelData>();
        }

       
        public int GetChannelValue (int channelNum)
        {
            if (channelNum >= 1 && channelNum <= m_NumberOfChannels)
            {
                foreach (ChannelData d in m_UnitDataList)
                {
                    if (d.Channel == channelNum)
                    {
                        return d.Value;
                    }
                }
            }
            return -1;
        }

        // get a channel currency
        public string GetChannelCurrency (int channelNum)
        {
            if (channelNum >= 1 && channelNum <= m_NumberOfChannels)
            {
                foreach (ChannelData d in m_UnitDataList)
                {
                    if (d.Channel == channelNum)
                    {
                        return new string (d.Currency);
                    }
                }
            }
            return string.Empty;
        }

      
        public bool OpenComPort ()
        {
            
            if (!eSSP.OpenSSPComPort (cmd))
            {
                return false;
            }
            return true;
        }

        public bool SendCommand ()
        {
            // Backup data and length in case we need to retry
            byte[] backup = new byte[255];
            cmd.CommandData.CopyTo(backup, 0);
            byte length = cmd.CommandDataLength;

            // attempt to send the command
            if (eSSP.SSPSendCommand (cmd, info) == false)
            {
                eSSP.CloseComPort ();
                return false;
            }

            return true;
        }

        private bool CheckGenericResponses ()
        {
            bool result = false;
             
            if (cmd.ResponseData[0] == CCommands.SSP_RESPONSE_CMD_OK)
            {
                result = true;
            }
            else
            {
                switch (cmd.ResponseData[0])
                {
                    case CCommands.SSP_RESPONSE_CMD_CANNOT_PROCESS:
                        Log.Debug("Command response is  CANNOT PROCESS COMMAND");
                        break;
                    case CCommands.SSP_RESPONSE_CMD_FAIL:
                        Log.Debug("Command response is FAIL");
                        break;
                    case CCommands.SSP_RESPONSE_CMD_KEY_NOT_SET:
                        Log.Debug("Command response is KEY NOT SET");
                        break;
                    case CCommands.SSP_RESPONSE_CMD_PARAM_OUT_OF_RANGE:
                        Log.Debug("Command response is PARAM OUT OF RANGE");
                        break;
                    case CCommands.SSP_RESPONSE_CMD_SOFTWARE_ERROR:
                        Log.Debug("Command response is SOFTWARE ERROR");
                        break;
                    case CCommands.SSP_RESPONSE_CMD_UNKNOWN:
                        Log.Debug("Command response is UNKNOWN");
                        break;
                    case CCommands.SSP_RESPONSE_CMD_WRONG_PARAMS:
                        Log.Debug("Command response is WRONG PARAMETERS");
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public bool NegotiateKeys ()
        {
            byte i;
            cmd.EncryptionStatus = false;

            // send sync      
            cmd.CommandData[0] = CCommands.SSP_CMD_SYNC;
            cmd.CommandDataLength = 1;
            if (!SendCommand ())
            {
                return false;
            }

            eSSP.InitiateSSPHostKeys (keys, cmd);

            // send generator
            cmd.CommandData[0] = CCommands.SSP_CMD_SET_GENERATOR;
            cmd.CommandDataLength = 9;
          
            for (i = 0; i < 8; i++)
            {
                cmd.CommandData[i + 1] = (byte)(keys.Generator >> (8 * i));
            }
            if (!SendCommand ())
            {
                return false;
            }

            // send modulus
            cmd.CommandData[0] = CCommands.SSP_CMD_SET_MODULUS;
            cmd.CommandDataLength = 9;
           
            for (i = 0; i < 8; i++)
            {
                cmd.CommandData[i + 1] = (byte)(keys.Modulus >> (8 * i));
            }
            if (!SendCommand ())
            {
                return false;
            }

            // send key exchange
            cmd.CommandData[0] = CCommands.SSP_CMD_KEY_EXCHANGE;
            cmd.CommandDataLength = 9;
         
            for (i = 0; i < 8; i++)
            {
                cmd.CommandData[i + 1] = (byte)(keys.HostInter >> (8 * i));
            }
            if (!SendCommand ())
            {
                return false;
            }

            keys.SlaveInterKey = 0;
            for (i = 0; i < 8; i++)
            {
                keys.SlaveInterKey += (UInt64)cmd.ResponseData[1 + i] << (8 * i);
            }

            eSSP.CreateSSPHostEncryptionKey (keys);

            // get full encryption key
            cmd.Key.FixedKey = 0x0123456701234567;
            cmd.Key.VariableKey = keys.KeyHost;

            return true;
        }

        public void SetProtocolVersion (byte pVersion)
        {
            cmd.CommandData[0] = CCommands.SSP_CMD_HOST_PROTOCOL_VERSION;
            cmd.CommandData[1] = pVersion;
            cmd.CommandDataLength = 2;
            SendCommand ();
        }

       
        public bool EnableValidator ()
        {
            bool result = false;

            cmd.CommandData[0] = CCommands.SSP_CMD_ENABLE;
            cmd.CommandDataLength = 1;

            if (SendCommand ())
            {
                result = CheckGenericResponses ();
            }

            return result;
        }

     
        public bool DisableValidator ()
        {
            bool result = false;

            cmd.CommandData[0] = CCommands.SSP_CMD_DISABLE;
            cmd.CommandDataLength = 1;

            if (SendCommand ())
            {
                result = CheckGenericResponses ();
            }

            return result;
        }

        
        public bool  Reset ()
        {
            bool result = false;

            cmd.CommandData[0] = CCommands.SSP_CMD_RESET;
            cmd.CommandDataLength = 1;
            if (SendCommand ())
            {
                result = CheckGenericResponses();
            }

            return result;
        }

       
        public bool SendSync ()
        {
            bool result = false;

            cmd.CommandData[0] = CCommands.SSP_CMD_SYNC;
            cmd.CommandDataLength = 1;
            if (SendCommand ())
            {
                result = CheckGenericResponses ();
            }

            return result;
        }

        public bool SetInhibits (decimal ActValue, decimal maxLimit)
        {
            bool result = false;
            byte b1 = 0, b2 = 0, b3 = 1;

            for (int i = 0; i < m_UnitDataList.Count; i++)
            {
                if (maxLimit == 0)
                {
                    Log.Info ("NoteAcceptor Limit set to 0. All Channels not set to value 0.0.");
                }
                bool flag = ((ActValue + m_UnitDataList[i].Value) <= maxLimit  && ( m_UnitDataList[i].Value != 0));
                if (flag)
                {
                    int shift = 0;
                    if (i < 8)
                    {
                        shift = (i & 31);
                        b1 |= (byte)(b3 << shift);
                        continue;
                    }

                    shift = ((i - 8) & 31);
                    b2 |= (byte)(b3 << shift);
                }
            }
         

            cmd.CommandData[0] = CCommands.SSP_CMD_SET_INHIBITS;
            cmd.CommandData[1] = b1;
            cmd.CommandData[2] = b2;
            cmd.CommandDataLength = 3;

            if (SendCommand ())
            {
                result = CheckGenericResponses ();
            }

            return result;
        }

       

        public string GetDatasetVersion ()
        {         
            cmd.CommandData[0] = CCommands.SSP_CMD_DATASET_VERSION;
            cmd.CommandDataLength = 1;

            if (SendCommand ())
            {
                if (CheckGenericResponses ())
                {
                    StringBuilder sb = new StringBuilder ();
                    for (int i = 1; i < cmd.ResponseData.Length; i++)
                    {
                        if (cmd.ResponseData[i] == 0)
                        {
                            break;
                        }

                        sb.Append ((char)cmd.ResponseData [i]);
                    }

                    return sb.ToString();
                }
            }
            return string.Empty;
        }

        public bool GetSerialNumber (out string serial)
        {
            bool result = false;

            serial = string.Empty;
            cmd.CommandData[0] = CCommands.SSP_CMD_GET_SERIAL_NUMBER;
            cmd.CommandDataLength = 1;

            if (SendCommand ())
            {
                if (CheckGenericResponses () && cmd.ResponseData.Length > 3)
                {
                    serial = (cmd.ResponseData[2] * 65536 + cmd.ResponseData[3] * 256 + cmd.ResponseData[4]).ToString();
                    result = true;
                }
            }

            return result;
        }

        public bool GetFirmwareVesion (out string fw, out string dev_type)
        {
            bool result = false;

            fw = null;
            dev_type = null;

            cmd.CommandData[0] = CCommands.SSP_CMD_GET_FW_VERSION;
            cmd.CommandDataLength = 1;

            if (SendCommand ())
            {
                if (CheckGenericResponses ())
                {
                    StringBuilder fw_sb = new StringBuilder();
                    StringBuilder dt_sb = new StringBuilder();
                    result = true;

                    for (int i = 1; i < cmd.ResponseData.Length; i++)
                    {
                        if (cmd.ResponseData[i] == 0)
                        {
                            break;
                        }

                        fw_sb.Append ((char)cmd.ResponseData [i]);
                        if (i < 5)
                        {
                            dt_sb.Append ((char)cmd.ResponseData [i]);
                        }
                    }

                    fw = fw_sb.ToString ();
                    dev_type = dt_sb.ToString ();
                    if (dev_type == "NV90")
                    {
                        dev_type = "NV9";
                    }
                }
            }

            return result;
        }

        public bool DoPool (out decimal amount, out string currency)
        {
            bool result = false;

            cmd.CommandData[0] = CCommands.SSP_CMD_POLL;
            cmd.CommandDataLength = 1;
            amount = 0;
            currency = "UNKNOWN";
            
            if (SendCommand())
            {
                result = true;
                //parse poll response
                int noteVal = 0;
                for (int i = 1; i < cmd.ResponseDataLength; i++)
                {
                   
                    switch (cmd.ResponseData[i])
                    {
                        // This response indicates that the unit was reset and this is the first time a poll
                        // has been called since the reset.
                        case CCommands.SSP_POLL_RESET:
                            Log.Debug ("Bill Validator reset");
                            break;
                        // A note is currently being read by the validator sensors. The second byte of this response
                        // is zero until the note's type has been determined, it then changes to the channel of the 
                        // scanned note.
                        case CCommands.SSP_POLL_NOTE_READ:
                           
                            break;
                        // A credit event has been detected, this is when the validator has accepted a note as legal currency.
                        case CCommands.SSP_POLL_CREDIT:
                            noteVal = GetChannelValue (cmd.ResponseData[i + 1]);
                            if (noteVal != -1)
                            {
                                amount = (decimal)noteVal;
                                currency = GetChannelCurrency(cmd.ResponseData[i + 1]);
                            }
                            m_NumStackedNotes++;
                            i++;
                            break;
                        // A note is being rejected from the validator. This will carry on polling while the note is in transit.
                        case CCommands.SSP_POLL_REJECTING:
                            break;
                        // A note has been rejected from the validator, the note will be resting in the bezel. This response only
                        // appears once.
                        case CCommands.SSP_POLL_REJECTED:
                            Log.Debug ("Note rejected");                        
                            break;
                        // A note is in transit to the cashbox.
                        case CCommands.SSP_POLL_STACKING:
                            break;
                        // A note has reached the cashbox.
                        case CCommands.SSP_POLL_STACKED:
                            Log.Debug ("Note stacked");
                            break;
                        // A safe jam has been detected. This is where the user has inserted a note and the note
                        // is jammed somewhere that the user cannot reach.
                        case CCommands.SSP_POLL_SAFE_JAM:
                            Log.Debug ("Safe jam");
                            break;
                        // An unsafe jam has been detected. This is where a user has inserted a note and the note
                        // is jammed somewhere that the user can potentially recover the note from.
                        case CCommands.SSP_POLL_UNSAFE_JAM:
                            Log.Debug ("Unsafe jam");
                            break;
                        // The validator is disabled, it will not execute any commands or do any actions until enabled.
                        case CCommands.SSP_POLL_DISABLED:
                            break;
                        // A fraud attempt has been detected. The second byte indicates the channel of the note that a fraud
                        // has been attempted on.
                        case CCommands.SSP_POLL_FRAUD_ATTEMPT:
                            Log.Debug ("Fraud attempt");
                            i++;
                            break;
                        // The stacker (cashbox) is full. 
                        case CCommands.SSP_POLL_STACKER_FULL:
                            Log.Debug ("Stacker full");
                            break;
                        // A note was detected somewhere inside the validator on startup and was rejected from the front of the
                        // unit.
                        case CCommands.SSP_POLL_NOTE_CLEARED_FROM_FRONT:
                             Log.Debug ("Note cleared from front at reset");
                            i++;
                            break;
                        // A note was detected somewhere inside the validator on startup and was cleared into the cashbox.
                        case CCommands.SSP_POLL_NOTE_CLEARED_TO_CASHBOX:
                             Log.Debug ("Note cleared to stacker at reset");
                            i++;
                            break;
                        // The cashbox has been removed from the unit. This will continue to poll until the cashbox is replaced.
                        case CCommands.SSP_POLL_CASHBOX_REMOVED:
                          
                            break;
                        // The cashbox has been replaced, this will only display on a poll once.
                        case CCommands.SSP_POLL_CASHBOX_REPLACED:
                          
                            break;
                        // A bar code ticket has been detected and validated. The ticket is in escrow, continuing to poll will accept
                        // the ticket, sending a reject command will reject the ticket.
                        case CCommands.SSP_POLL_BAR_CODE_VALIDATED:
                          
                            break;
                        // A bar code ticket has been accepted (equivalent to note credit).
                        case CCommands.SSP_POLL_BAR_CODE_ACK:
                           
                            break;
                        // The validator has detected its note path is open. The unit is disabled while the note path is open.
                        // Only available in protocol versions 6 and above.
                        case CCommands.SSP_POLL_NOTE_PATH_OPEN:
                           
                            break;
                        // All channels on the validator have been inhibited so the validator is disabled. Only available on protocol
                        // versions 7 and above.
                        case CCommands.SSP_POLL_CHANNEL_DISABLE:
                          
                            break;
                        default:
                            Log.Debug ("Unrecognised poll response detected ");
                            break;
                    }
                }
            }
            return result;
        }

        public bool Setup ()
        {
            bool result = false;

            cmd.CommandData[0] = CCommands.SSP_CMD_SETUP_REQUEST;
            cmd.CommandDataLength = 1;

            if (SendCommand ())
            {
                try
                {
                    result = true;
                    StringBuilder sb = new StringBuilder (" ITL SSP Validator SetUp: \r\n");
                    int index = 12;
                  
                    m_NumberOfChannels = cmd.ResponseData[index++];
                    sb.AppendFormat ("\r\nNumber of Channels: {0}", m_NumberOfChannels);

                    index = 13 + (m_NumberOfChannels * 2); // Skip channel security
                    m_ValueMultiplier = cmd.ResponseData[index + 2];
                    m_ValueMultiplier += cmd.ResponseData[index + 1] << 8;
                    m_ValueMultiplier += cmd.ResponseData[index] << 16;
                    sb.AppendFormat ("\r\nReal Value Multiplier: {0}", m_ValueMultiplier);

                    index = 17 + (m_NumberOfChannels * 2);
                    int sectionEnd = 17 + (m_NumberOfChannels * 5);
                    int count = 0;
                    byte[] channelCurrencyTemp = new byte[3 * m_NumberOfChannels];
                    while (index < sectionEnd)
                    {
                        sb.AppendFormat("\r\nChannel {0} , currency: ", ((count / 3) + 1));
                        channelCurrencyTemp[count] = cmd.ResponseData[index++];
                        sb.Append ((char)channelCurrencyTemp[count++]);
                        channelCurrencyTemp[count] = cmd.ResponseData[index++];
                        sb.Append((char)channelCurrencyTemp[count++]);
                        channelCurrencyTemp[count] = cmd.ResponseData[index++];
                        sb.Append((char)channelCurrencyTemp[count++]);
                    }
                    // expanded channel values (table 17+(n*5) to 17+(n*9))
                    index = sectionEnd;
                    sb.Append ("\r\nExpanded channel values:\r\n");
                    sectionEnd = 17 + (m_NumberOfChannels * 9);
                    int n = 0;
                    count = 0;
                    int[] channelValuesTemp = new int[m_NumberOfChannels];
                    while (index < sectionEnd)
                    {
                        n = BitConverter.ToInt32 (cmd.ResponseData, index);
                        channelValuesTemp[count] = n;
                        index += 4;
                        sb.AppendFormat ("Channel {0}, value = {1}\r\n", ++count, n);
                    }

                  
                    m_UnitDataList.Clear (); 
                    for (byte i = 0; i < m_NumberOfChannels; i++)
                    {
                        ChannelData d = new ChannelData();
                        d.Channel = i;
                        d.Channel++; // Offset from array index by 1
                        d.Value = channelValuesTemp[i] /** Multiplier*/;
                        d.Currency[0] = (char)channelCurrencyTemp[0 + (i * 3)];
                        d.Currency[1] = (char)channelCurrencyTemp[1 + (i * 3)];
                        d.Currency[2] = (char)channelCurrencyTemp[2 + (i * 3)];
                        d.Level = 0; 
                        d.Recycling = false;

                        m_UnitDataList.Add (d);
                    }
                   // string str = sb.ToString();
                    Log.Debug (sb.ToString());
                }
                catch
                {
                    Log.Debug ("Exception in ITL SSP Validator SetUp ()");
                }

            }

            return result;
        }

        public List<DeviceInfo> GetDeviceInventory()
        {
            List<DeviceInfo> list = new List<DeviceInfo>(1);
            DeviceInfo di = new DeviceInfo();

            try
            {
                di.device_type = DeviceType.BILL_VALIDATOR;
                di.device_producer = "ITL";
                

                if (GetSerialNumber (out di.device_serial_number))
                {
                    GetFirmwareVesion (out di.device_firmware_version, out di.device_model);
                    list.Add (di);
                }
            }
            catch
            {
                Log.Debug ("SSP: Exception while getting device inventory");
            }

            return list;
        }

        public List<DeviceInfo> GetShortDeviceInventory()
        {
            List<DeviceInfo> list = new List<DeviceInfo>(1);
            DeviceInfo di = new DeviceInfo();

            try
            {
                di.device_type = DeviceType.BILL_VALIDATOR;
                di.device_producer = "ITL";
                

                if (GetFirmwareVesion (out di.device_firmware_version, out di.device_model))
                {
                    list.Add (di);
                }
            }
            catch
            {
                Log.Debug ("SSP:Exception while getting short device inventory ");
            }

            return list;
        }
    }

}
