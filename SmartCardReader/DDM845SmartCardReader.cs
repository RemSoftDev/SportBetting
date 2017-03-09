using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Ports;

namespace SmartCardReader
{
    public class DDM845_COMMAND
    {
       
        public static readonly byte[] READER_RESET = new byte[] { 0x01, 0x00, 0x00, 0x01, 0x7F, 0x7F };
        public static readonly byte[] READER_INIT_I2C_MMODE = new byte[] { 0x01, 0x00, 0x00, 0x03, 0x54, 0x04, 0x08, 0x5A };
        public static readonly byte[] READER_ACTIVATE = new byte[] { 0x01, 0x00, 0x00, 0x02, 0x4E, 0x00, 0x4D };
        public static readonly byte[] READER_DEACTIVATE = new byte[] { 0x01, 0x00, 0x00, 0x01, 0x6E, 0x6E };
        public static readonly byte[] READER_CHECK_POS = new byte[] { 0x01, 0x00, 0x00, 0x01, 0x38, 0x38 };

        public static readonly byte[] READ_BINARY_0X00_0X19 = new byte[] { 0x01, 0x00, 0x00, 0x06, 0x42, 0x00, 0xB0, 0x00, 0x00, 0x19, 0xEC };
        public static readonly byte[] READER_DEVICE_DESCRIPTORS = new byte[] { 0x01, 0x00, 0x00, 0x02, 0x37, 0x01, 0x35 };
        public static readonly byte[] READER_DEVICE_SERIAL = new byte[] { 0x01, 0x00, 0x00, 0x02, 0x37, 0x00, 0x34 };
    }

    public class DDM845SmartCardReader: SmartCardReader
    {
        private const int SN_RECORD_START_ADDRESS = 0x00;
        private const int SN_RECORD_SIZE = 0x19;

        private bool CardReaderInitialized = false;
        private static object ReadWriteOperationLock = new Object();
        private string VirtualCOM = String.Empty;
        private ComPort serialPort = null;

        public DDM845SmartCardReader (string vcom):base (SN_RECORD_SIZE, SN_RECORD_START_ADDRESS)
        {
            VirtualCOM = vcom;
            new Thread (RunCardDetection).Start();
        }
       
        private void RunCardDetection()
        {
            while (true)
            {
                while (!CardReaderInitialized)
                {
                    lock (ReadWriteOperationLock)
                    {
                        CardReaderInitialized = InitSmartCardReader ();
                    }
                    Thread.Sleep (1000);
                }
               
                CheckCardPosition ();
              
                Thread.Sleep (500);
            }
        }

        private bool TryOpenPort()
        {
            bool result = false;

            if (serialPort == null)
            {
                if (!String.IsNullOrEmpty (VirtualCOM))
                {
                    serialPort = new ComPort(VirtualCOM, 38400, Parity.None, StopBits.One);
                }
            }

            if (serialPort != null)
            {
                result = serialPort.TryOpen();
            }
            return result;
        }

        private byte CalculateCRC (byte[] data)
        {
            byte crc = 0;
            if (data != null && data.Length > 0)
            {
                for (int i = 0; i < data.Length -1; i++)
                {
                    crc ^= data [i];
                }
            }
            return crc;
        }

        private bool InitSmartCardReader ()
        {
            bool result = false;
          
            try
            {
                if (TryOpenPort ())
                {
                  //  serialPort.SendCmd (DDM845_COMMAND.READER_RESET, 5000);
                    byte[] reply = serialPort.SendCmd (DDM845_COMMAND.READER_INIT_I2C_MMODE);
                    if (reply != null && reply.Length == 6 && reply[4] == 0x5E)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        private void CheckCardPosition ()
        {
            CardPosition cardpos = SmartCardPos;
            uint result = 0;
            byte[] reply = null;
          
            try
            {
                lock (ReadWriteOperationLock)
                {
                    reply = serialPort.SendCmd (DDM845_COMMAND.READER_CHECK_POS);      
                }

                if (reply != null && reply.Length == 6)
                {
                    if (reply[4] == 0x70)
                    {
                        cardpos = CardPosition.REMOVED;
                    }
                    else if (reply[4] == 0x73)
                    {
                        cardpos = CardPosition.INSERTED;
                    }
                    ProcessCard (cardpos);
                }
            }
            catch
            {
                OnCardError ();
                CardReaderInitialized = false;
            }
        }

        public bool WriteDataA (string data)
        {
            return WrireData (SN_RECORD_START_ADDRESS, data);
        }

        public override bool WrireData (int start_address, string wr_data)
        {
            bool result = false;
            byte cmd_len = 0;
            const byte MAIN_CMD_SIZE = 6;
            const int RETRY_MAX = 3;

            if (SmartCardPos == CardPosition.INSERTED)
            {
                lock (ReadWriteOperationLock)
                {
                    if (!String.IsNullOrEmpty (wr_data))
                    {
                        try
                        {
                            byte[] data = Encoding.ASCII.GetBytes (wr_data);
                            cmd_len = (byte)(data.Length + MAIN_CMD_SIZE);
                            byte[] mem_trans_cmd = new byte[] { 0x01, 0x00, 0x00, cmd_len, 0x42, 0x00, 0xD0, 0x00, (byte)start_address, (byte)data.Length };

                            byte[] cmd = new byte[mem_trans_cmd.Length + data.Length + 1];
                            Array.Copy(mem_trans_cmd, cmd, mem_trans_cmd.Length);
                            Array.Copy(data, 0, cmd, mem_trans_cmd.Length, data.Length);
                            byte crc = CalculateCRC(cmd);
                            cmd [cmd.Length - 1] = crc;

                            for (int attempt = 0; attempt < RETRY_MAX; attempt++)
                            {
                                byte[] reply = serialPort.SendCmd (DDM845_COMMAND.READER_ACTIVATE);
                                if (reply != null && reply.Length == 13)
                                {
                                    reply = serialPort.SendCmd(cmd);

                                    if (reply != null && reply.Length == 7)
                                    {
                                        if (reply[4] == 0x90 && reply[5] == 0x00)
                                        {
                                            result = true;
                                            Verify ();
                                        }
                                    }
                                }
                                
                                reply = serialPort.SendCmd (DDM845_COMMAND.READER_DEACTIVATE);
                                if (result)
                                {
                                    break;
                                }
                            }
                            if (!result)
                            {
                                OnCardInserted ("CARDERROR");
                            }
                        }
                        catch
                        {
                        }

                    }
                }
            }

          /*  if (!result)
            {
                OnCardError();
            }*/
            return result;
        }

        public override bool ReadData (int start_address, int len, ref string data_str)
        {
            bool result = false;

            data_str = String.Empty;
            byte[] data_arr = new byte [len];

            if (ReadData (start_address, len, ref data_arr))
            {
                try
                {
                    data_str = System.Text.Encoding.ASCII.GetString (data_arr);
                    result = true;
                }
                catch
                {
                }
        
            }
            if (!result)
            {
                OnCardError ();
            }
            return result;
        }

        public override bool ReadData (int start_address, int len, ref byte[] data)
        {
            bool result = false;
            const int RETRY_MAX = 3;

            if (SmartCardPos == CardPosition.INSERTED)
            {            
                lock (ReadWriteOperationLock)
                {
                    for (int attempt = 0; attempt < RETRY_MAX; attempt++)
                    {
                        byte[] reply = serialPort.SendCmd (DDM845_COMMAND.READER_ACTIVATE);
                        if (reply != null && reply.Length == 13)
                        {
                            reply = serialPort.SendCmd (DDM845_COMMAND.READ_BINARY_0X00_0X19);
                            if (reply != null && reply.Length == 0x20)
                            {
                                Array.Copy(reply, 4, data, 0, len);
                                {
                                    result = true;
                                }
                            }
                        }
                        reply = serialPort.SendCmd (DDM845_COMMAND.READER_DEACTIVATE);
                        if (result)
                        {
                            break;
                        }
                    }                   
                   
                }
            }
            return result;
        }

      
        public override bool GetLastError (out string error)
        {
            error = String.Empty;
            return false;
        }

        public override bool GetCardStatus (ref CardPosition pos)
        {
            pos = SmartCardPos;
            return true;
        }

        private bool ParseInventory (string serial, ref CardReaderInfo cri)
        {
            bool result = false;

            if (!String.IsNullOrEmpty (serial))
            {
                int len = serial.Length;
                int pos = serial.IndexOf(',');
                if (pos > 0)
                {
                    cri.type = serial.Substring(0, pos);
                }
                if (len > pos + 1)
                {
                    int pos_next = serial.IndexOf(',', pos + 1);
                    if (pos_next > 0)
                    {
                        cri.firmware = serial.Substring(pos + 1, pos_next - pos - 1);
                    }
                    pos = serial.IndexOf('"', pos_next);
                    if (pos > 0)
                    {
                        if (len > pos + 1)
                        {
                            cri.manufacturer = serial.Substring(pos + 1, len - pos - 2);
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        public override CardReaderInfo GetReaderInventory () 
        {
            const int RETRY_MAX = 3;
            CardReaderInfo cri = null;
            bool result = false;
            byte[] reply = null;

            try
            {
                lock (ReadWriteOperationLock)
                {
                    for (int attempt = 0; attempt < RETRY_MAX; attempt++)
                    {
                        reply = serialPort.SendCmd(DDM845_COMMAND.READER_DEVICE_DESCRIPTORS);
                        if (reply != null && reply.Length > 0x40)
                        {
                            result = true;
                            break;
                        }
                    }
                }

                if (result)
                {
                    cri = new CardReaderInfo();
                    ParseInventory(Encoding.ASCII.GetString(reply, 4, reply.Length - 5), ref cri);
                    lock (ReadWriteOperationLock)
                    {
                        for (int attempt = 0; attempt < RETRY_MAX; attempt++)
                        {
                            reply = serialPort.SendCmd(DDM845_COMMAND.READER_DEVICE_SERIAL);
                            if (reply != null && reply.Length > 0x0a)
                            {
                                result = true;
                                cri.sn = Encoding.ASCII.GetString(reply, 4, reply.Length - 5);
                                cri.sn = cri.sn.Replace(".", "");
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                cri = null;
            }
            return cri;
        }
    }
}
