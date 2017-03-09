using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Navigation;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class ToUpperCaseConverter : IValueConverter
    {

        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToString().ToUpperInvariant();
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
