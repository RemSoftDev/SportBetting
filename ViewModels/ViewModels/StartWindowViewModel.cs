//using SportBetting.WPF.Prism.Models.Repositories;

using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using IocContainer;
using Ninject;
using WsdlRepository;
//using SportRadar.Cassier.Services.Impl;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class StartWindowViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartWindowViewModel"/> class.
        /// </summary>
        public StartWindowViewModel()
        {
            SendLogsCommand = new Command(OnSendLogsCommand);
        }

        #region properties
        public IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        #endregion

        #region Commands
        public Command SendLogsCommand { get; set; }

        #endregion


        #region Methods

        private void OnSendLogsCommand()
        {
            LogSending.stationNumber = StationRepository.StationNumber;
            LogSending.SendLogs();
        }


        #endregion
    }
}
