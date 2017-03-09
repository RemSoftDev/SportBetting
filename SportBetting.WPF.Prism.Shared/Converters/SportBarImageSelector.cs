using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SportBetting.WPF.Prism.Shared.Converters;
using SportRadar.Common.Logs;
using SportRadar.DAL.OldLineObjects;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class SportBarImageSelector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object image = null;
            if (parameter != null)
            {
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
