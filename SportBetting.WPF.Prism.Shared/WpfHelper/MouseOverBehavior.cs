using System;
using System.Windows;

namespace SportBetting.WPF.Prism.WpfHelper
{
	public static class MouseOverBehavior
	{
		public static readonly DependencyProperty IsFMouseOverProperty =
			DependencyProperty.RegisterAttached("IsMouseOver", typeof(bool?), typeof(MouseOverBehavior), new FrameworkPropertyMetadata(IsFocusedChanged));

		public static bool? GetIsMouseOver(DependencyObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			return (bool?)element.GetValue(IsFMouseOverProperty);
		}

		public static void SetIsMouseOver(DependencyObject element, bool? value)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			element.SetValue(IsFMouseOverProperty, value);
		}

		private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var fe = (FrameworkElement)d;

			if (e.OldValue == null)
			{
				fe.MouseEnter += FrameworkElement_GotFocus;
				fe.MouseLeave += FrameworkElement_LostFocus;
			}

			if (!fe.IsVisible)
			{
				fe.IsVisibleChanged += new DependencyPropertyChangedEventHandler(fe_IsVisibleChanged);
			}

			if (e.NewValue != null && (bool)e.NewValue)
			{
				fe.Focus();
			}
		}

		private static void fe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var fe = (FrameworkElement)sender;
			if (fe.IsVisible)
			{
				fe.IsVisibleChanged -= fe_IsVisibleChanged;
				fe.Focus();
			}
		}

		private static void FrameworkElement_GotFocus(object sender, RoutedEventArgs e)
		{
			((FrameworkElement)sender).SetValue(IsFMouseOverProperty, true);
		}

		private static void FrameworkElement_LostFocus(object sender, RoutedEventArgs e)
		{
			((FrameworkElement)sender).SetValue(IsFMouseOverProperty, false);
		}
	}
}