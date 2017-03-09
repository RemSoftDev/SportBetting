using System;
using System.Windows;
using System.Windows.Data;
using SportBetting.WPF.Prism.Models;

namespace SportBetting.WPF.Prism.Shared
{
	public class BooleanToVisibilityCollapsedConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(Visibility))
				throw new InvalidOperationException("The target must be a Visibility");

            if (parameter != null && parameter.ToString() == "decimal")
            {
                string s = value.ToString();
                // HS 27.01.15
                // HACK to fix switching to Russian. 
                // "0.00" is coming from somwehere. Actually "." symbol makes it impossible for decimal to parse string. Has to be replaced with ","
                // TODO: find where "." is coming from                
                if (s.Contains("."))
                    s = s.Replace(".", ",");
                value = decimal.Parse(s) > 0;
            }

            if (value == null)
            {
                value = false;
            }

            if (value is string)
            {
                value = true;
            }
            
            if (value is int)
            {
                value = (int) value > 0 ? true : false;
            }


            return (bool) value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}