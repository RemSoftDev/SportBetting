using System.Windows;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace MVVMTest
{
    public class TestGroupVw : IGroupVw
    {
        private GroupLn _lineObject = new GroupLn();
        public string DisplayName { get; set; }
        public bool Active { get; private set; }
        public GroupVw ParentGroupView { get; private set; }
        public Visibility Visibility { get; private set; }
        public bool IsEnabled { get; private set; }
        public IGroupVw TournamentSportView { get; set; }
        public IGroupVw TournamentCountryView { get; private set; }
        public int Sort { get; set; }

        public GroupLn LineObject
        {
            get { return _lineObject; }
            set { _lineObject = value; }
        }

        public static IGroupVw CreateGroup(long id, bool isSport)
        {
            var group = new TestGroupVw();
            group.DisplayName = "groupname" + id;
            group.LineObject = new GroupLn();
            group.LineObject.GroupId = id;
            if (isSport)
            {
                group.LineObject.Type = "group_sport";
                group.LineObject.GroupSport.SportDescriptor = "groupname" + id;
            }
            else
                group.LineObject.Type = "group_tournament";
            return group;
        }
    }
}