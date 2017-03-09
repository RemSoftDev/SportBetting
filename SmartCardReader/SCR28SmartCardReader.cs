using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SmartCardReader
{
    public class SCR28SmartCardReader : SmartCardReader
    {
        private const int SN_RECORD_START_ADDRESS = 0x20;
        private const int SN_RECORD_SIZE = 0x19;
        private NativeMethods ReaderNativeMethods = new NativeMethods ();

        private bool CardReaderInitialized = false;
        private List<string> Readers_List = new List<string>(0);

        private static object ReadWriteOperationLock = new Object();
        private Dictionary<string, string> SupportedCardType = null;

        public SCR28SmartCardReader ():base (SN_RECORD_SIZE, SN_RECORD_START_ADDRESS)
        {
            SupportedCardType = new Dictionary<string, string>();
            SupportedCardType.Add ("3B 04 A2 13 10 91", "SLE4442");

            new Thread (RunCardDetection).Start();
        }
        public bool SmartCardReaderInitialized
        {
            get { return CardReaderInitialized; }
        }
       
        private void RunCardDetection()
        {
            while (true)
            {
                while (!CardReaderInitialized)
                {
                    lock (ReadWriteOperationLock)
                    {
                        CardReaderInitialized = InitSmartCardReader();
                    }
                    Thread.Sleep (1000);
                }
               
                CheckCardPosition ();
              
                Thread.Sleep (500);
            }
        }


        public bool CheckSmartCardReaderByName (string reader, out string exact_reader)
        {
            bool result = false;

            exact_reader = String.Empty;
            try
            {           
                foreach (string str in Readers_List)
                {
                    if (str.Contains (reader))
                    {
                        result = true;
                        exact_reader = str;
                        break;
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        public bool GetReaderInventory (out string reader_name, out string chip_version, out string fw_version)
        {
            bool result = false;
            reader_name = "UNKNOWN";
            chip_version = "UNKNOWN ";
            fw_version = "UNKNOWN ";

            try
            {
                lock (ReadWriteOperationLock)
                {
                    List <string> readers = ReaderNativeMethods.SCReader_GetReaderNames ();
                    if (readers.Count > 0)
                    {
                        reader_name = readers [0];
                        if (ReaderNativeMethods.SCReader_ReaderConnect() == 0)
                        {
                            if (ReaderNativeMethods.SCReader_GetReaderInventory(out chip_version, out fw_version) == 0)
                            {
                                result = true;
                            }
                            ReaderNativeMethods.SCReader_ReaderDisconnect ();
                        }

                    }
                }
                    
            }
            catch
            {
            }

            return result;
        }

        private bool InitSmartCardReader ()
        {
            bool result = false;
            uint ret = 0;
            try
            {
                ret = ReaderNativeMethods.SCReader_EstablishContext ();
                if (ret == 0)
                {
                    Readers_List = ReaderNativeMethods.SCReader_GetReaderNames ();
                    string exact_reader = null;
                    if (CheckSmartCardReaderByName ("OEM SDK USB", out exact_reader))
                    {
                        ReaderNativeMethods.SetActiveReader (exact_reader);
                        if (ReaderNativeMethods.SCReader_ReaderConnect () == 0)
                        {
                            if (ReaderNativeMethods.SCReader_SetToSync () == 0)
                            {
                                 ReaderNativeMethods.SCReader_ReaderDisconnect ();
                                 result = true;
                            }
                        }
                        
                    }
                }
                else if (ret == 0x8010002EL)  // SCARD_E_NO_READERS_AVAILABLE
                {
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

            try
            {
                lock (ReadWriteOperationLock)
                {
                    result = ReaderNativeMethods.SCReader_CardConnect ();
                }
                if (result == 0)
                {
                    cardpos = CardPosition.INSERTED;
                    ProcessCard (cardpos);
                }
                else if (result == 0x80100069L)
                {
                    // SCARD_W_REMOVED_CARD
                    cardpos = CardPosition.REMOVED;
                    ProcessCard(cardpos);
                }
                else if (result == 0x80100009L)
                {
                    // SCARD_E_UNKNOWN_READER
                    CardReaderInitialized = false;
                    OnCardError ();
                }
                else /*if (result == 0x801000066)*/
                {
                    ReaderNativeMethods.SCReader_ReleaseContext ();
                    CardReaderInitialized = false;
                }
            }
            catch
            {
            }

        }

        private bool CheckIfCardTypeSupported ()
        {
            bool result = false;
            string atr = String.Empty;

            if (ReaderNativeMethods.SCReader_GetATR (ref atr) == 0)
            {
                if (SupportedCardType.ContainsKey (atr))
                {
                    if (ReaderNativeMethods.SCReader_SelectSLE4442 () == 0)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        public bool WriteDataA (string data)
        {          
            return WrireData (SN_RECORD_START_ADDRESS, data);
        }

        public override bool WrireData (int start_address, string wr_data)
        {
            bool result = false;
           
            if (SmartCardPos == CardPosition.INSERTED)
            {
                lock (ReadWriteOperationLock)
                {
                    if (CheckIfCardTypeSupported ())
                    {
                        byte[] data = Encoding.ASCII.GetBytes (wr_data);
                        if (ReaderNativeMethods.SCReader_WriteSLE4442 ((byte)start_address, data, new byte[] { 0xff, 0xff, 0xff }) == 0)
                        {
                            result = true;
                        }
                        
                    }
                    Verify ();
                }
            }

            /*if (!result)
            {
                OnCardError ();
            }*/
            return result;
        }

        public override bool WriteData (int start_address, byte[] data, int len)
        {
            return false;
        }

        public override bool ReadData (int start_address, int len, ref string data_str)
        {
            bool result = false;

            data_str = String.Empty;
            byte[] data_arr = null;

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

            if (SmartCardPos == CardPosition.INSERTED)
            {
                string atr = String.Empty;
                data = null;
                lock (ReadWriteOperationLock)
                {
                    if (CheckIfCardTypeSupported())
                    {
                        if (ReaderNativeMethods.SCReader_Read((byte)start_address, (byte)len, out data) == 0)
                        {
                            result = true;
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

        public override bool GetCardStatus(ref CardPosition pos)
        {
            pos = SmartCardPos;
            return true;
        }

        public override CardReaderInfo GetReaderInventory()
        {
            CardReaderInfo  cri = null;
            string reader_name;
            string chip_version;
            string fw_version;

            if (GetReaderInventory (out reader_name, out chip_version, out fw_version))
            {
                cri = new CardReaderInfo ();
                cri.sn = "N/A";
                cri.firmware = fw_version;
                cri.type = reader_name;
                cri.manufacturer = "TransMac";
            }
            else if (!String.IsNullOrEmpty (reader_name))
            {
                if (reader_name != "UNKNOWN")
                {
                    cri = new CardReaderInfo();
                    cri.sn = "N/A";
                    cri.firmware = "N/A";
                    cri.type = reader_name;
                    cri.manufacturer = "TransMac";
                }
            }

            return cri;
        }
    }
}
