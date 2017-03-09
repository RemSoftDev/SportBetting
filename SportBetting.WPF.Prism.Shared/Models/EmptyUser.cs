using System;
using IocContainer;
using Ninject;
using Shared;
using SportRadar.Common.Logs;
using WsdlRepository;

namespace SportBetting.WPF.Prism.Models
{
    public class EmptyUser : User
    {

        private static readonly ILog Log = LogFactory.CreateLog(typeof(EmptyUser));

        public override void Withdrawmoney(decimal amount)
        {
        }
        private IStationRepository _stationRepository;
        public IStationRepository StationRepository
        {
            get
            {
                return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>());
            }
        }
        public override void Addmoney(decimal amount)
        {
        }

        public override void Refresh()
        {

        }
    }
}
