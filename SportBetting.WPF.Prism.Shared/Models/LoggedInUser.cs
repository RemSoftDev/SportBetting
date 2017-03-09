using System;
using System.Collections.Generic;
using System.Timers;
using IocContainer;
using Ninject;
using Shared;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Services;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Models
{
    public class LoggedInUser : User
    {
        public LoggedInUser(long id, string sessionId, decimal cashpool,decimal dailyLimit, decimal weeklyLimit, decimal montlyLimit)
        {
            AccountId = id;
            SessionId = sessionId;
            Cashpool = cashpool;
            AvailableCash = Cashpool - TicketHandler.Stake;
            this.DailyLimit = dailyLimit;
            this.WeeklyLimit = weeklyLimit;
            this.MonthlyLimit = montlyLimit;
        }

       

        private ITicketHandler _ticketHandler;
        public ITicketHandler TicketHandler
        {
            get
            {
                return _ticketHandler ?? (_ticketHandler = IoCContainer.Kernel.Get<ITicketHandler>());
            }
        }

        private IStationRepository _stationRepository;
        public IStationRepository StationRepository
        {
            get
            {
                return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>());
            }
        }

        private string _sUsername;
        public new string Username
        {
            get
            {
                if (_sUsername == null)
                    return null;
                return _sUsername.ToLowerInvariant();

            }
            set
            {
                _sUsername = value;
                OnPropertyChanged();
            }
        }
        private DateTime _lastLoginDate = DateTime.Today;
        private decimal _cashpool;

        public DateTime LastLoginDate
        {
            get { return _lastLoginDate; }
            set { _lastLoginDate = value; }
        }

        private bool _isLoggedInWithIDCard = false;

        public override bool IsLoggedInWithIDCard
        {
            get
            {
                return _isLoggedInWithIDCard;
            }
            set { _isLoggedInWithIDCard = value; OnPropertyChanged(); }
        }


        public override decimal Cashpool
        {
            get { return _cashpool; }
            set
            {
                if (value == _cashpool)
                    return;
                _cashpool = value;
                OnPropertyChanged();
            }
        }

        public override void Withdrawmoney(decimal amount)
        {
            Refresh();
        }

        public override void Addmoney(decimal amount)
        {
            Refresh();
        }
        private static IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }

        private bool requestedTaxNumber = false;

        private profileForm form;
        [WsdlServiceSyncAspectSilent]
        public override void Refresh()
        {
            if ((string.IsNullOrEmpty(this.TaxNumber) && !requestedTaxNumber) || form == null)
            {
                form = WsdlRepository.LoadProfile(StationRepository.GetUid(this));
                if (form != null && form.fields != null)
                    foreach (var formField in form.fields)
                    {
                        if (formField.name == "tax_number")
                        {
                            TaxNumber = formField.value;
                        }
                    }
                requestedTaxNumber = true;
            }

            decimal reserved = 0;
            decimal factor;
            Cashpool = WsdlRepository.GetBalance(StationRepository.GetUid(this), out reserved, out factor) - reserved;
            AvailableCash = Cashpool - TicketHandler.Stake;

            _userConfidenceFactor = factor;
        }

       

        private decimal _userConfidenceFactor = 1;
        public override decimal UserConfidenceRaiting
        {
            get { return _userConfidenceFactor; }
        }

        public DateTime LastLoginDisplay { get { return LastLoginDate; } }
        public new uid GetUsernameUid()
        {
            uid uid = StationRepository.GetBasicUid();
            uid.username = Username;
            return uid;
        }





    }
}
