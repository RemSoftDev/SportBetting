using System;
using System.Data;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest
{
    public class TestGroupLn : ObjectBase, ILineObjectWithId<TestGroupLn>, IRemovableLineObject<TestGroupLn>, IGroupLn
    {

        public TestGroupLn()
            : base(true)
        {
        }


        public static IGroupLn CreateGroup(string type)
        {
            var group = new TestGroupLn();
            group.Type = type;
            group.GroupSport = new GroupSportExternalState();
            group.GroupSport.SportDescriptor = SportSr.SPORT_DESCRIPTOR_SOCCER;
            return group;
        }

        public bool IsNew { get; set; }

        public override void FillFromDataRow(DataRow dr)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(IGroupLn objSource)
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(TestGroupLn objSource)
        {
            throw new NotImplementedException();
        }

        public long Id { get; set; }
        public long RemoveId { get; private set; }
        public GroupVw GroupView { get; private set; }
        public GroupSportExternalState GroupSport { get; private set; }
        public long GroupId { get; set; }
        public string KeyName { get; private set; }
        public string Type { get; set; }
        public GroupTournamentExternalState GroupTournament { get; private set; }
        public ObservableProperty<long?> ParentGroupId { get; private set; }
        public ObservableProperty<bool> Active { get; private set; }
        public ObservableProperty<int> Sort { get; private set; }
        public GroupLn ParentGroup { get; private set; }
        public long SvrGroupId { get; private set; }

        public string GetDisplayName(string language)
        {
            throw new NotImplementedException();
        }
    }
}