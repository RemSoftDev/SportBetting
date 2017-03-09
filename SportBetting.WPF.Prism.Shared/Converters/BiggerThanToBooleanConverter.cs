using System;
using System.Windows;
using System.Windows.Data;
using SportBetting.WPF.Prism.Models;
using System.Globalization;

namespace SportBetting.WPF.Prism.Shared
{
    public class BiggerThanToBooleanConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{

            decimal tempValue = 0;
            decimal tempValue2 = 0;

            Decimal.TryParse(value.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out tempValue);
            Decimal.TryParse(parameter.ToString(), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out tempValue2);

            return tempValue > tempValue2;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}