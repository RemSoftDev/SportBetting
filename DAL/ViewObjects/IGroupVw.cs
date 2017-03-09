using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.ViewObjects
{
    public interface IGroupVw
    {
        GroupLn LineObject { get; }
        string DisplayName { get; }
        bool Active { get; }
        GroupVw ParentGroupView { get; }
        System.Windows.Visibility Visibility { get; }
        bool IsEnabled { get; }
        IGroupVw TournamentSportView { get; }
        IGroupVw TournamentCountryView { get; }
        int Sort { get; set; }
    }
}