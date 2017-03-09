using System.Windows.Threading;
using System;
using System.Threading;

namespace MainWpfWindow.Views
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml.
    /// </summary>
    public partial class StartWindow
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        //public string Message;
        public void SetMessage(string sValue)
        {
            txtMessage.Text = sValue;
            bMessages.UpdateLayout();
            Thread.Sleep(500);
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        //public string Message;
        public void SetMessageControlledSleep(string sValue, int sleep)
        {
            txtMessage.Text = sValue;
            bMessages.UpdateLayout();
            Thread.Sleep(sleep);
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
    }
}
