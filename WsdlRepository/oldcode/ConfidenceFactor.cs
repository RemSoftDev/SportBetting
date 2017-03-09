using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;
using IocContainer;
using SportRadar.Common;
using SportRadar.DAL.NewLineObjects;
using IocContainer;
using SharedInterfaces;
using Shared;
using Ninject;
using SportRadar.DAL.ViewObjects;

namespace WsdlRepository.oldcode
{
    public class ConfidenceFactor : IConfidenceFactor
    {

        private ILineSr _LineSr;
        public ILineSr MyLineSr
        {
            get
            {
                return _LineSr ?? (_LineSr = IoCContainer.Kernel.Get<ILineSr>());
            }
        }
        public decimal CalculateFactor(Ticket ticket)
        {

            //return 1000000;
            //should not work for virtual sports
            if (ticket.TipItems.Count > 0)
            {
                SportRadar.DAL.OldLineObjects.eServerSourceType type = ticket.TipItems.ToSyncList().ElementAt(0).Odd.BetDomain.Match.SourceType;
                if (type == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl || type == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc)
                    return Decimal.MaxValue;
            }

            string localDelimeter = "*";
            string serverDelimeter = "|";

            decimal matchConfidenceFactor = 0;
            decimal maxBetLiability = 0;
            decimal marketConfidenceFactor = 0;

            decimal userConfidenceFactor = 1;
            if (ticket.User != null)
                userConfidenceFactor = ticket.User.UserConfidenceRaiting;

            foreach (ITipItemVw tipItemVw in ticket.TipItems.ToSyncList())
            {
                string matchId = tipItemVw.Match.MatchId.ToString();
                string oddTag = tipItemVw.Odd.OddTag.Value;
                string tournamentId = tipItemVw.Match.MatchView == null ? "" : tipItemVw.Match.MatchView.TournamentView.LineObject.SvrGroupId.ToString();
                string btrTournamentId = "";
                if (tipItemVw.IsLiveBet && tipItemVw.Match.MatchView != null)
                {
                    var btrid = (SportRadarId.FromLongId(tipItemVw.Match.MatchView.TournamentView.LineObject.Id).ObjectId - 2) >> 6;

                    btrTournamentId = btrid.ToString();
                }

                string sportId = tipItemVw.Match.MatchView == null ? "" : tipItemVw.Match.MatchView.SportView.LineObject.SvrGroupId.ToString();
                if (tipItemVw.IsLiveBet && tipItemVw.Match.MatchView != null)
                {
                    var btrid = (SportRadarId.FromLongId(tipItemVw.Match.MatchView.SportView.LineObject.Id).ObjectId - 3) >> 6;
                    var group = MyLineSr.GetAllGroups().Where(x => x.GroupSport != null).Where(x => x.SvrGroupId > 0).FirstOrDefault(x => x.GroupSport.BtrSportId == btrid);
                    if (@group != null)
                    {

                        sportId = @group.SvrGroupId.ToString();
                    }
                }
                LiabilityLn liab = MyLineSr.GetAllLiabilities(matchId + localDelimeter + LineSr.MATCH_FACTOR);
                if (liab != null)
                {
                    decimal factor = liab.factor;

                    if (factor < matchConfidenceFactor || matchConfidenceFactor == 0)
                    {

                        matchConfidenceFactor = factor;
                    }
                }
                else
                {
                    if (1 < matchConfidenceFactor)
                        matchConfidenceFactor = 1;
                }

                liab = MyLineSr.GetAllLiabilities(tournamentId + localDelimeter + LineSr.TOURN_CONF_RATING);
                if (liab != null)
                {
                    decimal factor = liab.factor;

                    LiabilityLn franchisorRating = MyLineSr.GetAllLiabilities(factor.ToString() + localDelimeter + LineSr.CONF_RATING_VALUES);

                    if (franchisorRating != null)
                    {
                        decimal franfactor = tipItemVw.IsLiveBet ? franchisorRating.livefactor * franchisorRating.factor : franchisorRating.factor;
                        if (franfactor < maxBetLiability || maxBetLiability == 0)
                            maxBetLiability = franfactor;
                    }
                }
                else
                {
                    liab = MyLineSr.GetAllLiabilities(btrTournamentId + localDelimeter + LineSr.TOURN_CONF_RATING);
                    if (liab != null)
                    {
                        decimal factor = liab.factor;

                        LiabilityLn franchisorRating = MyLineSr.GetAllLiabilities(factor.ToString() + localDelimeter + LineSr.CONF_RATING_VALUES);

                        if (franchisorRating != null)
                        {
                            decimal franfactor = tipItemVw.IsLiveBet ? franchisorRating.livefactor * franchisorRating.factor : franchisorRating.factor;
                            if (franfactor < maxBetLiability || maxBetLiability == 0)
                                maxBetLiability = franfactor;
                        }
                    }
                    else
                    {
                        LiabilityLn franchisorRating = MyLineSr.GetAllLiabilities("3" + localDelimeter + LineSr.CONF_RATING_VALUES); //default value if specific not found
                        if (franchisorRating != null)
                        {
                            decimal franfactor = tipItemVw.IsLiveBet ? franchisorRating.livefactor * franchisorRating.factor : franchisorRating.factor;
                            if (franfactor < maxBetLiability || maxBetLiability == 0)
                                maxBetLiability = franfactor;
                        }
                    }
                }

                liab = MyLineSr.GetAllLiabilities("MATCH" + serverDelimeter + matchId + serverDelimeter + oddTag + localDelimeter + LineSr.LIMIT_FACTORS);
                if (liab == null)
                {
                    liab = MyLineSr.GetAllLiabilities("TOURNAMENT" + serverDelimeter + tournamentId + serverDelimeter + oddTag + localDelimeter + LineSr.LIMIT_FACTORS);
                    if (liab == null)
                    {
                        liab = MyLineSr.GetAllLiabilities("SPORT" + serverDelimeter + sportId + serverDelimeter + oddTag + localDelimeter + LineSr.LIMIT_FACTORS);
                        if (liab == null)
                        {
                            liab = MyLineSr.GetAllLiabilities("SPORT" + serverDelimeter + sportId + serverDelimeter + "DEFAULT" + localDelimeter + LineSr.LIMIT_FACTORS);
                            if (liab != null)
                            {
                                decimal factor = liab.factor;
                                if ((factor < marketConfidenceFactor || marketConfidenceFactor == 0))
                                    marketConfidenceFactor = factor;
                            }
                            else
                            {
                                if (1 < marketConfidenceFactor)
                                    marketConfidenceFactor = 1;
                            }
                        }
                        else
                        {
                            decimal factor = liab.factor;
                            if (factor < marketConfidenceFactor || marketConfidenceFactor == 0)
                                marketConfidenceFactor = factor;
                        }
                    }
                    else
                    {
                        decimal factor = liab.factor;

                        if (factor < marketConfidenceFactor || marketConfidenceFactor == 0)
                            marketConfidenceFactor = factor;
                    }
                }
                else
                {
                    decimal factor = liab.factor;

                    if (factor < marketConfidenceFactor || marketConfidenceFactor == 0)
                        marketConfidenceFactor = factor;
                }
            }

            if (matchConfidenceFactor == 0)
                matchConfidenceFactor = 1;

            if (marketConfidenceFactor == 0)
                marketConfidenceFactor = 1;


            return ticket.TotalOdd == 1 ? 0 : matchConfidenceFactor * maxBetLiability * marketConfidenceFactor * userConfidenceFactor / (ticket.TotalOddDisplay - 1);
        }
    }
}
