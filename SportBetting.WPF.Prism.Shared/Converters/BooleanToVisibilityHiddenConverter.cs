using System;
using System.Windows;
using System.Windows.Data;

namespace SportBetting.WPF.Prism.Shared
{
    public class BooleanToVisibilityHiddenConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(Visibility))
				throw new InvalidOperationException("The target must be a Visibility");

            bool returnValue = false;
            if (value == null)
                return Visibility.Hidden;

            if (value is string)
            {
                returnValue = System.Convert.ToBoolean((string)value);
            }
            else
            {
                returnValue = (bool)value;
            }
            return returnValue ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}