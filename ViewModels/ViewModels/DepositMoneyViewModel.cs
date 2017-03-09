using System;
using System.ComponentModel;
using BaseObjects.ViewModels;
using Nbt.Services.Scf.CashIn.Validator;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.OldCode;

namespace ViewModels.ViewModels
{


    /// <summary>
    /// Categories view model.
    /// </summary>
    [ServiceAspect]
    public class DepositMoneyViewModel : BaseViewModel
    {
        private decimal _depositedMoney;
        private decimal _lastDepositedMoney;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DepositMoneyViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public DepositMoneyViewModel()
        {

                StationSettings.SubscribeCashin(DepositCashIn_CashIn);
            StationRepository.EnableCashIn();

        }

        void TrackCashpoolPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }



        #endregion

        #region Properties

        public decimal DepositedMoney
        {
            get { return _depositedMoney; }
            set
            {
                _depositedMoney = value;
                OnPropertyChanged("DepositedMoney");
            }
        }

        public decimal LastDepositedMoney
        {
            get { return _lastDepositedMoney; }
            set
            {
                _lastDepositedMoney = value;
                OnPropertyChanged("LastDepositedMoney");
            }
        }

        public decimal Cashpool
        {
            get { return ChangeTracker.CurrentUser.Cashpool; }
        }

        #endregion

        #region Commands


        #endregion

        #region Methods


        public override void Close()
        {
            var minLimit = ChangeTracker.CurrentUser.DailyLimit;
            if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
            if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                minLimit = ChangeTracker.CurrentUser.MonthlyLimit;

            StationRepository.SetCashInDefaultState(minLimit);

            StationSettings.UnSubscribeCashin(DepositCashIn_CashIn);

            base.Close();
        }
        [AsyncMethod]
        void DepositCashIn_CashIn(object sender, CashInEventArgs e)
        {
            LastDepositedMoney = e.MoneyIn;
            DepositedMoney += LastDepositedMoney;
        }

        #endregion
    }
}