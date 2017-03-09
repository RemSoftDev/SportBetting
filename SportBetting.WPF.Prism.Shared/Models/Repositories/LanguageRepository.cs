using System.Collections.ObjectModel;
using System.Linq;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces;
using SportRadar.Common.Collections;
using SportRadar.DAL.NewLineObjects;
using WsdlRepository;
using IocContainer;
using Ninject;
using System.Linq;
using System.Collections.Generic;
using System;

namespace SportBetting.WPF.Prism.Shared.Models.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        public SyncObservableCollection<Language> GetAllLanguages(SyncObservableCollection<Language> languages)
        {
            var languagesLn = LineSr.Instance.GetTerminalLanguages();

            List<Language> languagesToRemove = new List<Language>();
            foreach (var languageLn in languagesLn)
            {
                if (languages.Count(x => x.Id == languageLn.LanguageId) < 1 && StationRepository.SystemLanguages != null && StationRepository.SystemLanguages.Contains(languageLn.ShortName.ToUpperInvariant()))
                {
                    languages.Add(new Language(languageLn.ShortName.ToUpperInvariant()) { Id = languageLn.LanguageId });
                }
                else if (languages.Count(x => x.Id == languageLn.LanguageId) >= 1 && StationRepository.SystemLanguages != null && !StationRepository.SystemLanguages.Contains(languageLn.ShortName.ToUpperInvariant()))
                {
                    languagesToRemove.Add(languages.Where(x => x.Id == languageLn.LanguageId).FirstOrDefault());
                }
            }

            if(languagesToRemove.Count > 0)
                foreach (Language lan in languagesToRemove)
                {
                    languages.Remove(lan);
                }

            return languages;
        }

        private IStationRepository _stationRepository;
        public IStationRepository StationRepository
        {
            get
            {
                return _stationRepository ?? (_stationRepository = IoCContainer.Kernel.Get<IStationRepository>());
            }
        }
    }
}