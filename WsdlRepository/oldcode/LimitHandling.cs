using System;
using IocContainer;
using Ninject;
using Shared;

namespace WsdlRepository.oldcode {
    /// <summary>
    /// checks limits on Odds and Tournaments, locks offer if a limit is hit and StationSr.LockOfferOnLimit is true and sends warning to server
    /// </summary>
    public class LimitHandling
    {
        //private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(LimitHandling));

        // Repositories

        private static IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }


		private static decimal MaxWinSystemBet(Ticket ticket)
		{
            return StationRepository.GetMaxWinSystemBet(ticket); 
		}

        private static decimal MinStakeSystemBet(Ticket ticket)
		{
            return StationRepository.GetMinStakeSystemBet(ticket); 
		}




        /// <summary>
        /// common limit for all bets (single, combi, system)
        /// </summary>
        /// <param name="maxOdd">maximum odd of the bet</param>
        /// <returns></returns>
        public static bool BetMaxOddAllowed(decimal maxOdd,Ticket ticket)
        {
            return maxOdd <= StationRepository.GetMaxOdd(ticket);
        }


        /// <summary>
        /// calculates the maximum stake for a specific singleBet is according to its Odd
        /// </summary>
        /// <param name="maxOdd">the odd of the singleBet</param>
        /// <returns>the value of the maximum stake allowed</returns>
        public static decimal SingleMaxStake(decimal maxOdd, Ticket ticketToCalculate)
        {
            return Math.Min(StationRepository.GetMaxWinSingleBet((ticketToCalculate)) / maxOdd, StationRepository.GetMaxStakeSingleBet(ticketToCalculate));

        }

        /// <summary>
        /// checks if Y of system bet x/Y is allowed because of StationSr.MaxSystemBet
        /// </summary>
        /// <param name="numSystemY">the Y value of System X/Y</param>
        /// <returns>true if ok</returns>
        public static bool SystemBetYAllowed(int numSystemY,Ticket ticket)
        {
            return (numSystemY <= StationRepository.GetMaxSystemBet(ticket));
        }




        /// <summary>
        /// Checks if the totalOdd of a systemBet is smaller than BetMaxOddAllowed and maxWinSystemBet is not larger than allowed
        /// </summary>
        /// <param name="maxOdd">the maxOdd of the system</param>
        /// <param name="multiWayOddFactor">the multiplication-factor of the multiWayTips</param>
        /// <param name="rowCount">Number of rows of the system</param>
        /// <returns></returns>
        public static bool SystemBetOddAllowed(decimal maxOdd, decimal multiWayOddFactor, decimal rowCount,Ticket ticket)
        {
            bool retVal = ((MinStakeSystemBet(ticket) * maxOdd * multiWayOddFactor / rowCount) < MaxWinSystemBet(ticket));
            return retVal && BetMaxOddAllowed(maxOdd,ticket);
        }




    }
}
