using System.Windows;

namespace SportBetting.WPF.Prism.Shared.Controls
{
    /// <summary>
    /// Interaction logic for Loader.xaml
    /// </summary>
    public partial class Loader : Window
    {

        public Loader(string text = "")
        {
            InitializeComponent();
            if(!string.IsNullOrEmpty(text))
                OverLayText.Text = text;
        }
        public string PleaseWaitText
        {
            get { return OverLayText.Text; }
            set { OverLayText.Text = value; }
        }
    }
}
