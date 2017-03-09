using System;
using System.Globalization;
using System.Windows.Data;

// IValueConverter


namespace SportBetting.WPF.Prism.Modules.Keyboard
{
    #region CapsLockToTextConverter
    /// <summary>
    /// Converts a boolean to a string that expresses the state of the Caps Lock button
    /// </summary>
    /// <example>
    /// If CapsLock is on => returns "Caps ON" otherwise just "Caps"
    /// <code>
    /// Text="{Binding IsCapsLock, Converter=CapsLockToTextConverter}"
    /// </code>
    /// </example>
    public class CapsLockToTextConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            //if (targetType != typeof(string))
            //{
            //   throw new InvalidOperationException("The target for a CapsTextConverter must be a String!");
            //}
            bool bCapsLockOn = (bool)value;
            // For Spanish (see http://en.wikipedia.org/wiki/Keyboard_layout#Spanish_.28Spain.29.2C_aka_Spanish_.28International_sort.29
            // Caps Lock would be  "Bloq Mayús" 
            if (bCapsLockOn)
            {
                return "Caps ON";
            }
            else
            {
                return "Caps";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion CapsTextConverter

    #region ShiftLockToTextConverter
    /// <summary>
    /// Converts a boolean to a string that expresses the state of the Shift Lock button
    /// </summary>
    /// <example>
    /// If ShiftLock is on => returns "Shift ON" otherwise just "Shift"
    /// <code>
    /// Text="{Binding IsShiftLock, Converter=ShiftLockToTextConverter}"
    /// </code>
    /// </example>
    public class ShiftLockToTextConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool bShiftLockOn = (bool)value;
            if (bShiftLockOn)
            {
                return "Shift ON";
            }
            else
            {
                return "Shift";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion CapsTextConverter

    #region WhichKBLayoutToStringConverter
    /// <summary>
    /// Converts between a WhichKeyboardLayout enum value and it's string equivalent
    /// </summary>
    public class WhichKBLayoutToStringConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string sValue = "?";
            try
            {
                //Console.WriteLine("in WhichKBLayoutToStringConverter, targetType is " + targetType.ToString() + ", value.ToString yields " + value.ToString());
                sValue = value.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an exception within WhichKBLayoutToStringConverter: " + e.Message);
            }
            return sValue;
        }
        #endregion

        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            WhichKeyboardLayout keyboardLayout = WhichKeyboardLayout.Unknown;
            if (!Enum.TryParse((String)value, out keyboardLayout))
            {
                Console.Write("in WhichLanguageToStringConverter.ConvertBack, failed to parse \"");
                Console.WriteLine(value + "\".");
            }
            //Console.WriteLine("in WhichKBLayoutToStringConverter.ConvertBack, converted to " + keyboardLayout);
            return keyboardLayout;
        }
        #endregion
    }
    #endregion
}
