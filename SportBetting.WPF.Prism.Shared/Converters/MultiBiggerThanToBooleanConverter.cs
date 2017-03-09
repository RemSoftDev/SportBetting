using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Shared
{
    public class MultiBiggerThanToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal[] tempValues = new decimal[values.Length];
            decimal tempParameter = 0;
            decimal temp = 0;

            for (int i = 0; i < values.Length; i++)
            {
                var tempValue = values[i];
                Decimal.TryParse(tempValue.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out temp);
                tempValues[i] = temp;
            }

            Decimal.TryParse(parameter.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out tempParameter);

            foreach (var value in tempValues)
            {
                if (value <= tempParameter)
                    return false;
            }
            return true;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
