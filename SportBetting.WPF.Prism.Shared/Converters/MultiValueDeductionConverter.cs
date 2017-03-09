using System;
using System.Globalization;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Shared
{
    public class MultiValueDeductionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            decimal startValue = 0;
            if (values != null)
            {
                startValue = (decimal)values[0];
                for (int i = 1; i < values.Length; i++)
                {
                    startValue -= (decimal) values[i];
                }
            }
            return startValue.ToString(CultureInfo.InvariantCulture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}