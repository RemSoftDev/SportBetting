using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SportRadar.DAL.Annotations;
using SportRadar.DAL.NewLineObjects;
using System.Windows;

namespace SportRadar.DAL.ViewObjects
{
    public class TipItemVw : ITipItemVw, INotifyPropertyChanged
    {
        private bool _isWay;
        private bool _isBank;
        private bool _isBankReadOnly;
        private bool _isSelected;
        private bool _isChecked = true;
        private bool _isBankEnabled = true;
        private bool _isBankEditable = true;
        private decimal _value;
        private bool _changedUp;
        private bool _changedDown;
        public IOddLn Odd { get; protected set; }
        public IBetDomainLn BetDomain { get; protected set; }
        public IMatchLn Match { get; protected set; }
        public IGroupLn TournamentGroup { get; protected set; }
        public IGroupLn SportGroup { get; protected set; }

        public decimal StartValue { get; protected set; }

        public TipItemVw(IOddLn odd)
        {
            Debug.Assert(odd != null);

            this.Odd = odd;
            this.BetDomain = odd.BetDomain;
            this.Match = this.BetDomain.Match;
            this.Value = Odd.Value.Value;
            this.StartValue = this.Odd.Value.Value;

            this.Match.ParentGroups.SafelyForEach(delegate(GroupLn group)
            {
                if (group.Type == GroupLn.GROUP_TYPE_GROUP_T)
                {
                    this.TournamentGroup = group;
                }
                else if (group.Type == GroupLn.GROUP_TYPE_SPORT)
                {
                    this.SportGroup = group;
                }

                return false;
            });
        }

        public void Release()
        {
            this.Odd = null;
            this.BetDomain = null;
            this.Match = null;
            this.TournamentGroup = null;
            this.SportGroup = null;
        }

        public IOddVw OddView { get { return this.Odd.OddView; } }
        public int MatchCode { get { return this.Match.Code.Value; } }
        public string SportName { get { return this.SportGroup.GetDisplayName(DalStationSettings.Instance.Language); } }
        public string TournamentName { get { return this.TournamentGroup.GetDisplayName(DalStationSettings.Instance.Language); } }
        public string HomeCompetitor { get { return this.Match.outright_type == eOutrightType.None ? this.Match.HomeCompetitor.GetDisplayName(DalStationSettings.Instance.Language) : string.Empty; } }
        public string AwayCompetitor { get { return this.Match.outright_type == eOutrightType.None ? this.Match.AwayCompetitor.GetDisplayName(DalStationSettings.Instance.Language) : string.Empty; } }
        public DateTime StartDate { get { return this.Match.StartDate.Value.LocalDateTime; } }
        public bool IsEnabled { get { return this.Odd.OddView.IsEnabled; } }
        public bool IsLiveBet { get { return this.Match.IsLiveBet.Value; } }
        public string BetDomainName
        {
            get
            {
                if (Odd.BetDomain.BetDomainView.IsToInverse)
                    return String.Format(LineSr.Instance.AllObjects.TaggedStrings.GetStringSafely(BetDomain.NameTag, DalStationSettings.Instance.Language), Odd.OddView.SpecialBetdomainValue);
                else
                    return this.BetDomain.GetDisplayName(DalStationSettings.Instance.Language);
            }
        }
        public string DisplayName { get { return this.Odd.OddView.DisplayName; } }
        public decimal Value
        {
            get { return _value; }
            set
            {
                ChangedDown = false;
                ChangedUp = false;
                if (value > _value)
                    ChangedUp = true;
                if (value < _value)
                    ChangedDown = true;
                _value = value;
                OnPropertyChanged();

            }
        }

        public bool ChangedUp
        {
            get { return _changedUp; }
            set
            {
                _changedUp = value;
                OnPropertyChanged();
            }
        }

        public bool ChangedDown
        {
            get { return _changedDown; }
            set
            {
                _changedDown = value;
                OnPropertyChanged();
            }
        }


        public Visibility AreAdditionalOddsNumberVisible
        {
            get
            {
                if (this.Match.SourceType == OldLineObjects.eServerSourceType.BtrVhc)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public bool IsWay
        {
            get { return _isWay; }
            set
            {
                if (_isWay == value)
                    return;

                _isWay = value;
                OnPropertyChanged();
            }
        }

        public bool IsBank
        {
            get { return _isBank; }
            set
            {
                if (_isBank == value)
                    return;

                _isBank = value;
                OnPropertyChanged();
            }
        }

        public bool IsBankReadOnly
        {
            get
            {
                return _isBankReadOnly;
            }
            set
            {
                if (_isBankReadOnly == value)
                    return;
                IsBankEditable = !value;
                _isBankReadOnly = value;
                OnPropertyChanged();
            }
        }
        public bool IsBankEditable
        {
            get { return _isBankEditable; }
            set
            {
                if (_isBankEditable == value)
                    return;

                _isBankEditable = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value)
                    return;

                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsBankEnabled
        {
            get { return _isBankEnabled; }
            set
            {
                if (_isBankEnabled == value)
                    return;
                _isBankEnabled = value;
                OnPropertyChanged();
            }
        }

        public override int GetHashCode()
        {
            return this.Odd != null ? this.Odd.GetHashCode() : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            TipItemVw tiv = obj as TipItemVw;

            return tiv != null && tiv.Odd != null && this.Odd != null ? this.Odd.OddId.Equals(tiv.Odd.OddId) : base.Equals(obj);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
