using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class FoundUser : INotifyPropertyChanged
    {
        private bool _isVerified;
        private bool _active;
        private int _hasCard;
        public int AccountId { get; set; }
        public string Username { get; set; }
        public string EMail { get; set; }
        public string Phone { get; set; }
        public string Firstname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string Address { get; set; }
        public string Lastname { get; set; }
        public string FullName { get { return Firstname + " " + Lastname; } }
        public int HasCard
        {
            get { return _hasCard; }
            set
            {
                _hasCard = value;
                OnPropertyChanged("HasCard");
                OnPropertyChanged("ActiveCardImage");
                OnPropertyChanged("BlockUserCard");
            }
        }
        public bool IsVerified
        {
            get { return _isVerified; }
            set
            {
                _isVerified = value;
                OnPropertyChanged("IsVerified");
                OnPropertyChanged("ActiveUserImage");
            }
        }

        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                OnPropertyChanged("Active");
                OnPropertyChanged("ActiveUserImage");
            }
        }

        public FoundUser()
        {

        }
        public FoundUser(int accountId, string username, string email, string phone, string firstname, DateTime dateOfBirth,
            string documentNumber, string documentType, string address, string lastname, int hasCard)
        {
            this.AccountId = accountId;
            this.Address = address;
            this.DateOfBirth = dateOfBirth;
            this.DocumentNumber = documentNumber;
            this.DocumentType = documentType;
            this.EMail = email;
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.Phone = phone;
            this.Username = username;
            this.HasCard = hasCard;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public BitmapImage ActiveUserImage
        {
            get
            {
                string sFile = "";
                if (!IsVerified) sFile = "inactive_user.png";
                else if (!Active) sFile = "account_blocked.png";
                else sFile = "account_active.png";

                ImagePathConverter ipc = new ImagePathConverter();
                string sPath = ipc.Convert(null, null, sFile, null).ToString();

                try
                {
                    return new BitmapImage(new Uri((string)sPath.Replace("\\\\", "\\")));
                }
                catch (Exception)
                {
                    return new BitmapImage();
                }
            }
        }

        public BitmapImage ActiveCardImage
        {
            get
            {
                string sFile = "";
                if (HasCard == 169) sFile = "no_card.png";
                else if (HasCard == 0) sFile = "disable_card.png";
                else sFile = "status_active.png";

                ImagePathConverter ipc = new ImagePathConverter();
                string sPath = ipc.Convert(null, null, sFile, null).ToString();

                try
                {
                    return new BitmapImage(new Uri((string)sPath.Replace("\\\\", "\\")));
                }
                catch (Exception)
                {
                    return new BitmapImage();
                }
            }
        }
    }
}
