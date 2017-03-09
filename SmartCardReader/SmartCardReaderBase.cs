using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCardReader
{
    public class CardEventArgs<T> : EventArgs
    {
        public CardEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }

    public enum CardPosition
    {
        REMOVED = 0,
        INSERTED
    }

    public class CardReaderInfo
    {
        public string manufacturer;
        public string type;
        public string sn;
        public string firmware;
    }

    public abstract class  SmartCardReaderBase
    {
        abstract public bool WrireData(int address, string data);
        abstract public bool WriteData(int address, byte[] data, int len);
        abstract public bool ReadData(int address, int len, ref string data);
        abstract public bool ReadData(int address, int len, ref byte[] data);
        abstract public bool GetLastError(out string error);
        abstract public bool GetCardStatus(ref CardPosition pos);

        abstract public CardReaderInfo GetReaderInventory();
    }

    public class SmartCardReader: SmartCardReaderBase
    {
     
        protected CardPosition SmartCardPos = CardPosition.REMOVED;

        public EventHandler<CardEventArgs<string>> SmartCardInsertEventHandler = null;
        public EventHandler SmartCardRemoveEventHandler = null;
        public EventHandler<CardEventArgs<string>> SmartCardErrorEventHandler = null;

        private int sn_record_size = 0x19;
        private int sn_record_start_address = 0x00;

        public SmartCardReader(int _sn_record_size, int _sn_record_start_address)
        {
            sn_record_size = _sn_record_size;
            sn_record_start_address = _sn_record_start_address;
        }
        protected void OnCardInserted (string sn)
        {
            EventHandler<CardEventArgs<string>> handler = SmartCardInsertEventHandler;
            if (handler != null)
            {
                handler(this, new CardEventArgs<string>(sn));
            }
        }

        protected void OnCardRemoved ()
        {
            EventHandler handler = SmartCardRemoveEventHandler;
            if (handler != null)
            {
                handler (this, null);
            }
        }

        protected void OnCardError ()
        {
            EventHandler<CardEventArgs<string>> handler = SmartCardErrorEventHandler;
            if (handler != null)
            {
                handler (this, null);
            }
        }

        protected bool CheckIfSerialValid (string str)
        {
            bool result = false;
          
            try
            {
                if (!String.IsNullOrEmpty (str))
                {
                    int len = str.Length;
                    if (len == sn_record_size)
                    {
                        result = true;
                        for (int i = 0; i < len; i++)
                        {
                            if (str[i] < 32 || str[i] > 126)
                            {
                                result = false;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        protected void Verify ()
        {
            string sn = String.Empty;
            try
            {
                ReadData (sn_record_start_address, sn_record_size, ref sn);
            }
            catch
            {
            }
            if (String.IsNullOrEmpty(sn))
            {
                sn = "CARDERROR";
            }
            OnCardInserted (sn);
        }

        protected void ProcessCard (CardPosition cardpos)
        {
            if (cardpos != SmartCardPos)
            {
                SmartCardPos = cardpos;

                if (cardpos == CardPosition.INSERTED)
                {
                    string sn = String.Empty;
                    if (ReadData (sn_record_start_address, sn_record_size, ref sn))
                    {
                        if (!String.IsNullOrEmpty(sn))
                        {
                            if (CheckIfSerialValid(sn))
                            {
                                OnCardInserted (sn);
                            }
                        }
                    }
                }
                else
                {
                    OnCardRemoved ();
                }
            }
        }

        public override bool WriteData (int start_address, byte[] data, int len)
        {
            return false;
        }
        public override bool WrireData (int start_address, string data)
        {
            return false;
        }
        public override bool ReadData (int start_address, int len, ref string data_str)
        {
            return false;
        }
        public override bool ReadData (int start_address, int len, ref byte[] data)
        {
            return false;
        }
        public override bool GetCardStatus (ref CardPosition pos)
        {          
            return false;
        }
        public override bool GetLastError (out string error)
        {
            error = String.Empty;
            return false;
        }
        public override CardReaderInfo GetReaderInventory()
        {
            return null;
        }
    }
}
