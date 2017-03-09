using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SportBetting.WPF.Prism.WpfHelper
{
    public class ScrollViewerBehaviour
    {
        public static DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached(
            "VerticalOffset",
            typeof(double),
            typeof(ScrollViewerBehaviour),
            new UIPropertyMetadata(0.0, VerticalOffsetChangedCallback));

        public static DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached(
            "HorizontalOffset",
            typeof(double),
            typeof(ScrollViewerBehaviour),
            new UIPropertyMetadata(0.0, HorizontalOffsetChangedCallback));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }
        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }

        public static void SetHorizontalOffset(FrameworkElement target, double value)
        {
            target.SetValue(HorizontalOffsetProperty, value);
        }
        public static double GetHorizontalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(HorizontalOffsetProperty);
        }

        private static void VerticalOffsetChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        private static void HorizontalOffsetChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
            }
        }
    }
}
