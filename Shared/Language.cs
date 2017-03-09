using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shared
{
    public class Language : INotifyPropertyChanged
    {
        private string _shortName;

        public Language(string lang)
        {
            ShortName = lang;
        }
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
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}