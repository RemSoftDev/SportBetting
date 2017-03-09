using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using IocContainer;
using Nbt.Services.Spf.Printer;
using Ninject;
using Shared;
using SharedInterfaces;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Annotations;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportBetting.WPF.Prism.OldCode;
using SportRadar.DAL.ViewObjects;
using TimeZoneTest;
using TranslationByMarkupExtension;
using WsdlRepository;
using WsdlRepository.WsdlServiceReference;
using Nbt.Services.Scf.CashIn.Validator.CCTalk;

namespace SportBetting.WPF.Prism.Shared.Models.Repositories
{

    public enum eStationType
    {
        Undefined = 0,
        Terminal = 1,
        Outlet = 2
    }
    [ServiceAspect]
    public sealed class StationRepository : IStationRepository, INotifyPropertyChanged
    {
        private static ILog Log = LogFactory.CreateLog(typeof(StationRepository));

        public decimal UserConfidenceRaiting { get { return ChangeTracker.CurrentUser.UserConfidenceFactor; } }
        private static string auto_restart_time = "NONE";
        private static volatile bool ar_watcher_created = false;
        private static int wait_after_startup = 0;
        private static int station_is_offline_counter = 0;

        //private static log4net.ILog m_Logger = log4net.LogManager.GetLogger(typeof(StationRepository));

        private string m_sCurrency = " EUR";

        private IWsdlRepository WsdlRepository
        {
            get { return IoCContainer.Kernel.Get<IWsdlRepository>(); }
        }

        private IStationSettings StationSettings
        {
            get { return IoCContainer.Kernel.Get<IStationSettings>(); }
        }




        public string LastPrintedObjects
        {
            get
            {
                return GetStationAppConfigValue("LastPrintedObjects").ValueString;
            }
            set
            {
                SetStationAppConfigValue<string>("LastPrintedObjects", value);
            }
        }

        private string _layoutName = null;
        public string LayoutName
        {
            get
            {
                if (_layoutName == null)
                    _layoutName = GetStationAppConfigValue("LayoutName").ValueString;
                return _layoutName;
            }
            set
            {
                SetStationAppConfigValue<string>("LayoutName", value);
            }
        }

        private bool? _isTerminal = null;
        public bool IsCashier
        {
            get
            {
                if (!_isTerminal.HasValue)
                    _isTerminal = (GetStationAppConfigValue("IsCashier").ValueString == true.ToString());
                return _isTerminal.Value;
            }
            set
            {
                SetStationAppConfigValue<string>("IsCashier", value.ToString());
            }
        }

        public uid GetUid(User user)
        {
            uid uid = GetBasicUid();
            uid.session_id = user.SessionId;
            uid.account_id = user.AccountId.ToString();
            return uid;
        }


        public uid GetUid(long accountId)
        {
            uid uid = GetBasicUid();
            uid.account_id = accountId.ToString();
            return uid;
        }

        public uid GetBasicUid()
        {
            uid uid = new uid();
            uid.location_id = LocationID;
            uid.station_id = StationNumber;
            uid.franchisor_id = FranchisorID;
            return uid;
        }


        private void TimerThread()
        {
            TimeSpan time = new TimeSpan(0, 0, 1, 0);
            const int RESTART_WIN_NOW = 1;
            const int RESTART_WIN_SOON = 2;
            const int RESTART_APP_NOW = 3;
            const int RESTART_APP_SOON = 4;

            bool IsWarned = false;
            bool IsRestartRequested = false;

            int hours = 0;
            int minutes = 0;

            while (true)
            {
                try
                {
                    while (true)
                    {
                        lock (auto_restart_time)
                        {
                            int pos = auto_restart_time.IndexOf(":");
                            if (pos > 0)
                            {
                                int len = auto_restart_time.Length;
                                bool result = int.TryParse(auto_restart_time.Substring(0, pos), out hours);
                                result &= int.TryParse(auto_restart_time.Substring(pos + 1, len - pos - 1), out minutes);
                                if (!result)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        TimeSpan restartTime = TimeSpan.FromMinutes(60 * hours + minutes);
                        TimeSpan dif = restartTime - DateTime.Now.TimeOfDay;
                        if (Math.Abs(dif.TotalSeconds) <= 60 || Math.Abs(dif.TotalSeconds) >= 86340) //86400
                        {
                            if (!IsWarned)
                            {
                                break;
                            }
                        }
                        if (Math.Abs(dif.TotalSeconds) <= 120 || Math.Abs(dif.TotalSeconds) >= 86280)
                        {
                            if (!IsWarned)
                            {
                                IsWarned = true;
                                // Mediator.SendMessage<int>(RESTART_APP_SOON, MsgTag.CheckRestart);
                            }
                            else if (Math.Abs(dif.TotalSeconds) <= 60 || Math.Abs(dif.TotalSeconds) >= 86340)
                            {
                                if (!IsRestartRequested)
                                {
                                    IsRestartRequested = true;
                                    int adjustment = 0;
                                    if (Math.Abs(dif.TotalSeconds) <= 60 && dif.TotalSeconds > 0)
                                    {
                                        adjustment = (int)dif.TotalSeconds * 1000;
                                    }
                                    if (Math.Abs(dif.TotalSeconds) >= 86340 && dif.TotalSeconds < 0)
                                    {
                                        adjustment = (86400 + (int)dif.TotalSeconds) * 1000;

                                    }
                                    if (adjustment > 15000 && adjustment < 59000)
                                    {
                                        System.Threading.Thread.Sleep(adjustment);
                                    }
                                    Mediator.SendMessage<int>(RESTART_APP_NOW, MsgTag.CheckRestart);

                                }
                            }
                        }
                        if (IsRestartRequested || IsWarned)
                        {
                            if (Math.Abs(dif.TotalMinutes) > 3 && Math.Abs(dif.TotalMinutes) < 1437)
                            {
                                IsRestartRequested = false;
                                IsWarned = false;
                            }
                        }
                        break;

                    }

                }
                catch
                {
                }
                System.Threading.Thread.Sleep(time);
            }
        }


        public enum StationState : int
        {
            Locked = 0,
            Active = 1,
            WithoutOdds = 2,
            TestMode = 3,
            CashLocked = 4
        }

        public bool IsImmediateWithdrawEnabled { get; set; }
        public bool IsCreditNoteImmediatelyPaid { get; set; }

        public bool AllowVfl
        {
            get { return GetStationAppConfigValue("AllowVfl").ValueInt == 1; }
            set { SetStationAppConfigValue<int>("AllowVfl", value ? 1 : 0); OnPropertyChanged(); }
        }

        public bool AllowVhc
        {
            get { return GetStationAppConfigValue("AllowVhc").ValueInt == 1; }
            set { SetStationAppConfigValue<int>("AllowVhc", value ? 1 : 0); OnPropertyChanged(); }
        }

        private bool _allowLive = true;
        public bool AllowLive
        {
            get { return _allowLive && IsLiveMatchEnabled; }
            set { _allowLive = value; }
        }

        private int _timeZoneOffset = -12;
        public int TimeZoneOffset
        {
            get { return _timeZoneOffset; }
            set { _timeZoneOffset = value; OnPropertyChanged(); }
        }

        public bool IsPrematchEnabled
        {
            get
            {
                return GetStationAppConfigValue("IsPrematchEnabled").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("IsPrematchEnabled", value ? 1 : 0);
                OnPropertyChanged();
                OnPropertyChanged("ResultsVisible");
            }
        }
        public bool AuthorizedTicketScanning
        {
            get
            {
                return GetStationAppConfigValue("AuthorizedTicketScanning").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("AuthorizedTicketScanning", value ? 1 : 0);
                OnPropertyChanged();
            }
        }

        private bool _responsibleGaming = false;
        public bool ResponsibleGaming
        {
            get
            {
                return _responsibleGaming;
            }
            set
            {
                if (_responsibleGaming != value)
                {
                    _responsibleGaming = value;
                    OnPropertyChanged("ResponsibleGaming");
                }
            }
        }

        public int CardBarcodeLen
        {
            get { return _cardBarcodeLen; }
            set { _cardBarcodeLen = value; }
        }

        public bool IsLiveMatchEnabled
        {
            get
            {
                return GetStationAppConfigValue("IsLiveMatchEnabled").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("IsLiveMatchEnabled", value ? 1 : 0);
                OnPropertyChanged();
                OnPropertyChanged("ResultsVisible");
            }
        }

        public bool ResultsVisible
        {
            get
            {
                return IsPrematchEnabled || IsLiveMatchEnabled;
            }
        }

        private bool? isidcardEnabled;
        public bool IsIdCardEnabled
        {
            get
            {
                if (isidcardEnabled == null)
                    isidcardEnabled = GetStationAppConfigValue("IsIdCardEnabled").ValueInt == 1;
                return isidcardEnabled.Value;
            }
            set
            {
                SetStationAppConfigValue<int>("IsIdCardEnabled", value ? 1 : 0);
                //OnPropertyChanged();
            }
        }

        public bool IsAutoLogoutEnabled
        {
            get
            {
                return GetStationAppConfigValue("IsAutoLogoutEnabled").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("IsAutoLogoutEnabled", value ? 1 : 0);
                OnPropertyChanged();
            }
        }

        public bool IsStoreTicketEnabled
        {
            get
            {
                return GetStationAppConfigValue("IsStoreTicketEnabled").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("IsStoreTicketEnabled", value ? 1 : 0);
                OnPropertyChanged();
            }
        }

        public string StationTyp { get { return "Terminal"; } }
        public string LiveStreamFeedUrl { get { return ConfigurationSettings.AppSettings["LiveStreamFeedUrl"]; } }

        public formField[] GetRegistrationForm()
        {
            try
            {
                if (DateTime.Now - CashedREgistrationFormUpdateTime > new TimeSpan(0, 10, 0))
                {
                    CachedRegistrationFrom = WsdlRepository.GetRegistrationForm().fields;
                    if (CachedRegistrationFrom.Length > 0)
                        CashedREgistrationFormUpdateTime = DateTime.Now;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
            return CachedRegistrationFrom;
        }

        protected formField[] CachedRegistrationFrom { get; set; }
        public DateTime CashedREgistrationFormUpdateTime { get; set; }

        public string StationName { get; set; }
        public string FranchiserName { get; set; }
        public string LocationName { get; set; }
        private Dictionary<string, string> cultureInfos;
        public Dictionary<string, string> CultureInfos
        {
            get
            {
                if (cultureInfos == null)
                {
                    cultureInfos = new Dictionary<string, string>();
                    var infosStr = ConfigurationSettings.AppSettings["CultureInfos"];
                    foreach (var info in infosStr.Split(';'))
                    {
                        var infoArr = info.Split(',');
                        cultureInfos.Add(infoArr[0], infoArr[1]);
                    }
                }
                return cultureInfos;
            }
        }

        private bool _enableLiveStreaming = false;
        public bool EnableLiveStreaming
        {
            get { return _enableLiveStreaming; }
            set { _enableLiveStreaming = value; }
        }

        public bool DisableTransferToTerminal
        {
            get { return bool.Parse(System.Configuration.ConfigurationManager.AppSettings["DISABLE_TRANSFER_TO_TERMINAL"]); }
        }

        public bool IsStatisticsEnabled
        {
            get
            {
                return GetStationAppConfigValue("IsStatisticsEnabled").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("IsStatisticsEnabled", value ? 1 : 0);
                OnPropertyChanged();
            }
        }

        public bool IsReady { get; set; }

        public decimal PrintingLanguageSetting
        {
            get { return _printingLanguageSetting; }
            set { _printingLanguageSetting = value; }
        }

        public decimal GetMaxOdd(Ticket ticket)
        {
            return MaxOdd;
        }

        public decimal GetMinStakeCombiBet(Ticket ticket)
        {
            return MinStakeCombiBet;
        }

        public decimal GetMinStakeSystemBet(Ticket ticket)
        {
            return MinStakeSystemBet;
        }

        public decimal GetMinStakeSingleBet(Ticket ticket)
        {
            return MinStakeSingleBet;
        }

        public decimal GetMinStakePerRow(Ticket ticket)
        {
            return MinStakePerRow;
        }

        public decimal GetMaxWinSystemBet(Ticket ticket)
        {
            return MaxWinSystemBet;
        }

        public decimal GetMaxStakeSingleBet(Ticket ticket)
        {
            return MaxStakeSingleBet;
        }

        public decimal GetMaxWinSingleBet(Ticket ticket)
        {
            return MaxWinSingleBet;
        }

        public decimal GetMaxSystemBet(Ticket ticket)
        {
            return MaxSystemBet;
        }

        public decimal GetMaxStakeSystemBet(Ticket ticket)
        {
            return MaxStakeSystemBet;
        }

        public decimal GetMaxStakeCombi(Ticket ticket)
        {
            return Math.Round(MaxStakeCombi, 2);
        }

        public int GetMinCombination(Ticket ticket)
        {
            return MinCombination;
        }

        public int GetMaxCombination(Ticket ticket)
        {
            return MaxCombination;
        }

        public bool GetAllowMultiBet(Ticket ticket)
        {
            return AllowMultiBet;
        }

        private bool? _multibetAllowed = null;
        public bool AllowMultiBet
        {
            get
            {
                if (_multibetAllowed == null)
                    _multibetAllowed = GetStationAppConfigValue("AllowMultiBet").ValueInt == 1;
                return (bool)_multibetAllowed;
            }
            set
            {
                SetStationAppConfigValue<int>("AllowMultiBet", value ? 1 : 0);
            }
        }

        private int SortCombiLimitList(CombiLimitWS brX, CombiLimitWS brY)
        {
            return brX.combiSize.CompareTo(brY.combiSize);
        }

        public decimal GetMaxWinMultiBet(Ticket ticket)
        {
            int tipCount = ticket.TipItems.ToSyncList().Count(x => x.IsChecked);
            int iWaysCount = ticket.TipItems.ToSyncList().Count(x => x.IsWay);
            int iPathExtra = 0;
            int iPathsCount = PathCount(ticket.TipItems.ToSyncList(), out iPathExtra);
            tipCount -= iPathsCount + iPathExtra;


            decimal maxWin = 0;

            if (CombiLimitList != null)
            {
                List<CombiLimitWS> cls = CombiLimitList;
                cls.Sort(SortCombiLimitList);


                foreach (CombiLimitWS combiLimitWs in cls)
                {
                    if (combiLimitWs.combiSize <= tipCount)
                    {
                        maxWin = combiLimitWs.limit;
                        if (combiLimitWs.combiSize == tipCount)
                            break;
                    }
                }
            }

            return maxWin;
        }

        private int SortManipulationFeeList(CombiLimitWS brX, CombiLimitWS brY)
        {
            return brX.combiSize.CompareTo(brY.combiSize);
        }

        public decimal GetManipulationFeePercentage(Ticket ticket)
        {
            var tipitems = ticket.TipItems.ToSyncList().Where(x => x.IsChecked);
            List<CombiLimitWS> cls = CombiLimitList;
            if (CombiLimitList == null)
                return 0;
            cls.Sort(SortManipulationFeeList);
            int combisize = 0;
            int tipCount = ticket.TicketState == TicketStates.Single ? 1 : tipitems.Count(x => x.IsChecked);
            int iWaysCount = tipitems.Count(x => x.IsWay);
            int iBanksCount = tipitems.Count(x => x.IsBank);

            int iPathExtra = 0;
            int iPathsCount = PathCount(ticket.TipItems.ToSyncList(), out iPathExtra);
            switch (ticket.TicketState)
            {
                case TicketStates.System:
                    int iX = iPathsCount > 0 ? iPathsCount + (iBanksCount - iWaysCount) : iBanksCount;
                    tipCount = ticket.SystemX + iX;
                    break;
                case TicketStates.Multy:
                    tipCount -= iPathsCount + iPathExtra;
                    break;
            }

            var isLiveBet = tipitems.Where(x => x.IsChecked).Any(x => x.Odd.IsLiveBet.Value);

            while (tipCount > 0)
            {
                if (cls.Count(x => x.combiSize == tipCount) > 0)
                {
                    break;
                }
                tipCount--;

            }

            decimal manipulationFee = 0;
            foreach (CombiLimitWS combiLimitWs in cls)
            {
                if (combiLimitWs.combiSize == tipCount && combiLimitWs.combiSize > combisize)
                {
                    combisize = combiLimitWs.combiSize;
                    manipulationFee = isLiveBet ? combiLimitWs.manipulationFee_Livebet : combiLimitWs.manipulationFee;
                    if (combisize == tipCount)
                        break;
                }
            }

            return manipulationFee;
        }


        public decimal GetMaxStake(Ticket ticket)
        {
            int tipCount = ticket.TipItems.ToSyncList().Count(x => x.IsChecked);

            if (tipCount < MinCombination)
            {
                return MaxStakeCombi;
            }

            List<CombiLimitWS> clTable = CombiLimitList;

            decimal maxStake = MaxStakeCombi;
            if (clTable != null && clTable.Count > 0)
            {
                CombiLimitWS clMax = clTable[0];
                for (int i = 1; i < clTable.Count; i++)
                {
                    if (clTable[i].combiSize == tipCount && (clTable[i].combiSize > clMax.combiSize || clMax.combiSize > tipCount))
                        clMax = clTable[i];
                    if (clTable[i].combiSize == tipCount)
                        break;
                }
                maxStake = clMax.limit / (ticket.TotalOdd - 1);
            }
            return Math.Floor(Math.Min(maxStake, MaxStakeCombi));
        }

        private int SortBonusList(BonusRangeWS brX, BonusRangeWS brY)
        {
            return brX.tipSize.CompareTo(brY.tipSize);
        }

        public decimal GetBonusValueForBets(Ticket ticket)
        {
            var tipitems = ticket.TipItems.ToSyncList().Where(x => x.IsChecked).ToList();
            int tipCount = tipitems.Where(x => x.Value >= BonusFromOdd).Count(x => x.IsChecked);
            int iPathExtra = 0;
            int iPathsCount = PathCount(ticket.TipItems.ToSyncList(), out iPathExtra);


            switch (ticket.TicketState)
            {
                case TicketStates.System:

                    if (tipitems.Count(x => x.IsWay) > 0)
                    {
                        // create list of valueables items
                        Dictionary<long, TipItemVw> dictX = new Dictionary<long, TipItemVw>();
                        foreach (TipItemVw tipItemVw in tipitems)
                        {
                            if (!dictX.Keys.Contains(tipItemVw.Match.MatchId))
                            {
                                if (tipItemVw.Value >= BonusFromOdd)
                                {
                                    dictX.Add(tipItemVw.Match.MatchId, tipItemVw);
                                }
                                else
                                {
                                    // add empty value to prevent future odds insertions from same Match
                                    dictX.Add(tipItemVw.Match.MatchId, null);
                                }
                            }
                            else
                            {
                                if (tipItemVw.Value >= BonusFromOdd && dictX[tipItemVw.Match.MatchId] != null)
                                {
                                    if (dictX[tipItemVw.Match.MatchId].Value < tipItemVw.Value)
                                    {
                                        // take bigger Odd Value from ways into calculations
                                        dictX[tipItemVw.Match.MatchId] = tipItemVw;
                                    }
                                }
                            }
                        }

                        var countAll = dictX.Count(x => x.Value != null);
                        var countWays = dictX.Count(x => x.Value != null && x.Value.IsWay);
                        var countBanks = dictX.Count(x => x.Value != null && !x.Value.IsWay && x.Value.IsBank);
                        var bonusSuitable = tipitems.Where(x => x.IsBank == false).Where(x => x.IsWay == false).Where(x => x.Value >= BonusFromOdd).Count() > ticket.SystemX ? ticket.SystemX : tipitems.Where(x => x.IsBank == false).Where(x => x.IsWay == false).Where(x => x.Value >= BonusFromOdd).Count();
                        var notSuitable = tipitems.Where(x => x.IsBank == false).Where(x => x.IsWay == false).Where(x => x.Value < BonusFromOdd).Count();
                        var countSystem = bonusSuitable - notSuitable > ticket.SystemX ? ticket.SystemX : bonusSuitable - notSuitable;
                        if (countSystem < 0)
                            countSystem = 0;
                        tipCount = countSystem + countWays + countBanks;

                    }
                    else
                    {
                        int iAllTipsBank = tipitems.Count(x => x.IsChecked & x.IsBank);
                        int iAllTipsNoBank = tipitems.Count(x => x.IsChecked & !x.IsBank);

                        int iNoBonusTipsBank = tipitems.Count(x => x.Odd.Value.Value < BonusFromOdd && x.IsChecked & x.IsBank);
                        int iNoBonusTipsNoBank = tipitems.Count(x => x.Odd.Value.Value < BonusFromOdd && x.IsChecked & !x.IsBank);

                        int iAllSuitable = iAllTipsNoBank;
                        int iBonusSuitable = iAllTipsBank - iNoBonusTipsBank;

                        int iSystemX = iAllSuitable < ticket.SystemX ? iAllSuitable : ticket.SystemX;
                        int iFinally = iSystemX + iBonusSuitable - iNoBonusTipsNoBank;

                        tipCount = iFinally;
                    }

                    break;
                default:
                    tipCount = SuitableWaysCount(ticket.TipItems.ToSyncList());
                    break;

            }

            List<BonusRangeWS> bonuses = BonusRangeList;
            bonuses.Sort(SortBonusList);

            decimal bonus = 0;

            foreach (BonusRangeWS bonusRangeWs in bonuses)
            {
                if (bonusRangeWs.tipSize <= tipCount)
                {
                    bonus = bonusRangeWs.bonus;
                    if (bonusRangeWs.tipSize == tipCount)
                        break;
                }
            }
            return bonus;
        }

        public int SuitableWaysCount(IList<ITipItemVw> TipItems)
        {
            // PathExtra used to help count up all odds that are "participating" in multiway 
            // e.g. if user selects 3 odds for 1 match, then PathCount = 1 and PathExtra = 1
            // useful for GetBonusValueForBets() 

            Dictionary<long, IList<ITipItemVw>> bdX = new Dictionary<long, IList<ITipItemVw>>();
            foreach (TipItemVw tipItemVw in TipItems.Where(x => x.IsChecked))
            {
                if (!bdX.ContainsKey(tipItemVw.BetDomain.BetDomainId))
                {
                    bdX.Add(tipItemVw.BetDomain.BetDomainId, new List<ITipItemVw>() { tipItemVw });
                }
                else
                {
                    bdX[tipItemVw.BetDomain.BetDomainId].Add(tipItemVw);
                }
            }
            int counter = 0;
            foreach (var betdomainToTipItem in bdX)
            {
                bool suitable = true;
                foreach (var tipItem in betdomainToTipItem.Value)
                {
                    if (tipItem.Value < BonusFromOdd)
                    {
                        suitable = false;
                        break;

                    }
                }
                if (suitable)
                    counter++;
            }
            return counter;
        }

        public int PathCount(IList<ITipItemVw> TipItems, out int PathExtra)
        {
            // PathExtra used to help count up all odds that are "participating" in multiway 
            // e.g. if user selects 3 odds for 1 match, then PathCount = 1 and PathExtra = 1
            // useful for GetBonusValueForBets() 

            List<long> bdX = new List<long>();
            List<long> bdPaths = new List<long>();
            PathExtra = 0;
            foreach (TipItemVw tipItemVw in TipItems.Where(x => x.IsChecked))
            {
                if (!bdX.Contains(tipItemVw.BetDomain.BetDomainId))
                {
                    bdX.Add(tipItemVw.BetDomain.BetDomainId);
                }
                else
                {
                    if (!bdPaths.Contains(tipItemVw.BetDomain.BetDomainId))
                    {
                        bdPaths.Add(tipItemVw.BetDomain.BetDomainId);
                    }
                    else
                    {
                        PathExtra++;
                    }
                }
            }
            return bdPaths.Count;
        }

        public List<UpdateRecordSr> UpdateLocalization(long lastUpdateFileId)
        {
            throw new NotImplementedException();
        }

        public List<UpdateRecordSr> UpdateLine(long lastUpdateFileId, DateTime lastUpdate, out int? total)
        {
            throw new NotImplementedException();
        }

        public void SaveStationAppConfig(Ticket ticket)
        {
            throw new NotImplementedException();
        }
        private IMediator _mediator;
        public IMediator Mediator
        {
            get { return _mediator ?? (_mediator = IoCContainer.Kernel.Get<IMediator>()); }
        }
        public const int WAIT_AFTER_STARTUP_CIRCLE = 3;
        public const int STATION_IS_OFFLINE_MAX_CIRCLE = 0;

        public bool CheckDevicesChange()
        {
            PeripheralInfo peripheralInfo = null;
            PeripheralChecker pc = PeripheralChecker.Instance();

            peripheralInfo = new PeripheralInfo();
            peripheralInfo = pc.GetPeripheralInfo();
            DisplayHelper mon_inf = new DisplayHelper();
            peripheralInfo.monitors = mon_inf.GetDisplayDataJson();
            peripheralInfo.monitors_updated_at = mon_inf.MonitorsUpdatedTime();


            DriversMonitor DrvMonitor = DriversMonitor.Instance;
            DriverInfo[] di = null;
            List<DriverData> DriverDB = DrvMonitor.GetDriversData();
            if (DriverDB.Count > 0)
            {
                di = new DriverInfo[DriverDB.Count];
                WsdlRepository.WsdlServiceReference.DriverType dt;

                for (int i = 0; i < DriverDB.Count; i++)
                {
                    if (DriverDB.ElementAt(i) != null)
                    {
                        di[i] = new DriverInfo();
                        switch (DriverDB.ElementAt(i).type)
                        {
                            case SportBetting.WPF.Prism.Shared.DriverTypeDM.DRIVER_TYPE_DA2:
                                dt = DriverType.DRIVER_CASH_COIN_ACCEPTOR;
                                break;
                            case SportBetting.WPF.Prism.Shared.DriverTypeDM.DRIVER_TYPE_ID_READER:
                                dt = DriverType.DRIVER_CARD_READER;
                                break;
                            case SportBetting.WPF.Prism.Shared.DriverTypeDM.DRIVER_TYPE_TOUCH_SCREEN:
                                dt = DriverType.DRIVER_TOUCH_SCREEN;
                                break;
                            case SportBetting.WPF.Prism.Shared.DriverTypeDM.DRIVER_TYPE_PRINTER:
                                dt = DriverType.DRIVER_PRINTER;
                                break;
                            default:
                                continue;
                        }


                        di[i].type = dt;
                        if (dt != DriverType.DRIVER_PRINTER)
                        {
                            di[i].version = DriverDB.ElementAt(i).driver_version;
                        }
                        else
                        {
                            di[i].version = DriverDB.ElementAt(i).driver_file_version;
                        }
                        di[i].manufacturer = DriverDB.ElementAt(i).mnf_name;
                        di[i].details = DriverDB.ElementAt(i).installed_drivers;


                    }
                }
            }
            if (this.PeripheralInfo == null)
                return false;
            if (this.DriverInfos.Count == 0)
                return false;
            if (di == null)
                return true;
            if (di.Length != DriverInfos.Count)
                return false;
            for (int i = 0; i < di.Length; i++)
            {
                if (di[i].details != DriverInfos[i].details)
                    return false;
                if (di[i].manufacturer != DriverInfos[i].manufacturer)
                    return false;
                if (di[i].type != DriverInfos[i].type)
                    return false;
                if (di[i].version != DriverInfos[i].version)
                    return false;
            }
            if (this.PeripheralInfo.billValidatorState != peripheralInfo.billValidatorState)
                return false;
            if (this.PeripheralInfo.billValidatorVersion != peripheralInfo.billValidatorVersion)
                return false;
            if (this.PeripheralInfo.cardReaderState != peripheralInfo.cardReaderState)
                return false;
            if (this.PeripheralInfo.cardReaderVersion != peripheralInfo.cardReaderVersion)
                return false;
            if (this.PeripheralInfo.coinAcceptorState != peripheralInfo.coinAcceptorState)
                return false;
            if (this.PeripheralInfo.coinAcceptorVersion != peripheralInfo.coinAcceptorVersion)
                return false;
            if (this.PeripheralInfo.monitors != peripheralInfo.monitors)
                return false;
            if (this.PeripheralInfo.printerState != peripheralInfo.printerState)
                return false;
            if (this.PeripheralInfo.printerVersion != peripheralInfo.printerVersion)
                return false;

            return true;

        }

        private PeripheralInfo PeripheralInfo = null;
        private List<DriverInfo> DriverInfos = new List<DriverInfo>();
        [WsdlServiceSyncAspectSilent]
        public bool Refresh()
        {

            try
            {
                if (!ar_watcher_created)
                {
                    ar_watcher_created = true;
                    new System.Threading.Thread(TimerThread).Start();
                }
                int iStationStatusBeforeRefresh = Active;

                string sIpAddress = "";
                string sHostName = Dns.GetHostName();

                if (!string.IsNullOrEmpty(sHostName))
                {
                    IPHostEntry entry = Dns.GetHostEntry(sHostName);

                    if (entry != null)
                    {
                        foreach (IPAddress iPAdr in entry.AddressList)
                        {
                            if (iPAdr != null && iPAdr.ToString().Length > 0)
                            {
                                sIpAddress = iPAdr.ToString();
                            }
                        }
                    }
                }

                valueForm hubResponse;
                BsmHubConfigurationResponse hubConfigurationResponse;
                StationWS stationWS;
                // test to generate an exception
                //int sessionId; string dummy_lang;
                //WsdlRepository.GetHubClient().OpenSession("1000", false, "dummy_user_from_stationREPO",
                //                                          string.Empty, out sessionId, out dummy_lang); 
                PeripheralChecker pc = PeripheralChecker.Instance();

                var pi = pc.GetPeripheralInfo();
                DisplayHelper mon_inf = new DisplayHelper();
                pi.monitors = mon_inf.GetDisplayDataJson();
                pi.monitors_updated_at = mon_inf.MonitorsUpdatedTime();
                PeripheralInfo = ObjectCopier.Clone(pi);


                DriversMonitor DrvMonitor = DriversMonitor.Instance;
                DriverInfo[] di = null;
                List<DriverData> DriverDB = DrvMonitor.GetDriversData();
                if (DriverDB.Count > 0)
                {
                    di = new DriverInfo[DriverDB.Count];
                    DriverType dt;

                    for (int i = 0; i < DriverDB.Count; i++)
                    {
                        if (DriverDB.ElementAt(i) != null)
                        {
                            di[i] = new DriverInfo();
                            switch (DriverDB.ElementAt(i).type)
                            {
                                case SportBetting.WPF.Prism.Shared.DriverTypeDM.DRIVER_TYPE_DA2:
                                    dt = DriverType.DRIVER_CASH_COIN_ACCEPTOR;
                                    break;
                                case SportBetting.WPF.Prism.Shared.DriverTypeDM.DRIVER_TYPE_ID_READER:
                                    dt = DriverType.DRIVER_CARD_READER;
                                    break;
                                case SportBetting.WPF.Prism.Shared.DriverTypeDM.DRIVER_TYPE_TOUCH_SCREEN:
                                    dt = DriverType.DRIVER_TOUCH_SCREEN;
                                    break;
                                case SportBetting.WPF.Prism.Shared.DriverTypeDM.DRIVER_TYPE_PRINTER:
                                    dt = DriverType.DRIVER_PRINTER;
                                    break;
                                default:
                                    continue;
                            }


                            di[i].type = dt;
                            if (dt != DriverType.DRIVER_PRINTER)
                            {
                                di[i].version = DriverDB.ElementAt(i).driver_version;
                            }
                            else
                            {
                                di[i].version = DriverDB.ElementAt(i).driver_file_version;
                            }
                            di[i].manufacturer = DriverDB.ElementAt(i).mnf_name;
                            di[i].details = DriverDB.ElementAt(i).installed_drivers;


                        }
                    }
                }
                DriverInfos.Clear();
                if (di != null)
                    foreach (var driverInfo in di)
                    {
                        DriverInfos.Add(ObjectCopier.Clone(driverInfo));
                    }

                stationWS = WsdlRepository.GetStationProperties(StationNumber, sIpAddress, TeamViewerID, ServerVersion, PeripheralInfo, out hubResponse, out hubConfigurationResponse, DriverInfos.ToArray());
                if (stationWS != null)
                {
                    auto_restart_time = stationWS.stationAutorestartTime;

                    station_is_offline_counter = 0;
                    AllowMultiBet = stationWS.allowMultiBet;
                    LineSr.AllowMultiway = AllowMultiBet;

                    string mode = "Active";
                    if (stationWS.active == 3)
                    {
                        mode = "TestMode";
                    }
                    CCTCommunication.SetMode(mode);

                    bool _immediateWithdrawEnabled = false;
                    if (hubResponse != null)
                    {
                        foreach (var registrationField in hubResponse.fields)
                        {
                            if (registrationField.name == "paymentnote_immidiate_withdraw")
                            {
                                if (registrationField.value == "true")
                                {
                                    _immediateWithdrawEnabled = true;
                                }
                                break;
                            }
                        }
                    }
                    IsImmediateWithdrawEnabled = _immediateWithdrawEnabled;
                    IsCreditNoteImmediatelyPaid = stationWS.creditnoteImmediatelyPaid;

                    Mediator.SendMessage<int>(stationWS.doRestart, MsgTag.CheckRestart);

                    if (stationWS.updateCmData == 1 && wait_after_startup >= WAIT_AFTER_STARTUP_CIRCLE)
                    {
                        Mediator.SendMessage<bool>(true, MsgTag.UpdateLiveMonitorTemplates);
                    }


                    IsLocked = stationWS.active != (int)StationState.Active && stationWS.active != (int)StationState.TestMode;
                    IsPrematchEnabled = Convert.ToBoolean(stationWS.propertyPrematch);
                    IsLiveMatchEnabled = Convert.ToBoolean(stationWS.propertyLiveMatch);
                    AllowVhc = Convert.ToBoolean(stationWS.enableVHC);
                    AllowVfl = Convert.ToBoolean(stationWS.enableVFL);

                    IsCashier = stationWS.stationType == "Outlet";//TODO use stationws. setting

                    Active = stationWS.active; // is used in ShowCashAcceptorLockedLabel

                    if (stationWS.active != (int)StationState.CashLocked && iStationStatusBeforeRefresh != (int)StationState.CashLocked && (stationWS.active != iStationStatusBeforeRefresh))
                    {
                        Mediator.SendMessage<long>(2, MsgTag.LockStation);
                    }
                    else if (stationWS.active == (int)StationState.Locked)
                    {
                        Mediator.SendMessage<long>(0, MsgTag.LockStation);
                    }
                    else
                    {
                        Mediator.SendMessage<long>(1, MsgTag.LockStation);
                    }

                    // lock cash acceptors
                    ChangeTracker.LockCashAcceptors = stationWS.active == (int)StationState.CashLocked || stationWS.active == (int)StationState.Locked;
                    CashAcceptorAlwayActive = stationWS.cashAcceptorAlwayActive;
                    AllowAnonymousBetting = stationWS.isAnonymousBettingEnabled;

                    var minLimit = 0m;
                    if (ChangeTracker.CurrentUser != null)
                    {
                        minLimit = ChangeTracker.CurrentUser.DailyLimit;
                        if (ChangeTracker.CurrentUser.WeeklyLimit < minLimit)
                            minLimit = ChangeTracker.CurrentUser.WeeklyLimit;
                        if (ChangeTracker.CurrentUser.MonthlyLimit < minLimit)
                            minLimit = ChangeTracker.CurrentUser.MonthlyLimit;
                    }
                    SetCashInDefaultState(minLimit);
                    Mediator.SendMessage("", MsgTag.ShowCashAcceptorLockedLabel);

                    //set printing languages
                    PrintingLanguageId = stationWS.printingDefaultLanguage;
                    PrintingLanguageSetting = (int)stationWS.printingLanguageSetting;
                    //hack
                    //StationSettings.GetSettings.AllowAnonymousBetting = true;
                    IsStatisticsEnabled = stationWS.isStatisticsEnabled;

                    HubSettings.Clear();
                    PrintingLanguageSetting = stationWS.printingLanguageSetting;
                    IsIdCardEnabled = stationWS.authorizationSetting != "BARCODE";
                    IsAutoLogoutEnabled = Convert.ToBoolean(stationWS.propertyAutoLogout);
                    StationName = stationWS.stationName;
                    Currency = stationWS.currencyCode;
                    LocationName = stationWS.locationName;
                    FranchiserName = stationWS.franchisorName;
                    BarcodeScannerAlwaysActive = stationWS.barcodeScannerAlwayActive;
                    AuthorizedTicketScanning = stationWS.authorizedTicketScanning;
                    UserCardPinSetting = stationWS.userCardPinSetting;
                    OperatorCardPinSetting = stationWS.operatorCardPinSetting;
                    CreditNoteExpirationDays = stationWS.creditNoteExpireDays;
                    PayoutExpiredPaymentCreditNotes = stationWS.payoutExpiredPaymentCreditNotes;
                    IsStoreTicketEnabled = Convert.ToBoolean(stationWS.stationStoreTicketEnabled);
                    AllowMixedStakes = stationWS.allowMixedStakes;
                    BonusFromOdd = stationWS.multiBetBonusFromOdd;
                    BonusRangeList = stationWS.bonusRanges.ToList();
                    MaxStakeSingleBet = stationWS.maxStakeSingleBet;
                    StationSettings.Active = stationWS.active;
                    RollingFileSize = stationWS.logRotationFileSize;
                    RollingFileCount = stationWS.logRotationNumber;
                    CombiLimitList = stationWS.combiLimits.ToList();
                    MaxOdd = stationWS.maxOdd;
                    MinStakeCombiBet = stationWS.minStakeCombiBet;
                    MaxStakeCombi = stationWS.maxStakeCombi;

                    MaxWinSingleBet = stationWS.maxWinSingleBet;
                    MinStakeSingleBet = stationWS.minStakeSingleBet;
                    MaxStakeSystemBet = stationWS.maxStakeSystemBet;
                    MinStakePerRow = stationWS.minStakePerRow;
                    MinStakeSystemBet = stationWS.minStakeSystemBet;
                    MaxWinSystemBet = stationWS.maxWinSystemBet;
                    DisplayTaxNumber = stationWS.displayTaxField;
                    MaxSystemBet = stationWS.maxSystemBet;
                    Log.SetMaxSizeAndFileCount(stationWS.logRotationFileSize * 1024 * 1024, stationWS.logRotationNumber);
                    AllowFutureMatches = stationWS.stationAllowFutureMatches;
                    EnableOddsChangeIndication = stationWS.enableOddsChangeIndication;
                    SngLiveBetTicketAcceptTime = stationWS.sngLiveBetTicketAcceptTime;
                    CombiLiveBetTicketAcceptTime = stationWS.combiLiveBetTicketAcceptTime;
                    MinCombination = stationWS.minCombination;
                    MaxCombination = stationWS.maxCombination;
                    FooterLine1 = stationWS.footerLine1;
                    FooterLine2 = stationWS.footerLine2;
                    BetTerms = stationWS.betTerms;
                    PrintLogo = stationWS.printLogo;
                    HeaderLine1 = stationWS.headerLine1;
                    HeaderLine2 = stationWS.headerLine2;
                    HeaderLine3 = stationWS.headerLine3;
                    HeaderLine4 = stationWS.headerLine4;
                    SyncInterval = stationWS.syncInterval;
                    LayoutName = stationWS.layoutName;
                    SportRadar.DAL.DalStationSettings.Instance.EnableOddsChangeIndication = stationWS.enableOddsChangeIndication;
                    ResponsibleGaming = stationWS.showAddictionMsg;

                    Mediator.SendMessage<long>(-1, MsgTag.ShowSystemMessage);

                    if (stationWS.terminalLanguages != null)
                        SystemLanguages = stationWS.terminalLanguages.Split(';').ToList();

                    if (TimeZoneOffset != stationWS.timezoneOffset)
                    {
                        Mediator.SendMessage(true, MsgTag.OffsetChanged);
                        TimeZoneOffset = stationWS.timezoneOffset;
                        SetTimeZone(TimeZoneOffset, stationWS.hasLiveBet);
                    }


                    if (stationWS.enableVideoLiveStreamingSpecified)
                    {
                        if (stationWS.enableVideoLiveStreaming == 0)
                            EnableLiveStreaming = false;
                        else
                            EnableLiveStreaming = true;
                    }

                    //archive logs, configs and then send them
                    if (stationWS.doExportLog)
                    {
                        LogSending.stationNumber = StationNumber;
                        LogSending.maxFileSize = stationWS.logRotationFileSize;
                        LogSending.SendLogs();
                    }

                    if (hubResponse != null)
                    {
                        foreach (var valueField in hubResponse.fields)
                        {
                            HubSettings[valueField.name] = valueField.value;
                        }
                    }

                    if (Created_At < DateTimeUtils.DATETIMENULL)
                    {
                        Created_At = DateTimeUtils.DATETIMENULL;
                    }

                    FranchisorID = stationWS.franchisorID;
                    LocationID = stationWS.locationID;

                    VFLSource = stationWS.vflVideoSourceType;

                    IsReady = true;
                    Mediator.SendMessage<string>("", MsgTag.RefreshStation);

                    string sLogoFile = Directory.GetCurrentDirectory().ToString() + "\\Images\\TicketLogo_" + stationWS.ticketLogoId.ToString() + ".jpg";
                    string existingLogoPath = ConfigurationManager.AppSettings["logoFile"];
                    if (stationWS.ticketLogoId != 0 && !File.Exists(sLogoFile))
                    {
                        try
                        {
                            byte[] bytes = Convert.FromBase64String(WsdlRepository.GetStationResource(stationWS.ticketLogoId));
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                Image.FromStream(ms).Save(sLogoFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            // do nothing
                            Log.Error(ex.Message, ex);
                        }

                        try
                        {
                            File.Copy(sLogoFile, existingLogoPath, true);
                        }
                        catch (Exception ex)
                        {
                            // rollback
                            Log.Error(ex.Message, ex);
                            File.Delete(sLogoFile);
                        }


                    }
                    else if (stationWS.ticketLogoId != 0 && File.Exists(sLogoFile))
                    {
                        FileInfo f = new FileInfo(sLogoFile);
                        FileInfo f2 = new FileInfo(existingLogoPath);

                        if (f.Length != f2.Length)
                        {
                            try
                            {
                                File.Copy(sLogoFile, existingLogoPath, true);
                            }
                            catch (Exception ex)
                            {
                                // rollback
                                Log.Error(ex.Message, ex);
                                File.Delete(sLogoFile);
                            }
                        }
                    }

                    return true;
                }
            }

            catch (Exception excp)
            {

                string sError = ExcpHelper.FormatException(excp, "Refresh() ERROR");
                Log.Error(sError, excp);
                if (excp is System.ServiceModel.EndpointNotFoundException || excp is System.ServiceModel.ProtocolException)
                {

                    if (station_is_offline_counter < STATION_IS_OFFLINE_MAX_CIRCLE)
                    {
                        if (wait_after_startup >= WAIT_AFTER_STARTUP_CIRCLE)
                        {
                            station_is_offline_counter++;
                        }
                    }
                    else if (Active != 0)
                    {

                        /* Mediator.SendMessage<long>(2, MsgTag.LockStation);
                         var lostConnection = new Tuple<string, string>("LostInternetConnection", "");
                         Mediator.SendMessage(lostConnection, "Error");*/
                        Mediator.SendMessage(new Tuple<string, string, bool, int>("LostInternetConnection", "", false, 0), "Error");
                        Active = 0;
                    }


                }
                if (excp is System.ServiceModel.Security.SecurityNegotiationException)
                {

                    Mediator.SendMessage(new Tuple<string, string, bool, int>("SertificateError", "", false, 0), "Error");

                } if (excp is System.InvalidOperationException && excp.Message.Contains("X.509"))
                {

                    Mediator.SendMessage(new Tuple<string, string, bool, int>("SertificateError", "", false, 0), "Error");

                }
                throw excp;
            }


            return false;
        }

        public bool DisplayTaxNumber { get; set; }

        public bool AllowMixedStakes { get; set; }

        protected int PrintingLanguageId
        {
            get
            {
                return GetStationAppConfigValue("PrintingLanguageId").ValueInt;
            }
            set
            {
                SetStationAppConfigValue<int>("PrintingLanguageId", value);
                OnPropertyChanged();
            }
        }

        public DateTime? Created_At
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public void Init()
        {
            Log.Debug("Init stationrepository");

            LoadSettings();
            StationSettings.Init();
            StationSettings.ReadPrefFileData();
            LineSr.AllowMultiway = AllowMultiBet;

            //Refresh();

        }



        public static int RollingFileSize { get; set; }
        public static int RollingFileCount { get; set; }

        private static string GetStringAppSettings(string sKeyName, string sDefaultValue)
        {
            try
            {
                string sString = ConfigurationManager.AppSettings[sKeyName];
                return !string.IsNullOrEmpty(sString) ? sString : sDefaultValue;
            }
            catch (Exception excp)
            {
                string sError = ExcpHelper.FormatException(excp, "GetStringAppSettings(sKey = {0}, sDefault = {1}) ERROR", sKeyName, sDefaultValue);
                Log.Error(sError, excp);
            }

            return sDefaultValue;
        }

        private static int GetIntAppSettings(string sKeyName, int iDefaultValue)
        {
            try
            {
                string sInt = ConfigurationManager.AppSettings[sKeyName];
                return Convert.ToInt32(sInt);
            }
            catch (Exception excp)
            {
                string sError = ExcpHelper.FormatException(excp, "GetIntAppSettings(sKey = {0}, iDefault = {1}) ERROR", sKeyName, iDefaultValue);
                Log.Error(sError, excp);
            }

            return iDefaultValue;
        }

        private static bool GetBoolAppSettings(string sKeyName, bool bDefaultValue)
        {
            try
            {
                string sBool = ConfigurationManager.AppSettings[sKeyName];
                return Convert.ToBoolean(sBool);
            }
            catch (Exception excp)
            {
                string sError = ExcpHelper.FormatException(excp, "GetBoolAppSettings(sKey = {0}, iDefault = {1}) ERROR", sKeyName, bDefaultValue);
                Log.Error(sError, excp);
            }

            return bDefaultValue;
        }

        public void LoadSettings()
        {
            StationSettings.PrefFileName = GetStringAppSettings("PrefFilePathAndName", @"..\PrefFile.txt");
            AutoLogoutWindowLiveTimeInSec = GetIntAppSettings("AutoLogoutWindowLiveTimeInSec", 7);
            UsePrinter = GetBoolAppSettings("UsePrinter", true);
            TurnOffCashInInit = GetBoolAppSettings("TurnOffCashInInit", false);
            TicketNumberLength = GetIntAppSettings("TicketNumberLength", 13);
            PaymentNoteLength = GetIntAppSettings("PaymentNoteLength", 13);

            CheckSumLength = GetIntAppSettings("CheckSumLength", 4);
            TaxNumberLength = GetIntAppSettings("TaxNumberLength", 16);
            StoreTicketExpirationHours = GetIntAppSettings("StoreTicketExpirationHours", 24);


        }

        public static IRepository Repository
        {
            get { return IoCContainer.Kernel.Get<IRepository>(); }
        }
        private readonly Object _stationAppConfigLockObj = new Object();
        private Dictionary<string, StationAppConfigSr> _stationAppConfig;
        private Dictionary<string, StationAppConfigSr> GetStationAppConfig
        {
            get
            {
                lock (_stationAppConfigLockObj)
                {
                    if (_stationAppConfig == null || IsTestMode)
                    {
                        _stationAppConfig = new Dictionary<string, StationAppConfigSr>();
                        try
                        {


                            List<StationAppConfigSr> lConfigs = Repository.GetAllStationAppConfigs();
                            if (lConfigs != null)
                                foreach (var stationAppConfigSr in lConfigs)
                                {
                                    if (!_stationAppConfig.ContainsKey(stationAppConfigSr.PropertyName))
                                        _stationAppConfig.Add(stationAppConfigSr.PropertyName, stationAppConfigSr);
                                }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    return _stationAppConfig;
                }
            }
        }

        public void SetStationAppConfigValue<T>(string name, T value)
        {
            StationAppConfigSr oldValue;
            if (!GetStationAppConfig.ContainsKey(name))
                oldValue = Repository.GetStationAppConfigByName(name);
            else
            {
                oldValue = GetStationAppConfig[name];
            }

            if (oldValue != null)
            {
                if (typeof(T) == typeof(decimal))
                {
                    oldValue.SetValue(Convert.ToDecimal(value));
                }
                if (typeof(T) == typeof(string))
                {
                    oldValue.SetValue(value as string);
                }
                if (typeof(T) == typeof(int))
                {
                    oldValue.SetValue(Convert.ToInt32(value));
                }
                if (typeof(T) == typeof(DateTime))
                {
                    oldValue.SetValue(Convert.ToDateTime(value));
                }
                oldValue.Save();
            }
            else
            {
                StationAppConfigSr newValue = null;
                if (typeof(T) == typeof(decimal))
                {
                    newValue = new StationAppConfigSr(name, Convert.ToDecimal(value));
                }
                if (typeof(T) == typeof(string))
                {
                    newValue = new StationAppConfigSr(name, value as string);
                }
                if (typeof(T) == typeof(int))
                {
                    newValue = new StationAppConfigSr(name, Convert.ToInt32(value));
                }
                if (typeof(T) == typeof(DateTime))
                {
                    newValue = new StationAppConfigSr(name, Convert.ToDateTime(value));
                }
                GetStationAppConfig[name] = newValue;
                Repository.Save(newValue);
            }
        }

        public StationAppConfigSr GetStationAppConfigValue(string name)
        {
            var newValue = new StationAppConfigSr(name, 0);
            if (GetStationAppConfig.ContainsKey(name))
            {
                var oldValue = GetStationAppConfig[name];
                return oldValue;
            }
            try
            {
                var oldValue = Repository.GetStationAppConfigByName(name);
                if (oldValue != null)
                    return oldValue;
                GetStationAppConfig[name] = newValue;
            }
            catch (Exception)
            {
                GetStationAppConfig[name] = newValue;
            }
            return newValue;
        }


        public bool TurnOffCashInInit
        {
            get { return StationSettings.TurnOffCashInInit; }
            set { StationSettings.TurnOffCashInInit = value; }
        }

        public string ServerVersion { get; set; }

        public int AutoLogoutWindowLiveTimeInSec { get; set; }

        public string DefaultDisplayLanguage
        {
            get { return LineSr.Instance.GetTerminalLanguages().Where(x => x.LanguageId == PrintingLanguageId).Select(x => x.ShortName).FirstOrDefault() ?? "EN"; }
            set { DefaultDisplayLanguage = value; }
        }

        public bool UsePrinter
        {
            get { return StationSettings.UsePrinter; }
            set { StationSettings.UsePrinter = value; }
        }


        public string TeamViewerID { get; set; }

        public IPrinter Printer
        {
            get { return StationSettings.Printer; }
            set { StationSettings.Printer = value; }
        }

        public int PrinterStatus
        {
            get { return StationSettings.PrinterStatus; }
            set { StationSettings.PrinterStatus = value; }
        }


        private static string _stationNumber = "0000";

        public string StationNumber
        {
            get
            {
                if (!String.IsNullOrEmpty(GetStationAppConfigValue("StationNumber").ValueString))
                {
                    _stationNumber = GetStationAppConfig["StationNumber"].ValueString;
                }
                return _stationNumber;
            }
        }

        private bool _isTestMode = false;
        public bool IsTestMode
        {

            get
            {
                return _isTestMode;
            }

            set { _isTestMode = value; OnPropertyChanged(); }
        }

        public string DisplayLanguage { get; set; }

        public System.Globalization.CultureInfo Culture
        {
            get { return StationSettings.Culture; }
        }


        public void AddTestMoNeyFromKeyboard(decimal money)
        {
            StationSettings.AddTestMoNeyFromKeyboard(money);
        }
        private static IChangeTracker ChangeTracker { get { return IoCContainer.Kernel.Get<IChangeTracker>(); } }



        public int SyncInterval
        {
            get { return StationSettings.SyncInterval; }
            set { StationSettings.SyncInterval = value; }
        }




        public int FranchisorID
        {
            get
            {
                return GetStationAppConfigValue("FranchisorID").ValueInt;
            }
            set
            {
                SetStationAppConfigValue<int>("FranchisorID", value);
                OnPropertyChanged();
            }
        }

        public int LocationID
        {
            get
            {
                return GetStationAppConfigValue("LocationID").ValueInt;
            }
            set
            {
                SetStationAppConfigValue<int>("LocationID", value);
                OnPropertyChanged();
            }
        }


        public void EnableCashIn()
        {
            if (ChangeTracker.LockCashAcceptors)
            {
                DisableCashIn();
                return;
            }
            StationSettings.EnableCashIn(0, 100000);
        }

        public void DisableCashIn()
        {
            if (StationSettings.IsCashinOk)
                StationSettings.CashInDisable();
            StationSettings.IsCashInEnabled = false;
        }

        public void SetCashInDefaultState(decimal minLimit)
        {
            if (ChangeTracker.LockCashAcceptors)
            {
                DisableCashIn();
                return;
            }

            if (ChangeTracker.CurrentUser is AnonymousUser && !AllowAnonymousBetting)
            {
                DisableCashIn();
                return;
            }
            if (ChangeTracker.CurrentUser is LoggedInUser)
            {
                if (CashAcceptorAlwayActive)
                {
                    StationSettings.EnableCashIn(0, minLimit);
                    return;
                }
                if (ChangeTracker.IsBasketOpen)
                {
                    StationSettings.EnableCashIn(0, minLimit);
                    return;
                }
            }
            else if (ChangeTracker.CurrentUser is AnonymousUser)
            {
                if (ChangeTracker.IsBasketOpen)
                {
                    StationSettings.EnableCashIn(0, 1000000);
                    return;
                }
                if (CashAcceptorAlwayActive)
                {
                    StationSettings.EnableCashIn(0, 1000000);
                    return;
                }
            }
            DisableCashIn();
        }



        public bool IsCashDatasetValid()
        {
            return StationSettings.IsCashDatasetValid();
        }

        public string Currency
        {
            get
            {
                return m_sCurrency;
            }
            set
            {
                m_sCurrency = value;
                OnPropertyChanged("Currency");
            }
        }


        public decimal BonusFromOdd { get; set; }

        private bool _enableOddsChangeIndication;
        public bool EnableOddsChangeIndication
        {
            get
            {
                return _enableOddsChangeIndication;
            }
            set
            {
                if (_enableOddsChangeIndication == value)
                    return;
                _enableOddsChangeIndication = value;
                OnPropertyChanged();
            }
        }

        public decimal MaxOdd { get; set; }

        public decimal MinStakeCombiBet { get; set; }

        public decimal MinStakeSystemBet { get; set; }

        public decimal MinStakeSingleBet { get; set; }

        public decimal MinStakePerRow { get; set; }

        public bool AllowFutureMatches { get; set; }

        public decimal MaxWinSystemBet { get; set; }

        public string LastTicketNbr
        {
            get { return GetStationAppConfigValue("LastTicketNbr").ValueString; }
            set { SetStationAppConfigValue("LastTicketNbr", value); }
        }

        private decimal _maxStakeSingleBet;
        public decimal MaxStakeSingleBet
        {
            get { return _maxStakeSingleBet; }
            set { _maxStakeSingleBet = value; }
        }

        public decimal MaxWinSingleBet { get; set; }

        public int SngLiveBetTicketAcceptTime { get; set; }

        public int CombiLiveBetTicketAcceptTime { get; set; }

        public decimal MaxSystemBet { get; set; }

        public decimal MaxStakeSystemBet { get; set; }

        public decimal MaxStakeCombi { get; set; }

        public int MinCombination { get; set; }

        public int MaxCombination { get; set; }


        public List<CombiLimitWS> CombiLimitList { get; set; }

        private List<BonusRangeWS> _bonusRanges = new List<BonusRangeWS>();
        public List<BonusRangeWS> BonusRangeList
        {
            get
            {
                return _bonusRanges; // StationSettings.Station.bonusRanges.ToList(); 
            }
            set { _bonusRanges = value; }
        }



        public string FooterLine1 { get; set; }

        public string FooterLine2 { get; set; }

        public string BetTerms { get; set; }

        public bool PrintLogo { get; set; }

        public string HeaderLine1 { get; set; }

        public string HeaderLine2 { get; set; }

        public string HeaderLine3 { get; set; }

        public string HeaderLine4 { get; set; }

        public bool BarcodeScannerAlwaysActive
        {
            get
            {
                return GetStationAppConfigValue("BarcodeScannerAlwaysActive").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("BarcodeScannerAlwaysActive", value ? 1 : 0);
                OnPropertyChanged();
            }
        }

        public bool BarcodeScannerTempActive { get; set; }

        public int TicketNumberLength { get; set; }

        public int CheckSumLength { get; set; }

        public int TaxNumberLength { get; set; }


        public int StoreTicketExpirationHours { get; set; }

        public bool CashAcceptorAlwayActive
        {
            get
            {
                return GetStationAppConfigValue("CashAcceptorAlwayActive").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("CashAcceptorAlwayActive", value ? 1 : 0);
                OnPropertyChanged();
            }
        }

        public int Active
        {
            get { return _active; }
            set
            {
                if (value == _active)
                    return;
                _active = value;
                OnPropertyChanged();
            }
        }

        public bool AllowAnonymousBetting
        {
            get
            {
                return GetStationAppConfigValue("AllowAnonymousBetting").ValueInt == 1;
            }
            set
            {
                SetStationAppConfigValue<int>("AllowAnonymousBetting", value ? 1 : 0);
                OnPropertyChanged();
            }
        }

        public List<string> SystemLanguages
        {
            get
            {
                if (GetStationAppConfigValue("SystemLanguages") == null || GetStationAppConfigValue("SystemLanguages").ValueString == null)
                {
                    return new List<string>();
                }
                return GetStationAppConfigValue("SystemLanguages").ValueString.Split('|').ToList();
            }
            set
            {
                var languages = "";
                foreach (var lang in value)
                {
                    languages += lang + "|";
                }
                languages = languages.TrimEnd('|');
                SetStationAppConfigValue<string>("SystemLanguages", languages);
                OnPropertyChanged();
            }
        }
        private decimal _printingLanguageSetting;
        private int _active;
        private static DateTime? _createdAt;
        private TicketWS _lastTicket;
        private bool _isLocked;
        private Dictionary<string, string> _hubSettings = new Dictionary<string, string>();
        private int _cardBarcodeLen = 15;


        public Dictionary<string, string> HubSettings
        {
            get { return _hubSettings; }
            set { _hubSettings = value; }
        }

        public int PaymentNoteLength { get; set; }

        public int CreditNoteExpirationDays { get; set; }
        public decimal GetBonusFromOdd(Ticket newTicket)
        {
            return BonusFromOdd;
        }

        public bool PayoutExpiredPaymentCreditNotes { get; set; }

        public bool IsLocked
        {
            get { return _isLocked; }
            private set
            {
                if (value.Equals(_isLocked))
                    return;
                _isLocked = value;
                OnPropertyChanged();
            }
        }



        public TicketWS LastTicket
        {
            get
            {
                if (_lastTicket == null)
                    _lastTicket = BusinessPropsHelper.GetLastTicket();
                return _lastTicket;
            }
            set { _lastTicket = value; }
        }

        public int UserCardPinSetting { get; set; }

        public int OperatorCardPinSetting { get; set; }

        public int VFLSource { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool BarcodeScannerTestMode { get; set; }

        private void SetTimeZone(int timeZone, bool refreshLive)
        {
            TimeZoneControl.ChangeTimeZone(timeZone, refreshLive);
        }
    }
    public static class ObjectCopier
    {
        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }


}
