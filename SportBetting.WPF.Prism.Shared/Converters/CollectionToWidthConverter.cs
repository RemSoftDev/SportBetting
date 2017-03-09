using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Collections;

namespace SportBetting.WPF.Prism.Shared
{
    public class CollectionToWidthConverter : IValueConverter
    {
        #region Constructors
        #endregion

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            

            string[] strParameters = ((parameter == null) ? string.Empty : parameter.ToString()).Split('-');

            int width;
            if (!((strParameters.Length > 0) && int.TryParse(strParameters[0], out width)))
            {
                width = 0;
            }

            int margin;
            if (!((strParameters.Length > 1) && int.TryParse(strParameters[1], out margin)))
            {
                margin = 0;
            }

            int count = 0;

            ICollection collection = value as ICollection;
            if (collection != null)
            {
                count = collection.Count;
            }
            else if (value is int)
            {
                count = (int)value;
            }

            return (double)((count * width) + margin);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}