using System;
using System.Windows;
using System.Windows.Data;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportBetting.WPF.Prism.Shared
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            if (value == null)
                return Visibility.Collapsed;

            bool returnValue = false;

            if ((eMatchStatus)value == eMatchStatus.NotStarted)
            {
                returnValue = true;
            }
            if ((eMatchStatus)value == eMatchStatus.Undefined)
            {
                returnValue = true;
            }

            return returnValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
