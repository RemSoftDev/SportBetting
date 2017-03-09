using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;
using System.Timers;

namespace Emulator
{
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

    public class ComPort: IDisposable
    {
        private SerialPort serialPort;
        private Dictionary<byte, Func<byte[], byte[]>> register;
        private static string com_port;
        private static List <string> ComPorts = new List<string> ();
        private System.Timers.Timer LinkTimer;
        private bool _IsConnectionAlive = false;
      

        public ComPort (string port, Dictionary<byte, Func<byte[], byte[]>> reg)
        {
            if (String.IsNullOrEmpty(port))
            {
                GetEmulatorComPort();
            }
            else
            {
                com_port = port;
            }
            register = reg;
            LinkTimer = new System.Timers.Timer (2000);
            LinkTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            LinkTimer.AutoReset = true;
            LinkTimer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _IsConnectionAlive = false;
        }

        public bool IsConnectionAlive()
        {
            return _IsConnectionAlive;
        }

        public string GetComPortNane ()
        {
            return com_port;
        }

        public static List<string> GetInstalledVirtualPorts()
        {
            ComPorts.Clear();
            GetEmulatorComPort();

            return ComPorts;
        }

       
        public bool TryOpen()
        {
            bool result = false;

            if (!String.IsNullOrEmpty(com_port))
            {
                try
                {
                    TryClose();
                    serialPort = new SerialPort (com_port, 9600, Parity.None, 8, StopBits.One);

                    serialPort.Open();
                    serialPort.DataReceived += OnDataReceived;
                    result = true;
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }

        public bool TryClose()
        {
            bool result = false;

            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.DataReceived -= OnDataReceived;
                    serialPort.Close();
                    serialPort.Dispose();
                    serialPort = null;
                    result = true;
                }
            }

            return result;
        }
       
        public bool IsOpen()
        {
            return serialPort.IsOpen;
        }

        public void Dispose()
        {
            if (serialPort != null)
            {
                TryClose();
            }
        }


        private void OnDataReceived (object s, SerialDataReceivedEventArgs e)
        {
           
          /*  byte[] data = new byte[serialPort.BytesToRead];
            serialPort.Read(data, 0, data.Length);*/
            List<byte> storage = new List<byte>();
            while (serialPort.BytesToRead > 0)
            {
                storage.Add((byte)serialPort.ReadByte());
            }

            if (storage.Count > 0)
            {               
                ProcessData(storage.ToArray ());
            }
             
        }

        private void ProcessData (byte[] data)
        {
            _IsConnectionAlive = true;
            LinkTimer.Stop();
            LinkTimer.Start();

            foreach (KeyValuePair<byte, Func<byte[], byte[]>> item in register)
            {
                try
                {
                  /*  byte[] echo = new byte[data.Length];
                    Array.Copy (data, echo, data.Length);*/
                    byte[] answer = item.Value(data);

                    if (answer!= null && answer.Length >0)
                    {
                      /*  byte [] reply = new byte [echo.Length + answer.Length];
                        Array.Copy(echo, 0, reply, 0, echo.Length);
                        Array.Copy(answer, 0, reply, echo.Length, answer.Length);*/
                        serialPort.Write(answer, 0, answer.Length);
                       
                        break;
                    }
                }
                catch
                {
                }
            }
        }

        private static void GetEmulatorComPort ()
        {
            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0 and Caption like 'ELTIMA Virtual%'");
            ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
            ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");
            ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);

            try
            {
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
                                    com_port = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")", string.Empty);
                                    int pos = com_port.IndexOf("->");
                                    if (pos > 0)
                                    {
                                        com_port = com_port.Substring (0, pos);
                                        ComPorts.Add (com_port);
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
           
        }

        public static bool CheckDA2Adapter()
        {
            bool result = false;
            ObjectQuery objectQuery = new ObjectQuery ("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0 and Caption like 'ITL USB (High Speed)%'");
            ConnectionOptions options = ProcessConnection.ProcessConnectionOptions();
            ManagementScope connectionScope = ProcessConnection.ConnectionScope(Environment.MachineName, options, @"\root\CIMV2");
            ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);

            try
            {
                using (comPortSearcher)
                {
                    
                    foreach (ManagementObject obj in comPortSearcher.Get())
                    {
                        if (obj != null)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
            }

            return result;
        }

    }
}
