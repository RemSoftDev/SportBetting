using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Configuration;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;

namespace SportBetting.WPF.Prism.Shared.WpfHelper
{
    public class ScrollViewerModule
    {
        private static readonly ReaderWriterLockSlim _ReaderWriterLockSlim = new ReaderWriterLockSlim();
        private static bool? _UseSmoothScrolling = null;
        private IChangeTracker ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
        public static bool UseSmoothScrolling
        {
            get
            {
                bool? result = null;

                _ReaderWriterLockSlim.EnterReadLock();
                try
                {
                    result = _UseSmoothScrolling;
                }
                finally
                {
                    _ReaderWriterLockSlim.ExitReadLock();
                }

                if (result == null)
                {
                    _ReaderWriterLockSlim.EnterWriteLock();
                    try
                    {
                        if (_UseSmoothScrolling == null)
                        {
                            _UseSmoothScrolling = ((ConfigurationManager.AppSettings["USE_SMOOTH_SCROLLING"] ?? string.Empty).Trim().ToLowerInvariant() == "true");
                            result = _UseSmoothScrolling;
                        }
                    }
                    finally
                    {
                        _ReaderWriterLockSlim.ExitWriteLock();
                    }
                }

                return result.Value;
            }
        }

        public void OnScrollLeftStartExecute(ScrollViewer scrollViewer, bool? useSmoothScrolling = null)
        {

            Dispatcher.BeginInvoke(() =>
            {
                this.HorizontalScroll(true, scrollViewer, useSmoothScrolling);
            });

        }

        public void OnScrollRightStartExecute(ScrollViewer scrollViewer, bool? useSmoothScrolling = null)
        {

            Dispatcher.BeginInvoke(() =>
            {
                this.HorizontalScroll(false, scrollViewer, useSmoothScrolling);
            });

        }

        /// <summary>
        /// Method to invoke when the ScrollDownStart command is executed.
        /// </summary>
        public void OnScrollDownStartExecute(ScrollViewer scrollViewer, bool? useSmoothScrolling = null, bool? doHugeScroll = null)
        {

            Dispatcher.BeginInvoke(() =>
                {
                    this.Scroll(true, scrollViewer, useSmoothScrolling, doHugeScroll);
                });

        }
        /// <summary>
        /// Method to invoke when the ScrollDownStop command is executed.
        /// </summary>
        public void OnScrollDownStopExecute()
        {
            Dispatcher.BeginInvoke(() =>
                { this.StopAndResetStoryboard(); });
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStart command is executed.
        /// </summary>
        public void OnScrollUpStartExecute(ScrollViewer scrollViewer, bool? useSmoothScrolling = null)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    this.StopAndResetStoryboard();
                    this.Scroll(false, scrollViewer, useSmoothScrolling);
                });
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStop command is executed.
        /// </summary>
        public void OnScrollUpStopExecute()
        {
            Dispatcher.BeginInvoke(() =>
                { this.StopAndResetStoryboard(); });
        }

        private void StopAndResetStoryboard()
        {
            //if (this.Storyboard != null)
            //{
            //    this.Storyboard.Pause();
            //    this.Storyboard = null;
            //}
        }

        private Storyboard Storyboard = new Storyboard();
        private IDispatcher Dispatcher;
        public ScrollViewerModule(IDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }


        private void Scroll(bool down, ScrollViewer scrollViewer, bool? useSmoothScrolling = null, bool? doHugeScroll = null)
        {
            if (scrollViewer == null)
            {
                return;
            }

            double distance = 0;

            //set offset
            if (doHugeScroll != null)
            {
                distance = 100000;
            }
            else if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
            {
                distance = 350;
            }
            else
                distance = 600;

            if (!down)
                distance = distance * -1;

            bool useSmooth = useSmoothScrolling ?? UseSmoothScrolling;
            if (useSmooth)
            {
                if (this.Storyboard != null)
                {
                    this.Storyboard.Pause();
                    this.Storyboard = null;
                }

                //if (scrollViewer.CanContentScroll)
                //{
                //    scrollViewer.CanContentScroll = false;
                //}

                DoubleAnimation verticalAnimation = new DoubleAnimation();
                verticalAnimation.AccelerationRatio = 0.5;
                verticalAnimation.From = scrollViewer.VerticalOffset;
                verticalAnimation.To = scrollViewer.VerticalOffset + distance;
                double millis = 300;// ((down ? scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset : scrollViewer.VerticalOffset) * 3.0);
                if (millis > 5000.0)
                {
                    millis = 5000.0;
                }
                verticalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(millis));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(verticalAnimation);
                Storyboard.SetTarget(verticalAnimation, scrollViewer);
                Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(SportBetting.WPF.Prism.WpfHelper.ScrollViewerBehaviour.VerticalOffsetProperty));
                storyboard.Begin();

                this.Storyboard = storyboard;
            }
            else
            {
                if (!scrollViewer.CanContentScroll)
                {
                    scrollViewer.CanContentScroll = true;
                }

                scrollViewer.ScrollToVerticalOffset(scrollViewer.ContentVerticalOffset + distance);
            }
        }

        private void HorizontalScroll(bool left, ScrollViewer scrollViewer, bool? useSmoothScrolling = null)
        {
            if (scrollViewer == null)
            {
                return;
            }

            double distance = 0;

            //set offset
            if (ChangeTracker.IsLandscapeMode || ChangeTracker.Is34Mode)
            {
                distance = 350;
            }
            else
                distance = 600;

            if (left)
                distance = distance * -1;

            bool useSmooth = useSmoothScrolling ?? UseSmoothScrolling;
            if (useSmooth)
            {
                if (this.Storyboard != null)
                {
                    this.Storyboard.Pause();
                    this.Storyboard = null;
                }

                DoubleAnimation horizontalAnimation = new DoubleAnimation();
                horizontalAnimation.AccelerationRatio = 0.5;
                horizontalAnimation.From = scrollViewer.HorizontalOffset;
                horizontalAnimation.To = scrollViewer.HorizontalOffset + distance;
                double millis = 200;
                if (millis > 5000.0)
                {
                    millis = 5000.0;
                }
                horizontalAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(millis));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(horizontalAnimation);
                Storyboard.SetTarget(horizontalAnimation, scrollViewer);
                Storyboard.SetTargetProperty(horizontalAnimation, new PropertyPath(SportBetting.WPF.Prism.WpfHelper.ScrollViewerBehaviour.HorizontalOffsetProperty));
                storyboard.Begin();

                this.Storyboard = storyboard;
            }
            else
            {
                if (!scrollViewer.CanContentScroll)
                {
                    scrollViewer.CanContentScroll = true;
                }

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.ContentHorizontalOffset + distance);
            }
        }
    }
}
