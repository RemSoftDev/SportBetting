using System;
using System.Windows;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Shared
{
    public class InversTimeToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return Visibility.Collapsed; if (value == null)
                return Visibility.Collapsed;

            bool returnValue = false;
            if (value is DateTime)
            {
                returnValue = ((DateTime)value).Date == DateTime.Today || ((DateTime)value).Date == DateTime.Today.AddDays(-1);
            }

            var returnvalue = !returnValue ? Visibility.Visible : Visibility.Collapsed;
            return returnvalue;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
