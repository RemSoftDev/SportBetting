using BaseObjects;
using BaseObjects.ViewModels;
using System;
using System.Globalization;
using SportBetting.WPF.Prism.Modules.Aspects;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class MoneyPayoutViewModel : BaseViewModel
    {
        private const string AmountDescriptionFormatStringTemplate = "####,###,###,###,###,###,##0";

        /// <summary>
        /// Initializes a new instance of the <see cref="MoneyPayoutViewModel"/> class.
        /// </summary>
        public MoneyPayoutViewModel()
        {
            this.Amount = 0;
            this.NumberOfDecimals = 0;
            this.NumberOfDecimals = 2;

            Mediator.Register<string>(this, OnClearExecute, "ClearTicketNumber");
            Mediator.Register<string>(this, OnBackSpaceExecute, "PinBackspace");
            Mediator.Register<string>(this, OnPinButtonExecute, "PinButton");
        }



        #region --- props ---

        /// <summary>
        /// Gets or sets NumberOfDecimals (2 by default)
        /// </summary>
        public int NumberOfDecimals
        {
            get { return _numberOfDecimals; }
            set
            {
                _numberOfDecimals = value;
                OnPropertyChanged();
                NumberOfDecimalsChanged(value);
            }
        }



        /// <summary>
        /// Gets or sets Amount (0 by default)
        /// </summary>
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged();
                AmountChanged();
            }
        }


        private int _numberOfDecimals;
        private decimal _amount;

        /// <summary>
        /// Gets AmountDescription (Amount as string)
        /// </summary>
        public string AmountDescription
        {
            get
            {
                CultureInfo cultureInfo = null;

                if ((StationRepository != null) && (StationRepository.Culture != null))
                {
                    cultureInfo = StationRepository.Culture;
                }
                else
                {
                    cultureInfo = CultureInfo.InvariantCulture;
                }

                return Amount.ToString(this.AmountDescriptionFormatString, cultureInfo);
            }
        }

        /// <summary>
        /// Gets AmountDescriptionFormatString (format string that Amount is formated with)
        /// </summary>
        public string AmountDescriptionFormatString { get; private set; }

        public decimal Cashpool
        {
            get { return ChangeTracker.CurrentUser.Cashpool; }
        }

        #endregion


        #region --- methods ---

        public override void OnNavigationCompleted()
        {
            MyRegionManager.NavigateUsingViewModel<PinKeyboardViewModel>(RegionNames.EnterAmountPayoutPinKeyboardRegion);
            
            base.OnNavigationCompleted();
        }
        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.EnterAmountPayoutPinKeyboardRegion);
            
            base.Close();
        }

        private void AmountChanged()
        {
            this.OnPropertyChanged("AmountDescription");
        }

        private void NumberOfDecimalsChanged(int newValue)
        {
            if (newValue > 0)
            {
                this.AmountDescriptionFormatString = AmountDescriptionFormatStringTemplate
                    + "." + string.Join(string.Empty, System.Linq.Enumerable.Repeat("0", newValue));
            }
            else
            {
                this.AmountDescriptionFormatString = AmountDescriptionFormatStringTemplate;
            }

            this.OnPropertyChanged("AmountDescriptionFormatString");
        }

        private void OnPinButtonExecute(string obj)
        {
            int number;
            if (int.TryParse(obj, out number))
            {
                decimal power = this.Power(this.NumberOfDecimals);

                var amount = this.Amount;
                amount *= 10m; // move numbers forward
                amount += (number / power); // add number as last decimal

                this.Amount = amount;
            }
        }

        private void OnBackSpaceExecute(string obj)
        {
            decimal power = this.Power(this.NumberOfDecimals);

            var amount = this.Amount;
            if (amount != 0m) // if already 0 then do nothing
            {
                amount *= power; // get rid of decimals
                amount = Math.Floor(amount / 10); // remove last number
                amount = amount / power; // create decimals

                this.Amount = amount;
            }
        }

        private void OnClearExecute(string obj)
        {
            this.Amount = 0m;
        }

        private decimal Power(int numOfDecimals)
        {
            decimal result = 1m;

            if (numOfDecimals > 0)
            {
                for (int nI = 0; nI < numOfDecimals; nI++)
                {
                    result *= 10m;
                }
            }
            else if (numOfDecimals < 0)
            {
                for (int nI = numOfDecimals; nI < 0; nI++)
                {
                    result /= 10m;
                }
            }

            return result;
        }

        #endregion
    }
}
