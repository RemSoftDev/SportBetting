using System;
using System.Globalization;
using System.Windows.Data;
using SportBetting.WPF.Prism.Modules.Aspects;

namespace SportBetting.WPF.Prism.Shared
{
    [WsdlServiceAsyncAspect]
    public class MatchOneTwoLineConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (values == null)  
                return false;

            double TeamsHolder = (double)values[0];
            double HomeTeamHolder = (double)values[1];
            double AwayTeamHolder = (double)values[2];
            double TeamsScore = (double)values[3];

            double _space = TeamsHolder - HomeTeamHolder - AwayTeamHolder - TeamsScore - 15;
            // if home and away team info does not fit in ome line we set two line configuration to true
            bool _twoLines = (_space < 0) ? true : false;

            return _twoLines;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

