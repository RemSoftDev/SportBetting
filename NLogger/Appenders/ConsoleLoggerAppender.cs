using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogger.Appenders
{
    /// <summary>
    /// Console Logger Appender
    /// </summary>
    public class ConsoleLoggerAppender : ILogAppender
    {
        public ConsoleLoggerAppender()
        {
            LoggingLevels = new List<LoggingLevel>();
        }
        public void Log(string message, Exception exception, LoggingLevel level)
        {
            Console.WriteLine(Logger.FormatLog(string.IsNullOrEmpty(LogPattern) ? DefaultLogPattern : LogPattern, new LogItem(message, exception, level)));
            if (OnLogWritten != null)
                OnLogWritten(new List<LogItem>() {new LogItem(message, exception, level)});
        }

        public void Dispose()
        {

        }

        private const string DefaultLogPattern = "[%date][%level] %message";

        public string Name { get; set; }
        public List<LoggingLevel> LoggingLevels { get; set; }
        public long Queued { get { return 0; } }
        public string LogPattern { get; set; }
        public string Parameters { get; set; }
        public TimeSpan TimeSinceLastWrite { get; set; }
        public int MaxQueueCache { get; set; }
        public int TimeBetweenChecks { get; set; }
        public long MaxFileSize { get; set; }
        public string Location { get; set; }
        public int MaxLogCount { get; set; }

        /// <summary>
        /// Fired when log is written
        /// </summary>
        public event Logger.LogWritten OnLogWritten;
    }
}
