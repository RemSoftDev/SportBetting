using System;
using System.Globalization;
using System.Windows.Data;
using SportBetting.WPF.Prism.Modules.Aspects;

namespace SportBetting.WPF.Prism.Shared
{
    [WsdlServiceAsyncAspect]
    public class DivideBlockWidthConverter : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {

            if (values == null)
                return false;

            double WholeDataHolder = (double)values;
            double pieces = 1;
            double.TryParse(parameter.ToString(), out pieces);

            double _width = WholeDataHolder / pieces;

            return _width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

