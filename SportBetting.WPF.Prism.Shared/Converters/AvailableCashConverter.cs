using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Converters
{
    public class AvailableCashConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal value1 = 0;
            decimal.TryParse(values[0].ToString(), out value1);

            decimal value2 = 0;
            decimal.TryParse(values[1].ToString(),out value2);

            return String.Format("{0:0.00}", value1 - value2);
            //return value1 - value2;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}