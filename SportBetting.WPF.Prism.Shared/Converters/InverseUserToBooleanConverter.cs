
using System;
using System.Windows;
using System.Windows.Data;
using SportBetting.WPF.Prism.Models;

namespace SportBetting.WPF.Prism.Shared
{
    public class InverseUserToBooleanConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
		    if (value == null)
		        value = "";
		    var type = value.GetType().ToString();
		    var targetTypestr = parameter.ToString();
            return !type.Contains(targetTypestr);
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}