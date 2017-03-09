// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EffectsHelper.cs" company="Catel development team">
//   Copyright (c) 2011 - 2012 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace BaseObjects
{


    /// <summary>
    /// Helper class for special window or framework effects.
    /// </summary>
    public static class EffectsHelper
    {
        private static readonly Storyboard _dimmStoryboard = new Storyboard();
        private const double DimmedValue = 0.75d;
        private static readonly Storyboard _undimmStoryboard = new Storyboard();
        private const double UndimmedValue = 1d;

        /// <summary>
        /// Initializes static members of the <see cref="EffectsHelper"/> class.
        /// </summary>
        static EffectsHelper()
        {
#if NETFX_CORE
            var propertyPath = "Opacity";
#else
            var propertyPath = new PropertyPath("Opacity");
#endif

            var dimmAnimation = new DoubleAnimation();
            dimmAnimation.To = DimmedValue;
            dimmAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            dimmAnimation.SetValue(Storyboard.TargetPropertyProperty, propertyPath);
            _dimmStoryboard.Children.Add(dimmAnimation);

            var undimmAnimation = new DoubleAnimation();
            undimmAnimation.To = UndimmedValue;
            undimmAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            undimmAnimation.SetValue(Storyboard.TargetPropertyProperty, propertyPath);
            _undimmStoryboard.Children.Add(undimmAnimation);
        }

        /// <summary>
        /// Dimms the specified framework element and sets the <see cref="UIElement.IsHitTestVisible"/> to <c>false</c>.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <param name="completedDelegate">The completed delegate. If <c>null</c>, the callback will not be called.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="frameworkElement"/> is <c>null</c>.</exception>
        public static void Dimm(this FrameworkElement frameworkElement, Action completedDelegate = null)
        {

            frameworkElement.IsHitTestVisible = false;

            RunStoryboardWithCallback(frameworkElement, _dimmStoryboard, completedDelegate);
        }

        /// <summary>
        /// Undimms the specified framework element and sets the <see cref="UIElement.IsHitTestVisible"/> to <c>true</c>.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <param name="completedDelegate">The completed delegate. If <c>null</c>, the callback will not be called.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="frameworkElement"/> is <c>null</c>.</exception>
        public static void Undimm(this FrameworkElement frameworkElement, Action completedDelegate = null)
        {

            frameworkElement.IsHitTestVisible = true;

            RunStoryboardWithCallback(frameworkElement, _undimmStoryboard, completedDelegate);
        }

        public static void Blur(this FrameworkElement frameworkElement, Action completedDelegate = null)
        {

            var blur = new BlurEffect();
            blur.Radius = 5;

            frameworkElement.Effect = blur;

            Dimm(frameworkElement, completedDelegate);
        }

        /// <summary>
        /// Unblurs the specified framework element.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <param name="completedDelegate">The completed delegate. If <c>null</c>, the callback will not be called.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="frameworkElement"/> is <c>null</c>.</exception>
        public static void Unblur(this FrameworkElement frameworkElement, Action completedDelegate = null)
        {

            frameworkElement.Effect = null;

            Undimm(frameworkElement, completedDelegate);
        }

        /// <summary>
        /// Runs the storyboard with the completed callback.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <param name="storyboardToRun">The storyboard to run.</param>
        /// <param name="completedDelegate">The completed delegate. If <c>null</c>, the callback will not be called.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="frameworkElement"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="storyboardToRun"/> is <c>null</c>.</exception>
        private static void RunStoryboardWithCallback(FrameworkElement frameworkElement, Storyboard storyboardToRun, Action completedDelegate)
        {

            if (completedDelegate != null)
            {
                EventHandler del = null;
                del = (sender, e) =>
                {
                    completedDelegate();
                    storyboardToRun.Completed -= del;
                };

                storyboardToRun.Completed += del;
            }

            Storyboard.SetTarget(storyboardToRun, frameworkElement);
        }
    }
}