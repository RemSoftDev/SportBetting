using System;
using System.Collections.Generic;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using TranslationByMarkupExtension;

namespace ViewModels.ViewModels
{



    [ServiceAspect]
    public class SystemInfoMonitorsViewModel : BaseViewModel
    {      
        #region Constructors

        public SystemInfoMonitorsViewModel()
        {
            IdentifyMonitors = new Command(OnIdentifyMonitors);
            CalibrateMonitor = new Command(OnCalibrateMonitor);
            TouchScreenAssignment = new Command(OnTouchScreenAssignment);
            PrintChangeOrientationBarcode = new Command(OnPrintChangeOrientationBarcode);
            Mediator.Register<string>(this, ShowSystemInfoMonitors, MsgTag.LanguageChosenHeader);
        }

        #endregion

        #region Properties

        private bool _isFocused;

        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                if (_isFocused)
                {
                    Mediator.SendMessage(MsgTag.ShowKeyboard, MsgTag.ShowKeyboard);
                }
                else
                {
                    Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                }
            }
        }

        public string MonitorResolution_1
        {
            get { return _monitor_1_resolution; }
            set { _monitor_1_resolution = value; OnPropertyChanged("MonitorResolution_1"); }
        }
        public string MonitorOrientation_1
        {
            get { return _monitor_1_orientation; }
            set { _monitor_1_orientation = value; OnPropertyChanged("MonitorOrientation_1"); }
        }
        public string MonitorConfiguration_1
        {
            get { return _monitor_1_configuration; }
            set { _monitor_1_configuration = value; OnPropertyChanged("MonitorConfiguration_1"); }
        }

        public string MonitorResolution_2
        {
            get { return _monitor_2_resolution; }
            set { _monitor_2_resolution = value; OnPropertyChanged("MonitorResolution_2"); }
        }
        public string MonitorOrientation_2
        {
            get { return _monitor_2_orientation; }
            set { _monitor_2_orientation = value; OnPropertyChanged("MonitorOrientation_2"); }
        }
        public string MonitorConfiguration_2
        {
            get { return _monitor_2_configuration; }
            set { _monitor_2_configuration = value; OnPropertyChanged("MonitorConfiguration_2"); }
        }

        public string MonitorResolution_3
        {
            get { return _monitor_3_resolution; }
            set { _monitor_3_resolution = value; OnPropertyChanged("MonitorResolution_3"); }
        }
        public string MonitorOrientation_3
        {
            get { return _monitor_3_orientation; }
            set { _monitor_3_orientation = value; OnPropertyChanged("MonitorOrientation_3"); }
        }
        public string MonitorConfiguration_3
        {
            get { return _monitor_3_configuration; }
            set { _monitor_3_configuration = value; OnPropertyChanged("MonitorConfiguration_3"); }
        }
        public string MonitorResolution_4
        {
            get { return _monitor_4_resolution; }
            set { _monitor_4_resolution = value; OnPropertyChanged("MonitorResolution_4"); }
        }
        public string MonitorOrientation_4
        {
            get { return _monitor_4_orientation; }
            set { _monitor_4_orientation = value; OnPropertyChanged("MonitorOrientation_4"); }
        }
        public string MonitorConfiguration_4
        {
            get { return _monitor_4_configuration; }
            set { _monitor_4_configuration = value; OnPropertyChanged("MonitorConfiguration_4"); }
        }

        public string MonitorResolution_5
        {
            get { return _monitor_5_resolution; }
            set { _monitor_5_resolution = value; OnPropertyChanged("MonitorResolution_5"); }
        }
        public string MonitorOrientation_5
        {
            get { return _monitor_5_orientation; }
            set { _monitor_5_orientation = value; OnPropertyChanged("MonitorOrientation_5"); }
        }
        public string MonitorConfiguration_5
        {
            get { return _monitor_5_configuration; }
            set { _monitor_5_configuration = value; OnPropertyChanged("MonitorConfiguration_5"); }
        }

       
        public bool IsFooterMonitorVisible_1
        {
            get { return _footer_monitor_1_visible; }
            set
            {
                _footer_monitor_1_visible = value;
                OnPropertyChanged("IsFooterMonitorVisible_1");
            }
        }
        public bool IsFooterMonitorVisible_2
        {
            get { return _footer_monitor_2_visible; }
            set
            {
                _footer_monitor_2_visible = value;
                OnPropertyChanged("IsFooterMonitorVisible_2");
            }
        }
        public bool IsFooterMonitorVisible_3
        {
            get { return _footer_monitor_3_visible; }
            set
            {
                _footer_monitor_3_visible = value;
                OnPropertyChanged("IsFooterMonitorVisible_3");
            }
        }
        public bool IsFooterMonitorVisible_4
        {
            get { return _footer_monitor_4_visible; }
            set
            {
                _footer_monitor_4_visible = value;
                OnPropertyChanged("IsFooterMonitorVisible_4");
            }
        }

        private string _monitor_1_resolution;
        private string _monitor_1_orientation;
        private string _monitor_1_configuration;

        private string _monitor_2_resolution;
        private string _monitor_2_orientation;
        private string _monitor_2_configuration;

        private string _monitor_3_resolution;
        private string _monitor_3_orientation;
        private string _monitor_3_configuration;

        private string _monitor_4_resolution;
        private string _monitor_4_orientation;
        private string _monitor_4_configuration;

        private string _monitor_5_resolution;
        private string _monitor_5_orientation; 
        private string _monitor_5_configuration;

        private bool _footer_monitor_1_visible;
        private bool _footer_monitor_2_visible;
        private bool _footer_monitor_3_visible;
        private bool _footer_monitor_4_visible;

        #endregion

        #region Commands
        public Command IdentifyMonitors { get; private set; }
        public Command CalibrateMonitor { get; private set; }
        public Command TouchScreenAssignment { get; private set; }
        public Command PrintChangeOrientationBarcode { get; private set; }

        #endregion

        #region Methods
        public override void OnNavigationCompleted()
        {
            ChangeTracker.SystemInfoMonitorsChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_PRINT_SYSTEM;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_MONITORS;
            ShowSystemInfoMonitors("");
            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            base.Close();
        }

       
        private void OnIdentifyMonitors()
        {
            DisplayHelper dh = new DisplayHelper();
            dh.EnumerateDisplays();
        }

        private void OnCalibrateMonitor()
        {
            ChangeTracker.IsCalibration = true;
            Mediator.SendMessage("", MsgTag.ShowCalibration);
        }

        private void OnTouchScreenAssignment()
        {
            ChangeTracker.IsCalibration = false;
            Mediator.SendMessage("", MsgTag.ShowCalibration);
        }

        private void OnPrintChangeOrientationBarcode()
        {
            PrinterHandler.PrintChangeOrientationBarcode();
        }

        private void ShowSystemInfoMonitors(String s)
        {
            LoadMonitorsInfo();
        }

        private void LoadMonitorsInfo()
        {
            DisplayHelper dh = new DisplayHelper();
            List <MonitorData> md = dh.GetDisplaysData();

            for (int i= 0; i < md.Count; i++)
            //foreach (MonitorData m in md)
            {
                switch (md[i].position)
                {
                    case 1:
                        MonitorResolution_1 = md[i].width.ToString() + "x" + md[i].height.ToString();
                        MonitorOrientation_1 = md[i].orientation;
                        MonitorConfiguration_1 = md[i].conf_mode;
                        IsFooterMonitorVisible_1 = !String.IsNullOrEmpty(MonitorResolution_1);
                    break;
                    case 2:
                        MonitorResolution_2 = md[i].width.ToString() + "x" + md[i].height.ToString();
                        MonitorOrientation_2 = md[i].orientation;
                        MonitorConfiguration_2 = md[i].conf_mode;
                        IsFooterMonitorVisible_2 = !String.IsNullOrEmpty(MonitorResolution_2);
                    break;
                    case 3:
                        MonitorResolution_3 = md[i].width.ToString() + "x" + md[i].height.ToString();
                        MonitorOrientation_3 = md[i].orientation;
                        MonitorConfiguration_3 = md[i].conf_mode;
                        IsFooterMonitorVisible_3 = !String.IsNullOrEmpty(MonitorResolution_3);
                    break;   
                    case 4:
                    
                        MonitorResolution_4 = md[i].width.ToString() + "x" + md[i].height.ToString();
                        MonitorOrientation_4 = md[i].orientation;
                        MonitorConfiguration_4 = md[i].conf_mode;
                        IsFooterMonitorVisible_4 = !String.IsNullOrEmpty(MonitorResolution_4);

                    break;
                    case 5:
                         MonitorResolution_5 = md[i].width.ToString() + "x" + md[i].height.ToString();
                         MonitorOrientation_5 = md[i].orientation;
                         MonitorConfiguration_5 = md[i].conf_mode;
                    break;
                    default:
                    break;
                }
            }

            IsFooterMonitorVisible_1 &= IsFooterMonitorVisible_2;
            IsFooterMonitorVisible_2 &= IsFooterMonitorVisible_3;
            IsFooterMonitorVisible_3 &= IsFooterMonitorVisible_4;
            IsFooterMonitorVisible_4 = !String.IsNullOrEmpty(MonitorResolution_5);

        }
    
        #endregion
    }
}