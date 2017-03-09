using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shared;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SharedInterfaces
{
    public interface IRepository
    {
        SortableObservableCollection<IMatchVw> FindMatches(SortableObservableCollection<IMatchVw> ocMatches, string sSearch, string sLanguage, LineSr.DelegateFilterMatches dfm, Comparison<IMatchVw> comparison);
        void FindMatches(SyncObservableCollection<IMatchVw> socMatches, string sSearch, string sLanguage, LineSr.DelegateFilterMatches dfm);
        void FindResults(SortableObservableCollection<MatchResultVw> ocMatchResults, LineSr.DelegateFilterResults dfr, Comparison<MatchResultVw> comparison);
        void VerifySelectedOdds(SyncObservableCollection<ITipItemVw> syncList);
        IOddVw GetOddBySvrId(long svrOddId);
        void GetAllLanguages(ObservableCollection<Language> languages);
        StationAppConfigSr GetStationAppConfigByName(string propertyName);
        IMatchLn GetByBtrMatchId(long btrMatchId, bool b);
        void Save(StationAppConfigSr stationAppConfigSr);
        IMatchLn GetByMatchId(long id);
        List<StationAppConfigSr> GetAllStationAppConfigs();
    }
}