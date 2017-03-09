using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogger
{
    internal static class EventLogWriter
    {
        private const string Source = "NLogger";
        private const string LogName = "Application";
        private static readonly object Lock = new object();

        public static void Log(string message, EventLogEntryType type, int id)
        {
            /*if(!EventLog.SourceExists(Source))
                EventLog.CreateEventSource(Source, LogName);

            EventLog.WriteEntry(Source, message, type, id);*/
            try
            {
                lock (Lock)
                {
                    File.AppendAllText(GetFileName(),
                                       string.Format("[{0}] {1} | {4} | {2}{3}", type, id, message, Environment.NewLine,
                                                     DateTime.Now));
                }
            }
            catch(Exception e)
            {}
        }

        private static string GetFileName()
        {
            var time = Process.GetCurrentProcess().StartTime;
            return string.Format(".\\log\\NLoggerErrors-{0}.log", time.ToString("yyyy-MM-dd_HH-mm-ss"));
        }

    }
}
