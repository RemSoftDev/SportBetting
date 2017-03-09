using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using System.Text;
using System.Threading;
using NLogger;
using SerialPortManager;
using System.Linq;


namespace Nbt.Services.Scf.CashIn.Validator.CCTalk
{

     public sealed class CCTCommunication
    {

        public const byte feedMasher = 99;
        public const int rotatePlaces = 12;

        //static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(CCTCommunication).Name);

        private int _baudrate;
        private static string _COMPort = "COM12";
        private object readWriteLock;
        public byte[] secArray;
        private SerialPort SerPort;
        public byte[] tapArray;
        public byte[] emptyBuffer = new byte[] { };
        private byte[] buffer = new byte[512];
        private CCTCrc16 crc16;
        private CCTCrc8 crc8;

        private static string _mode;
        private static bool _VirtualPortInUse;
        private static volatile bool _PortIsDisapiered;
        private static SerialPortManager.SerialPortManager spm = SerialPortManager.SerialPortManager.Instance;

        private static volatile CCTCommunication instance;

        public static void SetMode(string mode)
        {
            _mode = mode;
        }

        private int CCTalkBaudrate
        {
            get
            {
                return _baudrate;
            }
            set
            {
                _baudrate = value;
            }
        }

        public static CCTCommunication Instance
        {
            get
            {
                if (CCTCommunication.instance == null)
                    CCTCommunication.instance = new CCTCommunication();
                return CCTCommunication.instance;
            }
        }

        private CCTCommunication()
        {
            Log.Debug("Starting CCTCommunication ...");
            tapArray = new byte[] { 7, 4, 5, 3, 1, 2, 3, 2, 6, 1 };
            secArray = new byte[6];
            emptyBuffer = new byte[] { };
            buffer = new byte[256];
            crc8 = new CCTCrc8();
            crc16 = new CCTCrc16();
            _baudrate = 9600;
            readWriteLock = new Object();

            // ReadComPort();
            InitSerialPort();
        }

        //static CCTCommunication()
        //{
        //}

        private void CalculateCRCChecksum(ref byte[] sendbuffer)
        {
            ushort ush = 0;
            byte[] bArr = new byte[(sendbuffer.Length - 2)];
            bArr[0] = sendbuffer[0];
            bArr[1] = sendbuffer[1];
            for (int i1 = 0; i1 < (bArr.Length - 2); i1++)
            {
                bArr[2 + i1] = sendbuffer[3 + i1];
            }
            for (int i2 = 0; i2 < bArr.Length; i2++)
            {
                ush ^= (ushort)(bArr[i2] << 8);
                for (int i3 = 0; i3 < 8; i3++)
                {
                    if ((ush & 32768) != 0)
                        ush = (ushort)((ush << 1) ^ (ushort)4129);
                    else
                        ush <<= 1;
                }
            }
            sendbuffer[2] = (byte)(ush & 255);
            sendbuffer[sendbuffer.Length - 1] = (byte)(ush >> 8);
        }


        private void CalculateSimpleChecksumForSend(ref byte[] sendBuffer)
        {
            byte b1 = 0;
            byte[] bArr = sendBuffer;
            for (int i = 0; i < bArr.Length; i++)
            {
                byte b2 = bArr[i];
                b1 += b2;
            }
            b1 = (byte)(256 - b1);
            sendBuffer[sendBuffer.Length - 1] = b1;
        }

        public bool CheckAcceptorPresent(string acceptorType, byte CCTalkAddress, ref bool useCRCChecksum, byte[] encrytionKey)
        {
            bool flag1;
            byte[] bArr2;

            byte[] bArr3 = new byte[1];
            byte[] bArr1 = bArr3;
            string s1 = null;
            bool flag2 = false;
            string s2 = null;
            ASCIIEncoding asciiencoding = new ASCIIEncoding();
            useCRCChecksum = false;
            CCTCommunication.Instance.SendCCTalkCommand(1, CCTalkAddress, 245, true, bArr1, out flag1, out bArr2, out s1, false, useCRCChecksum, encrytionKey);
            // CCTCommunication.Instance.SendCCTalkCommand(CCTalkAddress, 245, bArr1, 5000, out bArr2, CCTMessageType.StandardCRC8, encrytionKey);
            if (bArr2 != null)
            {
                s2 = asciiencoding.GetString(bArr2);
                if (s2 == acceptorType)
                    return true;
            }
            useCRCChecksum = true;
            CCTCommunication.Instance.SendCCTalkCommand(1, CCTalkAddress, 245, true, bArr1, out flag1, out bArr2, out s1, false, useCRCChecksum, encrytionKey);
            if (bArr2 != null)
            {
                s2 = asciiencoding.GetString(bArr2);
                if (s2 == acceptorType)
                    return true;
            }
            return flag2;
        }

        public bool CheckAcceptorPresentEncrypted(string acceptorType, byte address, ref bool useCRCChecksum, ref bool useEncryption, byte[] encrytionKey)
        {
            //for (int i = 0; i <= 256; i++)
            //{
            bool flag1;
            byte[] bArr2;
            string s1 = null;
            ASCIIEncoding asciiencoding = new ASCIIEncoding();
            byte[] bArr3 = new byte[0];
            byte[] bArr1 = bArr3;
            string s2 = null;
            useCRCChecksum = true;
            useEncryption = true;
            // address = (byte)i;
            int timeOut = 10;
            //CCTCommunication.Instance.SendCCTalkCommand(address, 245, bArr1, timeOut, out bArr2, CCTMessageType.EncryptedCRC16, encrytionKey);
            CCTCommunication.Instance.SendCCTalkCommand(1, address, 245, true, bArr1, out flag1, out bArr2, out s1, useEncryption, useCRCChecksum, encrytionKey);
            if (bArr2 != null)
            {
                s2 = asciiencoding.GetString(bArr2);
                if (s2 == acceptorType)
                {
                    useEncryption = true;
                    useCRCChecksum = true;
                    return true;
                }
            }
            useCRCChecksum = true;
            useEncryption = false;
            CCTCommunication.Instance.SendCCTalkCommand(address, 245, bArr1, timeOut, out bArr2, CCTMessageType.StandardCRC16, encrytionKey);
            // CCTCommunication.Instance.SendCCTalkCommand(1, address, 245, true, bArr1, out flag1, out bArr2, out s1, useEncryption, useCRCChecksum, encrytionKey);
            if (bArr2 != null)
            {
                s2 = asciiencoding.GetString(bArr2);
                if (s2 == acceptorType)
                {
                    useEncryption = false;
                    useCRCChecksum = true;
                    return true;
                }
            }
            useCRCChecksum = false;
            useEncryption = false;
            CCTCommunication.Instance.SendCCTalkCommand(address, 245, bArr1, timeOut, out bArr2, CCTMessageType.StandardCRC8, encrytionKey);
            // CCTCommunication.Instance.SendCCTalkCommand(1, address, 245, true, bArr1, out flag1, out bArr2, out s1, useEncryption, useCRCChecksum, encrytionKey);
            if (bArr2 != null)
            {
                s2 = asciiencoding.GetString(bArr2);
                if (s2 == acceptorType)
                {
                    useEncryption = false;
                    useCRCChecksum = false;
                    return true;
                }
            }
            useEncryption = true;
            useCRCChecksum = false;
            CCTCommunication.Instance.SendCCTalkCommand(address, 245, bArr1, timeOut, out bArr2, CCTMessageType.EncryptedCRC8, encrytionKey);
            //CCTCommunication.Instance.SendCCTalkCommand(1, address, 245, true, bArr1, out flag1, out bArr2, out s1, useEncryption, useCRCChecksum, encrytionKey);
            if (bArr2 != null)
            {
                s2 = asciiencoding.GetString(bArr2);
                if (s2 == acceptorType)
                {
                    useEncryption = true;
                    useCRCChecksum = false;
                    return true;
                }
            }
            //// useEncryption = true;
            //CCTCommunication.Instance.SendCCTalkCommand(address, 245, bArr1, 5000, out bArr2, CCTMessageType.StandardCRC16, encrytionKey);
            //// CCTCommunication.Instance.SendCCTalkCommand(1, address, 245, true, bArr1, out flag1, out bArr2, out s1, useEncryption, useCRCChecksum);
            //if (bArr2 != null)
            //{
            //    s2 = asciiencoding.GetString(bArr2);
            //    if (s2 == acceptorType)
            //    {
            //        useEncryption = false;
            //        useCRCChecksum = true;
            //        return true;
            //    }
            //}
            ////useCRCChecksum = true;
            ////useEncryption = false;
            //CCTCommunication.Instance.SendCCTalkCommand(address, 245, bArr1, timeOut, out bArr2, CCTMessageType.EncryptedCRC16, encrytionKey);
            //// CCTCommunication.Instance.SendCCTalkCommand(1, address, 245, true, bArr1, out flag1, out bArr2, out s1, useEncryption, useCRCChecksum);
            //if (bArr2 != null)
            //{
            //    s2 = asciiencoding.GetString(bArr2);
            //    if (s2 == acceptorType)
            //    {
            //        useEncryption = true;
            //        useCRCChecksum = true;
            //        return true;
            //    }
            //}
            //useEncryption = true;
            //CCTCommunication.Instance.SendCCTalkCommand(1, address, 245, true, bArr1, out flag1, out bArr2, out s1, useEncryption, useCRCChecksum);
            //if (bArr2 != null)
            //{
            //    s2 = asciiencoding.GetString(bArr2);
            //    if (s2 == acceptorType)
            //        return true;
            //}
            //}
            return false;
        }

        private bool CheckCRCChecksum (byte[] receivebuffer)
        {
            bool flag = false;
            ushort ush = 0;
            byte[] bArr = new byte[(receivebuffer.Length - 2)];
            bArr[0] = receivebuffer[0];
            bArr[1] = receivebuffer[1];
            for (int i1 = 0; i1 < (bArr.Length - 2); i1++)
            {
                bArr[2 + i1] = receivebuffer[3 + i1];
            }
            for (int i2 = 0; i2 < bArr.Length; i2++)
            {
                ush ^= (ushort)(bArr[i2] << 8);
                for (int i3 = 0; i3 < 8; i3++)
                {
                    if ((ush & 32768) != 0)
                        ush = (ushort)((ush << 1) ^ (ushort)4129);
                    else
                        ush <<= 1;
                }
            }
            if ((receivebuffer[2] == (byte)(ush & 255)) && (receivebuffer[receivebuffer.Length - 1] == (byte)(ush >> 8)))
                flag = true;
            return flag;
        }

        private bool CheckSimpleChecksumForReceive(byte[] receivebuffer)
        {
            byte b1 = 0;
            bool flag = false;
            byte[] bArr = receivebuffer;
            for (int i = 0; i < bArr.Length; i++)
            {
                byte b2 = bArr[i];
                b1 += b2;
            }
            if (b1 == 0)
                flag = true;
            return flag;
        }

        //private void DecryptCcTalk(ref byte[] buffer)
        //{
        //    int i1, i2;

        //    int i6 = buffer.Length;
        //    byte b = (byte)((16 * secArray[2]) + secArray[2]);
        //    for (i1 = 0; i1 < i6; i1++)
        //    {
        //        buffer[i1] = (byte)(buffer[i1] ^ b);
        //    }
        //    for (i1 = 11; i1 >= 0; i1--)
        //    {
        //        int i5 = 0;
        //        if ((buffer[0] & 128) != null)
        //        {
        //            i5 = 1;
        //        }

        //        for (i2 = 0; i2 < i6; i2++)
        //        {
        //            if ((buffer[i2] & (1 << ((tapArray[(secArray[1] + i2) % 10] - 1) & 31))) != null)
        //                i5 ^= 1;
        //        }
        //        for (i2 = i6 - 1; i2 >= 0; i2--)
        //        {
        //            int i4 = (buffer[i2] & 128) != null ? 1 : 0;
        //            if (((secArray[5] ^ 99) & (1 << (((i1 + i2 - 1) % 8) & 31))) != null)
        //                i4 ^= 1;
        //            buffer[i2] = (byte)((buffer[i2] << 1) + i5);
        //            i5 = i4;
        //        }
        //    }
        //    for (i1 = 0; i1 < i6; i1++)
        //    {
        //        if ((secArray[3] & (1 << ((i1 % 4) & 31))) != null)
        //        {
        //            int i3 = 0;
        //            for (i2 = 0; i2 < 8; i2++)
        //            {
        //                if ((buffer[i1] & (1 << (i2 & 31))) != null)
        //                    i3 += 128 >> (i2 & 31);
        //            }
        //            buffer[i1] = (byte)i3;
        //        }
        //    }
        //    b = (byte)~((16 * secArray[0]) + secArray[4]);
        //    for (i1 = 0; i1 < i6; i1++)
        //    {
        //        buffer[i1] = (byte)(buffer[i1] ^ b);
        //    }
        //}

        //private void EncryptCcTalk(ref byte[] buffer)
        //{
        //    int i1, i2;

        //    int i6 = buffer.Length;
        //    byte b = (byte)~((16 * secArray[0]) + secArray[4]);
        //    for (i1 = 0; i1 < i6; i1++)
        //    {
        //        buffer[i1] = (byte)(buffer[i1] ^ b);
        //    }
        //    for (i1 = 0; i1 < i6; i1++)
        //    {
        //        if ((secArray[3] & (1 << ((i1 % 4) & 31))) != null)
        //        {
        //            int i3 = 0;
        //            for (i2 = 0; i2 < 8; i2++)
        //            {
        //                if ((buffer[i1] & (1 << (i2 & 31))) != null)
        //                    i3 += 128 >> (i2 & 31);
        //            }
        //            buffer[i1] = (byte)i3;
        //        }
        //    }
        //    for (i1 = 0; i1 < 12; i1++)
        //    {
        //        int i5 = (buffer[i6 - 1] & 1) != null ? 128 : 0;
        //        for (i2 = 0; i2 < i6; i2++)
        //        {
        //            if ((buffer[i2] & (1 << (tapArray[(secArray[1] + i2) % 10] & 31))) != null)
        //                i5 ^= 128;
        //        }
        //        for (i2 = 0; i2 < i6; i2++)
        //        {
        //            int i4 = (buffer[i2] & 1) != null ? 128 : 0;
        //            if (((secArray[5] ^ 99) & (1 << (((i1 + i2) % 8) & 31))) != null)
        //                i4 ^= 128;
        //            buffer[i2] = (byte)((buffer[i2] >> 1) + i5);
        //            i5 = i4;
        //        }
        //    }
        //    b = (byte)((16 * secArray[2]) + secArray[2]);
        //    for (i1 = 0; i1 < i6; i1++)
        //    {
        //        buffer[i1] = (byte)(buffer[i1] ^ b);
        //    }
        //}

        public bool FindAcceptor(string acceptorType, ref byte CCTalkAddress, ref bool useCRCChecksum, byte[] encryptionCode)
        {
            bool flag1;
            byte[] bArr2;

            byte[] bArr3 = new byte[1];
            byte[] bArr1 = bArr3;
            string s1 = null;
            bool flag2 = false;
            string s2 = null;
            ASCIIEncoding asciiencoding = new ASCIIEncoding();
            useCRCChecksum = false;
            flag2 = CCTCommunication.Instance.SendCCTalkCommand(1, 0, 253, true, bArr1, out flag1, out bArr2, out s1, false, useCRCChecksum, encryptionCode);
            // flag2 = CCTCommunication.Instance.SendCCTalkCommand(1, 253, bArr1, 5000, out bArr2, CCTMessageType.StandardCRC8, encryptionCode);
            try
            {

          
                if (flag2)
                {
                    byte[] bArr4 = bArr2;
                    for (int i1 = 0; i1 < bArr4.Length; i1++)
                    {
                        byte b1 = bArr4[i1];
                        CCTCommunication.Instance.SendCCTalkCommand(1, b1, 245, true, bArr1, out flag1, out bArr2, out s1, false, useCRCChecksum, encryptionCode);
                        s2 = asciiencoding.GetString(bArr2);
                        if (s2 == acceptorType)
                        {
                            CCTalkAddress = b1;
                            return true;
                        }
                    }
                }
                useCRCChecksum = true;
                flag2 = CCTCommunication.Instance.SendCCTalkCommand(1, 0, 253, true, bArr1, out flag1, out bArr2, out s1, true, useCRCChecksum, encryptionCode);
                // flag2 = CCTCommunication.Instance.SendCCTalkCommand(1, 253, bArr1, 5000, out bArr2, CCTMessageType.StandardCRC16, encryptionCode);
                if (flag2)
                {
                    byte[] bArr5 = bArr2;
                    for (int i2 = 0; i2 < bArr5.Length; i2++)
                    {
                        byte b2 = bArr5[i2];
                        CCTCommunication.Instance.SendCCTalkCommand(1, b2, 245, true, bArr1, out flag1, out bArr2, out s1, true, useCRCChecksum, encryptionCode);
                        s2 = asciiencoding.GetString(bArr2);
                        if (s2 == acceptorType)
                        {
                            CCTalkAddress = b2;
                            return true;
                        }
                    }
                }
                useCRCChecksum = false;
                flag2 = CCTCommunication.Instance.SendCCTalkCommand(1, 0, 253, true, bArr1, out flag1, out bArr2, out s1, true, useCRCChecksum, encryptionCode);
                // flag2 = CCTCommunication.Instance.SendCCTalkCommand(1, 253, bArr1, 5000, out bArr2, CCTMessageType.EncryptedCRC16, encryptionCode, encryptionCode);
                if (flag2)
                {
                    byte[] bArr5 = bArr2;
                    for (int i2 = 0; i2 < bArr5.Length; i2++)
                    {
                        byte b2 = bArr5[i2];
                        CCTCommunication.Instance.SendCCTalkCommand(1, b2, 245, true, bArr1, out flag1, out bArr2, out s1, true, useCRCChecksum, encryptionCode);
                        s2 = asciiencoding.GetString(bArr2);
                        if (s2 == acceptorType)
                        {
                            CCTalkAddress = b2;
                            return true;
                        }
                    }
                }
                useCRCChecksum = true;
                flag2 = CCTCommunication.Instance.SendCCTalkCommand(1, 0, 253, true, bArr1, out flag1, out bArr2, out s1, false, useCRCChecksum, encryptionCode);
                // flag2 = CCTCommunication.Instance.SendCCTalkCommand(1, 253, bArr1, 5000, out bArr2, CCTMessageType.EncryptedCRC8, encryptionCode);
                if (flag2)
                {
                    byte[] bArr5 = bArr2;
                    for (int i2 = 0; i2 < bArr5.Length; i2++)
                    {
                        byte b2 = bArr5[i2];
                        CCTCommunication.Instance.SendCCTalkCommand(1, b2, 245, true, bArr1, out flag1, out bArr2, out s1, false, useCRCChecksum, encryptionCode);
                        s2 = asciiencoding.GetString(bArr2);
                        if (s2 == acceptorType)
                        {
                            CCTalkAddress = b2;
                            return true;
                        }
                    }
                }
                return flag2;
            }
            catch (Exception)
            {

                return false;
            }
        }

        private void InitSerialPort()
        {
            var port = GetVirtualComPort();

            try
            {
                if (string.IsNullOrEmpty(port))
                {
                    Log.Error("Cant find cashacceptor port name");
                    return;
                }
                SerPort = new SerialPort (port, CCTalkBaudrate, Parity.None, 8, StopBits.One);

                SerPort.ReadTimeout = 2000;
                SerPort.WriteTimeout = 2000;
                SerPort.Open();
                SerPort.ErrorReceived += new SerialErrorReceivedEventHandler (SerPort_ErrorReceived);
            }
            catch (Exception ex)
            {

                Log.Error("Cant open " + port + ". " + ex.Message + ". " + ex.StackTrace);
            }

        }

        public static string GetVirtualComPort()
        {

            List<CommunicationResource> sp = spm.GetSafeSerialPortsMap();
            bool PfysicalValidatorIsConnected = false;

            string coin_port = null;
            foreach (CommunicationResource cr in sp)
            {
                if ( cr.ProtocolType == DeviceProtocol.PROTOCOL_CCTALK )
                {
                    if (cr.PortType == ResourceType.BILL_VALIDATOR_SERIAL_PORT)
                    {
                        return cr.PortName;
                    }
                    if (cr.PortType == ResourceType.COIN_ACCEPTOR_SERIAL_PORT)
                    {
                        coin_port = cr.PortName;
                    }
                }
                else if (cr.ProtocolType == DeviceProtocol.PROTOCOL_ID003)
                {
                    PfysicalValidatorIsConnected = true;
                }
                else if (cr.ProtocolType == DeviceProtocol.PROTOCOL_SSP)
                {
                    PfysicalValidatorIsConnected = true;
                }

            }

            if (coin_port != null)
            {
                return coin_port;
            }


            if (_mode == "TestMode" && PfysicalValidatorIsConnected == false)
            {
                string com = GetEmulatorComPort();
                if (!String.IsNullOrEmpty(com))
                {
                    int pos = com.IndexOf('-');
                    if (pos != -1)
                    {
                        com = com.Substring(0, pos);
                        _VirtualPortInUse = true;
                    }
                    return com;
                }
            }

            return string.Empty;
        }

         public bool CheckIfPortExists()
        {
            bool result = false;
            if (SerPort != null)
            {
                result = SerialPort.GetPortNames().Any(x => x == SerPort.PortName);
            }

            return result;
        }
        private static bool Checkmode()
        {
            bool  result = true;

            if (_VirtualPortInUse)
            {
                if (_mode != "TestMode")
                {
                    result = false;
                }
            }
            return result;
        }

        public static string GetEmulatorComPort()
        {

            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0 and Caption like 'ELTIMA Virtual%'");
            ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
            ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");
            ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);

            using (comPortSearcher)
            {
                string caption = null;
                foreach (ManagementObject obj in comPortSearcher.Get())
                {
                    if (obj != null)
                    {
                        object captionObj = obj["Caption"];
                        if (captionObj != null)
                        {
                            caption = captionObj.ToString();
                            if (caption.Contains("(COM"))
                            {
                                return caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")", string.Empty);
                            }
                        }
                    }
                }
            }

            return null;
        }

        //private void ReadComPort()
        //{
        //    try
        //    {
        //        CCTalkCOMPort = _pref.GetStringEntry(_COMPORTSTR);
        //    }
        //    catch (Exception Ex)
        //    {
        //        _logger.Error("In ReadComPort.", Ex);
        //    }

        //}

        public bool SendCCTalkCommand(byte srcAddress, byte destAddress, byte commandHeader, bool noParamBytes, byte[] paramBuffer, out bool noRxDataBytes, out byte[] rxDataBuffer, out string commsStatus, bool encrypted, bool useCRCChecksum, byte[] secArray)
        {
            bool flag1 = false, flag2 = false;
            int i1 = 0, i2 = 0;
            byte[] bArr1 = new byte[256];
            noRxDataBytes = true;
            rxDataBuffer = null;
            commsStatus = "";
            byte[] bArr2 = null;
            byte[] bArr3 = new byte[256];
            byte[] bArr4 = null;
            try 
            {
                if (!Checkmode())
                {
                    return false;
                }

                if (_PortIsDisapiered)
                {
                    if (CheckIfPortExists())
                    {
                        _PortIsDisapiered = false;
                    }
                    else
                    {
                        return false;
                    }
                }

                lock (readWriteLock)
                {
                    if (noParamBytes)
                        bArr2 = new byte[5];
                    else
                        bArr2 = new byte[(5 + paramBuffer.Length)];
                    bArr2[0] = destAddress;
                    if (noParamBytes)
                        bArr2[1] = 0;
                    else
                        bArr2[1] = (byte)paramBuffer.Length;
                    bArr2[2] = srcAddress;
                    bArr2[3] = commandHeader;
                    if (!noParamBytes)
                    {
                        for (int i3 = 0; i3 < paramBuffer.Length; i3++)
                        {
                            bArr2[i3 + 4] = paramBuffer[i3];
                        }
                        bArr2[3 + paramBuffer.Length + 1] = 0;
                    }
                    else
                    {
                        bArr2[4] = 0;
                    }
                    if (encrypted)
                    {
                        // byte[] bArr5 = new byte[(bArr2.Length - 2)];

                        //new added
                        int leng1 = 5;
                        int parambufferleng = 0;
                        if (paramBuffer != null)
                        {
                            parambufferleng = paramBuffer.Length;
                        }
                        leng1 = 5 + parambufferleng;
                        CCTCrc16 crc16 = new CCTCrc16();
                        crc16.Reset();
                        crc16.PushByte(bArr2[0]);
                        crc16.PushByte(bArr2[1]);
                        crc16.PushData(bArr2, 3, leng1 - 4);
                        bArr2[2] = (byte)(crc16.CRC & 255);

                        if (noParamBytes)
                        {
                            bArr2[4] = (byte)(crc16.CRC >> 8);
                        }
                        else if (4 + parambufferleng + 1 <= bArr2.Length)
                        {
                            bArr2[4 + parambufferleng] = (byte)(crc16.CRC >> 8);
                        }
                        else
                        {
                            Log.Error("CRC16-Encryption-Error. Buffer:" + bArr2.ToString());
                            flag1 = false; flag2 = false;
                            return false;
                        }

                        // CalculateCRCChecksum(ref bArr2);
                        // CalculateSimpleChecksumForSend(ref bArr2);
                        //for (int i4 = 0; i4 < bArr5.Length; i4++)
                        //{
                        //    bArr5[i4] = bArr2[i4 + 2];
                        //}

                        //int leng = 5;
                        //if (paramBuffer != null)
                        //{
                        //    leng = 5 + paramBuffer.Length;
                        //}
                        CCTCrypt.Encrypt(bArr2, 2, bArr2.Length - 2, secArray);
                        // CCTCrypt.Encrypt(bArr2, 2, leng - 2, secArray);
                        //EncryptCcTalk(ref bArr5);
                        //for (int i5 = 0; i5 < bArr5.Length; i5++)
                        //{
                        //    bArr2[i5 + 2] = bArr5[i5];
                        //}
                        // bArr5 = null;
                    }
                    else if (useCRCChecksum)
                    {
                        CalculateCRCChecksum(ref bArr2);
                    }
                    else
                    {
                        CalculateSimpleChecksumForSend(ref bArr2);
                    }

                    try
                    {
                        if (SerPort != null)
                        {

                            SerPort.DiscardInBuffer();
                            SerPort.Write(bArr2, 0, bArr2.Length);
                            if ((commandHeader == 253) || (commandHeader == 252))
                                Thread.Sleep(1500);
                            else
                                Thread.Sleep(100);

                            i1 = i2 = 0;
                            while (SerPort.BytesToRead != 0)
                            {
                                byte readbyte = (byte)SerPort.ReadByte();
                                if (i1 < bArr1.Length)
                                {
                                    bArr1[i1] = readbyte;
                                    if (i1 >= bArr2.Length)
                                    {
                                        bArr3[i2] = bArr1[i1];
                                        flag2 = true;
                                        i2++;
                                    }
                                }
                                else
                                {
                                    Log.Error("Resultbuffer <  bytes read from comPort.");
                                }
                                i1++;
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("", e);
                        _PortIsDisapiered = !CheckIfPortExists();
                        return false;
                    }




                    if (flag2)
                    {
                        flag1 = true;
                        if ((commandHeader == 253) || (commandHeader == 252))
                        {
                            noRxDataBytes = false;
                            rxDataBuffer = new byte[i2];
                            for (int i6 = 0; i6 < i2; i6++)
                            {
                                rxDataBuffer[i6] = bArr3[i6];
                            }
                        }
                        else
                        {
                            bArr4 = new byte[i2];
                            for (int i7 = 0; i7 < i2; i7++)
                            {
                                bArr4[i7] = bArr3[i7];
                            }
                            bool flag3 = false;
                            if (encrypted)
                            {
                                byte[] bArr6 = new byte[(bArr4.Length - 2)];
                                for (int i8 = 0; i8 < bArr6.Length; i8++)
                                {
                                    bArr6[i8] = bArr4[i8 + 2];
                                }
                                //DecryptCcTalk(ref bArr6);
                                //for (int i9 = 0; i9 < bArr6.Length; i9++)
                                //{
                                //    bArr4[i9 + 2] = bArr6[i9];
                                //}
                                bArr6 = null;
                                CCTCrypt.Decrypt(bArr4, 2, bArr4.Length - 2, secArray);
                                // CCTCrypt.Decrypt(bArr4, 2, i2 - 2, secArray);
                                crc16.Reset();
                                crc16.PushByte(bArr4[0]);
                                crc16.PushByte(bArr4[1]);
                                crc16.PushData(bArr4, 3, i2 - 4);
                                flag3 = (bArr4[2] + (bArr4[i2 - 1] << 8)) == crc16.CRC;
                                // flag3 = CheckCRCChecksum(bArr4);
                                // flag3 = CheckSimpleChecksumForReceive(bArr4);
                            }
                            else if (useCRCChecksum)
                            {
                                flag3 = CheckCRCChecksum(bArr4);
                            }
                            else
                            {
                                flag3 = CheckSimpleChecksumForReceive(bArr4);
                            }

                            if (!flag3)
                            {
                                commsStatus = "Checksum Error at Receive";
                                flag1 = false;
                            }
                            else if (bArr4[1] != 0)
                            {
                                noRxDataBytes = false;
                                rxDataBuffer = new byte[bArr4[1]];
                                for (int i10 = 0; i10 < bArr4[1]; i10++)
                                {
                                    rxDataBuffer[i10] = bArr4[i10 + 4];
                                }
                            }
                        }
                    }
                    else
                    {
                        commsStatus = "No Data received";
                        flag1 = false;
                    }
                }
            }
            catch (Exception Ex)
            {
                Log.Error("In SendCCTalkCommand.", Ex);
                _PortIsDisapiered = !CheckIfPortExists();
                return false;
            }
            return flag1;
        }

        public bool SendCCTalkCommand(byte address, byte command, byte[] data, int timeOut, out byte[] response, CCTMessageType msgType, byte[] encryptionCode)
        {
            if (data.Length > 252)
                throw new Exception("to many bytes in ccTalk data");
            bool flag1 = false;
            response = emptyBuffer;

            if (!Checkmode())
            {
                return false;
            }
            lock (this)
            {
                int i1 = 4 + data.Length + 1;
                buffer[0] = address;
                buffer[1] = (byte)data.Length;
                buffer[2] = 1;
                buffer[3] = command;
                data.CopyTo(buffer, 4);
                switch (msgType)
                {
                    case CCTMessageType.NoneCRC:
                        break;
                    case CCTMessageType.EncryptedNoneCRC:
                        CCTCrypt.Encrypt(buffer, 2, i1 - 2, encryptionCode);
                        break;
                    case CCTMessageType.StandardCRC8:
                        crc8.Reset();
                        crc8.PushData(buffer, 0, i1 - 1);
                        buffer[4 + data.Length] = crc8.CRC;
                        break;
                    case CCTMessageType.EncryptedCRC8:
                        crc8.Reset();
                        crc8.PushData(buffer, 0, i1 - 1);
                        buffer[4 + data.Length] = crc8.CRC;
                        CCTCrypt.Encrypt(buffer, 2, i1 - 2, encryptionCode);
                        break;
                    case CCTMessageType.StandardCRC16:
                        crc16.Reset();
                        crc16.PushByte(buffer[0]);
                        crc16.PushByte(buffer[1]);
                        crc16.PushData(buffer, 3, i1 - 4);
                        buffer[2] = (byte)(crc16.CRC & 255);
                        buffer[4 + data.Length] = (byte)(crc16.CRC >> 8);
                        break;

                    case CCTMessageType.EncryptedCRC16:
                        crc16.Reset();
                        crc16.PushByte(buffer[0]);
                        crc16.PushByte(buffer[1]);
                        crc16.PushData(buffer, 3, i1 - 4);
                        buffer[2] = (byte)(crc16.CRC & 255);
                        buffer[4 + data.Length] = (byte)(crc16.CRC >> 8);
                        CCTCrypt.Encrypt(buffer, 2, i1 - 2, encryptionCode);
                        break;
                }
                Thread.Sleep(20);

                if (SerPort == null) 
                    return false;
                SerPort.DiscardInBuffer();
                SerPort.Write(buffer, 0, i1);

                if ((command == 253) || (command == 252))
                    Thread.Sleep(1500);
                else
                    Thread.Sleep(100);

                bool flag2 = false, flag3 = false;
                int i2 = 0, i3 = 0;
                CCTPerformanceTimer performanceTimer = new CCTPerformanceTimer();
                performanceTimer.Start();
                while (!flag2)
                {
                    try
                    {
                        Thread.Sleep(20);
                        while (SerPort.BytesToRead > 0)
                        {
                            if (flag3)
                            {
                                TimeSpan timeSpan1 = performanceTimer.TimeSpan;
                                timeOut = (int)timeSpan1.TotalMilliseconds + 50;
                            }
                            buffer[i2++] = (byte)SerPort.ReadByte();
                            i3 = i2 >= 3 ? 5 + buffer[1] : -1;
                            if (i2 == i3)
                            {
                                if (buffer[0] == address)
                                {
                                    i2 = 0;
                                    flag3 = true;
                                }
                                if (buffer[0] == 1)
                                {
                                    flag2 = true;
                                    switch (msgType)
                                    {
                                        case CCTMessageType.NoneCRC:
                                            break;
                                        case CCTMessageType.EncryptedNoneCRC:
                                            CCTCrypt.Decrypt(buffer, 2, i2 - 2, encryptionCode);
                                            break;
                                        case CCTMessageType.StandardCRC8:
                                            crc8.Reset();
                                            crc8.PushData(buffer, 0, i2 - 1);
                                            flag1 = buffer[i2 - 1] == crc8.CRC;
                                            break;
                                        case CCTMessageType.EncryptedCRC8:
                                            CCTCrypt.Decrypt(buffer, 2, i2 - 2, encryptionCode);
                                            crc8.Reset();
                                            crc8.PushData(buffer, 0, i2 - 1);
                                            flag1 = buffer[i2 - 1] == crc8.CRC;
                                            break;
                                        case CCTMessageType.StandardCRC16:
                                            crc16.Reset();
                                            crc16.PushByte(buffer[0]);
                                            crc16.PushByte(buffer[1]);
                                            crc16.PushData(buffer, 3, i2 - 4);
                                            flag1 = (buffer[2] + (buffer[i2 - 1] << 8)) == crc16.CRC;
                                            break;

                                        case CCTMessageType.EncryptedCRC16:
                                            CCTCrypt.Decrypt(buffer, 2, i2 - 2, encryptionCode);
                                            crc16.Reset();
                                            crc16.PushByte(buffer[0]);
                                            crc16.PushByte(buffer[1]);
                                            crc16.PushData(buffer, 3, i2 - 4);
                                            flag1 = (buffer[2] + (buffer[i2 - 1] << 8)) == crc16.CRC;
                                            break;
                                    }
                                    if (flag1)
                                    {
                                        response = new byte[buffer[1]];
                                        Array.Copy(buffer, 4, response, 0, response.Length);
                                    }
                                }
                            }
                        }
                        TimeSpan timeSpan2 = performanceTimer.TimeSpan;
                        if (timeSpan2.TotalMilliseconds > (double)timeOut)
                            flag2 = true;

                    }
                    catch (Exception ex)
                    {
                        Log.Error("", ex);
                    }
                }


            }
            response = new byte[buffer[1]];
            Array.Copy(buffer, 4, response, 0, response.Length);
            return flag1;
        }

        private void SerPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            string s = "";
            switch (e.EventType)
            {
                case SerialError.Frame:
                    s = "Frame Error";
                    break;

                case SerialError.Overrun:
                    s = "Overrun";
                    break;

                case SerialError.RXOver:
                    s = "RXOver";
                    break;

                case SerialError.RXParity:
                    s = "RXParity";
                    break;

                case SerialError.TXFull:
                    s = "TXFull";
                    break;

                case SerialError.RXOver | SerialError.Overrun:
                    Log.Error("Error at CCTalk communication on serial port:" + _COMPort + " Error:" + s);
                    break;
            }
        }

    } // class CCTCommunication
    internal class ProcessConnection
    {

        public static ConnectionOptions ProcessConnectionOptions()
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;
            options.Authentication = AuthenticationLevel.Default;
            options.EnablePrivileges = true;
            return options;
        }

        public static ManagementScope ConnectionScope(string machineName, ConnectionOptions options, string path)
        {
            ManagementScope connectScope = new ManagementScope();
            connectScope.Path = new ManagementPath(@"\\" + machineName + path);
            connectScope.Options = options;
            connectScope.Connect();
            return connectScope;
        }
    }
}

