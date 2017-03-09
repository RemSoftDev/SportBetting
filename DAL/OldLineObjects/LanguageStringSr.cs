using System;
using System.Data;
using System.Diagnostics;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;
using SportRadar.Common.Windows;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("LanguageStringSr")]
    public class LanguageStringSr : DatabaseBase
    {
        public override void FillFromDataRow(DataRow dr)
        {
            this.LanguageStringID = DbConvert.ToInt64(dr, "LanguageStringId");
            this.Text = DbConvert.ToString(dr, "Text");
            this.MultiStringID = DbConvert.ToInt64(dr, "MultiStringID");
            this.LanguageID = DbConvert.ToInt64(dr, "LanguageID");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
            this.SvrLanguageStringID = DbConvert.ToInt64(dr, "SvrLanguageStringID");
            this.IsLiveBet = DbConvert.ToBool(dr, "IsLiveBet");
        }

        public static LanguageStringSr CreateFromDataRow(LineBase lb, string sMultiStringTag, DataRow dr)
        {
            LanguageStringSr lss = new LanguageStringSr();

            lss.MultiStringTag = sMultiStringTag.ToLowerInvariant();

            lss.FillFromDataRow(dr);

            return lss;
        }

        //protected static TableSpec m_Table = new TableSpec("LanguageString", "LanguageStringID", "SvrLanguageStringID");
        //public override TableSpec Table { get { return m_Table; } }

        [XmlElement(ElementName = "m1")]
        public long LanguageStringID { get; set; }
        [XmlElement(ElementName = "m2")]
        public string Text { get; set; }
        [XmlElement(ElementName = "m3")]
        public long MultiStringID { get; set; }
        [XmlElement(ElementName = "m4")]
        public long LanguageID { get; set; }
        [XmlElement(ElementName = "m5")]
        public System.DateTime LastModified { get; set; }

        [XmlIgnore]
        public long SvrLanguageStringID { get; set; }
        [XmlIgnore]
        public string MultiStringTag { get; set; }
        [XmlIgnore]
        public bool IsLiveBet { get; set; }
        [XmlIgnore]
        public override long ORMID { get { return this.LanguageStringID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrLanguageStringID; } set { this.SvrLanguageStringID = value; } }

        public override string ToString()
        {
            return string.Format("LanguageStringSr {{LanguageStringId={0}, MultiStringId={1}, MultiStringTag={2}, LanguageId={3}, Text={4}}}", this.LanguageStringID, this.MultiStringID, this.MultiStringTag, this.Text);
        }
    }
}