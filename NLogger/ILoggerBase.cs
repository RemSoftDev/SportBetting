using System;

namespace NLogger
{
    public interface ILoggerBase
    {
        /// <summary>
        /// Logs a message with exception and specified logging level
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="level">Logging level</param>
        void Log(string message, Exception exception, LoggingLevel level);
    }
}