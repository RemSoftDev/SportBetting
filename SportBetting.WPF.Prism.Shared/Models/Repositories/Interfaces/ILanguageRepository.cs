using System.Collections.Generic;
using System.Collections.ObjectModel;
using SportRadar.Common.Collections;

namespace SportBetting.WPF.Prism.Shared.Models.Repositories.Interfaces
{
    public interface ILanguageRepository
    {
        SyncObservableCollection<Language> GetAllLanguages(SyncObservableCollection<Language> languages);
    }
}