using System;
using System.Collections.Generic;
using SerialPortManager;
using System.Threading;


namespace SmartCardReader
{
    public class SmartCardManager
    {
        private static SerialPortManager.SerialPortManager spm = SerialPortManager.SerialPortManager.Instance;
        private static readonly SmartCardManager instance = new SmartCardManager ();

        public EventHandler<CardEventArgs<string>> SmartCardInsertEventHandler = null;
        public EventHandler SmartCardRemoveEventHandler = null;
        public EventHandler<CardEventArgs<string>> SmartCardErrorEventHandler = null;

        SCR28SmartCardReader scrSCR28 = null;
        DDM845SmartCardReader scrDDM845 = null;
        private volatile bool CardReaderDetected;

        public bool SmartCardReaderDetected
        {
            get { return CardReaderDetected; }   
        }

        private string CheckReaderVirtualComPort ()
        {
            string port = null;
            
            List<CommunicationResource> sp = spm.GetSafeSerialPortsMap();
            foreach (CommunicationResource cr in sp)
            {
                if (cr.PortType == ResourceType.ID_CARD_READER_SERIAL_PORT)
                {
                    port = cr.PortName;
                    break;
                }
            }
            return port;
        }

        private SmartCardManager()
        {
            bool result = false;
            string port = CheckReaderVirtualComPort ();

            if (!String.IsNullOrEmpty (port))
            {
                scrDDM845 = new DDM845SmartCardReader(port);

                scrDDM845.SmartCardInsertEventHandler += OnCardInserted;
                scrDDM845.SmartCardRemoveEventHandler += OnCardRemoved;
                scrDDM845.SmartCardErrorEventHandler += OnCardError;
                result = true;
            }
            else 
            {             
                scrSCR28 = new SCR28SmartCardReader ();
                string exact_reader = null;
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(500);
                    if (scrSCR28.SmartCardReaderInitialized)
                    {
                        break;
                    }
                }
                if (scrSCR28.CheckSmartCardReaderByName ("OEM SDK USB", out exact_reader))
                {
                    scrSCR28.SmartCardInsertEventHandler += OnCardInserted;
                    scrSCR28.SmartCardRemoveEventHandler += OnCardRemoved;
                    scrSCR28.SmartCardErrorEventHandler += OnCardError;
                    result = true;
                }
                else
                {
                    scrSCR28 = null;
                }
            }
            CardReaderDetected = result;
        }

      
        public static SmartCardManager GetManager()
        {
            return instance;
        }

        public bool WrireData (string wr_data)
        {
            if (scrDDM845 != null)
            {
                return scrDDM845.WriteDataA(wr_data);
            }
            if (scrSCR28 != null)
            {
                return scrSCR28.WriteDataA (wr_data);
            }

            return false;
        }

        public CardReaderInfo GetCardReaderInventory()
        {
            if (scrDDM845 != null)
            {
                return scrDDM845.GetReaderInventory ();
            }
            if (scrSCR28 != null)
            {
                return scrSCR28.GetReaderInventory ();
            }

            return null;
        }
        public void OnCardInserted (object sender, CardEventArgs<string> e)
        {
            EventHandler<CardEventArgs<string>> handler = SmartCardInsertEventHandler;
            if (handler != null)
            {
                handler (this, e);
            }
        }

        public void OnCardRemoved (object sender, EventArgs e)
        {
            EventHandler handler = SmartCardRemoveEventHandler;
            if (handler != null)
            {
                handler (this, null);
            }
        }
        public void OnCardError (object sender, CardEventArgs<string> e)
        {
            EventHandler<CardEventArgs<string>> handler = SmartCardErrorEventHandler;
            if (handler != null)
            {
                handler(this, null);
            }
        }
    }
}
