
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using IocContainer;
using Ninject;
using TranslationByMarkupExtension;

namespace SportBetting.WPF.Prism.Shared
{
    public class MultistringTagConverter : IValueConverter
    {
        #region Constructors
        #endregion

        #region IValueConverter Members
        private ITranslationProvider _translationProvider;
        public ITranslationProvider TranslationProvider
        {
            get
            {
                return _translationProvider ?? (_translationProvider = IoCContainer.Kernel.Get<ITranslationProvider>());
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MultistringTag)
            {
                return TranslationProvider.Translate((MultistringTag)value);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
