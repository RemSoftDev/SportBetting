using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;

namespace SportRadar.DAL.NewLineObjects
{
    public sealed class DatabaseCache
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(DatabaseCache));

        public DictionaryOfLineObjectCollection m_diAll = null;

        private static DatabaseCache m_Instance = null;
        private long m_lNextGroupId = 0;

        private DatabaseCache()
        {
            m_diAll = new DictionaryOfLineObjectCollection();
            m_diAll.Initialize();
        }

        public DictionaryOfLineObjectCollection AllObjects { get { return m_diAll; } }

        private void FillFromDataTable<T>(IDbConnection conn, string sQuery, ILineObjectCollection<T> locObjects) where T : ILineObject<T>
        {
            try
            {
                using (DataTable dt = DataCopy.GetDataTable(conn, null, sQuery))
                {
                    foreach (var dr in dt.AsEnumerable())
                    {
                        T obj = Activator.CreateInstance<T>();
                        obj.FillFromDataRow(dr);

                        locObjects.AddStrictly(obj);
                    }

                }
            }
            catch (Exception excp)
            {
                m_logger.Error(excp.Message, excp);
                throw;
            }
        }

        private void FillFromDatabase()
        {
            try
            {
                using (IDbConnection conn = ConnectionManager.GetConnection())
                {

                    //FillFromDataTable<RelatedStringLn>(conn, "SELECT * FROM RelatedString", m_diAll.RelatedStringsByKey);
                    FillFromDataTable<TaggedStringLn>(conn, "SELECT * FROM strings", m_diAll.TaggedStrings);
                    FillFromDataTable<TimeTypeLn>(conn, "SELECT * FROM time_type", m_diAll.TimeTypes);
                    FillFromDataTable<ScoreTypeLn>(conn, "SELECT * FROM score_type", m_diAll.ScoreTypes);
                    FillFromDataTable<BetTypeLn>(conn, "SELECT * FROM bet_type", m_diAll.BetTypes);
                    FillFromDataTable<BetDomainTypeLn>(conn, "SELECT * FROM betdomain_type", m_diAll.BetDomainTypes);
                    FillFromDataTable<GroupLn>(conn, "SELECT * FROM Groups", m_diAll.Groups);
                    FillFromDataTable<CompetitorLn>(conn, "SELECT * FROM Competitor", m_diAll.Competitors);
                    FillFromDataTable<MatchLn>(conn, "SELECT * FROM Matches", m_diAll.Matches);
                    FillFromDataTable<CompetitorToOutrightLn>(conn, "SELECT * FROM competitor_to_outright", m_diAll.CompetitorsToOutright);
                    FillFromDataTable<LiveMatchInfoLn>(conn, "SELECT * FROM LiveMatchInfo", m_diAll.LiveMatchInfos);
                    FillFromDataTable<MatchToGroupLn>(conn, "SELECT * FROM MatchToGroup", m_diAll.MatchesToGroups);
                    FillFromDataTable<BetDomainLn>(conn, "SELECT * FROM BetDomain", m_diAll.BetDomains);
                    FillFromDataTable<OddLn>(conn, "SELECT * FROM Odd", m_diAll.Odds);
                    FillFromDataTable<MatchResultLn>(conn, "SELECT * FROM MatchResult", m_diAll.MatchResults);
                    FillFromDataTable<ResourceRepositoryLn>(conn, "SELECT * FROM ResourseRepository", m_diAll.Resources);
                    FillFromDataTable<ResourceAssignmentLn>(conn, "SELECT * FROM ResourceAssignment", m_diAll.ResourceAssignments);
                    FillFromDataTable<CompetitorInfosLn>(conn, "SELECT * FROM competitorinfos", m_diAll.CompetitorInfos);
                    FillFromDataTable<MatchInfosLn>(conn, "SELECT * FROM matchinfos", m_diAll.MatchInfos);
                    FillFromDataTable<TournamentInfosLn>(conn, "SELECT * FROM tournamentinfos", m_diAll.TournamentInfos);
                    FillFromDataTable<LiabilityLn>(conn, "SELECT * FROM conffactor", m_diAll.Liabilities);
                    FillFromDataTable<TournamentMatchLocksLn>(conn, "SELECT * FROM tournament_match_lock", m_diAll.TournamentMatchLocks);
                    FillFromDataTable<LanguageLn>(conn, "SELECT * FROM languages", m_diAll.Languages);
                    FillFromDataTable<MultistringGroupLn>(conn, "SELECT * FROM MultistringGroup", m_diAll.MultistringGroups);
                }
                long.TryParse(DataCopy.ExecuteScalar("select MAX(GroupId) FROM Groups").ToString(), out m_lNextGroupId);
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "DatabaseCache.FillFromDatabase() ERROR");
                throw;
            }
        }

        public static DatabaseCache Instance
        {
            get { return m_Instance; }
        }

        private delegate void DelegateSetCacheItem<T>(T obj) where T : ILineObject<T>;

        private void AddNewObjectsAfterCommit<T>(DelegateSetCacheItem<T> func) where T : ILineObject<T>
        {
            LineObjectCollection<T> loc = LineSr.Instance.NewOrChangedObjects.GetLineObjectCollection<T>();

            if (loc != null)
            {
                foreach (T obj in loc.Values)
                {
                    func(obj);
                }
            }
        }

        public void AddNewObjectsAfterCommit()
        {
            AddNewObjectsAfterCommit<TaggedStringLn>(delegate(TaggedStringLn tsl) { m_diAll.TaggedStrings.AddSafely(tsl); });

            AddNewObjectsAfterCommit<TimeTypeLn>(delegate(TimeTypeLn ttl) { m_diAll.TimeTypes.AddSafely(ttl); });
            AddNewObjectsAfterCommit<ScoreTypeLn>(delegate(ScoreTypeLn stl) { m_diAll.ScoreTypes.AddSafely(stl); });
            AddNewObjectsAfterCommit<BetTypeLn>(delegate(BetTypeLn btl) { m_diAll.BetTypes.AddSafely(btl); });
            AddNewObjectsAfterCommit<BetDomainTypeLn>(delegate(BetDomainTypeLn bdtl) { m_diAll.BetDomainTypes.AddSafely(bdtl); });

            AddNewObjectsAfterCommit<GroupLn>(delegate(GroupLn grpl) { m_diAll.Groups.AddSafely(grpl); });
            AddNewObjectsAfterCommit<CompetitorLn>(delegate(CompetitorLn cmpl) { m_diAll.Competitors.AddSafely(cmpl); });
            AddNewObjectsAfterCommit<MatchLn>(delegate(MatchLn mtch) { m_diAll.Matches.AddSafely(mtch); });
            AddNewObjectsAfterCommit<CompetitorToOutrightLn>(delegate(CompetitorToOutrightLn cto) { m_diAll.CompetitorsToOutright.AddSafely(cto); });
            AddNewObjectsAfterCommit<MatchToGroupLn>(delegate(MatchToGroupLn mtog) { m_diAll.MatchesToGroups.AddSafely(mtog); });
            AddNewObjectsAfterCommit<LiveMatchInfoLn>(delegate(LiveMatchInfoLn lmil) { m_diAll.LiveMatchInfos.AddSafely(lmil); });
            AddNewObjectsAfterCommit<BetDomainLn>(delegate(BetDomainLn bdmn) { m_diAll.BetDomains.AddSafely(bdmn); });
            AddNewObjectsAfterCommit<OddLn>(delegate(OddLn bodd) { m_diAll.Odds.AddSafely(bodd); });

            AddNewObjectsAfterCommit<ResourceAssignmentLn>(delegate(ResourceAssignmentLn ra) { m_diAll.ResourceAssignments.AddSafely(ra); });
            AddNewObjectsAfterCommit<ResourceRepositoryLn>(delegate(ResourceRepositoryLn rr) { m_diAll.Resources.AddSafely(rr); });

            AddNewObjectsAfterCommit<MatchInfosLn>(delegate(MatchInfosLn mil) { m_diAll.MatchInfos.AddSafely(mil); });
            AddNewObjectsAfterCommit<CompetitorInfosLn>(delegate(CompetitorInfosLn cil) { m_diAll.CompetitorInfos.AddSafely(cil); });
            AddNewObjectsAfterCommit<TournamentInfosLn>(delegate(TournamentInfosLn til) { m_diAll.TournamentInfos.AddSafely(til); });

            AddNewObjectsAfterCommit<MatchResultLn>(delegate(MatchResultLn rslt) { m_diAll.MatchResults.AddSafely(rslt); });
            AddNewObjectsAfterCommit<LiabilityLn>(delegate(LiabilityLn li) { m_diAll.Liabilities.AddSafely(li); });
            AddNewObjectsAfterCommit<LanguageLn>(delegate(LanguageLn lang) { m_diAll.Languages.AddSafely(lang); });
            AddNewObjectsAfterCommit<MultistringGroupLn>(delegate(MultistringGroupLn msGroup) { m_diAll.MultistringGroups.AddSafely(msGroup); });
            AddNewObjectsAfterCommit<TournamentMatchLocksLn>(delegate(TournamentMatchLocksLn mtl) { m_diAll.TournamentMatchLocks.AddSafely(mtl); });
        }

        private void RemoveObjectsAfterCommit<T>(DelegateSetCacheItem<T> func) where T : ILineObject<T>
        {
            LineObjectCollection<T> loc = LineSr.Instance.ObjectsToRemove.GetLineObjectCollection<T>();

            if (loc != null)
            {
                foreach (T obj in loc.Values)
                {
                    func(obj);
                }
            }
        }

        public void RemoveObjectsAfterCommit()
        {
            RemoveObjectsAfterCommit<MatchToGroupLn>(delegate(MatchToGroupLn mtog) { m_diAll.MatchesToGroups.Remove(mtog.KeyName); });
            RemoveObjectsAfterCommit<MatchLn>(delegate(MatchLn mtch) { m_diAll.Matches.Remove(mtch.MatchId); });
            RemoveObjectsAfterCommit<LiveMatchInfoLn>(delegate(LiveMatchInfoLn lmil) { m_diAll.LiveMatchInfos.Remove(lmil.MatchId); });
            RemoveObjectsAfterCommit<CompetitorToOutrightLn>(delegate(CompetitorToOutrightLn cto) { m_diAll.CompetitorsToOutright.Remove(cto.match2competitorid); });
            RemoveObjectsAfterCommit<BetDomainLn>(delegate(BetDomainLn bdmn) { m_diAll.BetDomains.Remove(bdmn.BetDomainId); });
            RemoveObjectsAfterCommit<OddLn>(delegate(OddLn bodd) { m_diAll.Odds.Remove(bodd.OutcomeId); });
            RemoveObjectsAfterCommit<MatchResultLn>(delegate(MatchResultLn rslt) { m_diAll.MatchResults.Remove(rslt.MatchId); });
        }

        public static void Clear()
        {
            m_Instance = null;
        }

        public static void EnsureDatabaseCache()
        {
            if (m_Instance == null)
            {
                m_Instance = new DatabaseCache();

                if (DalStationSettings.Instance.CreateDatabase)
                {
                    m_Instance.FillFromDatabase();
                }
            }
        }

        // Groups
        public bool DoesGroupExist(string sGroupType, long lServerId)
        {
            return m_diAll.Groups.SafelyGetGroupByKeyName(sGroupType, lServerId) != null;
        }

        public long GetGroupId(string sGroupType, long lServerId)
        {
            var group = LineSr.Instance.AllObjects.Groups.SafelyGetGroupByKeyName(sGroupType, lServerId);

            if (group != null)
            {
                return group.GroupId;
            }

            group = m_diAll.Groups.SafelyGetGroupByKeyName(sGroupType, lServerId);

            return group != null ? group.GroupId : ++m_lNextGroupId;
        }
    }
}
