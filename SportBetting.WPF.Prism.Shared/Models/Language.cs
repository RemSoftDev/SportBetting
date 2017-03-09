using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SportBetting.WPF.Prism.Shared.Models
{


    public class Language : INotifyPropertyChanged
    {
        private string _shortName;

        #region Constructor & destructor
        public Language()
            : this(null)
        {
        }


        public Language(string shortName)
        {
            ShortName = shortName;
        }

        #endregion


        public string ShortName
        {
            get { return _shortName; }
            set
            {
                _shortName = value;
                OnPropertyChanged();
            }
        }

        public long Id { get; set; }

        #region Methods

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}