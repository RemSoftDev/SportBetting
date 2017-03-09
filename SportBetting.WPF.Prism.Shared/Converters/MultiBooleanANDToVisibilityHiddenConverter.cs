using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Converters
{
    public class MultiBooleanANDToVisibilityHiddenConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = true;
            foreach (object value in values)
            {
                if (value is bool)
                    visible = visible && (bool)value;
            }

            if (visible)
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}