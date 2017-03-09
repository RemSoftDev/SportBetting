using System;
using System.Windows;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportBetting.WPF.Prism.Shared.WpfHelper;

namespace BaseObjects
{
    public class DateHelper : ISelectDate
    {
        private static readonly Lazy<DateHelper> _Instance = new Lazy<DateHelper>(() => new DateHelper(), true);
        public static ISelectDate Instance { get { return _Instance.Value; } }
        public DateHelper()
        {
        }

        private IMyRegionManager _myRegionManager;
        public IMyRegionManager MyRegionManager
        {
            get
            {
                return _myRegionManager ?? (_myRegionManager = IoCContainer.Kernel.Get<IMyRegionManager>());
            }
        }

        public static DateTime? SelectDate(DateTime? initialDate, DateTime? minDate = null, DateTime? maxDate = null)
        {
            return DateHelper.Instance.SelectDate(initialDate, minDate, maxDate);
        }

        private IDispatcher _dispatcher;
        public IDispatcher Dispatcher
        {
            get
            {
                return _dispatcher ?? (_dispatcher = IoCContainer.Kernel.Get<IDispatcher>());
            }
        }

        private IChangeTracker _changeTracker;
        public IChangeTracker ChangeTracker
        {
            get
            {
                return _changeTracker ?? (_changeTracker = IoCContainer.Kernel.Get<IChangeTracker>());
            }
        }
        private DateTime? SelectDateInternal(DateTime? initialDate, DateTime? minDate = null, DateTime? maxDate = null)
        {
            ChangeTracker.initDate = initialDate;
            ChangeTracker.minDate = minDate;
            ChangeTracker.maxDate = maxDate;
            DateTime? result = null;

            //var viewModel = new DateTimeViewModel(initialDate, minDate, maxDate);
            //var uiVisualizerService = Catel.IoC.ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            //uiVisualizerService.ShowDialog(viewModel);

            var window = MyRegionManager.FindWindowByViewModel<DateTimeViewModel>(false);
            window.Left = 0;
            window.Top = 0;
            window.WindowState = WindowState.Normal;
            window.WindowState = WindowState.Maximized;
            window.ShowDialog();
            //result = viewModel.Date ?? initialDate;
            result = ChangeTracker.BirthDate ?? initialDate;
            ChangeTracker.BirthDate = null;
            return result;
        }

        DateTime? ISelectDate.SelectDate(DateTime? initialDate, DateTime? minDate, DateTime? maxDate)
        {
            return this.SelectDateInternal(initialDate, minDate, maxDate);
        }
    }
}
