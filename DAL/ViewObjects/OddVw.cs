using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Linq;

namespace SportRadar.DAL.ViewObjects
{
    public class OddVw : ViewObjectBase<OddLn>, IOddVw
    {
        public const string PROPERTY_NAME_VALUE = "Value";
        public const string PROPERTY_NAME_IS_ENABLED = "IsSelected";
        public const string PROPERTY_NAME_IS_SELECTED = "IsEnabled";

        protected static SyncDictionary<string, List<string>> m_diLinePropsToViewProps = new SyncDictionary<string, List<string>>()
        {
            {"Active", new List<string>(){"Visibility", "IsEnabled", "VisibilityBetDomainView"}},
            {"IsManuallyDisabled", new List<string>(){"Visibility", "IsEnabled", "VisibilityBetDomainView"}},
            {"Value", new List<string>(){"DisplayValue"}},
        };

        protected bool m_bChangedUp = false;
        protected bool m_bChangedDown = false;
        private bool? _isIncreased;
        protected bool? m_bIsOutright = null;

        public bool EnableOddsChangeIndication
        {
            get { return DalStationSettings.Instance.EnableOddsChangeIndication; }
        }

        public IBetDomainVw BetDomainView
        {
            get { return m_objLine.BetDomain != null ? m_objLine.BetDomain.BetDomainView : null; }
        }

        public bool IsOutright
        {
            get
            {
                if (m_bIsOutright == null)
                {
                    Debug.Assert(m_objLine.BetDomain != null && m_objLine.BetDomain.Match != null);
                    m_bIsOutright = m_objLine.BetDomain.Match.outright_type != eOutrightType.None;
                }

                return (bool)m_bIsOutright;
            }
        }

        protected override void OnPropertyChanged(CommonObjects.ObservablePropertyBase opb)
        {
            base.OnPropertyChanged(opb);

            if (opb.PropertyName == "Active")
            {
                DoPropertyChanged("Visibility");
            }
            if (opb.PropertyName == "IsManuallyDisabled")
            {
                DoPropertyChanged("Visibility");
            }
        }

        protected override SyncDictionary<string, List<string>> LinePropsToViewProps
        {
            get { return m_diLinePropsToViewProps; }
        }

        public OddVw(OddLn oddLn)
            : base(oddLn)
        {
        }


        public bool ChangedUp
        {
            get { return m_bChangedUp; }
            set
            {
                m_bChangedUp = value;
                DoPropertyChanged("ChangedUp");
            }
        }

        public bool ChangedDown
        {
            get { return m_bChangedDown; }
            set
            {
                m_bChangedDown = value;
                DoPropertyChanged("ChangedDown");
            }
        }

        public override void UnsetChanged()
        {
            base.UnsetChanged();

            if (m_bChangedUp)
            {
                m_bChangedUp = false;
                DoPropertyChanged("ChangedUp");
            }

            if (m_bChangedDown)
            {
                m_bChangedDown = false;
                DoPropertyChanged("ChangedDown");
            }
        }

        public string SpecialBetdomainValue
        {
            get
            {
                double value;
                if (this.BetDomainView.IsToInverse)
                {
                    double.TryParse(this.BetDomainView.SpecialOddValue, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out value);

                    if (this.Sort > 0)
                        return (value * -1.0).ToString();
                    else
                        return value.ToString();
                }
                else
                {
                    return this.BetDomainView.SpecialOddValue;
                }
            }
        }

        protected override void OnRaisePropertiesChanged<T>(T objLine)
        {
            if (m_objLine.ChangedProps.Contains(m_objLine.Value))
            {
                if (EnableOddsChangeIndication)
                {
                    if (m_objLine.Value.Value > m_objLine.Value.PreviousValue)
                    {
                        m_bChangedUp = true;
                        AddToChangedPropNames("ChangedUp");
                        DoPropertyChanged("ChangedUp");
                    }
                    else if (m_objLine.Value.Value < m_objLine.Value.PreviousValue)
                    {
                        m_bChangedDown = true;
                        AddToChangedPropNames("ChangedDown");
                        DoPropertyChanged("ChangedDown");
                    }
                }
            }
        }

        public OddVw ThisView
        {
            get { return this; }
        }

        public long OutcomeId { get { return m_objLine.OutcomeId; } }
        public long OddId { get { return m_objLine.OddId.Value; } }
        public int Sort { get { return m_objLine.Sort.Value; } }

        public SyncList<CompetitorLn> OutrightsCompetitors
        {
            get
            {
                RefreshOutrightCompetitors();
                return m_lOutrightCompetitors;
            }
        }

        protected SyncList<CompetitorLn> m_lOutrightCompetitors = null;
        protected SyncList<string> m_lOutrightNames = new SyncList<string>();

        protected void RefreshOutrightCompetitors()
        {
            Debug.Assert(m_objLine.BetDomain != null && m_objLine.BetDomain.Match != null);

            string sTag = m_objLine.OddTag.Value;
            Debug.Assert(!string.IsNullOrEmpty(sTag));

            string[] arrPositions = sTag.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            PositionToOutrightDictionary diOutrightCompetitors = m_objLine.BetDomain.Match.OutrightCompetitors;
            Debug.Assert(diOutrightCompetitors != null);

            m_lOutrightCompetitors = new SyncList<CompetitorLn>();

            foreach (string sPos in arrPositions)
            {
                long lPos = 0L;

                if (long.TryParse(sPos, out lPos))
                {
                    CompetitorToOutrightLn cto = diOutrightCompetitors.SafelyGetValue(lPos);
                    Debug.Assert(cto != null);
                    CompetitorLn cmpt = cto.GetCompetitor();
                    Debug.Assert(cmpt != null);

                    m_lOutrightCompetitors.Add(cmpt);
                }
                else if (sPos == "OTHERS")
                {

                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        public string DisplayName
        {
            get
            {
                return m_objLine.GetDisplayName(DalStationSettings.Instance.Language);
            }
        }


        public string DisplayNameForPrinting(string language)
        {

            return m_objLine.GetDisplayName(language);

        }


        public decimal Value
        {
            get { return m_objLine.Value.Value; }
        }

        public string DisplayValue
        {
            get
            {
                if (m_objLine.Value.Value < 100)
                {
                    return string.Format("{0:0.00}", m_objLine.Value.Value);
                }
                return string.Format("{0:0}", m_objLine.Value.Value);
            }
        }

        public bool Active
        {
            get { return m_objLine.Active.Value; }
        }

        public bool IsVisible
        {
            get
            {
                return this.Visibility == System.Windows.Visibility.Visible;
            }
        }


        public override Visibility Visibility
        {
            get
            {
                if (this.Value <= 1.0m)
                {
                    return Visibility.Hidden;
                }
                if (this.LineObject.IsManuallyDisabled.Value)
                    return Visibility.Hidden;

                return m_objLine.BetDomain.BetDomainView.Visibility;
            }
        }

        public Visibility VisibilityBetDomainView
        {
            get
            {
                if (this.Value <= 1.0m)
                {
                    return Visibility.Collapsed;
                }
                if (this.LineObject.IsManuallyDisabled.Value)
                    return Visibility.Collapsed;


                return m_objLine.BetDomain.BetDomainView.Visibility;
            }
        }

        public override bool IsEnabled { get { return m_objLine.IsEnabled; } }
        public bool IsSelected { get { return m_objLine.IsSelected; } }

        public bool? IsIncreased
        {
            get { return _isIncreased; }
            set { _isIncreased = value; }
        }

        public override int GetHashCode()
        {
            return m_objLine.OddId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            OddVw ov = obj as OddVw;

            return ov != null ? m_objLine.OddId.Equals(ov.m_objLine.OddId) : base.Equals(obj);
        }

        public override string ToString()
        {
            string sLanguage = DalStationSettings.Instance.Language;
            string sDisplayName = m_objLine.GetDisplayName(sLanguage);
            return string.Format("OddVw {{OddId={0}, BetDomainId={1}, OddTag={2}, DisplayName({3})='{4}'}}", m_objLine.OddId, m_objLine.BetDomainId, m_objLine.OddTag, sLanguage, sDisplayName);
        }

        private SyncObservableCollection<VirtualHorsesCompetitor> vHCCompetitors = new SyncObservableCollection<VirtualHorsesCompetitor>();
        public SyncObservableCollection<VirtualHorsesCompetitor> VHCCompetitors
        {
            get
            {
                getVHCCompetitors();
                return vHCCompetitors;
            }
        }

        private void getVHCCompetitors()
        {
            if (vHCCompetitors.Count <= 0)
            {
                Debug.Assert(m_objLine.BetDomain != null && m_objLine.BetDomain.Match != null);
                string sTag = m_objLine.OddTag.Value;
                Debug.Assert(!string.IsNullOrEmpty(sTag));
                string[] arrPositions = sTag.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string pos in arrPositions)
                {
                    if (pos == "OTHERS")
                        continue;

                    VirtualHorsesCompetitor comp = new VirtualHorsesCompetitor();
                    comp.Position = pos;
                    comp.ShirtImage = m_objLine.BetDomain.Match.MatchView.OutrightCompetitorsVHC.Where(x => x.Position.ToString() == pos).Select(x => x.ShirtImage).FirstOrDefault();
                    vHCCompetitors.SafelyAdd(comp);
                }
            }
        }
    }

    public class VirtualHorsesCompetitor
    {
        public string Position { get; set; }
        public ImageSource ShirtImage { get; set; }
    }
}
