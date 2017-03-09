using System;
using System.Data;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest
{
    public class TestMatchLn : ObjectBase, ILineObjectWithId<TestOdd>, IRemovableLineObject<TestOdd>, IMatchLn
    {
        private SyncList<GroupLn> _parentGroups = new SyncList<GroupLn>();

        public TestMatchLn()
            : base(true)
        {
        }

        public bool IsNew { get; set; }

        public override void FillFromDataRow(DataRow dr)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(IMatchLn objSource)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(TestOdd objSource)
        {
            throw new NotImplementedException();
        }

        public long Id { get; set; }
        public long RemoveId { get; set; }
        public ObservableProperty<int> Code { get; set; }
        public ObservableProperty<bool> Active { get; set; }
        public ObservableProperty<DateTimeSr> ExpiryDate { get; set; }
        public ObservableProperty<string> NameTag { get; set; }
        public BetDomainList BetDomains { get; set; }
        public IMatchVw MatchView { get; set; }

        public SyncList<GroupLn> ParentGroups
        {
            get { return _parentGroups; }
            set { _parentGroups = value; }
        }

        public CompetitorLn HomeCompetitor { get; set; }
        public CompetitorLn AwayCompetitor { get; set; }
        public ObservableProperty<DateTimeSr> StartDate { get; set; }
        public long MatchId { get; set; }

        public SyncList<BetDomainLn> GetSortedBetDomains()
        {
            return new SyncList<BetDomainLn>();
        }

        public IBetDomainLn SelectedBetDomain { get; set; }
        public void SetSelected(IOddLn oddToDo, bool bIsSelected)
        {

        }

        public LiveMatchInfoLn LiveMatchInfo { get; set; }
        public eServerSourceType SourceType { get; set; }

        public eOutrightType outright_type { get; set; }

        public long VhcChannelId { get; set; }

        public PositionToOutrightDictionary OutrightCompetitors { get; set; }
        public long BtrMatchId { get; set; }
        public long Sort { get; set; }
        public MatchExternalState MatchExternalState { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsEnabled { get; private set; }
        public ObservableProperty<int> ChangedCount { get; set; }

        public ObservableProperty<bool> IsLiveBet { get; set; }

        public string BtrPreLiveKeyName { get; private set; }
        public ObservableProperty<DateTimeSr> EndDate { get; set; }
        public ObservableProperty<long> HomeCompetitorId { get; set; }

        public void SetActiveChanged()
        {
        }

        public string GetOutrightDisplayName(string language)
        {
            throw new NotImplementedException();
        }

        public IBetDomainLn GetBaseBetDomain()
        {
            throw new NotImplementedException();
        }

        public static IMatchLn CreateMatch(long id, bool livebet, bool isOutright = false)
        {
            var match = new TestMatchLn()
            {
                MatchId = id,
                BtrMatchId = id,
                Active = { Value = true },
                IsLiveBet = { Value = livebet },
                StartDate = { Value = new DateTimeSr(DateTime.Now.AddHours(3)) },
                ExpiryDate = { Value = new DateTimeSr(DateTime.Now.AddHours(3)) },
                outright_type = isOutright ? eOutrightType.Outright : eOutrightType.None,

            };
            match.MatchView = TestMatchVw.CreateMatch(id, livebet, isOutright);
            //match.ParentGroups.Add(TestGroupLn.CreateGroup(GroupLn.GROUP_TYPE_SPORT));
            //match.BetDomains = new BetDomainList(match);

            return match;
        }
    }
}