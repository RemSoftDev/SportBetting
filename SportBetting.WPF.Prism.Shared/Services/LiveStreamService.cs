using IocContainer;
using Ninject;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using WsdlRepository;

namespace SportBetting.WPF.Prism.Shared.Services
{
    public class LiveStreamService : ILiveStreamService
    {

        private IWsdlRepository _wsdlRepository;
        public IWsdlRepository WsdlRepository
        {
            get
            {
                return _wsdlRepository ?? (_wsdlRepository = IoCContainer.Kernel.Get<IWsdlRepository>());
            }
        }

        [WsdlServiceSyncAspect]
        public string GetLiveStreamFeed()
        {
            return WsdlRepository.GetLiveStreamFeed();
        }
    }
}