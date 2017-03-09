using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Collections;

namespace SportBetting.WPF.Prism.Shared
{
    public class CollectionToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            ICollection collection = value as ICollection;

            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            return (((collection == null) || (collection.Count == 0)) ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}