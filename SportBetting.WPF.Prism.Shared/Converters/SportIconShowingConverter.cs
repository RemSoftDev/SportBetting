using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class SportIconShowingConverter : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            List<int> supportedSportIds = new List<int>(new int[] {1, 2, 3, 4, 5, 6, 10, 11, 15, 19, 34, 36 });

            try
            {
                int sportId = System.Convert.ToInt32(values);
                foreach (var number in supportedSportIds)
                {
                    if (number == sportId)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
