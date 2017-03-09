using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;

namespace SportRadar.DAL.CommonObjects
{
    [XmlType("SportRadarLineContainer")]
    public class SportRadarLineContainer
    {
        public MultiStringGroupSr[] MultiStringGroup { get; set; }
        public MultiStringSr[] MultiString { get; set; }
        public LanguageSr[] Language { get; set; }
        public LanguageStringSr[] LanguageString { get; set; }

        public CountrySr[] Country { get; set; }
        public SportSr[] Sport { get; set; }
        public CategorySr[] Category { get; set; }
        public TournamentSr[] Tournament { get; set; }
        public TournamentLockSr[] TournamentLock { get; set; }
        public CompetitorSr[] Competitor { get; set; }
        public MatchSr[] Match { get; set; }
        public BetDomainSr[] BetDomain { get; set; }
        public OddSr[] Odd { get; set; }
        public MatchToCompetitorSr[] MatchToCompetitor { get; set; }

        public ResourceRepositorySR[] BinaryResources { get; set; }
        public ResourceAssignmentSr[] BinaryResourceAssignments { get; set; }
        public BetDomainTypeSr[] BetDomainTypeLnList { get; set; }

        public CompetitorInfoSr[] CompetitorInfos { get; set; }
        public MatchInfoSr[] MatchInfos { get; set; }
        public TournamentInfoSr[] TournamentInfos { get; set; }

        public LiabilitySR[] CFs { get; set; }

        public ActiveTournamentSr[] ActiveTournaments { get; set; }

        public static SportRadarLineContainer FromXmlString(string sXmlString)
        {
            return LineSerializeHelper.StringToObject<SportRadarLineContainer>(sXmlString);
        }
        public static string ToXmlString(SportRadarLineContainer srlc)
        {
            return LineSerializeHelper.ObjectToString<SportRadarLineContainer>(srlc);
        }
        public string ToXmlString()
        {
            return LineSerializeHelper.ObjectToString<SportRadarLineContainer>(this);
        }

        public static bool IsObjectArrayEmpty<T>(T[] array)
        {
            return array == null || array.Length == 0;
        }

        public bool IsEmpty
        {
            get
            {
                return
                    IsObjectArrayEmpty<MultiStringGroupSr>(MultiStringGroup) &&
                    IsObjectArrayEmpty<MultiStringSr>(MultiString) &&
                    IsObjectArrayEmpty<LanguageSr>(Language) &&
                    IsObjectArrayEmpty<LanguageStringSr>(LanguageString) &&

                    IsObjectArrayEmpty<CountrySr>(Country) &&
                    IsObjectArrayEmpty<SportSr>(Sport) &&
                    IsObjectArrayEmpty<CategorySr>(Category) &&
                    IsObjectArrayEmpty<TournamentSr>(Tournament) &&
                    //IsObjectArrayEmpty<TournamentLockSr>(TournamentLock) &&
                    IsObjectArrayEmpty<CompetitorSr>(Competitor) &&
                    IsObjectArrayEmpty<MatchSr>(Match) &&
                    IsObjectArrayEmpty<BetDomainSr>(BetDomain) &&
                    IsObjectArrayEmpty<OddSr>(Odd) &&
                    IsObjectArrayEmpty<MatchToCompetitorSr>(MatchToCompetitor) &&

                    IsObjectArrayEmpty<ResourceRepositorySR>(BinaryResources) &&
                    IsObjectArrayEmpty<ResourceAssignmentSr>(BinaryResourceAssignments) &&

                    IsObjectArrayEmpty<CompetitorInfoSr>(CompetitorInfos) &&
                    IsObjectArrayEmpty<MatchInfoSr>(MatchInfos) &&
                    IsObjectArrayEmpty<BetDomainTypeSr>(BetDomainTypeLnList) &&
                    IsObjectArrayEmpty<TournamentInfoSr>(TournamentInfos) &&
                    IsObjectArrayEmpty<LiabilitySR>(CFs) &&
                    IsObjectArrayEmpty<ActiveTournamentSr>(ActiveTournaments);
            }
        }
    }

    public class LiveSportRadarLineContainer : SportRadarLineContainer
    {
        public const string DEFAULT_MESSAGE_ID = "69576C19-602B-43F8-A565-92E623DFDC0C";

        public string SessionId { get; set; }
        public string MessageId { get; set; }
        public bool IsInitialLoad { get; set; }
        public DateTimeSr ServerLastModified { get; set; }
        public DateTimeSr ServerCreated { get; set; }

        [XmlIgnore]
        public TimeSpan Duration { get; set; }

        public LiveSportRadarLineContainer()
        {
            this.MessageId = DEFAULT_MESSAGE_ID;
            this.ServerLastModified = new DateTimeSr();
            this.ServerCreated = new DateTimeSr();
        }

        public static LiveSportRadarLineContainer FromXmlStringLive(string sXmlString)
        {
            return LineSerializeHelper.StringToObject<LiveSportRadarLineContainer>(sXmlString);
        }

        public static string ToXmlStringLive(LiveSportRadarLineContainer srlc)
        {
            return LineSerializeHelper.ObjectToString<LiveSportRadarLineContainer>(srlc);
        }

        public string ToXmlStringLive()
        {
            return LineSerializeHelper.ObjectToString<LiveSportRadarLineContainer>(this);
        }

        private static void ArrayToInfo(List<string> lInfo, string sName, object[] arr)
        {
            if (arr != null)
            {
                lInfo.Add(string.Format("{0}:{1}", sName, arr.Length));
            }
        }

        public override string ToString()
        {
            List<string> lInfo = new List<string>();

            ArrayToInfo(lInfo, "Matches", this.Match);
            ArrayToInfo(lInfo, "BetDomains", this.BetDomain);
            ArrayToInfo(lInfo, "Odds", this.Odd);
            ArrayToInfo(lInfo, "Competitors", this.Competitor);
            ArrayToInfo(lInfo, "MultiStrings", this.MultiString);
            ArrayToInfo(lInfo, "LangaugeStrings", this.LanguageString);

            string sDescription = string.Join(", ", lInfo.ToArray());

            return string.Format("Live {0} {{sid='{1}', mid={2}, st={3}, '{4}', Duration='{5}'}}", this.IsInitialLoad ? "Initial" : "Delta", this.SessionId, this.MessageId, this.ServerLastModified, sDescription, this.Duration);
        }
    }

    public class TraceDictionary : SyncDictionary<Type, List<string>>
    {
        public bool IsSomeChildWithContent { get { return this.Count > 0; } }

        public override void Clear()
        {
            lock (m_oLocker)
            {
                foreach (List<string> l in m_di.Values)
                {
                    l.Clear();
                }

                base.Clear();
            }
        }

        private delegate string DelegateFormatString<T>(T obj);

        private void FillFromArray<T>(T[] array, DelegateFormatString<T> dfs)
        {
            Type type = typeof(T);

            if (!SportRadarLineContainer.IsObjectArrayEmpty<T>(array))
            {
                List<string> l = new List<string>();

                foreach (T obj in array)
                {
                    string sData = dfs(obj);
                    l.Add(sData);
                }

                lock (m_oLocker)
                {
                    m_di[type] = l;
                }
            }
            else if (this.ContainsKey(type))
            {
                Debug.Assert(false);
            }
        }

        public void FillFromContainer(SportRadarLineContainer srlc)
        {
            this.Clear();

            FillFromArray<MatchSr>(srlc.Match, delegate(MatchSr mtch) { return string.Format("{0}:{1}:{2}", mtch.MatchID, mtch.BtrMatchID, mtch.LiveBetStatusEx); });
            FillFromArray<TournamentSr>(srlc.Tournament, delegate(TournamentSr trmt) { return string.Format("{0}:'{1}'", trmt.TournamentID, trmt.DefaultName); });
            FillFromArray<CategorySr>(srlc.Category, delegate(CategorySr ctrg) { return string.Format("{0}:'{1}'", ctrg.CategoryID, ctrg.DefaultName); });
            FillFromArray<SportSr>(srlc.Sport, delegate(SportSr sprt) { return string.Format("{0}:'{1}'", sprt.SportID, sprt.DefaultName); });
            FillFromArray<CountrySr>(srlc.Country, delegate(CountrySr cntr) { return string.Format("{0}:'{1}'", cntr.CountryID, cntr.DefaultName); });
            FillFromArray<BetDomainSr>(srlc.BetDomain, delegate(BetDomainSr bdmn) { return string.Format("{0}:{1}:{2}", bdmn.BetDomainID, bdmn.BtrLiveBetID, BetDomainSr.GetStatusString(bdmn.Status)); });
            FillFromArray<OddSr>(srlc.Odd, delegate(OddSr bodd) { return string.Format("{0}:{1}:{2}", bodd.OddID, bodd.Value, bodd.Active.Value ? "1" : "0"); });
            FillFromArray<CompetitorSr>(srlc.Competitor, delegate(CompetitorSr cmpt) { return string.Format("{0}:'{1}':{2}", cmpt.CompetitorID, cmpt.DefaultName, cmpt.MultiStringID); });
            FillFromArray<LanguageStringSr>(srlc.LanguageString, delegate(LanguageStringSr lstr) { return string.Format("{0}:{1}", lstr.LanguageStringID, lstr.MultiStringID); });
            FillFromArray<MultiStringSr>(srlc.MultiString, delegate(MultiStringSr mstr) { return string.Format("{0}", mstr.MultiStringID); });
            FillFromArray<LanguageSr>(srlc.Language, delegate(LanguageSr lng) { return string.Format("{0}:'{1}':{2}", lng.LanguageID, lng.ShortName, lng.IsTerminalLanguage); });
            FillFromArray<MultiStringGroupSr>(srlc.MultiStringGroup, delegate(MultiStringGroupSr msgp) { return string.Format("{0}:'{1}'", msgp.MultiStringGroupID, msgp.MultiStringGroupTag); });
        }

        public string GetContentMessage(string sInfoFormat, params object[] args)
        {
            string sInfo = string.Format(sInfoFormat, args) + "\r\n";

            foreach (Type type in this.Keys)
            {
                List<string> list = this[type];
                Debug.Assert(list != null && list.Count > 0);

                sInfo += string.Format("{0} (Count={1}): {2}\r\n", type.Name, list.Count, string.Join(", ", list.ToArray()));
            }

            return sInfo;
        }
    }
}