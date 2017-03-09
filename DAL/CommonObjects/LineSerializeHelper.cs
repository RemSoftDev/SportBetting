using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SportRadar.Common.Windows;

namespace SportRadar.DAL.CommonObjects
{
    public static class LineSerializeHelper
    {
        public const string DEFAULT_NAMESPACE = "sr";

        private static readonly SportRadar.Common.Logs.ILog m_logger = SportRadar.Common.Logs.LogFactory.CreateLog(typeof(IdentityList));

        private static XmlSerializerNamespaces m_Namespaces = null;

        private static XmlSerializerNamespaces Namespaces
        {
            get
            {
                if (m_Namespaces == null)
                {
                    m_Namespaces = new XmlSerializerNamespaces();
                    m_Namespaces.Add(string.Empty, DEFAULT_NAMESPACE);
                }

                return m_Namespaces;
            }
        }

        public static string ObjectToString<T>(object oInstance)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.None;
            settings.Indent = false;


            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlWriter xw = XmlWriter.Create(sw, settings))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));

                        serializer.Serialize(xw, oInstance, Namespaces);
                        return sw.ToString();
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "LineSerializeHelper.ObjectToString({0}) ERROR", oInstance);
            }

            return string.Empty;
        }

        public static T StringToObject<T>(string sSerialized)
        {
            try
            {
                if (string.IsNullOrEmpty(sSerialized))
                     return default(T);
                using (var sr = new StringReader(sSerialized))
                {
                    XmlSerializer s = new XmlSerializer(typeof(T));
                    return (T)s.Deserialize(sr);
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "LineSerializeHelper.StringToObject() ERROR");
            }

            return default(T);
        }
    }
}