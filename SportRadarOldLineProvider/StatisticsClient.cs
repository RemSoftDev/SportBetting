using System;
using System.Collections.Generic;
using System.Configuration;
using SportRadar.DAL.CommonObjects;
using System.Threading;
using IocContainer;
using Ninject;
using SportRadar.Common.Windows;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;

namespace SportRadar.DAL.SportRadarOldLineProvider
{
    class StatisticsClient
    {
        public const string STATISTICS_THREAD_NAME = "StatisticsClientThread";
        protected static string StationNumber { get; set; }

        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        private static IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }

        public static void Initialize(string stationNumber)
        {
            StationNumber = stationNumber;
        }

        public static void Run()
        {
            ThreadHelper.RunThread(STATISTICS_THREAD_NAME, StatisticsSync, ThreadPriority.BelowNormal);
        }

        private static void StatisticsSync(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {
                if (StationRepository.IsStatisticsEnabled)
                {

                    //get statistics
                    try
                    {
                        var stringsUpdateId = UpdateFileEntrySr.GetLastUpdate(eDataSyncCacheType.Statistic);
                        var id = stringsUpdateId == null ? 0 : stringsUpdateId.DataSyncCacheID;

                        var updateline = WsdlRepository.UpdateStatistics(StationRepository.StationNumber, id);
                        if (updateline != null && updateline.Length > 0)
                            ProviderHelper.UpdateDatabase(DateTime.Now, DalStationSettings.Instance.UseDatabaseForOthers, DataArrayToList(updateline));

                        if (updateline != null && updateline.Length > 0)
                            continue;
                    }
                    catch (Exception)
                    {
                    }
                }


                //get flags
                try
                {
                    var resourceUpdateId = UpdateFileEntrySr.GetLastUpdate(eDataSyncCacheType.Resources);
                    var id = resourceUpdateId == null ? 0 : resourceUpdateId.DataSyncCacheID;
                    
                    var updateline = WsdlRepository.UpdateFlags(StationRepository.StationNumber, id);

                    if (updateline != null && updateline.Length > 0)
                    {
                        ProviderHelper.UpdateDatabase(DateTime.Now, DalStationSettings.Instance.UseDatabaseForOthers, DataArrayToList(updateline));
                        LineSr.ProcessDataSqlUpdateSucceeded(eUpdateType.PreMatches, "SportRadar Pre-Match Update.");

                    }
                }
                catch (Exception ex)
                {
                }
                
                var syncinterval = Convert.ToInt32(ConfigurationManager.AppSettings["STATIONPROPERTY_SYNC_INTERVAL"]);
                if (StationRepository.SyncInterval > 0)
                    syncinterval = StationRepository.SyncInterval;

                Thread.Sleep(syncinterval * 1000);
            }
        }

        public static List<UpdateRecordSr> DataArrayToList(UpdateRecord[] arrUpdates)
        {
            List<UpdateRecordSr> lResult = new List<UpdateRecordSr>();

            foreach (UpdateRecord record in arrUpdates)
            {
                lResult.Add(new UpdateRecordSr(record.dataSyncCacheId, record.fileName, (eDataSyncCacheType)record.dataSyncCacheType, record.data, record.description));
            }

            foreach (UpdateRecordSr rec in lResult)
            {
                string res = rec.GetXmlData();

                if (rec.Description.Contains("trnm"))
                {
                }
            }

            return lResult;
        }
    }
}
