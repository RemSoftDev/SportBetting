using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Shared.Annotations;
using SportRadar.DAL.OldLineObjects;
using System.IO;
using System.Windows.Media.Imaging;
using SportRadar.DAL.NewLineObjects;
using TranslationByMarkupExtension;
using IocContainer;
using System.Windows.Documents;
using Ninject;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class MatchStatistic : INotifyPropertyChanged
    {
        long HomeCompId = 0;
        long AwayCompId = 0;

        public MatchStatistic(long lMatchId, long? lTournamentId)
        {

            MatchInfosLn temp = LineSr.Instance.AllObjects.MatchInfos.SafelyGetValue(lMatchId);
            if(temp !=null)
                Values = temp.external_state.StatisticValues.ToList();

            //find competitors
            if (temp != null)
            {
                Int64.TryParse(Values.Where(x => x.Name == "COMPETITOR_1_BTR_SUPER_ID").Select(x => x.Value).FirstOrDefault(), out HomeCompId);
                Int64.TryParse(Values.Where(x => x.Name == "COMPETITOR_2_BTR_SUPER_ID").Select(x => x.Value).FirstOrDefault(), out AwayCompId);
                if (AwayCompId != 0 && HomeCompId != 0)
                {
                    this.HomeCompetitorInfo = LineSr.Instance.AllObjects.CompetitorInfos.SafelyGetValue(HomeCompId);
                    this.AwayCompetitorInfo = LineSr.Instance.AllObjects.CompetitorInfos.SafelyGetValue(AwayCompId);

                    if (HomeCompetitorInfo != null && AwayCompetitorInfo != null)
                    {
                        TshirtHomeImage = LoadImage(Convert.FromBase64String(HomeCompetitorInfo.TshirtHome));
                        TshirtAwayImage = LoadImage(Convert.FromBase64String(AwayCompetitorInfo.TshirtAway));

                        var homecolor = HomeCompetitorInfo.external_state.StatisticValues.Where(x => x.Name == "SHIRT_1_COLOR").Select(x => x.Value).FirstOrDefault();
                        HomeTeamColor = String.Format("#{0}", homecolor);

                        var awaycolor = AwayCompetitorInfo.external_state.StatisticValues.Where(x => x.Name == "SHIRT_2_COLOR").Select(x => x.Value).FirstOrDefault();
                        AwayTeamColor = String.Format("#{0}", awaycolor);
                    }
                }
            }

            //find tournament and competitors in tournament
            if (temp != null && lTournamentId != null)
            {
                this.TournamentInfo = LineSr.Instance.AllObjects.TournamentInfos.SafelyGetValue(lTournamentId.Value);
            }

        }

        #region Performance

        private List<StatisticValueSr> _values;
        private string _tshirtAway;
        private string _tshirtHome;
        private BitmapImage _tshirtAwayImage;
        private BitmapImage _tshirtHomeImage;
        private CompetitorInfosLn _homeCompetitorInfo;
        private CompetitorInfosLn _awayCompetitorInfo;
        private TournamentInfosLn _tournamentInfo;

        public List<StatisticValueSr> Values
        {
            get { return _values; }
            set { _values = value; }
        }

        public string HomeTournamentSortPosition
        {
            get
            {
                if (TournamentInfo != null)
                {
                    var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompetitorInfo.SuperBtrId).FirstOrDefault();
                    if (competitor != null)
                    {
                        return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_SORT_POSITION").Select(x => x.Value).FirstOrDefault();
                    }
                }
                return string.Empty;
            }
        }

        public CompetitorInfosLn HomeCompetitorInfo
        {
            get { return _homeCompetitorInfo; }
            set { _homeCompetitorInfo = value; }
        }

        public CompetitorInfosLn AwayCompetitorInfo
        {
            get { return _awayCompetitorInfo; }
            set { _awayCompetitorInfo = value; }
        }

        public TournamentInfosLn TournamentInfo
        {
            get { return _tournamentInfo; }
            set { _tournamentInfo = value; }
        }
        #endregion

        #region Statistics
        public BitmapImage TshirtAwayImage
        {
            get
            {
                return _tshirtAwayImage ??
                       new BitmapImage(new Uri(new ImagePathConverter().Convert(null, null, "AwayTeam.png", null).ToString()));
            }
            set { _tshirtAwayImage = value; }
        }

        public BitmapImage TshirtHomeImage
        {
            get 
            { 
                return _tshirtHomeImage ?? new BitmapImage(new Uri(new ImagePathConverter().Convert(null, null, "HomeTeam.png", null).ToString())); 
            }
            set { _tshirtHomeImage = value; }
        }

        public string TshirtAway
        {
            get
            {
                if (AwayCompetitorInfo != null)
                {
                    var competitor = AwayCompetitorInfo.TshirtAway;
                    if (competitor != null)
                    {
                        return competitor;
                    }

                }
                return null;
            }
        }

        public string TshirtHome
        {
            get
            {
                if (HomeCompetitorInfo != null)
                {
                    var competitor = HomeCompetitorInfo.TshirtHome;
                    if (competitor != null)
                    {
                        return competitor;
                    }
                }
                return null;
            }
        }

        #endregion

        #region LeagueStatistic

        //League table stats
        public string HomePosition
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_SORT_POSITION").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string HomePositionChange
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_POSITION_CHANGE").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string HomeMatchesPlayed
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_MATCHES_PLAYED").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string HomeMatchesWon
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_MATCHES_WON").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string HomeMatchesDraw
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_MATCHES_DRAW").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string HomeMatchesLost
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_MATCHES_LOST").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string HomeGoalsFor
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_GOALS_FOR").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string HomeGoalsAgainst
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_GOALS_AGAINST").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string HomePoints
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == HomeCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_POINTS").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayPosition
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_SORT_POSITION").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayPositionChange
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_POSITION_CHANGE").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayMatchesPlayed
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_MATCHES_PLAYED").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayMatchesWon
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_MATCHES_WON").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayMatchesDraw
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_MATCHES_DRAW").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayMatchesLost
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_MATCHES_LOST").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayGoalsFor
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_GOALS_FOR").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayGoalsAgainst
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_GOALS_AGAINST").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        public string AwayPoints
        {
            get
            {
                if (TournamentInfo != null)
                {
                    if (TournamentInfo.external_state.CompetitorsContainer != null)
                    {
                        var competitor = TournamentInfo.external_state.CompetitorsContainer.Where(x => x.CompetitorInfoId == AwayCompId).FirstOrDefault();
                        if (competitor != null)
                        {
                            return competitor.StatisticValues.Where(x => x.Name == "TOURNAMENT_POINTS").Select(x => x.Value).FirstOrDefault();
                        }
                    }
                }
                return null;
            }
        }

        #endregion

        #region Performance

        public string HomeLastMatchesTotal
        {
            get 
            {
                string lastMatchesCount = this.HomeLastMatchesCount ?? "";
                return TranslationProvider.Translate(MultistringTags.LAST_MATCHES_TOTAL,  lastMatchesCount);
            }
        }

        public string HomeLastMatchesAverage
        {
            get 
            {
                string lastMatchesCount = this.HomeLastMatchesCount ?? "";
                return TranslationProvider.Translate(MultistringTags.LAST_MATCHES_AVERAGE,  lastMatchesCount);
            }
        }

        public string AwayLastMatchesTotal
        {
            get
            {
                string lastMatchesCount = this.AwayLastMatchesCount ?? "";
                return TranslationProvider.Translate(MultistringTags.LAST_MATCHES_TOTAL, lastMatchesCount);
            }
        }

        public string AwayLastMatchesAverage
        {
            get
            {
                string lastMatchesCount = this.AwayLastMatchesCount ?? "";
                return TranslationProvider.Translate(MultistringTags.LAST_MATCHES_AVERAGE, lastMatchesCount);
            }
        }

        public string HomeLastMatchesCount
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_1_TOURNAMENT_LAST_MATCHES_COUNT").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string HomeLastMatchesTendencyString
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_1_TOURNAMENT_LAST_5_MATCHES_PERFORMANCE_RECORD").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public List<PerformanceModel> HomeLastMatchesTendency
        {
            get
            {
                if (Values != null)
                {
                    string performance = Values.Where(x => x.Name == "COMPETITOR_1_TOURNAMENT_LAST_5_MATCHES_PERFORMANCE_RECORD").Select(x => x.Value).FirstOrDefault();
                    if (performance == null)
                        return null;

                    string[] performanceState = performance.Split(',');
                    List<PerformanceModel> PerformanceList = new List<PerformanceModel>();
                    foreach (string result in performanceState)
                    {
                        PerformanceList.Add(new PerformanceModel() { State = result });
                    }
                    return PerformanceList;
                }

                return null;
            }
        }

        public string HomeLastMatchesPerformancePercent
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_1_TOURNAMENT_LAST_MATCHES_PERFORMANCE_PERCENT").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string HomeLastMatchesGoalsFor
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_1_TOURNAMENT_LAST_MATCHES_GOALS_FOR").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string HomeLastMatchesGoalsAgainst
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_1_TOURNAMENT_LAST_MATCHES_GOALS_AGAINST").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string HomeLastMatchesAverageGoalsFor
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_1_TOURNAMENT_LAST_MATCHES_AVERAGE_GOALS_FOR").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string HomeLastMatchesAverageGoalsAgainst
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_1_TOURNAMENT_LAST_MATCHES_AVERAGE_GOALS_AGAINST").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string AwayLastMatchesCount
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_2_TOURNAMENT_LAST_MATCHES_COUNT").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string AwayLastMatchesTendencyString
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_2_TOURNAMENT_LAST_5_MATCHES_PERFORMANCE_RECORD").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public List<PerformanceModel> AwayLastMatchesTendency
        {
            get
            {
                if (Values != null)
                {
                    string performance = Values.Where(x => x.Name == "COMPETITOR_2_TOURNAMENT_LAST_5_MATCHES_PERFORMANCE_RECORD").Select(x => x.Value).FirstOrDefault();
                    if (performance == null)
                        return null;

                    string[] performanceState = performance.Split(',');
                    List<PerformanceModel> PerformanceList = new List<PerformanceModel>();
                    foreach (string result in performanceState)
                    {
                        PerformanceList.Add(new PerformanceModel() { State = result });
                    }
                    return PerformanceList;
                }

                return null;
            }
        }

        public string AwayLastMatchesPerformancePercent
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_2_TOURNAMENT_LAST_MATCHES_PERFORMANCE_PERCENT").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string AwayLastMatchesGoalsFor
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_2_TOURNAMENT_LAST_MATCHES_GOALS_FOR").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string AwayLastMatchesGoalsAgainst
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_2_TOURNAMENT_LAST_MATCHES_GOALS_AGAINST").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string AwayLastMatchesAverageGoalsFor
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_2_TOURNAMENT_LAST_MATCHES_AVERAGE_GOALS_FOR").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string AwayLastMatchesAverageGoalsAgainst
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "COMPETITOR_2_TOURNAMENT_LAST_MATCHES_AVERAGE_GOALS_AGAINST").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        #endregion

        #region Head to Head

        public string HeadToHeadLastFiveMatchesString
        {
            get
            {
                if (Values != null)
                {
                    return Values.Where(x => x.Name == "HEAD_TO_HEAD_LAST_5_MATCHES").Select(x => x.Value).FirstOrDefault();
                }
                return null;
            }
        }

        public string HomeTeamColor { get; set; }

        public string AwayTeamColor { get; set; }


        public List<ScoresModel> HeadToHeadLastFiveMatches
        {
            get
            {
                if (Values != null)
                {
                    string headToHead = Values.Where(x => x.Name == "HEAD_TO_HEAD_LAST_5_MATCHES").Select(x => x.Value).FirstOrDefault();
                    if (headToHead == null)
                        return null;

                    string[] headToHeadScores = headToHead.Split(';');
                    List<ScoresModel> Scores = new List<ScoresModel>();
                    foreach (string score in headToHeadScores)
                    {
                        string[] teamGoals = score.Split(':');
                        Scores.Add(new ScoresModel() { HomeGoals = teamGoals[0], AwayGoals = teamGoals[1], HomeColor = HomeTeamColor, AwayColor = AwayTeamColor });
                    }
                    return Scores;
                }

                return null;
            }
        }

        public string HomeTeamPerformancePercent
        {
            get 
            {
                string performancePercent = this.HomeLastMatchesPerformancePercent ?? "";
                return String.Format("{0}%", performancePercent);
            }
        }

        public string AwayTeamPerformancePercent
        {
            get
            {
                string performancePercent = this.AwayLastMatchesPerformancePercent ?? "";
                return String.Format("{0}%", performancePercent);
            }
        }

        #endregion

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static ITranslationProvider TranslationProvider
        {
            get { return IoCContainer.Kernel.Get<ITranslationProvider>(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

   
    }
}
