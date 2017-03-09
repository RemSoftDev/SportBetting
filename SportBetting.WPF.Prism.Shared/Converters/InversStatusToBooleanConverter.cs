using System;
using System.Windows;
using System.Windows.Data;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportBetting.WPF.Prism.Shared
{
    public class InversStatusToBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return true;

            bool returnValue = false;
            if ((eMatchStatus)value == eMatchStatus.NotStarted || (eMatchStatus)value == eMatchStatus.Undefined)
            {
                returnValue = true;
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
