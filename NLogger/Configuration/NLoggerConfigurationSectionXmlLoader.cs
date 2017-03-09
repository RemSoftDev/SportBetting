using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NLogger.Configuration
{
    /// <summary>
    /// Customization to enable loading from an XML string
    /// </summary>
    public sealed class NLoggerConfigurationSectionXmlLoader : NLoggerConfigurationSection
    {
        /// <summary>
        /// Initializes a new class
        /// </summary>
        /// <param name="xml">XML string</param>
        public NLoggerConfigurationSectionXmlLoader(string xml)
        {
            using (var reader = new XmlTextReader(new StringReader(xml)))
            {
                DeserializeSection(reader);
            }
        }
    }
}
