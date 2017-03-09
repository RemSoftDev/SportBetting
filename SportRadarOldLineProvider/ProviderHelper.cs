using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nbt.Common.Utils.Windows;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using System.Globalization;

namespace SportRadar.DAL.SportRadarOldLineProvider
{
    public static class ProviderHelper
    {
        private const long COLLECTION_CHANGED_MASK = (long)eOperationMask.AddedToCollection | (long)eOperationMask.RemovedFromCollection;

        private static ILog m_logger = LogFactory.CreateLog(typeof(LiveBetClient));

        private static SyncDictionary<long, MultiStringSr> m_diTaggedMultstrings = new SyncDictionary<long, MultiStringSr>();
        private static SyncList<RelatedObject> m_diMultiStringIdToRelatedObject = new SyncList<RelatedObject>();

        private static SyncDictionary<long, string> m_diGroupIdToGroupTag = new SyncDictionary<long, string>();
        private static SyncDictionary<long, string> m_diLanguageIdToShortName = new SyncDictionary<long, string>();
        private static SyncDictionary<string, long> m_diShortNameToLanguageId = new SyncDictionary<string, long>();

        private static SyncDictionary<long, long> m_diCategoryIdToGroupId = new SyncDictionary<long, long>();
        private static SyncDictionary<long, long> m_diTournamentIdToGroupId = new SyncDictionary<long, long>();
        private static SyncDictionary<long, long> m_diCountryIdToGroupId = new SyncDictionary<long, long>();
        private static SyncDictionary<long, long> m_diSportIdToGroupId = new SyncDictionary<long, long>();

        private static IdentityList m_ilExceptMatchIds = new IdentityList();
        private static IdentityList m_ilExceptBetDomainIds = new IdentityList();

        public static long UpdateDatabase(DateTime dtUpdateRequested, bool bUseDatabase, List<UpdateRecordSr> lUpdateRecords)
        {
            DateTime dtUpdateResponsed = DateTime.Now;
            long lLastUpdateFileId = 0;

            try
            {
                int iSkippedCounter = 0;
                int iSuccessCounter = 0;
                int iErrorCounter = 0;

                UpdateStatistics us = new UpdateStatistics();

                //IDbConnection conn = ConnectionManager.GetConnection();
                //{
                for (int iData = 0; iData < lUpdateRecords.Count; iData++)
                {
                    try
                    {
                        UpdateRecordSr ur = lUpdateRecords[iData];

                        UpdateFileEntrySr ufe = UpdateFileEntrySr.GetByFileId(ur.DataSyncCacheId);

                        if (ufe != null)
                        {
                            m_logger.InfoFormat("Skipped file (Length: {0}) {1} of {2} because such FileID ({3}, '{4}') already exists in database (Created: {5})", ur.Data.Length, iData, lUpdateRecords.Count, ur.DataSyncCacheId, ur.FileName, ufe.CreateDate);
                            iSkippedCounter++;

                            continue;
                        }

                        m_logger.InfoFormat("Importing file (Length: {0}) {1} of {2}) {3}", ur.Data.Length, iData + 1, lUpdateRecords.Count, ur);

                        string sXmlData = ur.GetXmlData();
                        SportRadarLineContainer srlc = SportRadarLineContainer.FromXmlString(sXmlData);

                        if (srlc != null)
                        {
                            eFileSyncResult fsr = eFileSyncResult.Failed;

                            fsr = LineSr.SyncRoutines(eUpdateType.PreMatches, string.Format("SportRadar Pre-Match Update. DataSyncCacheId = {0}", ur.DataSyncCacheId), bUseDatabase, us, delegate(object objParam)
                            {
                                return ProviderHelper.MergeFromSportRadarLineContainer(srlc, ur.DataSyncCacheId);
                            });


                            if (fsr == eFileSyncResult.Failed)
                            {
                                m_logger.Error("failed update file " + lUpdateRecords[iData].FileName, new Exception());
                            }

                            lLastUpdateFileId = ur.DataSyncCacheId;

                            // DK: We save file identification (UpdateRecordWS.DataSyncCacheID for files when succeeded or skipped

                            UpdateFileEntrySr newEntry = new UpdateFileEntrySr();

                            newEntry.DataSyncCacheID = ur.DataSyncCacheId;
                            newEntry.DataSyncCacheType = ur.DataSyncCacheType.ToString();
                            newEntry.FileName = ur.FileName;
                            newEntry.Description = ur.Description;
                            newEntry.CreateDate = DateTime.Now;

                            newEntry.Insert();

                            UpdateFileEntrySr.SetLastUpdate(newEntry);

                            Thread.Sleep(10);
                        }
                        else
                        {
                            iErrorCounter++;
                        }
                    }
                    catch (Exception exception)
                    {
                        m_logger.Excp(exception, "UpdateDatabase(dtUpdateRequested = '{0}', ocUpdateRecordId = {1}) ERROR", dtUpdateRequested, lUpdateRecords != null ? lUpdateRecords[iData].DataSyncCacheId.ToString("G") : "0");
                    }
                }
                //}

                DateTime dtUpdateCompleted = DateTime.Now;

                if (iErrorCounter > 0 || iSuccessCounter > 0)
                {
                    string sInfo = @"
SyncDatabase() Report:
-----------------------------------
   Requested Time: {4}
   Responsed Time: {5}
   Completed Time: {6}
      Update Time: {7}
       TOTAL Time: {8}
-----------------------------------
Total Files Count: {0}
    Skipped Files: {1}
  Succeeded Files: {2}
    ERROR Occured: {3}

     Last File ID: {9}
-----------------------------------

{10}
";

                    m_logger.InfoFormat(sInfo,
                                        lUpdateRecords.Count,  // 0
                                        iSkippedCounter,        // 1
                                        iSuccessCounter,        // 2
                                        iErrorCounter > 0,      // 3
                                        dtUpdateRequested,      // 4
                                        dtUpdateResponsed,      // 5
                                        dtUpdateCompleted,      // 6
                                        dtUpdateCompleted - dtUpdateResponsed,  // 7
                                        dtUpdateCompleted - dtUpdateRequested,  // 8
                                        lLastUpdateFileId,      // 9
                                        us); // 10     
                }
                else
                {
                    m_logger.InfoFormat("SyncDatabase() Report: No records updated or inserted. Total Time: {0}", dtUpdateCompleted - dtUpdateRequested);
                }
            }
            catch (System.Exception excp)
            {
                m_logger.Excp(excp, "UpdateDatabase(dtUpdateRequested = '{0}', ocUpdateRecords = {1}) ERROR", dtUpdateRequested, lUpdateRecords != null ? lUpdateRecords.Count.ToString("G") : "NULL");
            }

            return lLastUpdateFileId;
        }

        private static void MergeStrings(TranslationDictionary diLineStrings)
        {
            foreach (ObjectStringDictionary di in diLineStrings.Values)
            {
                var ro = m_diMultiStringIdToRelatedObject.Clone().Where(x => x.MultistringId == di.MultiStringId).ToList();

                foreach (TaggedStringLn str in di.Values)
                {
                    foreach (var relatedObject in ro)
                    {
                        str.SetRelation(relatedObject.ObjectType, relatedObject.ObjectId);
                        LineSr.Instance.AllObjects.TaggedStrings.MergeLineObject(str);
                    }

                    Debug.Assert(str.StringId != 0);
                    LineSr.Instance.AllObjects.TaggedStrings.MergeLineObject(str);
                }
            }
        }

        private static void PreMergeStrings(SportRadarLineContainer srlc, TranslationDictionary diLineStrings)
        {

            try
            {
                if (srlc.Language != null)
                {
                    foreach (LanguageSr lng in srlc.Language)
                    {
                        m_diLanguageIdToShortName[lng.LanguageID] = lng.ShortName.ToLowerInvariant();
                        m_diShortNameToLanguageId[lng.ShortName.ToLowerInvariant()] = lng.LanguageID;

                        LanguageLn lang = new LanguageLn();
                        try
                        {

                            lang.LanguageId = lng.LanguageID;
                            lang.ShortName = lng.ShortName;
                            lang.IsTerminal = lng.IsTerminalLanguage > 0;

                            LineSr.Instance.AllObjects.Languages.MergeLineObject(lang);
                            //LineSr.Instance.EnsureLanguage(lng.LanguageID, lng.ShortName, lng.IsTerminalLanguage > 0);
                        }
                        catch (Exception ex)
                        {
                            m_logger.Excp(ex, "LineSr.PreMergeStrings ERROR. Cannot Merge {0} from {1}", lang, lng);
                            throw;
                        }
                    }
                }

                if (srlc.MultiStringGroup != null)
                {
                    foreach (MultiStringGroupSr mgrp in srlc.MultiStringGroup)
                    {
                        m_diGroupIdToGroupTag[mgrp.MultiStringGroupID] = mgrp.MultiStringGroupTag;
                        var msGroup = new MultistringGroupLn(mgrp.MultiStringGroupID, mgrp.MultiStringGroupTag);
                        LineSr.Instance.AllObjects.MultistringGroups.MergeLineObject(msGroup);

                    }
                }

                if (srlc.MultiString != null)
                {
                    foreach (MultiStringSr ms in srlc.MultiString)
                    {
                        ms.MultiStringTag = ms.MultiStringTag.Trim();

                        /*
                        if (ms.MultiStringID == 821)
                        {
                    
                        }
                        //*/

                        m_diTaggedMultstrings[ms.MultiStringID] = ms;
                    }
                }

                if (srlc.LanguageString != null)
                {
                    foreach (LanguageStringSr ls in srlc.LanguageString)
                    {
                        // Tagged Strings (to be located in DB table 'TaggedString')
                        if (m_diTaggedMultstrings.ContainsKey(ls.MultiStringID))
                        {
#if DEBUG
                            if (ls.LanguageStringID == 350)
                            {

                            }
#endif
                            MultiStringSr msParent = m_diTaggedMultstrings[ls.MultiStringID];

                            TaggedStringLn tsl = new TaggedStringLn();

                            tsl.StringId = ls.LanguageStringID;
                            tsl.Tag = msParent.MultiStringTag.ToLowerInvariant();
                            if (msParent.MultiStringID == -997)
                            {
                                tsl.Language = "3";
                            }
                            if (!m_diLanguageIdToShortName.ContainsKey(ls.LanguageID))
                                continue;
                            tsl.Language = m_diLanguageIdToShortName[ls.LanguageID].ToLowerInvariant();

                            if (msParent.MultiStringGroupID != null)
                            {
                                tsl.Category = m_diGroupIdToGroupTag[(long)msParent.MultiStringGroupID];
                            }

                            tsl.Text = ls.Text;

                            diLineStrings.AddString(msParent.MultiStringID, tsl);
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "LineSr.MergeStrings() ERROR");
                throw;
            }
        }

        private static void SetMultiStringRelatedObject(long lMultiStringId, eObjectType eot, long lObjectId)
        {
            RelatedObject ro = m_diMultiStringIdToRelatedObject.Clone().Where(x => x.MultistringId == lMultiStringId).Where(x => x.ObjectId == lObjectId).FirstOrDefault();

            if (ro == null)
            {
                m_diMultiStringIdToRelatedObject.Add(new RelatedObject(eot, lObjectId, lMultiStringId));
            }
            else
            {
                Debug.Assert(ro.ObjectType == eot);
                //Debug.Assert(ro.ObjectId == lObjectId);
            }
        }

        private static void MergeGroups(SportRadarLineContainer srlc, TranslationDictionary diLineStrings)
        {
#if DEBUG
            const long SEARCH_MULTISTRING_ID = 280;
#endif

            try
            {
                // Countries
                if (srlc.Country != null)
                {
                    foreach (CountrySr cntr in srlc.Country)
                    {
#if DEBUG
                        if (cntr.MultiStringID == SEARCH_MULTISTRING_ID)
                        {

                        }
#endif
                        var group = new GroupLn();

                        group.GroupId = DatabaseCache.Instance.GetGroupId(GroupLn.GROUP_TYPE_COUNTRY, cntr.CountryID);
                        group.ParentGroupId.Value = null;
                        group.Type = GroupLn.GROUP_TYPE_COUNTRY;
                        group.SvrGroupId = cntr.CountryID;
                        group.Sort.Value = 0;

                        m_diCountryIdToGroupId.SafelyAdd(cntr.CountryID, group.GroupId);

                        if (cntr.MultiStringID != null)
                        {
                            SetMultiStringRelatedObject((long)cntr.MultiStringID, eObjectType.Group, group.GroupId);
                        }

                        LineSr.Instance.AllObjects.Groups.MergeLineObject(group);
                    }
                }


                // Sports
                if (srlc.Sport != null)
                {
                    foreach (SportSr sprt in srlc.Sport)
                    {
#if DEBUG
                        if (sprt.MultiStringID == SEARCH_MULTISTRING_ID)
                        {

                        }
#endif

                        var group = new GroupLn();

                        group.GroupId = DatabaseCache.Instance.GetGroupId(GroupLn.GROUP_TYPE_SPORT, sprt.SportID);
                        group.ParentGroupId.Value = null;
                        group.Type = GroupLn.GROUP_TYPE_SPORT;
                        group.Sort.Value = 0;
                        group.SvrGroupId = sprt.SportID;
                        Debug.Assert(!string.IsNullOrEmpty(sprt.Tag));
                        group.GroupSport.SportDescriptor = sprt.Tag;

                        MultiStringSr ms = m_diTaggedMultstrings.SafelyGetValue(sprt.MultiStringID);

                        if (ms != null)
                        {
                            if (ms.MultiStringTag == SportSr.SOCCER_SPRT_MST)
                            {
                                group.Sort.Value = SportSr.SOCCER_SORT;
                            }
                            else if (ms.MultiStringTag == SportSr.TENNIS_SPRT_MST)
                            {
                                group.Sort.Value = SportSr.TENNIS_SORT;
                            }
                            else if (ms.MultiStringTag == SportSr.BASKETBALL_SPRT_MST)
                            {
                                group.Sort.Value = SportSr.BASKETBALL_SORT;
                            }
                            else if (ms.MultiStringTag == SportSr.ICE_HOCKEY_SPRT_MST)
                            {
                                group.Sort.Value = SportSr.ICE_HOCKEY_SORT;
                            }
                            else if (ms.MultiStringTag == SportSr.VOLLEYBALL_SPRT_MST)
                            {
                                group.Sort.Value = SportSr.VOLLEYBALL_SORT;
                            }
                            else if (ms.MultiStringTag == SportSr.RUGBY_SPRT_MST)
                            {
                                group.Sort.Value = SportSr.RUGBY_SORT;
                            }
                        }
                        group.GroupSport.BtrSportId = sprt.BtrSportID;
                        m_diSportIdToGroupId.SafelyAdd(sprt.SportID, group.GroupId);

                        SetMultiStringRelatedObject(sprt.MultiStringID, eObjectType.Group, group.GroupId);
                        LineSr.Instance.AllObjects.Groups.MergeLineObject(group);
                    }
                }

                // Categories
                if (srlc.Category != null)
                {
                    foreach (CategorySr ctrg in srlc.Category)
                    {
#if DEBUG
                        if (ctrg.MultiStringID == SEARCH_MULTISTRING_ID)
                        {

                        }
#endif
                        var group = new GroupLn();

                        group.GroupId = DatabaseCache.Instance.GetGroupId(GroupLn.GROUP_TYPE_GROUP_C, ctrg.CategoryID);
                        group.ParentGroupId.Value = null;
                        group.Type = GroupLn.GROUP_TYPE_GROUP_C;
                        group.SvrGroupId = ctrg.CategoryID;
                        group.Sort.Value = ctrg.Sort;

                        m_diCategoryIdToGroupId.SafelyAdd(ctrg.CategoryID, group.GroupId);

                        SetMultiStringRelatedObject(ctrg.MultiStringID, eObjectType.Group, group.GroupId);
                        LineSr.Instance.AllObjects.Groups.MergeLineObject(group);
                    }
                }

                // Tournaments
                if (srlc.Tournament != null)
                {
                    foreach (TournamentSr trmt in srlc.Tournament)
                    {
#if DEBUG
                        if (trmt.MultiStringID == SEARCH_MULTISTRING_ID)
                        {

                        }
#endif
                        var group = new GroupLn();
                        group.GroupId = DatabaseCache.Instance.GetGroupId(GroupLn.GROUP_TYPE_GROUP_T, trmt.TournamentID);
                        group.ParentGroupId.Value = null;

                        if (trmt.CategoryID != null)
                        {
                            long lCategoryId = (long)trmt.CategoryID;

                            if (m_diCategoryIdToGroupId.ContainsKey(lCategoryId))
                            {
                                group.ParentGroupId.Value = m_diCategoryIdToGroupId[lCategoryId];
                            }
                            else
                            {
                                var parentGroup = LineSr.Instance.AllObjects.Groups.SafelyGetGroupByKeyName(GroupLn.GROUP_TYPE_GROUP_C, lCategoryId);

                                if (parentGroup != null)
                                {
                                    group.ParentGroupId.Value = parentGroup.GroupId;
                                }
                                else
                                {

                                }
                            }
                        }

                        group.Type = GroupLn.GROUP_TYPE_GROUP_T;
                        group.SvrGroupId = trmt.TournamentID;
                        group.Sort.Value = trmt.Sort;
                        group.GroupTournament.MinCombination = trmt.MinCombination;

                        if (m_diSportIdToGroupId.ContainsKey(trmt.SportID))
                        {
                            long lSportGroupId = m_diSportIdToGroupId[trmt.SportID];

                            group.GroupTournament.SportGroupId = lSportGroupId;
                        }
                        else
                        {
                            var sportGroup = LineSr.Instance.AllObjects.Groups.SafelyGetGroupByKeyName(GroupLn.GROUP_TYPE_SPORT, trmt.SportID);

                            group.GroupTournament.SportGroupId = sportGroup.GroupId;
                        }

                        if (trmt.CountryID != null)
                        {
                            long lCountryId = (long)trmt.CountryID;

                            if (m_diCountryIdToGroupId.ContainsKey(lCountryId))
                            {
                                long lCountryGroupId = m_diCountryIdToGroupId[lCountryId];

                                group.GroupTournament.CountryGroupId = lCountryGroupId;
                            }
                            else
                            {
                                var countryGroup = LineSr.Instance.AllObjects.Groups.SafelyGetGroupByKeyName(GroupLn.GROUP_TYPE_COUNTRY, lCountryId);

                                group.GroupTournament.CountryGroupId = countryGroup.GroupId;
                            }
                        }

                        if (trmt.BtrTournamentID != null)
                        {
                            group.GroupTournament.BtrTournamentId = trmt.BtrTournamentID;
                        }

                        m_diTournamentIdToGroupId.SafelyAdd(trmt.TournamentID, group.GroupId);
                        if (trmt.MultiStringID == -997)
                        {
                            trmt.MultiStringID = -997;
                        }
                        SetMultiStringRelatedObject(trmt.MultiStringID, eObjectType.Group, group.GroupId);

                        LineSr.Instance.AllObjects.Groups.MergeLineObject(group);
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "LineSr.MergeGroups() ERROR");
            }
        }

        private static void MergeCompetitors(SportRadarLineContainer srlc, TranslationDictionary diLineStrings)
        {
            if (srlc.Competitor != null)
            {
                foreach (CompetitorSr cmpt in srlc.Competitor)
                {
                    CompetitorLn competitor = null;

                    try
                    {
                        competitor = new CompetitorLn();

                        competitor.CompetitorId = cmpt.CompetitorID;
                        competitor.BtrCompetitorId = cmpt.BtrCompetitorID == null ? 0 : (long)cmpt.BtrCompetitorID;

                        if (cmpt.MultiStringID != null)
                        {
                            SetMultiStringRelatedObject(cmpt.MultiStringID.Value, eObjectType.Competitor, competitor.CompetitorId);
                        }

                        LineSr.Instance.AllObjects.Competitors.MergeLineObject(competitor);
                    }
                    catch (Exception excp)
                    {
                        m_logger.Excp(excp, "LineSr.MergeCompetitors() ERROR. Cannot Merge {0} from {1}", competitor, cmpt);
                        throw;
                    }
                }
            }
        }

        private static void MergeStatistics(SportRadarLineContainer srlc)
        {
            if (srlc.MatchInfos != null)
            {
                foreach (MatchInfoSr info in srlc.MatchInfos)
                {
                    MatchInfosLn matchInfo = null;

                    try
                    {
                        matchInfo = new MatchInfosLn();
                        matchInfo.MatchInfoId = info.MatchInfoId;

                        DateTime date = new DateTime();
                        DateTime.TryParse(info.LastModifiedString, out date);

                        matchInfo.LastModified = date;
                        matchInfo.external_state.StatisticValues = info.StatisticValues;

                        LineSr.Instance.AllObjects.MatchInfos.MergeLineObject(matchInfo);
                    }
                    catch (Exception ex)
                    {
                        m_logger.Excp(ex, "LineSr.MergeMatchInfos ERROR. Cannot Merge {0} from {1}", matchInfo, info);
                        throw;
                    }
                }
            }

            if (srlc.TournamentInfos != null)
            {
                foreach (TournamentInfoSr info in srlc.TournamentInfos)
                {
                    TournamentInfosLn TournamentInfo = null;

                    try
                    {
                        TournamentInfo = new TournamentInfosLn();
                        TournamentInfo.TournamentInfoId = info.TournamentInfoId;

                        DateTime date = new DateTime();
                        DateTime.TryParse(info.LastModifiedString, out date);

                        TournamentInfo.LastModified = date;

                        TournamentInfo.external_state.CompetitorsContainer = info.CompetitorInfoCollections.CompetitorInfos;

                        LineSr.Instance.AllObjects.TournamentInfos.MergeLineObject(TournamentInfo);
                    }
                    catch (Exception ex)
                    {
                        m_logger.Excp(ex, "LineSr.MergeTournamentInfos ERROR. Cannot Merge {0} from {1}", TournamentInfo, info);
                        throw;
                    }
                }
            }

            if (srlc.CompetitorInfos != null)
            {
                foreach (CompetitorInfoSr info in srlc.CompetitorInfos)
                {
                    CompetitorInfosLn competitorInfo = null;

                    try
                    {
                        competitorInfo = new CompetitorInfosLn();
                        competitorInfo.CompetitorInfoId = info.CompetitorInfoId;
                        competitorInfo.SuperBtrId = Int64.Parse(info.StatisticValues.Where(x => x.Name == "BTR_SUPER_ID").Select(x => x.Value).FirstOrDefault());

                        DateTime date = new DateTime();
                        DateTime.TryParse(info.LastModifiedString, out date);

                        competitorInfo.LastModified = date;
                        competitorInfo.TshirtAway = info.TshirtAway;
                        competitorInfo.TshirtHome = info.TshirtHome;
                        competitorInfo.external_state.StatisticValues = info.StatisticValues;

                        LineSr.Instance.AllObjects.CompetitorInfos.MergeLineObject(competitorInfo);
                    }
                    catch (Exception ex)
                    {
                        m_logger.Excp(ex, "LineSr.MergeCompetitorInfos ERROR. Cannot Merge {0} from {1}", competitorInfo, info);
                        throw;
                    }
                }
            }
        }

        private static void MergeActiveTournaments(SportRadarLineContainer srlc)
        {
            LineSr.Instance.AllObjects.ActiveTournaments.Clear();
            ActiveTournamentLn actTour = null;
            foreach (ActiveTournamentSr at in srlc.ActiveTournaments)
            {
                try
                {
                    actTour = new ActiveTournamentLn();
                    actTour.Active = true;
                    actTour.Id = at.Id;
                    decimal factor = 1;
                    Decimal.TryParse(at.OddIncreaseDecrease, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out factor);
                    actTour.OddIncreaseDecrease = factor;
                    actTour.Markets = at.VisibleMarkets;

                    LineSr.Instance.AllObjects.ActiveTournaments.MergeLineObject(actTour);
                }
                catch (Exception ex)
                {
                    m_logger.Excp(ex, "LineSr.MergeActiveTournaments ERROR. Cannot Merge {0} from {1}", actTour, at);
                    throw;
                }
            }

            foreach (KeyValuePair<long, MatchLn> match in LineSr.Instance.AllObjects.Matches.ToSyncDictionary())
            {
                if (match.Value.SourceType != eServerSourceType.BtrPre)
                    continue;

                match.Value.MatchView.RefreshProps();
            }
        }

        private static void MergeConfidenceFactors(SportRadarLineContainer srlc)
        {
            decimal factor;
            LiabilityLn liab = null;

            TournamentMatchLocksLn tmlX = null;
            DatabaseCache.Instance.AllObjects.TournamentMatchLocks.Clear();
            DatabaseCache.Instance.AllObjects.Liabilities.Clear();
            LineSr.Instance.AllObjects.TournamentMatchLocks.Clear();
            LineSr.Instance.AllObjects.Liabilities.Clear();

            foreach (LiabilitySR li in srlc.CFs)
            {
                try
                {
                    if (li != null && li.LiabilityType == "MATCH_TOURNAMENT_LOCKS")
                    {
                        //TODO: later to be moved to separate place
                        tmlX = new TournamentMatchLocksLn();
                        tmlX.TMKey = li.LiabilityID;
                        tmlX.arrlocks = li.LiabilityValue;

                        LineSr.Instance.AllObjects.TournamentMatchLocks.MergeLineObject(tmlX);
                    }
                    else
                    {
                        liab = new LiabilityLn();

                        if (li.LiabilityType == LineSr.CONF_RATING_VALUES)
                        {
                            liab.CFKey = li.LiabilityID + "*" + li.LiabilityType;
                            Decimal.TryParse(li.LiabilityValue.Split('|')[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out factor);
                            liab.factor = factor;
                            try
                            {
                                if (li.LiabilityValue.Split('|').Length > 1)
                                    Decimal.TryParse(li.LiabilityValue.Split('|')[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out factor);
                                liab.livefactor = factor;

                            }
                            catch (Exception)
                            {
                            }
                            LineSr.Instance.AllObjects.Liabilities.MergeLineObject(liab);


                        }
                        else if (li.LiabilityType == LineSr.TOURN_CONF_RATING)
                        {
                            try
                            {
                                liab = new LiabilityLn();
                                liab.CFKey = li.LiabilityID + "*" + li.LiabilityType;

                                int id = 0;
                                int btrid = 0;
                                int.TryParse(li.LiabilityID.Split('|')[0], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out btrid);
                                int.TryParse(li.LiabilityID.Split('|')[1], System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out id);
                                liab.CFKey = id + "*" + li.LiabilityType;
                                Decimal.TryParse(li.LiabilityValue, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out factor);
                                liab.factor = factor;
                                LineSr.Instance.AllObjects.Liabilities.MergeLineObject(liab);

                                liab = new LiabilityLn();
                                liab.CFKey = btrid + "*" + li.LiabilityType;
                                liab.factor = factor;
                                LineSr.Instance.AllObjects.Liabilities.MergeLineObject(liab);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            liab.CFKey = li.LiabilityID + "*" + li.LiabilityType;
                            Decimal.TryParse(li.LiabilityValue, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out factor);
                            liab.factor = factor;
                            LineSr.Instance.AllObjects.Liabilities.MergeLineObject(liab);

                        }

                    }
                }
                catch (Exception ex)
                {
                    m_logger.Excp(ex, "LineSr.MergeConfidenceFactors ERROR. Cannot Merge {0} from {1}", liab, li);
                    throw;
                }
            }
        }

        private static void MergeFlagResources(SportRadarLineContainer srlc)
        {
            if (srlc.BinaryResourceAssignments != null)
                foreach (ResourceAssignmentSr res in srlc.BinaryResourceAssignments)
                {
                    ResourceAssignmentLn resource = null;
                    try
                    {
                        resource = new ResourceAssignmentLn();
                        resource.ObjectId = res.ObjectId;
                        resource.ResourceType = (eAssignmentType)Enum.Parse(typeof(eAssignmentType), res.ObjectClass);
                        resource.Active= res.IsActive;
                        resource.ResourceId = res.ResourceId;

                        LineSr.Instance.AllObjects.ResourceAssignments.MergeLineObject(resource);
                    }
                    catch (Exception excp)
                    {
                        m_logger.Excp(excp, "LineSr.MergeFlagResources() ERROR. Cannot Merge {0} from {1}", resource, res);
                        throw;
                    }
                }

            if (srlc.BinaryResources != null)
                foreach (ResourceRepositorySR res in srlc.BinaryResources)
                {
                    ResourceRepositoryLn resource = null;
                    try
                    {
                        resource = new ResourceRepositoryLn();
                        resource.ResourceId = res.ResourceId;
                        resource.ResourceType = (eResourceType)Enum.Parse(typeof(eResourceType), res.ResourceType);
                        resource.MimeType = res.MimeType;
                        resource.Data = res.Data;

                        LineSr.Instance.AllObjects.Resources.MergeLineObject(resource);
                    }
                    catch (Exception excp)
                    {
                        m_logger.Excp(excp, "LineSr.MergeFlagResources() ERROR. Cannot Merge {0} from {1}", resource, res);
                        throw;
                    }
                }
        }

        /*
        private static string BuildMatchToCompetitorKey(long lMatchId, int iHomeTeam)
        {
            return string.Format("{0}*{1}", lMatchId, iHomeTeam);
        }
        */

        private static long GetCompetitorId(Dictionary<long, Dictionary<int, MatchToCompetitorSr>> diMatchToCompetitor, MatchSr mtch, int iHomeTeam)
        {
            ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(!diMatchToCompetitor.ContainsKey(mtch.MatchID), "Cannot find Match Competitor for {0}", mtch);

            Dictionary<int, MatchToCompetitorSr> diCompetitorPositions = diMatchToCompetitor[mtch.MatchID];

            ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(!diCompetitorPositions.ContainsKey(iHomeTeam), "Cannot find {0} Competitor for {1}", iHomeTeam, mtch);

            MatchToCompetitorSr mtoc = diCompetitorPositions[iHomeTeam];

            return mtoc.CompetitorID;
        }

        private static void MergeMatchToGroup(long lMatchId, GroupLn group, int iSort)
        {
#if DEBUG
            if (lMatchId == 52169)
            {

            }
#endif

            MatchToGroupLn mtog = new MatchToGroupLn();

            mtog.MatchId = lMatchId;
            mtog.GroupId = group.GroupId;
            mtog.Type = group.Type;
            mtog.Sort = iSort;

            LineSr.Instance.AllObjects.MatchesToGroups.MergeLineObject(mtog);
        }

        private static GroupLn GetTournamentGroup(long lTournamentId)
        {
            if (m_diTournamentIdToGroupId.ContainsKey(lTournamentId))
            {
                return LineSr.Instance.AllObjects.Groups.GetObject(m_diTournamentIdToGroupId[lTournamentId]);
            }

            return LineSr.Instance.AllObjects.Groups.SafelyGetGroupByKeyName(GroupLn.GROUP_TYPE_GROUP_T, lTournamentId);
        }

        /*
        private static GroupLn GetTournamentSportGroup(long lTournamentId)
        {
            ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(!m_diTournamentIdToSportGroupId.ContainsKey(lTournamentId), "GetTournamentSportGroup({0}) ERROR. No relation to Sport.", lTournamentId);

            return LineSr.Instance.AllObjects.Groups.GetObject(m_diTournamentIdToSportGroupId[lTournamentId]);
        }

        private static GroupLn GetTournamentCountryGroup(long lTournamentId)
        {
            if (m_diTournamentIdToCountryGroupId.ContainsKey(lTournamentId))
            {
                return LineSr.Instance.AllObjects.Groups.GetObject(m_diTournamentIdToCountryGroupId[lTournamentId]);
            }

            return null;
        }
        */

        private static void MergeMatchResult(MatchSr mtch, Dictionary<long, Dictionary<int, MatchToCompetitorSr>> diMatchToCompetitor)
        {
            MatchResultLn rslt = new MatchResultLn();

            rslt.MatchId = mtch.MatchID;
            rslt.BtrMatchId = mtch.BtrMatchID != null ? (long)mtch.BtrMatchID : 0;
            rslt.StartDate.Value = mtch.StartDateOffset ?? new DateTimeSr(mtch.StartDate);
            rslt.HomeCompetitorId.Value = GetCompetitorId(diMatchToCompetitor, mtch, MatchSr.HOME_TEAM);
            rslt.AwayCompetitorId.Value = GetCompetitorId(diMatchToCompetitor, mtch, MatchSr.AWAY_TEAM);
            rslt.IsLiveBet.Value = mtch.IsLiveBet;
            rslt.Score.Value = mtch.MatchScore;
            rslt.TeamWon = mtch.TeamWon;
            rslt.ExtendedState.Value = string.Empty;

            GroupLn groupSport;
            string sSportId = "";

            if (mtch.TournamentID != null)
            {
                long lTournamentId = (long)mtch.TournamentID;

                var groupTournament = GetTournamentGroup(lTournamentId);
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(groupTournament == null, "Cannot find Tournament Group for {0} and {1}", mtch, rslt);
                rslt.TournamentGroupId.Value = groupTournament.GroupId;

                groupSport = LineSr.Instance.AllObjects.Groups.GetObject(groupTournament.GroupTournament.SportGroupId);
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(groupSport == null, "Cannot find Sport Group for {0} and {1}", mtch, rslt);
                rslt.SportGroupId.Value = groupSport.GroupId;
                sSportId = groupSport.GroupSport.SportDescriptor;

                if (groupTournament == null || groupTournament.ParentGroupId == null || groupTournament.ParentGroupId.Value == null)
                    return;

                rslt.CategoryGroupId.Value = groupTournament.ParentGroupId.Value;
            }


            //if (!mtch.IsLiveBet)
            //{
            //    rslt.Score.Value = mtch.FormatResult(sSportId);
            //}

            LineSr.Instance.AllObjects.MatchResults.MergeLineObject(rslt);
        }

#if DEBUG || TRACE_ID_FROM_FILE
        private static void BtrMatchIdFromFileFound(long lFoundLong, params object[] args)
        {
            MatchSr mtch = args[0] as MatchSr;
            m_logger.InfoFormat("MergeMatches() found pointed {0}", mtch);
        }
#endif


        private static void MergeMatches(SportRadarLineContainer srlc, Dictionary<long, Dictionary<int, MatchToCompetitorSr>> diMatchToCompetitor, SyncDictionary<long, long> diBaseBetDomainsToMathes, ref long lOperationMask)
        {
            if (srlc.Match != null)
            {
                foreach (MatchSr mtch in srlc.Match)
                {
                    var match = new MatchLn();

#if DEBUG || TRACE_ID_FROM_FILE
                    if (TraceHelper.TraceFromFileLongValues("BtrMatchIds.txt", (long)mtch.BtrMatchID, new TraceHelper.DelegateLongFromRegistryFound(BtrMatchIdFromFileFound), mtch))
                    {
                        // Put breakpoint here to catch certain match by mtch.BtrMatchID
                    }
#endif
                    try
                    {
                        if (mtch.BetDomainID != null)
                        {
                            diBaseBetDomainsToMathes.SafelyAdd((long)mtch.BetDomainID, mtch.MatchID);
                        }

                        // Check if incoming match is result
                        bool bIsMatchResultInLive = false;
                        bool bIsMatchResult = false;

                        if (mtch.IsLiveBet)
                        {
                            bIsMatchResultInLive = mtch.LiveBetStatus == MatchSr.MATCH_STATE_LIVEBET_STARTED || mtch.LiveBetStatus == MatchSr.MATCH_STATE_LIVEBET_STOPPED;
                            bIsMatchResult = bIsMatchResultInLive || mtch.LiveBetStatus == MatchSr.MATCH_STATE_LIVEBET_ENDED;
                        }
                        else
                        {
                            bIsMatchResult = mtch.State == MatchSr.MATCH_STATE_CALCULATED || mtch.State == MatchSr.MATCH_STATE_RESULTENTERED || mtch.State == MatchSr.MATCH_STATE_CALCULATION_RUNNING;
                        }

                        if (bIsMatchResult)
                        {
                            // This is Pre-Match result
                            MergeMatchResult(mtch, diMatchToCompetitor);

                            if (!bIsMatchResultInLive)
                            {
                                LineSr.Instance.RemoveMatch(mtch.MatchID);
                                m_ilExceptMatchIds.AddUnique(mtch.MatchID); // We have to let know child objects to skipmul

                                lOperationMask |= (long)eOperationMask.RemovedFromCollection;

                                continue;
                            }
                        }

                        // MatchLn
                        match.MatchId = mtch.MatchID;
                        match.BtrMatchId = mtch.BtrMatchID != null ? (long)mtch.BtrMatchID : 0;
                        match.StartDate.Value = mtch.StartDateOffset ?? new DateTimeSr(mtch.StartDate);
                        match.ExpiryDate.Value = mtch.ExpiryDateOffset ?? new DateTimeSr(mtch.ExpiryDate.Value);
                        match.EndDate.Value = mtch.EndDateOffset ?? new DateTimeSr(mtch.EndDate != null ? (DateTime)mtch.EndDate : DbConvert.DATETIMENULL);
                        match.outright_type = (eOutrightType)(mtch.OutrightType ?? 0);

                        if (match.outright_type == eOutrightType.None)
                        {
                            match.HomeCompetitorId.Value = GetCompetitorId(diMatchToCompetitor, mtch, MatchSr.HOME_TEAM);
                            match.AwayCompetitorId.Value = GetCompetitorId(diMatchToCompetitor, mtch, MatchSr.AWAY_TEAM);
                        }
                        else if (match.outright_type == eOutrightType.Outright)
                        {
                            ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(!diMatchToCompetitor.ContainsKey(mtch.MatchID), "Cannot find Match Competitor for {0}", mtch);
                            Dictionary<int, MatchToCompetitorSr> diCompetitorPositions = diMatchToCompetitor[mtch.MatchID];

                            PositionToOutrightDictionary diOld = LineSr.Instance.AllObjects.CompetitorsToOutright.GetPositionToOutrightDictionaryByMatchId(match.MatchId);
                            SyncList<CompetitorToOutrightLn> lOldCompetitorToOutrightItems = diOld != null ? diOld.ToSyncList() : null; // Remember here old items
                            SyncList<long> lNewCompetitorToOutrightIds = new SyncList<long>();

                            if (diOld != null)
                            {
                                diOld.Clear(); // Clear old item because we ALWAYS replace old CompetitorToOutrightLn items with new ones
                            }

                            foreach (int iOutrightPos in diCompetitorPositions.Keys)
                            {
                                MatchToCompetitorSr mtoc = diCompetitorPositions[iOutrightPos];
                                Debug.Assert(mtoc.MatchID == match.MatchId);

                                CompetitorToOutrightLn cto = new CompetitorToOutrightLn();

                                cto.match2competitorid = mtoc.MatchToCompetitorID;
                                cto.CompetitorId = mtoc.CompetitorID;
                                cto.MatchId = mtoc.MatchID;
                                cto.hometeam = mtoc.HomeTeam;
                                cto.ExtendedId = 0;
                                cto.ExtendedState = string.Empty;

                                LineSr.Instance.AllObjects.CompetitorsToOutright.MergeLineObject(cto);

                                lNewCompetitorToOutrightIds.Add(cto.match2competitorid); // Remeber here new item id(s)
                            }

                            if (lOldCompetitorToOutrightItems != null)
                            {
                                foreach (CompetitorToOutrightLn cto in lOldCompetitorToOutrightItems)
                                {
                                    if (!lNewCompetitorToOutrightIds.Contains(cto.match2competitorid)) // Check if old item is expred
                                    {
                                        // Remove old item
                                        LineSr.Instance.AllObjects.CompetitorsToOutright.Remove(cto.match2competitorid);
                                        LineSr.Instance.ObjectsToRemove.SafelyAddObject(cto);
                                    }
                                }
                            }

                            if (diOld != null)
                            {
                                Debug.Assert(diOld.Count == diCompetitorPositions.Count);
                            }

                            match.HomeCompetitorId.Value =
                            match.AwayCompetitorId.Value = 0L;

                            if (mtch.InfoMultiStringId != null)
                            {
                                int iMultiStringId = (int)mtch.InfoMultiStringId;

                                if (m_diTaggedMultstrings.ContainsKey(iMultiStringId))
                                {
                                    match.NameTag.Value = m_diTaggedMultstrings[iMultiStringId].MultiStringTag;
                                }
                            }
                        }
                        else
                        {
                            Debug.Assert(false);
                        }

                        match.Code.Value = mtch.Code;
                        match.Active.Value = mtch.Active;
                        match.IsLiveBet.Value = mtch.IsLiveBet;
                        match.SourceType = mtch.ServerSourceType;

                        if (match.IsLiveBet.Value && match.SourceType == eServerSourceType.BtrPre)
                        {
                            match.SourceType = eServerSourceType.BtrLive;
                        }

                        match.MatchExternalState.CardsTeam1 = mtch.CardsTeam1;
                        match.MatchExternalState.CardsTeam2 = mtch.CardsTeam2;
                        match.MatchExternalState.MinCombination = mtch.MinCombination;

                        var matchMerged = LineSr.Instance.AllObjects.Matches.MergeLineObject(match);
                        Debug.Assert(matchMerged != null);

                        if (matchMerged.IsNew)
                        {
                            lOperationMask |= (long)eOperationMask.AddedToCollection;
                        }

                        int i = 0;

                        if (matchMerged.ChangedProps.Contains(matchMerged.IsLiveBet))
                        {
                            if (matchMerged.IsLiveBet.Value == false && matchMerged.IsLiveBet.PreviousValue == true)
                            {
                                i++;
                                m_logger.Info(i.ToString("G"));
                            }
                        }


                        if (mtch.TournamentID != null)
                        {
                            long lTournamentId = (long)mtch.TournamentID;

                            var groupTournament = GetTournamentGroup(lTournamentId);
                            ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(groupTournament == null, "Cannot find Tournament Group for {0} and {1}", mtch, matchMerged);
                            MergeMatchToGroup(match.MatchId, groupTournament, 0);

                            var groupSport = LineSr.Instance.AllObjects.Groups.GetObject(groupTournament.GroupTournament.SportGroupId);
                            ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(groupSport == null, "Cannot find Sport Group for {0} and {1}", mtch, matchMerged);
                            MergeMatchToGroup(match.MatchId, groupSport, 0);

                            if (groupTournament.GroupTournament.CountryGroupId != null)
                            {
                                var groupCountry = LineSr.Instance.AllObjects.Groups.GetObject((long)groupTournament.GroupTournament.CountryGroupId);
                                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(groupCountry == null, "Cannot find Country Group for {0} and {1}", mtch, matchMerged);
                                MergeMatchToGroup(match.MatchId, groupCountry, 0);
                            }
                        }

                        if (mtch.IsLiveBet)
                        {
                            // LiveMatchInfoLn in separate table
                            LiveMatchInfoLn lmil = new LiveMatchInfoLn();

                            lmil.MatchId = mtch.MatchID;
                            lmil.ExpiryDate.Value = mtch.ExpiryDateOffset ?? new DateTimeSr(mtch.ExpiryDate != null ? (DateTime)mtch.ExpiryDate : DbConvert.DATETIMENULL);
                            lmil.MatchMinute.Value = mtch.MatchMinute;
                            lmil.PeriodInfo.Value = LiveMatchInfoLn.IntToPeriodInfo(mtch.LB_PeriodInfo);
                            lmil.Status.Value = LiveMatchInfoLn.IntToMatchStatus(mtch.LiveBetStatus);
                            lmil.Score.Value = mtch.MatchScore;

                            LineSr.Instance.AllObjects.LiveMatchInfos.MergeLineObject(lmil);
                        }
                    }
                    catch (RelatedLineObjectNotFoundException excp)
                    {
                        m_logger.Excp(excp, "LineSr.MergeMatches() ERROR. Cannot Merge {0} from {1}", match, mtch);
                    }
                    catch (Exception excp)
                    {
                        m_logger.Excp(excp, "LineSr.MergeMatches() ERROR. Cannot Merge {0} from {1}", match, mtch);
                        throw;
                    }
                }
            }
        }

#if DEBUG || TRACE_ID_FROM_FILE
        private static void BtrLiveBetIdFromFileFound(long lFoundLong, params object[] args)
        {
            BetDomainSr bdmn = args[0] as BetDomainSr;
            m_logger.InfoFormat("MergeBetDomains() found pointed {0}", bdmn);
        }
#endif

        private static void MergeBetDomains(SportRadarLineContainer srlc, SyncDictionary<long, long> diBaseBetDomainsToMathes, ref long lOperationMask)
        {
            if (srlc.BetDomain != null)
            {
                foreach (BetDomainSr bdmn in srlc.BetDomain)
                {
#if DEBUG || TRACE_ID_FROM_FILE
                    if (TraceHelper.TraceFromFileLongValues("BtrLiveBetIds.txt", bdmn.BtrLiveBetID, new TraceHelper.DelegateLongFromRegistryFound(BtrLiveBetIdFromFileFound), bdmn))
                    {
                        // Put breakpoint here to catch certain match by bdmn.BtrLiveBetID
                    }
#endif
                    // First let's find MatchId (It may be BaseBetDomain => MatchId might be null)
                    long lMatchId = 0;

                    if (bdmn.MatchID != null)
                    {
                        lMatchId = (long)bdmn.MatchID;
                    }
                    else
                    {
                        ExcpHelper.ThrowIf(!diBaseBetDomainsToMathes.ContainsKey(bdmn.BetDomainID), "Base Bet Domain ({0}) does not have relation to Match", bdmn);
                        lMatchId = diBaseBetDomainsToMathes[bdmn.BetDomainID];
                    }

                    // Second - let's check if we do not need to merge because Parent Match should be removed from line
                    if (LineSr.Instance.ObjectsToRemove.ContainsObjectById<BetDomainLn>(bdmn.BetDomainID) || m_ilExceptMatchIds.Contains(lMatchId))
                    {
                        m_ilExceptBetDomainIds.AddUnique(bdmn.BetDomainID); // We have to let know child Odds to skip
                        lOperationMask |= (long)eOperationMask.RemovedFromCollection;
                        continue;
                    }

                    // Third - Check if current BetDomain should be removed from line
                    eBetDomainStatus betDomainStatus = BetDomainLn.IntToBetDomainStatus(bdmn.Status);
                    if (betDomainStatus == eBetDomainStatus.Forbidden)
                    {
                        LineSr.Instance.RemoveBetDomain(bdmn.BetDomainID);

                        m_ilExceptBetDomainIds.AddUnique(bdmn.BetDomainID); // We have to let know child Odds to skip
                        lOperationMask |= (long)eOperationMask.RemovedFromCollection;
                        continue;
                    }

                    BetDomainLn betDomain = null;

                    try
                    {
                        BetDomainMapItem bdmi = BetDomainMap.Instance.GetBetDomainMapItem(bdmn.BetDomainNumber);
                        ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(bdmi == null, "Cannot map type for {0}", bdmn);

                        string sBetTag = bdmi.IsGeneric ? string.Format(bdmi.BetTag, Convert.ToInt32(bdmn.SpecialOddValue)) : bdmi.BetTag;

                        betDomain = new BetDomainLn();

                        betDomain.BetDomainId = bdmn.BetDomainID;
                        betDomain.BtrLiveBetId.Value = bdmn.BtrLiveBetID;
                        betDomain.MatchId = lMatchId;
                        betDomain.Status.Value = betDomainStatus;
                        betDomain.BetTag = sBetTag;
                        betDomain.BetDomainNumber.Value = bdmn.BetDomainNumber;

                        long lMultiStringId = 0;

                        if (bdmn.MultiStringID != null)
                        {
                            lMultiStringId = (long)bdmn.MultiStringID;
                        }
                        else if (bdmn.MultiStringID2 != null)
                        {
                            lMultiStringId = (long)bdmn.MultiStringID2;
                        }

                        ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(lMultiStringId == 0, "MergeBetDomains() ERROR. Incoming LineSr does not contain translation. {0}", bdmn);

                        ExcpHelper.ThrowIf(!m_diTaggedMultstrings.ContainsKey(lMultiStringId), "MergeBetDomains() ERROR. Cannot find MultiString (MultiStringID = {0}) for {1}", bdmn.MultiStringID, bdmn);
                        betDomain.NameTag = m_diTaggedMultstrings[lMultiStringId].MultiStringTag;

                        betDomain.Sort.Value = bdmn.Sort != null ? (int)bdmn.Sort : 0;
                        betDomain.IsLiveBet.Value = bdmn.IsLiveBet;
                        betDomain.SpecialOddValue.Value = bdmn.SpecialOddValue;
                        betDomain.SpecialOddValueFull.Value = bdmn.SpecialLiveOddValue_Full;
                        betDomain.Result.Value = string.Empty;
                        betDomain.BetDomainExternalState.MinCombination = bdmn.MinCombination;
                        betDomain.IsManuallyDisabled.Value = bdmn.IsManualLiveBetDomain ?? false;


                        var betDomainMerged = LineSr.Instance.AllObjects.BetDomains.MergeLineObject(betDomain);

                        if (betDomainMerged.IsNew)
                        {
                            lOperationMask |= (long)eOperationMask.AddedToCollection;
                        }

#if DEBUG
                        IBetDomainVw vw = betDomainMerged.BetDomainView;
                        string s1 = vw.BetTypeName;
                        string s2 = vw.ScoreTypeName;
                        string s3 = vw.TimeTypeName;
#endif
                    }
                    catch (RelatedLineObjectNotFoundException excp)
                    {
                        m_logger.WarnFormat("LineSr.MergeBetDomains() Warning. Cannot Merge {0} from {1}", betDomain, bdmn);
                    }
                    catch (Exception excp)
                    {
                        m_logger.Excp(excp, "LineSr.MergeBetDomains() ERROR. Cannot Merge {0} from {1}", betDomain, bdmn);
                        throw;
                    }
                }
            }
        }

        private static void MergeOdds(SportRadarLineContainer srlc)
        {
            if (srlc.Odd != null)
            {
                foreach (OddSr bodd in srlc.Odd)
                {
                    if (bodd.BetDomainID != null && (LineSr.Instance.ObjectsToRemove.ContainsObjectById<OddLn>(bodd.OddID) || m_ilExceptBetDomainIds.Contains(bodd.BetDomainID.Value)))
                    {
                        continue;
                    }

                    OddLn odd = null;
                    if (LineSr.Instance.AllObjects.Odds.ContainsKey(bodd.OddID))
                        odd = LineSr.Instance.AllObjects.Odds.SafelyGetValue(bodd.OddID);
                    else
                    {
                        odd = new OddLn();
                    }
                    try
                    {
                        odd.OutcomeId =
                        odd.OddId.Value = bodd.OddID; // DK - For SportRadar always OddId == OutcomeId. Except special case for Liga Stavok. 
                        if (bodd.BetDomainID != null)
                            odd.BetDomainId = bodd.BetDomainID.Value;
                        if (bodd.Value != null)
                            odd.Value.Value = bodd.Value.Value;
                        odd.OddTag.Value = bodd.OddTag;

                        if (bodd.MultiStringID != null)
                        {
                            int iMultiStringId = (int)bodd.MultiStringID;
                            ExcpHelper.ThrowIf(!m_diTaggedMultstrings.ContainsKey(iMultiStringId), "MergeOdds() ERROR. Cannot find MultiString (MultiStringID = {0}) for {1}", bodd.MultiStringID, bodd);
                            odd.NameTag.Value = m_diTaggedMultstrings[iMultiStringId].MultiStringTag;
                        }
                        else
                        {
                            if (bodd.OddTag != null)
                                odd.NameTag.Value = bodd.OddTag;
                        }

                        if (bodd.Sort != null)
                            odd.Sort.Value = bodd.Sort.Value;
                        if (bodd.Active != null)
                            odd.Active.Value = bodd.Active.Value;
                        if (bodd.Status != null)
                            odd.IsManuallyDisabled.Value = bodd.Status.Value == 2;
                        if (bodd.IsLiveBet != null)
                            odd.IsLiveBet.Value = bodd.IsLiveBet.Value;
                        odd.ExtendedState.Value = string.Empty;

                        LineSr.Instance.AllObjects.Odds.MergeLineObject(odd);
                    }
                    catch (RelatedLineObjectNotFoundException excp)
                    {
                        m_logger.WarnFormat("LineSr.MergeOdds() Warning. Cannot Merge {0} from {1}", odd, bodd);
                    }
                    catch (Exception excp)
                    {
                        m_logger.Excp(excp, "LineSr.MergeOdds() ERROR. Cannot Merge {0} from {1}", odd, bodd);
                        throw;
                    }
                }
            }
        }

        public static bool MergeFromSportRadarLineContainer(SportRadarLineContainer srlc, long dataSyncCacheId)
        {
            if (m_diLanguageIdToShortName.Count == 0)
            {
                foreach (var lang in DatabaseCache.Instance.AllObjects.Languages)
                {
                    m_diLanguageIdToShortName[lang.Value.Id] = lang.Value.ShortName;
                    m_diShortNameToLanguageId[lang.Value.ShortName.ToLowerInvariant()] = lang.Value.Id;
                }
            }
            if (m_diGroupIdToGroupTag.Count == 0)
            {
                foreach (var msGroup in DatabaseCache.Instance.AllObjects.MultistringGroups)
                {
                    m_diGroupIdToGroupTag[msGroup.Value.Id] = msGroup.Value.MultiStringGroupTag;
                }
            }

            long lOperationMask = 0L;

            //TraceDictionary di = new TraceDictionary();
            // di.FillFromContainer(srlc);
            /*  if (di.IsSomeChildWithContent)
              {
                  LiveSportRadarLineContainer lsrlc = srlc as LiveSportRadarLineContainer;

                  string sInfo = lsrlc != null ? di.GetContentMessage("Received Live (Duration='{0}')", lsrlc.Duration) : di.GetContentMessage("Received Container");
                  //m_logger.Info(sInfo);
              }*/

            CheckTime ct = new CheckTime("MergeFromSportRadarLineContainer");
            m_ilExceptMatchIds.Clear();
            m_ilExceptBetDomainIds.Clear();

            // Strings
            TranslationDictionary diLineStrings = new TranslationDictionary();

            PreMergeStrings(srlc, diLineStrings);

            if (srlc.BetDomainTypeLnList != null)
            {
                MergeBetDomainType(srlc);
                ct.AddEvent("BetDomainType Merged");
            }

            if (srlc.BinaryResources != null || srlc.BinaryResourceAssignments != null)
            {
                MergeFlagResources(srlc);
                ct.AddEvent("FlagResources Merged");
            }

            if (srlc.MatchInfos != null || srlc.TournamentInfos != null || srlc.CompetitorInfos != null)
            {
                MergeStatistics(srlc);
                ct.AddEvent("Statistics Merged");
            }

            if (srlc.CFs != null)
            {
                MergeConfidenceFactors(srlc);
            }

            if (srlc.ActiveTournaments != null)
            {
                MergeActiveTournaments(srlc);
            }

            ///////////////////////////////////////////////////////////////////////
            // Convert Categories, Tournaments, Countries, Sports into Groups
            ///////////////////////////////////////////////////////////////////////
            MergeGroups(srlc, diLineStrings);
            ct.AddEvent("Groups Merged");

            MergeCompetitors(srlc, diLineStrings);
            ct.AddEvent("Competitors Merged ({0})", srlc.Match != null ? srlc.Competitor.Length : 0);

            MergeStrings(diLineStrings);
            ct.AddEvent("Strings Merged");

            Dictionary<long, Dictionary<int, MatchToCompetitorSr>> diMatchToCompetitor = new Dictionary<long, Dictionary<int, MatchToCompetitorSr>>();

            // MatchToCompetitor
            if (srlc.MatchToCompetitor != null)
            {
                for (int i = 0; i < srlc.MatchToCompetitor.Length; i++)
                {
                    MatchToCompetitorSr mtoc = srlc.MatchToCompetitor[i];
                    mtoc.UpdateId = dataSyncCacheId;

                    Dictionary<int, MatchToCompetitorSr> diCompetitorPositions = null;
                    if (mtoc.HomeTeam == -1)
                        continue;
                    if (!diMatchToCompetitor.ContainsKey(mtoc.MatchID))
                    {
                        diMatchToCompetitor[mtoc.MatchID] = diCompetitorPositions = new Dictionary<int, MatchToCompetitorSr>();
                    }
                    else
                    {
                        diCompetitorPositions = diMatchToCompetitor[mtoc.MatchID];
                    }

                    if (!diCompetitorPositions.ContainsKey(mtoc.HomeTeam))
                        diCompetitorPositions.Add(mtoc.HomeTeam, mtoc);
                }
            }

            SyncDictionary<long, long> diBaseBetDomainsToMathes = new SyncDictionary<long, long>();

            // Matches
            MergeMatches(srlc, diMatchToCompetitor, diBaseBetDomainsToMathes, ref lOperationMask);
            ct.AddEvent("Matches Merged ({0})", srlc.Match != null ? srlc.Match.Length : 0);

            // Bet Domains
            MergeBetDomains(srlc, diBaseBetDomainsToMathes, ref lOperationMask);
            ct.AddEvent("BetDomains Merged ({0})", srlc.BetDomain != null ? srlc.BetDomain.Length : 0);

            // Odds
            MergeOdds(srlc);
            ct.AddEvent("Odd Merged ({0})", srlc.Odd != null ? srlc.Odd.Length : 0);

#if DEBUG2
            LineObjectCollection<LiveMatchInfoLn> lNewOrChangedMatches = LineSr.Instance.NewOrChangedObjects.GetLineObjectCollection<LiveMatchInfoLn>();

            if (lNewOrChangedMatches != null && lNewOrChangedMatches.Count > 0)
            {
                int iRnd = m_rnd.Next(100);

                ExcpHelper.ThrowIf(iRnd > 90, "Test Exception");
            }
#endif

            ct.Info(m_logger);

            return (lOperationMask & COLLECTION_CHANGED_MASK) > 0;
        }

        private static void MergeBetDomainType(SportRadarLineContainer srlc)
        {
            if (srlc.BetDomainTypeLnList != null)
            {
                foreach (BetDomainTypeSr typeLn in srlc.BetDomainTypeLnList)
                {
                    BetDomainTypeLn betDomainTypeLn = null;

                    try
                    {
                        betDomainTypeLn = new BetDomainTypeLn();
                        betDomainTypeLn.Tag = typeLn.Tag;
                        betDomainTypeLn.BetTypeTag = typeLn.BetTypeTag;
                        betDomainTypeLn.ScoreTypeTag = typeLn.ScoreTypeTag;
                        betDomainTypeLn.TimeTypeTag = typeLn.TimeTypeTag;
                        betDomainTypeLn.ExternalState = typeLn.ExternalState;
                        betDomainTypeLn.Sort = typeLn.Sort;
                        betDomainTypeLn.Name = typeLn.Name;
                        betDomainTypeLn.Active = typeLn.Active;
                        betDomainTypeLn.EnsureExternalObjects();

                        LineSr.Instance.AllObjects.BetDomainTypes.MergeLineObject(betDomainTypeLn);
                    }
                    catch (Exception ex)
                    {
                        m_logger.Excp(ex, "LineSr.MergeMatchInfos ERROR. Cannot Merge {0} from {1}", betDomainTypeLn, typeLn);
                        throw;
                    }
                }
            }
        }

#if DEBUG
        private static Random m_rnd = new Random();
#endif

        public static string SaveErrorFile(string sFileId, string sXmlContent)
        {
            string sXmlFileName = Path.Combine(LogFactory.GetLogFilePath(), string.Format("{0}.ERROR.xml", sFileId));

            if (!File.Exists(sXmlFileName))
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sXmlFileName))
                    {
                        string sXml = XElement.Parse(sXmlContent).ToString();
                        sw.Write(sXml);
                    }
                }
                catch (Exception excp)
                {
                    sXmlFileName = string.Format("{0}\r\n{1}", excp.Message, excp.StackTrace);
                }

                return sXmlFileName;
            }

            return string.Empty;
        }
    }
}
