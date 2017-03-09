using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using Emulator;

namespace Emulator.Converters
{
    class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            string param = (string)parameter;

            if (param == "DIRECT")
            {
                if (text == Emulator.MainWindow.DETECTED || text == Emulator.MainWindow.CONNECTED || text == Emulator.MainWindow.ENABLE)
                {
                    return Brushes.LimeGreen;
                }
                return Brushes.Red;
            }

            if (text == Emulator.MainWindow.DETECTED || text == Emulator.MainWindow.CONNECTED || text == Emulator.MainWindow.ENABLE)
            {
                return Brushes.Red;
            }
            return Brushes.LimeGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
