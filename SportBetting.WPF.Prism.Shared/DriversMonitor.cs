using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using SportRadar.Common.Windows;
using SerialPortManager;

namespace SportBetting.WPF.Prism.Shared
{
  
    public enum DriverTypeDM
    {
        DRIVER_TYPE_DA2 = 0,
        DRIVER_TYPE_ID_READER,
        DRIVER_TYPE_TOUCH_SCREEN,
        DRIVER_TYPE_PRINTER,
        DRIVER_TYPE_VIDEO
    }

    public class DriverData
    {
        public DriverTypeDM type;
        public string device_name;
        public string mnf_name;
        public string driver_inf;
        public DateTime inf_modified;
        public string installed_drivers;
        public string driver_version;
        public string driver_file_version;
        public DateTime date_modified; 

    }

    public class OldDriversData
    {
        public DriverTypeDM type;
        public DateTime inf_modified;
        public DateTime date_modified;
        public string driver_version;

    }

   public class WinSpoolPrinterInfo
   {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int OpenPrinter(string pPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault);
    
        [DllImport("winspool.drv", CharSet=CharSet.Auto, SetLastError=true)]
        static extern bool GetPrinterDriver(IntPtr hPrinter, string pEnvironment, Int32 Level, IntPtr pDriverInfo, Int32 cbBuf, out Int32 pcbNeeded);
    
        [DllImport("winspool.drv", SetLastError = true)]
        public static extern int ClosePrinter(IntPtr hPrinter);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_DEFAULTS
        {
            public IntPtr pDatatype;
            public IntPtr pDevMode;
            public int DesiredAccess;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DRIVER_INFO_8
        {
            public uint cVersion;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pEnvironment;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDriverPath;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDataFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pConfigFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pHelpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDependentFiles;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pMonitorName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDefaultDataType;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszzPreviousNames;
            public FILETIME ftDriverDate;
            public UInt64 dwlDriverVersion;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszMfgName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszOEMUrl;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszHardwareID;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszProvider;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszPrintProcessor;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszVendorSetup;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszzColorProfiles;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszInfPath;
            public uint dwPrinterDriverAttributes;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszzCoreDriverDependencies;
            public FILETIME ftMinInboxDriverVerDate;
            public UInt64 dwlMinInboxDriverVerVersion;
        }


        public DRIVER_INFO_8? GetPrinterInfo(String printerName)
        {
            IntPtr pHandle;      
            PRINTER_DEFAULTS defaults = new PRINTER_DEFAULTS();
            DRIVER_INFO_8? Info8 = null;
      
            OpenPrinter(printerName, out pHandle, ref defaults);
      
            Int32 cbNeeded = 0;
            //GetPrinterDriver(IntPtr hPrinter, string pEnvironment, uint Level, IntPtr pDriverInfo, uint cbBuf, out uint pcbNeeded);
            bool bRet = GetPrinterDriver (pHandle, null, 8, IntPtr.Zero, 0, out cbNeeded);
      
            if (cbNeeded > 0)
            {
                IntPtr pAddr = Marshal.AllocHGlobal((int)cbNeeded);
                bRet = GetPrinterDriver (pHandle, null, 8, pAddr, cbNeeded, out cbNeeded);
        
                if (bRet)        
                {
                    Info8 = (DRIVER_INFO_8)Marshal.PtrToStructure(pAddr, typeof(DRIVER_INFO_8));
                }
        
                Marshal.FreeHGlobal(pAddr);
            }
      
            ClosePrinter(pHandle);
      
            return Info8;
        }

    }

    public class DriversMonitor
    {
        private static volatile DriversMonitor instance;
        private static object syncRoot = new Object();
        private static TimeSpan timeOut = new TimeSpan(0, 0, 15, 0);
        private  List <DriverData> DriverDB = new List <DriverData> (10);
        private List<OldDriversData> OldDriverDB = new List<OldDriversData>(10);
        private static ManagementObjectCollection CIMcollection;
        private readonly string log_path = System.AppDomain.CurrentDomain.BaseDirectory + "log\\SystemInfo.log";
        private static SerialPortManager.SerialPortManager spm = SerialPortManager.SerialPortManager.Instance;


        private DriversMonitor()
        {
            SportRadar.Common.Windows.ThreadHelper.RunThread("driversmonitor", Run, ThreadPriority.BelowNormal);
        }

        public List<DriverData> GetDriversData()
        {
            lock (DriverDB)
            {
                return new List<DriverData>(DriverDB);
            }
        }

        private string GetUserFriendlyDriverType (DriverTypeDM dt)
        {
            string type = null;
            switch (dt)
            {
                case DriverTypeDM.DRIVER_TYPE_DA2:
                    type = "DRIVER_TYPE_DA2";
                    break;
                case DriverTypeDM.DRIVER_TYPE_ID_READER:
                    type = "DRIVER_TYPE_ID_READER";
                    break;
                case DriverTypeDM.DRIVER_TYPE_PRINTER:
                    type = "DRIVER_TYPE_PRINTER";
                    break;
                case DriverTypeDM.DRIVER_TYPE_TOUCH_SCREEN:
                    type = "DRIVER_TYPE_TOUCH_SCREEN";
                    break;

            }
            return (type);
        }

        private void LoadOldDriversList()
        {
          
            OldDriverDB.Clear();
            OldDriversData odd = new OldDriversData();
            odd.type = DriverTypeDM.DRIVER_TYPE_DA2;
            try
            {
                odd.inf_modified = Properties.Settings.Default.DA2_DRIVER_INF_MODIFIED;
                odd.driver_version = Properties.Settings.Default.DA2_DRIVER_VERSION;
                odd.date_modified = Properties.Settings.Default.DA2_DRIVER_DATE_MODIFIED;
            }
            catch { }
            finally
            {
                OldDriverDB.Add(odd);
            }

            odd = new OldDriversData();
            odd.type = DriverTypeDM.DRIVER_TYPE_ID_READER;
            try
            {
                odd.inf_modified = Properties.Settings.Default.ID_READER_DRIVER_INF_MODIFIED;
                odd.driver_version = Properties.Settings.Default.ID_READER_DRIVER_VERSION;
                odd.date_modified = Properties.Settings.Default.ID_READER_DRIVER_DATE_MODIFIED;
            }
            catch { }
            finally
            {
                OldDriverDB.Add(odd);
            }

            odd = new OldDriversData();
            odd.type = DriverTypeDM.DRIVER_TYPE_PRINTER;
            try
            {
                odd.inf_modified = Properties.Settings.Default.PRINTER_DRIVER_INF_MODIFIED;
                odd.driver_version = Properties.Settings.Default.PRINTER_DRIVER_VERSION;
                odd.date_modified = Properties.Settings.Default.PRINTER_DRIVER_DATE_MODIFIED;
            }
            catch { }
            finally
            {                
                OldDriverDB.Add(odd);
            }

            odd = new OldDriversData();
            odd.type = DriverTypeDM.DRIVER_TYPE_TOUCH_SCREEN;
            try
            {
                odd.inf_modified = Properties.Settings.Default.TOUCH_SCREEN_DRIVER_INF_MODIFIED;
                odd.driver_version = Properties.Settings.Default.TOUCH_SCREEN_DRIVER_VERSION;
                odd.date_modified = Properties.Settings.Default.TOUCH_SCREEN_DRIVER_DATE_MODIFIED;
            }
            catch { }
            finally
            {
                OldDriverDB.Add(odd);
            }
           
        }

        private void SaveDriver (DriverData dd)
        {
            switch (dd.type)
            {
                case DriverTypeDM.DRIVER_TYPE_DA2:
                        Properties.Settings.Default.DA2_DRIVER_INF_MODIFIED = dd.inf_modified;
                        Properties.Settings.Default.DA2_DRIVER_VERSION = dd.driver_version;
                        Properties.Settings.Default.DA2_DRIVER_DATE_MODIFIED = dd.date_modified;
                        Properties.Settings.Default.Save();
                        break;
                case DriverTypeDM.DRIVER_TYPE_ID_READER:
                        Properties.Settings.Default.ID_READER_DRIVER_INF_MODIFIED = dd.inf_modified;
                        Properties.Settings.Default.ID_READER_DRIVER_VERSION= dd.driver_version;
                        Properties.Settings.Default.ID_READER_DRIVER_DATE_MODIFIED = dd.date_modified;
                        Properties.Settings.Default.Save();
                        break;
                case DriverTypeDM.DRIVER_TYPE_PRINTER:
                        Properties.Settings.Default.PRINTER_DRIVER_INF_MODIFIED = dd.inf_modified;
                        Properties.Settings.Default.PRINTER_DRIVER_VERSION = dd.driver_version;
                        Properties.Settings.Default.PRINTER_DRIVER_DATE_MODIFIED = dd.date_modified;
                        Properties.Settings.Default.Save();
                        break;
                case DriverTypeDM.DRIVER_TYPE_TOUCH_SCREEN:
                        Properties.Settings.Default.TOUCH_SCREEN_DRIVER_INF_MODIFIED = dd.inf_modified;
                        Properties.Settings.Default.TOUCH_SCREEN_DRIVER_VERSION = dd.driver_version;
                        Properties.Settings.Default.TOUCH_SCREEN_DRIVER_DATE_MODIFIED = dd.date_modified;
                        Properties.Settings.Default.Save();
                        break;
   
            }
        }

        private void WriteLogFile (DriverData dd, string action)
        {
            
            using (StreamWriter sw = (File.Exists(log_path)) ? File.AppendText(log_path) : File.CreateText(log_path))
            {             
                sw.WriteLine("\r\n==================================================================================================");
                sw.WriteLine("==================================================================================================");
                sw.WriteLine ("\r\nRecord Date: {0}", DateTime.Now.ToString());
                sw.WriteLine("Action: {0}!\r\n", action);
                sw.WriteLine ("Driver type: {0}", GetUserFriendlyDriverType (dd.type));
                sw.WriteLine ("Driver name: {0}", dd.device_name);
                sw.WriteLine("Manufacturer name: {0}", dd.mnf_name);
                if (dd.type != DriverTypeDM.DRIVER_TYPE_PRINTER)
                {
                    sw.WriteLine("Driver version: {0}", dd.driver_version);
                }
                else
                {
                    sw.WriteLine("Driver version: {0}", dd.driver_file_version);
                }
                sw.WriteLine("Installed drivers: {0}", dd.installed_drivers);
                sw.WriteLine("Inf File: {0}", dd.driver_inf);
                sw.WriteLine("Modified: {0}", dd.inf_modified.ToLongDateString());
                sw.WriteLine("==================================================================================================");

            } 
        }

        private void WriteRemoveRecordLogFile (DriverTypeDM dt)
        {
           
            using (StreamWriter sw = (File.Exists(log_path)) ? File.AppendText(log_path) : File.CreateText(log_path))
            {
                sw.WriteLine("\r\n==================================================================================================");
                sw.WriteLine("==================================================================================================");
                sw.WriteLine("\r\nRecord Date: {0}", DateTime.Now.ToString());
                sw.WriteLine("Action: Driver was removed!\r\n");
                sw.WriteLine("Driver type: {0}", GetUserFriendlyDriverType(dt));
                sw.WriteLine("==================================================================================================");

            }
        }

        private void TrackDriversChange ()
        {
            string action = null;

            foreach (OldDriversData odd in OldDriverDB)
            {
                var item = DriverDB.Find(x => x.type == odd.type);
                if (item == null)  
                {
                    // removed or has never been installed
                    if (odd.inf_modified.Year >1900)
                    {
                        // removed
                        WriteRemoveRecordLogFile(odd.type);
                        DriverData d = new DriverData();
                        d.inf_modified = new DateTime (1900, 1, 1);
                        d.type = odd.type;
                        SaveDriver(d);

                    }
                }
                else
                {
                    if (String.Compare (odd.driver_version, item.driver_version)!= 0||
                        DateTime.Compare (odd.inf_modified, item.inf_modified) != 0)
                    {
                        // has been modified
                        SaveDriver (item);
                        if (odd.inf_modified.Year > 1900)
                        {
                            action = "Driver replaced the old one";
                        }
                        else
                        {
                            action = "Driver was first time detected";
                        }
                        
                        WriteLogFile (item ,action);
                    }
                }
            }

        }

        private ManagementObjectCollection GetSystemDriverDataFile()
        {
            string query = "SELECT * FROM Win32_PnPSignedDriverCIMDataFile";
            ManagementObjectSearcher sr =  new ManagementObjectSearcher("root\\CIMV2", query);

            return sr.Get();
        }

        private DriverData GetDriverInfo(string query, DriverTypeDM type)
        {
            DriverData data = null;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", query);
            ManagementObjectCollection moc = searcher.Get();

            foreach (var device in moc)
            {
                if (data == null)
                {
                    data = new DriverData();
                }
                data.type = type;
                data.device_name = device["Name"].ToString();
                string q = string.Format("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceID = '{0}'", device.GetPropertyValue("DeviceID"));
                ManagementObjectSearcher sr = new ManagementObjectSearcher("root\\CIMV2", q.Replace("\\", "\\\\"));
                ManagementObjectCollection mc = sr.Get();
                foreach (var manObj in mc)
                {
                    if (manObj == null)
                    {
                        break; 
                    }
                    data.driver_inf = manObj["InfName"].ToString();
                    data.driver_version = manObj["DriverVersion"].ToString();
                    data.mnf_name = manObj["Manufacturer"].ToString();
                    string date = manObj.GetPropertyValue("DriverDate").ToString ();
                    if (date != null)
                    {
                        data.date_modified = ManagementDateTimeConverter.ToDateTime (date);
                    }
                    if (data.driver_inf != null)
                    {
                        string system_path = Environment.GetEnvironmentVariable("windir");
                        data.inf_modified = File.GetLastWriteTime(system_path + "\\inf\\"+ data.driver_inf);
                        data.inf_modified = data.inf_modified.AddTicks(-data.inf_modified.Ticks % 10000000);

                    }

                    foreach (var queryObj in CIMcollection)
                    {
                        string key = device["DeviceID"].ToString().Replace("\\", "\\\\");
                        string param = queryObj["Antecedent"].ToString();
                        if (param.IndexOf(key) > 0)
                        {

                            int len = queryObj["Dependent"].ToString().Length;
                            if (len > 0)
                            {
                                int pos = queryObj["Dependent"].ToString().IndexOf('"');
                                if (pos != -1)
                                {
                                    if (data.driver_file_version != null)
                                    {
                                        data.driver_file_version += ", ";
                                    }
                                    data.driver_file_version += GetFileVersion(queryObj["Dependent"].ToString().Substring(pos + 1, len - pos - 2));
                                    pos = queryObj["Dependent"].ToString().LastIndexOf('\\');
                                    if (pos > 0)
                                    {
                                        if (data.installed_drivers != null)
                                        {
                                            data.installed_drivers += ", ";
                                        }
                                        data.installed_drivers += queryObj["Dependent"].ToString().Substring(pos + 1, len - pos - 2);
                                      
                                    }
                                }
                            }
                        }
                    }
                  

                }

            }
            return data;
        }

        private List<DriverData> GetVideoControllertDriverInfo ()
        {
            List<DriverData> VideoDriver = new List<DriverData>(3);
            DriverData data = null;
            string query = "SELECT * FROM Win32_VideoController";

            ManagementObjectSearcher ms = new ManagementObjectSearcher("root\\CIMV2", query);
            ManagementObjectCollection moc = ms.Get();
            foreach (ManagementObject obj in moc)
            {
                if (data == null)
                {
                    data = new DriverData();
                }
                data.type = DriverTypeDM.DRIVER_TYPE_VIDEO;
                data.device_name = obj["Name"].ToString();
                data.driver_version = obj["DriverVersion"].ToString();
                data.driver_inf = obj["InfFilename"].ToString();
                data.installed_drivers = obj["InstalledDisplayDrivers"].ToString();
                VideoDriver.Add(data);
            }

            return VideoDriver;
        }

        private DriverData GetPrinterDriverInfo()
        {
         
            string key = null;
            DriverData data = null;
            ManagementObjectSearcher ms = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Printer WHERE default = 'true'");
            ManagementObjectCollection moc = ms.Get();
            foreach (var manObj in moc)
            {
                if (manObj == null)
                {
                    break;
                }
                if (data == null)
                {
                    data = new DriverData();
                }
               // data.driver_inf = manObj["InfName"].ToString();
                data.type = DriverTypeDM.DRIVER_TYPE_PRINTER;
                data.driver_version = manObj["DriverName"].ToString();
                key = manObj.GetPropertyValue("DeviceID").ToString().Replace("\\", "\\\\");
               
                foreach (var queryObj in CIMcollection)
                {
                   
                    string param = queryObj["Antecedent"].ToString();
                    if (param.IndexOf(key) > 0)
                    {

                        int len = queryObj["Dependent"].ToString().Length;
                        if (len > 0)
                        {
                            // int pos = queryObj["Dependent"].ToString().IndexOf('"');
                            int pos = queryObj["Dependent"].ToString().LastIndexOf('\\');
                            if (pos > 0)
                            {
                                if (data.installed_drivers != null)
                                {
                                    data.installed_drivers += ", ";
                                }
                                data.installed_drivers += queryObj["Dependent"].ToString().Substring (pos + 1, len - pos - 2);
                              
                            }
                        }
                    }
                }
            }

            return data;
        }


        private string GetShortPath(string driver_path)
        {
            if (driver_path != null)
            {
                int len = driver_path.Length;
                if (len > 0)
                {
                    int pos = driver_path.LastIndexOf('\\');
                    if (pos > 0)
                    {
                       driver_path = driver_path.Substring(pos + 1, len - pos - 1);
                    }
                }
            }
            return driver_path;
        }
         
        public DriverData GetPrinterDriverInfoA ()
        {
            DriverData data = null;
            PrinterSettings settings = new PrinterSettings();

            WinSpoolPrinterInfo wpi = new WinSpoolPrinterInfo();
            WinSpoolPrinterInfo.DRIVER_INFO_8? Info8 = wpi.GetPrinterInfo(settings.PrinterName);

            if (Info8.HasValue)
            {
                data = new DriverData();
                data.type = DriverTypeDM.DRIVER_TYPE_PRINTER;
                data.device_name = Info8.Value.pName;
                data.driver_inf = GetShortPath (Info8.Value.pszInfPath);
                data.installed_drivers = GetShortPath(Info8.Value.pDriverPath);
                if (Info8.Value.pDriverPath != null)
                {
                    data.driver_file_version = GetFileVersion(Info8.Value.pDriverPath);
                }
                if (Info8.Value.pszInfPath != null)
                {
                    data.inf_modified = File.GetLastWriteTime(Info8.Value.pszInfPath);
                }
                else if (Info8.Value.pDriverPath != null)
                {
                    data.inf_modified = File.GetLastWriteTime(Info8.Value.pDriverPath);
                }
                data.inf_modified = data.inf_modified.AddTicks(-data.inf_modified.Ticks % 10000000);
                data.mnf_name = Info8.Value.pszMfgName;
                long ft = (((long)Info8.Value.ftDriverDate.dwHighDateTime)<< 32) + Info8.Value.ftDriverDate.dwLowDateTime;
                data.date_modified = DateTime.FromFileTime(ft);

               
                if (Info8.Value.dwlDriverVersion != null)
                {
                    data.driver_version = Info8.Value.dwlDriverVersion.ToString();
                }              

            }
            
            return data;
        }

        private string GetFileVersion (string file)
        {
            string version = null;
            try
            {
                FileVersionInfo ver = FileVersionInfo.GetVersionInfo(file);
                version = ver.FileVersion;
            }
            catch (Exception e)
            {

            }
            return version;

        }

        private DriverData GetFTDIBaseDriverInfo(string device_name, DriverTypeDM type)
        {
            try
            {
                string ft_select = string.Format("SELECT * FROM Win32_PnPEntity WHERE Manufacturer = 'FTDI' and Name like '{0}%'", device_name);
                return GetDriverInfo(ft_select, type);
            }
            catch
            {
                return null;
            }
          
        }

        private DriverData GetTouchScreenDriverInfo ()
        {
             DriverData dm = null;

            try
            {
            
                string elo_select = "SELECT * FROM Win32_PnPEntity WHERE Manufacturer = 'Elo Touch Solutions'";
                dm = GetDriverInfo(elo_select, DriverTypeDM.DRIVER_TYPE_TOUCH_SCREEN);

                if (dm == null)
                {
                    string _3m_select = "SELECT * FROM Win32_PnPEntity WHERE Manufacturer = '(3M devices)'and Name like '%Sensor%'";
                    dm = GetDriverInfo (_3m_select, DriverTypeDM.DRIVER_TYPE_TOUCH_SCREEN);
                }
            }
            catch
            {
                dm = null;
            }

            return dm;
        }

        private void AddToDriverDB(DriverData item)
        {
            if (item != null)
            {
                DriverDB.Add(item);
            }
           
        }

        private string FindCashValidatorSerialPort()
        {
            string port = null;
            List<SerialPortManager.CommunicationResource> sp = spm.GetSafeSerialPortsMap();

            foreach (CommunicationResource cr in sp)
            {
                if (cr.PortType == ResourceType.BILL_VALIDATOR_SERIAL_PORT)
                {
                    port = cr.PortName;
                    break;
                }
                if (cr.PortType == ResourceType.COIN_ACCEPTOR_SERIAL_PORT)
                {
                    port = cr.PortName;
                    break;
                }
            }

            return port;
        }

        private string FindCardreaderSerialPort ()
        {
            string port = null;
            List<SerialPortManager.CommunicationResource> sp = spm.GetSafeSerialPortsMap();

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

        private void ScanDrivers ()
        {
            try
            {              
               CIMcollection = GetSystemDriverDataFile();
               LoadOldDriversList();
               string cash_port = FindCashValidatorSerialPort ();
               string card_reader_port = FindCardreaderSerialPort();
               lock (DriverDB)
               {
                   DriverDB.Clear();
                   if (!String.IsNullOrEmpty(cash_port))
                   {
                       string select = String.Format ("SELECT * FROM Win32_PnPEntity WHERE Name Like '%{0}%'", cash_port);
                       AddToDriverDB(GetDriverInfo (select, DriverTypeDM.DRIVER_TYPE_DA2));
                   }
                   if (!String.IsNullOrEmpty(card_reader_port))
                   {
                       AddToDriverDB(GetFTDIBaseDriverInfo("%" + card_reader_port, DriverTypeDM.DRIVER_TYPE_ID_READER));
                   }
                   else
                   {
                       string select = "SELECT * FROM Win32_PnPEntity WHERE Name Like '%SDK%'";
                       AddToDriverDB (GetDriverInfo (select, DriverTypeDM.DRIVER_TYPE_ID_READER));
                   }
                 
                   AddToDriverDB(GetTouchScreenDriverInfo());
                   AddToDriverDB(GetPrinterDriverInfoA());
                   //DriverDB = DriverDB.Concat(GetVideoControllertDriverInfo()).ToList();
                   TrackDriversChange();
               }
            }
            catch
            {
            }

        }

        public static DriversMonitor Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new DriversMonitor();
                        }
                     }
                 }

                 return instance;
            }
        }

        void Run(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {
                ScanDrivers();
                Thread.Sleep(timeOut);
            }
        }

    }
}
