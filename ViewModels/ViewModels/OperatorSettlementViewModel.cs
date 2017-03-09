using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using System.Collections.ObjectModel;
using System;
using TranslationByMarkupExtension;
using System.Windows;
using WsdlRepository.WsdlServiceReference;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class OperatorSettlementViewModel : BaseViewModel
    {
        #region Constructors
        public OperatorSettlementViewModel()
        {
            Mediator.Register<bool>(this, LoadOperators, MsgTag.LoadOperators);

            BackCommand = new Command(OnBackCommand);
            ProduceSettlementCommand = new Command(onProduceSettlementCommand);
            onNextPageClicked = new Command(NextPage);
            onPreviousPageClicked = new Command(PrevPage);
            GridCreated = new Command<UIElement>(OnGridCreated);
            ItemCreated = new Command<UIElement>(OnRowItemCreated);
        }
        #endregion

        #region Commands

        public Command BackCommand { get; set; }
        public Command ProduceSettlementCommand { get; set; }
        public Command onNextPageClicked { get; private set; }
        public Command onPreviousPageClicked { get; private set; }
        public Command<UIElement> GridCreated { get; set; }
        public Command<UIElement> ItemCreated { get; set; }

        #endregion

        #region Properties

        private int pageSize = 0;
        private int currentPosition = 0;

        private ObservableCollection<OperatorWithShift> globalCollection = new ObservableCollection<OperatorWithShift>();

        /*
        public ObservableCollection<OperatorWithShift> _Operators;
        public ObservableCollection<OperatorWithShift> Operators
        {
            get { return _Operators; }
        }
        */
        public ObservableCollection<OperatorWithShift> Operators { get; protected set; }

        private OperatorWithShift _selectedOperator;
        public OperatorWithShift SelectedOperator
        {
            get { return _selectedOperator; }
            set
            {
                if (value == null)
                    return;

                _selectedOperator = value;
                OnPropertyChanged("SelectedOperator");
            }
        }

        private double GridHeight = 0.0;
        private double ItemHeight = 0.0;

        #endregion

        #region Methods

        private void LoadOperators(bool isNeeded)
        {
            //onLoadData();
            LoadInitialData();
        }
        public override void OnNavigationCompleted()
        {
            LoadOperators(true);
            base.OnNavigationCompleted();
        }

        private void LoadInitialData()
        {
            ObservableCollection<OperatorWithShift> tempCollection = new ObservableCollection<OperatorWithShift>();
            OperatorWithShift testCP = new OperatorWithShift();
            tempCollection.Add(testCP);
            this.Operators = new ObservableCollection<OperatorWithShift>(tempCollection);
            OnPropertyChanged("Operators");
        }

        [WsdlServiceSyncAspect]
        private void onLoadData(object sender, EventArgs e)
        {
            OperatorCriterias crit = new OperatorCriterias();
            uid sentUid = new uid();
            sentUid.location_id = StationRepository.GetUid(ChangeTracker.CurrentUser).location_id;
            sentUid.account_id = StationRepository.GetUid(ChangeTracker.CurrentUser).account_id;

            globalCollection = new ObservableCollection<OperatorWithShift>(WsdlRepository.GetAllOperatorsWithShifts(sentUid));

            ObservableCollection<OperatorWithShift> tempCollection = new ObservableCollection<OperatorWithShift>();
            if ((currentPosition + pageSize) < globalCollection.Count)
            {
                for (int i = currentPosition; i < pageSize; i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }
            }
            else
                for (int i = currentPosition; i < globalCollection.Count; i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }

            this.Operators = new ObservableCollection<OperatorWithShift>(tempCollection);

            OnPropertyChanged("Operators");
            if (this.Operators.Count > 0)
                SelectedOperator = this.Operators[0];
        }

        private void OnBackCommand()
        {
            MyRegionManager.NavigatBack(RegionNames.UsermanagementContentRegion);
            Mediator.SendMessage<bool>(true, MsgTag.LoadOperShiftReports);
        }

        [WsdlServiceSyncAspect]
        private void onProduceSettlementCommand()
        {
            if (SelectedOperator == null)
                return;

            bool result = WsdlRepository.CheckForEmptyBoxAndPayments(StationRepository.LocationID, SelectedOperator.id);

            if (!result)
                ProduceSettlement(null, null);
            else
            {

                var text = TranslationProvider.Translate(MultistringTags.TERMINAL_CONTINUE_WITH_SETTLEMENT,  SelectedOperator.firstname, SelectedOperator.surname);

                QuestionWindowService.ShowMessage(text, null, null, ProduceSettlement, NotContinue);
            }
        }

        private void NotContinue(object sender, EventArgs e)
        {
        }

        [WsdlServiceSyncAspect]
        private void ProduceSettlement(object sender, EventArgs e)
        {
            if (SelectedOperator == null)
                return;

            //produce settlement and reload page
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage();
                return;
            }

            int operatorID = SelectedOperator.id; //should be selected operator ID
            string opName = "";
            string opLName = "";
            string frName = "";
            string locName = "";
            string locOwnerName = "";
            DateTime stDate;
            DateTime enDate;
            CheckpointSlip[] cpArray;
            TotalSettlementSection totSection = new TotalSettlementSection();

            ProduceOperatorSettlementResponse opSett = new ProduceOperatorSettlementResponse();

            try
            {
                WsdlRepository.ProduceOperatorSettlement(ref operatorID, (int)ChangeTracker.CurrentUser.AccountId, out opName, out opLName, out frName, out locName, out locOwnerName, out stDate, out enDate, out cpArray, out totSection);
                opSett.checkpoints = cpArray;
                opSett.currentDate = DateTime.Now;
                opSett.franchisorName = frName;
                opSett.locationName = locName;
                opSett.operatorFirstName = opName;
                opSett.operatorLastName = opLName;
                opSett.settlementPeriodEndDate = enDate;
                opSett.settlementPeriodStartDate = stDate;
                opSett.total = totSection;
                opSett.operatorId = operatorID;
                opSett.LocationOwnerName = locOwnerName;
            }
            catch (System.ServiceModel.FaultException<HubServiceException> ex)
            {
                System.ServiceModel.FaultException<HubServiceException> exep = (System.ServiceModel.FaultException<HubServiceException>)ex;
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_SETTLEMENT_NO_SHIFTS).ToString(), onLoadData, false);
                return;
            }
            WaitOverlayProvider.ShowWaitOverlay();

            //print
            if (opSett.checkpoints != null)
            {
                bool isPrinted = PrinterHandler.PrintOperatorSettlementResponce(opSett);
                if (!isPrinted)
                    ShowPrinterErrorMessage();

                isPrinted = PrinterHandler.PrintOperatorSettlementResponce(opSett);
                if (!isPrinted)
                    ShowPrinterErrorMessage();
            }
            onLoadData(null, null);
        }

        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;

            string errorMessage = "";

            switch (status)
            {
                case 0:
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), null, true);
                    return;
                case 4:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_PAPER).ToString();
                    break;
                case 6:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_TONER).ToString();
                    break;
                case 7:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OPEN).ToString();
                    break;
                case 8:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_JAMMED).ToString();
                    break;
                case 9:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OFFLINE).ToString();
                    break;
            }

            ShowError(errorMessage, null, true);
        }

        private void NextPage()
        {
            if (globalCollection.Count < (currentPosition + pageSize))
                return;

            currentPosition += pageSize;
            LoadPage();
        }

        private void PrevPage()
        {
            if (currentPosition == 0)
                return;

            currentPosition -= pageSize;
            if (currentPosition < 0)
                currentPosition = 0;

            LoadPage();
        }

        private void LoadPage()
        {
            ObservableCollection<OperatorWithShift> tempCollection = new ObservableCollection<OperatorWithShift>();

            if ((currentPosition + pageSize) < globalCollection.Count)
            {
                for (int i = currentPosition; i < (currentPosition + pageSize); i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }
            }
            else
                for (int i = currentPosition; i < globalCollection.Count; i++)
                {
                    tempCollection.Add(globalCollection[i]);
                }

            this.Operators = new ObservableCollection<OperatorWithShift>(tempCollection);
            OnPropertyChanged("Operators");

            if (this.Operators.Count > 0)
                SelectedOperator = this.Operators[0];

            OnPropertyChanged("SelectedOperator");
        }

        private void OnGridCreated(UIElement obj)
        {
            if (GridHeight > 0)
                return;

            GridHeight = obj.RenderSize.Height - 12;

            if (ItemHeight == 0.0)
                return;

            CalculatePageSize();
        }

        private void OnRowItemCreated(UIElement obj)
        {
            if (ItemHeight > 0)
                return;

            ItemHeight = obj.RenderSize.Height;
            this.Operators = new ObservableCollection<OperatorWithShift>();
            OnPropertyChanged("Operators");

            if (GridHeight == 0.0)
                return;

            CalculatePageSize();
        }

        private void CalculatePageSize()
        {
            pageSize = (int)(GridHeight / ItemHeight);
            onLoadData(null, null);
        }

        #endregion
    }
}
