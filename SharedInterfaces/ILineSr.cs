using System;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SharedInterfaces
{
    public interface ILineSr
    {
        void SubsribeToEvent(DelegateDataSqlUpdateSucceeded dataSqlUpdateSucceeded);
        void UnsubscribeFromEnent(DelegateDataSqlUpdateSucceeded dataSqlUpdateSucceeded);
        void VerifySelectedOdds(SortableObservableCollection<ITipItemVw> sortableObservableCollection, SyncHashSet<ITipItemVw> shsToRemove = null);
        bool IsTournamentVisible(string svrId);
        TournamentMatchLocksDictionary TournamentMatchLocks();
        SyncList<GroupLn> GetAllGroups();
        LiabilityLn GetAllLiabilities(string key);
    }
}