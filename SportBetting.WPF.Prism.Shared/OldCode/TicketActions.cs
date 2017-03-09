using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using IocContainer;
using Nbt.Station.Design;
using Ninject;
using Shared;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.ViewObjects;
using SportRadar.DAL.NewLineObjects;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;
using System.Globalization;

namespace SportBetting.WPF.Prism.Shared.OldCode
{
    public class TicketActions : ITicketActions
    {
        private readonly ILog Log = LogFactory.CreateLog(typeof(TicketActions));


        private IChangeTracker ChangeTracker
        {
            get { return IoCContainer.Kernel.Get<IChangeTracker>(); }
        }


        public IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }

        private IPrinterHandler _printerHandler;
        public IPrinterHandler PrinterHandler
        {
            get
            {
                return _printerHandler ?? (_printerHandler = IoCContainer.Kernel.Get<IPrinterHandler>());
            }
        }
        private IStationRepository StationRepository
        {
            get { return IoCContainer.Kernel.Get<IStationRepository>(); }
        }
        private ITicketSaveHandler TicketSaveHandler
        {
            get { return IoCContainer.Kernel.Get<ITicketSaveHandler>(); }
        }
        private IBusinessPropsHelper BusinessPropsHelper
        {
            get { return IoCContainer.Kernel.Get<IBusinessPropsHelper>(); }
        }

        private string StationTyp
        {
            get { return StationRepository.StationTyp; }
        }


        public TicketWS CreateNewTicketWS( Ticket newticket)
        {
            var rTicketWs = new TicketWS();

            List<ITipItemVw> ti = newticket.TipItems.ToSyncList().Where(x => x.IsChecked).ToList();


            try
            {
                PasswordGenerator PWGen = new PasswordGenerator();
                //if (string.IsNullOrEmpty(rTicketWs.TicketNbr))

                //try
                //{
                //    long number, creditnumber, transactionnumber;
                //    creditnumber = WsdlRepository.GetBusinessProps(StationRepository.StationNumber, out number, out transactionnumber);
                //    BusinessPropsHelper.Initialize(StationRepository.StationNumber, new BusinessProps(number, creditnumber, transactionnumber));
                //}
                //catch (Exception excp)
                //{
                //    Log.Error(ExcpHelper.FormatException(excp, "Initialize(sStationNumber = '{0}', {1}) ERROR", StationRepository.StationNumber, ""));

                //}
                rTicketWs.ticketNbr = BusinessPropsHelper.GenerateNextTicketNumber();


                //rTicketWs.TicketNbr = StationSettings.GetSettings.StationNumber+DateTime.Now.ToString("yy")+StationSettings.GetSettings.NewTicketNumber.ToString("0000000");
                //rTicketWs.TicketNbr = PWGen.Generate(14, 14, true);
                rTicketWs.checkSum = PWGen.Generate(4, 4, true);


                rTicketWs.paidBy = StationTyp;
                rTicketWs.paidTime = DateTime.Now;
                rTicketWs.paid = false;
                rTicketWs.acceptedBy = StationTyp;
                rTicketWs.acceptedTime = DateTime.Now;
                rTicketWs.stake = newticket.Stake;
                rTicketWs.cancelledTime = DateTimeUtils.DATETIMENULL;
                rTicketWs.enablePayTime = DateTime.MaxValue;
                rTicketWs.wonExpireTime = DateTimeUtils.DATETIMENULL;
                rTicketWs.userId = (int)ChangeTracker.CurrentUser.AccountId;

                //string bonusX = newticket.BonusValue.ToString(CultureInfo.InvariantCulture);
                //string manFeeX = newticket.ManipulationFeeValue.ToString(CultureInfo.InvariantCulture);

                //string a = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;

                //if (bonusX.IndexOf(a) != -1)
                //{
                //    bonusX = bonusX.Substring(0, bonusX.IndexOf(a) + 3);
                //    newticket.BonusValue = Decimal.Parse(bonusX, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                //}
                //if (manFeeX.IndexOf(a) != -1)
                //{
                //    manFeeX = manFeeX.Substring(0, manFeeX.IndexOf(a) + 3);
                //    newticket.ManipulationFeeValue = Decimal.Parse(manFeeX, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                //}

                newticket.BonusValue = ChangeTracker.TruncateDecimal(newticket.BonusValue);
                newticket.ManipulationFeeValue = ChangeTracker.TruncateDecimal(newticket.ManipulationFeeValue);

                rTicketWs.manipulationFeeValue = ChangeTracker.TruncateDecimal(newticket.ManipulationFeeValue);// newticket.ManipulationFeeValue;
                rTicketWs.superBonusValue = ChangeTracker.TruncateDecimal(newticket.BonusValue);

                BetWS bets = new BetWS();
                bets.systemX = bets.systemY = newticket.NumberOfBets;

                switch (newticket.TicketState)
                {
                    case TicketStates.Multy:

                        var dup = newticket.TipItems.ToSyncList().GroupBy(x => new { x.Match.MatchId }).Select(group => new { Name = group.Key, Count = group.Count() }).OrderByDescending(x => x.Count);
                        int duplicatesCount = 0;
                        foreach (var x in dup)
                        {
                            if (x.Count > 1)
                                duplicatesCount += x.Count;
                        }
                        int baseSize = newticket.NumberOfBets - duplicatesCount;
                        if (duplicatesCount > 0 && baseSize > 0) //means there are ways
                            bets.systemX = bets.systemY = baseSize;

                        bets.betType = Bet.BET_TYPE_COMBI;
                        break;
                    case TicketStates.System:
                        bets.betType = Bet.BET_TYPE_SYSTEM;
                        bets.systemX = newticket.SystemX;
                        bets.systemY = newticket.SystemY;
                        break;
                    default:
                        bets.betType = Bet.BET_TYPE_SINGLE;
                        break;
                }
                bets.maxOdd = newticket.TotalOddDisplay;
                //bets.MaxWin = rTicketWs.Stake * bets.MaxOdd;
                bets.maxWin = newticket.CurrentTicketPossibleWin;
                bets.stake = rTicketWs.stake;
                bets.isMaxOddBet = newticket.IsMaxOddBet;

                rTicketWs.bets = new BetWS[] { bets };
                rTicketWs.superBonus = StationRepository.GetBonusValueForBets(newticket) / 100 + 1;
                rTicketWs.manipulationFee = StationRepository.GetManipulationFeePercentage(newticket);

                Dictionary<long, int> matchIDCountDictionary = new Dictionary<long, int>();

                foreach (TipItemVw t in ti)
                {
                    long iMatchCode = t.Match.MatchId;

                    if (matchIDCountDictionary.ContainsKey(iMatchCode))
                    {
                        matchIDCountDictionary[iMatchCode]++;
                    }
                    else
                    {
                        matchIDCountDictionary.Add(iMatchCode, 1);
                    }
                }

                int liveBetTipCount = 0;
                int sportBetTipCount = 0;
                var tips2BetMulti = new ObservableCollection<TipWS>();
                var bankTips = new ObservableCollection<TipWS>();
                foreach (TipItemVw t in ti)
                {
                    if (t.Odd != null)
                    {
                        if (t.Odd.IsLiveBet.Value)
                        {
                            liveBetTipCount++;
                        }
                        else
                        {
                            sportBetTipCount++;
                        }
                    }
                    TipWS tip = new TipWS();
                    tip.bank = t.IsBank;

                    if (t.Odd != null)
                    {
                        tip.odd = t.Value;
                        tip.svrOddID = t.Odd.OutcomeId; //TODO: Wurde auf ServerOddID umgewandelt *g* 05.03.2008 by GMU

                        SportRadar.DAL.OldLineObjects.eServerSourceType sType = t.Odd.BetDomain.Match.SourceType;
                        if (sType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrLive || sType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl || sType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc)
                        {
                            tip.svrOddID = tip.svrOddID * -1;
                        }

                        tip.betDomainNumber = t.Odd.BetDomain.BetDomainNumber.Value;
                        tip.matchCode = t.Odd.BetDomain.Match.Code.Value;
                        tip.oddTag = t.Odd.OddTag.Value;
                    }

                    if (tip.bank || matchIDCountDictionary[t.Match.MatchId] != 1)
                    {
                        tip.bankGroupID = t.Match.MatchId;
                        tip.bank = matchIDCountDictionary[t.Match.MatchId] == 1; //Mehrwege haben kein Bank-Flag
                        if (newticket.TicketState == TicketStates.Multy && matchIDCountDictionary[t.Match.MatchId] != 1)
                        {
                            bets.betType = Bet.BET_TYPE_COMBIPATH;
                        }
                        else if (newticket.TicketState == TicketStates.System && matchIDCountDictionary[t.Match.MatchId] != 1)
                        {
                            bets.betType = Bet.BET_TYPE_SYSTEMPATH;
                        }
                        tips2BetMulti.Add(tip); //banken von system mit banken
                    }
                    else
                    {
                        bankTips.Add(tip); //system, kombi, einzeln
                    }
                }
                bets.tips2BetMulti = tips2BetMulti.ToArray();
                bets.bankTips = bankTips.ToArray();

                if (newticket.TipItems[0].Match.MatchView.LineObject.SourceType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVfl)
                {
                    rTicketWs.ticketTyp = Ticket.TICKET_TYP_VFL;
                }
                else if (newticket.TipItems[0].Match.MatchView.LineObject.SourceType == SportRadar.DAL.OldLineObjects.eServerSourceType.BtrVhc)
                {
                    rTicketWs.ticketTyp = Ticket.TICKET_TYP_VHC;
                }
                else if (liveBetTipCount > 0 && sportBetTipCount == 0)
                {
                    rTicketWs.ticketTyp = Ticket.TICKET_TYP_LIVEBET;
                }
                else if (liveBetTipCount == 0 && sportBetTipCount > 0)
                {
                    rTicketWs.ticketTyp = Ticket.TICKET_TYP_SPORTBET;
                }
                else
                {
                    rTicketWs.ticketTyp = Ticket.TICKET_TYP_BOTH;
                }

                int rowCount = 1;
                int[,] temp;
                if (newticket.TicketState == TicketStates.System)
                {
                    OddUtilities.SetPermutations(out temp, bets.systemY, bets.systemX);
                    rowCount = temp.GetLength(0);
                }
                Dictionary<int, int> tempMatchCodeCountDict = new Dictionary<int, int>();
                foreach (TipWS t in bets.tips2BetMulti)
                {
                    if (tempMatchCodeCountDict.ContainsKey(t.matchCode))
                    {
                        tempMatchCodeCountDict[t.matchCode]++;
                    }
                    else
                    {
                        tempMatchCodeCountDict.Add(t.matchCode, 1);
                    }
                }
                foreach (int curCount in tempMatchCodeCountDict.Values)
                {
                    rowCount *= curCount;
                }
                bets.rows = rowCount;
                return rTicketWs;
            }
            catch (Exception ex)
            {
                WriteRemoteError2Log(ex.Message, 1, ex.GetType().ToString(), NbtLogSr.MSG_TERMINAL);
                return null;
            }


        }

        [WsdlServiceSyncAspectSilent]
        private void WriteRemoteError2Log(string message, int i, string type, string msgTerminal)
        {
            WsdlRepository.WriteRemoteError2Log(message, 1, type, msgTerminal);
        }


        public int SaveTicket(ref TicketWS ticket, User user)
        {

            if (ticket != null && ticket.bets != null && ticket.bets.Length > 0 && ticket.bets[0] != null && (ticket.ticketTyp == Ticket.TICKET_TYP_BOTH || ticket.ticketTyp == Ticket.TICKET_TYP_LIVEBET))
            {
                if (ticket.bets[0].betType == Bet.BET_TYPE_SINGLE)
                {
                    Thread.Sleep(StationRepository.SngLiveBetTicketAcceptTime);
                }
                else
                {
                    Thread.Sleep(StationRepository.CombiLiveBetTicketAcceptTime);
                }
            }

            return TicketSaveHandler.SaveTicket(ticket, StationRepository.StationNumber, false, user);
        }

        public int StoreTicket(User user, ref TicketWS ticket, Ticket newTicket, string pin)
        {
            ticket = CreateNewTicketWS(newTicket);

            if (ticket != null && ticket.bets != null && ticket.bets.Length > 0 && ticket.bets[0] != null && newTicket.TipItems.ToSyncList().Any(x => x.IsLiveBet))
            {
                if (ticket.bets[0].betType == Bet.BET_TYPE_SINGLE)
                {
                    Thread.Sleep(StationRepository.SngLiveBetTicketAcceptTime);
                }
                else
                {
                    Thread.Sleep(StationRepository.CombiLiveBetTicketAcceptTime);
                }
            }

            TicketSaveHandler th = new TicketSaveHandler();
            var saveRet = th.StoreTicket(user, ticket, StationRepository.StationNumber, false, pin, newTicket.TipItems.ToSyncList().Any(x => x.IsLiveBet));

            return saveRet;
        }


        public TicketWS SaveTicket(out int saveRet, TicketWS ticket, User user)
        {
            saveRet = SaveTicket(ref ticket, user);

            return ticket;
        }

        public TicketWS StoreTicket(User user, string pin, out int saveRet, TicketWS ticket, Ticket newTicket)
        {
            saveRet = StoreTicket(user, ref ticket, newTicket, pin);


            return ticket;
        }


        public bool PrintTicket(TicketWS ticket, bool isDuplicate)
        {
            return PrinterHandler.PrintTicket(ticket, isDuplicate);
        }

        public bool PrintStoredTicket(TicketWS ticket, string pin, string stationNumber, DateTime expireDate, long userId)
        {
            return PrinterHandler.PrintStoredTicket(ticket, pin, stationNumber, expireDate, userId.ToString());
        }

    }
}