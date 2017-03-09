using System;
using System.ComponentModel;

namespace ViewModels.ViewModels
{
    public class HistoryFile : INotifyPropertyChanged
    {
        private string _cashDate;
        public String CashDate
        {
            get { return _cashDate ?? DateValue.ToString("HH:mm:ss dd.MM.yyyy"); }
            set { _cashDate = value; }
        }
        public String CashValue { get; set; }
        public DateTime DateValue { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}