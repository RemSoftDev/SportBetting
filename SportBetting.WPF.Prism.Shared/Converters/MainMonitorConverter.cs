using System;
using TranslationByMarkupExtension;
using Ninject;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using IocContainer;

namespace SportBetting.WPF.Prism.Shared
{
    public class MainMonitorToVisibiltyConverter : IValueConverter
    {
        public ITranslationProvider TranslationProvider
        {
            get { return IoCContainer.Kernel.Get<ITranslationProvider>(); }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (String.Compare((string)value, TranslationProvider.Translate(MultistringTags.TERMINAL_MAIN_MONITOR)) == 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MainMonitorToVisibiltyInverseConverter : IValueConverter
    {
        public ITranslationProvider TranslationProvider
        {
            get { return IoCContainer.Kernel.Get<ITranslationProvider>(); }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (String.Compare((string)value, TranslationProvider.Translate(MultistringTags.TERMINAL_MAIN_MONITOR)) == 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}