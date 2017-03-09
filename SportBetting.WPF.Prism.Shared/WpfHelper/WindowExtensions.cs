using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using SportBetting.WPF.Prism.Shared.Controls;

namespace SportBetting.WPF.Prism.Shared.WpfHelper
{
    public static class WindowExtensions
    {


        private enum OverlayType
        {
            Loading,
            Locked
        }

        public static LoadingAdorner ShowLoadingOverlay(this Window activeWindow)
        {
            return ChangeOverlayInternal(activeWindow, true, OverlayType.Loading);
        }
        public static void HideLoadingOverlay(this Window activeWindow)
        {
            ChangeOverlayInternal(activeWindow, false, OverlayType.Loading);
        }




        private static LoadingAdorner ChangeOverlayInternal(Window window, bool newShowFlag, OverlayType overlayType)
        {
            if (newShowFlag)
            {
                if (overlayType == OverlayType.Loading)
                {

                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer((Visual)window.Content);
                    if (adornerLayer != null)
                    {
                        var loadingAdorner = new LoadingAdorner(adornerLayer);
                        adornerLayer.Add(loadingAdorner);
                        return loadingAdorner;
                    }
                }
            }
            return null;

        }

        public class LoadingAdorner : Adorner
        {
            VisualCollection children;
            FrameworkElement child;

            // Be sure to call the base class constructor.
            public LoadingAdorner(UIElement adornedElement)
                : base(adornedElement)
            {
                this.children = new VisualCollection(this);
                var contentControl = new ContentControl();
                contentControl.Content = new Loader();
                this.child = contentControl;
                this.children.Add(this.child);
                this.AddLogicalChild(this.child);
            }


            protected override Visual GetVisualChild(int index)
            {
                return this.children[index];
            }

            protected override int VisualChildrenCount
            {
                get
                {
                    return this.children.Count;
                }
            }

            protected override Size MeasureOverride(Size constraint)
            {
                this.child.Measure(constraint);
                return AdornedElement.RenderSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                Point location = new Point(0, 0);
                Rect rect = new Rect(location, finalSize);
                this.child.Arrange(rect);
                return this.child.RenderSize;
            }
        }

    }
}