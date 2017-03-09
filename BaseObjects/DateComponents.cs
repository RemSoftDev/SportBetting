using BaseObjects.ViewModels;

namespace BaseObjects
{
    public abstract class DateComponentBase : BaseViewModel
    {

        private int _Id = 0;
        private string _DisplayName = string.Empty;
        private bool _IsSelected = false;
        private bool _IsEnabled = false;

        public int Id
        {
            get { return this._Id; }
            set
            {
                if (this._Id != value)
                {
                    this._Id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        public string DisplayName
        {
            get { return this._DisplayName; }
            set
            {
                if (this._DisplayName != value)
                {
                    this._DisplayName = value;
                    this.OnPropertyChanged("DisplayName");
                }
            }
        }
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }
        public bool IsEnabled
        {
            get { return this._IsEnabled; }
            set
            {
                if (this._IsEnabled != value)
                {
                    this._IsEnabled = value;
                    this.OnPropertyChanged("IsEnabled");
                }
            }
        }

       
    }

    public sealed class Year : DateComponentBase
    {
    }

    public sealed class Month : DateComponentBase
    {
    }

    public sealed class Day : DateComponentBase
    {
    }
}
