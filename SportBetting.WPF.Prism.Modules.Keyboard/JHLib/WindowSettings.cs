using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.designforge.com/windows", "JHLib")]

namespace JHLib
{
    /// <summary>
    /// Persists a Window's Size, Location and WindowState to UserScopeSettings 
    /// </summary>
    public class WindowSettings
    {
        #region Constructor

        public WindowSettings(Window window, bool sizeOnly = false)
        {
            _window = window;
            _isSavingSizeOnly = sizeOnly;
            // Use the class-name of the given Window, minus the namespace prefix, as the instance-key.
            // This is so that we have a distinct setting for different windows.
            string sWindowType = window.GetType().ToString();
            int iPos = sWindowType.LastIndexOf('.');
            string sKey;
            if (iPos > 0)
            {
                sKey = sWindowType.Substring(iPos+1);
            }
            else
            {
                sKey = sWindowType;
            }
            _sInstanceSettingsKey = sKey;
        }

        #endregion

        #region WindowApplicationSettings helper class
        public class WindowApplicationSettings : ApplicationSettingsBase
        {
            public WindowApplicationSettings(WindowSettings windowSettings, string sInstanceKey)
                : base(sInstanceKey)
            {
            }

            [UserScopedSetting]
            public Rect Location
            {
                get
                {
                    try
                    {
                        if (this["Location"] != null)
                        {
                            return ((Rect)this["Location"]);
                        }
                    }
                    catch (System.Configuration.ConfigurationErrorsException x)
                    {
                        // I added this for diagnosing a failure. See http://forums.msdn.microsoft.com/en-US/vbgeneral/thread/41cfc8e2-c7f4-462d-9a43-e751500deb0a
                        Console.WriteLine("Oh geez - you got an ConfigurationErrorsException within JHLib.WindowSettings.Location.Get. Something may be wrong with your app config file!" + x.ToString());

                        System.Configuration.Configuration exeConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                        Console.WriteLine("  exe config FilePath is " + exeConfig.FilePath);

                        System.Configuration.Configuration localConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal);
                        Console.WriteLine("  local config FilePath is " + localConfig.FilePath);

                        System.Configuration.Configuration roamingConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoaming);
                        Console.WriteLine("  roaming config FilePath is " + roamingConfig.FilePath);
                    }
                    return Rect.Empty;
                }
                set
                {
                    this["Location"] = value;
                }
            }

            [UserScopedSetting]
            public WindowState WindowState
            {
                get
                {
                    try
                    {
                        if (this["WindowState"] != null)
                        {
                            return (WindowState)this["WindowState"];
                        }
                    }
                    catch (System.Configuration.ConfigurationErrorsException x)
                    {
                        // I added this for diagnosing a failure. See http://forums.msdn.microsoft.com/en-US/vbgeneral/thread/41cfc8e2-c7f4-462d-9a43-e751500deb0a
                        Console.WriteLine("Oh geez - you got an ConfigurationErrorsException within JHLib.WindowSettings.WindowState.Get. Something may be wrong with your app config file! " + x.Message);

                        System.Configuration.Configuration exeConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                        Console.WriteLine("  exe config FilePath is " + exeConfig.FilePath);

                        System.Configuration.Configuration localConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal);
                        Console.WriteLine("  local config FilePath is " + localConfig.FilePath);

                        System.Configuration.Configuration roamingConfig = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoaming);
                        Console.WriteLine("  roaming config FilePath is " + roamingConfig.FilePath);
                    }
                    return WindowState.Normal;
                }
                set
                {
                    this["WindowState"] = value;
                }
            }
        }
        #endregion

        #region Attached "Save" Property Implementation
        /// <summary>
        /// Register the "Save" attached property and the "OnSaveInvalidated" callback 
        /// </summary>
        public static readonly DependencyProperty SaveProperty
           = DependencyProperty.RegisterAttached("Save", typeof(bool), typeof(WindowSettings),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSaveInvalidated)));

        public static void SetSave(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(SaveProperty, enabled);
        }

        /// <summary>
        /// Called when Save is changed on an object.
        /// </summary>
        private static void OnSaveInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            Window window = dependencyObject as Window;
            if (window != null)
            {
                if ((bool)e.NewValue)
                {
                    WindowSettings settings = new WindowSettings(window);
                    settings.Attach();
                }
            }
        }
        #endregion

        #region Attached "SaveSize" Property Implementation
        /// <summary>
        /// Register the "SaveSize" attached property and the "OnSaveSizeInvalidated" callback 
        /// </summary>
        public static readonly DependencyProperty SaveSizeProperty
           = DependencyProperty.RegisterAttached("SaveSize", typeof(bool), typeof(WindowSettings),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSaveSizeInvalidated)));

        public static void SetSaveSize(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(SaveSizeProperty, enabled);
        }

        /// <summary>
        /// Called when SaveSize is changed on an object.
        /// </summary>
        private static void OnSaveSizeInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            Window window = dependencyObject as Window;
            if (window != null)
            {
                if ((bool)e.NewValue)
                {
                    WindowSettings settings = new WindowSettings(window, true);
                    settings.Attach();
                }
            }
        }
        #endregion

        #region LoadWindowState
        /// <summary>
        /// Load the Window Size Location and State from the settings object
        /// </summary>
        protected virtual void LoadWindowState()
        {
            this.Settings.Reload();
            // Deal with multiple monitors.
            if (this.Settings.Location != Rect.Empty)
            {
                this._window.Width = this.Settings.Location.Width;
                this._window.Height = this.Settings.Location.Height;
                //Console.WriteLine("in LoadWindowState, setting " + _sInstanceSettingsKey + " Width=" + Settings.Location.Width
                //    + ", Height=" + Settings.Location.Height + ", _isSavingSizeOnly is " + _isSavingSizeOnly);
                if (!_isSavingSizeOnly)
                {
                    this._window.Left = this.Settings.Location.Left;
                    this._window.Top = this.Settings.Location.Top;

                    // Apply a correction if the previous settings had it located on a monitor that no longer is available.
                    //
                    double VirtualScreenTop = System.Windows.SystemParameters.VirtualScreenTop;
                    double VirtualScreenWidth = System.Windows.SystemParameters.VirtualScreenWidth;
                    double VirtualScreenHeight = System.Windows.SystemParameters.VirtualScreenHeight;
                    double VirtualScreenLeft = System.Windows.SystemParameters.VirtualScreenLeft;
                    double VirtualScreenRight = VirtualScreenLeft + VirtualScreenWidth;
                    double VirtualScreenBottom = VirtualScreenTop + VirtualScreenHeight;
                    double MyWidth = _window.Width;
                    double MyBottom = _window.Top + _window.Height;

                    // If the 2nd monitor was to the right, and is now not..
                    if (_window.Left > (VirtualScreenRight - MyWidth))
                    {
                        _window.Left = VirtualScreenRight - MyWidth;
                    }
                    // or if it was to the left..
                    else if (_window.Left < VirtualScreenLeft)
                    {
                        _window.Left = VirtualScreenLeft;
                    }
                    // or if there was a vertical change..
                    if (MyBottom > VirtualScreenBottom)
                    {
                        _window.Top = VirtualScreenBottom - _window.Height;
                    }
                    else if (_window.Top < VirtualScreenTop)
                    {
                        _window.Top = VirtualScreenTop;
                    }
                }
            }
            if (this.Settings.WindowState != WindowState.Maximized)
            {
                this._window.WindowState = this.Settings.WindowState;
            }
        }
        #endregion

        #region SaveWindowState
        /// <summary>
        /// Save the Window Size, Location and State to the settings object
        /// </summary>
        protected virtual void SaveWindowState()
        {
            Settings.WindowState = _window.WindowState;
            Settings.Location = _window.RestoreBounds;
            Settings.Save();
            //Console.WriteLine("in WindowSettings.SaveWindowState, saving " + _sInstanceSettingsKey + " Width=" + Settings.Location.Width
            //    + ", Height=" + Settings.Location.Height + ", _isSavingSizeOnly is " + _isSavingSizeOnly);
        }
        #endregion

        #region Attach
        private void Attach()
        {
            if (this._window != null)
            {
                this._window.Closing += new CancelEventHandler(window_Closing);
                this._window.Initialized += new EventHandler(window_Initialized);
                this._window.Loaded += new RoutedEventHandler(window_Loaded);
            }
        }
        #endregion

        #region window_Loaded
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Settings.WindowState == WindowState.Maximized)
            {
                this._window.WindowState = this.Settings.WindowState;
            }
        }
        #endregion

        #region window_Initialized
        private void window_Initialized(object sender, EventArgs e)
        {
            LoadWindowState();
        }
        #endregion

        #region window_Closing
        private void window_Closing(object sender, CancelEventArgs e)
        {
            SaveWindowState();
        }
        #endregion

        #region Settings Property Implementation

        protected virtual WindowApplicationSettings CreateWindowApplicationSettingsInstance()
        {
            return new WindowApplicationSettings(this, _sInstanceSettingsKey);
        }

        [Browsable(false)]
        public WindowApplicationSettings Settings
        {
            get
            {
                if (_windowApplicationSettings == null)
                {
                    _windowApplicationSettings = CreateWindowApplicationSettingsInstance();
                }
                return _windowApplicationSettings;
            }
        }
        #endregion

        #region fields

        private Window _window = null;
        /// <summary>
        /// This is used to dictate whether we're saving this Window's size+position, or just the size alone.
        /// </summary>
        private bool _isSavingSizeOnly;
        private WindowApplicationSettings _windowApplicationSettings;
        private string _sInstanceSettingsKey;

        #endregion
    }
}
