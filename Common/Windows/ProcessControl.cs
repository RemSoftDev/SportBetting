using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadState = System.Diagnostics.ThreadState;

namespace SportRadar.Common.Windows
{
    public static class SystemControl
    {
        private static PerformanceCounter m_pcCpu = null;
        private static PerformanceCounter m_pcMem = null;

        static SystemControl()
        {
            m_pcCpu = new PerformanceCounter();
            m_pcCpu.CategoryName = "Processor";
            m_pcCpu.CounterName = "% Processor Time";
            m_pcCpu.InstanceName = "_Total";

            // will always start at 0
            m_pcMem = new PerformanceCounter("Memory", "Available MBytes");
            System.Threading.Thread.Sleep(1000);
        }

        public static string GetCurrentCpuUsage()
        {
            return m_pcCpu.NextValue() + " %";
        }

        // Call this method every time you need to get the amount of the available RAM in Mb 
        public static string GetAvailableRAM()
        {
            return m_pcMem.NextValue() + " Mb";
        }

        public static string GetSystemInfo()
        {
            string sFormat = @"
System:
CPU     {0}
Memory  {1}
";

            return string.Format(sFormat, GetCurrentCpuUsage(), GetAvailableRAM());
        }
    }

    public sealed class ProcessControl
    {
        private const int PAD_LEFT_CHAR_COUNT = 18;
        private const char PAD_CHAR = '.';
        private static readonly CultureInfo DEFAULT_CI = new CultureInfo("en-us");

        private Process m_proc = null;

        private static ProcessControl m_pcCurrent = null;

        public ProcessControl(Process proc)
        {
            m_proc = proc;
        }

        public static ProcessControl Current
        {
            get
            {
                if (m_pcCurrent == null)
                {
                    m_pcCurrent = new ProcessControl(Process.GetCurrentProcess());
                }

                return m_pcCurrent;
            }
        }

        public string MachineName { get { return m_proc.MachineName; } }
        public string ProcessName { get { return m_proc.ProcessName; } }
        public int ProcId { get { return m_proc.Id; } }

        public int ThreadCount { get { return m_proc.Threads.Count; } }
        public long PrivateMemorySize { get { return m_proc.PrivateMemorySize64; } }

        public long WorkingSet { get { return m_proc.WorkingSet64; } }
        public long PeakWorkingSet { get { return m_proc.PeakWorkingSet64; } }

        public long VirtualMemorySize { get { return m_proc.VirtualMemorySize64; } }
        public long PeakVirtualMemorySize { get { return m_proc.PeakVirtualMemorySize64; } }

        public long PagedMemorySize { get { return m_proc.PagedMemorySize64; } }
        public long PeakPagedMemorySize { get { return m_proc.PeakPagedMemorySize64; } }

        public long PagedSystemMemorySize { get { return m_proc.PagedSystemMemorySize64; } }
        public long NonpagedSystemMemorySize { get { return m_proc.NonpagedSystemMemorySize64; } }

        public override string ToString()
        {
            return string.Format("Process {{Machine='{0}', Name='{1}', Id={2}}}", this.MachineName, this.ProcessName, this.ProcId);
        }

        public static string LongToString(long lValue)
        {
            return lValue.ToString("N0", DEFAULT_CI).PadLeft(PAD_LEFT_CHAR_COUNT, PAD_CHAR);
        }

        private static string ProcessThreadToString(ProcessThread pt)
        {
            return string.Format("ProcessThread {{Id={0}, Started={1}, Priority={2}, State={3}}}", pt.Id, pt.StartTime, pt.PriorityLevel, pt.ThreadState);
        }

        public string GetThreadSummary()
        {
            int iCount = m_proc.Threads.Count;
            string sThreadSummary = string.Format("Thread Count {0}", iCount);

            for (int i = 0; i < iCount; i++)
            {
                try
                {
                    ProcessThread pt = m_proc.Threads[i];
                    if (pt.ThreadState == ThreadState.Running)
                        sThreadSummary += ProcessThreadToString(pt) + "\r\n";
                }
                catch (Exception excp)
                {
                }
            }

            return sThreadSummary;
        }

        public string ToDetailedString()
        {
            string sFormat = @"
{0}

ThreadCount = {1}

PrivateMemorySize.........{2}

WorkingSet................{3}
PeakWorkingSet............{4}

VirtualMemorySize.........{5}
PeakVirtualMemorySize.....{6}

PagedMemorySize...........{7}
PeakPagedMemorySize.......{8}

PagedSystemMemorySize.....{9}
NonpagedSystemMemorySize..{10}
";

            return string.Format(sFormat,
                this.ToString(), this.ThreadCount, LongToString(this.PrivateMemorySize),
                LongToString(this.WorkingSet), LongToString(this.PeakWorkingSet),
                LongToString(this.VirtualMemorySize), LongToString(this.PeakVirtualMemorySize),
                LongToString(this.PagedMemorySize), LongToString(this.PeakPagedMemorySize),
                LongToString(this.PagedSystemMemorySize), LongToString(this.NonpagedSystemMemorySize));
        }
    }
}
