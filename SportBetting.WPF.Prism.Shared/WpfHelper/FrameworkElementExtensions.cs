using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;

namespace SportBetting.WPF.Prism.Shared.WpfHelper
{
    public static class FrameworkElementExtensions
    {
        private static readonly Storyboard DimmStoryboard = new Storyboard();
        private const double DimmedValue = 0.75d;
        private static readonly Storyboard UndimmStoryboard = new Storyboard();
        private const double UndimmedValue = 1d;

        /// <summary>
        /// Initializes the <see cref="FrameworkElementExtensions"/> class.
        /// </summary>
        static FrameworkElementExtensions()
        {
            var dimmAnimation = new DoubleAnimation();
            dimmAnimation.To = DimmedValue;
            dimmAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            dimmAnimation.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("Opacity"));
            DimmStoryboard.Children.Add(dimmAnimation);

            var undimmAnimation = new DoubleAnimation();
            undimmAnimation.To = UndimmedValue;
            undimmAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            undimmAnimation.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath("Opacity"));
            UndimmStoryboard.Children.Add(undimmAnimation);
        }

        public static void Blur(this FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                var blur = new BlurEffect();
                blur.Radius = 5;
                frameworkElement.Effect = blur;

                Dimm(frameworkElement);
            }
        }

        public static void Unblur(this FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                frameworkElement.Effect = null;

                Undimm(frameworkElement);
            }
        }

        public static void Dimm(this FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                frameworkElement.IsHitTestVisible = false;
                DimmStoryboard.Begin(frameworkElement);
            }
        }

        public static void Undimm(this FrameworkElement frameworkElement)
        {
            if (frameworkElement != null)
            {
                frameworkElement.IsHitTestVisible = false;
                UndimmStoryboard.Begin(frameworkElement);
            }
        }
    }
}
