using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Nbt.Services.Scf.CashIn.Validator;
using Preferences.Services.Preference;
using SerialPortManager;
using SportRadar.Common.Logs;
using System.Management;
using TabioSvc;

namespace Nbt.Services.Scf.CashIn.Validator.TabTerminal
{

    public class TabTerminal : AValidator
    {
        private decimal _credit;
        private decimal _maxLimit;

        private TabioSvc.ITabioCashAcceptorsFlat8 m_chashAcceptorsInterface = null;
        private TabioSvc.Tabio m_tabioClass = null;
        private static object enableLock = new Object();
        private static ILog Log = LogFactory.CreateLog (typeof (TabTerminal));

        TabTerminal ()
        {
            Log.Info("TabTerminal: constructor is called!");
        }

        ~TabTerminal()
        {
            Disable();
        }

        private void Handle_CashAcceptorCashInFwd (int nDeviceID, int nChannelNum, int nCounterDelta, int nTickCount)
        {
            string currency;
            decimal m_collected = Convert.ToDecimal(nCounterDelta) * m_chashAcceptorsInterface.GetChannelValue (nDeviceID, nChannelNum, out currency);
            _credit += m_collected;
            OnCashIn (new CashInEventArgs(m_collected, false));
        }

        private void Handle_CashAcceptorEscrowEvent(int nDeviceID, int nChannelNum, int nCounterDelta, int nTickCount)
        {
            m_chashAcceptorsInterface.CommitEscrow(nDeviceID, nChannelNum, 1);
        }

        private bool Initialize (ref TabioSvc.Tabio TabIO_Class)
        {
            bool result = false;
            int m_deviceId;
            int m_deviceType;
            int m_numberOfChannels;
            int m_deviceSubType;
            _COMPORTSTR = "TAB IO PCI BOARD";
           
            m_chashAcceptorsInterface = (ITabioCashAcceptorsFlat8)TabIO_Class;
            m_tabioClass = (TabioSvc.Tabio)m_chashAcceptorsInterface;

            m_tabioClass.CashAcceptorCashIn += new DTabioCashAcceptorEvents_CashAcceptorCashInEventHandler (Handle_CashAcceptorCashInFwd);
            m_tabioClass.CashAcceptorEscrow += new DTabioCashAcceptorEvents_CashAcceptorEscrowEventHandler (Handle_CashAcceptorEscrowEvent);

            int m_dev_numb = m_chashAcceptorsInterface.NumberOfDevices;

            for (int i = 0; i < m_dev_numb; i++)
            {
                m_deviceId = m_chashAcceptorsInterface.EnumDeviceIDs (i);
                m_deviceType = m_chashAcceptorsInterface.GetDeviceType (m_deviceId, out m_deviceSubType);
                m_numberOfChannels = m_chashAcceptorsInterface.GetNumberOfChannels (m_deviceId);
                int caps = m_chashAcceptorsInterface.GetDeviceCaps (m_deviceId);

                if (m_deviceType == (int)eCashAcceptorType.CDEVT_COIN_ACCEPTOR || m_deviceType == (int)eCashAcceptorType.CDEVT_NOTE_ACCEPTOR)
                {
                    try
                    {
                        m_chashAcceptorsInterface.SetDeviceEnabled (m_deviceId, 0);

                        if ((caps & (int)TabioSvc.eCashAcceptorCaps.CDEVCAPS_ESCROW) == (int)TabioSvc.eCashAcceptorCaps.CDEVCAPS_ESCROW)
                        {
                            m_chashAcceptorsInterface.SetEscrowEnabled (m_deviceId, 1);
                        }
                        result = true;
                     /*   for (int channelNumber = 0; channelNumber < m_numberOfChannels; channelNumber++)
                        {
                            if (m_chashAcceptorsInterface.CanDisableChannel (m_deviceId, channelNumber) == 1)
                            {
                                m_chashAcceptorsInterface.SetChannelEnabled (m_deviceId, channelNumber, 1);
                            }
                        }*/
                        string info = String.Format(" ID: {0} ", m_deviceId);
                        Log.Info ("TabTerminal: CashAcceptor found: " + info + m_chashAcceptorsInterface.GetInformationString (m_deviceId, "manufacturer"));
                    }
                    catch
                    {
                    }
                }

            }

            return result;
        }

        private bool setChannels (decimal ActValue)
        {
            bool result = false;
            int m_deviceSubType;
            string mint;

            try
            {
                int m_dev_numb = m_chashAcceptorsInterface.NumberOfDevices;

                for (int i = 0; i < m_dev_numb; i++)
                {
                   int m_deviceId = m_chashAcceptorsInterface.EnumDeviceIDs (i);
                   int m_deviceType = m_chashAcceptorsInterface.GetDeviceType (m_deviceId, out m_deviceSubType);
                   int m_numberOfChannels = m_chashAcceptorsInterface.GetNumberOfChannels (m_deviceId);
                   int caps = m_chashAcceptorsInterface.GetDeviceCaps (m_deviceId);

                   if (m_deviceType == (int)eCashAcceptorType.CDEVT_COIN_ACCEPTOR || m_deviceType == (int)eCashAcceptorType.CDEVT_NOTE_ACCEPTOR)
                   {
                       for (int channelNumber = 0; channelNumber < m_numberOfChannels; channelNumber++)
                       {
                           if (m_chashAcceptorsInterface.CanDisableChannel (m_deviceId, channelNumber) == 1)
                           {
                               decimal chan_value = m_chashAcceptorsInterface.GetChannelValue (m_deviceId, channelNumber, out mint);
                               if ((chan_value != 0) && ((ActValue + chan_value) <= _maxLimit))
                               {
                                   m_chashAcceptorsInterface.SetChannelEnabled (m_deviceId, channelNumber, 1);
                               }
                               else
                               {
                                   m_chashAcceptorsInterface.SetChannelEnabled (m_deviceId, channelNumber, 0);
                               }
                           }
                       }
                   }
                
                }
            }
            catch
            {
                Log.Info ("TabTerminal: Exception while setting channels in setChannels!");
            }


            return result;
        }

        public static bool CheckTabBoard ()
        {
            bool result = false;

            string str = "SELECT * FROM Win32_PnPEntity WHERE DESCRIPTION LIKE '%TAB IO%'";
            ManagementObjectSearcher mos = new ManagementObjectSearcher ("root\\CIMV2", str);

            foreach (ManagementObject queryObj in mos.Get())
            {
                result = true;
                break;
            }

            return result;
        }

        private bool SetEnableOrDisable (bool action)
        {
            bool result = false;
            int enable = 0;

            lock (enableLock)
            {
                if (m_chashAcceptorsInterface != null)
                {
                    if (action)
                    {
                        enable = 1;
                    }
                    try
                    {
                        int m_deviceSubType;
                        int m_dev_numb = m_chashAcceptorsInterface.NumberOfDevices;
                        for (int i = 0; i < m_dev_numb; i++)
                        {
                            int m_deviceId = m_chashAcceptorsInterface.EnumDeviceIDs (i);
                            int m_deviceType = m_chashAcceptorsInterface.GetDeviceType (m_deviceId, out m_deviceSubType);

                            if (m_deviceType == (int)eCashAcceptorType.CDEVT_COIN_ACCEPTOR || m_deviceType == (int)eCashAcceptorType.CDEVT_NOTE_ACCEPTOR)
                            {
                                m_chashAcceptorsInterface.SetDeviceEnabled (m_deviceId, enable);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return result;
        }

        public TabTerminal (IPrefSupplier pref, string prefKey, string mode)
        {
            try
            {
                TabioSvc.Tabio mTabioSvc = new Tabio ();
                ValFound = Initialize (ref mTabioSvc);

                base.Enable(0);
                Thread.Sleep(800);
                base.Disable();
            }
            catch
            {
                ValFound = false;
            }

            if (ValFound)
            {
                Log.Info ("TabTerminal: TabioSvc object was initialized!");
            }
            else
            {
                Log.Info ("TabTerminal: Can't initialize TabioSvc!");
            }
        }

        private DeviceInfo GetDeviceInfo (int m_deviceId, int m_deviceType)
        {
            DeviceInfo di = null;

            if (m_deviceType == (int)eCashAcceptorType.CDEVT_COIN_ACCEPTOR)
            {
                di = new DeviceInfo();
                di.device_type = DeviceType.COIN_ACCEPTOR;
            }
            else if (m_deviceType == (int)eCashAcceptorType.CDEVT_NOTE_ACCEPTOR)
            {
                di = new DeviceInfo();
                di.device_type = DeviceType.BILL_VALIDATOR;
            }
            if (di != null)
            {
                di.device_producer = m_chashAcceptorsInterface.GetInformationString (m_deviceId, "manufacturer");
                di.device_serial_number = m_chashAcceptorsInterface.GetInformationString (m_deviceId, "serial number");
                di.device_firmware_version = "N/A";
                di.device_model = "N/A";
            }

            return di;
        }

        public override List<DeviceInfo> GetShortDeviceInventory()
        {
            return GetDeviceInventory ();
        }

        public override List <DeviceInfo> GetDeviceInventory ()
        {
            List<DeviceInfo> list = new List<DeviceInfo> (2);
           
            try 
            {
                int m_deviceSubType;
                int m_dev_numb = m_chashAcceptorsInterface.NumberOfDevices;
                for (int i = 0; i < m_dev_numb; i++)
                {
                    int m_deviceId = m_chashAcceptorsInterface.EnumDeviceIDs(i);
                    int m_deviceType = m_chashAcceptorsInterface.GetDeviceType(m_deviceId, out m_deviceSubType);

                    if (m_deviceType == (int)eCashAcceptorType.CDEVT_COIN_ACCEPTOR || m_deviceType == (int)eCashAcceptorType.CDEVT_NOTE_ACCEPTOR)
                    {
                        DeviceInfo di = GetDeviceInfo (m_deviceId, m_deviceType);
                        if (di != null)
                        {
                            list.Add (di);
                        }
                    }
                    
                }
               
            }
            catch
            {
                Log.Info("TabTerminal: Exception while getting Device Inventory!");
            }        

            return list;
        }


        public override bool IsDataSetValid ()
        {
            return true;
        }


        public override bool SetEnabledChannels (decimal ActValue)
        {
            lock (enableLock)
            {
                base.SetEnabledChannels (ActValue);
                return setChannels (ActValue);
            }
        }

        public override bool Disable ()
        {

            base.Disable();
            _credit = 0;  
        
            return SetEnableOrDisable (false);
        }

        public override bool Enable (decimal MaxLimit)
        {

            bool result = false;

            base.Enable(MaxLimit);
            _maxLimit = MaxLimit < 0 ? 0 : MaxLimit;

            result = SetEnableOrDisable (true);
            setChannels (this._credit);
           
             return result;
        }

        public override decimal GetCredit ()
        {
            base.GetCredit();
            return _credit;
        }

        public override void SetCredit (decimal credit)
        {
            base.SetCredit (credit);
            _credit = credit;
        }
        public override bool ResetCredit ()
        {
            base.ResetCredit ();
            _credit = 0;
            return true;
        }

        private bool CheckCashValidator (eCashAcceptorType type)
        {
             bool result = false;

             try
             {
                 int m_dev_numb = m_chashAcceptorsInterface.NumberOfDevices;
                 int m_deviceSubType;

                 for (int i = 0; i < m_dev_numb; i++)
                 {
                     int m_deviceId = m_chashAcceptorsInterface.EnumDeviceIDs(i);
                     int m_deviceType = m_chashAcceptorsInterface.GetDeviceType(m_deviceId, out m_deviceSubType);

                     if (m_deviceType == (int)type)
                     {
                         result = true;
                         break;
                     }
                 }
             }
             catch
             {
                 Log.Info("TabTerminal: Exception while verifying CashAcceptor!");
             }
             return result;
        }

        public override bool CheckBillValidator ()
        {
            return CheckCashValidator (eCashAcceptorType.CDEVT_NOTE_ACCEPTOR);
        }

        public override bool CheckCoinAcceptor()
        {
             return CheckCashValidator (eCashAcceptorType.CDEVT_COIN_ACCEPTOR);
        }
    }

}
