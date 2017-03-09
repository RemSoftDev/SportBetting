using System;
using System.Collections.Generic;

namespace NLogger.Appenders
{
    /// <summary>
    /// Defines the LogAppender interface
    /// </summary>
    public interface ILogAppender : ILoggerBase, IDisposable
    {
        /// <summary>
        /// Gets or sets the appender's name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Logging level
        /// </summary>
        List<LoggingLevel> LoggingLevels { get; set; }

        /// <summary>
        /// Number of items in the log queue
        /// </summary>
        long Queued { get; }

        /// <summary>
        /// Pattern for logging
        /// </summary>
        string LogPattern { get; set; }

        /// <summary>
        /// Parameters for logging
        /// </summary>
        string Parameters { get; set; }

        /// <summary>
        /// Maximum amount of time to cache data between writes
        /// </summary>
        TimeSpan TimeSinceLastWrite { get; set; }

        /// <summary>
        /// Maximum number of items in queue before writes
        /// </summary>
        int MaxQueueCache { get; set; }

        /// <summary>
        /// Time between cache write checks in milliseconds
        /// </summary>
        int TimeBetweenChecks { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed log file size before a new file is created
        /// </summary>
        long MaxFileSize { get; set; }

        /// <summary>
        /// Gets or sets the location of log file
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// Gets or sets the maximum amount of logs kept
        /// </summary>
        int MaxLogCount { get; set; }

        /// <summary>
        /// Event fired when log is written
        /// </summary>
        event Logger.LogWritten OnLogWritten;
    }
}