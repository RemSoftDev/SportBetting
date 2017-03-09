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
    public enum eBetDomainStatus
    {
        Visible = 0,
        Hidden = 1,
        Forbidden = 2,
        Inactive = 3,
        Entered = 8,
        Calculated = 7
    }

    public class BetDomainLn : ObjectBase, ILineObjectWithId<BetDomainLn>, IRemovableLineObject<BetDomainLn>, IBetDomainLn
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(BetDomainLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("BetDomain", false, "BetDomainId");

        public long BetDomainId { get; set; }
        public long MatchId { get; set; }
        public ObservableProperty<long> BtrLiveBetId { get; set; }
        public ObservableProperty<eBetDomainStatus> Status { get; set; }
        public string BetTag { get; set; }
        public ObservableProperty<int> BetDomainNumber { get; set; }
        public string NameTag { get; set; }
        public ObservableProperty<int> Sort { get; set; }
        public ObservableProperty<bool> IsLiveBet { get; set; }
        public ObservableProperty<bool> IsManuallyDisabled { get; set; }
        public ObservableProperty<string> SpecialOddValue { get; set; }
        public ObservableProperty<string> SpecialOddValueFull { get; set; }
        public ObservableProperty<string> Result { get; set; }
        public string ExtendedState { get; set; }

        public ObservableProperty<int> ChangedCount { get; set; }

        public BetDomainTypeLn BetDomainType { get; protected set; }

        public OddList Odds { get; protected set; }

        public SyncDictionary<int, OddLn> SortedOdds { get; protected set; }

        public IMatchLn Match { get; protected set; }

        protected static BetDomainLn m_EmptyBetDomain = null;

        private SyncList<IOddLn> m_lSelectedOdds = new SyncList<IOddLn>();

        protected BetDomainExternalState m_bdes = null;

        public BetDomainLn()
            : base(false)
        {
            this.Odds = new OddList(this);

            this.BtrLiveBetId = new ObservableProperty<long>(this, m_lChangedProps, "BtrLiveBetId");
            this.Status = new ObservableProperty<eBetDomainStatus>(this, m_lChangedProps, "Status");
            this.BetDomainNumber = new ObservableProperty<int>(this, m_lChangedProps, "BetDomainNumber");
            this.Sort = new ObservableProperty<int>(this, m_lChangedProps, "Sort");
            this.IsLiveBet = new ObservableProperty<bool>(this, m_lChangedProps, "IsLiveBet");
            this.SpecialOddValue = new ObservableProperty<string>(this, m_lChangedProps, "SpecialOddValue");
            this.SpecialOddValueFull = new ObservableProperty<string>(this, m_lChangedProps, "SpecialOddValueFull");
            this.Result = new ObservableProperty<string>(this, m_lChangedProps, "Result");
            this.ChangedCount = new ObservableProperty<int>(this, m_lChangedProps, "ChangedCount");
            this.IsManuallyDisabled = new ObservableProperty<bool>(this, m_lChangedProps, "IsManuallyDisabled");
        }

        public BetDomainExternalState BetDomainExternalState
        {
            get
            {
                if (m_bdes == null)
                {
                    m_bdes = new BetDomainExternalState();
                }

                return m_bdes;
            }
        }

        public SyncList<IOddLn> GetSelectedOdds() { return m_lSelectedOdds.Clone(); }
        public bool IsSelected { get { return m_lSelectedOdds.Count > 0; } }

        public void SetSelected(IOddLn odd, bool bSelected)
        {
            bool bIsSelected = this.IsSelected;
            odd.SetSelected(bSelected);
            odd.OddView.DoPropertyChanged("IsEnabled");

            if (bSelected)
            {
                m_lSelectedOdds.SafelyAdd(odd);
            }
            else
            {
                m_lSelectedOdds.Remove(odd);
            }

            if (this.IsSelected != bIsSelected)
            {
                if (m_objView != null)
                {
                    m_objView.DoPropertyChanged("IsSelected");
                }
            }
        }

        public static BetDomainLn EmptyBetDomain
        {
            get
            {
                if (m_EmptyBetDomain == null)
                {
                    m_EmptyBetDomain = new BetDomainLn();

                    m_EmptyBetDomain.BetDomainId = 0;
                    m_EmptyBetDomain.BtrLiveBetId.Value = 0;
                    m_EmptyBetDomain.MatchId = 0;
                    m_EmptyBetDomain.BetDomainType = BetDomainTypeLn.EmptyBetDomainType;

                    m_EmptyBetDomain.UpdateId = 0;

                    m_EmptyBetDomain.Status.Value = eBetDomainStatus.Hidden;
                    m_EmptyBetDomain.BetTag = string.Empty;
                    m_EmptyBetDomain.BetDomainNumber.Value = 0;
                    m_EmptyBetDomain.NameTag = string.Empty;
                    m_EmptyBetDomain.Sort.Value = 0;
                    m_EmptyBetDomain.IsLiveBet.Value = false;
                    m_EmptyBetDomain.SpecialOddValue.Value = string.Empty;
                    m_EmptyBetDomain.SpecialOddValueFull.Value = string.Empty;
                    m_EmptyBetDomain.Result.Value = string.Empty;
                    m_EmptyBetDomain.ExtendedState = string.Empty;

                    m_EmptyBetDomain.Match = MatchLn.EmptyMatch;
                }

                return m_EmptyBetDomain;
            }
        }
        SyncDictionary<int, OddLn> diSortedOdds = new SyncDictionary<int, OddLn>();

        public SyncDictionary<int, OddLn> GetSortedOdds()
        {

            var lOdds = this.Odds.Clone();

            int iIncrement = lOdds.Count > 0 && lOdds[0].Sort.Value == 0 ? 1 : 0;

            foreach (var odd in lOdds)
            {
                try
                {
                   // var oddExisting = diSortedOdds.SafelyGetValue(odd.Sort.Value + iIncrement);

                    //ExcpHelper.ThrowIf(oddExisting != null, "Two odds have the same sort value for BetDomain {0}:\r\n{1}\r\n{2})", this, oddExisting, odd);
                    diSortedOdds[odd.Sort.Value + iIncrement] = odd;
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "GetSortedOdds() ERROR.");
                    throw;
                }
            }

            return diSortedOdds;
        }

        public static eBetDomainStatus IntToBetDomainStatus(int iValue)
        {
            switch (iValue)
            {
                case BetDomainSr.STATUS_VISIBLE: return eBetDomainStatus.Visible;
                case BetDomainSr.STATUS_HIDDEN: return eBetDomainStatus.Hidden;
                case BetDomainSr.STATUS_INACTIVE: return eBetDomainStatus.Inactive;

                // DK - We don't need here default as usual
            }

            return eBetDomainStatus.Forbidden;
        }

        public string GetDisplayName(string sLanguage)
        {
            string sTranslation = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(this.NameTag, sLanguage);
            if (string.IsNullOrEmpty(sTranslation))
                sTranslation = this.NameTag;
            if (this.BetDomainType != null && this.BetDomainType.TranslationArgsCount > 0)
            {
                sTranslation = this.BetDomainType.FormatTranslation(sTranslation, this.SpecialOddValue.Value, this.SpecialOddValueFull.Value);
            }

            return String.IsNullOrEmpty(sTranslation) ? this.NameTag : sTranslation;
            //return sTranslation;
        }

        public override int GetHashCode()
        {
            return this.BetDomainId.GetHashCode();
        }

        private void EnsureExternalObjects()
        {
            m_bdes = !string.IsNullOrEmpty(this.ExtendedState) ? LineSerializeHelper.StringToObject<BetDomainExternalState>(this.ExtendedState) : new BetDomainExternalState();
        }

        private void EnsureExternalState()
        {
            this.ExtendedState = LineSerializeHelper.ObjectToString<BetDomainExternalState>(this.BetDomainExternalState);
        }

        public override void FillFromDataRow(DataRow dr)
        {
            this.BetDomainId = DbConvert.ToInt64(dr, "BetDomainId");
            this.BtrLiveBetId.Value = DbConvert.ToInt64(dr, "BtrLiveBetId");
            this.MatchId = DbConvert.ToInt64(dr, "MatchId");

            this.UpdateId = DbConvert.ToInt64(dr, "UpdateId");

            string sStatus = DbConvert.ToString(dr, "Status");

            this.Status.Value = (eBetDomainStatus)Enum.Parse(typeof(eBetDomainStatus), sStatus, true);
            this.BetTag = DbConvert.ToString(dr, "BetTag");
            this.BetDomainNumber.Value = DbConvert.ToInt32(dr, "BetDomainNumber");
            this.NameTag = DbConvert.ToString(dr, "NameTag");
            this.Sort.Value = DbConvert.ToInt32(dr, "Sort");
            this.IsLiveBet.Value = DbConvert.ToBool(dr, "IsLiveBet");
            this.SpecialOddValue.Value = DbConvert.ToString(dr, "SpecialOddValue");
            this.SpecialOddValueFull.Value = DbConvert.ToString(dr, "SpecialOddValueFull");
            this.Result.Value = DbConvert.ToString(dr, "Result");
            this.ExtendedState = DbConvert.ToString(dr, "ExtendedState");
            this.IsManuallyDisabled.Value = DbConvert.ToBool(dr, "IsManuallyDisabled");

            EnsureExternalObjects();
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            EnsureExternalState();

            dr["BetDomainId"] = this.BetDomainId;
            dr["BtrLiveBetId"] = this.BtrLiveBetId.Value;
            dr["MatchId"] = this.MatchId;
            dr["UpdateId"] = this.UpdateId;
            dr["Status"] = Status.Value.ToString();
            dr["BetTag"] = BetTag;
            dr["BetDomainNumber"] = BetDomainNumber.Value;
            dr["NameTag"] = NameTag;
            dr["Sort"] = Sort.Value;
            dr["IsLiveBet"] = IsLiveBet.Value;
            dr["SpecialOddValue"] = SpecialOddValue.Value;
            dr["SpecialOddValueFull"] = SpecialOddValueFull.Value;
            dr["Result"] = Result.Value;
            dr["IsManuallyDisabled"] = IsManuallyDisabled.Value;
            dr["ExtendedState"] = ExtendedState;

            return dr;
        }


        public long Id
        {
            get { return this.BetDomainId; }
        }

        public long RemoveId
        {
            get { return this.BetDomainId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.BetDomains.ContainsKey(this.BetDomainId); }
        }

        public void MergeFrom(BetDomainLn objSource)
        {
            Debug.Assert(this.BetDomainId == objSource.BetDomainId);
            Debug.Assert(this.MatchId == objSource.MatchId);

            objSource.EnsureExternalState();

            this.BtrLiveBetId.Value = objSource.BtrLiveBetId.Value;
            this.Status.Value = objSource.Status.Value;
            this.BetTag = objSource.BetTag;
            this.BetDomainNumber.Value = objSource.BetDomainNumber.Value;
            this.NameTag = objSource.NameTag;
            this.IsManuallyDisabled.Value = objSource.IsManuallyDisabled.Value;
            this.Sort.Value = objSource.Sort.Value;
            this.IsLiveBet.Value = objSource.IsLiveBet.Value;
            this.SpecialOddValue.Value = objSource.SpecialOddValue.Value;
            this.SpecialOddValueFull.Value = objSource.SpecialOddValueFull.Value;
            this.Result.Value = objSource.Result.Value;
            this.ExtendedState = objSource.ExtendedState;

#if DEBUG
            if (this.ChangedProps.Count > 0)
            {
                string sPropNames = GetChangedPropNames();
            }
#endif

            if (this.ExtendedState != null)
            {
                this.EnsureExternalObjects();
            }

            this.SetRelations();
        }

        public override void SetRelations()
        {
            try
            {
                if (this.BetTag != null  || this.BetDomainType == null)
                {
                    this.BetDomainType = LineSr.Instance.AllObjects.BetDomainTypes.GetObject(this.BetTag);

                    ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(this.BetDomainType == null, "Cannot find BetDomainType (BetTag='{0}') for {1}", this.BetTag, this);
                }

                if (this.Match == null)
                {
                    this.Match = LineSr.Instance.AllObjects.Matches.GetObject(this.MatchId);

                    ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(this.Match == null, "Cannot get parent Match (MatchId={0})", this.MatchId);
                }


                if (this.ChangedProps.Contains(this.Status) || this.ChangedProps.Contains(this.IsManuallyDisabled))
                {
                    //this.SetActiveChanged();
                    this.Match.SetActiveChanged();
                }

                this.Match.BetDomains.SafelyAdd(this);
            }
            catch (RelatedLineObjectNotFoundException excp)
            {
                m_logger.WarnFormat("BetDomainLn.SetRelations() Warning '{0}' for {1}", excp.Message, this);
            }
            catch (Exception excp)
            {
                ExcpHelper.ThrowUp(excp, "BetDomainLn.SetRelations() ERROR for {0}", this);
            }
        }

        public void SetActiveChanged()
        {
            this.ChangedCount.Value++;
        }

        public void NotifyOddsEnabledChanged()
        {
            SyncList<OddLn> lOdds = this.Odds.Clone();

            foreach (var bodd in lOdds)
            {
                if (bodd.DoesViewObjectExist)
                {
                    bodd.OddView.DoPropertyChanged("IsEnabled");
                }
            }
        }

        public override ISerializableObject Serialize()
        {
            EnsureExternalState();

            dynamic so = new SerializableObject(this.GetType());

            so.BetDomainId = this.BetDomainId;
            so.BtrLiveBetId = this.BtrLiveBetId.Value;
            so.MatchId = this.MatchId;
            so.Status = this.Status.Value;
            so.BetTag = this.BetTag;
            so.BetDomainNumber = this.BetDomainNumber.Value;
            so.NameTag = this.NameTag;
            so.Sort = this.Sort.Value;
            so.IsLiveBet = this.IsLiveBet.Value;
            so.SpecialOddValue = this.SpecialOddValue.Value;
            so.SpecialOddValueFull = this.SpecialOddValueFull.Value;
            so.Result = this.Result.Value;
            so.ExtendedState = this.ExtendedState;

            return so;
        }

        public override void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.BetDomainId = dso.BetDomainId.Value;
            this.BtrLiveBetId.Value = dso.BtrLiveBetId.Value;
            this.MatchId = dso.MatchId.Value;
            this.Status.Value = dso.Status.Value;
            this.BetTag = dso.BetTag.Value;
            this.BetDomainNumber.Value = dso.BetDomainNumber.Value;
            this.NameTag = dso.NameTag.Value;
            this.Sort.Value = dso.Sort.Value;
            this.IsLiveBet.Value = dso.IsLiveBet.Value;
            this.SpecialOddValue.Value = dso.SpecialOddValue.Value;
            this.SpecialOddValueFull.Value = dso.SpecialOddValueFull.Value;
            this.Result.Value = dso.Result.Value;
            this.ExtendedState = dso.ExtendedState.Value;

            EnsureExternalObjects();
        }

        public bool IsCashierEnabled
        {
            get
            {
                if (!this.Match.Active.Value)
                {
                    return false;
                }

                return this.Status.Value == eBetDomainStatus.Visible;
            }
        }

        public bool IsCashierAllowed
        {
            get
            {
                IBetDomainLn bdmnSelected = this.Match.SelectedBetDomain;

                if (bdmnSelected == null)
                {
                    return true;
                }

                if (bdmnSelected.BetDomainId == this.BetDomainId && LineSr.AllowMultiway)
                {
                    // DK - probably here we must add checking if already all odds are selected
                    return true;
                }

                return this.Status.Value == eBetDomainStatus.Visible;
            }
        }

        public override string ToString()
        {
            string sLang = DalStationSettings.Instance.Language;

            return string.Format("BetDomainLn {{BetDomainId={0}, MatchId={1}, Name({2})='{3}', Status={4}, {5}, IsLiveBet={6}, OddCount={7}, IsNew={8}, ChangedProps={9}}}",
                this.BetDomainId, this.MatchId, sLang, this.GetDisplayName(sLang), this.Status.Value, this.BetDomainType, this.IsLiveBet.Value, this.Odds.Count, this.IsNew, this.ChangedProps.Count);
        }

        // View
        public IBetDomainVw BetDomainView
        {
            get
            {
                if (m_objView == null)
                {
                    m_objView = new BetDomainVw(this);
                }

                return m_objView as BetDomainVw;
            }
        }
    }

    [XmlRoot("state", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = true)]
    public class BetDomainExternalState
    {
        [XmlElement(ElementName = "mincomb")]
        public int? MinCombination { get; set; }

        [XmlIgnore]
        public bool MinCombinationSpecified { get { return this.MinCombination != null; } }

        public override int GetHashCode()
        {
            return this.MinCombination != null ? this.MinCombination.GetHashCode() : 0;
        }

        public override bool Equals(object obj)
        {
            BetDomainExternalState bdes = obj as BetDomainExternalState;

            return bdes != null && bdes.GetHashCode() == this.GetHashCode();
        }
    }

    public class OddList : SyncList<OddLn>
    {
        protected BetDomainLn m_betDomain = null;

        public OddList(BetDomainLn bdmnOwner)
            : base()
        {
            Debug.Assert(bdmnOwner != null);
            m_betDomain = bdmnOwner;
        }

        public override void Add(OddLn item)
        {
            lock (m_oLocker)
            {
                m_list.Add(item);
                NotifyParent();
            }
        }

        public override bool SafelyAdd(OddLn item)
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

        public override bool Remove(OddLn item)
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
            m_betDomain.SetActiveChanged();
        }
    }

    public class BetDomainDictionary : LineObjectDictionaryByIdBase<BetDomainLn>
    {
    }
}
