using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SportBetting.WPF.Prism.Shared.Annotations;
using System.Windows.Media;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.CommonObjects;
using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Timers;
using TranslationByMarkupExtension;
using IocContainer;
using Ninject;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class TournamentVw : INotifyPropertyChanged
    {
        Timer FlagsTimer;

        public int TemporaryMatchesCount;
        public long SportId;
        public long CategoryId;

        public bool ContainsOutrights = false;

        public TournamentVw()
        {
        }

        public static ITranslationProvider TranslationProvider
        {
            get { return IoCContainer.Kernel.Get<ITranslationProvider>(); }
        }

        void FlagsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetFlag();
        }

        public TournamentVw(long id)
        {
            this.Id = id;
        }

        public TournamentVw(long id, long svrid, string name, long countryId, int sort, string minCombinationString, string country)
            : this(id)
        {
            this.Id = id;
            this.SvrId = svrid;
            this.Name = name;
            this.CountryId = countryId;
            this.Country = country;
            this.Sort = sort;
            this.MinimumCombinationString = minCombinationString;
            SetFlag();

            FlagsTimer = new Timer(10000);
            FlagsTimer.Elapsed += FlagsTimer_Elapsed;
            //FlagsTimer.Start();
        }

        public TournamentVw(long id, long svrid, string name, long countryId, int sort, string minCombinationString, string country, long sportId, bool isoutrightoralltournaments = false, string sportname = "", long categoryid = 0)
            : this(id)
        {
            this.Id = id;
            this.SvrId = svrid;
            this.Name = name;
            this.CountryId = countryId;
            this.Country = country;
            this.Sort = sort;
            this.MinimumCombinationString = minCombinationString;
            this.SportId = sportId;
            this.IsOutrightGroup = isoutrightoralltournaments;
            this.SportName = sportname;
            this.CategoryId = categoryid;
            SetFlag();

            FlagsTimer = new Timer(10000);
            FlagsTimer.Elapsed += FlagsTimer_Elapsed;
            
            //FlagsTimer.Start();
        }

        public int Sort { get; set; }

        public void ApplayTemporaryMatchesCount()
        {
            if (this.MatchesCount != TemporaryMatchesCount)
            {
                _matchesCount = TemporaryMatchesCount;
                OnPropertyChanged("MatchesCount");
            }

            TemporaryMatchesCount = 0;
        }

        public void AddMatch()
        {
            TemporaryMatchesCount++;
        }

        public long Id { get; set; }

        public long CountryId { get; set; }
        public long SvrId { get; set; }

        public void SetFlag()
        {
            if (this.IsOutrightGroup)
                return;

            ResourceAssignmentLn CountryShowing = LineSr.Instance.AllObjects.ResourceAssignments.SafelyGetValue(eAssignmentType.T_CONFIG.ToString() + ObjectBase.KEY_SEPARATOR + this.SvrId.ToString());
            if (CountryShowing != null)
            {
                if (CountryShowing.Active)
                    ShowCountryName = Visibility.Visible;
                else
                    ShowCountryName = Visibility.Collapsed;
            }
            else
                ShowCountryName = Visibility.Visible;

            ResourceRepositoryLn Resource = null;

            ResourceAssignmentLn TournamentAssignment = LineSr.Instance.AllObjects.ResourceAssignments.SafelyGetValue(eAssignmentType.TOURNAMENT.ToString() + ObjectBase.KEY_SEPARATOR + this.SvrId.ToString());
            if (TournamentAssignment != null && TournamentAssignment.Active)
            {
                Resource = LineSr.Instance.AllObjects.Resources.SafelyGetValue(TournamentAssignment.ResourceId);

                if (Resource != null)
                {
                    SetImageData(Convert.FromBase64String(Resource.Data));
                    return;
                }
            }

            ResourceAssignmentLn CountryAssignment = LineSr.Instance.AllObjects.ResourceAssignments.SafelyGetValue(eAssignmentType.COUNTRY.ToString() + ObjectBase.KEY_SEPARATOR + this.CountryId.ToString());
            if (CountryAssignment != null)
            {

                Resource = LineSr.Instance.AllObjects.Resources.SafelyGetValue(CountryAssignment.ResourceId);

                if (Resource != null)
                    SetImageData(Convert.FromBase64String(Resource.Data));
            }

        }

        private Visibility _showCountryName = Visibility.Collapsed;
        public Visibility ShowCountryName
        {
            get { return _showCountryName; }
            set
            {
                _showCountryName = value;
                OnPropertyChanged();
            }
        }

        ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public void SetImageData(byte[] data)
        {

            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = new MemoryStream(data);
            source.EndInit();
            source.Freeze();
            ImageSource = source;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string AllTournamentsName
        {
            get 
            {
                string result = string.Format(TranslationProvider.Translate(MultistringTags.TERMINAL_ALL_TOURNAMENTS_IN_SPORT) as string, this.SportName);
                return result; 
            }
        }

        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                OnPropertyChanged();
            }
        }


        public string MinimumCombinationString
        {
            get { return _minimumCombinationString; }
            set
            {
                _minimumCombinationString = value;
                OnPropertyChanged();
            }
        }


        public int MatchesCount
        {
            get { return _matchesCount; }
            set
            {
                _matchesCount = value;
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

        public bool IsOutrightGroup { get; set; }
        public bool IsAllTournamentsGroup { get; set; }

        private string _name;
        private string _country;
        private int _matchesCount;
        private bool _isSelected;
        private string _minimumCombinationString;
        public string SportName = "";

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            TournamentVw trmt = obj as TournamentVw;
            return trmt != null ? this.GetHashCode() == trmt.GetHashCode() : base.Equals(obj);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return string.Format("TournamentVw (TournamentId={0}, Name='{1}', MatchesCount={2}, SportId={3})", this.Id, this.Name, this.MatchesCount, this.SportId);
        }
    }
}