using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace JHLib
{
    #region The Visibility value converters: A set of four different value converters that map a Boolean (or a nullable-Boolean) to a Visibility value.

    #region BooleanToVisibleOrCollapsedConverter
    /// <summary>
    /// This converts between a boolean and a WPF visibility enum.
    /// true maps to Visible, false maps to Collapsed
    /// </summary>
    /// <remarks>
    /// The source for this derived from this article: http://www.timhibbard.com/Blog.aspx
    /// It's for the scenario in which I wish to make a control visible based upon the state of a checkbox (for example).
    /// Here is it's sample usage in XAML:
    /// <code>
    /// <Window x:Class="DF.Windows.Window1"
    ///  xmlns:converters="clr-namespace:DLib.Converters">
    ///     <StackPanel Orientation="Vertical">
    ///         <StackPanel.Resources>
    ///             <converters:BooleanToHiddenVisibility x:Key="boolToVis"/>
    ///         </StackPanel.Resources>
    ///         <CheckBox Content="Check to show text box below me" Name="ckbxViewTextBox"/>
    ///         <TextBox Text="Only seen when above is checked!" Visibility="{Binding Path=IsChecked, ElementName=ckbxViewTextBox, Converter={StaticResource boolToVis}}"/>
    ///     </StackPanel>
    /// </Window>
    /// </code>
    /// </remarks>
    public class BooleanToVisibleOrCollapsedConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility rv = Visibility.Visible;
            try
            {
                bool bValue = true;
                if (bool.TryParse(value.ToString(), out bValue))
                {
                    if (!bValue)
                    {
                        rv = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Probably should NOT be using an exception here like this.  jh
                // At least, we should log it.
                Console.WriteLine("There was an exception within BooleanToVisibleOrCollapsedConverter: " + e.Message);
            }
            return rv;
        }
        #endregion

        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
        #endregion
    }
    #endregion

    #region BooleanToNotVisibleOrCollapsedConverter
    /// <summary>
    /// The opposite of the above.
    /// true maps to Collapsed, false maps to Visible
    /// </summary>
    public class BooleanToNotVisibleOrCollapsedConverter : IValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility rv = Visibility.Visible;
            try
            {
                bool bValue = true;
                if (bool.TryParse(value.ToString(), out bValue))
                {
                    if (bValue)
                    {
                        rv = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception e)
            {
                //TODO: Probably should NOT be using an exception here like this.  jh
                // At least, we should log it.
                Console.WriteLine("There was an exception within BooleanToNotVisibleOrCollapsedConverter: " + e.Message);
            }
            //Console.WriteLine("in BooleanToNotVisibleOrCollapsedConverter, returning " + rv.ToString());
            return rv;
        }
        #endregion

        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
        #endregion
    }
    #endregion

    #region BooleanToVisibleOrHiddenConverter
    /// <summary>
    /// This subclass of IValueConverter converts a boolean, or a nullable-boolean (as from the IsChecked property of a CheckBox)
    /// to either Visibility.Visible (if the value is True) or Visibility.Hidden (if the value is False).
    /// </summary>
    public class BooleanToVisibleOrHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bOn = false;
            Visibility rv = Visibility.Visible;
            Boolean? nbValue = value as Boolean?;
            if (nbValue == null)
            {
                bool bV = (bool)value;
                if (bV)
                {
                    bOn = true;
                }
            }
            else
            {
                if (nbValue.HasValue && nbValue.Value)
                {
                    bOn = true;
                }
            }
            if (!bOn)
            {
                rv = Visibility.Hidden;
            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region BooleanToNotVisibleOrHiddenConverter
    /// <summary>
    /// The opposite of the above.
    /// True maps to Hidden, False maps to Visible
    /// </summary>
    public class BooleanToNotVisibleOrHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bOn = false;
            Visibility rv = Visibility.Visible;
            Boolean? nbValue = value as Boolean?;
            if (nbValue == null)
            {
                bool bV = (bool)value;
                if (bV)
                {
                    bOn = true;
                }
            }
            else // it's a nullable-Boolean
            {
                if (nbValue.HasValue && nbValue.Value)
                {
                    bOn = true;
                }
            }
            if (bOn)
            {
                rv = Visibility.Hidden;
            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #endregion The Visibility value converters.

    #region The NotNullToBooleanConverter, for enabling a control if an ItemsSource item is selected.

    /// <summary>
    /// This subclass of IValueConverter converts an object's null-state (as from the IsChecked property of a CheckBox)
    /// to a boolean, that is.. null -> false, any other value -> true.
    /// It's purpose is to bind a button's IsEnabled property to a ListBox's SelectedItem property.
    /// </summary>
    public class NotNullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool rv = true;
            if (value == null)
            {
                rv = false;
            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

}
