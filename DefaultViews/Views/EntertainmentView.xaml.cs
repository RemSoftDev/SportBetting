using System.Windows;
using System.Reflection;
using ViewModels.ViewModels;

namespace DefaultViews.Views
{
    public partial class EntertainmentView
    {
        

        public EntertainmentView()
        {
            InitializeComponent();
        }

        private void browser_Loaded_1(object sender, System.Windows.RoutedEventArgs e)
        {
            var dataContext = this.DataContext;

            browser.Navigated += (a, b) => { HideScriptErrors(browser, true); };

            //browser.Navigate(((EntertainmentViewModel)dataContext).WebAddress);
            browser.LoadCompleted += browser_LoadCompleted;
        }

        void browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var dataContext = this.DataContext;
            if (dataContext != null)
                ((EntertainmentViewModel)dataContext).WebBrowserVisibility = Visibility.Visible;
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
