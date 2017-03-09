using System.Configuration;

namespace NLogger.Configuration
{
    /// <summary>
    /// Logger configuration section
    /// </summary>
    public class NLoggerConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Appenders section
        /// </summary>
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public NLoggerAppenderCollection Appenders
        {
            get { return (NLoggerAppenderCollection) base[""]; }
        }

        /// <summary>
        /// Root appender section
        /// </summary>
        [ConfigurationProperty("root")]
        public RootAppender Root
        {
            get { return (RootAppender) base["root"]; }
        }
    }
}
