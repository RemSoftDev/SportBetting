using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using System;
using System.Globalization;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class SportCategory
    {
        public SportCategory(string sportName, SyncObservableCollection<TournamentVw> tournaments, long categoryId)
        {
            this.SportName = sportName;
            this.Tournaments = tournaments;
            this.CategoryID = categoryId;
        }

        private SyncObservableCollection<TournamentVw> _tournaments = new SyncObservableCollection<TournamentVw>();
        public SyncObservableCollection<TournamentVw> Tournaments
        {
            get
            {
                return _tournaments;
            }
            set
            {
                _tournaments = value;
                OnPropertyChanged("Tournaments");
            }
        }

        public long CategoryID = -1;

        private string _sportName = "";
        public string SportName 
        {
            get
            {
                return _sportName;
            }
            set
            {
                _sportName = value;
                OnPropertyChanged("SportName");
            }
        }

        public int Sort = 999;

        public override string ToString()
        {
            return string.Format("SportCategory (GroupId={0}, Name='{1}', TournamentsCount={2})", this.CategoryID, this.SportName, this.Tournaments.Count);
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
