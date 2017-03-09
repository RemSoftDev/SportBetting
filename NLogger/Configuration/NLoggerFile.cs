using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogger.Configuration
{
    /// <summary>
    /// Logger file configuration
    /// </summary>
    public class NLoggerFile : ConfigurationElement
    {
        /// <summary>
        /// File location
        /// </summary>
        [ConfigurationProperty("location")]
        public string Location
        {
            get { return (string) this["location"]; }
            set { this["location"] = value; }
        }

        /// <summary>
        /// Maximum allowed file size before creating a new file
        /// </summary>
        [ConfigurationProperty("maxsize")]
        public string MaxSize
        {
            get { return (string) this["maxsize"]; }
            set { this["maxsize"] = value; }
        }

        /// <summary>
        /// Maximum number of logs to keep
        /// </summary>
        [ConfigurationProperty("maxcount")]
        public int MaxCount
        {
            get { return (int) this["maxcount"]; }
            set { this["maxcount"] = value; }
        }
    }
}
