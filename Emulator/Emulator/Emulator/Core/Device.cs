using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emulator
{
    public enum CCTALK_COMMANDS
    {
        REQ_MANUFACTURER_ID = 246,
        REQ_EQUIPMENT_CATEGORY = 245,
        REQ_PRODUCT_CODE = 244,
        REQ_SERIAL_NUMBER = 242,
        REQ_SW_VERSION = 241,
        SET_INHIBIT = 231,
        REQ_BUFFERED_CREDIT_CODES = 229,
        SET_MASTER_INHIBIT = 228,
        REQ_COIN_ID = 184,
        READ_BUFFERED_BILL_EVENTS = 159,
        REQ_BILL_ID = 157,
        REQ_CURRENCY_REVISION = 145,
        REQ_BILL_OPER_MODE = 152,
        MODIFY_BILL_OPER_MODE = 153
       
    }

    public class DeviceConfig
    {

        public byte device_address;
        public byte master_address;
        public string equipment_category;
        public string dataset;
        public bool use_crc16;
        public bool use_cryptograthy;
        public Dictionary <int, string> money_id_list;
        
    }

    public class Device
    {
        private byte device_address;
        private byte master_address = 1;
        private string equipment_category;
        private string dataset;
        private bool use_crc16;
        private bool use_cryptograthy;
        private Func<byte[], byte[]> handler = null;
       
        private readonly byte[] encryption_key = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
        private Dictionary<int, string> money_id_list;
        private const string manufacturer_id = "Sportradar";
        private bool is_device_enabled = false;

        private byte event_counter;
        private byte[] event_buffer = new byte[EVENT_BUF_LEN];
        private int buffer_pointer = 0;

        private const int MSG_MIN_LEN = 4;
        private const int EVENT_BUF_LEN = 10;

        private readonly string log_path = System.AppDomain.CurrentDomain.BaseDirectory + "log\\Emulator.log";

        public Device (DeviceConfig dc)
        {
            this.device_address = dc.device_address;
            this.master_address = dc.master_address;
            this.equipment_category = dc.equipment_category;
            this.dataset = dc.dataset;
            this.use_crc16 = dc.use_crc16;
            this.use_cryptograthy = dc.use_cryptograthy;
            this.money_id_list = dc.money_id_list;
            
            handler =  ProcessIncomingMsg;
        }

        public bool GetMsgHandler(out byte address, out  Func<byte[], byte[]> h)
        {
             address = device_address;
             h = handler;
            
             return true;  
        }

        public bool IsDeviceEnabled()
        {
            return is_device_enabled;
        }

        private void WriteLogFile (string error_type, string eror_data)
        {

            using (StreamWriter sw = (File.Exists(log_path)) ? File.AppendText(log_path) : File.CreateText(log_path))
            {
                sw.WriteLine("\r\n==================================================================================================");
                sw.WriteLine("==================================================================================================");
                sw.WriteLine("\r\nRecord Date: {0}", DateTime.Now.ToString());
                sw.WriteLine(error_type + " : " + eror_data);
                sw.WriteLine("==================================================================================================");

            }
        }

        private  string ByteArrayToString (byte[] array)
        {
            StringBuilder hex = new StringBuilder (array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat ("{0:X2}", b);
                hex.Append(" ");
            }
            return hex.ToString();
        }

        private bool ValidateIncomingMsg (byte[] msg, ref int cmd)
        {
            bool result = false;

            try
            {
                if (msg.Length > MSG_MIN_LEN)
                {
                    int address = msg[0];

                    if (address == device_address)
                    {
                        if (use_cryptograthy)
                        {
                            CCTCrypt.Decrypt(msg, 2, msg.Length - 2, encryption_key);
                        }

                        if (msg[1] != msg.Length - 5)
                        {
                            
                            WriteLogFile("ERROR IN MSG", ByteArrayToString(msg));
                        }

                        cmd = msg[3];
                        result = true;
                    }
                    else if (!(address == 2 || address == 0x28))
                    {
                        WriteLogFile("ERROR IN MSG (WRONG ADDRESS)", ByteArrayToString(msg));
                    }

                }
                else
                {
                     WriteLogFile("ERROR IN MSG", ByteArrayToString(msg));
                }
            }
            catch
            {
            }

            return result;
        }
         
        private byte[] OnCurrencyRevisionRequest()
        {
            if (String.IsNullOrEmpty(dataset))
            { 
                return null;
            }

            return PrepareMessage (dataset);
        }

        private byte[] OnBillIdRequest (byte[] msg)
        {
           
            string bill_id;
            CCTCrc16 crc16 = new CCTCrc16();

            bool result = money_id_list.TryGetValue((int)msg[4], out bill_id);
            if (!result)
            {
                money_id_list.TryGetValue(-1, out bill_id); //get default value
            }
            int msg_size = bill_id.Length;
            byte[] reply = new byte[msg_size + 5];

            reply[0] = master_address;
            reply[1] = (byte)msg_size;        
            reply[3] = 0;

            Array.Copy(System.Text.Encoding.ASCII.GetBytes(bill_id), 0, reply, 4, msg_size);

            crc16.Reset();
            crc16.PushByte(reply[0]);
            crc16.PushByte(reply[1]);
            crc16.PushData(reply, 3, msg_size + 1);
            reply [2] = (byte)(crc16.CRC & (ushort)0x00FF);
            reply[4 + msg_size] = (byte)(crc16.CRC >> 8);
            if (use_cryptograthy)
            {
                CCTCrypt.Encrypt(reply, 2, 3 + msg_size, encryption_key);
            }

            return reply;
        }

        private byte[] PrepareMessage (string msg)
        {
            CCTCrc16 crc16 = new CCTCrc16();
            int msg_len = msg.Length;
            byte[] reply = new byte[msg_len + 5];           

            reply[0] = master_address;
            reply[1] = (byte)msg_len;
            reply[3] = 0;
            Array.Copy (System.Text.Encoding.ASCII.GetBytes (msg), 0, reply, 4, msg_len);

            crc16.Reset();
            crc16.PushByte(reply[0]);
            crc16.PushByte(reply[1]);
            crc16.PushData(reply, 3, msg_len + 1);
            reply[2] = (byte)(crc16.CRC & (ushort)0x00FF);
            reply[4 + msg_len] = (byte)(crc16.CRC >> 8);
            if (use_cryptograthy)
            {
                CCTCrypt.Encrypt(reply, 2, 3 + msg_len, encryption_key);
            }

            return reply;
        }

        private byte[] PrepareMessage (byte[] msg)
        {
            CCTCrc16 crc16 = new CCTCrc16();
            int msg_len = msg.Length;
            byte[] reply = new byte[msg_len + 5];

            reply[0] = master_address;
            reply[1] = (byte)msg_len;
            reply[3] = 0;
            Array.Copy (msg, 0, reply, 4, msg_len);

            crc16.Reset();
            crc16.PushByte(reply[0]);
            crc16.PushByte(reply[1]);
            crc16.PushData(reply, 3, msg_len + 1);
            reply[2] = (byte)(crc16.CRC & (ushort)0x00FF);
            reply[4 + msg_len] = (byte)(crc16.CRC >> 8);
            if (use_cryptograthy)
            {
                CCTCrypt.Encrypt(reply, 2, 3 + msg_len, encryption_key);
            }

            return reply;
        }

        private byte[] SendACK ()
        {
            CCTCrc16 crc16 = new CCTCrc16();
            byte[] reply = new byte[5];

            reply[0] = master_address;
            reply[1] = 0;
            reply[3] = 0;
           
            crc16.Reset();
            crc16.PushByte(reply[0]);
            crc16.PushByte(reply[1]);
            crc16.PushByte(reply[3]);
            reply[2] = (byte)(crc16.CRC & (ushort)0x00FF);
            reply[4] = (byte)(crc16.CRC >> 8);
            if (use_cryptograthy)
            {
                CCTCrypt.Encrypt(reply, 2, 3, encryption_key);
            }

            return reply;
        }

        private byte[] OnManufacturerIdRequest()
        {
            return PrepareMessage (manufacturer_id);
        }

        private byte[] OnEquipmentCategoryRequest()
        {
            return PrepareMessage(equipment_category);
        }

        private byte[] OnProductCodeRequest()
        {
            return PrepareMessage ("SW Emulator");
        }

        private byte[] OnSerialNumberRequest()
        {
            byte[] sn = new byte[] { 0x0, 0x0, 0x0, 0x1 };
            return PrepareMessage (sn);
        }

        private byte[] OnSoftwareVersionRequest()
        {
            return PrepareMessage("29/09/2014");
        }

        private byte[] OnBillOperatingModenRequest ()
        {
            byte[] mode = new byte[] {0x1};
            return PrepareMessage (mode);
        }

        private byte[] OnSetMasterInhibitRequest (byte[] msg)
        {
            if ((msg[4] & (byte)0x01) == 1)
            {
                is_device_enabled = true;
            }
            else
            {
                is_device_enabled = false;
            }

            return SendACK();
        }

        private byte[] OnBufferEventsRequest ()
        {           
            byte[] reply = new byte [EVENT_BUF_LEN + 1];       

            lock (event_buffer)
            {
                reply[0] = event_counter;
                Array.Copy (event_buffer, 0, reply, 1, EVENT_BUF_LEN);
                buffer_pointer = (int)0;
                for (int i = 0; i < EVENT_BUF_LEN; i++)
                {                
                    event_buffer[i] = 0;
                }
            }

            return PrepareMessage (reply);
        }

        private byte[] OnModifyBillOperatingModeRequest()
        {
            return SendACK();
        }

        public bool InjectCashEvent (string cash)
        {
            bool result = false;

            int value = money_id_list.FirstOrDefault(x => x.Value == cash).Key;

            if (value != 0)
            {
                lock (event_buffer)
                {
                    event_counter++;
                    if (event_counter == 0)
                    {
                        event_counter = 1;
                    }
                    if (buffer_pointer > EVENT_BUF_LEN - 2)
                    {
                        buffer_pointer = (int)0;
                    }
                    event_buffer[buffer_pointer++] = (byte)value;
                    event_buffer[buffer_pointer++] = 0;

                    result = true;
                   // System.Threading.Thread.Sleep (200);
                }
            }

            return result;
        }

        private byte[] ProcessIncomingMsg  (byte[] msg)
        {
            int cmd = 0;
            int msg_len = 0;

            if (ValidateIncomingMsg (msg, ref cmd))
            {
                byte[] answer = null;
                byte[] echo = new byte[msg.Length];
                Array.Copy(msg, echo, msg.Length);

                switch (cmd)
                {
                    case (byte)CCTALK_COMMANDS.REQ_EQUIPMENT_CATEGORY:
                        answer = OnEquipmentCategoryRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_CURRENCY_REVISION:
                        answer = OnCurrencyRevisionRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_BILL_ID:
                        answer = OnBillIdRequest(msg);
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_COIN_ID:
                        answer = OnBillIdRequest(msg);
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_MANUFACTURER_ID:
                        answer = OnManufacturerIdRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_PRODUCT_CODE:
                        answer = OnProductCodeRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_SERIAL_NUMBER:
                        answer = OnSerialNumberRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_SW_VERSION:
                        answer = OnSoftwareVersionRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_BILL_OPER_MODE:
                        answer = OnBillOperatingModenRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.SET_MASTER_INHIBIT:
                        answer = OnSetMasterInhibitRequest(msg);
                        break;

                    case (byte)CCTALK_COMMANDS.READ_BUFFERED_BILL_EVENTS:
                        answer = OnBufferEventsRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.REQ_BUFFERED_CREDIT_CODES:
                        answer = OnBufferEventsRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.MODIFY_BILL_OPER_MODE:
                        answer = OnModifyBillOperatingModeRequest();
                        break;

                    case (byte)CCTALK_COMMANDS.SET_INHIBIT:
                        //answer = SendACK();
                        answer = OnSetMasterInhibitRequest(msg);
                        break;

                    default:
                        WriteLogFile("UNSUPPORTED CMD IN MSG", ByteArrayToString(msg));
                        break;

                }
                if (answer != null && answer.Length > 0)
                {
                    byte[] reply = new byte[echo.Length + answer.Length];
                    Array.Copy(echo, 0, reply, 0, echo.Length);
                    Array.Copy(answer, 0, reply, echo.Length, answer.Length);
                    return reply;
                }
            }
            return null;
        }
    }
}
