using System;

namespace NLogger
{
    /// <summary>
    /// Static class for logging
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Instance of logger
        /// </summary>
        public static readonly ILogger Instance;

        static Log()
        {
            Instance = new Logger().Initialize();
        }

        public static void SetMaxSizeAndFileCount(long fileSize, int fileCount)
        {
            for (int i = 0; i < Instance.Appenders.Count; i++)
            {
                Instance.Appenders[i].MaxFileSize = fileSize;
                Instance.Appenders[i].MaxLogCount = fileCount;
            }

        }

        private static void LogMessage(string message, Exception exception = null,
                                       LoggingLevel level = LoggingLevel.Info)
        {
            if (Instance != null)
                Instance.Log(message, exception, level);
        }

        /// <summary>
        ///     Logs a fatal message
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Fatal(string message)
        {
            LogMessage(message, null, LoggingLevel.Fatal);
        }

        /// <summary>
        ///     Logs a fatal message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        public static void Fatal(string message, Exception exception)
        {
            LogMessage(message, exception, LoggingLevel.Fatal);
        }


        /// <summary>
        ///     Logs an error message
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Error(string message)
        {
            LogMessage(message, null, LoggingLevel.Error);
        }

        /// <summary>
        ///     Logs an error message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        public static void Error(string message, Exception exception)
        {
            LogMessage(message, exception, LoggingLevel.Error);
        }


        /// <summary>
        ///     Logs a warning message
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Warning(string message)
        {
            LogMessage(message, null, LoggingLevel.Warning);
        }

        /// <summary>
        ///     Logs a warning message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        public static void Warning(string message, Exception exception)
        {
            LogMessage(message, exception, LoggingLevel.Warning);
        }


        /// <summary>
        ///     Logs an information message
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Info(string message)
        {
            LogMessage(message);
        }

        /// <summary>
        ///     Logs an information message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        public static void Info(string message, Exception exception)
        {
            LogMessage(message, exception);
        }


        /// <summary>
        ///     Logs a debug message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Debug(string message)
        {
            LogMessage(message, null, LoggingLevel.Debug);
        }

        /// <summary>
        ///     Logs a debug message with exception
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        public static void Debug(string message, Exception exception)
        {
            LogMessage(message, exception, LoggingLevel.Debug);
        }

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Trace(string message)
        {
            LogMessage(message, null, LoggingLevel.Trace);
        }

        /// <summary>
        ///     Logs a trace message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        public static void Trace(string message, Exception exception)
        {
            LogMessage(message, exception, LoggingLevel.Trace);
        }


        /// <summary>
        /// Logs a fatal message with format arguments
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Message arguments</param>
        public static void FatalFormat(string message, params object[] args)
        {
            Fatal(string.Format(message, args));
        }

        /// <summary>
        /// Logs an error message with format arguments
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Message arguments</param>
        public static void ErrorFormat(string message, params object[] args)
        {
            Error(string.Format(message, args));
        }

        /// <summary>
        /// Logs a warning message with format arguments
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Message arguments</param>
        public static void WarningFormat(string message, params object[] args)
        {
            Warning(string.Format(message, args));
        }

        /// <summary>
        /// Logs a debug message with format arguments
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Message arguments</param>
        public static void DebugFormat(string message, params object[] args)
        {
            if (args.Length > 0)
                Debug(string.Format(message, args));
            else
                Debug(message);
        }

        /// <summary>
        /// Logs an info message with format arguments
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Message arguments</param>
        public static void InfoFormat(string message, params object[] args)
        {
            if(args.Length > 0)
            Info(string.Format(message, args));
            else
            Info(message);
        }

        /// <summary>
        /// Logs a trace message with format arguments
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="args">Message arguments</param>
        public static void TraceFormat(string message, params object[] args)
        {
            Trace(string.Format(message, args));
        }
    }
}