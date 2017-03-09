using System;
using System.Data;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest
{
    public class TestBetDomain : ObjectBase, ILineObjectWithId<TestBetDomain>, IRemovableLineObject<TestBetDomain>, IBetDomainLn
    {
        private IMatchLn _match = new TestMatchLn();
        public OddList Odds { get; set; }
        public IMatchLn Match
        {
            get { return _match; }
            set { _match = value; }
        }

        public IBetDomainVw BetDomainView { get; set; }
        public ObservableProperty<eBetDomainStatus> Status { get; set; }
        public long BetDomainId { get; set; }

        public string GetDisplayName(string language)
        {
            return "name";
        }

        public ObservableProperty<int> BetDomainNumber { get; set; }
        public string BetTag { get; set; }
        public ObservableProperty<string> SpecialOddValue { get; set; }
        public ObservableProperty<int> ChangedCount { get; set; }
        public ObservableProperty<string> SpecialOddValueFull { get; set; }
        public ObservableProperty<bool> IsLiveBet { get; set; }
        public ObservableProperty<long> BtrLiveBetId { get; set; }
        public ObservableProperty<string> Result { get; set; }
        public string ExtendedState { get; set; }

        public void SetSelected(IOddLn odd, bool bSelected)
        {

        }

        public SyncList<IOddLn> GetSelectedOdds()
        {
            return new SyncList<IOddLn>();
        }
        public string NameTag { get; set; }
        public ObservableProperty<int> Sort { get; set; }
        public BetDomainTypeLn BetDomainType { get; set; }
        public long Id { get; private set; }
        public ObservableProperty<bool> IsManuallyDisabled { get; set; }
        public bool IsSelected { get; private set; }
        public BetDomainExternalState BetDomainExternalState { get; private set; }
        public long MatchId { get; set; }

        public bool IsCashierEnabled { get; private set; }

        public void SetActiveChanged()
        {

        }

        public void NotifyOddsEnabledChanged()
        {
            throw new NotImplementedException();
        }

        public SyncDictionary<int, OddLn> GetSortedOdds()
        {
            throw new NotImplementedException();
        }

        public bool IsNew { get; private set; }

        public override void FillFromDataRow(DataRow dr)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(IBetDomainLn objSource)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(TestBetDomain objSource)
        {
            throw new NotImplementedException();
        }

        public long RemoveId { get; private set; }

        public static IBetDomainLn CreateBetDomain(int id, bool isbase, int numberOfOdds, bool isvisible)
        {
            var betdomain = new TestBetDomain()
                {
                    BetDomainId = id,
                };
            betdomain.Sort = new ObservableProperty<int>(new BetDomainLn(), new ObservablePropertyList(), "ChangedCount");
            betdomain.Sort.Value = id;
            betdomain.Status = new ObservableProperty<eBetDomainStatus>(new BetDomainLn(), new ObservablePropertyList(), "ChangedCount");
            betdomain.Status.Value = eBetDomainStatus.Visible;
            betdomain.IsManuallyDisabled = new ObservableProperty<bool>(new BetDomainLn(), new ObservablePropertyList(), "ChangedCount");
            betdomain.IsManuallyDisabled.Value = !isvisible;
            if (isbase)
            {
                betdomain.BetTag = BetDomainTypeLn.BET_TAG_WINFTR;
            }
            else
            {
                betdomain.BetTag = BetDomainTypeLn.BET_TAG_WINFTR + "dsg";
            }
            betdomain.BetDomainView = TestBetDomainVw.CreateBetDomain();
            //betdomain.Odds = new OddList<IOddLn>(betdomain);
            for (int i = 0; i < numberOfOdds; i++)
            {
                var odd = TestOdd.CreateOdd(id + i, id + i, true,betdomain);
                //betdomain.Odds.Add(odd);
            }
            betdomain.Match = TestMatchLn.CreateMatch(id + 1, true);

            return betdomain;
        }

    }
}