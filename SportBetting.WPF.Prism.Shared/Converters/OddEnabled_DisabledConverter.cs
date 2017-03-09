using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.DAL.ViewObjects;

namespace SportBetting.WPF.Prism.Shared
{
    public class OddEnabled_DisabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            /*
             * values order: 
             * match IsEnabled
             * odd IsEnabled
             * odd IsSelected
             */

            for (int i = 0; i < values.Length; i++)
                if (values[i] == DependencyProperty.UnsetValue)
                    return false;

            bool returnValue = (bool)values[0] && (bool)values[1];

            if (!returnValue && (bool)values[2])
                returnValue = true;

            return returnValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
