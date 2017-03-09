using System;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class ScoresModel
    {
        private string _homeColor;
        private string _awayColor;
        private string _drawColor = "DimGray";

        public string AwayGoals { get; set; }
        public string HomeGoals { get; set; }

        public string HomeColor
        {
            get { return _homeColor ?? "Blue"; }
            set { _homeColor = value; }
        }

        public string AwayColor
        {
            get
            { return _awayColor ?? "White"; }
            set { _awayColor = value; }
        }

        public string Color
        {
            get
            {

                if (Int32.Parse(AwayGoals) > Int32.Parse(HomeGoals))
                {
                    return AwayColor;
                }
                else if (Int32.Parse(AwayGoals) == Int32.Parse(HomeGoals))
                {
                    return _drawColor;
                }
                else
                {
                    return HomeColor;
                }
            }
        }
        public string Score
        {
            get { return String.Format("{0} : {1}", HomeGoals, AwayGoals); }
        }
        public string LineWidth
        {
            get
            {
#if BETCENTER
                return String.Format("{0}", ((Int32.Parse(HomeGoals) + Int32.Parse(AwayGoals)) * 20) + 60);
#else
                return String.Format("{0}", ((Int32.Parse(HomeGoals) + Int32.Parse(AwayGoals)) * 35) + 100);
#endif
            }
        }
        public string Margin
        {
            get
            {
                if (Int32.Parse(HomeGoals) > Int32.Parse(AwayGoals))
                {
#if BETCENTER
                    int marginRight = (Int32.Parse(HomeGoals) - Int32.Parse(AwayGoals)) * 20;
#else
                    int marginRight = (Int32.Parse(HomeGoals) - Int32.Parse(AwayGoals)) * 35;
#endif
                    return String.Format("{0}, {1}, {2}, {3}", 0, 0, marginRight, 5);
                }
                else if (Int32.Parse(HomeGoals) < Int32.Parse(AwayGoals))
                {
#if BETCENTER
                    int marginLeft = (Int32.Parse(AwayGoals) - Int32.Parse(HomeGoals)) * 20;
#else
                    int marginLeft = (Int32.Parse(AwayGoals) - Int32.Parse(HomeGoals)) * 35;
#endif
                    return String.Format("{0}, {1}, {2}, {3}", marginLeft, 0, 0, 5);
                }
                else
                {
                    return "0, 0, 0, 5";
                }
            }
        }
    }
}
