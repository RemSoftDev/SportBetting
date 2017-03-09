using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class DateTimeToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            DateTime tempTime;
            DateTime.TryParse(value.ToString(), out tempTime);

            if (tempTime == DateTime.MinValue) return Visibility.Hidden;

            return Visibility.Visible;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
