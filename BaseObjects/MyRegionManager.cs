using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using BaseObjects.ViewModels;
using IocContainer;
using Ninject;
using SharedInterfaces;
using SportBetting.WPF.Prism.Shared.Services;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;
using SportRadar.Common.Logs;
using WsdlRepository;
using SportBetting.WPF.Prism.Shared;

namespace BaseObjects
{
    public class MyRegionManager : IMyRegionManager
    {

        private static Dictionary<string, MyContentRegion> Regions = new Dictionary<string, MyContentRegion>();

        public static DependencyProperty RegionNameProperty = DependencyProperty.RegisterAttached("RegionName", typeof(string), typeof(MyRegionManager), new FrameworkPropertyMetadata(IsNameChanged));
        private static ILog Log = LogFactory.CreateLog(typeof(MyRegionManager));


        private static Dictionary<string, IList<ViewToViewModel>> RegionHistory = new Dictionary<string, IList<ViewToViewModel>>();
        private static Dictionary<string, ViewToViewModel> RegionToViewModel = new Dictionary<string, ViewToViewModel>();

        private IChangeTracker ChangeTracker = IoCContainer.Kernel.Get<IChangeTracker>();
        private IStationRepository StationRepository = IoCContainer.Kernel.Get<IStationRepository>();
        public static string DefaultLayout = "DefaultViews";
        public string ViewsNamespace
        {
            get
            {
                if (!string.IsNullOrEmpty(StationRepository.LayoutName))
                    return StationRepository.LayoutName;
                return DefaultLayout;
            }
        }

        private IDispatcher _dispatcher;
        public IDispatcher Dispatcher
        {
            get
            {
                return _dispatcher ?? (_dispatcher = IoCContainer.Kernel.Get<IDispatcher>());
            }
        }

        public static string GetRegionName(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (string)element.GetValue(RegionNameProperty);
        }

        public static void SetRegionName(DependencyObject element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            var region = (MyContentRegion)element;
            if (!region.IsVirtualRegion)
                Regions[value] = (MyContentRegion)element;
            else
            {
                Regions[value].ChildRegions.Add(value, region);
            }
            element.SetValue(RegionNameProperty, value);
        }

        private static void IsNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var region = (MyContentRegion)d;

            if (!region.IsVirtualRegion)
            {
                Regions[(string)e.NewValue] = region;
                RegionHistory[(string)e.NewValue] = new List<ViewToViewModel>();
            }
            else
            {
                Regions[(string)e.NewValue].ChildRegions.Add(region.VirtualRegionName, region);
            }
        }

        public T NavigateUsingViewModel<T>(string regionName, params object[] args)
        {
            if (ConfigurationManager.AppSettings["disable_views"] != null)
            {
                if (!Regions.ContainsKey(regionName))
                {
                    Regions[regionName] = new MyContentRegion();
                    RegionHistory[regionName] = new List<ViewToViewModel>();
                }
            }
            T returnValue = default(T);
            if (Regions.ContainsKey(regionName))
            {

                Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            var region = Regions[regionName];

                            ClearForwardHistory(regionName);
                            var modelName = typeof(T).Name;



                            if (region.ChildRegions.ContainsKey(modelName))
                            {
                                foreach (var childRegion in region.ChildRegions.Values)
                                {
                                    childRegion.Close();
                                }

                                region.Visibility = Visibility.Collapsed;
                                var oldControl = (UserControl)region.Content;

                                if (oldControl != null)
                                {
                                    var baseViewModel = oldControl.DataContext as IBaseViewModel;
                                    if (baseViewModel != null)
                                    {
                                        oldControl.DataContext = null;
                                        baseViewModel.Close();
                                    }
                                }
                                foreach (var childRegion in region.ChildRegions.Values)
                                {
                                    if (childRegion.VirtualRegionName == modelName)
                                    {
                                        childRegion.Visibility = Visibility.Visible;
                                        if (childRegion.Content != null && ((UserControl)childRegion.Content).DataContext.GetType().Name == modelName)
                                        {
                                            var baseViewModel = ((UserControl)childRegion.Content).DataContext as IBaseViewModel;
                                            if (baseViewModel != null)
                                            {
                                                baseViewModel.OnNavigationCompleted();
                                                returnValue = (T)baseViewModel;
                                                var vievToViewModel = new ViewToViewModel((UserControl)childRegion.Content, baseViewModel, childRegion, args);
                                                RegionHistory[regionName].Add(vievToViewModel);
                                                RegionToViewModel[regionName] = vievToViewModel;

                                            }
                                        }
                                        else
                                        {
                                            var control = FindViewByViewModel<T>();
                                            var viewModel = Activator.CreateInstance(typeof(T), args);
                                            childRegion.Content = control;
                                            control.DataContext = viewModel;
                                            returnValue = (T)viewModel;
                                            if (viewModel is IBaseViewModel)
                                                ((IBaseViewModel)viewModel).View = control;
                                            var vievToViewModel = new ViewToViewModel(control, (IBaseViewModel)viewModel, childRegion, args);
                                            RegionHistory[regionName].Add(vievToViewModel);
                                            RegionToViewModel[regionName] = vievToViewModel;
                                        }

                                    }
                                    else
                                    {
                                        childRegion.Close();
                                    }
                                }
                            }
                            else
                            {
                                foreach (var childRegion in region.ChildRegions.Values)
                                {
                                    childRegion.Close();
                                }
                                var oldControl = (UserControl)region.Content;

                                if (oldControl != null)
                                {
                                    var baseViewModel = oldControl.DataContext as IBaseViewModel;
                                    if (baseViewModel != null)
                                    {
                                        oldControl.DataContext = null;
                                        baseViewModel.Close();
                                    }
                                }
                                region.Visibility = Visibility.Visible;

                                var control = FindViewByViewModel<T>();
                                var viewModel = Activator.CreateInstance(typeof(T), args);
                                region.Content = control;
                                control.DataContext = viewModel;
                                returnValue = (T)viewModel;
                                if (viewModel is IBaseViewModel)
                                    ((IBaseViewModel)viewModel).View = control;
                                var vievToViewModel = new ViewToViewModel(control, (IBaseViewModel)viewModel, region, args);
                                RegionHistory[regionName].Add(vievToViewModel);
                                RegionToViewModel[regionName] = vievToViewModel;

                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message, ex);
                        }
                    });


            }
            else
            {
                throw new Exception("Invalid region name.");
            }

            return returnValue;
        }




        public void CloseAllViewsInRegion(string regionName)
        {
            if (Regions.ContainsKey(regionName))
            {

                Dispatcher.Invoke((Action)(() =>
                {
                    for (int i = 0; i < RegionHistory[regionName].Count; i++)
                    {
                        ViewToViewModel vtVm = RegionHistory[regionName].ElementAt(i) as ViewToViewModel;
                        var control = vtVm.View;
                        var vievModel = control.DataContext as IBaseViewModel;
                        if (vievModel != null)
                        {
                            control.DataContext = null;
                            vievModel.Close();
                        }
                    }
                }));
            }
        }


        private UserControl FindViewByViewModel<T>()
        {
            if (ConfigurationManager.AppSettings["disable_views"] != null)
            {
                return new EmptyControl();
            }
            var type = typeof(T);
            string typeName = type.ToString();
            string[] strArray = typeName.Split('.');
            var defaultViewName = strArray.Last().Replace("ViewModel", "View");

            var shortViewName = defaultViewName;

            var defaultName = typeName.Replace("ViewModels", "Views").Replace(strArray.Last(), defaultViewName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), ViewsNamespace + ".dll");
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(path);

            }
            catch (Exception)
            {

            }


            Type defaulttype = null;
            if (ChangeTracker.Is34Mode && assembly != null)
                defaulttype = assembly.GetType(ViewsNamespace + ".Views.Mode3x4." + shortViewName);
            if (ChangeTracker.IsLandscapeMode && assembly != null)
                defaulttype = assembly.GetType(ViewsNamespace + ".Views.Landscape." + shortViewName);
            if (defaulttype == null && assembly != null)
                defaulttype = assembly.GetType(ViewsNamespace + ".Views." + shortViewName);
            if (defaulttype == null)
            {
                var defaultpath = Path.Combine(Directory.GetCurrentDirectory(), DefaultLayout + ".dll");
                Log.Debug(defaultpath);
                var defaultassembly = Assembly.LoadFrom(defaultpath);

                if (ChangeTracker.Is34Mode)
                    defaulttype = defaultassembly.GetType(DefaultLayout + ".Views.Mode3x4." + shortViewName);
                else if (ChangeTracker.IsLandscapeMode)
                    defaulttype = defaultassembly.GetType(DefaultLayout + ".Views.Landscape." + shortViewName);
                if (defaulttype == null)
                    defaulttype = defaultassembly.GetType(DefaultLayout + ".Views." + shortViewName);
            }
            if (defaulttype == null)
                defaulttype = type.Assembly.GetType(defaultName, true);
            var control = Activator.CreateInstance(defaulttype, true);
            return (UserControl)control;



        }


        public Window FindWindowByViewModel<T>(bool init = true)
        {

            var type = typeof(T);
            string typeName = type.ToString();
            string[] strArray = typeName.Split('.');
            var defaultViewName = strArray.Last().Replace("ViewModel", "Window");

            var shortViewName = defaultViewName;

            var shortViewName2 = defaultViewName;

            if (ChangeTracker.Is34Mode)
                shortViewName2 = "Mode3x4." + defaultViewName;
            else if (ChangeTracker.IsLandscapeMode)
                shortViewName2 = "Landscape." + defaultViewName;

            var viewName = typeName.Replace("ViewModels", "Views").Replace(strArray.Last(), shortViewName2);
            var defaultName = typeName.Replace("ViewModels", "Views").Replace(strArray.Last(), defaultViewName);

            Window window = null;

            if (ConfigurationManager.AppSettings["disable_views"] != null)
            {
                window = new EmptyWindow();
            }
            else
            {


                var newtype = type.Assembly.GetType(viewName);
                if (newtype != null)
                {
                    var control = Activator.CreateInstance(newtype);
                    window = (Window)control;
                }
                else
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), ViewsNamespace + ".dll");
                    var assembly = Assembly.LoadFrom(path);
                    Type defaulttype = null;
                    if (ChangeTracker.Is34Mode)
                        defaulttype = assembly.GetType(ViewsNamespace + ".Views.Mode3x4." + shortViewName);
                    if (ChangeTracker.IsLandscapeMode)
                        defaulttype = assembly.GetType(ViewsNamespace + ".Views.Landscape." + shortViewName);
                    if (defaulttype == null)
                        defaulttype = assembly.GetType(ViewsNamespace + ".Views." + shortViewName);
                    if (defaulttype == null)
                    {
                        var defaultpath = Path.Combine(Directory.GetCurrentDirectory(), DefaultLayout + ".dll");
                        Log.Debug(defaultpath);
                        var defaultassembly = Assembly.LoadFrom(defaultpath);

                        if (ChangeTracker.Is34Mode)
                            defaulttype = defaultassembly.GetType(DefaultLayout + ".Views.Mode3x4." + shortViewName);
                        else if (ChangeTracker.IsLandscapeMode)
                            defaulttype = defaultassembly.GetType(DefaultLayout + ".Views.Landscape." + shortViewName);
                        if (defaulttype == null)
                            defaulttype = defaultassembly.GetType(DefaultLayout + ".Views." + shortViewName);
                    }
                    if (defaulttype == null)
                        defaulttype = type.Assembly.GetType(defaultName, true);
                    var control = Activator.CreateInstance(defaulttype);
                    window = (Window)control;

                }
            }

            var vievModel = Activator.CreateInstance(typeof(T));
            window.DataContext = vievModel;
            if (vievModel is IBaseViewModel)
                ((IBaseViewModel)vievModel).ViewWindow = window;

            if (init && ConfigurationManager.AppSettings["disable_views"] == null)
            {
                window.Visibility = Visibility.Collapsed;
                window.Show();
                window.Hide();
                window.Visibility = Visibility.Visible;
            }
            if (Debugger.IsAttached)
                window.Topmost = false;
            return window;

        }


        public void NavigateForvard(string regionName)
        {
            if (Regions.ContainsKey(regionName))
            {
                Dispatcher.Invoke((Action)(() =>
                    {
                        var region = Regions[regionName];
                        int currentIndex = 0;
                        for (int i = RegionHistory[regionName].Count - 1; i >= 0; i--)
                        {
                            var viewToVieModel = RegionHistory[regionName][i];
                            if (Equals(viewToVieModel.View, Regions[regionName].ActiveRegion.Content))
                            {
                                currentIndex = i;
                            }
                        }
                        if (currentIndex < RegionHistory[regionName].Count - 1)
                        {
                            var oldControl = (UserControl)region.Content;
                            if (oldControl != null)
                            {
                                var baseViewModel = oldControl.DataContext as IBaseViewModel;
                                if (baseViewModel != null)
                                {
                                    oldControl.DataContext = null;
                                    baseViewModel.Close();
                                }
                            }

                            if (!RegionHistory[regionName][currentIndex + 1].Region.IsVirtualRegion)
                            {
                                foreach (var childRegion in region.ChildRegions.Values)
                                {
                                    childRegion.Close();
                                }
                                region.Visibility = Visibility.Visible;


                                var view = RegionHistory[regionName][currentIndex + 1].View;
                                view.DataContext = null;
                                var model = Activator.CreateInstance(RegionHistory[regionName][currentIndex + 1].ViewModelType, RegionHistory[regionName][currentIndex + 1].Args);
                                view.DataContext = model;
                                if ((model is IBaseViewModel))
                                    ((IBaseViewModel)model).View = view;

                                RegionHistory[regionName][currentIndex + 1].Region.Content = view;
                                var vievToViewModel = new ViewToViewModel(view, (IBaseViewModel)model, null);
                                RegionToViewModel[regionName] = vievToViewModel;
                            }
                            else
                            {
                                region.Visibility = Visibility.Collapsed;

                                RegionHistory[regionName][currentIndex + 1].Region.Visibility = Visibility.Visible;
                                var view = RegionHistory[regionName][currentIndex + 1].View;
                                if (RegionHistory[regionName][currentIndex + 1].Region.Content != null)
                                {
                                    var baseViewModel = ((UserControl)RegionHistory[regionName][currentIndex + 1].Region.Content).DataContext as IBaseViewModel;
                                    if (baseViewModel != null)
                                    {
                                        baseViewModel.OnNavigationCompleted();
                                    }
                                    var vievToViewModel = new ViewToViewModel(view, baseViewModel, null);
                                    RegionToViewModel[regionName] = vievToViewModel;

                                }


                            }

                        }
                    }));
            }
            else
            {
                throw new Exception("Invalid region name.");
            }
        }

        private IMessageStorage _mediator;
        public IMessageStorage Mediator
        {
            get
            {
                return _mediator ?? (_mediator = IoCContainer.Kernel.Get<IMessageStorage>());
            }
        }

        public object NavigatBack(string regionName)
        {
            object returnModel = null;
            if (Regions.ContainsKey(regionName))
            {
                Dispatcher.Invoke((Action)(() =>
                    {
                        var region = Regions[regionName];
                        int currentIndex = GetCurrentIndex(regionName);
                        if (currentIndex > 0)
                        {
                            var oldControl = (UserControl)region.Content;
                            if (oldControl != null)
                            {
                                var baseViewModel = oldControl.DataContext as IBaseViewModel;
                                if (baseViewModel != null)
                                {
                                    oldControl.DataContext = null;
                                    baseViewModel.Close();
                                }
                            }

                            try
                            {
                            if (!RegionHistory[regionName][currentIndex - 1].Region.IsVirtualRegion)
                            {
                                foreach (var childRegion in region.ChildRegions.Values)
                                {
                                    childRegion.Close();
                                }
                                region.Visibility = Visibility.Visible;

                                var view = RegionHistory[regionName][currentIndex - 1].View;
                                view.DataContext = null;
                                var model = Activator.CreateInstance(RegionHistory[regionName][currentIndex - 1].ViewModelType, RegionHistory[regionName][currentIndex - 1].Args);
                                view.DataContext = model;
                                returnModel = model;
                                if ((model is IBaseViewModel))
                                    ((IBaseViewModel)model).View = view;

                                RegionHistory[regionName][currentIndex - 1].Region.ActiveRegion.Content = view;
                                var vievToViewModel = new ViewToViewModel(view, (IBaseViewModel)model, null);
                                RegionToViewModel[regionName] = vievToViewModel;
                            }
                            else
                            {
                                region.Visibility = Visibility.Collapsed;

                                RegionHistory[regionName][currentIndex - 1].Region.Visibility = Visibility.Visible;
                                var view = RegionHistory[regionName][currentIndex - 1].View;
                                if (RegionHistory[regionName][currentIndex - 1].Region.Content != null)
                                {
                                    var baseViewModel = ((UserControl)RegionHistory[regionName][currentIndex - 1].Region.Content).DataContext as IBaseViewModel;
                                    if (baseViewModel != null)
                                    {
                                        baseViewModel.OnNavigationCompleted();
                                    }
                                    returnModel = baseViewModel;
                                    var vievToViewModel = new ViewToViewModel(view, baseViewModel, null);
                                    RegionToViewModel[regionName] = vievToViewModel;

                                }


                            }
                        }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message, ex);
                                Mediator.SendMessage(MsgTag.ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);
                            }
                        }
                    }));
            }
            else
            {
                throw new Exception("Invalid region name.");
            }
            return returnModel;
        }

        private static int GetCurrentIndex(string regionName)
        {
            int currentIndex = 0;
            for (int i = RegionHistory[regionName].Count - 1; i >= 0; i--)
            {
                var viewToVieModel = RegionHistory[regionName][i];
                if (Equals(viewToVieModel.View, Regions[regionName].ActiveRegion.Content))
                {
                    currentIndex = i;
                }
            }
            return currentIndex;
        }

        public void ClearForwardHistory(string regionName)
        {
            if (Regions.ContainsKey(regionName))
            {
                Dispatcher.Invoke(() =>
                    {
                        int currentIndex = 0;
                        for (int i = RegionHistory[regionName].Count - 1; i >= 0; i--)
                        {
                            var viewToVieModel = RegionHistory[regionName][i];
                            if (Equals(viewToVieModel.View, Regions[regionName].ActiveRegion.Content))
                            {
                                currentIndex = i;
                            }
                        }
                        while (currentIndex < RegionHistory[regionName].Count - 1)
                        {
                            RegionHistory[regionName].RemoveAt(RegionHistory[regionName].Count - 1);
                        }
                    });
            }
            else
            {
                throw new Exception("Invalid region name.");
            }
        }

        public IBaseViewModel CurrentViewModelInRegion(string regionName)
        {

            if (RegionToViewModel.ContainsKey(regionName))
                return RegionToViewModel[regionName].ViewModel;

            return null;
        }

        public void ClearHistory(string regionName)
        {
            if (ConfigurationManager.AppSettings["disable_views"] != null)
            {
                if (!Regions.ContainsKey(regionName))
                {
                    Regions[regionName] = new MyContentRegion();
                    RegionHistory[regionName] = new List<ViewToViewModel>();
                }
            }
            if (Regions.ContainsKey(regionName))
            {
                RegionHistory[regionName].Clear();
            }
            else
            {
                throw new Exception("Invalid region name.");
            }
        }

        public Type CurrentViewModelType(string regionName)
        {
            if (RegionToViewModel.ContainsKey(regionName))
                return RegionToViewModel[regionName].ViewModelType;
            return null;
        }

        public Type PreviousViewModelType(string regionName)
        {
            try
            {
                int currentIndex = GetCurrentIndex(regionName);
                if (currentIndex > 0) return RegionHistory[regionName][currentIndex - 1].ViewModelType;
            }
            catch (Exception)
            { }
            return null;
        }

    }

    public class EmptyWindow : Window
    {
    }

    internal class EmptyControl : UserControl
    {

    }

    internal class ViewToViewModel
    {
        public ViewToViewModel(UserControl control, IBaseViewModel type, MyContentRegion region, params object[] args)
        {
            View = control;
            ViewModel = type;
            Region = region;
            if (args != null)
                Args = args;
        }

        public object[] Args { get; set; }

        public Type ViewModelType { get { return ViewModel.GetType(); } }

        public UserControl View { get; set; }
        public MyContentRegion Region { get; set; }

        public IBaseViewModel ViewModel { get; set; }
    }
}
