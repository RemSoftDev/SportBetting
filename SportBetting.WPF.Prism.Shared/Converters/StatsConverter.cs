using System;
using System.Globalization;
using System.Windows.Data;
using SportBetting.WPF.Prism.Shared.Models;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class StatsConverter : IValueConverter
    {

        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            bool emptyStats;
            if (values == null)
                return false;

            if (values is MatchStatistic)
            {
                MatchStatistic msTemp = (MatchStatistic)values;

                if (msTemp.Values == null)
                    return false;
                if (msTemp.Values.Count == 0)
                    return false;
                foreach (var value in msTemp.Values)
                {
                    if (value.Value != null && !value.Value.Equals("0") && !value.Name.Contains("BTR_SUPER_ID"))
                    {
                        return true;
                    } 
                }

                return false;
            }

            if (values != null)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
