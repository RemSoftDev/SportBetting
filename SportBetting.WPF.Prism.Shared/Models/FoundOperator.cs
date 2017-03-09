using System;
using System.ComponentModel;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class FoundOperator : INotifyPropertyChanged
    {
        private bool _active;
        private bool _activeCard = true;
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string EMail { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Location { get; set; }
        public string Franchisor { get; set; }
        public string Role { get; set; }
        public string CardPin { get; set; }

        public string Type { get; set; }

        public bool ActiveCard
        {
            get { return _activeCard; }
            set
            {
                _activeCard = value;
                OnPropertyChanged("ActiveCard");
            }
        }

        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                OnPropertyChanged("Active");
            }
        }

        public FoundOperator()
        {
        }

        public FoundOperator(int accountId, string username, string email, string firstname, string lastname, string type, string cardPin)
        {
            this.AccountId = accountId;
            this.EMail = email;
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.Username = username;
            this.Type = type;
            this.CardPin = cardPin;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
