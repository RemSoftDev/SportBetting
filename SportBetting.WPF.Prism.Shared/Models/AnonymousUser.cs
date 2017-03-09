using System;
using IocContainer;
using Ninject;
using Shared;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Services;
using SportRadar.Common.Logs;
using WsdlRepository;

namespace SportBetting.WPF.Prism.Models
{
    public class AnonymousUser : User
    {
        private static readonly ILog Log = LogFactory.CreateLog(typeof(AnonymousUser));
        public AnonymousUser(string sessionId, long id)
        {
            SessionId = sessionId;
            AccountId = id;
            AvailableCash = Cashpool - TicketHandler.Stake;
            Refresh();

        }
        private IStationRepository _stationRepository;
        public IStationRepository StationRepository
        {
            get
            {
                return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>());
            }
        }
        private ITicketHandler _ticketHandler;
        public ITicketHandler TicketHandler
        {
            get
            {
                return _ticketHandler ?? (_ticketHandler = IoCContainer.Kernel.Get<ITicketHandler>());
            }
        }

        private decimal _cashpool;


        public override decimal UserConfidenceRaiting 
        {
            get { return 1; }
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

        [WsdlServiceSyncAspectSilent]
        public override void Refresh()
        {
            decimal reserved = 0;
            decimal factor;
            Cashpool = WsdlRepository.GetBalance(StationRepository.GetUid(this), out reserved, out factor) - reserved;
            AvailableCash = Cashpool - TicketHandler.Stake;
        }
    }
}
