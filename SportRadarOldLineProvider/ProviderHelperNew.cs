using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;

namespace SportRadar.DAL.SportRadarOldLineProvider
{
    public static class ProviderHelperNew
    {
        private const long EVENT_REASON_MASK = (long)eOperationMask.AddedToCollection | (long)eOperationMask.RemovedFromCollection | (long)eOperationMask.MatchPeriodInfoChanged;

        private static ILog m_logger = LogFactory.CreateLog(typeof(ProviderHelperNew));

        public static bool MergeFromLineContainer(LineContainer lc)
        {
            CheckTime ct = new CheckTime(false, "MergeFromLineContainer() entered");

            long lOperationMask = 0L;

            eServerSourceType esst = eServerSourceType.BtrPre;

            if (lc.Attributes.ContainsKey("line"))
            {
                string sLine = lc.Attributes["line"];
                ExcpHelper.ThrowIf(!Enum.TryParse(sLine, true, out esst), "Cannot parse LineType from '{0}'", sLine);
            }

            string sType = lc.Attributes["type"];
            string sMessageId = lc.Attributes["messageid"];

            try
            {

                /*
                sTrace += string.Format("{0} docid={1}\r\n", sTrace, sMessageId);

                if (bExtendTrace)
                {
                    sTrace += lc.BuildTraceString();
                }
                */

                // TimeTypeLn
                {
                    TimeTypeDictionary ttd = LineSr.Instance.AllObjects.GetLineObjectCollection<TimeTypeLn>() as TimeTypeDictionary;

                    int iCount = LineSr.MergeLineObjects<TimeTypeLn>(lc, ttd, new LineSr.MergeLineObjectsCallBack<TimeTypeLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<string> spTag = so.GetSerializableProperty("Tag") as SerializableProperty<string>;

                            return ttd.GetObject(spTag.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new TimeTypeLn();
                        }
                    });

                    ct.AddEvent("TimeType(s) Succeeded ({0})", iCount);
                }

                // ScoreTypeLn
                {
                    ScoreTypeDictionary std = LineSr.Instance.AllObjects.GetLineObjectCollection<ScoreTypeLn>() as ScoreTypeDictionary;

                    int iCount = LineSr.MergeLineObjects<ScoreTypeLn>(lc, std, new LineSr.MergeLineObjectsCallBack<ScoreTypeLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<string> spTag = so.GetSerializableProperty("Tag") as SerializableProperty<string>;

                            return std.GetObject(spTag.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new ScoreTypeLn();
                        }
                    });

                    ct.AddEvent("ScoreType(s) Succeeded ({0})", iCount);
                }

                // BetTypeLn
                {
                    BetTypeDictionary btd = LineSr.Instance.AllObjects.GetLineObjectCollection<BetTypeLn>() as BetTypeDictionary;

                    int iCount = LineSr.MergeLineObjects<BetTypeLn>(lc, btd, new LineSr.MergeLineObjectsCallBack<BetTypeLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<string> spTag = so.GetSerializableProperty("Tag") as SerializableProperty<string>;

                            return btd.GetObject(spTag.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new BetTypeLn();
                        }
                    });

                    ct.AddEvent("BetType(s) Succeeded ({0})", iCount);
                }

                // BetDomainTypeLn
                {
                    BetDomainTypeDictionary bdtd = LineSr.Instance.AllObjects.GetLineObjectCollection<BetDomainTypeLn>() as BetDomainTypeDictionary;

                    int iCount = LineSr.MergeLineObjects<BetDomainTypeLn>(lc, bdtd, new LineSr.MergeLineObjectsCallBack<BetDomainTypeLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<string> spTag = so.GetSerializableProperty("Tag") as SerializableProperty<string>;

                            return bdtd.GetObject(spTag.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new BetDomainTypeLn();
                        }
                    });

                    ct.AddEvent("BetDomainType(s) Succeeded ({0})", iCount);
                }

                // Groups
                {
                    GroupDictionary gd = LineSr.Instance.AllObjects.GetLineObjectCollection<GroupLn>() as GroupDictionary;

                    int iCount = LineSr.MergeLineObjects<GroupLn>(lc, gd, new LineSr.MergeLineObjectsCallBack<GroupLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            // 143881406612

                            SerializableProperty<string> spType = so.GetSerializableProperty("Type") as SerializableProperty<string>;
                            SerializableProperty<long> spSvrId = so.GetSerializableProperty("SvrGroupId") as SerializableProperty<long>;

                            Debug.Assert(spType != null);
                            Debug.Assert(spSvrId != null);

                            return gd.SafelyGetGroupByKeyName(spType.Value, spSvrId.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new GroupLn();
                        }
                    });

                    ct.AddEvent("Group(s) Succeeded ({0})", iCount);
                }

                // Competitors
                {
                    CompetitorDictionary cd = LineSr.Instance.AllObjects.GetLineObjectCollection<CompetitorLn>() as CompetitorDictionary;

                    int iCount = LineSr.MergeLineObjects<CompetitorLn>(lc, cd, new LineSr.MergeLineObjectsCallBack<CompetitorLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<long> spCompetitor = so.GetSerializableProperty("CompetitorId") as SerializableProperty<long>;

                            Debug.Assert(spCompetitor != null);

                            return cd.GetObject(spCompetitor.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new CompetitorLn();
                        }
                    });

                    ct.AddEvent("Competitor(s) Succeeded ({0})", iCount);
                }
                // CompetitorToOutrightLn
                {
                    var cd = LineSr.Instance.AllObjects.GetLineObjectCollection<CompetitorToOutrightLn>() as CompetitorToOutrightDictionary;

                    int iCount = LineSr.MergeLineObjects<CompetitorToOutrightLn>(lc, cd, new LineSr.MergeLineObjectsCallBack<CompetitorToOutrightLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<long> spCompetitor = so.GetSerializableProperty("match2competitorid") as SerializableProperty<long>;

                            Debug.Assert(spCompetitor != null);

                            return cd.GetObject(spCompetitor.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new CompetitorToOutrightLn();
                        }
                    });

                    ct.AddEvent("Competitor(s) Succeeded ({0})", iCount);
                }

                // Strings
                {
                    TaggedStringDictionary tsd = LineSr.Instance.AllObjects.GetLineObjectCollection<TaggedStringLn>() as TaggedStringDictionary;
                    int iCount = LineSr.MergeLineObjects<TaggedStringLn>(lc, tsd, new LineSr.MergeLineObjectsCallBack<TaggedStringLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<long> spStringId = so.GetSerializableProperty("StringId") as SerializableProperty<long>;

                            return tsd.GetObject(spStringId.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new TaggedStringLn();
                        }
                    });

                    ct.AddEvent("String(s) Succeeded ({0})", iCount);
                }

                SyncList<MatchLn> lMatchesToRemove = new SyncList<MatchLn>();

                // Matches
                {
                    SyncHashSet<long> lMatchIds = new SyncHashSet<long>();

                    MatchDictionary md = LineSr.Instance.AllObjects.GetLineObjectCollection<MatchLn>() as MatchDictionary;
                    int iCount = LineSr.MergeLineObjects<MatchLn>(lc, md, new LineSr.MergeLineObjectsCallBack<MatchLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<long> spMatchId = so.GetSerializableProperty("MatchId") as SerializableProperty<long>;

                            lMatchIds.Add(spMatchId.Value);

                            return md.GetObject(spMatchId.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new MatchLn();
                        },
                        OnLineObjectMerged = delegate(MatchLn match)
                        {
                            if (match != null)
                            {
                                if (match.Code.Value == 0)
                                {
                                    match.Code.Value = Math.Abs((int)(match.MatchId % 100000));
                                }

                                if (match.EndDate.Value == null)
                                {
                                    match.EndDate.Value = new DateTimeSr(DateTime.MaxValue);
                                }
                            }
                        },
                        RemoveLineObject = delegate(MatchLn match)
                        {
                            lMatchesToRemove.Add(match);
                        }
                    }, ref lOperationMask);

                    if (sType.ToLowerInvariant() == "initial")
                    {
                        SyncList<MatchLn> lAllLiveMatches = LineSr.Instance.QuickSearchMatches(delegate(MatchLn match)
                        {
                            return match.IsLiveBet.Value && match.SourceType == esst;
                        });

                        foreach (var match in lAllLiveMatches)
                        {
                            if (!lMatchIds.Contains(match.MatchId))
                            {
                                lMatchesToRemove.Add(match);
                            }
                        }
                    }

                    ct.AddEvent("Match(es) Succeeded ({0})", iCount);
                }

                // MatchToGroup items
                {
                    MatchToGroupDictionary mtogd = LineSr.Instance.AllObjects.GetLineObjectCollection<MatchToGroupLn>() as MatchToGroupDictionary;
                    int iCount = LineSr.MergeLineObjects<MatchToGroupLn>(lc, mtogd, new LineSr.MergeLineObjectsCallBack<MatchToGroupLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<long> spMatchId = so.GetSerializableProperty("MatchId") as SerializableProperty<long>;
                            SerializableProperty<long> spGroupId = so.GetSerializableProperty("GroupId") as SerializableProperty<long>;

                            return mtogd.GetObject(MatchToGroupLn.GetKeyName(spMatchId.Value, spGroupId.Value));
                        },
                        CreateLineObject = delegate()
                        {
                            return new MatchToGroupLn();
                        }
                    });

                    ct.AddEvent("MatchToGroup(s) Succeeded ({0})", iCount);
                }

                // LiveMatchInfo
                {
                    LiveMatchInfoDictionary lmid = LineSr.Instance.AllObjects.GetLineObjectCollection<LiveMatchInfoLn>() as LiveMatchInfoDictionary;
                    int iCount = LineSr.MergeLineObjects<LiveMatchInfoLn>(lc, lmid, new LineSr.MergeLineObjectsCallBack<LiveMatchInfoLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<long> spMatchId = so.GetSerializableProperty("MatchId") as SerializableProperty<long>;

                            return lmid.GetObject(spMatchId.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new LiveMatchInfoLn();
                        },
                        OnLineObjectMerged = delegate(LiveMatchInfoLn matchInfo)
                        {
                            if (matchInfo != null)
                            {
                                if (matchInfo.ExpiryDate.Value == null)
                                {
                                    matchInfo.ExpiryDate.Value = new DateTimeSr(DateTime.Now.AddMinutes(30)); // Half an hour
                                }

                                if (matchInfo.ChangedProps.Contains(matchInfo.PeriodInfo))
                                {
                                    lOperationMask |= (long)eOperationMask.MatchPeriodInfoChanged;
                                }
                            }
                        },
                        RemoveLineObject = delegate(LiveMatchInfoLn lmi)
                        {
                        }
                    });

                    ct.AddEvent("LiveMatchInfo(s) Succeeded ({0})", iCount);
                }

                // BetDomainLn
                {
                    BetDomainDictionary bdmd = LineSr.Instance.AllObjects.GetLineObjectCollection<BetDomainLn>() as BetDomainDictionary;
                    int iCount = LineSr.MergeLineObjects<BetDomainLn>(lc, bdmd, new LineSr.MergeLineObjectsCallBack<BetDomainLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<long> spBetDomainId = so.GetSerializableProperty("BetDomainId") as SerializableProperty<long>;

                            return bdmd.GetObject(spBetDomainId.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new BetDomainLn();
                        },
                        RemoveLineObject = delegate(BetDomainLn bdmn)
                        {
                            LineSr.Instance.RemoveBetDomain(bdmn);
                        }
                    }, ref lOperationMask);

                    ct.AddEvent("BetDomain(s) Succeeded ({0})", iCount);
                }

                // OddLn
                {
                    OddDictionary oddd = LineSr.Instance.AllObjects.GetLineObjectCollection<OddLn>() as OddDictionary;
                    int iCount = LineSr.MergeLineObjects<OddLn>(lc, oddd, new LineSr.MergeLineObjectsCallBack<OddLn>()
                    {
                        GetLineObject = delegate(ISerializableObject so)
                        {
                            SerializableProperty<long> spOutcomeId = so.GetSerializableProperty("OutcomeId") as SerializableProperty<long>;

                            return oddd.GetObject(spOutcomeId.Value);
                        },
                        CreateLineObject = delegate()
                        {
                            return new OddLn();
                        },
                        RemoveLineObject = delegate(OddLn odd)
                        {
                            odd.Active.Value = false;
                            //LineSr.Instance.RemoveOdd(odd);
                        }
                    });

                    ct.AddEvent("Odd(s) Succeeded ({0})", iCount);
                }

                // Process Removed matches
                foreach (var match in lMatchesToRemove)
                {
                    LiveMatchInfoLn lmi = match.LiveMatchInfo;

                    if (lmi.Status.Value == eMatchStatus.Ended && match.SourceType == eServerSourceType.BtrLive)
                    {
                        MergeMatchResult(match);
                    }

                    LineSr.Instance.RemoveMatch(match);
                }
            }
            catch (Exception excp)
            {
                ct.AddEvent("ERROR");
                m_logger.Error(excp.Message, excp);
                m_logger.Excp(excp, "MergeFromLigaStavok() ERROR");
                throw;
            }
            finally
            {
                //m_logger.Info(sTrace);
                //m_logger.DebugFormat("LineContainer Trace Length = {0}", sTrace.Length);
                ct.AddEvent("MergeFromLigaStavok(Type={0}, MessageId={1}) completed", sType, sMessageId);
                ct.Info(m_logger);
            }

#if DEBUG
            if ((lOperationMask & EVENT_REASON_MASK) > 0)
            {
                m_logger.InfoFormat("MergeFromLineContainer() result {0}", lOperationMask);
            }
#endif

            return true;
        }

        private static void MergeMatchResult(MatchLn mtch)
        {
            try
            {
                MatchResultLn rslt = new MatchResultLn();

                rslt.MatchId = mtch.MatchId;
                rslt.BtrMatchId = mtch.BtrMatchId;
                rslt.StartDate.Value = mtch.StartDate.Value;
                rslt.HomeCompetitorId.Value = mtch.HomeCompetitorId.Value;
                rslt.AwayCompetitorId.Value = mtch.AwayCompetitorId.Value;
                rslt.IsLiveBet.Value = mtch.IsLiveBet.Value;
                rslt.Score.Value = mtch.LiveMatchInfo.Score.Value;
                rslt.ExtendedState.Value = string.Empty;
                rslt.TeamWon = mtch.TeamWon.Value;

                var lGroups = mtch.ParentGroups.Clone();

                GroupLn groupTournament = null;
                GroupLn groupSport = null;
                GroupLn groupCategory = null;

                foreach (var group in lGroups)
                {
                    if (group.Type == GroupLn.GROUP_TYPE_GROUP_T)
                    {
                        groupTournament = group;
                    }
                    else if (group.Type == GroupLn.GROUP_TYPE_SPORT)
                    {
                        groupSport = group;
                    }
                    else if (group.Type == GroupLn.GROUP_TYPE_GROUP_C)
                    {
                        groupCategory = group;
                    }
                }

                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(groupTournament == null, "Cannot find Tournament Group for {0} and {1}", mtch, rslt);
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(groupSport == null, "Cannot find Sport Group for {0} and {1}", mtch, rslt);
                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(groupCategory == null, "Cannot find Category Group for {0} and {1}", mtch, rslt);

                rslt.TournamentGroupId.Value = groupTournament.GroupId;
                rslt.SportGroupId.Value = groupSport.GroupId;
                rslt.CategoryGroupId.Value = groupCategory.GroupId;

                LineSr.Instance.AllObjects.MatchResults.MergeLineObject(rslt);
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "MergeMatchResult ERROR for {0}", mtch);
            }
        }
    }

    public static class TempRestService
    {

    }
}
