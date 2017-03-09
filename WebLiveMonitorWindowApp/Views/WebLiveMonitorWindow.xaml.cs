using System.Reflection;

namespace WebLiveMonitorWindowApp.Views
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class WebLiveMonitorWindow 
    {
        public WebLiveMonitorWindow()
        {
            InitializeComponent();
        }



        private void browser_Loaded_2(object sender, System.Windows.RoutedEventArgs e)
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
    }
}
