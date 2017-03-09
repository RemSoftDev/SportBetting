using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLogger.Appenders;

namespace NLogger
{
    internal class RootAppender : ILogAppender
    {

        #region Fields

        private Queue<LogItem> _queue;

        #endregion


        #region Constructors and Destructors

        public RootAppender()
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
        public event Logger.LogWritten OnLogWritten;

        public void Log(string message, Exception exception, LoggingLevel level)
        {
            //_queue.Enqueue(new LogItem(message, exception, level));
            if (OnLogWritten == null) return;
            OnLogWritten(new List<LogItem>() {new LogItem(message, exception, level)});
        }
    }
}
