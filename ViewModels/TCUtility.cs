using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace ViewModels
{
    public static class TCUtility
    {
        public static readonly DependencyProperty BindableBlocksProperty =
            DependencyProperty.RegisterAttached("BindableBlocks", typeof(FlowDocument), typeof(TCUtility), new UIPropertyMetadata(null, BindableBlocksPropertyChanged));

        public static string GetBindableBlocks(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableBlocksProperty);
        }

        public static void SetBindableBlocks(DependencyObject obj, string value)
        {
            obj.SetValue(BindableBlocksProperty, value);
        }

        public static void BindableBlocksPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            object _lockerTimer = new object();
            lock (_lockerTimer)
            {
                FlowDocument fD = o as FlowDocument;
                if (fD != null)
                {
                    fD.Blocks.Clear();
                    FlowDocument flowDocument = e.NewValue as FlowDocument;
                    if (flowDocument != null)
                        foreach (var b in flowDocument.Blocks.ToArray())
                        {
                            fD.Blocks.Add(b);
                        }
                }
            }
        }

    }
}
