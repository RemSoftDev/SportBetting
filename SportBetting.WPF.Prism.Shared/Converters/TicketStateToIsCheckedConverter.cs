using System;
using System.Windows.Data;
using Shared;

namespace SportBetting.WPF.Prism.Shared.Converters
{
    public class TicketStateToIsCheckedConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var invert = parameter.ToString().Contains("invert");
            parameter = parameter.ToString().Replace("invert", "");
            TicketStates state;
            Enum.TryParse(value.ToString(), out state);
            TicketStates stateParameter;
            Enum.TryParse(parameter.ToString(), out stateParameter);

            if (invert)
                return state != stateParameter;

            return state == stateParameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            TicketStates state;
            Enum.TryParse(parameter.ToString(), out state);
            return state;
        }

        #endregion
    }
}