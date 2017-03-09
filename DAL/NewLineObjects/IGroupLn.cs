using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public interface IGroupLn
    {
        GroupVw GroupView { get; }
        GroupSportExternalState GroupSport { get; }
        long GroupId { get; set; }
        string KeyName { get; }
        string Type { get; set; }
        GroupTournamentExternalState GroupTournament { get;  }
        ObservableProperty<long?> ParentGroupId { get;  }
        ObservableProperty<bool> Active { get; }
        ObservableProperty<int> Sort { get; }
        GroupLn ParentGroup { get;  }
        long SvrGroupId { get; }
        string GetDisplayName(string language);
    }
}
