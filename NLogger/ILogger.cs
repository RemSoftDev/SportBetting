using System;
using System.Collections.Generic;
using NLogger.Appenders;
using NLogger.Configuration;

namespace NLogger
{
    public interface ILogger : ILoggerBase, IDisposable
    {
        /// <summary>
        /// Logger appenders
        /// </summary>
        IList<ILogAppender> Appenders { get; set; }

        /// <summary>
        /// Logger root
        /// </summary>
        ILogAppender Root { get; set; }

        /// <summary>
        /// Number of items in the queue
        /// </summary>
        long Queued { get; }

        /// <summary>
        /// Gets or sets debug mode
        /// </summary>
        bool Debug { get; set; }

        /// <summary>
        /// Gets or sets the default logging level
        /// </summary>
        LoggingLevel DefaultLoggingLevel { get; set; }

        /// <summary>
        /// Initializes the class and loads settings from configuration file
        /// </summary>
        /// <param name="configurationSection">Custom configuration section</param>
        /// <param name="ignoreConfigurationSection">Ignore configuration section</param>
        ILogger Initialize(NLoggerConfigurationSection configurationSection = null, bool ignoreConfigurationSection = false);


        /// <summary>
        /// Logs a message with default logging level
        /// </summary>
        /// <param name="message">Message to log</param>
        void Log(string message);

        /// <summary>
        /// Logs a message with specified logging level
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Logging level</param>
        void Log(string message, LoggingLevel level);


        /// <summary>
        /// Logs a message with exception and default logging level
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        void Log(string message, Exception exception);


        /// <summary>
        /// Logs a fatal message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogFatal(string message);

        /// <summary>
        /// Logs a fatal message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        void LogFatal(string message, Exception exception);


        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogError(string message);

        /// <summary>
        /// Logs an error message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        void LogError(string message, Exception exception);


        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogWarning(string message);

        /// <summary>
        /// Logs a warning message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        void LogWarning(string message, Exception exception);


        /// <summary>
        /// Logs an information message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogInfo(string message);

        /// <summary>
        /// Logs an information message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        void LogInfo(string message, Exception exception);


        /// <summary>
        /// Logs a debug message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogDebug(string message);

        /// <summary>
        /// Logs a debug message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        void LogDebug(string message, Exception exception);

        /// <summary>
        /// Logs a trace message
        /// </summary>
        /// <param name="message">Message to log</param>
        void LogTrace(string message);

        /// <summary>
        /// Logs a trace message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        void LogTrace(string message, Exception exception);
    }
}