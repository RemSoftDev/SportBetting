using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Shared;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SharedInterfaces
{
    public class Repository : IRepository
    {
        public SortableObservableCollection<IMatchVw> FindMatches(SortableObservableCollection<IMatchVw> ocMatches, string sSearch, string sLanguage, LineSr.DelegateFilterMatches dfm, Comparison<IMatchVw> comparison)
        {
            LineSr.Instance.SearchMatches(ocMatches, sSearch, sLanguage, dfm, comparison);

            return ocMatches;
        }

        public void FindMatches(SyncObservableCollection<IMatchVw> socMatches, string sSearch, string sLanguage, LineSr.DelegateFilterMatches dfm)
        {
            LineSr.Instance.SearchMatches(socMatches, sSearch, sLanguage, dfm);
        }

        public void FindResults(SortableObservableCollection<MatchResultVw> ocMatchResults, LineSr.DelegateFilterResults dfr, Comparison<MatchResultVw> comparison)
        {
            LineSr.Instance.SearchResults(ocMatchResults, dfr, comparison);
        }

        public void VerifySelectedOdds(SyncObservableCollection<ITipItemVw> syncList)
        {
            LineSr.Instance.VerifySelectedOdds(new SortableObservableCollection<ITipItemVw>(syncList));
        }

        public void GetAllLanguages(ObservableCollection<Language> languages)
        {
            var languagesLn = LineSr.Instance.GetTerminalLanguages();
            foreach (var languageLn in languagesLn)
            {
                if (languages.Count(x => x.Id == languageLn.LanguageId) < 1)
                    languages.Add(new Language(languageLn.ShortName.ToUpperInvariant()) { Id = languageLn.LanguageId });

            }
        }

        public StationAppConfigSr GetStationAppConfigByName(string propertyName)
        {
            return StationAppConfigSr.GetValueByName(propertyName);
        }

        public IMatchLn GetByBtrMatchId(long btrMatchId, bool b)
        {
            return LineSr.Instance.AllObjects.Matches.GetByBtrMatchId(btrMatchId, b);
        }

        public void Save(StationAppConfigSr stationAppConfigSr)
        {
            stationAppConfigSr.Save();
        }

        public IMatchLn GetByMatchId(long id)
        {
            var match = LineSr.Instance.AllObjects.Matches.GetObject(Math.Abs(id));
            return match;
        }

        public List<StationAppConfigSr> GetAllStationAppConfigs()
        {
            return StationAppConfigSr.GetAllSettings();
        }


        public IOddVw GetOddBySvrId(long svrOddId)
        {
            var oddLn = LineSr.Instance.AllObjects.Odds.GetObject(Math.Abs(svrOddId));
            return oddLn != null ? oddLn.OddView : null;
        }
    }
}