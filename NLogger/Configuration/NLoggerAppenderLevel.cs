using System.Configuration;

namespace NLogger.Configuration
{
    /// <summary>
    /// Appender levels definition
    /// </summary>
    public class NLoggerAppenderLevel : ConfigurationElement
    {
        /// <summary>
        /// Log fatal messages
        /// </summary>
        [ConfigurationProperty("fatal")]
        public bool Fatal
        {
            get { return (bool) this["fatal"]; }
            set { this["fatal"] = value; }
        }

        /// <summary>
        /// Log error messages
        /// </summary>
        [ConfigurationProperty("error")]
        public bool Error
        {
            get { return (bool) this["error"]; }
            set { this["error"] = value; }
        }

        /// <summary>
        /// Log warning messages
        /// </summary>
        [ConfigurationProperty("warning")]
        public bool Warning
        {
            get { return (bool)this["warning"]; }
            set { this["warning"] = value; }
        }

        /// <summary>
        /// Log info messages
        /// </summary>
        [ConfigurationProperty("info")]
        public bool Info
        {
            get { return (bool)this["info"]; }
            set { this["info"] = value; }
        }

        /// <summary>
        /// Log debug messages
        /// </summary>
        [ConfigurationProperty("debug")]
        public bool Debug
        {
            get { return (bool)this["debug"]; }
            set { this["debug"] = value; }
        }

        /// <summary>
        /// Log trace messages
        /// </summary>
        [ConfigurationProperty("trace")]
        public bool Trace
        {
            get { return (bool)this["trace"]; }
            set { this["trace"] = value; }
        }
    }
}