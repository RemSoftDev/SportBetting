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
    class TournamentFlagsClient
    {
        public const string FLAGS_THREAD_NAME = "FlagsClientThread";
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
            ThreadHelper.RunThread(FLAGS_THREAD_NAME, FlagsSync);
        }

        private static void FlagsSync(ThreadContext tc)
        {
            while (!tc.IsToStop)
            {
                try
                {
                    if (!StationRepository.IsPrematchEnabled)
                        continue;

                    var resourceUpdateId = UpdateFileEntrySr.GetLastUpdate(eDataSyncCacheType.Resources);
                    var id = resourceUpdateId == null ? 0 : resourceUpdateId.DataSyncCacheID;

                    UpdateRecord[] updateline = WsdlRepository.UpdateFlags(StationRepository.StationNumber, id);

                    if (updateline != null && updateline.Length > 0)
                    {
                        ProviderHelper.UpdateDatabase(DateTime.Now, DalStationSettings.Instance.UseDatabaseForOthers, DataArrayToList(updateline));
                        LineSr.ProcessDataSqlUpdateSucceeded(eUpdateType.PreMatches, "SportRadar Pre-Match Update.");

                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                var syncinterval = Convert.ToInt32(ConfigurationManager.AppSettings["STATIONPROPERTY_SYNC_INTERVAL"]);
                if (StationRepository.SyncInterval > 0)
                    syncinterval = StationRepository.SyncInterval;

                Thread.Sleep(syncinterval * 1000);

            }
        }

        public static List<UpdateRecordSr> DataArrayToList(IEnumerable<UpdateRecord> arrUpdates)
        {
            List<UpdateRecordSr> lResult = new List<UpdateRecordSr>();

            foreach (UpdateRecord record in arrUpdates)
            {
                lResult.Add(new UpdateRecordSr(record.dataSyncCacheId, record.fileName, (eDataSyncCacheType)record.dataSyncCacheType, record.data, record.description));
            }

            return lResult;
        }
    }
}
