using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public class OddLn : ObjectBase, ILineObjectWithId<OddLn>, IRemovableLineObject<OddLn>, IOddLn
    {
        public const decimal VALID_ODD_VALUE = 1.0m;

        private static ILog m_logger = LogFactory.CreateLog(typeof(OddLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("Odd", false, "OutcomeId");

        public long OutcomeId { get; set; }
        public ObservableProperty<long> OddId { get; set; }
        public long BetDomainId { get; set; }
        public ObservableProperty<decimal> Value { get; set; }
        public ObservableProperty<string> NameTag { get; set; }
        public ObservableProperty<string> OddTag { get; set; }
        public ObservableProperty<int> Sort { get; set; }
        public ObservableProperty<bool> Active { get; set; }
        public ObservableProperty<bool> IsLiveBet { get; set; }
        public ObservableProperty<string> ExtendedState { get; set; }
        public ObservableProperty<bool> IsManuallyDisabled { get; set; }

        // UI Properties
        public bool IsSelected { get; protected set; }

        public IBetDomainLn BetDomain { get; protected set; }

        protected static OddLn m_EmptyOdd = null;

        public OddLn()
            : base(false)
        {
            this.OddId = new ObservableProperty<long>(this, m_lChangedProps, "OddId");
            this.Value = new ObservableProperty<decimal>(this, m_lChangedProps, "Value");
            this.NameTag = new ObservableProperty<string>(this, m_lChangedProps, "NameTag");
            this.OddTag = new ObservableProperty<string>(this, m_lChangedProps, "OddTag");
            this.Sort = new ObservableProperty<int>(this, m_lChangedProps, "Sort");
            this.Active = new ObservableProperty<bool>(this, m_lChangedProps, "Active");
            this.IsLiveBet = new ObservableProperty<bool>(this, m_lChangedProps, "IsLiveBet");
            this.ExtendedState = new ObservableProperty<string>(this, m_lChangedProps, "ExtendedState");
            this.IsManuallyDisabled = new ObservableProperty<bool>(this, m_lChangedProps, "IsManuallyDisabled");

            this.IsSelected = false;
        }

        public static OddLn EmptyOdd
        {
            get
            {
                if (m_EmptyOdd == null)
                {
                    m_EmptyOdd = new OddLn();

                    m_EmptyOdd.OutcomeId = 0;
                    m_EmptyOdd.OddId.Value = 0;
                    m_EmptyOdd.BetDomainId = 0;
                    m_EmptyOdd.Value.Value = 0.0m;
                    m_EmptyOdd.NameTag.Value = string.Empty;
                    m_EmptyOdd.OddTag.Value = string.Empty;
                    m_EmptyOdd.UpdateId = 0;
                    m_EmptyOdd.Sort.Value = 0;
                    m_EmptyOdd.Active.Value = false;
                    m_EmptyOdd.IsLiveBet.Value = false;
                    m_EmptyOdd.ExtendedState.Value = string.Empty;
                    m_EmptyOdd.BetDomain = BetDomainLn.EmptyBetDomain;
                }

                return m_EmptyOdd;
            }
        }
        private ObjectStringDictionary m_diStrings = null;
        public ObjectStringDictionary Strings
        {
            get
            {
                if (m_diStrings == null)
                {
                    //Debug.Assert(this.OddId.Value != 0l);

                    m_diStrings = LineSr.Instance.AllObjects.TaggedStrings.GetOddStrings(this.OddId.Value);
                }

                return m_diStrings;
            }
        }
        public string GetDisplayName(string sLanguage)
        {

            TaggedStringLn tstr = this.Strings != null ? m_diStrings.SafelyGetString(sLanguage) : null;

            string sTranslation = LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(this.NameTag.Value, sLanguage);

            return tstr != null ? tstr.Text : sTranslation;
        }

        public override int GetHashCode()
        {
            return this.OddId.GetHashCode();
        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.OutcomeId = DbConvert.ToInt64(dr, "OutcomeId");
            this.OddId.Value = DbConvert.ToInt64(dr, "OddId");
            this.BetDomainId = DbConvert.ToInt64(dr, "BetDomainId");
            this.Value.Value = DbConvert.ToDecimal(dr, "Value");
            this.NameTag.Value = DbConvert.ToString(dr, "NameTag");
            this.OddTag.Value = DbConvert.ToString(dr, "OddTag");
            this.UpdateId = DbConvert.ToInt64(dr, "UpdateId");
            this.Sort.Value = DbConvert.ToInt32(dr, "Sort");
            this.Active.Value = DbConvert.ToBool(dr, "Active");
            this.IsLiveBet.Value = DbConvert.ToBool(dr, "IsLiveBet");
            this.ExtendedState.Value = DbConvert.ToString(dr, "ExtendedState");
            this.IsManuallyDisabled.Value = DbConvert.ToBool(dr, "IsManuallyDisabled");
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["OutcomeId"] = this.OutcomeId;
            dr["OddId"] = this.OddId.Value;
            dr["BetDomainId"] = this.BetDomainId;
            dr["Value"] = this.Value.Value;
            dr["NameTag"] = this.NameTag.Value;
            dr["OddTag"] = this.OddTag.Value;
            dr["UpdateId"] = this.UpdateId;
            dr["Sort"] = this.Sort.Value;
            dr["Active"] = this.Active.Value;
            dr["IsLiveBet"] = this.IsLiveBet.Value;
            dr["ExtendedState"] = this.ExtendedState.Value;
            dr["IsManuallyDisabled"] = this.IsManuallyDisabled.Value;

            return dr;
        }

        public long Id
        {
            get { return this.OutcomeId; }
        }

        public long RemoveId
        {
            get { return this.OutcomeId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.Odds.ContainsKey(this.OutcomeId); }
        }

        public void MergeFrom(OddLn objSource)
        {
            Debug.Assert(this.OutcomeId == objSource.OutcomeId);
            ExcpHelper.ThrowIf(objSource.BetDomainId != 0 && this.BetDomainId != objSource.BetDomainId, "MergeFrom(OddLn) ERROR. BetDomainIds are different ({0} != {1})\r\nSource = {2}\r\nTarget = {3}",
                objSource.BetDomainId, this.BetDomainId, objSource, this);

            //Debug.Assert(this.BetDomainId == objSource.BetDomainId);

            this.OddId.Value = objSource.OddId.Value;
            this.Value.Value = objSource.Value.Value;
            this.NameTag.Value = objSource.NameTag.Value;
            this.OddTag.Value = objSource.OddTag.Value;
            this.Sort.Value = objSource.Sort.Value;
            this.Active.Value = objSource.Active.Value;
            this.IsLiveBet.Value = objSource.IsLiveBet.Value;
            this.ExtendedState.Value = objSource.ExtendedState.Value;
            this.IsManuallyDisabled.Value = objSource.IsManuallyDisabled.Value;

            this.SetRelations();
        }

        public override void SetRelations()
        {
            if (this.BetDomain == null)
            {
                this.BetDomain = LineSr.Instance.AllObjects.BetDomains.GetObject(this.BetDomainId);

                ExcpHelper.ThrowIf<RelatedLineObjectNotFoundException>(this.BetDomain == null, "OddLn.Initialize() ERROR. Cannot get parent BetDomain for {0}", this);
            }

            if (this.ChangedProps.Contains(this.IsManuallyDisabled))
            {
                //this.SetActiveChanged();
                this.BetDomain.Match.SetActiveChanged();
            }

            this.BetDomain.Odds.SafelyAdd(this);
            this.BetDomain.Odds.Sort(delegate(OddLn o1, OddLn o2) { return o1.Sort.Value.CompareTo(o2.Sort.Value); });
        }

        // View
        public IOddVw OddView
        {
            get
            {
                if (m_objView == null)
                {
                    m_objView = new OddVw(this);
                }

                return m_objView as IOddVw;
            }
        }

        public void SetSelected(bool bIsSelected)
        {
            if (this.IsSelected != bIsSelected)
            {
                this.IsSelected = bIsSelected;

                if (m_objView != null)
                {
                    m_objView.DoPropertyChanged("IsSelected");
                }
            }
        }

        public bool IsLocked
        {
            get { return LineSr.Instance.LockedObjects.IsOddLocked(this); }
        }

        public bool IsCashierAllowed
        {
            get
            {
                if (this.IsSelected || this.IsLocked)
                {
                    return false;
                }

                IBetDomainLn bdmnSelected = this.BetDomain.Match.SelectedBetDomain;

                if (bdmnSelected == null)
                {
                    return true;
                }

                if (bdmnSelected == this.BetDomain && LineSr.AllowMultiway)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsCashierEnabled
        {
            get
            {
                if (!this.IsLocked)
                {
                    if (this.IsLiveBet.Value && !LineSr.LiveBetConnected)
                    {
                        return false;
                    }

                    if (this.BetDomain.Status.Value == eBetDomainStatus.Visible && this.Active.Value && this.BetDomain.Match.IsEnabled)
                    {
                        IBetDomainLn bdmnSelected = this.BetDomain.Match.SelectedBetDomain;

                        if (bdmnSelected == null)
                        {
                            return true;
                        }

                        if (bdmnSelected == this.BetDomain && LineSr.AllowMultiway)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool IsEnabled
        {
            get
            {
                Debug.Assert(this.BetDomain != null);

                if (this.IsSelected)
                {
                    return true;
                }

                return IsCashierEnabled;
            }
        }

        public override ISerializableObject Serialize()
        {
            dynamic so = new SerializableObject(this.GetType());

            so.OutcomeId = this.OutcomeId;
            so.OddId = this.OddId.Value;
            so.BetDomainId = this.BetDomainId;
            so.Value = this.Value.Value;
            so.NameTag = this.NameTag.Value;
            so.OddTag = this.OddTag.Value;
            so.UpdateId = this.UpdateId;
            so.Sort = this.Sort.Value;
            so.Active = this.Active.Value;
            so.IsLiveBet = this.IsLiveBet.Value;
            so.ExtendedState = this.ExtendedState.Value;

            return so;
        }

        public override void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.OutcomeId = dso.OutcomeId.Value;
            this.OddId.Value = dso.OddId.Value;
            this.BetDomainId = dso.BetDomainId.Value;
            this.Value.Value = dso.Value.Value;
            this.NameTag.Value = dso.NameTag.Value;
            this.OddTag.Value = dso.OddTag.Value;
            this.UpdateId = dso.UpdateId.Value;
            this.Sort.Value = dso.Sort.Value;
            this.Active.Value = dso.Active.Value;
            this.IsLiveBet.Value = dso.IsLiveBet.Value;
            this.ExtendedState.Value = dso.ExtendedState.Value;
        }

        public override string ToString()
        {
            return string.Format("OddLn {{OutcomeId={0}, OddId={1}, BetDomainId={2}, OddTag='{3}', NameTag='{4}', Value={5}, Sort={6}, Active={7}, IsLiveBet={8}, IsNew={9}}}",
                this.OutcomeId, this.OddId.Value, this.BetDomainId, this.OddTag.Value, this.NameTag.Value, this.Value.Value, this.Sort.Value, this.Active.Value, this.IsLiveBet.Value, this.IsNew);
        }
    }

    public class OddDictionary : LineObjectDictionaryByIdBase<OddLn>
    {
    }
}
