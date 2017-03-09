using System;
using System.Collections.Generic;

namespace NLogger.Appenders
{
    /// <summary>
    /// Memory Logger Appender
    /// </summary>
    public class MemoryLoggerAppender : ILogAppender
    {

        #region Fields

        private Queue<LogItem> _queue;

        #endregion


        #region Properties

        public string Name { get; set; }
        public List<LoggingLevel> LoggingLevels { get; set; }
        public long Queued { get { return _queue.Count; } }
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

        #endregion


        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new class of MemoryLoggerAppender
        /// </summary>
        public MemoryLoggerAppender()
        {
            _queue = new Queue<LogItem>();
            LoggingLevels = new List<LoggingLevel>();
        }

        #endregion


        public void Dispose()
        {
            _queue.Clear();
            _queue = null;
        }

        public void Log(string message, Exception exception, LoggingLevel level)
        {
            _queue.Enqueue(new LogItem(message, exception, level));
            if (OnLogWritten != null)
                OnLogWritten(new List<LogItem>() {new LogItem(message, exception, level)});
        }
    }
}
