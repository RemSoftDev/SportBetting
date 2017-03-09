using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;


namespace SerialPortManager
{
    public enum ResourceType
    {
        ID_CARD_READER_SERIAL_PORT = 0,
        BILL_VALIDATOR_SERIAL_PORT,
        COIN_ACCEPTOR_SERIAL_PORT
    }

    public enum DeviceProtocol
    {
        PROTOCOL_NONE,
        PROTOCOL_CCTALK,
        PROTOCOL_ID003,
        PROTOCOL_SSP
    }

    public class CommunicationResource
    {
        private string resource_name;
        private ResourceType resource_type;
        private DeviceProtocol device_protocol;

        public string PortName
        {
            get { return resource_name; }
        }

        public ResourceType PortType
        {
            get { return resource_type; }
        }

        public DeviceProtocol ProtocolType
        {
            get { return device_protocol; }
        }

        public CommunicationResource(ResourceType rt, DeviceProtocol dp, string rn)
        {
            resource_type = rt;
            resource_name = rn;
            device_protocol = dp;
        }
    }

    public class SerialPortManager
    {
        private static volatile SerialPortManager instance;
        private static object syncRoot = new Object();
        ComPort serialPort;

        private List<CommunicationResource> cr = null;

        private SerialPortManager()
        {
            cr = GetAllPereferialPorts();
        }

        public List<CommunicationResource> GetSafeSerialPortsMap()
        {
            return cr;
        }

        public string GetIdReaderSerialPort()
        {
            string[] avalible_serial_ports = CoreGetAvailableResources();
            string id_reader_port = null;

            foreach (string port in avalible_serial_ports)
            {
                try
                {
                    if (DetectIdReaderDevice(port))
                    {
                        id_reader_port = port;
                        break;
                    }
                }
                catch (Exception e)
                {

                }

            }

            return id_reader_port;
        }

        public List<CommunicationResource> GetBillValidatorPort ()
        {
            string[] avalible_serial_ports = CoreGetAvailableResources();
            List<CommunicationResource> portList = new List<CommunicationResource>();

            foreach (string port in avalible_serial_ports)
            {
                try
                {
                    CommunicationResource BillValidator = GetBillValidatorPort (port);
                    if (BillValidator != null)
                    {
                        portList.Add (BillValidator);                      
                    }

                }
                catch (Exception e)
                {

                }

            }

            return portList;
        }

        private CommunicationResource GetBillValidatorPort (string port)
        {
            CommunicationResource device = null;
            const int ATTEMPT_MAX = 3;

            for (int attempt = 0; attempt < ATTEMPT_MAX; attempt++)
            {
                try
                {
                    if (Id003_DetectBillValidatorDevice(port))
                    {
                        device = new CommunicationResource(ResourceType.BILL_VALIDATOR_SERIAL_PORT, DeviceProtocol.PROTOCOL_ID003, port);
                        break;
                    }
                    if (CcTalk_DetectBillValidatorDevice(port))
                    {
                        device = new CommunicationResource(ResourceType.BILL_VALIDATOR_SERIAL_PORT, DeviceProtocol.PROTOCOL_CCTALK, port);
                        break;
                    }
                    if (Ssp_DetectBillValidatorDevice(port))
                    {
                        device = new CommunicationResource(ResourceType.BILL_VALIDATOR_SERIAL_PORT, DeviceProtocol.PROTOCOL_SSP, port);
                        break;
                    }
                }
                catch (Exception e)
                {

                }
                System.Threading.Thread.Sleep (20);
            }

            return device;
        }

        public List<CommunicationResource> GetCoinAcceptorPort()
        {
            string[] avalible_serial_ports = CoreGetAvailableResources();
            List<CommunicationResource> portList = new List<CommunicationResource>();

            foreach (string port in avalible_serial_ports)
            {
                try
                {
                    if (CcTalk_DetectCoinAcceptorDevice (port))
                    {
                        CommunicationResource device = new CommunicationResource (ResourceType.COIN_ACCEPTOR_SERIAL_PORT, DeviceProtocol.PROTOCOL_CCTALK, port);
                        portList.Add(device);
                    }
                }
                catch (Exception e)
                {

                }

            }

            return portList;
        }


        public List<CommunicationResource> GetAllPereferialPorts()
        {
            string[] avalible_serial_ports = CoreGetAvailableResources();
            List<CommunicationResource> portList = new List<CommunicationResource>();

            foreach (string port in avalible_serial_ports)
            {
                try
                {
                    if (CcTalk_DetectCoinAcceptorDevice(port))
                    {
                        CommunicationResource device = new CommunicationResource(ResourceType.COIN_ACCEPTOR_SERIAL_PORT, DeviceProtocol.PROTOCOL_CCTALK, port);
                        portList.Add(device);
                        continue;
                    }

                    if (DetectIdReaderDevice(port))
                    {
                        CommunicationResource device = new CommunicationResource (ResourceType.ID_CARD_READER_SERIAL_PORT, DeviceProtocol.PROTOCOL_NONE, port);
                        portList.Add(device);
                        continue;
                    }

                    CommunicationResource BillValidator = GetBillValidatorPort (port);
                    if (BillValidator !=null)
                    {
                        portList.Add (BillValidator);
                        continue;
                    }
                }
                catch (Exception e)
                {

                }

            }
            return portList;
        }



        private string[] CoreGetAvailableResources()
        {
            return SerialPort.GetPortNames();
        }

        private bool CoreTryOpen (string port, int baud_rate, Parity parity, StopBits stop_bits)
        {
            bool result = port.StartsWith("COM", true, CultureInfo.InvariantCulture);
            if (result)
            {
                serialPort = new ComPort (port, baud_rate, parity, stop_bits);
                result = serialPort.TryOpen();
            }
            return result;
        }


        private bool CoreTryClose()
        {
            bool result = false;
            if (serialPort != null)
            {
                result = serialPort.TryClose();
                serialPort = null;

            }
            return result;
        }

        private bool DetectIdReaderDevice (string port)
        {
            bool result = false;

            lock (syncRoot)
            {
                if (CoreTryOpen(port, 38400, Parity.None, StopBits.One))
                {
                    byte[] reply = serialPort.SendCmd(new byte[] { 1, 0, 0, 2, 55, 2, 54 });
                    CoreTryClose();

                    if (reply != null)
                    {
                        if (reply.Length > 7)
                        {
                            string answer = System.Text.Encoding.ASCII.GetString(reply, 3, reply[3]);
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        private bool CCTALK_DetectCoinAcceptorDevice()
        {
            bool result = false;

            byte[] reply = serialPort.SendCmd(new byte[] { 2, 0, 1, 254, 255 });

            if (reply != null)
            {
                if (reply.Length == 10)
                {
                    result = true;
                }
            }

            return result;
        }

        private bool CCTALK_DetectBillValidatorDevice()
        {
            bool result = false;

            byte[] reply = serialPort.SendCmd(new byte[] { 40, 0, 127, 192, 137 });

            if (reply != null)
            {
                if (reply.Length == 10)
                {
                    result = true;
                }
            }
            return result;
        }

        private bool ID003_DetectBillValidatorDevice()
        {
            bool result = false;

            byte[] reply = serialPort.SendCmd (new byte[] { 252, 05, 136, 111, 95 });

            if (reply != null)
            {
                if (reply.Length > 10)
                {
                    result = true;
                }
            }
            return result;
        }

        private bool Id003_DetectBillValidatorDevice(string port)
        {
            bool result = false;

            lock (syncRoot)
            {
                if (CoreTryOpen(port, 9600, Parity.Even, StopBits.One))
                {
                    result = ID003_DetectBillValidatorDevice();
                    CoreTryClose();
                }

            }

            return result;
        }

        private bool CcTalk_DetectBillValidatorDevice(string port)
        {
            bool result = false;

            lock (syncRoot)
            {
                if (CoreTryOpen (port, 9600, Parity.None, StopBits.One))
                {
                    result = CCTALK_DetectBillValidatorDevice();
                    CoreTryClose();
                }
            }

            return result;
        }

        private bool CcTalk_DetectCoinAcceptorDevice(string port)
        {
            bool result = false;

            lock (syncRoot)
            {
                if (CoreTryOpen(port, 9600, Parity.None, StopBits.One))
                {
                    result = CCTALK_DetectCoinAcceptorDevice();
                    CoreTryClose();
                }
            }

            return result;
        }

        private bool SSP_DetectBillValidatorDevice()
        {
            bool result = false;
            byte[] sync_cmd_respond = new byte[] { 127, 128, 1, 240, 35, 128};
            byte[] reply = serialPort.SendCmd (new byte[] {127, 128, 1, 17, 101, 130});

            if (reply != null)
            {
                result = ByteArrayCompare (sync_cmd_respond, reply);
            }

            return result;
        }

        private bool Ssp_DetectBillValidatorDevice(string port)
        {
            bool result = false;

            lock (syncRoot)
            {
                if (CoreTryOpen(port, 9600, Parity.None, StopBits.Two))
                {
                    result = SSP_DetectBillValidatorDevice();
                    CoreTryClose();
                }
            }

            return result;
        }

        private bool ByteArrayCompare (byte[] a1, byte[] a2)
        {

            if (a1.Length != a2.Length)
            {
                return false;
            }

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static SerialPortManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new SerialPortManager();
                        }
                    }
                }

                return instance;
            }
        }

    }
}
