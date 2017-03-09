using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using System;
using System.Globalization;

namespace SportRadar.DAL.ViewObjects
{
    public class BetDomainVw : ViewObjectBase<BetDomainLn>, IBetDomainVw
    {
        public const int ODD_POSITION_1 = 1;
        public const int ODD_POSITION_2 = 2;
        public const int ODD_POSITION_3 = 3;

        protected static SyncDictionary<string, List<string>> m_diLinePropsToViewProps = new SyncDictionary<string, List<string>>()
        {
            {"IsSelected", new List<string>(){"IsSelected"}},
            {"Status", new List<string>(){"Visibility", "IsEnabled"}},
            {"ChangedCount", new List<string>(){"Status", "Visibility", "IsEnabled"}},
            {"IsManuallyDisabled", new List<string>(){"Status", "Visibility", "IsEnabled"}},
        };

        protected SortableObservableCollection<IOddVw> m_ocOdds = new SortableObservableCollection<IOddVw>();
        protected SyncObservableCollection<IOddVw> m_socOdds = new SyncObservableCollection<IOddVw>();

        protected bool? m_bIsOutright = null;
        protected int m_iAllOddsCount = 0;

        protected override SyncDictionary<string, List<string>> LinePropsToViewProps
        {
            get { return m_diLinePropsToViewProps; }
        }

        public BetDomainVw(BetDomainLn betDomain)
            : base(betDomain)
        {
        }

        public bool IsToInverse
        {
            get
            {
                return this.BetTag == "TTLPTSPR" || this.BetTag == "TTLPTOT";
            }
        }

        public bool IsOutright
        {
            get
            {
                if (m_bIsOutright == null)
                {
                    Debug.Assert(m_objLine.Match != null);
                    m_bIsOutright = m_objLine.Match.outright_type != eOutrightType.None;
                }

                return (bool)m_bIsOutright;
            }
        }

        public string DisplayName
        {
            get { return m_objLine.GetDisplayName(DalStationSettings.Instance.Language); }
        }

        public IMatchVw MatchView
        {
            get { return m_objLine.Match != null ? m_objLine.Match.MatchView : null; }
        }

        public new IBetDomainLn LineObject { get { return m_objLine; } }


        public long BetDomainId { get { return m_objLine.BetDomainId; } }

        public string BetTag { get { return m_objLine.BetTag; } }

        public string BetTypeTag { get { return m_objLine.BetDomainType.BetTypeTag; } }
        public string ScoreTypeTag { get { return m_objLine.BetDomainType.ScoreTypeTag; } }
        public string TimeTypeTag { get { return m_objLine.BetDomainType.TimeTypeTag; } }

        public string BetTypeName { get { return m_objLine.BetDomainType.BetTypeName; } }
        public string ScoreTypeName { get { return m_objLine.BetDomainType.ScoreTypeName; } }
        public string TimeTypeName { get { return m_objLine.BetDomainType.TimeTypeName; } }
        public int Sort { get { return m_objLine.BetDomainType.GetExternalSort(m_objLine.Match.MatchView.SportDescriptor); } }
        public bool IsSelected
        {
            get { return m_objLine.IsSelected; }
        }
        public string BetTypeDebugName { get { return m_objLine.BetDomainType != null ? m_objLine.BetDomainType.ToString() : string.Empty; } }

        public int AllOddsCount
        {
            get
            {
                if (m_iAllOddsCount == 0)
                {
                    m_iAllOddsCount = m_objLine.Odds.Count;
                }

                return m_iAllOddsCount;
            }
        }

        public int ChnagedCount { get { return m_objLine.ChangedCount.Value; } }

        protected override void OnRaisePropertiesChanged<T>(T objLine)
        {
            if (m_iAllOddsCount != m_objLine.Odds.Count)
            {
                m_iAllOddsCount = m_objLine.Odds.Count;

                AddToChangedPropNames("AllOddsCount");
                DoPropertyChanged("AllOddsCount");
            }

            if (m_objLine.ChangedProps.Contains(m_objLine.ChangedCount))
            {
                m_objLine.NotifyOddsEnabledChanged();
                RefreshOddViews();
            }
        }

        public List<string> OddTags
        {
            get
            {
                List<string> lOddTags = new List<string>();

                foreach (var ov in this.LineObject.Odds)
                {
                    if (!string.IsNullOrEmpty(ov.OddTag.Value))
                    {
                        lOddTags.Add(ov.OddTag.Value);
                    }
                }

                return lOddTags;
            }
        }

        public string BetDomainHelp
        {
            get
            {
                List<string> lOddTags = new List<string>();

                foreach (var ov in this.LineObject.Odds)
                {
                    lOddTags.Add(ov.OddTag.Value);
                }

                return string.Join(", ", lOddTags.ToArray());
            }
        }

        public string SpecialOddValue
        {
            get
            {
                decimal temp;
                bool b = Decimal.TryParse(m_objLine.SpecialOddValue.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out temp);
                if (!b)
                    return m_objLine.SpecialOddValue.Value;
                else
                    return String.Format("{0:0.0}", temp);
            }
        }

        public string SpecialOddValueFull
        {
            get { return m_objLine.SpecialOddValueFull.Value; }
        }

        public int MinCombination
        {
            get
            {
                return m_objLine.BetDomainExternalState != null && m_objLine.BetDomainExternalState.MinCombination != null ? (int)m_objLine.BetDomainExternalState.MinCombination : 0;
            }
        }

        public delegate bool DelegateFilterOdds(OddLn oddl);

        public SyncDictionary<int, OddLn> SortedOdds { get; protected set; }

        public SortableObservableCollection<IOddVw> Odds
        {
            get
            {
                this.SearchOdds(m_ocOdds, OddFilter);
                return m_ocOdds;
            }
        }

        protected void RefreshOddViews()
        {
#if DEBUG
            if (this.BetDomainId == 811116)
            {
            }
#endif

            var lOdds = m_objLine.Odds.Clone();
            lOdds.Sort(delegate(OddLn o1, OddLn o2) { return o1.Sort.Value.CompareTo(o2.Sort.Value); });

            SyncList<IOddVw> lOddViews = new SyncList<IOddVw>();

            foreach (var odd in lOdds)
            {
                lOddViews.Add(odd.OddView);
            }

            m_socOdds.ApplyChanges(lOddViews);
        }

        public SyncObservableCollection<IOddVw> OddViews
        {
            get
            {
                if (m_socOdds == null)
                {
                    m_socOdds = new SyncObservableCollection<IOddVw>();
                }

                if (m_socOdds.Count == 0)
                {
                    RefreshOddViews();
                }

                return m_socOdds;
            }
        }

        public bool ContainsOdds()
        {
            throw new NotImplementedException();
        }

        public SyncDictionary<int, OddLn> RefreshSortedOdds()
        {
            this.SortedOdds = m_objLine.GetSortedOdds();

            return this.SortedOdds;
        }

        public SyncDictionary<int, OddLn> EnsureSortedOdds()
        {
            if (this.SortedOdds == null || this.SortedOdds.Count == 0)
            {
                return RefreshSortedOdds();
            }

            return this.SortedOdds;
        }

        private IOddVw GetOddViewBySort(int iSort)
        {
            var odd = this.SortedOdds.SafelyGetValue(iSort);

            return odd != null ? odd.OddView : OddLn.EmptyOdd.OddView;
        }

        public IOddVw Odd1
        {
            get
            {
#if DEBUG
                if (this.BetDomainId == 811116)
                {
                }
#endif
                EnsureSortedOdds();

                IOddVw odd1 = this.GetOddViewBySort(ODD_POSITION_1);
                if (odd1.OddId == 0)
                    return UnderOverOdd1;
                return odd1;
            }
        }

        private IOddVw UnderOverOdd1
        {
            get
            {
                EnsureSortedOdds();

                IOddVw odd1 = this.GetOddViewBySort(0);
                return odd1;
            }
        }

        public IOddVw Odd2
        {
            get
            {
                EnsureSortedOdds();

                IOddVw odd2 = this.GetOddViewBySort(ODD_POSITION_2);
                if (odd2.OddId == Odd3.OddId)
                    return OddLn.EmptyOdd.OddView;
                return odd2;
            }
        }

        public IOddVw Odd3
        {
            get
            {
                EnsureSortedOdds();

                IOddVw odd3 = this.GetOddViewBySort(ODD_POSITION_3);
                if (odd3.OddId == 0)
                    return UnderOverOdd3;

                return odd3;
            }
        }
        private IOddVw UnderOverOdd3
        {
            get
            {
                EnsureSortedOdds();

                IOddVw odd3 = this.GetOddViewBySort(2);
                return odd3;
            }
        }

        public SortableObservableCollection<IOddVw> SearchOdds()
        {
            this.SearchOdds(m_ocOdds, null);

            return m_ocOdds;
        }

        public void SearchOdds(SortableObservableCollection<IOddVw> ocOdds, DelegateFilterOdds dfo)
        {
#if DEBUG
            if (this.BetDomainId == 811116)
            {
            }
#endif

            var hsOdds = new HashSet<IOddVw>();
            var lOdds = m_objLine.Odds.Clone();
            var lOddViews = ocOdds.ToSyncList();

            foreach (var oddLn in lOdds)
            {
                if (dfo == null || dfo(oddLn))
                {
                    hsOdds.Add(oddLn.OddView);

                    if (!lOddViews.Contains(oddLn.OddView))
                    {
                        lOddViews.Add(oddLn.OddView);
                    }
                }
            }

            for (int i = 0; i < lOddViews.Count; )
            {
                IOddVw oddView = lOddViews[i];

                if (!hsOdds.Contains(oddView))
                {
                    lOddViews.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if(this.LineObject.IsLiveBet.Value)
                lOddViews.Sort(delegate(IOddVw ov1, IOddVw ov2) { return ov1.LineObject.Sort.Value.CompareTo(ov2.LineObject.Sort.Value); });
            else if (this.IsOutright)
                lOddViews.Sort(delegate(IOddVw ov1, IOddVw ov2) { return ov1.LineObject.Value.Value.CompareTo(ov2.LineObject.Value.Value); });
            else
                lOddViews.Sort(delegate(IOddVw ov1, IOddVw ov2) { return ov1.LineObject.Sort.Value.CompareTo(ov2.LineObject.Sort.Value); });

            ocOdds.ApplyChanges(lOddViews);
        }

        private bool OddFilter(OddLn oddL)
        {
            if (((OddVw)oddL.OddView).VisibilityBetDomainView == Visibility.Collapsed)
                return false;

            return true;
        }

        public eBetDomainStatus Status { get { return m_objLine.Status.Value; } }

        public bool IsVisible 
        { 
            get 
            { 
                return Visibility == System.Windows.Visibility.Visible; 
            } 
        }

        public override Visibility Visibility
        {
            get
            {
                if (!m_objLine.Match.Active.Value || m_objLine.IsManuallyDisabled.Value)
                {
                    return Visibility.Collapsed;
                }

                return m_objLine.Status.Value == eBetDomainStatus.Visible || m_objLine.Status.Value == eBetDomainStatus.Inactive ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public override bool IsEnabled
        {
            get
            {
                return m_objLine.IsCashierEnabled;
            }
        }

        public override int GetHashCode()
        {
            return m_objLine.BetDomainId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            BetDomainVw bdv = obj as BetDomainVw;

            return bdv != null ? m_objLine.BetDomainId.Equals(bdv.m_objLine.BetDomainId) : base.Equals(obj);
        }

        public Visibility ShowSpecialValue
        {
            get
            {
                if (this.BetTypeTag == BetDomainTypeLn.BET_TYPE_UNDER_OVER)
                    return System.Windows.Visibility.Visible;
                else
                    return System.Windows.Visibility.Collapsed;
            }
        }

        public int ColumnCount
        {
            get
            {
                if (Odds.Count == 2)
                    return 2;

                if (IsOutright)
                {
                    return 2;
                }
                return 3;

            }
        }
    }
}
