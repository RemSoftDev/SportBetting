using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class TicketTextColorConverter : IValueConverter
    {

        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage temp = (BitmapImage) values;
            var image = (BitmapImage)new ResolveImagePath("ticket-button-sel.png").ProvideValue(null);
                
            if (image.UriSource.Equals(temp.UriSource))
            {
                return "Black";
            }
            return "White";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
