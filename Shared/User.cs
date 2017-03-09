using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Shared.Annotations;

namespace Shared
{
    public abstract class User : INotifyPropertyChanged
    {

        public enum roleId
        {

            /// <remarks/>
            Administrator,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Franchisor owner")]
            Franchisorowner,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Location owner")]
            Locationowner,

            /// <remarks/>
            Operator,

            /// <remarks/>
            [System.Xml.Serialization.XmlEnumAttribute("Up franchisor")]
            Upfranchisor,
        }

        private string _currency = "EUR";

        private string _sessionId;

        private long _accountId;

        public virtual string Username { get; set; }

        public virtual bool HasPermissions { get { return false; } }

        public virtual bool IsLoggedInWithIDCard { get; set; }

        private string _roleColor;

        public string CardNumber { get; set; }


        public string[] Permissions { protected get; set; }

        public string Role { get; set; }
        public string RoleColor
        {
            get { return "#FF" + _roleColor; }
            set { _roleColor = value; }
        }

        public virtual decimal DailyLimit { get; set; }
        public virtual decimal MonthlyLimit { get; set; }
        public virtual decimal WeeklyLimit { get; set; }


        public virtual bool AdminManagement { get { return false; } }
        public virtual bool BlockOperator { get { return false; } }
        public virtual bool BlockOperatorCard { get { return false; } }
        public virtual bool CreateOperator { get { return false; } }
        public virtual bool CloseBalance { get { return false; } }
        public virtual bool Credit { get { return false; } }
        public virtual bool UserManagement { get { return false; } }
        public virtual bool ActivateUser { get { return false; } }
        public virtual bool BindUserCard { get { return false; } }
        public virtual bool BlockUser { get { return false; } }
        public virtual bool BlockUserCard { get { return false; } }
        public virtual bool CreateUser { get { return false; } }
        public virtual bool CashStatistic { get { return false; } }
        public virtual bool EmptyBox { get { return false; } }
        public virtual bool PayoutPaymentNote { get { return false; } }
        public virtual bool PayoutCreditNote { get { return false; } }
        public virtual bool ViewStationBalance { get { return false; } }
        public virtual bool VerifyStation { get { return false; } }
        public virtual bool ViewSystemInfo { get { return false; } }
        public virtual bool ViewSystemInfoMonitors { get { return false; } }
        public virtual bool ViewSystemInfoNetwork { get { return false; } }
        public virtual bool TerminalRestart { get { return false; } }
        public virtual bool ProfitShareCheckpointRead { get { return false; } }
        public virtual bool ProfitShareCheckpointWrite { get { return false; } }
        public virtual bool ShopPaymentsRead { get { return false; } }
        public virtual bool ShopPaymentsWrite { get { return false; } }
        public virtual bool ShopPaymentsReadLocationOwner { get { return false; } }
        public virtual bool ViewCashHistory { get { return false; } }
        public virtual bool OperatorShiftSettlementWrite { get { return false; } }
        public virtual bool OperatorShiftSettlementRead { get { return false; } }
        public virtual bool OperatorShiftCheckpointWrite { get { return false; } }
        public virtual bool OperatorShiftCheckpointRead { get { return false; } }
        public virtual bool ShowOperatorShift { get { return false; } }
        public virtual bool ViewEmptyBox { get { return false; } }

        public virtual bool PinEnabled { get; set; }
        public virtual bool HasActiveCard { get; set; }
        public virtual bool AccessTestMode { get { return false; } }
        public virtual bool AuthenticateUser { get { return false; } }

        public virtual decimal Cashpool { get; set; }
        public virtual decimal UserConfidenceFactor { get; set; }

        public abstract void Withdrawmoney(decimal amount);
        public abstract void Addmoney(decimal amount);

        public string Currency
        {
            get { return _currency; }
            set
            {
                _currency = value;
                OnPropertyChanged("Currency");
            }
        }

        public decimal AvailableCash
        {
            get { return _availableCash + _cashToTransfer; }
            set
            {
                _availableCash = value;
                OnPropertyChanged();
            }
        }

        public string SessionId
        {
            get { return _sessionId; }
            set
            {
                _sessionId = value;
            }
        }

        public long AccountId
        {
            get { return _accountId; }
            set
            {
                _accountId = value;
            }
        }



        private roleId _roleID;
        private decimal _availableCash;
        private IDictionary<string, string> _userPropertiesDict = new Dictionary<string, string>();
        private IList<UserProperties> _userProperties = new List<UserProperties>();

        public roleId RoleID
        {
            get { return _roleID; }
            set { _roleID = value; }
        }

        public virtual decimal UserConfidenceRaiting
        {
            get { return _userConfidenceRaiting; }
            set { _userConfidenceRaiting = value; }
        }

        public string Language { get; set; }

        public IDictionary<string, string> UserPropertiesDict
        {
            get { return _userPropertiesDict; }
            set { _userPropertiesDict = value; }
        }

        public IList<UserProperties> UserProperties
        {
            get { return _userProperties; }
            set { _userProperties = value; }
        }

        public long ClientId { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract void Refresh();

        private decimal _cashToTransfer;
        private decimal _userConfidenceRaiting = 1;

        public decimal CashToTransfer
        {
            get { return _cashToTransfer; }
            set
            {
                _cashToTransfer = value;
                OnPropertyChanged("AvailableCash");
            }
        }

        public virtual string TaxNumber { get; set; }

        public virtual bool AccessShutdownTerminal
        {
            get { return false; }
        }
    }
}
