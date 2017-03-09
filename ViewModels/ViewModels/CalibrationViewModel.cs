using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using TranslationByMarkupExtension;
using System;
using SportBetting.WPF.Prism.Shared;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class CalibrationViewModel : BaseViewModel
    {
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_SHOWWINDOW = 0x0040;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd,
            int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
        static Process exeProcess = new Process();
        static Process twmProcess = new Process();
        private string ProcessName;

        #region Constructor and destructors


        public CalibrationViewModel()
        {
            CloseCommand = new Command(Close);
            PutProcessOnTopCommand = new Command(putProcessOnTop);
            Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.CloseCurrentWindow);

        }

        public override void OnNavigationCompleted()
        {
            //EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));
            base.OnNavigationCompleted();
            //runCalibration();
            if (ChangeTracker.IsCalibration)
            {
                runCalibration();
            }
            else
            {
                runTouchScreenAssignment();
            }

        }

        #endregion

        #region Properties
        #endregion

        #region Commands
        public Command CloseCommand { get; private set; }
        public Command PutProcessOnTopCommand { get; private set; }
        #endregion
      

        #region Methods
        

        public void CloseCurrentWindow(bool state)
        {
            ChangeTracker.IsCalibration = false;
            Close();
        }

        public override void Close()
        {
            
            try
            {
                if (exeProcess.ProcessName == ProcessName) exeProcess.Kill();
            }
            catch (Exception e)
            {
                
            }
            base.Close();
        }

        //static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    putProcessOnTop(exeProcess);
        //}

        [AsyncMethod]
        private void runCalibration()
        { 
            string url = ConfigurationManager.AppSettings["calibration_tool_path"];
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = url;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "TwUICP.dll CPMain";

            try
            {
                using (exeProcess = Process.Start(startInfo))
                {
                    ProcessName = exeProcess.ProcessName;
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_CANNOT_RUN_CALIBRATION_TOOL).ToString(), ErrorOkClick);
            }
        }

        [AsyncMethod]
        private void runTouchScreenAssignment()
        {
            string twMonitorUrl = ConfigurationManager.AppSettings["tw_monitor_path"];
            ProcessStartInfo twMonitorProcess = new ProcessStartInfo();
            twMonitorProcess.CreateNoWindow = false;
            twMonitorProcess.UseShellExecute = false;
            twMonitorProcess.FileName = twMonitorUrl;
            twMonitorProcess.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                twmProcess = Process.Start(twMonitorProcess);
            }
            catch
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_CANNOT_RUN_ASSIGNMENT_TOOL).ToString(), ErrorOkClick);
            }


            string url = ConfigurationManager.AppSettings["touch_screen_assignment_tool_path"];
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = url;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            try
            {
                using (exeProcess = Process.Start(startInfo))
                {
                    ProcessName = exeProcess.ProcessName;
                    putWindowAsside();
                    exeProcess.WaitForExit();
                    
                }
            }
            catch
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_CANNOT_RUN_ASSIGNMENT_TOOL).ToString(), ErrorOkClick);
            }
        }

        private void putProcessOnTop()
        {
            try
            {
                if (ChangeTracker.IsCalibration)
                {
                    SetWindowPos(exeProcess.MainWindowHandle,
                             HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                }
                else
                {
                    SetWindowPos(exeProcess.MainWindowHandle,
                             HWND_TOPMOST, -230, 0, 0, 0, SWP_NOSIZE);
                }
                
            }
            catch (Exception e)
            {
                
            }
               
        }

        [AsyncMethod]
        private void putWindowAsside()
        {
            try
            {
                SetWindowPos(exeProcess.MainWindowHandle,
                             HWND_TOPMOST, -230, 0, 0, 0, SWP_NOSIZE);
            }
            catch (Exception e)
            {

            }
        }

        void ErrorOkClick(object sender, EventArgs e)
        {
            Close();
        }


        #endregion
    }
}
