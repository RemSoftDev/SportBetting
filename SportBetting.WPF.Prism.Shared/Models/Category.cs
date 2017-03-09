using System.ComponentModel;
using System.Runtime.CompilerServices;
using SportBetting.WPF.Prism.Shared.Annotations;
using SportBetting.WPF.Prism.Shared.Converters;
using SportRadar.DAL.OldLineObjects;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class Category : INotifyPropertyChanged
    {

        public Category()
            : this(0, null)
        {
        }

        public Category(long id, string name)
        {
            Id = id;
            Name = name;
        }


        public long Id { get; set; }

        public int Sort { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string SportDescriptor = "";

        private string _name;

        public object CategoryIconBySport
        {
            get
            {
                object image = null;

                if (this.SportDescriptor.ToString() == "")
                    image = ResolveImagePath.ResolvePath("OtherSportsInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                    image = ResolveImagePath.ResolvePath("IceHockeyInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_SOCCER)
                    image = ResolveImagePath.ResolvePath("SoccerInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_BASEBALL)
                    image = ResolveImagePath.ResolvePath("BaseballInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_MIXED)
                    image = ResolveImagePath.ResolvePath("MixedSportsInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    image = ResolveImagePath.ResolvePath("TennisInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_RUGBY)
                    image = ResolveImagePath.ResolvePath("RugbyInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                    image = ResolveImagePath.ResolvePath("HandballInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                    image = ResolveImagePath.ResolvePath("VolleyInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_FOOTBALL)
                    image = ResolveImagePath.ResolvePath("AmFootballInctive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_MOTOSPORT)
                    image = ResolveImagePath.ResolvePath("MotorsportsInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_SNOOKER)
                    image = ResolveImagePath.ResolvePath("SnookerInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.ALL_SPORTS)
                    image = ResolveImagePath.ResolvePath("AllsportsInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    image = ResolveImagePath.ResolvePath("BasketballInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_DARTS)
                    image = ResolveImagePath.ResolvePath("DartslInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_WINTERSPORTS)
                    image = ResolveImagePath.ResolvePath("WintersportsInactive.png");
                else
                    image = ResolveImagePath.ResolvePath("OtherSportsInactive.png");

                return image;
            }
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