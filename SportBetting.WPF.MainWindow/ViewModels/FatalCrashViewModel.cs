using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Shared;

namespace MainWpfWindow.ViewModels
{
    class FatalCrashViewModel : BaseViewModel
    {
        public bool EnabledSendLogs { get; set; }
        public Visibility SendInProgress { get; set; }

        public FatalCrashViewModel()
        {
            Mediator.Register<string[]>(this, ActivateSendLogs, MsgTag.ZippedLogsUploaded);
            SendLogsCommand = new Command(OnSendLogsCommand);


            if (!LogSending.sendingThreadIsAlive)
            {
                EnabledSendLogs = true;
                SendInProgress = Visibility.Collapsed;
            }
            else
            {
                EnabledSendLogs = false;
                SendInProgress = Visibility.Visible;
            }

        }
        
        #region Commands
        public Command SendLogsCommand { get; set; }

        #endregion


        #region Methods


        private void OnSendLogsCommand()
        {
            LogSending.stationNumber = StationRepository.StationNumber;
            EnabledSendLogs = false;
            SendInProgress = Visibility.Visible;

            OnPropertyChanged("EnabledSendLogs");
            OnPropertyChanged("SendInProgress");

            LogSending.SendLogs();
        }

        private void ActivateSendLogs(string[] s)
        {
            EnabledSendLogs = true;
            SendInProgress = Visibility.Collapsed;
            OnPropertyChanged("EnabledSendLogs");
            OnPropertyChanged("SendInProgress");
        }


        #endregion
    }
}
