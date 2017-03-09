using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.DAL.ViewObjects;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class VFLMatchButton
    {

        public MatchVw MatchVw { get; set; }

        public VFLMatchButton(string homeCompetitor, string awayCompetitor, long btrMatchId, MatchVw match)
        {
            HomeCompetitorName = homeCompetitor;
            AwayCompetitorName = awayCompetitor;
            Id = btrMatchId;
            MatchVw = match;
        }
        public long Id { get; set; }
        public int Channel { get; set; }

        private string _homeCompetitorName;
        public string HomeCompetitorName
        {
            get { return _homeCompetitorName; }
            set { _homeCompetitorName = value; }
        }

        private string _awayCompetitorName;
        public string AwayCompetitorName
        {
            get { return _awayCompetitorName; }
            set { _awayCompetitorName = value; }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }

    }
}
