using System.Collections.Generic;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using System.Collections.ObjectModel;
using SportBetting.WPF.Prism.Shared.Models;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class OperatorSettlementHistoryViewModel : BaseViewModel
    {
        #region Constructors
        public OperatorSettlementHistoryViewModel()
        {
            HidePleaseWait = false;
            //onPreviousPageClicked = new Command(PrevPage);
        }
        #endregion

        #region Commands


        #endregion

        #region Properties

        private ObservableCollection<SettlementHistory> settlementHistoryCollection = new ObservableCollection<SettlementHistory>();
        public ObservableCollection<SettlementHistory> _Checkpoints;
        public ObservableCollection<SettlementHistory> Checkpoints
        {
            get { return _Checkpoints; }
            set { _Checkpoints = value; OnPropertyChanged("Checkpoints"); }
        }

        private ObservableCollection<HistoryCheckpoint> _listOfCheckpoints = new ObservableCollection<HistoryCheckpoint>();
        public ObservableCollection<HistoryCheckpoint> ListOfCheckpoints { get { return _listOfCheckpoints; } set { _listOfCheckpoints = value; OnPropertyChanged("ListOfCheckpoints"); } }

        private HistoryCheckpoint _selectedCheckpoint;
        public HistoryCheckpoint SelectedCheckpoint
        {
            get
            {
                
                return _selectedCheckpoint;
            }
            set
            {
                if (value == null)
                    return;
                _selectedCheckpoint = value;
                var historyList = new List<SettlementHistoryDetail>(
                       WsdlRepository.GetSettlementHistoryDetails(_selectedCheckpoint.Id));
                _selectedCheckpoint.CheckpointDetails = historyList;
                CollapseChecpointsDetails(_selectedCheckpoint);

                OnPropertyChanged("SelectedCheckpoint");
            }
        }

        private void CollapseChecpointsDetails(HistoryCheckpoint cPoint)
        {
            for (int i = 0; i < _listOfCheckpoints.Count; i++)
            {
                if (_listOfCheckpoints[i].Equals(cPoint))
                {
                    _listOfCheckpoints[i].IsVisible = true;
                }
                else
                {
                    _listOfCheckpoints[i].IsVisible = false;
                }
                
            }
            OnPropertyChanged("ListOfCheckpoints");
        }

        #endregion

        #region Methods



        [WsdlServiceSyncAspect]
        private void onLoadData(bool b)
        {
            uid accountUid = new uid();

            //accountUid.account_id = 466.ToString(CultureInfo.InvariantCulture);

            accountUid = StationRepository.GetUid(ChangeTracker.CurrentUser);
            
            settlementHistoryCollection = new ObservableCollection<SettlementHistory>(WsdlRepository.GetSettlementHistory(accountUid));

            _Checkpoints = new ObservableCollection<SettlementHistory>(settlementHistoryCollection);
            OnPropertyChanged("Checkpoints");

            _listOfCheckpoints.Clear();
            foreach (var checkpoint in _Checkpoints )
            {
                var cPoint = new HistoryCheckpoint(checkpoint.startDate, checkpoint.endDate, (int)checkpoint.id, checkpoint.settlement_saldo);
                _listOfCheckpoints.Add(cPoint);
            }
            OnPropertyChanged("ListOfCheckpoints");
            OnPropertyChanged("SelectedCheckpoint");
        }


        public override void OnNavigationCompleted()
        {
            //if (ChangeTracker.CurrentMatch != null)
            //{
            //    ChangeTracker.CurrentMatch.IsStartUp = true;
            //}
            onLoadData(true);
            base.OnNavigationCompleted();
        }

        #endregion
    }
}
