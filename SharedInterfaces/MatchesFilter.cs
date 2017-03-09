using System;
using System.Linq;
using SportRadar.Common.Entities;
using SportRadar.Common.Extensions;
using SportRadar.DAL.NewLineObjects;

namespace SharedInterfaces
{
    public class MatchesFilter
    {
        public long[] TournamentIds { get; set; }
        public Range<DateTime> DateRange { get; set; }
        /// <summary>
        /// Filter for the 1X2 odds
        /// </summary>
        public Range<decimal> BaseBetOddsRange { get; set; }

        public static bool IsMatchValid(MatchLn match, MatchesFilter filter)
        {
            if (match == null)
            {
                return false;
            }
            if (filter == null)
            {
                return true;
            }

            // filter by date
            if (filter.DateRange != null
                && filter.DateRange.IsInRange(match.MatchView.StartDate) == false)
            {
                return false;
            }

            // filter by tournament ids
            if (filter.TournamentIds != null
                && filter.TournamentIds.Length > 0)
            {
                if (match.MatchView.TournamentView == null
                    || match.MatchView.TournamentView.LineObject == null
                    || filter.TournamentIds.Contains(match.MatchView.TournamentView.LineObject.Id) == false)
                {
                    return false;
                }
            }

            // filter by base odds
            if (filter.BaseBetOddsRange != null)
            {
                var baseBetDomain = match.GetBaseBetDomain();

                if (baseBetDomain == null
                    || baseBetDomain.Odds == null)
                {
                    return false;
                }

                // take only valid odds >= 1.0
                var validOdds = baseBetDomain.Odds.Where(x => x.Value.Value >= 1M);
                // The smallest valid odd must be in range of the filter in order to pass
                if (validOdds.Count() <= 1
                    || filter.BaseBetOddsRange.IsInRange(validOdds.OrderBy(x => x.Value.Value).First().Value.Value) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}