using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using IocContainer;
using Nbt.Services.Scf.CashIn;
using Nbt.Services.Scf.CashIn.Validator;
using Nbt.Services.Scf.CashIn.Validator.CCTalk;
using Nbt.Services.Spf.Printer;
using Ninject;
using Preferences.Services.Preference;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Models.Repositories;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportBetting.WPF.Prism.Shared.Resources;
using WsdlRepository.WsdlServiceReference;
using eStationType = SportBetting.WPF.Prism.Shared.Models.Repositories.eStationType;
using System.Collections.Generic;

namespace SportBetting.WPF.Prism.OldCode
{
    public class StationSettings : IStationSettings
    {

        public enum ActivePrinter
        {
            USE_DEFAULT = 0,
            SET_ACTIVE_AS_DEFAULT = 1,
            SET_ACTIVE_USB_PRINTER = 2
        }

        //private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(StationSettings));
        private static ILog Log = LogFactory.CreateLog(typeof(StationSettings));


        private static StationSettings _StationSettings = null;
        private static string prefFileName;

        private static volatile CashInManager cashin;
        public static CashInManager CashIn
        {
            get
            {
                return cashin;
            }
        }
        //private bool keyMenuShowing = false;
        private static readonly object lockObj = new Object();
        //private decimal moneyIn;

        private static CultureInfo ci;
        public static IPrefSupplier PrefFile; //Used for hardware settings
        //	    private CardReader cardReader;
        private static int printerMargin = 5;
        private static int printerStatus = PrinterHandler.PRINTER_NOT_FOUND;
        private static ActivePrinter ActivePrinterIsDefault = ActivePrinter.USE_DEFAULT;


        public void Init()
        {


            //1) First of all read StationNumber from PrefFile
            PrefFile = new TextFilePref(PrefFileName, PrefFileName, true, false);
            string message = null;
            Log.Debug("Thread:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (PrefFile.Name == "ERROR")
            {
                throw new FileNotFoundException("Preffile loading error");
            }
            if (PrefFile.Name == "DUPLICATE")
            {
                message = "Pref-File corrupt. Station locked. Please check for duplicate lines (also in comments)";
            }

            if (message != null)
            {
                Log.Error(message, new Exception());
            }
        }

        public string PrefFileName
        {
            get { return prefFileName; }
            set { prefFileName = value; }
        }

        public void CashInDisable()
        {
            CashIn.Disable();
        }

        public bool IsCashDatasetValid()
        {
            bool result = false;
            if (CashIn != null)
            {
                result = CashIn.IsDataSetValid();
            }
            return result;
        }

        public void UnSubscribeCashin(EventHandler<CashInEventArgs> depositCashInCashIn)
        {
            CashIn.CashIn -= depositCashInCashIn;
        }


        public static bool CashinOK { get; set; }


        public bool TurnOffCashInInit { get; set; }
        public bool UsePrinter { get; set; }
        public IPrinter Printer { get; set; }

        public int PrinterStatus
        {
            get { return printerStatus; }
            set { printerStatus = value; }
        }
        public CultureInfo Culture
        {
            get { return ci; }
            set { ci = value; }
        }


        public void AddTestMoNeyFromKeyboard(decimal money)
        {
            if (Active == (int)StationSr.STATION_TEST || Debugger.IsAttached)
            {

                if (CashIn != null && IsCashInEnabled)
                {
                    if (money > Limit)
                    {
                        cashin.CashLimitExceededEventHandler.DynamicInvoke(null, null);
                    }
                    else
                    {
                        foreach (Delegate action in CashIn.GetInvocationList())
                        {
                            action.DynamicInvoke(null, new CashInEventArgs(money, money <= 2));
                        }
                    }
                }
            }

        }

        public int Active { get; set; }

        private static IChangeTracker ChangeTracker { get { return IoCContainer.Kernel.Get<IChangeTracker>(); } }


        public static User CurrentUser
        {
            get { return ChangeTracker.CurrentUser; }
        }

        //XXX Reduce visibility
        private static readonly object oLock = new object();

        private static void cashin_LimitExceeded(object sender, ValidatorEventArgs<string> e)
        {
        }

        private static void cashin_CashIn(object sender, CashInEventArgs e)
        {

            lock (oLock)
            {
                LogCash(e.MoneyIn);

                string name = "Terminal";
                var loggedInUser = CurrentUser as SportBetting.WPF.Prism.Models.LoggedInUser;
                if (loggedInUser != null)
                    name = loggedInUser.Username;
                else
                {
                    var operatorUser = CurrentUser as SportBetting.WPF.Prism.Models.OperatorUser;
                    if (operatorUser != null)
                        name = operatorUser.Username;
                }

                StationCashSr cash = new StationCashSr() { Cash = e.MoneyIn, MoneyIn = true, OperationType = "STATION_CASH_IN", OperatorID = name, CashCheckPoint = false, DateModified = DateTime.Now };

                using (var con = ConnectionManager.GetConnection())
                {
                    cash.Save(con, null);
                }
            }
            IoCContainer.Kernel.Get<IChangeTracker>().MouseClickLastTime = DateTime.Now;


        }


        private static void LogCash(decimal dMoneyIn)
        {
            string sWeekNumber = GetWeekNumber(DateTime.Now);
            string sFilePath = Convert.ToString(ConfigurationManager.AppSettings["CashLogFile"]);
            sFilePath = sFilePath.Replace("___", "_w" + sWeekNumber);


            using (StreamWriter file = new StreamWriter(sFilePath, true))
            {
                file.WriteLine(string.Format("{0}\t{1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), dMoneyIn.ToString()));
            }
        }

        private static string GetWeekNumber(DateTime dt)
        {
            //DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dt);
            //if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            //{
            //    dt = dt.AddDays(3);
            //}

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString() + dt.Year.ToString();
        }


        static int? syncInterval;

        public int SyncInterval
        {
            get
            {
                if (syncInterval != null)
                    return syncInterval.Value;
                return Convert.ToInt32(ConfigurationManager.AppSettings["INITIAL_SYNC_INTERVAL_SEC"]);
            }
            set { syncInterval = value; }
        }








        #region querypref help

        /// <summary>
        /// Old method name to query normal PrefFile.txt
        /// </summary>
        /// <param name="key">the key to search for</param>
        /// <param name="defaultVal">the default value to return if no value is found</param>
        /// <returns>the value stored in the prefFile or the defaultValue </returns>
        private static object QueryPref(string key, object defaultVal)
        {
            return QueryPref(PrefFile, key, defaultVal);
        }


        /// <summary>
        /// reads from Preffile and Settingsfile, if entry not in preffile uses settings and stores value in preffile
        /// IMPORTANT: If one entry in duplicate, the whole pref-file bevomes invalid. This means even duplicate commented lines are not allowed.
        /// e.g.
        /// #PrinterType = 1
        /// #PrinterType = 99
        /// --> error, whole pref-file is invalid, no data can be read from pref-file
        /// </summary>
        /// <param name="curPref">Preference-Object to search</param>
        /// <param name="key">the key to search for</param>
        /// <param name="defaulVal">the default value to return, if no data is found</param>
        /// <returns></returns>
        private static object QueryPref(IPrefSupplier curPref, string key, object defaultVal)
        {
            object val = null;

            //if (val.GetType() != defaulVal.GetType())
            //    return null;
            if (string.IsNullOrEmpty(key))
            {
                return defaultVal;
            }

            try
            {
                switch (defaultVal.GetType().FullName)
                {
                    case "System.Boolean":
                        val = curPref.GetBooleanEntry(key);
                        if (val == null)
                        {
                            val = defaultVal;
                            //							curPref.SetBooleanEntry(key, (bool)val);
                        }
                        else
                        {
                            val = (bool)val;
                        }
                        break;

                    case "System.Int32":
                        val = curPref.GetIntegerEntry(key);
                        if (val == null)
                        {
                            val = defaultVal;
                            //							curPref.SetIntegerEntry(key, (int)val);
                        }
                        else
                        {
                            val = (int)val;
                        }
                        break;

                    case "System.String":
                        val = curPref.GetStringEntry(key);
                        if (val == null)
                        {
                            val = defaultVal;
                            //							curPref.SetStringEntry(key, (string)val);
                        }
                        break;

                    case "System.Double":
                        val = curPref.GetDoubleEntry(key);
                        if (val == null)
                        {
                            val = defaultVal;
                            //							curPref.SetDoubleEntry(key, (double)val);
                        }
                        else
                        {
                            val = (double)val;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (SystemException e)
            {
                //e.g. InvalidCastException or NullReferenceException
                Log.Error("pref read error: key: " + key, e);
                return defaultVal;
            }

            return val;
        }

        #endregion

        #region INotifyPropertyChanged Members


        #endregion


        public enum StationSr
        {
            STATION_LOCKED = 0,
            STATION_ACTIVE = 1,
            STATION_NOODDS = 2,
            STATION_TEST = 3
        }

        public decimal Limit { get; set; }
        public void EnableCashIn(decimal stake, decimal limit)
        {
            try
            {
                if (CashIn == null)
                    InitializeCashIn();

                if (CashIn != null)
                {
                    CashIn.SetCredit(stake);
                    CashIn.Enable(limit);
                    IsCashInEnabled = limit > stake;
                    Limit = limit;

                }
            }
            catch
            {

            }

        }

        public List<DeviceInfo> GetDeviceInventoryList()
        {
            if (CashIn == null)
            {
                return null;
            }
            return CashIn.GetDeviceInventory();
        }

        public static List<DeviceInfo> GetShortDeviceInventoryList()
        {
            if (CashIn == null)
            {
                return null;
            }
            return CashIn.GetShortDeviceInventory();
        }

        public bool CheckBillValidator()
        {
            if (CashIn == null)
            {
                return false;
            }
            return CashIn.CheckBillValidator();
        }
        public bool CheckCoinAcceptor()
        {
            if (CashIn == null)
            {
                return false;
            }
            return CashIn.CheckCoinAcceptor();
        }

        public bool IsCashInEnabled { get; set; }
        public string Language { get; set; }


        [Localizable(false)]
        private static void ParsePrintersStatus()
        {
            int active_printer = 0;
            bool result = false;
            string DefaultPrinterName = null;
            active_printer = (int)QueryPref("ActivePrinterIsDefault", active_printer);
            ActivePrinterIsDefault = (ActivePrinter)active_printer;

            if (ActivePrinterIsDefault == ActivePrinter.SET_ACTIVE_AS_DEFAULT ||
                ActivePrinterIsDefault == ActivePrinter.SET_ACTIVE_USB_PRINTER)
            {

                ManagementObjectSearcher searcher;
                searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Default = True");
                foreach (ManagementObject defPrinter in searcher.Get())
                {
                    if (defPrinter["WorkOffline"].ToString().ToLowerInvariant() == "false")
                    {
                        Log.Info("Default Printer " + defPrinter["Name"].ToString() + "is Online");
                        break;
                    }
                    Log.Info("Default printer is not online. Try to finde online printer");
                    DefaultPrinterName = defPrinter["Name"].ToString();
                    if (ActivePrinterIsDefault == ActivePrinter.SET_ACTIVE_USB_PRINTER)
                    {
                        searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE PortName LIKE '%USB%'");
                    }
                    else
                    {
                        searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Default = False");
                    }
                    foreach (ManagementObject defPrint in searcher.Get())
                    {
                        defPrint.InvokeMethod("SetDefaultPrinter", new object[] { defPrint["Name"].ToString() });
                        if (defPrint["WorkOffline"].ToString().ToLowerInvariant() == "false")
                        {
                            Log.Info("Set Printer " + defPrint["Name"].ToString() + "as Default");
                            result = true;
                            break;
                        }

                    }
                    if (!result)
                    {
                        if (!String.IsNullOrEmpty(DefaultPrinterName))
                        {
                            //If we did not finde enything, restore original 
                            defPrinter.InvokeMethod("SetDefaultPrinter", new object[] { DefaultPrinterName });
                        }
                    }

                }

            }
        }
        public void ReadPrefFileData()
        {
            //ActivePrinterIsDefault=1
            UsePrinter = (bool)QueryPref("UsePrinter", UsePrinter);
            ParsePrintersStatus();
            // wird nicht mehr gebraucht, Zuweisung oben auf Grund des Discriminator-Typ
            printerMargin = (int)QueryPref("MarginDefaultPrinter", printerMargin);

            bool createPrinterSuccess = false;

            if (Printer == null)
            {

                PrinterOffline();
                try
                {
                    Printer = new DefaultPrinter(270) { Margin = printerMargin };

                    createPrinterSuccess = true;
                }
                catch (Exception ex)
                {
                    Log.Debug("In createPrinter: " + ex);
                    createPrinterSuccess = false;
                }
                if (!createPrinterSuccess)
                {
                    Log.Error(LogMessages.StationSettings10, new Exception());
                }
            }
            if (!UsePrinter)
            {
                //no warning, if no printer is found
                printerStatus = PrinterHandler.PRINTER_READY;
            }

            /*
            checkValues("Validator1_Channel_01", 5);
            checkValues("Validator1_Channel_02", 10);
            checkValues("Validator1_Channel_03", 20);
            checkValues("Validator1_Channel_04", 50);
            checkValues("Validator1_Channel_05", 100);
            checkValues("Validator1_Channel_06", 200);
            checkValues("Validator1_Channel_07", 500);

            checkValues("Validator2_Channel_01", 10);
            checkValues("Validator2_Channel_02", 20);
            checkValues("Validator2_Channel_03", 50);
            checkValues("Validator2_Channel_04", 100);
            checkValues("Validator2_Channel_05", 200);
            checkValues("Validator2_Channel_06", 0);

             */
            //GMA 23.08.2010 entfernt
            //checkValues("Validator1_Type", "InnovativeTechnology.NV10");
            //checkValues("Validator2_Type", "Comestero.RM5");

            // InitializeCashIn();
        }

        private static readonly object locker = new object();

        public CashInManager GetCashInManagerInstance()
        {
            string mode = "Active";

            if (Active == (int)StationSr.STATION_TEST)
            {
                mode = "TestMode";
            }

            if (cashin == null)
            {
                lock (locker)
                {
                    if (cashin == null && PrefFile != null)
                    {
                        cashin = new CashInManager(PrefFile, mode);

                        cashin.CashIn += cashin_CashIn;
                        cashin.CashLimitExceededEventHandler += cashin_LimitExceeded;
                    }
                }
            }
            return cashin;

        }

        public void InitializeCashIn()
        {
            Log.Debug("Init cashin");

            if (!TurnOffCashInInit)
            {
                try
                {
                    cashin = GetCashInManagerInstance();

                    if (CashIn != null && !CashIn.IsDataSetValid())
                    {
                        CashinOK = false;
                        Log.Info("Cash validator dataset is invalid!");
                        throw new CashinDatasetException("Cash validator dataset is invalid!");
                    }
                    if (CashIn != null && !CashIn.AceptorsFound)
                    {
                        Log.Error("Cash validators are not found! Geldschein- und Münzprüfer nicht gefunden", new Exception());
                        CashinOK = false;
                        if (!CashinOK)
                        {
                            AsyncConnectCashin();
                            if (!System.Diagnostics.Debugger.IsAttached)
                                throw new CashinException("Cashin is not working!");
                        }
                    }
                    else
                    {
                        CashinOK = true;
                        //  AssignCashInEventEx();
                    }
                }
                catch (CashinException e)
                {
                    throw;
                }
                catch (CashinDatasetException e)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Error("Error initialising Cash-Hardware", ex);
                }
            }
        }

        private void AsyncConnectCashin()
        {
            new Thread(() =>
                {
                    while (!CashinOK)
                    {
                        ConnectCashin();
                        Thread.Sleep(6000000);
                    }
                }).Start();

        }

        private void ConnectCashin()
        {
            if (!TurnOffCashInInit)
            {
                if (!CashinOK)
                {
                    try
                    {
                        //cashin = new CashInManager(PrefFile);
                        cashin = GetCashInManagerInstance();
                        //AssignCashInEventEx();
                        if (!CashIn.AceptorsFound)
                        {
                            Log.Error("Geldschein- und Münzprüfer nicht gefunden", new Exception());
                            CashinOK = false;

                        }
                        else
                        {
                            CashinOK = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        cashin = null;
                        Log.Error("Error initialising Cash-Hardware", ex);
                    }
                }


            }
        }

        /* public static void AssignCashInEventEx()
         {
             if (CashIn != null)
             {
                 CashIn.CashIn += cashin_CashIn;
                 CashIn.CashLimitExceededEventHandler += cashin_LimitExceeded;
             }
         }*/

        private static bool PrinterOffline()
        {
            return PrinterOffline("", false);
        }
        private static bool PrinterOffline(string name, bool port)
        {
            ManagementScope scope = new ManagementScope(@"\root\cimv2");
            scope.Connect();

            // Select Printers from WMI Object Collections
            ManagementObjectSearcher searcher;
            if (port)
            {
                searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE PortName = '" + name + "'");
            }
            else if (name != "")
            {
                searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Name LIKE '%" + name.ToLowerInvariant() + "%'");
            }
            else
            {
                searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer WHERE Default = 'true'");
            }


            string printerFullName = "";
            int count = 0;
            foreach (ManagementObject printer in searcher.Get())
            {
                count++;
                printerFullName = printer["Name"].ToString();
                if (printer["WorkOffline"].ToString().ToLowerInvariant().Equals("true"))
                {
                    Log.Debug("OfflinePrinter: " + printerFullName + " is offline");
                    return true;
                }
            }
            if (count == 0)
            {
                Log.Debug("OfflinePrinter: no printer found");
                return true;
            }
            return false;
        }


        private static void checkValues(string pChannelName, int pExpectedValue)
        {
            checkValues(pChannelName, pExpectedValue.ToString());
        }

        private static void checkValues(string pChannelName, string pExpectedValue)
        {
            string prefFileValue = PrefFile.GetStringEntry(pChannelName);
            if (prefFileValue != null && prefFileValue != pExpectedValue && prefFileValue != "0")
            {
                Log.Error(String.Format(LogMessages.StationSettings14, pExpectedValue, prefFileValue), new Exception());
            } //Do not write warning, if Value is set to 0
        }


        public bool IsCashinOk { get { return CashIn != null; } }
        public void SubscribeCashin(EventHandler<CashInEventArgs> asyncAddMoney)
        {
            CashIn.CashIn += asyncAddMoney;
        }

        public void SubscribeLimitExceeded(EventHandler<ValidatorEventArgs<string>> limitExceeded)
        {
            cashin.CashLimitExceededEventHandler += limitExceeded;
        }
    }


    public class CashinException : Exception
    {
        public CashinException(string cashinIsNotWorking)
            : base(cashinIsNotWorking)
        {
        }
    }

    public class CashinDatasetException : Exception
    {
        public CashinDatasetException(string cashinIsNotWorking)
            : base(cashinIsNotWorking)
        {
        }
    }
}