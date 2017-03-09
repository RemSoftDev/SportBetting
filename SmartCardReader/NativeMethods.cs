using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SmartCardReader
{
    struct SCARD_IO_REQUEST
    {
        public int dwProtocol;
        public int cbPciLength;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    class PCSC_SCMC
    {
        public IntPtr hContext;
        public IntPtr hCard;

        public uint ret;

        public int dwActReader;
        public int dwReaderCount;
        
        public string ReaderName;

        public SCARD_IO_REQUEST IO_Request;
        public int dwCardStatus;
        public uint dwActProtocol;

      // [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 260)]
        public byte[] brSendBuf = new byte [260];
        public int dwSendLen;

      //  [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 260)]
        public byte[] brRecvBuf  = new byte [260];
        public uint dwRecvLen;

      
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    internal struct SCARD_READERSTATE
    {
        internal string szReader;
        internal IntPtr pvUserData;
        internal uint dwCurrentState;
        internal uint dwEventState;
        internal uint cbAtr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x24, ArraySubType=UnmanagedType.U1)]
        internal byte[] rgbAtr;
    }

    public class NativeMethods : IDisposable
    {

        [DllImport("winscard.dll", EntryPoint = "SCardEstablishContext", CharSet = CharSet.Ansi)]
        static extern uint SCardEstablishContext (uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext);
        [DllImport("winscard.dll", EntryPoint = "SCardReleaseContext", CharSet = CharSet.Ansi)]
        static extern uint SCardReleaseContext (IntPtr hContext);
        [DllImport("winscard.dll", EntryPoint = "SCardListReadersA", CharSet = CharSet.Ansi)]
        static extern uint SCardListReaders(IntPtr hContext, byte[] mszGroups, byte[] mszReaders, ref UInt32 pcchReaders);

        [DllImport("winscard.dll", EntryPoint = "SCardConnect", CharSet = CharSet.Ansi)]
        static extern uint SCardConnect (IntPtr hContext, string szReader, uint dwShareMode, uint dwPreferredProtocols, out IntPtr phCard, out uint pdwActiveProtocol);
        [DllImport("winscard.dll", EntryPoint = "SCardDisconnect", CharSet = CharSet.Ansi)]
        static extern uint SCardDisconnect(IntPtr hCard, uint dwDisposition);
        [DllImport("winscard.dll", EntryPoint = "SCardTransmit", CharSet = CharSet.Ansi)]
        static extern uint SCardTransmit(IntPtr hCard, ref SCARD_IO_REQUEST pioSendPci, [In] byte[] pbSendBuffer, uint cbSendLength, ref SCARD_IO_REQUEST pioRecvPci, [In, Out] byte[] pbRecvBuffer, ref uint pcbRecvLength);
        [DllImport("winscard.dll", EntryPoint = "SCardControl", CharSet = CharSet.Ansi)]
        public static extern uint SCardControl(IntPtr hCard, uint dwControlCode, [In] byte[] lpInBuffer, uint nInBufferSize, [In, Out] byte[] lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned);

        [DllImport("winscard.dll", EntryPoint = "SCardStatus", CharSet = CharSet.Ansi)]
        static extern uint SCardStatus (IntPtr hCard, StringBuilder szReaderName, ref uint pcchReaderLen, out uint pdwState, out uint pdwProtocol, [Out] byte[] pbAtr, ref uint pcbAtrLen);
        [DllImport("winscard.dll", EntryPoint = "SCardCancel", CharSet = CharSet.Ansi)]
        static extern uint SCardCancel(IntPtr hContext);

        [DllImport("winscard.dll", EntryPoint = "SCardGetStatusChange", CharSet = CharSet.Ansi)]
        static extern uint SCardGetStatusChange (IntPtr hContext, uint dwTimeout, [In, Out] SCARD_READERSTATE[] rgReaderStates, uint cReaders);


        private const uint SCARD_SCOPE_USER = 0;
        private const uint SCARD_SCOPE_SYSTEM = 2;
        private const uint SCARD_E_UNEXPECTED = 0x8010001F;
        private PCSC_SCMC pcsc = new PCSC_SCMC ();
        private bool m_fDisposed;

        public NativeMethods ()
        {
            this.m_fDisposed = false;
            pcsc.hContext = IntPtr.Zero;
        }

        ~NativeMethods()
        {
            this._Dispose (false);
        }
        public void Dispose()
        {
            this._Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void _Dispose (bool fDisposing)
        {
            if (!this.m_fDisposed)
            {
                if (pcsc.hContext != IntPtr.Zero)
                {
                    SCardCancel (pcsc.hContext);
                    SCardReleaseContext (pcsc.hContext);
                    pcsc.hContext = IntPtr.Zero;
                }
                this.m_fDisposed = true;
            }
        }
        private uint ScardCtlCode (uint code)
        {
             const uint FILE_DEVICE_SMARTCARD = 0x31;
             const uint METHOD_BUFFERED = 0;
             const uint FILE_ANY_ACCESS = 0;

             return ((FILE_DEVICE_SMARTCARD << 16) | (FILE_ANY_ACCESS << 14) | (code << 2) | (METHOD_BUFFERED));
        }

        public uint SCReader_EstablishContext ()
        {
            uint result = SCARD_E_UNEXPECTED;
            try
            {
                result = SCardEstablishContext (SCARD_SCOPE_USER, IntPtr.Zero, IntPtr.Zero, out pcsc.hContext);
            }
            catch
            {
            }

            return result;
        }

        public uint SCReader_ReleaseContext ()
        {
            uint result = SCARD_E_UNEXPECTED;

            try
            {
                result = SCardReleaseContext (pcsc.hContext);
            }
            catch
            {
            }
            return result;
        }


        public uint SCReader_ReaderConnect ()
        {
            uint result = SCARD_E_UNEXPECTED;
            try
            {
                result = SCardConnect (pcsc.hContext, pcsc.ReaderName, 3, 0, out pcsc.hCard, out pcsc.dwActProtocol);
            }
            catch
            {
            }
            return result;
        }

        public uint SCReader_CardConnect()
        {
            uint result = SCARD_E_UNEXPECTED;
            const uint SCARD_PROTOCOL_T0 = 1;
            const uint SCARD_PROTOCOL_T1 = 2;
            const uint SCARD_PROTOCOL_SYNC = 4;
            try
            {
                result = SCardConnect(pcsc.hContext, pcsc.ReaderName, 2, SCARD_PROTOCOL_T0 | SCARD_PROTOCOL_T1 | SCARD_PROTOCOL_SYNC, out pcsc.hCard, out pcsc.dwActProtocol);
            }
            catch
            {
            }
            return result;
        }

        public uint SCReader_CardDisconnect ()
        {
            uint result = SCARD_E_UNEXPECTED;
            const uint SCARD_UNPOWER_CARD = 2;

            try
            {
                result = SCardDisconnect (pcsc.hCard, SCARD_UNPOWER_CARD);
            }
            catch
            {
            }
            return result;
        }

        public uint SCReader_ReaderDisconnect()
        {
            uint result = SCARD_E_UNEXPECTED;
            const uint SCARD_LEAVE_CARD = 0;

            try
            {
                result = SCardDisconnect (pcsc.hCard, SCARD_LEAVE_CARD);
            }
            catch
            {
            }
            return result;
        }

        public void SetActiveReader(string reader)
        {
            pcsc.ReaderName = reader;
        }

        public uint SCReader_SCardGetStatusChange()
        {
            uint result = SCARD_E_UNEXPECTED;
            try
            {
            }
            catch
            {
            }

            return result;
        }
        public uint SCReader_GetReaderInventory (out string chip_version, out string fw_version)
        {
            uint result = SCARD_E_UNEXPECTED;
            byte[] GET_CHIP_VERSION = new byte[] {0x08, 0};
            byte[] GET_FW_VERSION = new byte[] {0x02, 0};
            const uint IOCTL_SYSTEM = 0x880;
            uint ReturnedSize = 260;

            chip_version = "?";
            fw_version = "?";

            try
            {
                uint ControlCode = ScardCtlCode (IOCTL_SYSTEM);


                result = SCardControl (pcsc.hCard, ControlCode, GET_CHIP_VERSION, 2, pcsc.brRecvBuf, 260, out ReturnedSize);
                if (result == 0)
                {
                    if (ReturnedSize == 4 && pcsc.brRecvBuf[0] == 0x90 && pcsc.brRecvBuf[1] == 0x02)
                    {
                        if (pcsc.brRecvBuf[2] == 0x80)
                        {
                            chip_version = "STCII ";
                        }

                        if (pcsc.brRecvBuf[3] == 0x00)
                        {
                            chip_version += "A";
                        }
                        else if (pcsc.brRecvBuf[3] == 0x01)
                        {
                            chip_version += "B";
                        }
                        else
                        {
                            chip_version += "UNKNOWN";
                        }

                        result = SCardControl(pcsc.hCard, ControlCode, GET_FW_VERSION, 2, pcsc.brRecvBuf, 260, out ReturnedSize);
                        if (result == 0)
                        {
                            if (ReturnedSize != 4 || pcsc.brRecvBuf[0] != 0x90 || pcsc.brRecvBuf[1] != 0x02)
                            {
                                result = SCARD_E_UNEXPECTED;
                            }
                            else
                            {
                                fw_version = String.Format ("{0:X}x{1:X}", pcsc.brRecvBuf[2], pcsc.brRecvBuf[3]);
                            }
                        }
                    }
                }
                else
                {
                    result = SCARD_E_UNEXPECTED;
                }
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public List<string> SCReader_GetReaderNames()
        {
            List <string> readers =new List<string>(0);

            uint ret;
            uint pcchReaders = 0;

            try
            {
                 if (SCardListReaders (pcsc.hContext, null, null, ref pcchReaders) == 0)
                 {
                     byte[] mszReaders = new byte [pcchReaders];
                     ret = SCardListReaders (pcsc.hContext, null, mszReaders, ref pcchReaders);

                     ASCIIEncoding ascii = new ASCIIEncoding();

                     string currbuff = ascii.GetString(mszReaders);

                     int len = (int)pcchReaders;
                     int nullindex = -1;

                     if (len > 0)
                     {
                         while (currbuff [0] != (char)0)
                         {
                             nullindex = currbuff.IndexOf ((char)0);  
                             string reader = currbuff.Substring(0, nullindex);
                             readers.Add (reader);
                             len = len - (reader.Length + 1);
                             currbuff = currbuff.Substring (nullindex + 1, len);
                             //pcsc.ReaderName = reader;
                         }
                     }
                 }
            }
            catch
            {
            }
            return readers;
        }

        public uint SCReader_SetToSync ()
        {
            uint result = SCARD_E_UNEXPECTED;

            const uint IOCTL_SET_ICC_MODE = 0x917;
            byte[] SET_SYNC_MOD = {0x33};

            try
            {
                uint ControlCode = ScardCtlCode (IOCTL_SET_ICC_MODE);
                uint ReturnedSize = 2;
             
                result = SCardControl (pcsc.hCard, ControlCode, SET_SYNC_MOD, 1,  pcsc.brRecvBuf, 260, out ReturnedSize);
            }
            catch
            {
            }
            return result;
        }

        internal uint SCReader_Transmit (uint cbSendLength)
        {
            uint result = SCARD_E_UNEXPECTED;
            uint pcbRecvLength = (uint)pcsc.brRecvBuf.Length;

            try
            {
                pcsc.IO_Request.dwProtocol = (int) pcsc.dwActProtocol;
                pcsc.IO_Request.cbPciLength = Marshal.SizeOf (typeof (SCARD_IO_REQUEST));
                pcsc.dwRecvLen = (uint)pcsc.brRecvBuf.Length;
                result = SCardTransmit (pcsc.hCard, ref pcsc.IO_Request, pcsc.brSendBuf, cbSendLength, ref pcsc.IO_Request, pcsc.brRecvBuf, ref pcsc.dwRecvLen);
            }
            catch
            {             
            }
            return result;
        }

        private bool SCReader_VerifyAnswer ()
        {
            bool result = false;

            if (pcsc.dwRecvLen > 1 && pcsc.dwRecvLen< pcsc.brRecvBuf.Length)
            {
                if (pcsc.brRecvBuf [pcsc.dwRecvLen-1] == 0 && pcsc.brRecvBuf [pcsc.dwRecvLen -2] == 0x90)
                {
                    result = true;
                }
            }

            return result;
        }

        public uint SCReader_SelectSLE4442 ()
        {
            uint result = SCARD_E_UNEXPECTED;
            byte[] cmd = new byte[] { 0x80, 0x60, 0x01, 0x00, 0x03, 0x22, 0x01, 0x82 };

            Array.Copy (cmd, pcsc.brSendBuf, cmd.Length);
            result = SCReader_Transmit ((uint)cmd.Length);
            if (result == 0)
            {
                SCReader_VerifyAnswer ();
            }

            return result;
        }

        public uint SCReader_Read (byte start_pos, byte len, out byte [] data)
        {
            uint result = SCARD_E_UNEXPECTED;
            data = null;

            if (len >0)
            {
                byte[] cmd = new byte[] {0x00, 0xB0, 0x00, start_pos, len};

                Array.Copy (cmd, pcsc.brSendBuf, cmd.Length);
                result = SCReader_Transmit ((uint)cmd.Length);

                if (result == 0)
                {
                    try
                    {
                        SCReader_VerifyAnswer();
                        data = new byte [len];
                        Array.Copy(pcsc.brRecvBuf, data, len);
                    }
                    catch
                    {
                        result = SCARD_E_UNEXPECTED;
                    }
                }
            }
          
            return result;
        }

        public uint SCReader_WriteSLE4442 (byte start_pos, byte[] data, byte[] pin)
        {
            uint result = SCARD_E_UNEXPECTED;

            try
            {
                if (start_pos < 0x20 || pin == null || pin.Length != 3 || data == null || data.Length > 0xDF)
                {
                    return result;
                }

                byte[] select_file_3f82 = new byte[] { 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3f, 0x82 };
                byte[] read_err_counter = new byte[] { 0x00, 0xB0, 0x00, 0x00, 0x01 };
                byte[] verify_pin = new byte[] { 0x00, 0x20, 0x00, 0x00, 0x03, pin[0], pin[1], pin[2] };
                byte[] select_file_3f01 = new byte[] { 0x00, 0xA4, 0x00, 0x00, 0x02, 0x3f, 0x01 };

                byte[] wr_data = new byte[] { 0x00, 0xD0, 0x00, start_pos, (byte)data.Length };
                byte[] wr_data_cmd = new byte [wr_data.Length + data.Length];
                wr_data.CopyTo (wr_data_cmd, 0);
                data.CopyTo (wr_data_cmd, wr_data.Length);

                Array.Copy(select_file_3f82, pcsc.brSendBuf, select_file_3f82.Length);
                if (SCReader_Transmit((uint)select_file_3f82.Length) == 0)
                {
                    Array.Copy(read_err_counter, pcsc.brSendBuf, read_err_counter.Length);
                    if (SCReader_Transmit((uint)read_err_counter.Length) == 0 && SCReader_VerifyAnswer())
                    {
                        Array.Copy(verify_pin, pcsc.brSendBuf, verify_pin.Length);
                        if (SCReader_Transmit((uint)verify_pin.Length) == 0 && SCReader_VerifyAnswer())
                        {
                             Array.Copy(read_err_counter, pcsc.brSendBuf, read_err_counter.Length);
                             if (SCReader_Transmit((uint)read_err_counter.Length) == 0 && SCReader_VerifyAnswer())
                             {
                                 Array.Copy(select_file_3f01, pcsc.brSendBuf, select_file_3f01.Length);
                                 if (SCReader_Transmit((uint)select_file_3f01.Length) == 0 && SCReader_VerifyAnswer())
                                 {
                                     Array.Copy(wr_data_cmd, pcsc.brSendBuf, wr_data_cmd.Length);
                                     if (SCReader_Transmit((uint)wr_data_cmd.Length) == 0 && SCReader_VerifyAnswer())
                                     {
                                         result = 0;
                                     }
                                 }
                             }

                        }
                    }
                }
            }
            catch
            {

            }

            return result;
        }

        public uint SCReader_GetATR (ref string atr)
        {
            uint result = SCARD_E_UNEXPECTED;
            atr = String.Empty;

            try
            {
                StringBuilder sb = new StringBuilder ();
                byte[] ATR = new byte [128];
                uint ReaderLen = 128;
                uint ATRLen = 32;
                uint Protocol = 0;
                uint State = 0;

                result = SCardStatus (pcsc.hCard, sb, ref ReaderLen, out State, out Protocol,  ATR, ref ATRLen);

                if (result == 0)
                {
                    atr = BitConverter.ToString (ATR, 0, (int)ATRLen).Replace("-", " "); // 3B 04 A2 13 10 91 for SLE4432
                }
            }
            catch
            {
            }

            return result;
        }

    }
}
 