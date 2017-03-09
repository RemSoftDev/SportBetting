using System.Configuration;

namespace NLogger.Configuration
{
    /// <summary>
    /// Appender configuration section
    /// </summary>
    public class NLoggerAppenderCollection : ConfigurationElementCollection
    {
        private const string CollectionElementName = "appender";

        public override ConfigurationElementCollectionType CollectionType { get { return ConfigurationElementCollectionType.BasicMap; } }

        protected override string ElementName { get { return CollectionElementName; } }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NLoggerAppender();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NLoggerAppender) element).Name;
        }
    }
}