using System.Collections.Generic;
using BaseObjects;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Collections.ObjectModel;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Authorization Login view model.
    /// </summary>
    [ServiceAspect]
    public class BalanceCheckpointsViewModel : AccountingBaseViewModel
    {

        private readonly ScrollViewerModule _ScrollViewerModule;
        private int _pageNumber = 1;
        private readonly int _pageSize = 50;
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultsViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public BalanceCheckpointsViewModel()
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);
            ScrollDownStart = new Command(OnScrollDownStartExecute);
            ScrollDownStop = new Command(OnScrollDownStopExecute);
            ScrollUpStart = new Command(OnScrollUpStartExecute);
            ScrollUpStop = new Command(OnScrollUpStopExecute);

            var scroller = this.GetScrollviewer();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }
            //if (Balance == null)
            //    GetBalanceCheckPoints();
            Mediator.Register<bool>(this, CreatedCheckpoint, MsgTag.CreatedCheckpoint);
            Mediator.Register<bool>(this, LoadCheckpoint, MsgTag.ShowBalanceCheckpoints);
        }



        #endregion


        #region Properties


        #endregion

        #region Commands


        public Command ScrollDownStart { get; private set; }
        /// <summary>
        /// Gets the ScrollDownStop command.
        /// </summary>
        public Command ScrollDownStop { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStart command.
        /// </summary>
        public Command ScrollUpStart { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStop command.
        /// </summary>
        public Command ScrollUpStop { get; private set; }

        #endregion

        #region Methods

        public void LoadCheckpoint(bool result)
        {
            if (Balance == null)
                GetBalanceCheckPoints();
        }
        public override void OnNavigationCompleted()
        {
            ChangeTracker.BalanceOperationsChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.AdminTitle2 = MultistringTags.BALANCE_OPERATIONS;
            LoadCheckpoint(true);
            base.OnNavigationCompleted();
        }


        private void OnScrollDownStartExecute()
        {
            this._ScrollViewerModule.OnScrollDownStartExecute(GetScrollviewer());
        }
        /// <summary>
        /// Method to invoke when the ScrollDownStop command is executed.
        /// </summary>
        private void OnScrollDownStopExecute()
        {
            OnGetBalanceCheckPoints(++_pageNumber);
            this._ScrollViewerModule.OnScrollDownStopExecute();
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStart command is executed.
        /// </summary>
        private void OnScrollUpStartExecute()
        {
            this._ScrollViewerModule.OnScrollUpStartExecute(GetScrollviewer());
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStop command is executed.
        /// </summary>
        private void OnScrollUpStopExecute()
        {
            this._ScrollViewerModule.OnScrollUpStopExecute();
        }

        private void CreatedCheckpoint(bool obj)
        {
            OnGetBalanceCheckPoints();
        }

        [AsyncMethod]
        private void GetBalanceCheckPoints()
        {
            PleaseWaitOnGetBalanceCheckPoints();
        }

        [PleaseWaitAspect]
        private void PleaseWaitOnGetBalanceCheckPoints()
        {
            OnGetBalanceCheckPoints();
        }

        [WsdlServiceSyncAspectSilent]
        private void OnGetBalanceCheckPoints()
        {
            try
            {
                Balance = new ObservableCollection<BalanceCheckpoint>();
                ErrorLabel = string.Empty;
                var result = WsdlRepository.GetStationBalanceCheckpoints(StationRepository.StationNumber, 0, _pageSize);
                var tmpBalances = new ObservableCollection<BalanceCheckpoint>();
                foreach (var balanceCheckpoint in Balance)
                {
                    tmpBalances.Add(balanceCheckpoint);
                }
                Balance = null;
                var balancesNew = ConverCheckPoints(result);
                foreach (var balanceCheckpoint in balancesNew)
                {
                    tmpBalances.Add(balanceCheckpoint);
                }
                Balance = new ObservableCollection<BalanceCheckpoint>(tmpBalances);

            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                //Balance = new ObservableCollection<BalanceCheckpoint>();
                //ShowError(exception.Reason.Translations.Where(x => x.XmlLang.ToLowerInvariant() == SelectedLanguage.ToLowerInvariant()).Select(x => x.Text).FirstOrDefault());
                ErrorLabel = exception.Detail.message;
            }

        }



        [WsdlServiceSyncAspect]
        private void OnGetBalanceCheckPoints(int pagenumber)
        {
            if (Balance.Count % _pageSize != 0)
                return;
            _pageNumber = pagenumber;
            try
            {

                var result = WsdlRepository.GetStationBalanceCheckpoints(StationRepository.StationNumber, (pagenumber - 1) * _pageSize, _pageSize);
                var tmpBalances = new ObservableCollection<BalanceCheckpoint>();
                foreach (var balanceCheckpoint in Balance)
                {
                    tmpBalances.Add(balanceCheckpoint);
                }
                Balance = null;
                var balancesNew = ConverCheckPoints(result);
                foreach (var balanceCheckpoint in balancesNew)
                {
                    tmpBalances.Add(balanceCheckpoint);
                }
                Balance = new ObservableCollection<BalanceCheckpoint>(tmpBalances);
            }
            catch (System.ServiceModel.FaultException<HubServiceException>)
            {
                //ShowError(exception.Reason.Translations.Where(x => x.XmlLang.ToLowerInvariant() == SelectedLanguage.ToLowerInvariant()).Select(x => x.Text).FirstOrDefault());
            }

        }

        private IEnumerable<BalanceCheckpoint> ConverCheckPoints(IEnumerable<StationBalanceCheckpoint> result)
        {
            var checkPoints = new ObservableCollection<BalanceCheckpoint>();
            if (result == null)
                return checkPoints;
            foreach (var arrayOfCheckpoint in result)
            {
                var checkpoint = new BalanceCheckpoint
                {
                    CreationTime = arrayOfCheckpoint.created_at,
                    Operator = arrayOfCheckpoint.operator_username,
                    Payin = arrayOfCheckpoint.pay_in,
                    Payout = arrayOfCheckpoint.pay_out,
                    Credit = arrayOfCheckpoint.credit
                };
                checkPoints.Add(checkpoint);
            }
            return checkPoints;
        }

        #endregion
    }
}