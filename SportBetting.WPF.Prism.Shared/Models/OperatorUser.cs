using System;
using System.Linq;
using IocContainer;
using Ninject;
using Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;

namespace SportBetting.WPF.Prism.Models
{
    public class OperatorUser : User
    {
        public OperatorUser(string sessionId)
        {
            SessionId = sessionId;

        }

        public override bool AdminManagement
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "access_administrative_usermanagement");
            }
        }
        public override bool AccessShutdownTerminal
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "terminal_shutdown");
            }
        }

        public override bool BlockOperator
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "administrative_usermanagement_block_user");
            }
        }
        public override bool ShowOperatorShift
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "display_current_shift");
            }
        }

        public override bool BlockOperatorCard
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "administrative_usermanagement_block_user_id_card");
            }
        }

        public override bool CreateOperator
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "administrative_usermanagement_create_new_administrative_user");
            }
        }

        public override bool CloseBalance
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "access_close_balance_functionality");
            }
        }

        public override bool Credit
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "access_credit_functionality");
            }
        }
        public override bool UserManagement
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "access_usermanagement");
            }
        }
        public override bool HasPermissions
        {
            get
            {
                return Permissions != null;
            }
        }
        public override bool ActivateUser
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "usermanagement_authenticate_user");
            }
        }
        public override bool BindUserCard
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "usermanagement_bind_id_card");
            }
        }
        public override bool BlockUser
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "usermanagement_block_user");
            }
        }
        public override bool BlockUserCard
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "usermanagement_block_user_id_card");
            }
        }
        public override bool CreateUser
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "usermanagement_create_new_user");
            }
        }
        public override bool EmptyBox
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "access_empty_box_functionality");
            }
        }

        public override bool ViewStationBalance
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "view_station_balance");
            }
        }

        public override bool CashStatistic
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "view_cash_statistics");
            }
        }

        public override bool PayoutPaymentNote
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "payout_payment_note");
            }
        }

        public override bool PayoutCreditNote
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "payout_credit_note");
            }
        }

        public override bool VerifyStation
        {
            get
            {
                return Permissions != null && Permissions.Any(x=>x == "verify_station");
            }
        }

        public override bool ViewCashHistory
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "view_cash_history");
            }
        }

        public override bool ViewSystemInfo
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "system_info");
            }
        }

        public override bool ViewSystemInfoMonitors
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "system_info");
            }
        }

        public override bool ViewSystemInfoNetwork
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "system_info");
            }
        }

        public override bool ProfitShareCheckpointRead
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "profit_share_checkpoint");
            }
        }

        public override bool ProfitShareCheckpointWrite
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "write_profit_share_checkpoint");
            }
        }

        public override bool ShopPaymentsRead
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "shop_payments");
            }
        }

        public override bool ShopPaymentsWrite
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "write_shop_payments");
            }
        }

        public override bool ShopPaymentsReadLocationOwner
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "read_location_owner");
            }
        }

        public override bool OperatorShiftSettlementWrite
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "write_operator_shift_settlement");
            }
        }

        public override bool OperatorShiftSettlementRead
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "operator_shift_settlement");
            }
        }

        public override bool OperatorShiftCheckpointWrite
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "operator_shift_checkpoint");
            }
        }

        public override bool OperatorShiftCheckpointRead
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "operator_shift_checkpoint");
            }
        }

        public override bool ViewEmptyBox
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "access_empty_box_functionality");
            }
        }

        public override bool AuthenticateUser
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "access_usermanagement");
            }
        }

        public override void Withdrawmoney(decimal amount)
        {
        }

        public override void Addmoney(decimal amount)
        {
        }

        public override void Refresh()
        {
            
        }

        public override bool TerminalRestart
        {
            get
            {
                return Permissions != null && Permissions.Any(x => x == "terminal_restart");
            }
        }

        private string _sUsername;
        public override string Username
        {
            get { return _sUsername; }
            set
            {
                _sUsername = value;
                OnPropertyChanged("Username");
            }
        }

        private bool _pinEnabled;
        public override bool PinEnabled
        {
            get { return _pinEnabled; }
            set { _pinEnabled = value; OnPropertyChanged("PinEnabled"); }
        }

        private bool _hasActiveCard;
        public override bool HasActiveCard
        {
            get { return _hasActiveCard; }
            set { _hasActiveCard = value; OnPropertyChanged("HasActiveCard"); }
        }

        public DateTime LastLoginDisplay
        {
            get { return DateTime.Now; }
        }

        public override bool AccessTestMode
        {
            get {return Permissions != null && Permissions.Any(x => x == "test_mode");}
        }

        public override decimal UserConfidenceFactor
        {
            get { return 1; }
        }
    }
}
