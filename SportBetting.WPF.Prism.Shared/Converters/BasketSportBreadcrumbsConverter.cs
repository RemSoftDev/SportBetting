using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Converters
{
    public class BasketSportBreadcrumbsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string sBreadcrumbs = "";
            foreach (object value in values)
            {
                if (!(value is string))
                    continue;
                // try to avoid duplicates in Sport and Category.
                if ((string)value + " - " != sBreadcrumbs)
                    sBreadcrumbs += (string)value + " - ";
            }
            var resultStr = "";
            if (sBreadcrumbs.Length >= 3)
                resultStr = sBreadcrumbs.Substring(0, sBreadcrumbs.Length - 3);
            return resultStr;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
