using System.Configuration;

namespace NLogger.Configuration
{
    /// <summary>
    /// Generic logger appender configuration element
    /// </summary>
    public class NLoggerAppender : RootAppender
    {
        /// <summary>
        /// Appender name
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Appender type
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string) this["type"]; }
            set { this["type"] = value; }
        }

        /// <summary>
        /// Appender parameters
        /// </summary>
        [ConfigurationProperty("parameters", IsRequired = false)]
        public string Parameters
        {
            get { return (string) this["parameters"]; }
            set { this["parameters"] = value; }
        }

        /// <summary>
        /// Appender file options
        /// </summary>
        [ConfigurationProperty("file", IsRequired = false)]
        public NLoggerFile File
        {
            get { return (NLoggerFile) this["file"]; }
            set { this["file"] = value; }
        }

        /// <summary>
        /// Maximum time allowed between writes even if queue is not long enough
        /// </summary>
        [ConfigurationProperty("timesincelastwrite")]
        public string TimeSinceLastWrite
        {
            get { return (string) this["timesincelastwrite"]; }
            set { this["timesincelastwrite"] = value; }
        }

        /// <summary>
        /// Maximum allowed queue size before a write occurs
        /// </summary>
        [ConfigurationProperty("maxqueuesize")]
        public int MaxQueueSize
        {
            get { return (int) this["maxqueuesize"]; }
            set { this["maxqueuesize"] = value; }
        }

        /// <summary>
        /// Time between queue checks
        /// </summary>
        [ConfigurationProperty("timebetweenchecks")]
        public int TimeBetweenChecks
        {
            get { return (int) this["timebetweenchecks"]; }
            set { this["timebetweenchecks"] = value; }
        }
    }
}