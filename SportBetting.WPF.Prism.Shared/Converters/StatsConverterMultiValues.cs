using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SportBetting.WPF.Prism.Shared.Models;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class StatsConverterMultiValues : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return false;


            foreach (var value in values)
            {
                if (value == null)
                {
                    return Visibility.Collapsed;
                }
                if (value is MatchStatistic)
                {
                    MatchStatistic msTemp = (MatchStatistic)value;

                    if (msTemp.Values == null)
                        return Visibility.Collapsed;
                    if (msTemp.Values.Count == 0)
                        return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
