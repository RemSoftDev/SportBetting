using System;
using System.Threading;
using System.Windows.Data;
using SportRadar.Common.Collections;
using SportRadar.DAL.ViewObjects;
using System.Globalization;

namespace Shared
{


    public class Ticket : BaseModel
    {
        private decimal _bonusValue;
        private string _bonusValueRounded;
        private decimal _currentTicketPossibleWin;
        private bool _isVisibleBank;
        private decimal _manipulationFee;
        private decimal _maxBet;
        private decimal _maxWin;
        private decimal _minBet;
        private int _numberOfBets;
        private bool _maxOddExceeded;
        private decimal _stake;
        private int _systemX = 1;
        private TicketStates _ticketState = TicketStates.Single;
        private readonly SyncObservableCollection<ITipItemVw> _tipItems = new SyncObservableCollection<ITipItemVw>();
        private decimal _totalOdd;
        private decimal _stakeByRow;
        private string _systemButtonName = "System";
        private string _ticketNumber;
        private string _checkSum;

        public static int TICKET_TYP_SPORTBET = 0;
        public const int TICKET_TYP_LIVEBET = 1;
        //GMU 2011-01-21
        public const int TICKET_TYP_BOTH = 2;
        public const int TICKET_TYP_VFL = 3;

        public const int TICKET_TYP_VHC = 4;

        public const int TICKET_CANCEL_FAILED_TICKET_NOT_FOUND = -1;
        public const int TICKET_CANCEL_FAILED_ALREADY_CANCELED = -2;
        public const int TICKET_CANCEL_FAILED_ALREADY_BEGUN = -3;
        public const int TICKET_CANCEL_FAILED_TOO_OLD = -4;
        public const int TICKET_CANCEL_FAILED_CASHDRAWER_ALREADY_CLOSED = -5;
        public const int TICKET_ALREADY_PAIDED = -6;
        public const int TICKET_SAVE_FAILED = 0;//Server or Station has rejected the ticket because of an exception
        public const int TICKET_SAVE_SUCCESSFUL = 1;
        public const int TICKET_ALREADY_SAVED = 2;
        public const int TICKET_LIVE_ODD_CHANGED = 3;
        public const int TICKET_LIVE_ODD_INACTIVE = 4;
        public const int TICKET_SAVE_REJECTED = 5;//Server or Station has rejected the ticket because of invalid bet Data

        public const int USER_IS_ANONYMOUS = 3;
        public const int LOCATION_NOT_FOUND = 4;

        public const int TICKET_WONSTATUS_INVALID = -1;
        public const int TICKET_WONSTATUS_OPEN = 0;
        public const int TICKET_WONSTATUS_WON = 1;
        public const int TICKET_WONSTATUS_LOST = 2;
        public const int TICKET_WONSTATUS_PAID = 3;
        public const int TICKET_WONSTATUS_CANCELED = 4;

        private decimal _bonusPercentage;
        private int _rowCount;
        private int _systemY;
        private bool _isEditingStake;

        public Ticket()
        {

        }

        public bool TicketSaved { get; set; }

        public User User { get; set; }




        public string SystemButtonName
        {
            get { return _systemButtonName; }
            set
            {
                _systemButtonName = value;
                OnPropertyChanged();
            }
        }

        public string CheckSum
        {
            get { return _checkSum; }
            set { _checkSum = value; }
        }

        public string TicketNumber
        {
            get { return _ticketNumber; }
            set { _ticketNumber = value; }
        }


        public decimal ManipulationFeeValue
        {
            get { return _manipulationFee; }
            set
            {
                if (_manipulationFee == value)
                    return;

                _manipulationFee = value;
                OnPropertyChanged();
            }
        }


        public decimal CurrentTicketPossibleWin
        {
            get { return _currentTicketPossibleWin; }
            set
            {
                if (_currentTicketPossibleWin == value)
                    return;

                _currentTicketPossibleWin = value;
                OnPropertyChanged();
            }
        }

        public SyncObservableCollection<ITipItemVw> TipItems
        {
            get { return _tipItems; }
        }

        public int SystemX
        {
            get { return _systemX; }
            set
            {
                if (_systemX == value)
                    return;

                _systemX = value;
                OnPropertyChanged();
            }
        }

        public bool MaxOddExceeded
        {
            get { return _maxOddExceeded; }
            set
            {
                if (_maxOddExceeded == value)
                    return;

                _maxOddExceeded = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalOdd
        {
            get { return _totalOdd; }
        }

        public decimal TotalOddDisplay
        {
            get
            {
                decimal oddfactor = _totalOdd;
                var iOddFactor = (long)(oddfactor * 100);
                decimal dOddFactor = ((decimal)iOddFactor / 100);
                return dOddFactor;
            }
            set
            {
                if (_totalOdd == value)
                    return;
                _totalOdd = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfBets
        {
            get { return _numberOfBets; }
            set
            {
                if (_numberOfBets == value)
                    return;

                _numberOfBets = value;
                OnPropertyChanged();
            }
        }

        public TicketStates TicketState
        {
            get { return _ticketState; }
            set
            {
                if (_ticketState == value)
                    return;

                IsVisibleBank = value == TicketStates.System;
                _ticketState = value;
                OnPropertyChanged();
            }
        }

        public decimal Stake
        {
            get { return _stake; }
            set
            {
                if (_stake == value)
                    return;

                _stake = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisibleBank
        {
            get { return _isVisibleBank; }
            set
            {
                if (_isVisibleBank == value)
                    return;

                _isVisibleBank = value;
                OnPropertyChanged();
            }
        }

        public decimal TruncateDecimal(decimal valueToTruncate)
        {
            decimal multiplied = valueToTruncate * 100;
            decimal tempBonus = decimal.Truncate(multiplied);
            tempBonus = tempBonus / 100;

            return tempBonus;
        }

        public string BonusValueRounded
        {
            get
            {

                decimal tempBonus = TruncateDecimal(BonusValue);
                return tempBonus.ToString(CultureInfo.InvariantCulture);

                //_bonusValueRounded = BonusValue.ToString();
                //string a = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                //if (_bonusValueRounded.IndexOf(a) != -1)
                //{
                //    _bonusValueRounded = _bonusValueRounded.Substring(0, _bonusValueRounded.IndexOf(a) + 3);
                //}

                //return _bonusValueRounded;
            }
        }

        public decimal BonusValue
        {
            get { return _bonusValue; }
            set
            {
                if (_bonusValue == value)
                    return;
                _bonusValue = value;
                OnPropertyChanged("BonusValueRounded");
                OnPropertyChanged();
            }
        }

        public decimal MaxBet
        {
            get { return _maxBet; }
            set
            {
                if (_maxBet == value)
                    return;
                _maxBet = value;
                OnPropertyChanged();
            }
        }

        public decimal MaxWin
        {
            get { return _maxWin; }
            set
            {
                if (_maxWin == value)
                    return;

                _maxWin = value;
                OnPropertyChanged();
            }
        }

        public decimal MinBet
        {
            get { return _minBet; }
            set
            {
                if (_minBet == value)
                    return;
                _minBet = value;
                OnPropertyChanged();
            }
        }

        public decimal StakeByRow
        {
            get { return _stakeByRow; }
            set
            {
                if (_stakeByRow == value)
                    return;
                _stakeByRow = value;
                OnPropertyChanged();
            }
        }

        public decimal BonusPercentage
        {
            get { return _bonusPercentage; }
            set
            {
                if (value == _bonusPercentage)
                    return;
                _bonusPercentage = value;
                OnPropertyChanged("BonusPercentage");
            }
        }

        public int RowCount
        {
            get { return _rowCount; }
            set
            {
                if (value == _rowCount)
                    return;
                _rowCount = value;
                OnPropertyChanged();
            }
        }

        public int SystemY
        {
            get { return _systemY; }
            set
            {
                if (value == _systemY)
                    return;

                _systemY = value;
                OnPropertyChanged();
            }
        }

        public bool IsMaxOddBet { get; set; }

        public bool IsEditingStake
        {
            get { return _isEditingStake; }
            set
            {
                _isEditingStake = value;
                OnPropertyChanged();
            }
        }
    }
}