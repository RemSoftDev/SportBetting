using System;
using System.Windows;
using System.Windows.Data;
using SportBetting.WPF.Prism.Models;

namespace SportBetting.WPF.Prism.Shared
{
    public class TeamPositionChangeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {

            if (value != null)
            {
                var tempValue = Decimal.Parse(value.ToString());
                var tempValue2 = Decimal.Parse(parameter.ToString());

                if (tempValue > tempValue2)
                {
                    return 1;
                }
                if (tempValue == tempValue2)
                {
                    return 0;
                }
                return -1;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}