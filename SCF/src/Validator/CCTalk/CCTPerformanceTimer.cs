using System;
using System.Runtime.InteropServices;

namespace Nbt.Services.Scf.CashIn.Validator.CCTalk
{

    internal class CCTPerformanceTimer
    {

        private long offset;
        private bool paused;
        private long qpcStart;

        private static double ticksFactor;

        public bool IsRunning
        {
            get
            {
                return !paused;
            }
        }

        public TimeSpan TimeSpan
        {
            get
            {
                long l1 = offset;
                if (!paused)
                {
                    long l2 = (long)0;
                    CCTPerformanceTimer.QueryPerformanceCounter(ref l2);
                    l1 += l2 - qpcStart;
                }
                return new TimeSpan((long)((double)l1 * CCTPerformanceTimer.ticksFactor));
            }
        }

        static CCTPerformanceTimer()
        {
            long l = (long)0;
            CCTPerformanceTimer.QueryPerformanceFrequency(ref l);
            CCTPerformanceTimer.ticksFactor = 10000000.0 / (double)l;
        }

        public CCTPerformanceTimer()
        {
            offset = (long)0;
            qpcStart = (long)0;
            paused = true;
        }

        public void Pause()
        {
            if (!paused)
            {
                paused = true;
                long l = (long)0;
                CCTPerformanceTimer.QueryPerformanceCounter(ref l);
                offset += l - qpcStart;
            }
        }

        public void Resume()
        {
            if (paused)
            {
                paused = false;
                CCTPerformanceTimer.QueryPerformanceCounter(ref qpcStart);
            }
        }

        public void Start()
        {
            offset = (long)0;
            qpcStart = (long)0;
            paused = false;
            CCTPerformanceTimer.QueryPerformanceCounter(ref qpcStart);
        }

        [PreserveSig]
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private static extern bool QueryPerformanceCounter(ref long x);

        [PreserveSig]
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private static extern bool QueryPerformanceFrequency(ref long x);

    } // class CCTPerformanceTimer

}

