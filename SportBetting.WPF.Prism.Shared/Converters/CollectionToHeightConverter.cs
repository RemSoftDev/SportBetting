using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Collections;

namespace SportBetting.WPF.Prism.Shared
{
    public class CollectionToHeightConverter : IValueConverter
    {
        #region Constructors
        #endregion

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;
            double calcHeight = 0,
                   lineHeight = 50;
            
            ICollection collection = value as ICollection;
            if (collection != null)
            {
                count = collection.Count;
            }
            else if (value is int)
            {
                count = (int)value;
            }

            if (count > 0)
            {
                int columnCount = (count < 12) ? 1 : ((count < 36) ? 2 : 3);

                double division = (double) count / columnCount;
                calcHeight = (double) Math.Ceiling(division);
            }

            return calcHeight * lineHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}