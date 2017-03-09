using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NLogger.Appenders;
using NLogger.Configuration;

namespace NLogger
{
    /// <summary>
    /// Logger implementation
    /// </summary>
    public class Logger : ILogger
    {

        #region Fields

        private static readonly Dictionary<string, Func<LogItem, string>> DefaultFormatting = new Dictionary
            <string, Func<LogItem, string>>
            {
                {"%exception", x => x.Exception != null ? x.Exception.Message : ""},
                {
                    "%stacktrace",
                    x =>
                    x.Exception != null && x.Exception.StackTrace != null
                        ? x.Exception.StackTrace
                        : ""
                },
                {"%date", x => x.Created.ToString("yyyy/MM/dd HH:mm:ss.fffffff")},
                {"%shortdate", x => x.Created.ToString("yyyy/MM/dd HH:mm:ss")},
                {"%message", x => x.Message},
                {"%level", x => x.Level.ToString()}
            };

        #endregion

        /// <summary>
        /// Formats the log item into string according to specified formatting parameters
        /// </summary>
        /// <param name="format">Log format</param>
        /// <param name="item">Log item</param>
        /// <param name="overrides">Formatting parameter overrides</param>
        /// <returns>Formatted string</returns>
        public static string FormatLog(string format, LogItem item, Dictionary<string, Func<LogItem, string>> overrides = null)
        {
            for (var i = 0; i < DefaultFormatting.Count; i++)
            {
                string replace;
                var element = DefaultFormatting.ElementAt(i);

                if (overrides != null)
                {
                    replace = overrides.ContainsKey(element.Key)
                                  ? overrides[element.Key].Invoke(item)
                                  : element.Value.Invoke(item);
                    overrides.Remove(element.Key);
                }
                else
                    replace = element.Value.Invoke(item);
                format = format.Replace(element.Key, replace);
            }

            if (overrides != null)
                foreach (var or in overrides)
                    format = format.Replace(or.Key, or.Value.Invoke(item));

            return format;
        }

        /// <summary>
        /// Log Written delegated
        /// </summary>
        /// <param name="logItems">Log items</param>
        public delegate void LogWritten(IList<LogItem> logItems);

        #region Constructors and destructors

        /// <summary>
        /// Initializes a new Logger class
        /// </summary>
        public Logger()
        {
            Root = new RootAppender();
            Appenders = new List<ILogAppender>();
        }

        #endregion

        public ILogger Initialize(NLoggerConfigurationSection config = null, bool ignoreConfigurationSection = false)
        {
            if (config == null && !ignoreConfigurationSection)
                config = ConfigurationManager.GetSection("NLoggerConfiguration") as NLoggerConfigurationSection;

            if (config == null) return this;
            if (config.Root != null)
                Root.LoggingLevels = GetLoggingLevels(config.Root);


            foreach (NLoggerAppender item in config.Appenders)
            {
                var type = item.Type.Split(',');
                var appender = (ILogAppender)Activator.CreateInstance(type[1].Trim(), type[0].Trim()).Unwrap();

                appender.Parameters = item.Parameters;
                appender.LoggingLevels = GetLoggingLevels(item);
                if (item.Pattern != null)
                    appender.LogPattern = item.Pattern.Value;
                appender.Name = item.Name;
                appender.MaxQueueCache = item.MaxQueueSize;
                appender.TimeBetweenChecks = item.TimeBetweenChecks;
                if (item.TimeSinceLastWrite != "")
                {
                    TimeSpan timeSinceLastWrite;
                    if (TimeSpan.TryParse(item.TimeSinceLastWrite, out timeSinceLastWrite))
                        appender.TimeSinceLastWrite = timeSinceLastWrite;

                }
                if (item.File != null)
                {
                    appender.Location = item.File.Location;
                    if (string.IsNullOrEmpty(item.File.MaxSize))
                    {
                        appender.MaxFileSize = 30000 * 1024;
                    }
                    else
                    {
                        appender.MaxFileSize = Convert.ToInt64(item.File.MaxSize) * 1024;
                    }
                    appender.MaxLogCount = item.File.MaxCount;
                }

                Appenders.Add(appender);
            }
            return this;
        }


        #region Properties

        public IList<ILogAppender> Appenders { get; set; }
        public ILogAppender Root { get; set; }
        public long Queued { get { return Root.Queued; } }
        public bool Debug { get; set; }

        public LoggingLevel DefaultLoggingLevel { get; set; }

        #endregion


        #region ILogger Implemented methods

        public void Log(string message, Exception exception, LoggingLevel level)
        {
            bool rootlevel = Root.LoggingLevels.Contains(level);

            if (rootlevel)
                Root.Log(message, exception, level);

            for (var i = 0; i < Appenders.Count; i++)
            {
                if (Appenders[i].LoggingLevels.Count == 0)
                {
                    if (rootlevel)
                        Appenders[i].Log(message, exception, level);
                    continue;
                }

                if ((Appenders[i].LoggingLevels.Contains(level)))
                    Appenders[i].Log(message, exception, level);
            }

        }


        #region Log overloads

        public void Log(string message, LoggingLevel level)
        {
            Log(message, null, level);
        }

        public void Log(string message)
        {
            Log(message, DefaultLoggingLevel);
        }

        public void Log(string message, Exception exception)
        {
            Log(message, exception, DefaultLoggingLevel);
        }

        public void LogFatal(string message)
        {
            Log(message, LoggingLevel.Fatal);
        }

        public void LogFatal(string message, Exception exception)
        {
            Log(message, exception, LoggingLevel.Fatal);
        }

        public void LogError(string message)
        {
            Log(message, LoggingLevel.Error);
        }

        public void LogError(string message, Exception exception)
        {
            Log(message, exception, LoggingLevel.Error);
        }

        public void LogWarning(string message)
        {
            Log(message, LoggingLevel.Warning);
        }

        public void LogWarning(string message, Exception exception)
        {
            Log(message, exception, LoggingLevel.Warning);
        }

        public void LogInfo(string message)
        {
            Log(message, LoggingLevel.Info);
        }

        public void LogInfo(string message, Exception exception)
        {
            Log(message, exception, LoggingLevel.Info);
        }

        public void LogDebug(string message)
        {
            Log(message, LoggingLevel.Debug);
        }

        public void LogDebug(string message, Exception exception)
        {
            Log(message, exception, LoggingLevel.Debug);
        }

        public void LogTrace(string message)
        {
            Log(message, LoggingLevel.Trace);
        }

        public void LogTrace(string message, Exception exception)
        {
            Log(message, exception, LoggingLevel.Trace);
        }

        #endregion

        #endregion


        #region IDisposable Implemented Methods

        public void Dispose()
        {
            Root.Dispose();
            for (var i = 0; i < Appenders.Count; i++)
                Appenders[i].Dispose();

            Appenders = null;
        }

        #endregion


        #region Private methods

        private LoggingLevel GetLoggingLevel(string value)
        {
            return (LoggingLevel)Enum.Parse(typeof(LoggingLevel), value);
        }

        private static List<LoggingLevel> GetLoggingLevels(Configuration.RootAppender appender)
        {
            if (appender == null)
                return new List<LoggingLevel>();
            var list = new List<LoggingLevel>();
            if (appender.Level.Fatal)
                list.Add(LoggingLevel.Fatal);
            if (appender.Level.Error)
                list.Add(LoggingLevel.Error);
            if (appender.Level.Warning)
                list.Add(LoggingLevel.Warning);
            if (appender.Level.Debug)
                list.Add(LoggingLevel.Debug);
            if (appender.Level.Info)
                list.Add(LoggingLevel.Info);
            if (appender.Level.Trace)
                list.Add(LoggingLevel.Trace);

            return list;
        }

        #endregion


    }
}