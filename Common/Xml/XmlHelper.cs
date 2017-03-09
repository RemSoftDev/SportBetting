using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;

namespace SportRadar.Common.Xml
{
    public class AttributePairs : Dictionary<string, string>
    {

    }

    public static class XmlHelper
    {
        private static readonly ILog _logger = LogFactory.CreateLog(typeof(XmlHelper));

        private static XmlElement CreateElement(XmlDocument docParent, XmlElement elParent, string sElementName, AttributePairs ap = null, string sValue = null)
        {
            XmlElement el = docParent.CreateElement(sElementName);

            if (ap != null)
            {
                foreach (KeyValuePair<string, string> kvpAttr in ap)
                {
                    el.SetAttribute(kvpAttr.Key, kvpAttr.Value);
                }
            }

            if (sValue != null)
            {
                el.InnerText = sValue;
            }

            if (elParent != null)
            {
                elParent.AppendChild(el);
            }

            return el;
        }

        public static string GetElementInnerText(XmlNode nodeSource, string sXPath)
        {
            XmlNode node = nodeSource.SelectSingleNode(sXPath);
            ExcpHelper.ThrowIf(node == null, "SelectSingleNode('{0}') ERROR. Node does not exist.", sXPath);

            return node.InnerText;
        }

        public static string GetElementInnerTextSafely(XmlNode nodeSource, string sXPath, string sDefault)
        {
            XmlNode node = nodeSource.SelectSingleNode(sXPath);
            return node != null ? node.InnerText : sDefault;
        }

        public static List<string> GetElementsInnerTextSafely(XmlNode nodeSource, string sXPath, string sDefault)
        {
            List<string> lStrings = new List<string>();

            XmlNodeList xnl = nodeSource.SelectNodes(sXPath);

            foreach (XmlNode node in xnl)
            {
                lStrings.Add(node.InnerText);
            }

            return lStrings;
        }

        public static long GetElementInnerLong(XmlNode nodeSource, string sXPath)
        {
            return Convert.ToInt64(GetElementInnerText(nodeSource, sXPath));
        }

        public static decimal GetElementInnerDecimal(XmlNode nodeSource, string sXPath)
        {
            return Convert.ToDecimal(GetElementInnerText(nodeSource, sXPath));
        }

        public static decimal GetElementInnerDecimalSafely(XmlNode nodeSource, string sXPath, decimal dcDefault)
        {
            try
            {
                string sElement = GetElementInnerText(nodeSource, sXPath);
                return Convert.ToDecimal(sElement);
            }
            catch
            {
            }

            return dcDefault;
        }

        public static DateTime GetElementInnerDateTimeSafely(XmlNode nodeSource, string sXPath, DateTime dtDefault)
        {
            try
            {
                string sElement = GetElementInnerText(nodeSource, sXPath);
                return Convert.ToDateTime(sElement);
            }
            catch
            {
            }

            return dtDefault;
        }
    }
}
