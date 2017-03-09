using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using IocContainer;
using Ninject;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;

namespace SportRadar.DAL.SportRadarOldLineProvider
{
    class PreMatchClient
    {
        public const string PREMATCH_THREAD_NAME = "PreMatchClientThread";

        public static void Initialize(string stationNumber)
        {
            StationNumber = stationNumber;
        }

        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        private static IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }

        public static void Run()
        {
            ThreadHelper.RunThread(PREMATCH_THREAD_NAME, PreMatchSync, ThreadPriority.BelowNormal);
        }

        public static void Stop()
        {

        }

        private static void PreMatchSync(ThreadContext tc)
        {
            try
            {
                var arrUpdateRecords = WsdlRepository.GetLatestConfidenceFactorsUpdate(StationNumber);

                ProviderHelper.UpdateDatabase(DateTime.Now, DalStationSettings.Instance.UseDatabaseForOthers, DataArrayToList(arrUpdateRecords));

            }
            catch (Exception e)
            {
            }

            while (!tc.IsToStop)
            {
                int? iTotal = 0;

                try
                {
                    if (true)
                    {
                        /*
                        using (StreamReader streamReader = new StreamReader(@"C:\Library\Data.xml", Encoding.UTF8))
                        {
                            string sXml = streamReader.ReadToEnd();

                            SportRadarLineContainer srlc = SportRadarLineContainer.FromXmlString(sXml);

                            LineSr.SyncRoutines(eUpdateType.PreMatches, "Test", "None", null, delegate(object obj)
                            {
                                ProviderHelper.MergeFromSportRadarLineContainer(srlc);
                            });
                        }
                        */

                        while (!StationRepository.IsReady)
                        {
                            Thread.Sleep(1000);
                        }

                        valueForm vf = null;
                        BsmHubConfigurationResponse bhcr = null;

                        string sStationNumber = StationNumber;

                        var stringsUpdateId = UpdateFileEntrySr.GetLastUpdate(eDataSyncCacheType.String);
                        var id = stringsUpdateId == null ? 0 : stringsUpdateId.DataSyncCacheID;
                        UpdateRecord[] arrUpdateRecords = WsdlRepository.UpdateLocalization(sStationNumber, id);
                        ProviderHelper.UpdateDatabase(DateTime.Now, DalStationSettings.Instance.UseDatabaseForOthers, DataArrayToList(arrUpdateRecords));
                        if (arrUpdateRecords != null && arrUpdateRecords.Length > 0)
                        {
                            DataCopy.UpdateLanguages();
                        }
                        // Lock Offer
                        long[] arrLockedTournamentIds = null;
                        long[] arrLockedOddIds = WsdlRepository.GetLockedOffer(sStationNumber, out arrLockedTournamentIds);

                        // Sync Locked Odds
                        LineSr.Instance.LockedObjects.SyncLockedOdds(arrLockedOddIds);
                        int counter = 0;
                        do
                        {
                            var lastUpdateId = UpdateFileEntrySr.GetLastUpdate(eDataSyncCacheType.Match);
                            id = lastUpdateId == null ? 0 : lastUpdateId.DataSyncCacheID;
                            arrUpdateRecords = WsdlRepository.UpdateLine(sStationNumber, id, DateTime.MinValue, out iTotal);

                            if (iTotal > 0 && arrUpdateRecords != null)
                                iTotal = iTotal + arrUpdateRecords.Length;
                            DataCopy.UpdateProgressBar(iTotal);

                            ProviderHelper.UpdateDatabase(DateTime.Now, DalStationSettings.Instance.UseDatabaseForPreMatches, DataArrayToList(arrUpdateRecords));
                            LineSr.ProcessDataSqlUpdateSucceeded(eUpdateType.PreMatches, "SportRadar Pre-Match Update.");

                        } while (iTotal > 0 && counter++ < 100);

                        /*
                        // Sync Locked Groups
                        SyncList<long> lLockedGroupIds = new SyncList<long>();

                        // Tournament IDs to Group IDs
                        foreach (long lTournamentId in arrLockedTournamentIds)
                        {
                            GroupLn group = LineSr.Instance.AllObjects.Groups.SafelyGetGroupByKeyName(GroupLn.GROUP_TYPE_GROUP_T, lTournamentId);

                            if (group != null)
                            {
                                lLockedGroupIds.SafelyAdd(group.GroupId);
                            }
                        }

                        LineSr.Instance.LockedObjects.SyncLockedGroups(lLockedGroupIds);
                        */
                    }
                }
                catch (Exception excp)
                {
                }
                var syncinterval = Convert.ToInt32(ConfigurationManager.AppSettings["STATIONPROPERTY_SYNC_INTERVAL"]);
                if (StationRepository.SyncInterval > 0)
                    syncinterval = StationRepository.SyncInterval;
                if (iTotal > 0)
                    syncinterval = 0;
                Thread.Sleep(syncinterval * 1000);
            }

        }

        protected static string StationNumber { get; set; }

        public static List<UpdateRecordSr> DataArrayToList(IEnumerable<UpdateRecord> arrUpdates)
        {
            List<UpdateRecordSr> lResult = new List<UpdateRecordSr>();

            if (arrUpdates != null)
                foreach (UpdateRecord record in arrUpdates)
                {
                    lResult.Add(new UpdateRecordSr(record.dataSyncCacheId, record.fileName, (eDataSyncCacheType)record.dataSyncCacheType, record.data, record.description));
                }

            return lResult;
        }


    }
}