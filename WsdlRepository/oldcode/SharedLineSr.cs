using SharedInterfaces;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportBetting.WPF.Prism.Shared.Services
{
    public class SharedLineSr : ILineSr
    {
        LineSr Instance { get { return LineSr.Instance; } }
        public void SubsribeToEvent(DelegateDataSqlUpdateSucceeded dataSqlUpdateSucceeded)
        {
            LineSr.DataSqlUpdateSucceeded += dataSqlUpdateSucceeded;
        }

        public void UnsubscribeFromEnent(DelegateDataSqlUpdateSucceeded dataSqlUpdateSucceeded)
        {
            LineSr.DataSqlUpdateSucceeded -= dataSqlUpdateSucceeded;
        }

        public void VerifySelectedOdds(SortableObservableCollection<ITipItemVw> sortableObservableCollection, SyncHashSet<ITipItemVw> shsToRemove)
        {
            Instance.VerifySelectedOdds(sortableObservableCollection, shsToRemove);
        }

        public TournamentMatchLocksDictionary TournamentMatchLocks()
        {
            return Instance.TournamentMatchLocks();
        }

        public SyncList<GroupLn> GetAllGroups()
        {
            return LineSr.Instance.AllObjects.Groups.ToSyncList();
        }

        public LiabilityLn GetAllLiabilities(string key)
        {
            return LineSr.Instance.AllObjects.Liabilities.SafelyGetValue(key);
        }

        public bool IsTournamentVisible(string svrId)
        {
            return Instance.IsTournamentVisible(svrId);
        }
    }
}
