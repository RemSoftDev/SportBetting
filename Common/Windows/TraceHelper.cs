using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;

namespace Nbt.Common.Utils.Windows
{
    public static class TraceHelper
    {
        private const string DEFAULT_LONG_VALUES = "1000,2000,3000";

        private static ILog m_logger = LogFactory.CreateLog(typeof(TraceHelper));

        private static readonly TimeSpan CACHED_TEXT_TIMEOUT = new TimeSpan(0, 0, 5);

        public delegate void DelegateLongFromRegistryFound(long lFoundLong, params object[] args);
        public delegate void DelegateStringFromRegistryFound(string sFoundString, params object[] args);

        private class CachedLongValues
        {
            public static readonly char[] COMMA = new char[] { ',' };

            public string Value { get; private set; }
            public SyncHashSet<long> Values { get; private set; }
            public DateTime Time { get; private set; }

            private object m_oLocker = new object();

            public CachedLongValues()
            {
                this.Value = string.Empty;
                this.Values = new SyncHashSet<long>();
            }

            public void Initialize(string sValue)
            {
                lock (m_oLocker)
                {
                    this.Time = DateTime.UtcNow;

                    if (this.Value != sValue)
                    {
                        string[] arrSearch = sValue.Split(COMMA, StringSplitOptions.RemoveEmptyEntries);

                        this.Values.Clear();
                        long lValue = 0;

                        foreach (string sSearch in arrSearch)
                        {
                            if (long.TryParse(sSearch.Trim(), out lValue))
                            {
                                this.Values.Add(lValue);
                            }
                        }
                    }
                }
            }
        }

        private static SyncDictionary<string, CachedLongValues> m_di = new SyncDictionary<string, CachedLongValues>();

        private static DirectoryInfo m_diEntryAssemblyLocation = null;

        public static bool WriteTextToFile(string sFileName, string sText)
        {
            try
            {
                using (FileStream fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.Write(sText);
                        sw.Flush();

                        return true;
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.WarnFormat("WriteTextToFile(sFileName='{0}', sText='{1}') ERROR. {2}", sFileName, sText, excp.Message);
            }

            return false;
        }

        public static string ReadTextFromFile(string sFileName)
        {
            try
            {
                using (FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                if (WriteTextToFile(sFileName, DEFAULT_LONG_VALUES))
                {
                    m_logger.InfoFormat(@"Trace file '{0}' (value '{1}') Successfully created.", sFileName, DEFAULT_LONG_VALUES);
                }
            }
            catch (Exception excp)
            {
                m_logger.WarnFormat("ReadTextFileFile(sFileName='{0}') ERROR. {1}", sFileName, excp.Message);
            }

            return string.Empty;
        }

        private static CachedLongValues ReadCachedLongValuesFromFile(string sFileName)
        {
            CachedLongValues clv = m_di.SafelyGetValue(sFileName);

            if (clv == null)
            {
                string sValue = ReadTextFromFile(GetTracePath(sFileName));

                clv = new CachedLongValues();
                clv.Initialize(sValue);

                m_di[sFileName] = clv;
            }
            else if (clv.Time + CACHED_TEXT_TIMEOUT < DateTime.UtcNow)
            {
                string sValue = ReadTextFromFile(GetTracePath(sFileName));
                clv.Initialize(sValue);
            }

            return clv;
        }

        public static DirectoryInfo EntryAssemblyLocation
        {
            get
            {
                if (m_diEntryAssemblyLocation == null)
                {
                    var assembly = System.Reflection.Assembly.GetEntryAssembly();

                    string sEntryAssemblyLocation = Path.GetDirectoryName(".\\");
                    if (assembly != null)
                        sEntryAssemblyLocation = Path.GetDirectoryName(assembly.Location);
                    m_diEntryAssemblyLocation = new DirectoryInfo(sEntryAssemblyLocation);

                    Debug.Assert(m_diEntryAssemblyLocation.Exists);
                }

                return m_diEntryAssemblyLocation;
            }
        }

        public static string GetTracePath(string sFileName)
        {
            return Path.Combine(EntryAssemblyLocation.Parent.FullName, sFileName);
        }

        public static bool TraceFromFileLongValues(string sFileName, long lCurrentValue, DelegateLongFromRegistryFound dlfrf, params object[] args)
        {
            CachedLongValues clv = ReadCachedLongValuesFromFile(sFileName);

            if (clv != null && clv.Values.Contains(lCurrentValue))
            {
                dlfrf(lCurrentValue, args);
                return true;
            }

            return false;
        }

        /*
        public static void TraceFromRegistryStringValues(string sRegistryKeyName, string sValueName, string sCurrentValue, DelegateStringFromRegistryFound dsfrf, params object[] args)
        {
            try
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(sRegistryKeyName))
                {
                    if (rk != null)
                    {
                        string sValue = (string)rk.GetValue(sValueName);

                        if (!string.IsNullOrEmpty(sValue))
                        {
                            string[] arrSearch = sValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string sSearch in arrSearch)
                            {
                                try
                                {
                                    if (string.Compare(sSearch, sCurrentValue, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        dsfrf(sSearch, args);
                                    }
                                }
                                catch (Exception excp)
                                {
                                    m_logger.Error(ExcpHelper.FormatException(excp, "TraceFromRegistryStringValues(sRegistryKeyName='{0}', sValueName='{1}', sCurrentValue='{2}') ERROR for '{3}'", sRegistryKeyName, sValueName, sCurrentValue, sSearch));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "TraceFromRegistryStringValues(sRegistryKeyName='{0}', sValueName='{1}', sCurrentValue='{2}') GENERAL ERROR", sRegistryKeyName, sValueName, sCurrentValue);
            }
        }

        private static void SetValue(RegistryKey rk, string sValueName, string sDefaultValue)
        {
            string sValue = (string)rk.GetValue(sValueName);

            if (!string.IsNullOrEmpty(sValue))
            {
                m_logger.InfoFormat(@"Registry key '{0}\{1}' already exists and contains value '{2}'", rk.Name, sValueName, sValue);
            }
            else
            {
                rk.SetValue(sValueName, sDefaultValue);
                m_logger.InfoFormat(@"Registry key '{0}\{1}' (value '{2}') Successfully created.", rk.Name, sValueName, sDefaultValue);
            }
        }

        public static void InitializeRegistryKey(string sRegistryKeyName, string sValueName, string sDefaultValue)
        {
            try
            {
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(sRegistryKeyName, true))
                {
                    if (rk != null)
                    {
                        SetValue(rk, sValueName, sDefaultValue);
                    }
                    else
                    {
                        using (RegistryKey rkNew = Registry.LocalMachine.CreateSubKey(sRegistryKeyName))
                        {
                            SetValue(rkNew, sValueName, sDefaultValue);
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "InitializeRegistryKey(sRegistryKeyName='{0}', sValueName='{1}', sDefaultValue='{2}') GENERAL ERROR", sRegistryKeyName, sValueName, sDefaultValue);
            }
        }
        */

        public static void InitializeTraceFile(string sFileName, string sDefaultValue)
        {
            string sValue = ReadTextFromFile(GetTracePath(sFileName));

            if (!string.IsNullOrEmpty(sValue))
            {
                m_logger.InfoFormat(@"Trace file '{0}' already exists and contains value '{1}'", sFileName, sValue);
            }
            else if (WriteTextToFile(GetTracePath(sFileName), sDefaultValue))
            {
                m_logger.InfoFormat(@"Trace file '{0}' (value '{1}') Successfully created.", sFileName, sDefaultValue);
            }
        }
    }
}
