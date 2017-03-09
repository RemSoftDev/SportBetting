using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;


namespace Emulator
{
    
    class EmulatorA
    {
        private  ComPort cp = null;
        private Dictionary<byte, Func<byte[], byte[]>> register = new Dictionary<byte, Func<byte[], byte[]>>();
        public Device CoinAcceptor = null;
        public Device BillValidator = null;

        public EmulatorA ()
        {
        }

        public bool IsTerminalRunning()
        {
           /* foreach (Process clsProcess in Process.GetProcesses()) 
            {
                if (clsProcess.ProcessName.Contains ("SportBetting.WPF"))
                {
                    return true;
                }
            }
            return false;*/
            Mutex result;
            Mutex.TryOpenExisting ("SportBetting", out result);
            if (result == null)
            {           
                return false;
            }
            return true;       
        }

        public bool IsConnectionAlive()
        {
            if (cp != null)
            {
                return cp.IsConnectionAlive ();
            }

            return false;

        }

        public bool Run ()
        {
            bool result = true;

            CreateBillValidator ();
            CreateCoinAcceptor ();
            cp = new ComPort (null, new Dictionary<byte, Func<byte[], byte[]>>(register));
            result = cp.TryOpen();

            return result;
        }

        public string GetComPortName()
        {

            if (cp != null)
            {
                return cp.GetComPortNane();
            }

            return null;
        }

        public List<string> GetInstalledVirtualPorts()
        {
            return ComPort.GetInstalledVirtualPorts();
        }

        public bool IsDA2AdapterInstalled()
        {
            return ComPort.CheckDA2Adapter();
        }

        private void CreateBillValidator ()
        {
            DeviceConfig BillValidatorCfg = new DeviceConfig();
            BillValidatorCfg.device_address = 40;
            BillValidatorCfg.master_address = 1;
            BillValidatorCfg.equipment_category = "Bill Validator";
            BillValidatorCfg.use_crc16 = true;
            BillValidatorCfg.use_cryptograthy = true;
            BillValidatorCfg.dataset = "EUR05B08";
            // Bill ID List
            Dictionary<int, string> bill_id_list = new Dictionary<int, string>();
            bill_id_list.Add(-1, "0000000");
            bill_id_list.Add(1, "EU0005A");
            bill_id_list.Add(2, "EU0010A");
            bill_id_list.Add(3, "EU0020A"); 
            bill_id_list.Add(4, "EU0050A");
            bill_id_list.Add(5, "EU0100A");
            bill_id_list.Add(6, "EU0200A");
            BillValidatorCfg.money_id_list = bill_id_list;
            //
            BillValidator = new Device(BillValidatorCfg);
            RegisterDevice(BillValidator);

        }

        private void CreateCoinAcceptor()
        {
            DeviceConfig CoinAcceptorCfg = new DeviceConfig();
            CoinAcceptorCfg.device_address = 2;
            CoinAcceptorCfg.master_address = 1;
            CoinAcceptorCfg.equipment_category = "Coin Acceptor";
            CoinAcceptorCfg.use_crc16 = false;
            CoinAcceptorCfg.use_cryptograthy = false;
            CoinAcceptorCfg.dataset = null;

            // CoinID List
            Dictionary<int, string> coin_id_list = new Dictionary<int, string>();
            coin_id_list.Add(-1, "######");
            coin_id_list.Add(1, "EU200A");
            coin_id_list.Add(2, "EU100A");
            coin_id_list.Add(3, "EU050A");
            coin_id_list.Add(4, "EU020A");
            coin_id_list.Add(5, "EU010A");
            coin_id_list.Add(6, "EU005A");
            CoinAcceptorCfg.money_id_list = coin_id_list;

            CoinAcceptor = new Device (CoinAcceptorCfg);
            RegisterDevice (CoinAcceptor);
        }

        private bool RegisterDevice(Device dev)
        {
            bool result = true;
            byte address;
            Func<byte[], byte[]> handler;
            try
            {
                dev.GetMsgHandler(out address, out handler);
                register.Add(address, handler);
            }
            catch
            {
                result = false;
            }
            return result;
        }          

    }
}
