using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Nbt.Common.Utils.Windows;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public delegate void DelegateDataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription);

    public delegate bool DelegateMerge(object objParam);

    public sealed class LineSr : LineBase
    {
        private const int MAX_MATCH_CODE = 99999;

        private static ILog m_logger = LogFactory.CreateLog(typeof(LineSr));

        private static LineSr m_Instance = new LineSr();

        private DictionaryOfLineObjectCollection m_diAll = new DictionaryOfLineObjectCollection();
        private DictionaryOfLineObjectCollectionLight m_diChangedObjects = new DictionaryOfLineObjectCollectionLight();
        private DictionaryOfLineObjectCollectionLight m_diObjectsToRemove = new DictionaryOfLineObjectCollectionLight();

        private SyncDictionary<string, LanguageLn> m_diLanguages = new SyncDictionary<string, LanguageLn>();

        private SyncList<IOddLn> m_lSelectedOdds = new SyncList<IOddLn>();
        private static object m_oUpdateLocker = new object();
        private static object m_oReadLocker = new object();

        private LockedObjects m_lo = new LockedObjects();

        public static bool LiveBetConnected { get; set; }
        public static event DelegateDataSqlUpdateSucceeded DataSqlUpdateSucceeded = null;

        //confidence factor constants
        public const string CONF_RATING_VALUES = "CONF_RATING_VALUES";
        public const string TOURN_CONF_RATING = "TOURN_CONF_RATING";
        public const string MATCH_FACTOR = "MATCH_FACTOR";
        public const string LIMIT_FACTORS = "LIMIT_FACTORS";

        System.Timers.Timer timer = new System.Timers.Timer(60000);
        private LineSr()
        {
            m_diAll.Initialize();
            timer.Elapsed += LogChanges;
            timer.Start();
        }

        public static LineSr Instance
        {
            get
            {
                return m_Instance;
            }
        }

        public TournamentMatchLocksDictionary TournamentMatchLocks()
        {
            return m_diAll.TournamentMatchLocks;
        }

        public bool IsTournamentVisible(string svrId)
        {
            ActiveTournamentLn aT = m_diAll.ActiveTournaments.SafelyGetValue(svrId);

            if (aT != null)
                return true;

            return false;
        }

        public static void ProcessDataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription)
        {
            if (LineSr.DataSqlUpdateSucceeded != null)
            {
                try
                {
                    LineSr.DataSqlUpdateSucceeded(eut, sProviderDescription);
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "Event LineSr.ProcessDataSqlUpdateSucceeded(eUpdateType={0}, sProviderDescription='{1}')", eut, sProviderDescription);
                }
            }
        }

        public void EnsureLanguage(long lLanguageId, string sShortName, bool bIsTerminal)
        {
            string sLowerShortName = sShortName.ToLowerInvariant();

            if (!m_diLanguages.ContainsKey(sLowerShortName)) // We don't want to create an object
            {
                LanguageLn ll = new LanguageLn(lLanguageId, sLowerShortName, bIsTerminal);

                m_diLanguages.SafelyAdd(sLowerShortName, ll); // DK - It is  very rare case where some other thread MAY create similar language object
            }
        }

        public SyncList<LanguageLn> GetTerminalLanguages()
        {
            SyncList<LanguageLn> lTerminalLanguages = new SyncList<LanguageLn>();


            //m_diLanguages.SafelyForEach(delegate(LanguageLn lng)
            LineSr.Instance.AllObjects.Languages.SafelyForEach(delegate(LanguageLn lng)
            {
                //if (lng.IsTerminal.Value)
                //{
                //    lTerminalLanguages.Add(lng);
                //}
                lTerminalLanguages.Add(lng);

                return false;
            });

            return lTerminalLanguages;
        }

        public LockedObjects LockedObjects
        {
            get { return m_lo; }
        }

        public DictionaryOfLineObjectCollection AllObjects
        {
            get { return m_diAll; }
        }

        public DictionaryOfLineObjectCollectionLight NewOrChangedObjects
        {
            get { return m_diChangedObjects; }
        }

        public DictionaryOfLineObjectCollectionLight ObjectsToRemove
        {
            get { return m_diObjectsToRemove; }
        }

        public static bool AllowMultiway { get; set; }

        public string GetStringSafely(string sTag, string sLanguage = null)
        {
            return m_diAll.TaggedStrings.GetStringSafely(sTag, sLanguage);
        }

        public override string GetString(long? lMultiStringId, string sDefaultValue)
        {
            Debug.Assert(false);

            return null;
        }

        public delegate bool DelegateFilterMatches(MatchLn match);
        public delegate bool DelegateFilterResults(MatchResultLn result);

        public void SearchResults(SortableObservableCollection<MatchResultVw> ocMatchResults, DelegateFilterResults dfr, Comparison<MatchResultVw> comparison)
        {
            SyncList<MatchResultVw> lResultsToSync = ocMatchResults.ToSyncList();
            SearchResults(lResultsToSync, dfr);

            if (comparison != null)
            {
                lResultsToSync.Sort(comparison);
                ocMatchResults.ApplyChanges(lResultsToSync);
            }
        }

        public void SearchResults(SyncList<MatchResultVw> lResultsToSync, DelegateFilterResults dfr)
        {
            CheckTime ct = new CheckTime("SearchResults() entered");

            lock (m_oReadLocker)
            {
                ct.AddEvent("Inside of lock");

                if (lResultsToSync != null)
                {
                    HashSet<MatchResultVw> hsMatches = new HashSet<MatchResultVw>();

                    SyncList<MatchResultLn> lAllResults = m_diAll.MatchResults.ToSyncList();

                    foreach (MatchResultLn result in lAllResults)
                    {
                        if (dfr == null || dfr(result))
                        {
                            hsMatches.Add(result.MatchResultView);

                            if (!lResultsToSync.Contains(result.MatchResultView))
                            {
                                lResultsToSync.Add(result.MatchResultView);
                            }
                        }
                    }

                    for (int i = 0; i < lResultsToSync.Count; )
                    {
                        MatchResultVw matchResultView = lResultsToSync[i];

                        if (!hsMatches.Contains(matchResultView))
                        {
                            lResultsToSync.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }

            ct.AddEvent("Completed");
            ct.Info(m_logger);
        }

        public void SearchMatches(SortableObservableCollection<IMatchVw> ocMatches, string sSearch, string sLanguage, DelegateFilterMatches dfm, Comparison<IMatchVw> comparison)
        {
            SyncList<IMatchVw> lMatchesToSync = ocMatches.ToSyncList();
            SearchMatches(lMatchesToSync, sSearch, sLanguage, dfm);

            if (comparison != null)
            {
                lMatchesToSync.Sort(comparison);
            }
            ocMatches.ApplyChanges(lMatchesToSync);
        }

        private static void SearchMatchImp(HashSet<IMatchVw> hsMatches, SyncList<IMatchVw> lMatchesToSync, MatchLn mtch, DelegateFilterMatches dfm)
        {
            if (dfm == null || dfm(mtch))
            {
                hsMatches.Add(mtch.MatchView);

                if (!lMatchesToSync.Contains(mtch.MatchView))
                {
                    lMatchesToSync.Add(mtch.MatchView);
                }
            }
        }

        public SyncList<MatchLn> QuickSearchMatches(DelegateFilterMatches dfm)
        {
            Debug.Assert(dfm != null);

            SyncList<MatchLn> lResult = new SyncList<MatchLn>();
            SyncList<MatchLn> lAllMatches = m_diAll.Matches.ToSyncList();

            foreach (var match in lAllMatches)
            {
                if (dfm(match))
                {
                    lResult.Add(match);
                }
            }

            return lResult;
        }

#if DEBUG || TRACE_ID_FROM_FILE
        private static void MatchIdFromFileFound(long lFoundLong, params object[] args)
        {
            MatchLn mtch = args[0] as MatchLn;
            m_logger.InfoFormat("SearchMatches() found pointed {0}", mtch);
        }
#endif

        public void SearchMatches(SyncList<IMatchVw> lMatchesToSync, string sSearch, string sLanguage, DelegateFilterMatches dfm)
        {
            CheckTime ct = new CheckTime(false, "SearchMatches('{0}', '{1}') entered", sSearch, sLanguage);

            lock (m_oReadLocker)
            {
                if (lMatchesToSync != null)
                {
                    HashSet<IMatchVw> hsMatches = new HashSet<IMatchVw>();
                    SyncList<MatchLn> lAllMatches = m_diAll.Matches.ToSyncList();

                    if (!string.IsNullOrEmpty(sSearch))
                    {
                        ct.AddEvent("Search by string in {0} matches", lAllMatches.Count);

                        IdentityList ilGroups = new IdentityList();
                        IdentityList ilCompetitors = new IdentityList();

                        m_diAll.TaggedStrings.SearchRelatedStrings(sSearch, sLanguage, ilGroups, ilCompetitors);

                        int iCode = 0;
                        bool bIsNumber = int.TryParse(sSearch, out iCode) && 0 < iCode && iCode < MAX_MATCH_CODE;

                        if (ilCompetitors.Count > 0 || bIsNumber)
                        {
                            foreach (var mtch in lAllMatches)
                            {
#if DEBUG || TRACE_ID_FROM_FILE
                                if (TraceHelper.TraceFromFileLongValues("MatchIds.txt", (long)mtch.MatchId, new TraceHelper.DelegateLongFromRegistryFound(MatchIdFromFileFound), mtch))
                                {
                                    // Put breakpoint here to catch certain match by mtch.MatchId
                                }
#endif
                                string sCode = mtch.Code.Value.ToString("G");

                                if (sCode.IndexOf(sSearch, StringComparison.OrdinalIgnoreCase) >= 0 || ilCompetitors.Contains(mtch.HomeCompetitorId.Value) || ilCompetitors.Contains(mtch.AwayCompetitorId.Value))
                                {
                                    SearchMatchImp(hsMatches, lMatchesToSync, mtch, dfm);
                                }
                                //else
                                //{
                                //    SyncList<GroupLn> lGroups = LineSr.Instance.AllObjects.MatchesToGroups.GetMatchGroups(mtch.MatchId);

                                //    foreach (GroupLn group in lGroups)
                                //    {
                                //        if (ilGroups.Contains(group.GroupId))
                                //        {
                                //            SearchMatchImp(hsMatches, lMatchesToSync, mtch, dfm);
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                    else
                    {
                        ct.AddEvent("Search directly in {0} matches", lAllMatches.Count);

                        foreach (var mtch in lAllMatches)
                        {
#if DEBUG || TRACE_ID_FROM_FILE
                            if (TraceHelper.TraceFromFileLongValues("MatchIds.txt", (long)mtch.MatchId, new TraceHelper.DelegateLongFromRegistryFound(MatchIdFromFileFound), mtch))
                            {
                                // Put breakpoint here to catch certain match by mtch.MatchId
                            }
#endif
                            SearchMatchImp(hsMatches, lMatchesToSync, mtch, dfm);
                        }
                    }

                    ct.AddEvent("Search Completed");

                    for (int i = 0; i < lMatchesToSync.Count; )
                    {
                        IMatchVw matchView = lMatchesToSync[i];

                        if (!hsMatches.Contains(matchView))
                        {
                            lMatchesToSync.RemoveAt(i);
                        }
                        else
                        {
                            i++;
                        }
                    }

                    ct.AddEvent("Remove Completed");
                }
            }

            ct.AddEvent("Completed (Found {0} match(es))", lMatchesToSync.Count);
            ct.Info(m_logger);
        }

        private bool CheckOdd(IOddLn odd)
        {
            var oddToCheck = m_diAll.Odds.GetObject(odd.OutcomeId);

            if (oddToCheck != null)
            {
                if (oddToCheck.BetDomain.Match.Active.Value)
                {
                    eBetDomainStatus bdmnStatus = oddToCheck.BetDomain.Status.Value;

                    return bdmnStatus == eBetDomainStatus.Visible || bdmnStatus == eBetDomainStatus.Inactive;
                }
            }

            return false;
        }
        object _verifyLocker = new object();
        public void VerifySelectedOdds(SortableObservableCollection<ITipItemVw> socSelectedOdds, SyncHashSet<ITipItemVw> hsOddsToRemove = null)
        {
            CheckTime ct = new CheckTime(true, "VerifySelectedOdds(TipCount={0})", socSelectedOdds.Count);

            ExcpHelper.ThrowIf<ArgumentNullException>(socSelectedOdds == null, "VerifySelectedOdds(NULL) ERROR");

            lock (_verifyLocker)
            {
                ct.AddEvent("Lock Entered");

                if (hsOddsToRemove != null)
                {
                    hsOddsToRemove.Clear();
                }
                else
                {
                    hsOddsToRemove = new SyncHashSet<ITipItemVw>();
                }

                SyncList<ITipItemVw> lTipItems = socSelectedOdds.ToSyncList();

                foreach (TipItemVw tiv in lTipItems)
                {
                    // Check if selected odd is not expired
                    if (!CheckOdd(tiv.Odd))
                    {
                        hsOddsToRemove.Add(tiv);
                    }
                    // Check if selected odd is not yet in current collection (m_lSelectedOdds)
                    else if (!m_lSelectedOdds.Contains(tiv.Odd))
                    {
                        m_lSelectedOdds.Add(tiv.Odd);
                        tiv.Odd.BetDomain.Match.SetSelected(tiv.Odd, true);
                    }
                }

                ct.AddEvent("Check Completed");

                // Remove from socSelectedOdds and m_lSelectedOdds
                for (int i = 0; i < lTipItems.Count; )
                {
                    var tiv = lTipItems[i];

                    if (hsOddsToRemove.Contains(tiv))
                    {
                        // This Odd is expired
                        lTipItems.Remove(tiv);
                        socSelectedOdds.Remove(tiv);
                        m_lSelectedOdds.Remove(tiv.Odd);
                        tiv.Odd.BetDomain.Match.SetSelected(tiv.Odd, false);
                    }
                    else
                    {
                        i++;
                    }
                }

                ct.AddEvent("Remove from List Completed");

                // Remove from m_lSelectedOdd those items were not removed in previous cycle
                for (int i = 0; i < m_lSelectedOdds.Count; )
                {
                    IOddLn odd = m_lSelectedOdds[i];

                    TipItemVw tiv = new TipItemVw(odd);

                    if (!lTipItems.Contains(tiv))
                    {
                        m_lSelectedOdds.Remove(odd);
                        tiv.Odd.BetDomain.Match.SetSelected(tiv.Odd, false);
                    }
                    else
                    {
                        i++;
                    }
                }

                ct.AddEvent("Remove from List2 Completed");
            }

            ct.Info(m_logger);
        }

        private static void MergeCollections<T>(DictionaryOfLineObjectCollection dlocSource, DictionaryOfLineObjectCollection dlocTarget) where T : ILineObject<T>
        {
            ILineObjectCollection<T> loc = dlocSource.GetLineObjectCollection<T>();

            MergeCollections<T>(loc.ToSyncList(), dlocTarget);
        }

        private static void MergeCollections<T>(SyncList<T> lSource, DictionaryOfLineObjectCollection dlocTarget) where T : ILineObject<T>
        {
            Debug.Assert(lSource != null);
            ILineObjectCollection<T> locTarget = dlocTarget.GetLineObjectCollection<T>();

            foreach (T objSource in lSource)
            {
                ILineObjectWithId<T> objById = objSource as ILineObjectWithId<T>;
                ILineObjectWithKey<T> objByKey = objSource as ILineObjectWithKey<T>;

                try
                {
                    if (objById != null)
                    {
                        if (objById.Id != 0)
                        {
                            locTarget.MergeLineObject(objSource);
                        }
                    }
                    else if (objByKey != null)
                    {
                        locTarget.MergeLineObject(objSource);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "Could not merge object {0}", objSource);
                }
            }
        }

        public static void Clear()
        {
            m_Instance = new LineSr();
        }

        public static void EnsureFromCache()
        {
            DictionaryOfLineObjectCollection dlocSource = DatabaseCache.Instance.AllObjects;
            DictionaryOfLineObjectCollection dlocTarget = LineSr.Instance.AllObjects;

            MergeCollections<TimeTypeLn>(dlocSource, dlocTarget);
            MergeCollections<ScoreTypeLn>(dlocSource, dlocTarget);
            MergeCollections<BetTypeLn>(dlocSource, dlocTarget);
            MergeCollections<BetDomainTypeLn>(dlocSource, dlocTarget);

            MergeCollections<TaggedStringLn>(dlocSource, dlocTarget);

            ILineObjectCollection<GroupLn> locGroups = dlocSource.GetLineObjectCollection<GroupLn>();
            SyncList<GroupLn> lGroups = locGroups.ToSyncList();

            lGroups.Sort(delegate(GroupLn g1, GroupLn g2)
            {
                long lParentGroupId1 = g1.ParentGroupId.Value ?? 0;
                long lParentGroupId2 = g2.ParentGroupId.Value ?? 0;

                if (lParentGroupId1 == lParentGroupId2)
                {
                    return g1.GroupId.CompareTo(g2.GroupId);
                }

                return lParentGroupId1.CompareTo(lParentGroupId2);
            });

            MergeCollections<GroupLn>(lGroups, dlocTarget);
            MergeCollections<CompetitorLn>(dlocSource, dlocTarget);
            MergeCollections<CompetitorToOutrightLn>(dlocSource, dlocTarget);
            MergeCollections<MatchLn>(dlocSource, dlocTarget);
            MergeCollections<LiveMatchInfoLn>(dlocSource, dlocTarget);
            MergeCollections<MatchToGroupLn>(dlocSource, dlocTarget);
            MergeCollections<BetDomainLn>(dlocSource, dlocTarget);
            MergeCollections<OddLn>(dlocSource, dlocTarget);
            MergeCollections<MatchResultLn>(dlocSource, dlocTarget);
            MergeCollections<ResourceRepositoryLn>(dlocSource, dlocTarget);
            MergeCollections<ResourceAssignmentLn>(dlocSource, dlocTarget);
            MergeCollections<CompetitorInfosLn>(dlocSource, dlocTarget);
            MergeCollections<MatchInfosLn>(dlocSource, dlocTarget);
            MergeCollections<TournamentInfosLn>(dlocSource, dlocTarget);
            MergeCollections<LiabilityLn>(dlocSource, dlocTarget);
            MergeCollections<LanguageLn>(dlocSource, dlocTarget);
            MergeCollections<TournamentMatchLocksLn>(dlocSource, dlocTarget);

            LineSr.Instance.NewOrChangedObjects.UnsetPropertiesChanged();
            LineSr.Instance.NewOrChangedObjects.Clear();
            LineSr.Instance.ObjectsToRemove.Clear();
        }

        public void RemoveBetDomain(long lBetDomainId)
        {
            var bdmnToRemove = m_diAll.BetDomains.GetObject(lBetDomainId);

            if (bdmnToRemove != null)
            {
                this.RemoveBetDomain(bdmnToRemove);

            }
        }

        public void RemoveBetDomain(BetDomainLn betDomain)
        {
            Debug.Assert(betDomain.Match != null);

            betDomain.Match.BetDomains.Remove(betDomain);

            var lOdds = betDomain.Odds.Clone();

            foreach (var bodd in lOdds)
            {
                // Remove Odds
                m_diAll.Odds.Remove(bodd.OutcomeId);
                m_diObjectsToRemove.SafelyAddObject(bodd);

                //m_logger.DebugFormat("Removed from Line {0}", bodd);
            }

            // Remove BetDomain
            m_diAll.BetDomains.Remove(betDomain.BetDomainId);
            m_diObjectsToRemove.SafelyAddObject(betDomain);

            //m_logger.DebugFormat("Removed from Line {0}", betDomain);
        }


        public void RemoveMatch(MatchLn match)
        {

            var lMatchBetDomains = match.BetDomains.Clone();

            foreach (var betDomain in lMatchBetDomains)
            {
                RemoveBetDomain(betDomain);
            }

            if (match.LiveMatchInfo != null)
            {
                m_diAll.LiveMatchInfos.Remove(match.MatchId);
                m_diObjectsToRemove.SafelyAddObject(match.LiveMatchInfo);
            }

            SyncList<MatchToGroupLn> lMatchesToGroups = m_diAll.MatchesToGroups.RemoveByMatch(match);

            foreach (MatchToGroupLn mtog in lMatchesToGroups)
            {
                m_diObjectsToRemove.SafelyAddObject(mtog);
            }

            if (match.outright_type != eOutrightType.None)
            {
                m_diAll.CompetitorsToOutright.RemoveByOutrightMatchId(match.MatchId);
            }

           

            // Remove Match
            m_diAll.Matches.Remove(match.MatchId);
            m_diObjectsToRemove.SafelyAddObject(match);


            //m_logger.DebugFormat("Removed from Line {0}", match);
        }
        public void ClearOldData()
        {
            foreach (var taggedString in m_diAll.TaggedStrings.ToSyncList())
            {
                if (taggedString.RelationType == eObjectType.OddTranslation)
                {
                    if (!m_diAll.Odds.ContainsKey((long)taggedString.ObjectId))
                    {
                        m_diAll.TaggedStrings.Remove(taggedString.Id);
                        m_diObjectsToRemove.SafelyAddObject(taggedString);

                    }
                }
                if (taggedString.RelationType == eObjectType.Competitor)
                {
                    if (!m_diAll.Competitors.ContainsKey((long)taggedString.ObjectId))
                    {
                        m_diAll.TaggedStrings.Remove(taggedString.Id);
                        m_diObjectsToRemove.SafelyAddObject(taggedString);

                    }
                }
            }
            foreach (var matchTogroup in m_diAll.MatchesToGroups.ToSyncDictionary())
            {
                if (!m_diAll.Matches.ContainsKey((long)matchTogroup.Value.MatchId))
                {
                    m_diAll.MatchesToGroups.Remove(matchTogroup);
                    m_diObjectsToRemove.SafelyAddObject(matchTogroup.Value);

                }
                if (!m_diAll.Groups.ContainsKey((long)matchTogroup.Value.GroupId))
                {
                    m_diAll.MatchesToGroups.Remove(matchTogroup);
                    m_diObjectsToRemove.SafelyAddObject(matchTogroup.Value);

                }
            }
        }

        public void RemoveResult(MatchResultLn match)
        {
            m_diAll.Matches.Remove(match.MatchId);
            m_diObjectsToRemove.SafelyAddObject(match);

        }


        public void RemoveMatch(long lMatchId)
        {
            var matchToRemove = m_diAll.Matches.GetObject(lMatchId);

            if (matchToRemove != null)
            {
                this.RemoveMatch(matchToRemove);
            }
        }

        public bool RemoveMatches(DelegateForEach<MatchLn> dfe)
        {
            Debug.Assert(dfe != null);

            SyncList<MatchLn> lAllMatches = m_diAll.Matches.ToSyncList();

            bool bSomeMatcheRemoved = false;

            foreach (var match in lAllMatches)
            {
                if (dfe(match))
                {
                    this.RemoveMatchWithoutCleaning(match);
                    bSomeMatcheRemoved = true;
                }
            }

            return bSomeMatcheRemoved;
        }

        private void RemoveMatchWithoutCleaning(MatchLn match)
        {
            var lMatchBetDomains = match.BetDomains.Clone();

            foreach (var betDomain in lMatchBetDomains)
            {
                RemoveBetDomain(betDomain);
            }

            if (match.LiveMatchInfo != null)
            {
                m_diAll.LiveMatchInfos.Remove(match.MatchId);
                m_diObjectsToRemove.SafelyAddObject(match.LiveMatchInfo);
            }

            SyncList<MatchToGroupLn> lMatchesToGroups = m_diAll.MatchesToGroups.RemoveByMatch(match);

            foreach (MatchToGroupLn mtog in lMatchesToGroups)
            {
                m_diObjectsToRemove.SafelyAddObject(mtog);
            }

            if (match.outright_type != eOutrightType.None)
            {
                m_diAll.CompetitorsToOutright.RemoveByOutrightMatchId(match.MatchId);
            }



            // Remove Match
            m_diAll.Matches.Remove(match.MatchId);
            m_diObjectsToRemove.SafelyAddObject(match);
        }

        public void DisableLiveMatches(eServerSourceType lineType)
        {
            SyncList<MatchLn> lAllMatches = m_diAll.Matches.ToSyncList();

            foreach (var match in lAllMatches)
            {
                if (match.IsLiveBet.Value && match.SourceType == lineType)
                {
                    Debug.Assert(match.LiveMatchInfo != null);

                    if (match.LiveMatchInfo.Status.Value == eMatchStatus.Started)
                    {
                        match.LiveMatchInfo.Status.Value = eMatchStatus.Stopped;
                        match.SetActiveChanged();
                    }
                }
            }
        }

        public static eFileSyncResult SyncRoutines(eUpdateType eut, string sProviderDescription, bool bUseDatabase, UpdateStatistics us, DelegateMerge dm)
        {
            Debug.Assert(dm != null);

            CheckTime ct = new CheckTime(true, "SyncRoutines(UseDatabase={0}) entered", bUseDatabase);

            /*
            if (DalStationSettings.Instance.EnableRunProcessControl)
            {
                string sInfo = SystemControl.GetSystemInfo() + "\r\n" + ProcessControl.Current.ToDetailedString();
                m_logger.Info(sInfo);

#if DEBUG
                m_logger.Info(ProcessControl.Current.GetThreadSummary());
#endif

                ct.AddEvent("Control completed");
            }
            */

            try
            {
                eFileSyncResult fsr = eFileSyncResult.Failed;

                lock (m_oUpdateLocker)
                {
                    ct.AddEvent("Inside of lock");

                    LineSr.Instance.NewOrChangedObjects.UnsetPropertiesChanged();
                    LineSr.Instance.NewOrChangedObjects.Clear();

                    bool bProcessUpdateSucceeded = false;

                    lock (m_oReadLocker)
                    {
                        ct.AddEvent("Cache Update Started");
                        bProcessUpdateSucceeded = dm(null);
                        ct.AddEvent("Cache Update Completed ({0}, {1})", LineSr.Instance.NewOrChangedObjects.Count, LineSr.Instance.ObjectsToRemove.Count);
                    }

                    if (bUseDatabase)
                    {
                        // We use database to store Live Data
                        fsr = DataCopy.UpdateDatabase(ConnectionManager.GetConnection(), eut, sProviderDescription, us);
                        ct.AddEvent("Database Update Completed");

                        if (fsr == eFileSyncResult.Succeeded)
                        {
                            LineSr.Instance.NewOrChangedObjects.NotifyPropertiesChanged();
                            DatabaseCache.Instance.AddNewObjectsAfterCommit();
                            DatabaseCache.Instance.RemoveObjectsAfterCommit();
                            LineSr.Instance.ObjectsToRemove.Clear();

                            ct.AddEvent("Routines Completed (fsr=Succeeded)");
                        }
                        else
                        {
                            ct.AddEvent("Routines Completed (fsr=Skipped or Failed)");
                        }
                    }
                    else
                    {
                        // We DON'T use database to store Live Data
                        fsr = eFileSyncResult.Succeeded;
                        LineSr.Instance.NewOrChangedObjects.NotifyPropertiesChanged();
                        ct.AddEvent("Routines Completed");
                    }

                    if (fsr == eFileSyncResult.Succeeded && bProcessUpdateSucceeded)
                    {
                        ct.AddEvent("ProcessDataSqlUpdateSucceeded() Completed");
                    }
                }

                return fsr;
            }
            catch (Exception excp)
            {
                LineSr.Instance.ObjectsToRemove.Clear();
                m_logger.Excp(excp, "SyncRoutines(eUpdateType={0}, sProviderDescription='{1}') ERROR", eut, sProviderDescription);

                ct.AddEvent("Exception Completed");
                ct.Error(m_logger);
            }
            finally
            {
                ct.AddEvent("Completed");
                ct.Info(m_logger);
            }

            return eFileSyncResult.Failed;
        }

        private static void LineObjectCollectionToSerializableObjectList<T>(LineContainer lc, DictionaryOfLineObjectCollection diAll) where T : ILineObject<T>
        {
            LineObjectCollectionToSerializableObjectList<T>(lc, diAll.GetLineObjectCollection<T>());
        }

        private static void LineObjectCollectionToSerializableObjectList<T>(LineContainer lc, ILineObjectCollection<T> locObjects) where T : ILineObject<T>
        {
            string sObjectListName = LineContainer.ContentTypeToObjectListName(typeof(T));

            SyncList<T> lObjects = locObjects.ToSyncList();
            SerializableObjectList lSerializableObjects = lc.Objects.EnsureSerializableObjectList(typeof(T), sObjectListName);

            foreach (T objSource in lObjects)
            {
                try
                {
                    ISerializableObject so = objSource.Serialize();

                    lSerializableObjects.Add(so);
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "ToLineContainer() ERROR. Could not add serializable object for {0}", objSource);
                }
            }

            Thread.Sleep(1);
        }

        public static LineContainer ToLineContainer(DictionaryOfLineObjectCollection diAll)
        {
            dynamic lc = new LineContainer();

            LineObjectCollectionToSerializableObjectList<TaggedStringLn>(lc, diAll);

            LineObjectCollectionToSerializableObjectList<TimeTypeLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<ScoreTypeLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<BetTypeLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<BetDomainTypeLn>(lc, diAll);

            LineObjectCollectionToSerializableObjectList<GroupLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<CompetitorLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<MatchLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<LiveMatchInfoLn>(lc, diAll);
            //LineObjectCollectionToSerializableObjectList<MatchResultLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<MatchToGroupLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<BetDomainLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<OddLn>(lc, diAll);

            return lc;
        }

        public static LineContainer ToMetaDataLineContainer(DictionaryOfLineObjectCollection diAll)
        {
            dynamic lc = new LineContainer();

            LineObjectCollectionToSerializableObjectList<TaggedStringLn>(lc, diAll);

            LineObjectCollectionToSerializableObjectList<TimeTypeLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<ScoreTypeLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<BetTypeLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<BetDomainTypeLn>(lc, diAll);

            //LineObjectCollectionToSerializableObjectList<GroupLn>(lc, diAll);
            LineObjectCollectionToSerializableObjectList<CompetitorLn>(lc, diAll);
            //LineObjectCollectionToSerializableObjectList<MatchLn>(lc, diAll);
            //LineObjectCollectionToSerializableObjectList<LiveMatchInfoLn>(lc, diAll);
            //LineObjectCollectionToSerializableObjectList<MatchResultLn>(lc, diAll);
            //LineObjectCollectionToSerializableObjectList<MatchToGroupLn>(lc, diAll);
            //LineObjectCollectionToSerializableObjectList<BetDomainLn>(lc, diAll);
            //LineObjectCollectionToSerializableObjectList<OddLn>(lc, diAll);

            return lc;
        }

        public delegate T DelegateGetLineObject<T>(ISerializableObject so) where T : ILineObject<T>;
        public delegate T DelegateCreateLineObject<T>() where T : ILineObject<T>;
        public delegate void DelegateOnLineObjectMerged<T>(T obj) where T : ILineObject<T>;
        public delegate void DelegateRemoveLineObject<T>(T obj) where T : ILineObject<T>;

        public struct MergeLineObjectsCallBack<T> where T : ILineObject<T>
        {
            public DelegateGetLineObject<T> GetLineObject;
            public DelegateCreateLineObject<T> CreateLineObject;
            public DelegateOnLineObjectMerged<T> OnLineObjectMerged;
            public DelegateRemoveLineObject<T> RemoveLineObject;
        }

        public static int MergeLineObjects<T>(LineContainer lc, ILineObjectCollection<T> loc, MergeLineObjectsCallBack<T> objectsCallBack) where T : ILineObject<T>
        {
            long lOperationMask = 0L;

            return MergeLineObjects<T>(lc, loc, objectsCallBack, ref lOperationMask);
        }

        public static int MergeLineObjects<T>(LineContainer lc, ILineObjectCollection<T> loc, MergeLineObjectsCallBack<T> objectsCallBack, ref long lOperationMask) where T : ILineObject<T>
        {
            Debug.Assert(objectsCallBack.GetLineObject != null);
            Debug.Assert(objectsCallBack.CreateLineObject != null);

            int iSucceededCount = 0;

            if (loc != null)
            {
                string sObjectListName = LineContainer.ContentTypeToObjectListName(typeof(T));
                SerializableObjectList lObjects = lc.Objects.SafelyGetValue(sObjectListName);

                if (lObjects != null)
                {
                    foreach (ISerializableObject so in lObjects)
                    {
                        //so.MethodTag = dgo;

                        try
                        {
                            T tObj = objectsCallBack.GetLineObject(so);

                            if (so.IsToRemove())
                            {
                                if (tObj != null)
                                {
                                    tObj = loc.MergeLineObject(tObj, so);

                                    if (objectsCallBack.RemoveLineObject != null)
                                    {
                                        objectsCallBack.RemoveLineObject(tObj);
                                        //m_logger.InfoFormat("Removed from Line {0}", tObj);

                                        lOperationMask |= (long)eOperationMask.RemovedFromCollection;
                                    }
                                }
                            }
                            else
                            {
                                if (tObj == null)
                                {
                                    // Object is NEW - DOES NOT exist yet in line
                                    tObj = objectsCallBack.CreateLineObject();
                                    tObj.Deserialize(so);

                                    tObj = loc.MergeLineObject(tObj);
                                    lOperationMask |= (long)eOperationMask.AddedToCollection;
                                    //m_logger.DebugFormat("Added to Line {0}", tObj);
                                }
                                else
                                {
                                    // Object Already Exists                
                                    tObj = loc.MergeLineObject(tObj, so);

                                    if (tObj.ChangedProps != null && tObj.ChangedProps.Count > 0)
                                    {
                                        lOperationMask |= (long)eOperationMask.ObjectEdited;
                                    }
                                }

                                if (objectsCallBack.OnLineObjectMerged != null)
                                {
                                    objectsCallBack.OnLineObjectMerged(tObj);
                                }
                            }

                            iSucceededCount++;
                        }
                        catch (Exception excp)
                        {
                            m_logger.Error(excp.Message, excp);
                            ExcpHelper.ThrowUp(excp, "MergeLineObjects<{0}>() ERROR for {1}", typeof(T).Name, so);
                        }
                    }
                }
            }

            return iSucceededCount;
        }

        static IDictionary<Type, int> counters = new Dictionary<Type, int>();
        static IDictionary<Type, int> counters2 = new Dictionary<Type, int>();
        private static int counter = 0;
        private static bool _allowMultiway;

        public static void LogChanges(object obj, ElapsedEventArgs elapsedEventArgs)
        {
            StringBuilder sb = new StringBuilder();
            ++counter;
            foreach (var allObject in Instance.AllObjects)
            {
                if (counters.ContainsKey(allObject.Key))
                {
                    if (counters[allObject.Key] != allObject.Value.Count)
                    {

                        if (counters[allObject.Key] > allObject.Value.Count)
                        {

                        }
                        sb.AppendFormat("{0} = {1}, ({2})\r\n", allObject.Key, allObject.Value.Count, allObject.Value.Count - counters[allObject.Key]);
                    }
                }
                counters[allObject.Key] = allObject.Value.Count;
            }
            if (counter > 10)
            {
                foreach (var allObject in Instance.AllObjects)
                {
                    sb.AppendFormat("{0} = {1}, \r\n", allObject.Key, allObject.Value.Count);
                }
            }
            if (sb.Length > 0)
            {
                sb.Insert(0, "Line Objects \r\n");
                m_logger.Debug(sb.ToString());
            }
            sb.Clear();


            foreach (var allObject in DatabaseCache.Instance.AllObjects)
            {
                if (counters2.ContainsKey(allObject.Key))
                {
                    if (counters2[allObject.Key] != allObject.Value.Count)
                    {

                        if (counters2[allObject.Key] > allObject.Value.Count)
                        {

                        }
                        sb.AppendFormat("{0} = {1}, ({2})\r\n", allObject.Key, allObject.Value.Count, allObject.Value.Count - counters2[allObject.Key]);
                    }
                }
                counters2[allObject.Key] = allObject.Value.Count;
            }
            if (counter > 10)
            {
                counter = 0;

                foreach (var allObject in Instance.AllObjects)
                {
                    sb.AppendFormat("{0} = {1}, \r\n", allObject.Key, allObject.Value.Count);
                }
            }
            if (sb.Length > 0)
            {
                sb.Insert(0, "Cache Objects \r\n");
                m_logger.Debug(sb.ToString());
            }
        }
    }

    public class RelatedLineObjectNotFoundException : Exception
    {
        public RelatedLineObjectNotFoundException(string sMessage, Exception excpInner)
            : base(sMessage, excpInner)
        {

        }
    }
}
