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
    class MetainfoClient
    {
        public const string METAINFO_THREAD_NAME = "MetainfoClientThread";
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
            ThreadHelper.RunThread(METAINFO_THREAD_NAME, MetaSync,ThreadPriority.BelowNormal);
        }

        private static void MetaSync(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {

                if (StationRepository.IsPrematchEnabled)
                {
                    try
                    {
                        var stringsUpdateId = UpdateFileEntrySr.GetLastUpdate(eDataSyncCacheType.Metainfo);
                        var id = stringsUpdateId == null ? 0 : stringsUpdateId.DataSyncCacheID;

                        var updateline = WsdlRepository.GetMetainfo(StationRepository.StationNumber, id);
                        if (updateline != null && updateline.Length > 0)
                        {
                            ProviderHelper.UpdateDatabase(DateTime.Now, DalStationSettings.Instance.UseDatabaseForOthers, DataArrayToList(updateline));
                            LineSr.ProcessDataSqlUpdateSucceeded(eUpdateType.PreMatches, "SportRadar Metainfo Pre-Match Update.");
                        }

                        if (updateline != null && updateline.Length > 0)
                            continue;
                    }
                    catch (Exception)
                    {
                    }

                }

                var syncinterval = StationRepository.IsReady ? Convert.ToInt32(ConfigurationManager.AppSettings["METAINFO_SYNC_INTERVAL"]) : 1;

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
