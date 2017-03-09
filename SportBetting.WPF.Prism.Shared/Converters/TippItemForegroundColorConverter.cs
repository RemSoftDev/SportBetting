using System.Windows.Data;
using System;
using System.Globalization;
using System.Windows.Media;

namespace SportBetting.WPF.Prism.Converters
{
    public class TippItemForegroundColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (value == null || !(bool)value)
                    return new SolidColorBrush(Colors.DimGray);
            }

            Color color = (Color)ColorConverter.ConvertFromString("#FFEFEFEF");

            return new SolidColorBrush(color);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
