using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportRadar.DAL.ViewObjects;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    [WsdlServiceAsyncAspect]
    public class OddIncreaseDecreaseConverter : IMultiValueConverter
    {
        private static BitmapImage _image;
        private static readonly BitmapImage RedImage =(BitmapImage)new ResolveImagePath("red.png").ProvideValue(null);
        private static readonly BitmapImage GreenImage = (BitmapImage) new ResolveImagePath("green.png").ProvideValue(null);

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || !(values[0] is OddVw)) return null;

            OddVw odd = (OddVw)values[0];
         
            switch (odd.IsIncreased)
            {
                case null:
                    return null;
                case false:
                    _image = RedImage;
                    break;
                case true:
                    _image = GreenImage;
                    break;
            }
            return _image;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
