using System.Configuration;

namespace NLogger.Configuration
{
    /// <summary>
    /// Log pattern configuration element
    /// </summary>
    public class NLoggerAppenderPattern : ConfigurationElement
    {
        /// <summary>
        /// Pattern value
        /// </summary>
        [ConfigurationProperty("value")]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }
    }
}