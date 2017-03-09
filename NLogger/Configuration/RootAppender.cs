using System.Configuration;

namespace NLogger.Configuration
{
    /// <summary>
    /// Root appender
    /// </summary>
    public class RootAppender : ConfigurationElement
    {
        /// <summary>
        /// Logging level
        /// </summary>
        [ConfigurationProperty("level")]
        public NLoggerAppenderLevel Level
        {
            get { return (NLoggerAppenderLevel)this["level"]; }
            set { this["level"] = value; }
        }

        /// <summary>
        /// Logging pattern
        /// </summary>
        [ConfigurationProperty("pattern")]
        public NLoggerAppenderPattern Pattern
        {
            get { return (NLoggerAppenderPattern)this["pattern"]; }
            set { this["pattern"] = value; }
        }
    }
}