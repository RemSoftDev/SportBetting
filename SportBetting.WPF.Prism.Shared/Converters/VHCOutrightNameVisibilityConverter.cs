using System;
using System.Windows;
using System.Windows.Data;
using SportRadar.Common.Collections;
using SportRadar.DAL.ViewObjects;

namespace SportBetting.WPF.Prism.Shared
{
    public class VHCOutrightNameVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility returnValue = Visibility.Collapsed;

            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            if (value is SyncObservableCollection<VirtualHorsesCompetitor>)
            {
                if(((SyncObservableCollection<VirtualHorsesCompetitor>)value).Count<=0)
                    returnValue = Visibility.Visible;
            }

            //if (value is string)
            //{
            //    if ((value as String) == "Others")
            //        returnValue = Visibility.Visible;
            //}

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
