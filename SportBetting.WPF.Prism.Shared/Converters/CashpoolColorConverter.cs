using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class CashpoolColorConverter : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal value1 = 0;
            decimal.TryParse(values.ToString(), out value1);


            return value1 >= 0 ? Brushes.White : Brushes.Red;
            //return value1 - value2;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}