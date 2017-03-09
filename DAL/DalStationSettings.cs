using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using SportRadar.Common.Extensions;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL
{
    public class DalStationSettings
    {
        public const string DEFAULT_LANGUAGE = "en";
        public readonly static CultureInfo EN_US = new CultureInfo("en-US");

        protected static DalStationSettings m_Instance = null;
        protected static object m_oInstanceLocker = new Object();
        private int _maxSearchResult;

        public DalStationSettings()
        {
            this.EnableCheckTime = StringToString(GetAppSettings("enable_check_time"), "Undefined");
            this.DatabaseDialect = StringToString(GetAppSettings("database_dialect"), string.Empty);
            this.DatabaseName = StringToString(GetAppSettings("database_name"), string.Empty);
            this.ConnectionString = StringToString(GetAppSettings("database_connection_string"), string.Empty);
            this.DatabaseSchemaConfig = StringToString(GetAppSettings("database_schema_config"), string.Empty);
            this.Language = StringToString(GetAppSettings("language"), DEFAULT_LANGUAGE);

            this.LiveBetHost = StringToString(GetAppSettings("LiveBetHost"), string.Empty);
            this.LiveBetPort = StringToInt(GetAppSettings("LiveBetPort"), 0);
            this.VirtualLiveBetPort = StringToInt(GetAppSettings("VirtualLiveBetPort"), this.LiveBetPort);
            this.LiveTimeOutToDisableMilliseconds = StringToInt(GetAppSettings("LiveTimeOutToDisableMilliseconds"), 5000);
            this.LiveTimeOutToRemoveMilliseconds = StringToInt(GetAppSettings("LiveTimeOutToRemoveMilliseconds"), 25000);
            this.LineCleanerPeriodInSec = StringToInt(GetAppSettings("LineCleanerPeriodInSec"), 600);
            this.MaxSearchResult = StringToInt(GetAppSettings("MaxSearchResult"), 10);

            this.LineProviderName = StringToString(GetAppSettings("live_provider_name"), string.Empty);

            this.BetDomainMap = StringToString(GetAppSettings("bet_domain_map"), string.Empty);

            this.LigaStavokLiveInitial = StringToString(GetAppSettings(@"LigaStavokLiveInitial"), @"live/xml");
            this.LigaStavokLiveDelta = StringToString(GetAppSettings(@"LigaStavokLiveDelta"), @"live/xmld");
            this.LigaStavokPreMatchInitial = StringToString(GetAppSettings(@"LigaStavokPreMatchInitial"), @"prematch/xml");
            this.LigaStavokPreMatchDelta = StringToString(GetAppSettings(@"LigaStavokPreMatchDelta"), @"prematch/xmld");

            this.UseDatabaseForPreMatches = StringToBool(GetAppSettings(@"UseDatabaseForPreMatches"), true);
            this.UseDatabaseForLiveMatches = StringToBool(GetAppSettings(@"UseDatabaseForLiveMatches"), true);
            this.UseDatabaseForOthers = StringToBool(GetAppSettings(@"UseDatabaseForOthers"), true);

            this.LiveWarnQueueSize = StringToInt(GetAppSettings(@"LiveWarnQueueSize"), 5);
            this.LiveErrorQueueSize = StringToInt(GetAppSettings(@"LiveErrorQueueSize"), 50);
            this.EnableRunProcessControl = StringToBool(GetAppSettings(@"EnableRunProcessControl"), true);

            RestService = StringToString(GetAppSettings("rest_service"), "http://arsentyev:8080/SportREST");
            RestServiceRequests = StringToString(GetAppSettings("rest_service_requests"), "http://arsentyev:8080/SportREST");
            ClientCertificateSubject = StringToString(GetAppSettings("client_certificate_subject"), "431");
        }

        public static DalStationSettings Instance
        {
            get
            {
                lock (m_oInstanceLocker)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = new DalStationSettings();
                    }

                    return m_Instance;
                }
            }
        }

#if DEBUG

        public void SetDatabaseDebug(string sDatabaseDialect, string sConnectionString, string sDatabaseName)
        {
            this.DatabaseDialect = sDatabaseDialect;
            this.ConnectionString = sConnectionString;
            this.DatabaseName = sDatabaseName;
        }

#endif

        private Dictionary<string, StationAppConfigSr> _stationAppConfig;
        public Dictionary<string, StationAppConfigSr> GetStationAppConfig
        {
            get
            {
                if (_stationAppConfig == null)
                {
                    _stationAppConfig = new Dictionary<string, StationAppConfigSr>();

                    List<StationAppConfigSr> lConfigs = StationAppConfigSr.GetAllSettings();

                    foreach (var stationAppConfigSr in lConfigs)
                    {
                        _stationAppConfig.Add(stationAppConfigSr.PropertyName, stationAppConfigSr);
                    }
                }
                return _stationAppConfig;

            }
        }


        public string StationNumber
        {
            get
            {
                if (!GetStationAppConfig.ContainsKey("StationNumber"))
                {
                    return StringToString(GetAppSettings("station_number"), string.Empty);
                }
                return GetStationAppConfig["StationNumber"].ValueString;
            }
        }

        public string RestService { get; private set; }
        public string RestServiceRequests { get; private set; }
        public string ClientCertificateSubject { get; private set; }

        public string DatabaseDialect { get; protected set; }
        public string DatabaseName { get; protected set; }
        public string ConnectionString { get; protected set; }
        public string DatabaseSchemaConfig { get; protected set; }

        public string Language { get; set; }
        public string EnableCheckTime { get; protected set; }

        // Live Bet
        public string LiveBetHost { get; protected set; }
        public int LiveBetPort { get; protected set; }
        public int VirtualLiveBetPort { get; protected set; }
        public int LiveTimeOutToDisableMilliseconds { get; protected set; }
        public int LiveTimeOutToRemoveMilliseconds { get; protected set; }

        // Cleaner
        public int LineCleanerPeriodInSec { get; protected set; }

        // Liga Stavok Line Rest Service
        public string LigaStavokLiveInitial { get; protected set; }
        public string LigaStavokLiveDelta { get; protected set; }
        public string LigaStavokPreMatchInitial { get; protected set; }
        public string LigaStavokPreMatchDelta { get; protected set; }

        public bool CreateDatabase { get { return StringToBool(GetAppSettings(@"CreateDatabase"), true); } }
        public bool UseDatabaseForPreMatches { get; protected set; }
        public bool UseDatabaseForLiveMatches { get; protected set; }
        public bool UseDatabaseForOthers { get; protected set; }
        public bool UseDatabaseForLine { get { return this.UseDatabaseForPreMatches || this.UseDatabaseForLiveMatches; } }

        // Live Client Settings
        public int LiveWarnQueueSize { get; protected set; }
        public int LiveErrorQueueSize { get; protected set; }
        public bool EnableRunProcessControl { get; protected set; }

        // Line
        public string LineProviderName { get; protected set; }

        // BetDomain Maps (for different providers)
        public string BetDomainMap { get; protected set; }

        public int MaxSearchResult
        {
            get { return _maxSearchResult; }
            set { _maxSearchResult = value; }
        }

        public bool EnableOddsChangeIndication { get; set; }

        // Helper Methods


        public static string GetAppSettings(string sName)
        {
            try
            {
                return ConfigurationManager.AppSettings[sName];
            }
            catch
            {
            }

            return null;
        }

        public static string StringToString(string sValue, string sDefault)
        {
            return string.IsNullOrEmpty(sValue) ? sDefault : sValue;
        }

        public static int StringToInt(string sValue, int iDefault)
        {
            if (!string.IsNullOrEmpty(sValue))
            {
                try
                {
                    return Convert.ToInt32(sValue);
                }
                catch
                {
                }
            }

            return iDefault;
        }

        public static decimal StringToDecimal(string sValue, decimal dcDefault)
        {
            if (!string.IsNullOrEmpty(sValue))
            {
                try
                {
                    return Convert.ToInt32(sValue, EN_US);
                }
                catch
                {
                }
            }

            return dcDefault;
        }

        public static bool StringToBool(string sValue, bool bDefault)
        {
            if (!string.IsNullOrEmpty(sValue))
            {
                try
                {
                    return Convert.ToInt32(sValue, EN_US) == 1;
                }
                catch
                {
                }
            }

            return bDefault;
        }
    }

    public static class StationSettingsUtils
    {
        public static Assembly m_EntryAssembly = null;
        public static string m_sStartupPath = null;

        public static Assembly EntryAssembly
        {
            get
            {
                if (m_EntryAssembly == null)
                {
                    m_EntryAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                }

                return m_EntryAssembly;
            }
        }

        public static string StartupPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_sStartupPath))
                {
                    m_sStartupPath = Path.GetDirectoryName(StationSettingsUtils.EntryAssembly.Location);
                }

                return m_sStartupPath;
            }
        }
    }
}
