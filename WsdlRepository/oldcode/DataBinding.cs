using System;
using System.Collections.Generic;
using System.Linq;
using IocContainer;
using Ninject;
using Shared;
using SportRadar.DAL.ViewObjects;
using SportRadar.DAL.NewLineObjects;

namespace WsdlRepository.oldcode
{



    public class DataBinding : IDataBinding
    {
        private static TipListInfo _tipListInfo = new TipListInfo();

        public static List<ITipItemVw> TipItems
        {
            get
            {
                return Ticket.TipItems.ToSyncList().Where(x => x.IsChecked).ToList();
            }


        }

        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }

        public static Ticket Ticket
        {
            get;
            set;
        }


        private static decimal BonusFromOdd
        {
            get { return StationRepository.GetBonusFromOdd(Ticket); }
        }

        public static decimal MaxOdd
        {
            get { return StationRepository.GetMaxOdd(Ticket); }
        }

        private static decimal MinStakeCombiBet
        {
            get
            {
                return GetMax(StationRepository.GetMinStakeCombiBet(Ticket), StationRepository.GetMinStakePerRow(Ticket) * RowCountForMinStake);
            }
        }

        private static decimal MinStakeSystemBet
        {
            get
            {
                return GetMax(StationRepository.GetMinStakeSystemBet(Ticket), StationRepository.GetMinStakePerRow(Ticket) * RowCountForMinStake);
            }
        }

        private static decimal MinStakeSingleBet
        {
            get
            {
                return GetMax(StationRepository.GetMinStakeSingleBet(Ticket), StationRepository.GetMinStakePerRow(Ticket) * RowCountForMinStake);
            }
        }

        private static int _rowCountForMinStake = 1;
        private static int RowCountForMinStake
        {
            get { return _rowCountForMinStake; }
            set { _rowCountForMinStake = value; }
        }

        public TipListInfo TipListInfo
        {
            get { return _tipListInfo; }
            set { _tipListInfo = value; }
        }

        private static void RefreshBanks()
        {
            foreach (var tipItem in TipItems)
            {
                tipItem.IsBankReadOnly = false;

                if (TipItems.Count(x => x.Odd.BetDomain.BetDomainId == tipItem.Odd.BetDomain.BetDomainId) < 2)
                {
                    continue;
                }
                foreach (TipItemVw curItem in TipItems)
                {
                    if (curItem.Odd.BetDomain.BetDomainId != tipItem.Odd.BetDomain.BetDomainId)
                        continue;
                    curItem.IsBank = true;
                    curItem.IsBankReadOnly = true;
                    curItem.IsWay = true;
                }

            }

        }

        public static bool AddBank(OddVw curOdd)
        {
            int count = 0;
            foreach (TipItemVw curItem in TipItems)
            {
                if (curItem.Odd.BetDomain.BetDomainId == curOdd.LineObject.BetDomain.BetDomainId)
                {
                    count++;
                    curItem.IsBank = true;
                    curItem.IsBankReadOnly = true;
                }
            }
            return count > 0;

        }

        public static bool RemoveBank(OddVw curOdd)
        {
            int count = 0;
            TipItemVw modifyItem = null;
            foreach (TipItemVw curItem in TipItems)
            {
                if (curItem.Odd.BetDomain.BetDomainId == curOdd.LineObject.BetDomain.BetDomainId)
                {
                    count++;
                    modifyItem = curItem;
                }
            }
            if (count < 2 && modifyItem != null)
            {
                modifyItem.IsBank = false;
            }
            return count < 2;
        }

        private IConfidenceFactor _confidenceFactor;
        public IConfidenceFactor ConfidenceFactor
        {
            get
            {
                return _confidenceFactor ?? (_confidenceFactor = IoCContainer.Kernel.Get<IConfidenceFactor>());
            }
        }

        /// <summary>
        /// Eintrag aus Ticket l?schen
        /// </summary>
        /// <param name="ticketState"> </param>
        /// <param name="TipListInfo"> </param>
        /// <param name="tipItems"> </param>
        /// <param name="reset"> </param>
        /// <param name="p"></param>
        static object locker = new object();
        public void UpdateSystemOrCombiticket(Ticket ticketToCalculate)
        {
            lock (locker)
            {

                Ticket = ticketToCalculate;

                List<ITipItemVw> localTipItems = new List<ITipItemVw>();
                localTipItems = ticketToCalculate.TipItems.ToSyncList().Where(x => x.IsChecked).ToList();

                decimal oddVal = 1;
                decimal multiWayOddVal = 1;
                int? minCombMax = 0;
                int bonusTipsCount = 0;

                Dictionary<long, List<ITipItemVw>> tipItemDict = new Dictionary<long, List<ITipItemVw>>();
                foreach (TipItemVw t in localTipItems)
                {
                    long iSvrMatchId = t.Match.MatchId;

                    if (tipItemDict.ContainsKey(iSvrMatchId))
                    {
                        tipItemDict[iSvrMatchId].Add(t);
                    }
                    else
                    {
                        List<ITipItemVw> list = new List<ITipItemVw>();
                        list.Add(t);
                        tipItemDict.Add(iSvrMatchId, list);
                    }
                }
                // de: Anzahl der Wege berechnen                // en: Number of ways to calculate
                // de: h?chste Quote der Mehrwege finden        // en: highest rate of multipath are
                //
                int pathCount = 0;
                int rowCount = 1;
                int singlesCount = 0;

                int iBetDomainNumberOutrightPodium = 1050;
                int iPosWins = 1;
                int iSelections = 1;

                foreach (var list in tipItemDict.Values)
                {
                    if (list.Count >= 1)
                    {
                        decimal maxOdd = 1;

                        if (list.Count(y => y.BetDomain.BetDomainNumber != null && y.BetDomain.BetDomainNumber.Value == iBetDomainNumberOutrightPodium) >= 2)
                        {
                            var outrightsPodium = list.OrderByDescending(x => x.Value).Where(y => y.BetDomain.BetDomainNumber.Value == iBetDomainNumberOutrightPodium).Take(3);
                            maxOdd = outrightsPodium.Sum(x => x.Value) / outrightsPodium.Count();
                            iPosWins = list.Count(y => y.BetDomain.BetDomainNumber.Value == iBetDomainNumberOutrightPodium);
                            iSelections *= iPosWins <= 3 ? iPosWins : 3;
                        }
                        else
                        {
                            foreach (TipItemVw tip in list)
                            {
                                if (maxOdd < tip.Value)
                                {
                                    maxOdd = tip.Value;
                                }
                            }
                        }

                        ITipItemVw t = list[0];
                        if (t.IsBank || list.Count > 1)
                        {
                            pathCount++;
                            rowCount *= list.Count;
                            multiWayOddVal *= maxOdd;
                        }
                        else
                        {
                            oddVal *= maxOdd;
                            singlesCount++;
                        }
                        int? curCombMax = minCombinationOfAll(t);
                        if (curCombMax > minCombMax)
                        {
                            minCombMax = curCombMax;
                        }
                        if (list.Count == 1 && ticketToCalculate.TicketState == TicketStates.Multy)
                        {
                            if (t.Value >= BonusFromOdd)
                                //Bonus gibt es nicht bei Mehrwegen
                                bonusTipsCount++;
                        }
                    }
                }
                TipListInfo.Bet = ticketToCalculate.Stake;
                TipListInfo.MinCombination = minCombMax;
                TipListInfo.NumOfTipps = tipItemDict.Count;
                TipListInfo.PathCount = pathCount;
                TipListInfo.RowCount = rowCount;
                RowCountForMinStake = rowCount;
                TipListInfo.MultiWayOddFactor = multiWayOddVal;
                TipListInfo.CurrentTicketPossibleWin = 0;
                TipListInfo.FullOddFactor = 0;

                TipListInfo.ManipulationFeePercentage = StationRepository.GetManipulationFeePercentage(ticketToCalculate);
                TipListInfo.BonusFactor = StationRepository.GetBonusValueForBets(ticketToCalculate) / 100 + 1;

                bool isVirtual = IsVirtual(ticketToCalculate);

                switch (ticketToCalculate.TicketState)
                {
                    default:
                        foreach (var tipItem in localTipItems)
                        {
                            tipItem.IsBank = false;
                        }
                        if (localTipItems.Count == 1)
                        {
                            UpdateSingleticketItems(localTipItems, ticketToCalculate);
                        }
                        if (localTipItems.Count == 0)
                        {
                            TipListInfo.MinBet = 0;
                            TipListInfo.OddOfTipps = 0;
                            TipListInfo.MaxBet = 0;
                            TipListInfo.MinWin = 0;
                            TipListInfo.MaxWin = 0;
                            TipListInfo.ManipulationFeePercentage = 0;
                            ticketToCalculate.MaxOddExceeded = false;
                            TipListInfo.ResetNumXY();
                        }
                        break;
                    case TicketStates.Multy:
                        //rowCount = 1;
                        TipListInfo.ResetNumXY();
                        //TipListInfo.FullOddFactor = oddVal * multiWayOddVal * TipListInfo.BonusFactor;
                        TipListInfo.FullOddFactor = oddVal * multiWayOddVal;
                        TipListInfo.IllegalOddFactor = TipListInfo.FullOddFactor;
                        if (TipListInfo.FullOddFactor >= MaxOdd)
                        {
                            TipListInfo.IsMaxOddBet = true;
                            if (TipListInfo.FullOddFactor > MaxOdd)
                            {
                                TipListInfo.IllegalOddFactor = TipListInfo.FullOddFactor;
                                ticketToCalculate.MaxOddExceeded = true;
                            }
                            //TipListInfo.FullOddFactor = MaxOdd;
                            //TipListInfo.OddOfTipps = MaxOdd;
                        }
                        else
                        {
                            TipListInfo.OddOfTipps = oddVal * multiWayOddVal;
                            //TipListInfo.IllegalOddFactor = 0;
                            TipListInfo.IsMaxOddBet = false;
                            ticketToCalculate.MaxOddExceeded = false;
                        }
                        foreach (var tipItem in localTipItems)
                        {
                            tipItem.IsBank = false;
                        }
                        TipListInfo.MinBet = MinStakeCombiBet;
                        TipListInfo.MaxBet = StationRepository.GetMaxStakeCombi(ticketToCalculate);

                        //check confidence factor
                        ticketToCalculate.TotalOddDisplay = TipListInfo.FullOddFactor;
                        if (!StationRepository.IsTestMode && !isVirtual)
                        {
                            decimal conf = ConfidenceFactor.CalculateFactor(ticketToCalculate);
                            if (TipListInfo.MaxBet > (conf))
                            {
                                TipListInfo.MaxBet = Round(conf, 2);
                            }
                        }
                        else if (isVirtual)
                        {
                            TipListInfo.MaxBet = 1000;
                            TipListInfo.MinBet = 1;
                        }


                        TipListInfo.MinWin = TipListInfo.FullOddFactor * TipListInfo.MinBet / rowCount * (100 - TipListInfo.ManipulationFeePercentage) / 100;

                        var manipulationFee = (TipListInfo.MaxBet / rowCount * TipListInfo.ManipulationFeePercentage) / 100;
                        TipListInfo.MaxWin = (((TipListInfo.MaxBet - manipulationFee) / rowCount) * TipListInfo.FullOddFactor * TipListInfo.BonusFactor) - TipListInfo.MaxBet;

                        var maxWinMultiBetFromAdmin = StationRepository.GetMaxWinMultiBet(ticketToCalculate);
                        if (maxWinMultiBetFromAdmin > 0 && TipListInfo.MaxWin > maxWinMultiBetFromAdmin && !isVirtual)
                        {
                            var a0 = 100 * maxWinMultiBetFromAdmin * rowCount;
                            var a1 = (100 - TipListInfo.ManipulationFeePercentage);
                            var a2 = (a1 * TipListInfo.BonusFactor * TipListInfo.FullOddFactor);

                            var varX = (a0) / (a2 - 100 * rowCount);

                            TipListInfo.MaxBet = Round(varX, 2);
                        }

                        var pw1 = (TipListInfo.Bet - (TipListInfo.Bet * TipListInfo.ManipulationFeePercentage / 100)) * TipListInfo.BonusFactor * TipListInfo.FullOddFactor;
                        TipListInfo.CurrentTicketPossibleWin = pw1 / rowCount * (iSelections != 1 ? iSelections : 1);

                        TipListInfo.MaxWin = ((TipListInfo.MaxBet - (TipListInfo.MaxBet * TipListInfo.ManipulationFeePercentage / 100)) * TipListInfo.BonusFactor * TipListInfo.FullOddFactor) / rowCount;
                        TipListInfo.FullOddFactor = TipListInfo.FullOddFactor;
                        break;
                    case TicketStates.System:
                        TipListInfo.SystemX = ticketToCalculate.SystemX;
                        //TipListInfo.IllegalOddFactor = 0;

                        int ind = 0;
                        if (!LimitHandling.SystemBetYAllowed(TipListInfo.SystemY, ticketToCalculate))
                        {
                            //avoid useless time and memory consuming calculations
                            TipListInfo.MinBet = 0;
                            TipListInfo.OddOfTipps = 0;
                            TipListInfo.MaxBet = 0;
                            TipListInfo.MinWin = 0;
                            TipListInfo.MaxWin = 0;
                            TipListInfo.ManipulationFeePercentage = 0;
                            TipListInfo.ResetNumXY();
                            //UpdateSystemOrCombiticket(ticketToCalculate);
                            return;
                        }
                        if (TipListInfo.SystemX == 0)
                            TipListInfo.ResetNumXY();


                        decimal[] oddVals = new decimal[TipListInfo.SystemY];
                        bool disableBankBtn = TipListInfo.PathCount + TipListInfo.MinSystemY >= TipListInfo.NumOfTipps;
                        decimal dBankTipValue = 0;
                        foreach (TipItemVw t in localTipItems)
                        {
                            if (!t.IsBank && t.Odd != null)
                            {
                                if (ind < oddVals.Length)
                                    oddVals[ind++] = t.Value;
                                else
                                {
                                }


                                t.IsBankEnabled = !disableBankBtn;
                            }
                            else if (t.IsBank && t.Odd != null)
                            {
                                if (dBankTipValue == 0)
                                    dBankTipValue = 1; // to make multiplication possible;
                                dBankTipValue *= t.Value;
                            }
                        }
                        RefreshBanks();

                        decimal maxOdd = OddUtilities.AllCombinationsSum(oddVals, TipListInfo.SystemX); // TipListInfo.SystemX

                        if (maxOdd == 0)
                        {
                            ticketToCalculate.TicketState = TicketStates.Multy;
                            UpdateSystemOrCombiticket(ticketToCalculate);
                            return;
                        }

                        if (maxOdd * TipListInfo.MultiWayOddFactor > MaxOdd)
                        {
                            ticketToCalculate.MaxOddExceeded = true;
                        }
                        else
                        {
                            ticketToCalculate.MaxOddExceeded = false;
                        }

                        if (oddVals.Length < TipListInfo.SystemX)
                        {
                            UpdateSystemOrCombiticket(ticketToCalculate);
                            return;
                        }
                        int[,] temp;
                        OddUtilities.SetPermutations(out temp, TipListInfo.SystemY, TipListInfo.SystemX);
                        RowCountForMinStake = temp.GetLength(0) * tipItemDict.Aggregate(1, (current, t) => current * t.Value.Count);
                        //ViewControl.SetTicketErrorMessage(TicketError.NoError);
                        TipListInfo.MinBet = MinStakeSystemBet;
                        //TipListInfo.MaxBet = StationSettings.Station.MaxStakeSystemBet;
                        TipListInfo.OddOfTipps = maxOdd;

                        decimal manFee = 0;
                        manFee = StationRepository.GetManipulationFeePercentage(ticketToCalculate);

                        TipListInfo.MaxBet = (100 * StationRepository.GetMaxWinSystemBet(ticketToCalculate) * TipListInfo.RowCount) / ((100 - manFee) * TipListInfo.BonusFactor * (maxOdd * TipListInfo.MultiWayOddFactor));

                        TipListInfo.MaxBet = Round(Math.Min(TipListInfo.MaxBet, StationRepository.GetMaxStakeSystemBet(ticketToCalculate)), 2);

                        if (dBankTipValue > 0)
                        {
                            if (maxOdd * (multiWayOddVal / rowCount) > MaxOdd)
                            {
                                TipListInfo.FullOddFactor = maxOdd * multiWayOddVal;
                                ticketToCalculate.MaxOddExceeded = true;
                            }
                            else
                            {
                                TipListInfo.FullOddFactor = maxOdd * multiWayOddVal;
                                ticketToCalculate.MaxOddExceeded = false;
                            }
                        }
                        else
                        {

                            TipListInfo.FullOddFactor = maxOdd;

                        }

                        ticketToCalculate.TotalOddDisplay = TipListInfo.FullOddFactor;
                        ////check confidence factor
                        if (!StationRepository.IsTestMode && !isVirtual)
                        {
                            decimal conf = ConfidenceFactor.CalculateFactor(ticketToCalculate);
                            if (TipListInfo.MaxBet > (conf))
                            {
                                TipListInfo.MaxBet = Round(conf, 2);
                            }
                        }
                        else if (isVirtual)
                        {
                            TipListInfo.MaxBet = 1000;
                            TipListInfo.MinBet = 1;
                        }

                        TipListInfo.MinWin = TipListInfo.MinBet * maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount;
                        //TipListInfo.MaxWin = TipListInfo.MaxBet * maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount * TipListInfo.BonusFactor * (100 - TipListInfo.ManipulationFeePercentage) / 100;

                        TipListInfo.MaxWin = ((TipListInfo.MaxBet * (100 - TipListInfo.ManipulationFeePercentage) / 100) * TipListInfo.BonusFactor * maxOdd * TipListInfo.MultiWayOddFactor) / TipListInfo.RowCount;


                        /////////////////////////////////////////////
                        var maxWinSystemBetFromAdmin = StationRepository.GetMaxWinSystemBet(ticketToCalculate);
                        if (maxWinSystemBetFromAdmin > 0 && TipListInfo.MaxWin > maxWinSystemBetFromAdmin && !isVirtual)
                        {
                            TipListInfo.MaxWin = maxWinSystemBetFromAdmin;
                            //TipListInfo.MaxBet = (100 * TipListInfo.MaxWin) / ((100 - manFee) * TipListInfo.BonusFactor * (maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount));
                            TipListInfo.MaxBet = Round((100 * StationRepository.GetMaxWinSystemBet(ticketToCalculate) * TipListInfo.RowCount) / ((100 - manFee) * TipListInfo.BonusFactor * (maxOdd * TipListInfo.MultiWayOddFactor)), 2);
                        }
                        /////////////////////////////////////////////

                        //TipListInfo.CurrentTicketPossibleWin = TipListInfo.Bet * maxOdd * TipListInfo.MultiWayOddFactor / TipListInfo.RowCount * TipListInfo.BonusFactor * (100 - TipListInfo.ManipulationFeePercentage) / 100;

                        TipListInfo.CurrentTicketPossibleWin = ((TipListInfo.Bet - ((TipListInfo.Bet * TipListInfo.ManipulationFeePercentage) / 100)) * TipListInfo.BonusFactor * maxOdd * TipListInfo.MultiWayOddFactor) / TipListInfo.RowCount;

                        TipListInfo.OddOfTipps = maxOdd;


                        break;
                }

                ticketToCalculate.NumberOfBets = localTipItems.Count;
                ticketToCalculate.TotalOddDisplay = TipListInfo.FullOddFactor;
                if (TipListInfo.RowCount == iSelections)
                    ticketToCalculate.ManipulationFeeValue = TipListInfo.ManipulationFeeValue;
                else
                    ticketToCalculate.ManipulationFeeValue = TipListInfo.ManipulationFeeValue / TipListInfo.RowCount * iSelections;
                ticketToCalculate.MaxBet = TipListInfo.MaxBet;
                //ticketToCalculate.MaxBet = Math.Min(TipListInfo.MaxBet, StationRepository.GetMaxStake(ticketToCalculate));
                //ticketToCalculate.MaxBet = Math.Min(TipListInfo.MaxBet, TipListInfo.Bet);
                ticketToCalculate.MinBet = TipListInfo.MinBet;
                ticketToCalculate.RowCount = TipListInfo.RowCount;
                ticketToCalculate.BonusPercentage = TipListInfo.BonusFactorPerc;
                ticketToCalculate.BonusValue = TipListInfo.BonusValue / TipListInfo.RowCount;
                ticketToCalculate.MaxWin = TipListInfo.MaxWin;
                ticketToCalculate.StakeByRow = ticketToCalculate.Stake / TipListInfo.RowCount;
                ticketToCalculate.CurrentTicketPossibleWin = TipListInfo.CurrentTicketPossibleWin;
                ticketToCalculate.SystemX = TipListInfo.SystemX;
                ticketToCalculate.SystemY = TipListInfo.SystemY;
                ticketToCalculate.IsMaxOddBet = TipListInfo.IsMaxOddBet;
                //ticketToCalculate.SystemButtonName = TranslationProvider.Translate(MultistringTags.SYSTEM_FORMAT TipListInfo.SystemX, TipListInfo.SystemY, "+Banker");
            }

        }

        private static decimal Round(decimal value, int digitsAfterComa)
        {
            for (int i = 0; i < digitsAfterComa; i++)
            {
                value *= 10;
            }
            var intValue = (int)value;
            value = intValue;
            for (int i = 0; i < digitsAfterComa; i++)
            {
                value /= 10;
            }
            return value;

        }

        public void UpdateSingleticketItems()
        {
            UpdateSingleticketItems(TipItems);
        }

        public void UpdateSingleticketItems(IList<ITipItemVw> tipItems, Ticket ticketToCalculate = null)
        {
            decimal oddVal = 0;

            TipListInfo.Bet = ticketToCalculate.Stake;
            TipListInfo.BonusFactor = StationRepository.GetBonusValueForBets(ticketToCalculate) / 100 + 1;

            foreach (var tipItem in tipItems)
            {
                oddVal += tipItem.Value;
            }

            if (tipItems.Count > 0)
                TipListInfo.MinCombination = minCombinationOfAll(tipItems[0]);

            TipListInfo.NumOfTipps = 1;
            TipListInfo.OddOfTipps = oddVal;
            TipListInfo.FullOddFactor = oddVal;
            TipListInfo.IllegalOddFactor = TipListInfo.FullOddFactor;

            if (TipListInfo.FullOddFactor > MaxOdd)
            {
                TipListInfo.IsMaxOddBet = true;
                ticketToCalculate.MaxOddExceeded = true;
            }
            else
            {
                //TipListInfo.IllegalOddFactor = 0;
                TipListInfo.IsMaxOddBet = false;
                ticketToCalculate.MaxOddExceeded = false;
            }

            TipListInfo.MinBet = MinStakeSingleBet;

            if (tipItems.Count > 0)
                TipListInfo.MaxBet = Round(LimitHandling.SingleMaxStake(oddVal * TipListInfo.ManipulationFeeReduction, ticketToCalculate), 2);
            else
                TipListInfo.MaxBet = Round(LimitHandling.SingleMaxStake(1 * TipListInfo.ManipulationFeeReduction, ticketToCalculate), 2);

            //check confidence factor
            ticketToCalculate.TotalOddDisplay = TipListInfo.FullOddFactor;
            bool isVirtual = IsVirtual(ticketToCalculate);
            if (!StationRepository.IsTestMode && !isVirtual)
            {
                decimal conf = ConfidenceFactor.CalculateFactor(ticketToCalculate);
                if (TipListInfo.MaxBet > (conf))
                {
                    TipListInfo.MaxBet = Round(conf, 2);
                }
            }
            else if (isVirtual)
            {
                TipListInfo.MaxBet = 1000;
                TipListInfo.MinBet = 1;
            }


            TipListInfo.MaxWin = TipListInfo.OddOfTipps * TipListInfo.MaxBet *
                             ((TipListInfo.BonusFactor > 0) ? TipListInfo.BonusFactor : 1) *
                             TipListInfo.ManipulationFeeReduction;
            var maxWinSingleBetFromAdmin = StationRepository.GetMaxWinSingleBet(ticketToCalculate);
            if (maxWinSingleBetFromAdmin > 0 && TipListInfo.MaxWin > maxWinSingleBetFromAdmin && !isVirtual)
            {
                TipListInfo.MaxWin = maxWinSingleBetFromAdmin;
                var manipulationFee = StationRepository.GetManipulationFeePercentage(ticketToCalculate);
                TipListInfo.MaxBet = Round(100 * TipListInfo.MaxWin / ((100 - manipulationFee) * TipListInfo.BonusFactor * TipListInfo.FullOddFactor), 2);

                TipListInfo.MaxWin = TipListInfo.OddOfTipps * TipListInfo.MaxBet *
                             ((TipListInfo.BonusFactor > 0) ? TipListInfo.BonusFactor : 1) *
                             TipListInfo.ManipulationFeeReduction;
            }
            else if (isVirtual)
            {
                var manipulationFee = StationRepository.GetManipulationFeePercentage(ticketToCalculate);
            }

            TipListInfo.MinWin = TipListInfo.OddOfTipps * TipListInfo.MinBet *
                                 ((TipListInfo.BonusFactor > 0) ? TipListInfo.BonusFactor : 1) *
                                 TipListInfo.ManipulationFeeReduction;

            TipListInfo.CurrentTicketPossibleWin = TipListInfo.OddOfTipps * TipListInfo.Bet *
                                  ((TipListInfo.BonusFactor > 0) ? TipListInfo.BonusFactor : 1) *
                                  TipListInfo.ManipulationFeeReduction;
        }

        private bool IsVirtual(Ticket ticket)
        {
            if (ticket.TipItems.Count <= 0)
                return false;

            if (ticket.TipItems.ToSyncList().ElementAt(0).Odd == null)
                return false;

            SportRadar.DAL.OldLineObjects.eServerSourceType type = ticket.TipItems.ToSyncList().ElementAt(0).Odd.BetDomain.Match.SourceType;

            return type == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc || type == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl;
        }

        private static int? minCombinationOfAll(ITipItemVw item)
        {
            int? minComb = 0;
            //if (item != null && item.Odd != null && item.Odd.BetDomain != null)
            //{
            //    minComb = item.Odd.BetDomain.MinCombination;
            //    var curMatch = item.Odd.BetDomain.Match;
            //    if (curMatch != null && curMatch.MinCombination > minComb)
            //    {
            //        minComb = curMatch.MinCombination;
            //    }
            //}
            return minComb;
        }





        /// <summary>
        /// Change SystemX Value
        /// </summary>
        public void ChangeSystemX(int val, TicketStates state, Ticket newTicket)
        {
            if (val > 0 || TipListInfo.MinCombination < TipListInfo.SystemX + TipListInfo.PathCount)
            {
                newTicket.SystemX += val;
                UpdateSystemOrCombiticket(newTicket);
            }
        }
        public void SetSystemX(int val, TicketStates state)
        {
            if (val > 0 || TipListInfo.MinCombination < TipListInfo.SystemX + TipListInfo.PathCount)
            {
                TipListInfo.SystemX = val;
                UpdateSystemOrCombiticket(null);
            }
        }


        private static decimal GetMax(decimal x, decimal y)
        {
            return x > y ? x : y;
        }
    }

    public interface IDataBinding
    {
        void ChangeSystemX(int val, TicketStates state, Ticket newTicket);
        TipListInfo TipListInfo { get; }
        void UpdateSystemOrCombiticket(Ticket ticket);
    }
}