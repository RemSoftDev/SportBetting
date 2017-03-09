using System.Windows.Data;
using System;
using System.Globalization;
using System.Windows.Media;

namespace SportBetting.WPF.Prism.Converters
{
    public class TippItemBackgroundColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (value == null || !(bool)value)
                    return new SolidColorBrush(Colors.DimGray);
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
