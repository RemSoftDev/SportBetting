using System.Reflection;
using System.Runtime.InteropServices;

namespace DefaultViews.Views
{
    /// <summary>
    /// Interaction logic for FooterView.xaml
    /// </summary>
    public partial class FooterView 
    {
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool DeleteUrlCacheEntry(string lpszUrlName);

        public FooterView()
        {
            InitializeComponent();
        }

        private void WebBrowser_Loaded_1(object sender, System.Windows.RoutedEventArgs e)
        {
            browser.Navigated += (a, b) => { HideScriptErrors(browser, true); };
        }

        public void HideScriptErrors(System.Windows.Controls.WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(System.Windows.Controls.WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;

            var objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null)
            {
                wb.Loaded += (o, s) => HideScriptErrors(wb, hide);
                return;
            }
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }

        private void browser_Navigating_1(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            bool res = DeleteUrlCacheEntry("http://192.168.0.238/screen/jwplayer.flash.swf");
            int aaa = Marshal.GetLastWin32Error();
        }
    }
}
