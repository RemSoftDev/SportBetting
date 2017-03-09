using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Web.Script.Serialization;
using TranslationByMarkupExtension;
using IocContainer;
using Ninject;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Diagnostics;
using System.Windows.Interop;


namespace SportBetting.WPF.Prism.Shared
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        [MarshalAs(UnmanagedType.I4)]
        public int x;
        [MarshalAs(UnmanagedType.I4)]
        public int y;
    }

    [StructLayout(LayoutKind.Sequential,
    CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        // You can define the following constant
        // but OUTSIDE the structure because you know
        // that size and layout of the structure
        // is very important
        // CCHDEVICENAME = 32 = 0x50
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        // In addition you can define the last character array
        // as following:
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        //public Char[] dmDeviceName;

        // After the 32-bytes array
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmSpecVersion;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmDriverVersion;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmSize;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmDriverExtra;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmFields;

        public POINTL dmPosition;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayOrientation;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFixedOutput;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmColor;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmDuplex;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmYResolution;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmTTOption;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmCollate;

        // CCHDEVICENAME = 32 = 0x50
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        // Also can be defined as
        //[MarshalAs(UnmanagedType.ByValArray,
        //    SizeConst = 32, ArraySubType = UnmanagedType.U1)]
        //public Byte[] dmFormName;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmLogPixels;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmBitsPerPel;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPelsWidth;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPelsHeight;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFlags;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFrequency;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmICMMethod;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmICMIntent;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmMediaType;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDitherType;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmReserved1;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmReserved2;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPanningWidth;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPanningHeight;
    }

    public class MonitorData
    {
        public int height;
        public int width;
        public string orientation;
        public string conf_mode;
        public int position;

    }

    public class MonitorDataA
    {
        public string position;
        public string resolution;
        public string orientation;
        public string configuration;
    }

    public class DisplayHelper
    {
        [DllImport("user32.dll")]
        public extern static bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll",  EntryPoint = "SetWindowLong")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int ChangeDisplaySettings(
            [In, Out]
    ref DEVMODE lpDevMode,
            [param: MarshalAs(UnmanagedType.U4)]
    uint dwflags);

        private const int GWL_STYLE = -16;
        private const int SWP_SHOWWINDOW = 64;
        private const int HWND_TOPMOST = -1;

        List<Form> FormHolder = new List<Form>(MAX_DISPLAY_NUMBER);
        private static object syncRoot = new Object();
        private static volatile bool inProgress = false;
        const int BLINCK_TIME_MAX = 20;
        const int MAX_DISPLAY_NUMBER = 5;


        public DisplayHelper()
        {

        }
        ~DisplayHelper()
        {
            inProgress = false;
        }
        public static ITranslationProvider TranslationProvider
        {
            get { return IoCContainer.Kernel.Get<ITranslationProvider>(); }
        }
        private List<MonitorDataA> ConvertFromMonitorDataList(List<MonitorData> md)
        {
            List<MonitorDataA> mda = new List<MonitorDataA>(MAX_DISPLAY_NUMBER);
            foreach (MonitorData m in md)
            {
                if (m.width != 0 && m.height != 0)
                {
                    MonitorDataA monitor = new MonitorDataA();
                    monitor.position = m.position.ToString();
                    monitor.resolution = m.width.ToString() + "x" + m.height.ToString();
                    monitor.orientation = m.orientation;
                    monitor.configuration = m.conf_mode;
                    mda.Add(monitor);
                }
            }
            return mda;
        }

        public string GetDisplayDataJson()
        {
            List<MonitorDataA> mda = ConvertFromMonitorDataList(GetDisplaysData());
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            return serializer.Serialize(mda);
        }

        public DateTime MonitorsUpdatedTime()
        {
            //should be modified later
            return DateTime.Now;
        }
        public List<MonitorData> GetDisplaysData()
        {

            List<MonitorData> md = new List<MonitorData>(MAX_DISPLAY_NUMBER);
            Screen[] sc = Screen.AllScreens;

            for (int i = 0; i < sc.Length && i < MAX_DISPLAY_NUMBER; i++)
            {
                if (sc[i].DeviceName != null)
                {
                    MonitorData m = new MonitorData();
                   /* int len = sc[i].DeviceName.Length;
                    if (len > 0)
                    {
                        int.TryParse(sc[i].DeviceName.Substring(len - 1, 1), out m.position);
                    }*/
                    m.height = sc[i].Bounds.Height;
                    m.width = sc[i].Bounds.Width;
                    m.position = i+1;

                    if (sc[i].Bounds.Height < sc[i].Bounds.Width)
                    {
                        m.orientation = "Landscape";
                    }
                    else
                    {
                        m.orientation = "Portrait";
                    }
                    if (sc[i].Primary)
                    {
                        m.conf_mode = TranslationProvider.Translate(MultistringTags.TERMINAL_MAIN_MONITOR).ToString();// "\\main";
                    }
                    md.Add(m);
                }

            }

            return md;
        }

        private void ShowMonitorsPosition()
        {
            Screen[] sc = Screen.AllScreens;
            int len = 0;
            for (int i = 0; i < sc.Length; i++)
            {
                Form f = new Form();
                Panel pan = new Panel();
                Label lb = new Label();

                lb.AutoSize = true;
                lb.Location = new System.Drawing.Point(0, 0);
                lb.Font = new Font("Arial", 400);
              /*  if (sc[i].DeviceName != null)
                {
                    len = sc[i].DeviceName.Length;
                    if (len > 0)
                    {
                        lb.Text = sc[i].DeviceName.Substring(len - 1, 1);
                    }
                }
                else*/
                {
                    lb.Text = (i + 1).ToString();
                }
                pan.Size = f.Size = TextRenderer.MeasureText(lb.Text, lb.Font);

                lb.ForeColor = Color.White;
                lb.BackColor = Color.Transparent;
                pan.Controls.Add(lb);

                f.FormBorderStyle = FormBorderStyle.None;
                f.TransparencyKey = Color.Turquoise;
                f.BackColor = Color.Turquoise;

                f.Left = sc[i].Bounds.Left + 100;
                f.Top = sc[i].Bounds.Top + 100;
                f.StartPosition = FormStartPosition.Manual;
                f.TopMost = true;
                f.Controls.Add(pan);

                lb.Visible = false;
                f.Show();
                lb.Visible = true;
                f.Refresh();

                FormHolder.Add(f);
            }
        }

        private void ProcessDisplays()
        {
            ShowMonitorsPosition();
        
      
            for (int i = 0; i < BLINCK_TIME_MAX; i++)
            {
                foreach (Form f in FormHolder)
                {
                    if (f != null)
                    { 
                        if (f.Visible == true)
                        {
                            f.Visible = false;
                        }
                        else
                        {
                            f.Visible = true;
                        }
                    }
                }
                System.Threading.Thread.Sleep(200);
            }
            foreach (Form f in FormHolder)
            {
                if (f != null)
                {
                    f.Close();
                }
            }
            FormHolder.Clear();
            inProgress = false;
        }

        public void EnumerateDisplays()
        {
            lock (syncRoot)
            {
                if (!inProgress)
                {
                    inProgress = true;
                    new System.Threading.Thread(ProcessDisplays).Start();
                }
            }
        }

        public bool CheckProgress()
        {
            return inProgress;
        }

        public static void GetCurrentSettings()
        {
            DEVMODE mode = new DEVMODE();
            mode.dmSize = (ushort)Marshal.SizeOf(mode);

            if (EnumDisplaySettings(null,
                -1,
                ref mode) == true) // Succeeded
            {

            }
        }

        public static void RotateScreen(bool clockwise)
        {
            DEVMODE originalMode = new DEVMODE();
            originalMode.dmSize =
                (ushort)Marshal.SizeOf(originalMode);

            EnumDisplaySettings(null, -1, ref originalMode);

            // Making a copy of the current settings
            // to allow reseting to the original mode
            DEVMODE newMode = originalMode;

            // Rotating the screen
            if (clockwise)
                if (newMode.dmDisplayOrientation < 3)
                    newMode.dmDisplayOrientation++;
                else
                    newMode.dmDisplayOrientation = 0;

            // Swapping width and height;
            uint temp = newMode.dmPelsWidth;
            newMode.dmPelsWidth = newMode.dmPelsHeight;
            newMode.dmPelsHeight = temp;

            // Capturing the operation result
            // Capturing the operation result
            int result =
                ChangeDisplaySettings(ref newMode, 0);

            if (result == 0)
            {

                // Inspecting the new mode
                GetCurrentSettings();

                Console.WriteLine();

                // Waiting for seeing the results
                Console.ReadKey(true);

                ChangeDisplaySettings(ref originalMode, 0);
            }
            else if (result == -2)
                Console.WriteLine("Mode not supported.");
            else if (result == 1)
                Console.WriteLine("Restart required.");
            else
                Console.WriteLine("Failed. Error code = {0}", result);
        }

    }
}
