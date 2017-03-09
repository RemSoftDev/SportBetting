using System;
using System.Windows;
using System.Windows.Data;
using SportBetting.WPF.Prism.Models;

namespace SportBetting.WPF.Prism.Shared
{
    public class BiggerThanToVisibilityCollapsedConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {

            decimal tempValue = 0;
            Decimal.TryParse(value.ToString(), out tempValue);
            if (tempValue == null)
                return Visibility.Collapsed;

            var tempValue2 = Decimal.Parse(parameter.ToString());

            return tempValue > tempValue2 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}