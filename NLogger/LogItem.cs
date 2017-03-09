using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogger
{
    /// <summary>
    /// Log item
    /// </summary>
    public class LogItem
    {
        /// <summary>
        /// Initializes a new LogItem class
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="level">Message level</param>
        public LogItem(string message, Exception exception = null, LoggingLevel level = LoggingLevel.Info)
        {
            Message = message;
            Exception = exception;
            Created = DateTime.Now;
            Level = level;
        }

        /// <summary>
        /// Message to log
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Logging datetime
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Exception to include with log
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Logging level
        /// </summary>
        public LoggingLevel Level { get; set; }
        
    }
}
