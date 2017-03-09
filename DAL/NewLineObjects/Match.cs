using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    /*
    public enum eMatchState
    {
        Unknown = 0,                //Unknown
        Created = 1,                //Imported from BetRadar
        CreationAccepted = 2,		//Admin user accepted this match and its odds
        Active = 3,				    //Bets can be placed on this match
        Canceled = 4,				//all Odds are set to 1 and won
        Open = 5,					//Match ended but not calculated yet
        Calculated = 6,			    //All tips concerning this match are calculated
        CalculationAccepted = 7,	//All tips concerning this match are calculated and accepted by Admin//Not used
        ResultEntered = 8,		    //match result is entered and save from Admin to database
        CalculationRunning = 9,	    //match calculation is running
        LivebetSelected = 13,       //manually selected, not registered yet
    }
    */

    public enum eOutrightType
    {
        None = 0,
        Outright = 1
    }

    public class MatchLn : ObjectBase, ILineObjectWithId<MatchLn>, IRemovableLineObject<MatchLn>, IMatchLn
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(MatchLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("Matches", false, "MatchId");
        protected SyncList<GroupLn> m_lParentGroups = null;

        protected LiveMatchInfoLn m_lmil = null;

        protected PositionToOutrightDictionary m_diOutrightCompetitors = null;

        public CompetitorLn HomeCompetitor { get; protected set; }
        public CompetitorLn AwayCompetitor { get; protected set; }

        public BetDomainList BetDomains { get; protected set; }

        protected MatchExternalState m_mes = null;

        public SyncList<GroupLn> ParentGroups
        {
            get
            {
                if (m_lParentGroups == null)
                {
                    m_lParentGroups = LineSr.Instance.AllObjects.MatchesToGroups.GetMatchGroups(this.MatchId);
                }

                return m_lParentGroups;
            }
        }

        public long MatchId { get; set; }
        public long BtrMatchId { get; set; }
        public long Sort { get; set; }
        public ObservableProperty<string> TeamWon { get; set; }
        public ObservableProperty<DateTimeSr> StartDate { get; set; }
        public ObservableProperty<DateTimeSr> ExpiryDate { get; set; }
        public ObservableProperty<DateTimeSr> EndDate { get; set; }
        public ObservableProperty<long> HomeCompetitorId { get; set; }
        public ObservableProperty<long> AwayCompetitorId { get; set; }
        public ObservableProperty<int> Code { get; set; }
        public ObservableProperty<bool> Active { get; set; }
        public ObservableProperty<bool> IsLiveBet { get; set; }
        public ObservableProperty<int> CardsTeam1 { get; set; }
        public ObservableProperty<int> CardsTeam2 { get; set; }
        public eServerSourceType SourceType { get; set; }
        public eOutrightType outright_type { get; set; }
        public ObservableProperty<string> NameTag { get; set; }
        public ObservableProperty<string> ExtendedState { get; set; }

        public ObservableProperty<int> ChangedCount { get; set; }

        protected IBetDomainLn m_bdmnSelected = null;

        protected static MatchLn m_EmptyMatch = null;

        public MatchLn()
            : base(false)
        {
            this.BetDomains = new BetDomainList(this);

            this.StartDate = new ObservableProperty<DateTimeSr>(this, m_lChangedProps, "StartDate");
            this.ExpiryDate = new ObservableProperty<DateTimeSr>(this, m_lChangedProps, "ExpiryDate");
            this.EndDate = new ObservableProperty<DateTimeSr>(this, m_lChangedProps, "EndDate");
            this.HomeCompetitorId = new ObservableProperty<long>(this, m_lChangedProps, "HomeCompetitorId ");
            this.AwayCompetitorId = new ObservableProperty<long>(this, m_lChangedProps, "AwayCompetitorId");
            this.Code = new ObservableProperty<int>(this, m_lChangedProps, "Code");
            this.Active = new ObservableProperty<bool>(this, m_lChangedProps, "Active");
            this.IsLiveBet = new ObservableProperty<bool>(this, m_lChangedProps, "IsLiveBet");
            this.NameTag = new ObservableProperty<string>(this, m_lChangedProps, "NameTag");
            this.ExtendedState = new ObservableProperty<string>(this, m_lChangedProps, "ExtendedState");
            this.ChangedCount = new ObservableProperty<int>(this, m_lChangedProps, "ChangedCount");
            this.CardsTeam1 = new ObservableProperty<int>(this, m_lChangedProps, "CardsTeam1");
            this.CardsTeam2 = new ObservableProperty<int>(this, m_lChangedProps, "CardsTeam2");
            this.TeamWon = new ObservableProperty<string>(this, m_lChangedProps, "TeamWon");
        }

        public PositionToOutrightDictionary OutrightCompetitors { get { return m_diOutrightCompetitors; } }

        public string GetOutrightDisplayName(string sLanguage)
        {
            if (this.outright_type != eOutrightType.None)
            {
                string sTranslation = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(this.NameTag.Value, sLanguage);

                return !string.IsNullOrEmpty(sTranslation) ? sTranslation : this.NameTag.Value;
            }

            return string.Empty;
        }

        public MatchExternalState MatchExternalState
        {
            get
            {
                if (m_mes == null)
                {
                    m_mes = new MatchExternalState();
                }

                return m_mes;
            }
        }

        public IBetDomainLn SelectedBetDomain { get { return m_bdmnSelected; } }
        public bool IsSelected { get { return m_bdmnSelected != null; } }

        public void SetSelected(IOddLn oddToDo, bool bIsSelected)
        {
            IBetDomainLn bdmnSelected = m_bdmnSelected;

            if (bIsSelected)
            {
                // Check if operation is valid
                if (LineSr.AllowMultiway)
                {
                    ExcpHelper.ThrowIf<InvalidOperationException>(bdmnSelected != null && bdmnSelected.BetDomainId != oddToDo.BetDomain.BetDomainId, "Cannot select {0} because other betdomain already selected {1}", this, bdmnSelected);
                }
                else
                {
                    IOddLn oddSelected = null;

                    if (bdmnSelected != null)
                    {
                        SyncList<IOddLn> lSelectedOdds = bdmnSelected.GetSelectedOdds();

                        if (lSelectedOdds.Count > 0)
                        {
                            oddSelected = lSelectedOdds[0];
                        }
                    }

                    ExcpHelper.ThrowIf<InvalidOperationException>(bdmnSelected != null, "Cannot select {0} because other odd already selected {1}", this, oddSelected);
                }
            }

            oddToDo.BetDomain.SetSelected(oddToDo, bIsSelected);

            m_bdmnSelected = oddToDo.BetDomain.GetSelectedOdds().Count > 0 ? oddToDo.BetDomain : null;

            if (bdmnSelected != m_bdmnSelected)
            {
                this.MatchView.DoPropertyChanged("IsSelected");
            }

            var lMatchBetDomains = this.GetSortedBetDomains();

            foreach (var bdmn in lMatchBetDomains)
            {
                bdmn.NotifyOddsEnabledChanged();
            }
        }

        private void EnsureExternalObjects()
        {
            m_mes = !string.IsNullOrEmpty(this.ExtendedState.Value) ? LineSerializeHelper.StringToObject<MatchExternalState>(this.ExtendedState.Value) : new MatchExternalState();
        }

        private void EnsureExternalState()
        {
            this.ExtendedState.Value = LineSerializeHelper.ObjectToString<MatchExternalState>(this.MatchExternalState);
        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.MatchId = DbConvert.ToInt64(dr, "MatchId");
            this.BtrMatchId = DbConvert.ToInt64(dr, "BtrMatchId");
            this.StartDate.Value = DbConvert.ToDateTimeSr(dr, "StartDate");
            this.ExpiryDate.Value = DbConvert.ToDateTimeSr(dr, "ExpiryDate");
            this.EndDate.Value = DbConvert.ToDateTimeSr(dr, "EndDate");
            this.HomeCompetitorId.Value = DbConvert.ToInt64(dr, "HomeCompetitorId");
            this.AwayCompetitorId.Value = DbConvert.ToInt64(dr, "AwayCompetitorId");
            this.UpdateId = DbConvert.ToInt64(dr, "UpdateID");
            this.Code.Value = DbConvert.ToInt32(dr, "Code");
            this.Active.Value = DbConvert.ToBool(dr, "Active");
            this.IsLiveBet.Value = DbConvert.ToBool(dr, "IsLiveBet");
            this.SourceType = (eServerSourceType)DbConvert.ToInt32(dr, "SourceType");
            this.outright_type = (eOutrightType)DbConvert.ToInt32(dr, "outright_type");
            this.NameTag.Value = DbConvert.ToString(dr, "NameTag");
            this.TeamWon.Value = DbConvert.ToString(dr, "TeamWon");
            this.ExtendedState.Value = DbConvert.ToString(dr, "ExtendedState");

            EnsureExternalObjects();
        }

        public static MatchLn EmptyMatch
        {
            get
            {
                if (m_EmptyMatch == null)
                {
                    m_EmptyMatch = new MatchLn();

                    m_EmptyMatch.MatchId = 0;
                    m_EmptyMatch.BtrMatchId = 0;
                    m_EmptyMatch.StartDate.Value = new DateTimeSr(DateTime.Now);
                    m_EmptyMatch.ExpiryDate.Value = new DateTimeSr(DateTime.Now);
                    m_EmptyMatch.EndDate.Value = new DateTimeSr(DateTime.Now);
                    m_EmptyMatch.HomeCompetitorId.Value = 0;
                    m_EmptyMatch.AwayCompetitorId.Value = 0;
                    m_EmptyMatch.UpdateId = 0;
                    m_EmptyMatch.Code.Value = 0;
                    m_EmptyMatch.TeamWon.Value = "";
                    m_EmptyMatch.Active.Value = false;
                    m_EmptyMatch.IsLiveBet.Value = false;
                    m_EmptyMatch.SourceType = eServerSourceType.BtrPre;
                    m_EmptyMatch.outright_type = eOutrightType.None;
                    m_EmptyMatch.ExtendedState.Value = string.Empty;
                }

                return m_EmptyMatch;
            }
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            EnsureExternalState();

            dr["MatchId"] = this.MatchId;
            dr["BtrMatchId"] = this.BtrMatchId;
            dr["StartDate"] = this.StartDate.Value.LocalDateTime;
            var dateTimeSr = this.ExpiryDate.Value;
            if (dateTimeSr != null)
            {
                dr["ExpiryDate"] = dateTimeSr.LocalDateTime;
            }
            dr["EndDate"] = this.EndDate.Value.LocalDateTime;
            dr["HomeCompetitorId"] = this.HomeCompetitorId.Value;
            dr["AwayCompetitorId"] = this.AwayCompetitorId.Value;
            dr["UpdateID"] = this.UpdateId;
            dr["Code"] = this.Code.Value;
            dr["Active"] = this.Active.Value;
            dr["IsLiveBet"] = this.IsLiveBet.Value;
            dr["SourceType"] = (int)this.SourceType;
            dr["outright_type"] = (int)this.outright_type;
            dr["NameTag"] = this.NameTag.Value;
            dr["TeamWon"] = this.TeamWon.Value;
            dr["ExtendedState"] = this.ExtendedState.Value;

            return dr;
        }

        public long Id { get { return this.MatchId; } }
        public long RemoveId { get { return this.MatchId; } }

        public bool IsNew
        {
            get { return DatabaseCache.Instance != null && !DatabaseCache.Instance.AllObjects.Matches.ContainsKey(this.MatchId); }
        }

        public void MergeFrom(MatchLn objSource)
        {
            Debug.Assert(this.MatchId == objSource.MatchId);
            Debug.Assert(this.BtrMatchId == objSource.BtrMatchId);
            Debug.Assert(this.SourceType == objSource.SourceType);

            objSource.EnsureExternalState();

            this.StartDate.Value = objSource.StartDate.Value;
            this.ExpiryDate.Value = objSource.ExpiryDate.Value;
            this.EndDate.Value = objSource.EndDate.Value;
            this.HomeCompetitorId.Value = objSource.HomeCompetitorId.Value;
            this.AwayCompetitorId.Value = objSource.AwayCompetitorId.Value;
            this.Code.Value = objSource.Code.Value;
            this.Active.Value = objSource.Active.Value;
            this.IsLiveBet.Value = objSource.IsLiveBet.Value;
            this.ExtendedState.Value = objSource.ExtendedState.Value;
            this.TeamWon.Value = objSource.TeamWon.Value;

            if (this.ExtendedState.Value != null)
            {
                this.EnsureExternalObjects();
            }

            var cardsTeam1 = this.MatchExternalState.CardsTeam1;
            if (cardsTeam1 > 0)
                this.CardsTeam1.Value = cardsTeam1;
            var cardsTeam2 = this.MatchExternalState.CardsTeam2;
            if (cardsTeam2 > 0)
                this.CardsTeam2.Value = cardsTeam2;


            SetRelations();
        }

        public override void SetRelations()
        {
            HashSet<string> hsPropertyNames = this.ChangedProps.GetPropertyNames();

            if (this.outright_type == eOutrightType.None)
            {
                if (this.HomeCompetitor == null || hsPropertyNames.Contains("HomeCompetitorId"))
                {
                    this.HomeCompetitor = LineSr.Instance.AllObjects.Competitors.GetObject(this.HomeCompetitorId.Value);
                    ExcpHelper.ThrowIf(this.HomeCompetitor == null, "MatchLn.SetRelations() ERROR. Cannot get Home Competitor for {0}", this);
                }

                if (this.AwayCompetitor == null || hsPropertyNames.Contains("AwayCompetitorId"))
                {
                    this.AwayCompetitor = LineSr.Instance.AllObjects.Competitors.GetObject(this.AwayCompetitorId.Value);
                    ExcpHelper.ThrowIf(this.AwayCompetitor == null, "MatchLn.SetRelations() ERROR. Cannot get Away Competitor for {0}", this);
                }
            }
            else if (m_diOutrightCompetitors == null)
            {
                m_diOutrightCompetitors = LineSr.Instance.AllObjects.CompetitorsToOutright.GetPositionToOutrightDictionaryByMatchId(this.MatchId);
                ExcpHelper.ThrowIf(m_diOutrightCompetitors == null, "MatchLn.SetRelations() ERROR. Cannot get Outright Competitors for {0}", this);
            }

            if (hsPropertyNames.Contains("Active"))
            {
                this.SetActiveChanged();
            }
        }

        public void SetActiveChanged()
        {
            this.ChangedCount.Value++;
            LineSr.Instance.NewOrChangedObjects.SafelyAddObject(this);

            var lBetDomains = this.BetDomains.Clone();

            foreach (var bdmn in lBetDomains)
            {
                bdmn.SetActiveChanged();
            }
        }

        public void NotifyBetDomainsEnabledChanged()
        {
            var lBetDomains = this.BetDomains.Clone();

            foreach (var bdmn in lBetDomains)
            {
                if (bdmn.DoesViewObjectExist)
                {
                    bdmn.BetDomainView.DoPropertyChanged("IsEnabled");
                }

                bdmn.NotifyOddsEnabledChanged();
            }
        }

        public override ISerializableObject Serialize()
        {
            EnsureExternalState();

            dynamic so = new SerializableObject(this.GetType());

            so.MatchId = this.MatchId;
            so.BtrMatchId = this.BtrMatchId;
            so.StartDate = this.StartDate.Value;
            so.ExpiryDate = this.ExpiryDate.Value;
            so.EndDate = this.EndDate.Value;
            so.HomeCompetitorId = this.HomeCompetitorId.Value;
            so.AwayCompetitorId = this.AwayCompetitorId.Value;
            so.Code = this.Code.Value;
            so.Active = this.Active.Value;
            so.IsLiveBet = this.IsLiveBet.Value;
            so.SourceType = this.SourceType;
            so.ExtendedState = this.ExtendedState.Value;
            so.OutrightType = this.outright_type;

            return so;
        }

        public override void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;
                

            this.MatchId = dso.MatchId.Value;
            this.BtrMatchId = dso.BtrMatchId.Value;
            this.StartDate.Value = dso.StartDate.Value;
            this.ExpiryDate.Value = dso.ExpiryDate.Value;
            this.EndDate.Value = dso.EndDate.Value;
            this.HomeCompetitorId.Value = dso.HomeCompetitorId.Value;
            this.AwayCompetitorId.Value = dso.AwayCompetitorId.Value;
            this.Code.Value = dso.Code.Value;
            this.Active.Value = dso.Active.Value;
            this.IsLiveBet.Value = dso.IsLiveBet.Value;
            this.NameTag.Value = dso.NameTag.Value;
            this.CardsTeam1.Value = dso.CardsTeam1.Value;
            this.CardsTeam2.Value = dso.CardsTeam2.Value;
            this.TeamWon.Value = dso.TeamWon.Value;
            this.VhcChannelId = dso.VhcChannelId.Value;

            eServerSourceType sst = this.IsLiveBet.Value ? eServerSourceType.BtrLive : eServerSourceType.BtrPre;
            SerializableProperty sp = so.GetSerializableProperty("SourceType");
            this.SourceType = sp.IsSpecified ? (eServerSourceType)sp.PropertyValue : sst;
            SerializableProperty outrightType = so.GetSerializableProperty("outright_type");
            this.outright_type = outrightType.IsSpecified ? (eOutrightType)outrightType.PropertyValue : eOutrightType.None;
            if (this.outright_type == eOutrightType.Outright)
                Debug.Assert(HomeCompetitorId.Value == 0 && this.outright_type == eOutrightType.Outright);
            if (this.outright_type == eOutrightType.None)
                Debug.Assert(HomeCompetitorId.Value > 0 && this.outright_type == eOutrightType.None);
            this.ExtendedState.Value = dso.ExtendedState.Value;
                
            EnsureExternalObjects();
        }

        public long VhcChannelId { get; set; }

        public override string ToString()
        {
            string sLang = DalStationSettings.Instance.Language;

            string sHomeCompetitorName = this.HomeCompetitor != null ? this.HomeCompetitor.GetDisplayName(sLang) : string.Empty;
            string sAwayCompetitorName = this.AwayCompetitor != null ? this.AwayCompetitor.GetDisplayName(sLang) : string.Empty;

            string sCompetitors = string.Format("'{0}'-'{1}'", sHomeCompetitorName, sAwayCompetitorName);

            return string.Format("MatchLn {{MatchId={0}, BtrMatchId={1}, Code={2}, Competitors({3})={4}, IsLiveBet={5}, Active={6}, IsNew={7}}}", this.MatchId, this.BtrMatchId, this.Code.Value, sLang, sCompetitors, this.IsLiveBet.Value, this.Active.Value, this.IsNew);
        }

        public LiveMatchInfoLn LiveMatchInfo
        {
            get
            {
                if (this.IsLiveBet.Value && m_lmil == null)
                {
                    m_lmil = LineSr.Instance.AllObjects.LiveMatchInfos.GetObject(this.MatchId);
                }

                return m_lmil;
            }
        }

        // View
        public IMatchVw MatchView
        {
            get
            {
                if (m_objView == null)
                {
                    var mv = new MatchVw(this);
                    m_objView = mv;
                    mv.RefreshProps();
                    ((MatchVw)m_objView).RefreshProps();
                }

                return m_objView as MatchVw;
            }
        }

        public bool IsEnabled
        {
            get
            {
                if (this.IsLiveBet.Value)
                {
                    Debug.Assert(this.LiveMatchInfo != null);
                    return this.LiveMatchInfo.Status.Value == eMatchStatus.Started || this.LiveMatchInfo.Status.Value == eMatchStatus.NotStarted;
                }

                return this.Active.Value;
            }
        }

        public bool IsCashierEnabled
        {
            get
            {
                return this.IsEnabled;
            }
        }

        public bool IsCashierAllowed
        {
            get
            {
                IBetDomainLn bdmnSelected = this.SelectedBetDomain;

                if (bdmnSelected == null)
                {
                    return true;
                }

                if (LineSr.AllowMultiway)
                {
                    // DK - probably here we must add checking if already all odds are selected
                    return true;
                }

                return false;
            }
        }

        public IBetDomainLn GetBaseBetDomain()
        {
            IBetDomainLn bdmnWINFT = null;
            IBetDomainLn bdmnWINFTR = null;

            this.BetDomains.SafelyForEach(delegate(BetDomainLn bdmn)
            {
                //  System.Diagnostics.Debug.WriteLine(bdmn.BetTag.Value + " " + bdmn.GetDisplayName(DalStationSettings.DEFAULT_LANGUAGE) + "  " + bdmn.BetDomainType);
                if (bdmn.BetTag == BetDomainTypeLn.BET_TAG_WINFT)
                {
                    bdmnWINFT = bdmn;
                }

                if (bdmn.BetTag == BetDomainTypeLn.BET_TAG_WINFTR)
                {
                    bdmnWINFTR = bdmn;
                }

                return false;
            });
            //System.Diagnostics.Debug.WriteLine("**********************");
            return bdmnWINFT != null ? bdmnWINFT : bdmnWINFTR;
        }

        public SyncList<BetDomainLn> GetSortedBetDomains()
        {
            var lBetDomains = this.BetDomains.Clone();

            lBetDomains.Sort(delegate(BetDomainLn bdmn1, BetDomainLn bdmn2) { return bdmn1.Sort.Value.CompareTo(bdmn2.Sort.Value); });

            return lBetDomains;
        }

        public static string GetBtrPreLiveKeyName(long lBtrMatchId, bool bIsLiveBet)
        {
            return string.Format("{0}{1}{2}", lBtrMatchId, ObjectBase.KEY_SEPARATOR, bIsLiveBet ? "1" : "0");
        }

        public string BtrPreLiveKeyName { get { return GetBtrPreLiveKeyName(this.BtrMatchId, this.IsLiveBet.Value); } }

       
    }

    [XmlRoot("state", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = true)]
    public class MatchExternalState
    {
        [XmlElement(ElementName = "card1")]
        public int CardsTeam1 { get; set; }
        [XmlElement(ElementName = "card2")]
        public int CardsTeam2 { get; set; }
        [XmlElement(ElementName = "mincomb")]
        public int? MinCombination { get; set; }
        [XmlElement(ElementName = "vseason")]
        public int? VirtualSeason { get; set; }
        [XmlElement(ElementName = "vday")]
        public int? VirtualDay { get; set; }

        [XmlIgnore]
        public bool CardsTeam1Specified { get { return this.CardsTeam1 != null; } }
        [XmlIgnore]
        public bool CardsTeam2Specified { get { return this.CardsTeam2 != null; } }
        [XmlIgnore]
        public bool MinCombinationSpecified { get { return this.MinCombination != null; } }
        [XmlIgnore]
        public bool VirtualSeasonSpecified { get { return this.VirtualSeason != null; } }
        [XmlIgnore]
        public bool VirtualDaySpecified { get { return this.VirtualDay != null; } }

        public override int GetHashCode()
        {
            return this.MinCombination != null ? this.MinCombination.GetHashCode() : 0;
        }

        public override bool Equals(object obj)
        {
            MatchExternalState mes = obj as MatchExternalState;

            return mes != null && mes.GetHashCode() == this.GetHashCode();
        }
    }

    public class BetDomainList : SyncList<BetDomainLn>
    {
        protected MatchLn m_match = null;

        public BetDomainList(MatchLn mtchOwner)
            : base()
        {
            Debug.Assert(mtchOwner != null);
            m_match = mtchOwner;
        }

        public override void Add(BetDomainLn item)
        {
            lock (m_oLocker)
            {
                m_list.Add(item);
                NotifyParent();
            }
        }

        public override bool SafelyAdd(BetDomainLn item)
        {
            lock (m_oLocker)
            {
                if (!m_list.Contains(item))
                {
                    m_list.Add(item);
                    NotifyParent();

                    return true;
                }
            }

            return false;
        }

        public override bool Remove(BetDomainLn item)
        {
            lock (m_oLocker)
            {
                bool bResult = m_list.Remove(item);
                NotifyParent();

                return bResult;
            }
        }

        protected void NotifyParent()
        {
            m_match.SetActiveChanged();
        }
    }

    public class PreLiveMatchDictionary : SyncDictionary<string, MatchLn>
    {
    }

    public class MatchDictionary : LineObjectDictionaryByIdBase<MatchLn>
    {
        protected PreLiveMatchDictionary m_diBtrPreLive = new PreLiveMatchDictionary();

        public MatchLn GetByBtrMatchId(long lBtrMatchId, bool bIsLiveBet)
        {
            return m_diBtrPreLive.SafelyGetValue(MatchLn.GetBtrPreLiveKeyName(lBtrMatchId, bIsLiveBet));
        }

        public override MatchLn MergeLineObject(MatchLn objSource)
        {
            lock (m_oLocker)
            {
                var match = base.MergeLineObjectImp(objSource);

                m_diBtrPreLive.SafelyAdd(match.BtrPreLiveKeyName, match);
                // Debug.Assert(m_di.Count == m_diBtrPreLive.Count);

                return match;
            }
        }

        public override bool Remove(long lKey)
        {
            lock (m_oLocker)
            {
                if (m_di.ContainsKey(lKey))
                {
                    var match = m_di[lKey];

                    m_diBtrPreLive.Remove(match.BtrPreLiveKeyName);
                    return m_di.Remove(lKey);
                }
            }

            return false;
        }
    }
}
