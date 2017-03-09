using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SportRadar.Common.Logs;

namespace SportRadar.DAL.CommonObjects
{
    public sealed class DateTimeSr : IXmlSerializable
    {

        private static ILog Log = LogFactory.CreateLog(typeof(DateTimeSr));

        public DateTimeSr(DateTime dt)
        {
            try
            {
                if (dt == DateTime.MaxValue)
                    this.DateTimeOffset = DateTimeOffset.MaxValue;
                this.DateTimeOffset = new DateTimeOffset(dt);

                this.LocalDateTime = DateTimeOffset.ToLocalTime().DateTime;
                this.UtcDateTime = DateTimeOffset.ToUniversalTime().DateTime;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }

        }

        public DateTimeSr()
            : this(DateTime.Now)
        {
            try
            {
                this.LocalDateTime = DateTimeOffset.ToLocalTime().DateTime;
                this.UtcDateTime = DateTimeOffset.ToUniversalTime().DateTime;

            }
            catch (Exception e)
            {
                Log.Error(DateTimeOffset.ToString(),e);
                Log.Error(e.Message, e);

            }

        }

        [XmlIgnore]
        public DateTimeOffset DateTimeOffset { get; private set; }

        [XmlIgnore]
        public DateTime LocalDateTime { get; private set; }

        [XmlIgnore]
        public DateTime UtcDateTime { get; private set; }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //Debug.Assert(reader.NodeType == XmlNodeType.Element);
            //Debug.Assert(!reader.IsEmptyElement);

            reader.Read();
            //Debug.Assert(reader.NodeType == XmlNodeType.Text);
            //Debug.Assert(!string.IsNullOrEmpty(reader.Value));
            if (!string.IsNullOrEmpty(reader.Value))
            {
                this.FromXmlString(reader.Value);
                reader.Read();
                reader.Read();
            }

            //Debug.Assert(reader.NodeType == XmlNodeType.EndElement);

        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(this.ToXmlString());
        }

        public override int GetHashCode()
        {
            return this.DateTimeOffset.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            DateTimeSr dtsr = obj as DateTimeSr;

            return dtsr != null ? this.DateTimeOffset.Equals(dtsr.DateTimeOffset) : false;
        }

        public void FromXmlString(string sXml)
        {
            this.DateTimeOffset = XmlConvert.ToDateTimeOffset(sXml);
            this.LocalDateTime = DateTimeOffset.ToLocalTime().DateTime;
            this.UtcDateTime = DateTimeOffset.ToUniversalTime().DateTime;
        }

        public static DateTimeSr FromString(string sXml)
        {
            DateTimeSr dt = new DateTimeSr();
            dt.FromXmlString(sXml);

            return dt;
        }

        public string ToXmlString()
        {
            return XmlConvert.ToString(this.DateTimeOffset);
        }

        public override string ToString()
        {
            return this.DateTimeOffset.ToString();
        }
    }
}
