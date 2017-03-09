using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SportBetting.WPF.Prism.Shared.Annotations;
using SportBetting.WPF.Prism.Shared.Converters;
using SportRadar.DAL.OldLineObjects;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class SportBarItem : INotifyPropertyChanged
    {
        public SportBarItem(string name, string descriptor)
        {
            this.SportName = name;
            this.SportDescriptor = descriptor;
        }

        private string _name = "";
        private string _descriptor = "";
        private bool _isChecked = false;

        public string SportName
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("SportName");
                }
            }
        }

        public string SportDescriptor
        {
            get
            {
                return _descriptor;
            }
            set
            {
                if (_descriptor != value)
                {
                    _descriptor = value;
                    OnPropertyChanged("SportDescriptor");
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        public object Icon
        {
            get
            {
                object image = null;

                if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                    image = ResolveImagePath.ResolvePath("IceHockeyInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_SOCCER)
                    image = ResolveImagePath.ResolvePath("SoccerInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_BASEBALL)
                    image = ResolveImagePath.ResolvePath("BaseballInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_MIXED)
                    image = ResolveImagePath.ResolvePath("MixedSportsInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    image = ResolveImagePath.ResolvePath("TennisInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_RUGBY)
                    image = ResolveImagePath.ResolvePath("RugbyInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                    image = ResolveImagePath.ResolvePath("HandballInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                    image = ResolveImagePath.ResolvePath("VolleyInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_FOOTBALL)
                    image = ResolveImagePath.ResolvePath("AmFootballInctive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_MOTOSPORT)
                    image = ResolveImagePath.ResolvePath("MotorsportsInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_SNOOKER)
                    image = ResolveImagePath.ResolvePath("SnookerInactive.png");
                else if (this._descriptor.ToString() == "ALL_SPORTS")
                    image = ResolveImagePath.ResolvePath("AllsportsInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    image = ResolveImagePath.ResolvePath("BasketballInactive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_DARTS)
                    image = ResolveImagePath.ResolvePath("DartslInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_WINTERSPORTS)
                    image = ResolveImagePath.ResolvePath("WintersportsInactive.png");
                else
                    image = ResolveImagePath.ResolvePath("OtherSportsInactive.png");

                return image;
            }
        }

        public object IconActive
        {
            get
            {
                object image = null;

                if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                    image = ResolveImagePath.ResolvePath("IceHockeyActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_SOCCER)
                    image = ResolveImagePath.ResolvePath("SoccerActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_BASEBALL)
                    image = ResolveImagePath.ResolvePath("BaseballActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_MIXED)
                    image = ResolveImagePath.ResolvePath("MixedSportsActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    image = ResolveImagePath.ResolvePath("TennisActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_RUGBY)
                    image = ResolveImagePath.ResolvePath("RugbyActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                    image = ResolveImagePath.ResolvePath("HandballActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                    image = ResolveImagePath.ResolvePath("VolleyActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_FOOTBALL)
                    image = ResolveImagePath.ResolvePath("AmFootballActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_MOTOSPORT)
                    image = ResolveImagePath.ResolvePath("MotorsportsActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_SNOOKER)
                    image = ResolveImagePath.ResolvePath("SnookerActive.png");
                else if (this._descriptor.ToString() == SportSr.ALL_SPORTS)
                    image = ResolveImagePath.ResolvePath("AllsportsActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    image = ResolveImagePath.ResolvePath("BasketballActive.png");
                else if (this._descriptor.ToString() == SportSr.SPORT_DESCRIPTOR_DARTS)
                    image = ResolveImagePath.ResolvePath("DartslActive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_WINTERSPORTS)
                    image = ResolveImagePath.ResolvePath("WintersportsActive.png");
                else
                    image = ResolveImagePath.ResolvePath("OtherSportsActive.png");

                return image;
            }
        }

        public int SortingOrder
        {
            get
            {
                int sort = 9999;

                string descriptor = this.SportDescriptor;
                if (descriptor == SportSr.ALL_SPORTS)
                    sort = 0;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_SOCCER)
                    sort = 1;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    sort = 2;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    sort = 3;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                    sort = 4;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_RUGBY)
                    sort = 5;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                    sort = 6;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_FOOTBALL)
                    sort = 7;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                    sort = 8;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_MOTOSPORT)
                    sort = 9;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_WINTERSPORTS)
                    sort = 10;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_SNOOKER)
                    sort = 11;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_BASEBALL)
                    sort = 12;
                else if (descriptor == SportSr.SPORT_DESCRIPTOR_DARTS)
                    sort = 13;

                return sort;
            }
        }

        public override string ToString()
        {
            return string.Format("SportBarItem (SportName={0}, Descriptor={1}, Sorting={2})", this.SportName, this.SportDescriptor, this.SortingOrder);
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
