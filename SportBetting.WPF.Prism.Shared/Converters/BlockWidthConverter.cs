using System;
using System.Globalization;
using System.Windows.Data;
using SportBetting.WPF.Prism.Modules.Aspects;

namespace SportBetting.WPF.Prism.Shared
{
    [WsdlServiceAsyncAspect]
    public class BlockWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (values == null)  
                return false;

            double WholeDataHolder = (double)values[0];
            double NextDataHolder = (double)values[1];

            double _width = WholeDataHolder - NextDataHolder - 10;

            return _width;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

