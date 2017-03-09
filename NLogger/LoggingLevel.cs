using System;
using System.ComponentModel;

namespace NLogger
{
    /// <summary>
    /// Logging level types
    /// </summary>
    public enum LoggingLevel
    {
        /// <summary>
        /// Fatal error message
        /// </summary>
        [Description("Fatal")]
        Fatal,

        /// <summary>
        /// Error message
        /// </summary>
        [Description("Error")]
        Error,

        /// <summary>
        /// Warning message
        /// </summary>
        [Description("Warning")]
        Warning,

        /// <summary>
        /// Information message
        /// </summary>
        [Description("Info")]
        Info,

        /// <summary>
        /// Debug message
        /// </summary>
        [Description("Debug")]
        Debug,

        /// <summary>
        /// Trace message
        /// </summary>
        [Description("Trace")]
        Trace
    }
}