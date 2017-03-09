using System;
using System.Windows;
using System.Windows.Data;
using SportBetting.WPF.Prism.Models;

namespace SportBetting.WPF.Prism.Shared
{
    public class BooleanToBoldConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(FontWeight))
                throw new InvalidOperationException("The target must be a Visibility");

            if (parameter != null && parameter.ToString() == "decimal")
            {
                value = decimal.Parse(value.ToString()) > 0;
            }

            if (value == null)
            {
                value = false;
            }

            if (value is string)
            {
                value = true;
            }

            if (value is int)
            {
                value = (int)value > 0 ? true : false;
            }


            return (bool)value ? FontWeights.ExtraBold : FontWeights.Regular;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}