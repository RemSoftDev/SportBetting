using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nbt.Common.Utils.Windows;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.SportRadarOldLineProvider
{
    public class OldLineProvider : ILineProvider
    {
        public const string OLD_LINE_PROVIDER_NAME = "OldLineProvider";
        private static ILog Log = LogFactory.CreateLog(typeof(OldLineProvider));

        public string UniqueName
        {
            get { return OLD_LINE_PROVIDER_NAME; }
        }

        private static void ImportOutrightExample()
        {
            try
            {
                string sXml = TraceHelper.ReadTextFromFile("OutrightExample.xml");

                SportRadarLineContainer srlc = SportRadarLineContainer.FromXmlString(sXml);

                eFileSyncResult fsr = LineSr.SyncRoutines(eUpdateType.PreMatches, string.Format("SportRadar Pre-Match Update. DataSyncCacheId = {0}", 1), true, null, delegate(object objParam)
                {
                    return ProviderHelper.MergeFromSportRadarLineContainer(srlc, 1);
                });
            }
            catch (Exception excp)
            {
            }
        }

        public void Initialize(object objParam)
        {
            Log.Debug("init lineprovider");

            eFileSyncResult fsr = LineSr.SyncRoutines(eUpdateType.Initialize, "Adding bet types from configuration", DalStationSettings.Instance.UseDatabaseForLine, null, delegate(object obj)
            {
                BetDomainMap.EnsureInstance();
                return false;
            });


            ExcpHelper.ThrowIf(fsr == eFileSyncResult.Failed, "Cannot initialize Bet Types");

            string sStationNumber = objParam as string;
            Debug.Assert(!string.IsNullOrEmpty(sStationNumber));

            LiveBetClient.Initialize(sStationNumber);
            VirtualLiveBetClient.Initialize(sStationNumber);
            PreMatchClient.Initialize(sStationNumber);
            StatisticsClient.Initialize(sStationNumber);
            MetainfoClient.Initialize(sStationNumber);
            //TournamentFlagsClient.Initialize(objParam.ToString());

#if ADD_OUTRIGHT_XML
            ImportOutrightExample();
            ImportOutrightExample();
#endif
        }

        public void Run(eLineType elt)
        {
            Log.Debug("run line clients");

            if (((int)elt & (int)eLineType.PreMatches) > 0)
            {
                PreMatchClient.Run();
                StatisticsClient.Run();
                MetainfoClient.Run();
                //TournamentFlagsClient.Run();
            }

            if (((int)elt & (int)eLineType.LiveMatches) > 0)
            {
                LiveBetClient.Run();
                VirtualLiveBetClient.Run();
            }

            LineCleaner.Run();
        }

        public void Stop(eLineType elt)
        {
            if (((int)elt & (int)eLineType.PreMatches) > 0)
            {
                PreMatchClient.Stop();
            }

            if (((int)elt & (int)eLineType.LiveMatches) > 0)
            {
                LiveBetClient.Stop();
                VirtualLiveBetClient.Stop();
            }
        }

        public void Clear(eLineType elt)
        {
            throw new NotImplementedException();
        }
    }
}
