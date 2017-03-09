using System.Reflection;

namespace DefaultViews.Views
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class LiveMonitorWindow 
    {
        public LiveMonitorWindow()
        {
            InitializeComponent();
        }

        private void browser_Loaded_1(object sender, System.Windows.RoutedEventArgs e)
        {
            var dataContext = this.DataContext;

            bannerWebBrowser.Navigated += (a, b) => { HideScriptErrors(bannerWebBrowser, true); };
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
