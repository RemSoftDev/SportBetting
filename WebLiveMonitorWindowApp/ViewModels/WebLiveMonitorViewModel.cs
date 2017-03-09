using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using DdeNet.Client;
using Microsoft.Win32;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;

namespace WebLiveMonitorWindowApp.ViewModels
{

    [ServiceAspect]
    public class WebLiveMonitorViewModel : BaseViewModel
    {



        private bool _rotating = false;
        public bool rotating
        {
            get
            {
                return _rotating;
            }

            set
            {
                _rotating = value;
                OnPropertyChanged("rotating");
            }
        }

        public int rotationInterval = 10;

        public int liveOddsPage = 1000;
        #region Constructors

        private readonly int _screen = 0;
        public WebLiveMonitorViewModel(params object[] args)
        {
            HidePleaseWait = false;
            //LineSr.DataSqlUpdateSucceeded += DataCopy_DataSqlUpdateSucceeded;

            if (args.Length > 0)
                int.TryParse(args[0].ToString(), out _screen);

            Mediator.Register<string>(this, SetUpBrowser, MsgTag.SetUpBrowser);
            Mediator.Register<string>(this, ChangeVisibility, MsgTag.ChangeVisibility);

            _client = new DdeClient("terminal", MsgTag.SetUpBrowser);
            while (!_client.IsConnected)
            {
                try
                {
                    _client.Connect();
                    _client.Disconnected += client_Disconnected;
                    _client.StartAdvise("test" + _screen, 1, true, 6000);
                    _client.Advise += client_Advise;

                }
                catch (Exception e)
                {
                    Log.Error(e.Message, e);
                }
            }

        }

        private void ChangeVisibility(string obj)
        {
            Dispatcher.Invoke(() =>
                {
                    if (obj == Visibility.Visible.ToString())
                    {
                        this.ViewWindow.Visibility = Visibility.Visible;
                    }
                    if (obj == Visibility.Hidden.ToString())
                    {
                        this.ViewWindow.Visibility = Visibility.Hidden;
                        WebAddress = "about:blank";
                    }
                    if (obj == Visibility.Collapsed.ToString())
                    {
                        this.ViewWindow.Visibility = Visibility.Collapsed;
                        WebAddress = "about:blank";
                    }
                });

        }

        private IList<string> lastMessage = new List<string>();

        void client_Advise(object sender, DdeAdviseEventArgs e)
        {
            try
            {
                Log.Debug(e.Text);
                if (e.Text == "*")
                    return;
                lastMessage.Add(e.Text);
                var screen = e.Text.Split('|')[0];
                var value = e.Text.Split('|')[1];
                var tag = e.Text.Split('|')[2];
                if (screen == _screen.ToString())
                {
                    Mediator.SendMessage(value, tag);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message, exception);
            }
        }

        void client_Disconnected(object sender, DdeDisconnectedEventArgs e)
        {
            Environment.Exit(0);

        }



        #endregion

        #region Properties






        private Visibility _webBrowserVisibility = Visibility.Collapsed;
        public Visibility WebBrowserVisibility
        {
            get { return _webBrowserVisibility; }
            set
            {
                _webBrowserVisibility = value;

                OnPropertyChanged();
            }
        }

        private string _webAddress = "about:blank";
        public string WebAddress
        {
            get
            {
                return _webAddress;
            }
            set
            {
                _webAddress = value;
                OnPropertyChanged("WebAddress");
            }
        }

        #endregion

        #region Commands
        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            SetBrowserFeatureControl();
            IsClosed = false;
            if (HidePleaseWait)
            {
                WaitOverlayProvider.DisposeAll();
                Log.DebugFormat("hide Please wait:{0}", this.ToString());

            }
            Log.DebugFormat("navigated:{0}", this.ToString());

            //Mediator.ApplyRegistration();
            IsReady = true;
            if (View != null)
                View.Loaded -= View_Loaded;
            if (ViewWindow != null)
            {
                _viewWindow.Loaded -= View_Loaded;

            }



            if (this.ViewWindow != null)
                this.ViewWindow.Activated += ViewWindow_FocusableChanged;

            //Dispatcher.Invoke(() =>
            //{
            //    this.ViewWindow.Visibility = Visibility.Collapsed;
            //});
        }

        private void SetBrowserFeatureControl()
        {
            Log.Debug("SetBrowserFeatureControl");

            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode());
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_NINPUT_LEGACYMODE", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }

        private UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            UInt32 mode = 10001; // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode. Default value for Internet Explorer 10.
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                    break;
                default:
                    // use IE10 mode by default
                    break;
            }

            return mode;
        }


        private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }

        private DdeClient _client;

        private void ViewWindow_FocusableChanged(object sender, EventArgs eventArgs)
        {
            _client.Execute(MsgTag.GetFocus, 60000);
            // Mediator.SendMessage("", MsgTag.GetFocus);
            //Mediator.SendMessage("Collapsed", "ChangeVisibility");
        }

        private void SetUpBrowser(string obj)
        {
            var url = obj.Replace("1024", "1920");
            if (!string.Equals(url, WebAddress))
                WebAddress = url;
        }


        #endregion
    }
}