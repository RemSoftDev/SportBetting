using BaseObjects;
using SportBetting.WPF.Prism.Modules.Aspects;
using System.Windows;
using SportRadar.Common.Collections;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using SportBetting.WPF.Prism.Shared;

namespace ViewModels.ViewModels
{
    [ServiceAspect]
    public class ShopPaymentsViewModel : AccountingBaseViewModel
    {
        #region Constructors
        public ShopPaymentsViewModel()
        {

            GridCreated = new Command<UIElement>(OnGridCreated);
            ItemCreated = new Command<UIElement>(OnRowItemCreated);
            onAddPaymentClicked = new Command(AskPayment);
            onAddCreditClicked = new Command(AskCredit);

            //buttons from page were removed, instead used buttons in footer
            Mediator.Register<string>(this, PrevPage, MsgTag.LoadPrevPage);
            Mediator.Register<string>(this, NextPage, MsgTag.LoadNextPage);

            //onNextPageClicked = new Command(NextPage);
            //onPreviousPageClicked = new Command(PrevPage);

            if (ChangeTracker.CurrentUser.ShopPaymentsWrite)
            {
                AddCreditVisibility = Visibility.Visible;
                AddPaymentVisibility = Visibility.Visible;
            }
            else
            {
                AddCreditVisibility = Visibility.Collapsed;
                AddPaymentVisibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Commands
        //public Command onNextPageClicked { get; private set; }
        //public Command onPreviousPageClicked { get; private set; }
        public Command<UIElement> GridCreated { get; set; }
        public Command<UIElement> ItemCreated { get; set; }

        public Command onAddPaymentClicked { get; private set; }
        public Command onAddCreditClicked { get; private set; }
        #endregion

        #region Properties

        private double GridHeight = 0.0;
        private double ItemHeight = 0.0;

        public int itemsAmountPerPage = 0;
        private int currentPosition = 0;
        private long itemsTotal = 0;

        public Visibility AddCreditVisibility { get; set; }
        public Visibility AddPaymentVisibility { get; set; }

        private decimal _cashPosition;
        public decimal CashPosition
        {
            get { return _cashPosition; }
            set
            {
                _cashPosition = value;
                OnPropertyChanged("CashPosition");
            }
        }

        private decimal _paymentBalance;
        public decimal PaymentBalance
        {
            get { return _paymentBalance; }
            set
            {
                _paymentBalance = value;
                OnPropertyChanged("PaymentBalance");
            }
        }

        private decimal _cashBalance;
        public decimal CashBalance
        {
            get { return _cashBalance; }
            set
            {
                _cashBalance = value;
                OnPropertyChanged("CashBalance");
            }
        }

        private decimal _saldo;
        public decimal Saldo
        {
            get { return _saldo; }
            set
            {
                _saldo = value;
                OnPropertyChanged("Saldo");
            }
        }

        private PaymentFlowData _selectedPayment;
        private SyncObservableCollection<PaymentFlowData> _payments = new SyncObservableCollection<PaymentFlowData>();

        public PaymentFlowData SelectedPayment
        {
            get { return _selectedPayment; }
            set
            {
                if (value == null)
                    return;

                _selectedPayment = value;

                OnPropertyChanged("SelectedPayment");
            }
        }

        public SyncObservableCollection<PaymentFlowData> Payments
        {
            get { return _payments; }
            private set { _payments = value; }
        }

        #endregion

        #region Methods


        public override void OnNavigationCompleted()
        {
            ChangeTracker.ShopPaymentsChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_SHOP_PAYMENTS;
            if (itemsAmountPerPage == 0)
            {
                Payments.Add(new PaymentFlowData() { amount = 10, comment = "test" });
            }
            else
            {
                OnLoadData();
            }
            base.OnNavigationCompleted();
        }

        private void OnGridCreated(UIElement obj)
        {
            if (GridHeight > 0)
                return;

            GridHeight = obj.RenderSize.Height - 42.0;

            CalculatePageSize();


        }

        private void OnRowItemCreated(UIElement obj)
        {
            if (ItemHeight > 0)
                return;

            ItemHeight = obj.RenderSize.Height;

            CalculatePageSize();
        }

        private void CalculatePageSize()
        {
            itemsAmountPerPage = (int)(GridHeight / ItemHeight);
            if (itemsAmountPerPage > 0)
                OnLoadData();
        }

        [WsdlServiceSyncAspect]
        private void OnLoadData()
        {
            decimal paymentBalance = 0;
            decimal locationCashPosition = 0;
            decimal totalLocationBalance = 0;
            PaymentFlowData[] list;

            if (itemsAmountPerPage > 0)
            {
                ChangeTracker.ItemsAmmountPerPage = itemsAmountPerPage;
            }

            //download data from hub
            Saldo = WsdlRepository.GetStationPaymentFlowData(StationRepository.StationNumber, currentPosition, ChangeTracker.ItemsAmmountPerPage, out paymentBalance, out locationCashPosition, out totalLocationBalance, out list, out itemsTotal);

            CashPosition = locationCashPosition;
            CashBalance = totalLocationBalance;
            PaymentBalance = paymentBalance;

            Dispatcher.Invoke(() =>
            {
                Payments.Clear();

                SyncObservableCollection<PaymentFlowData> temp = new SyncObservableCollection<PaymentFlowData>();
                if (list != null)
                {
                    foreach (var paymentFlowData in list)
                    {
                        if (paymentFlowData.type == "OWED_PAYMENT") paymentFlowData.type = TranslationProvider.Translate(MultistringTags.OWED_PAYMENT) as string;
                        else if (paymentFlowData.type == "CREDIT") paymentFlowData.type = TranslationProvider.Translate(MultistringTags.CREDIT) as string;
                        else if (paymentFlowData.type == "PAYMENT") paymentFlowData.type = TranslationProvider.Translate(MultistringTags.PAYMENT) as string;
                        temp.Add(paymentFlowData);
                    }

                    Payments = new SyncObservableCollection<PaymentFlowData>(temp);
                    OnPropertyChanged("Payments");
                }
            });

        }

        private void AskCredit()
        {
            AskPaymentOrCredit("credit");
        }

        private void AskPayment()
        {
            AskPaymentOrCredit("payment");
        }

        public void AskPaymentOrCredit(string type)
        {
            ChangeTracker.PaymentFlowOperationType = type;
            MyRegionManager.NavigateUsingViewModel<AddCreditPaymentViewModel>(RegionNames.UsermanagementContentRegion);
        }

        [AsyncMethod]
        private void NextPage(string obj)
        {
            if (itemsTotal < (currentPosition + itemsAmountPerPage))
                return;

            currentPosition += itemsAmountPerPage;
            OnLoadData();
        }

        [AsyncMethod]
        private void PrevPage(string obj)
        {
            if (currentPosition == 0)
                return;

            currentPosition -= itemsAmountPerPage;
            if (currentPosition < 0)
                currentPosition = 0;

            OnLoadData();
        }

        #endregion
    }
}
