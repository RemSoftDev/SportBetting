using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Converters
{
    public class MultiBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool returnValue = true;
            foreach (object value in values)
            {
                if (value is bool)
                    returnValue = returnValue && (bool)value;
            }
            return returnValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}