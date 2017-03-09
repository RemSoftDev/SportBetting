using System;
using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("LanguageSr")]
    public class LanguageSr : DatabaseBase
    {
        public override void FillFromDataRow(DataRow dr)
        {
            this.LanguageID = DbConvert.ToInt64(dr, "LanguageID");
            this.ShortName = DbConvert.ToString(dr, "ShortName");
            this.MultiStringID = DbConvert.ToInt64(dr, "MultiStringID");
            this.Priority = DbConvert.ToNullableInt32(dr, "Priority");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
            this.SvrLanguageId = DbConvert.ToInt64(dr, "SvrLanguageId");
            this.IsTerminalLanguage = DbConvert.ToInt32(dr, "IsTerminalLanguage");
        }

        public LanguageSr Clone()
        {
            LanguageSr ls = new LanguageSr();

            ls.LanguageID = this.LanguageID;
            ls.ShortName = this.ShortName;
            ls.MultiStringID = this.MultiStringID;
            ls.Priority = this.Priority;
            ls.LastModified = this.LastModified;
            ls.SvrLanguageId = this.SvrLanguageId;
            ls.IsTerminalLanguage = this.IsTerminalLanguage;

            return ls;
        }

        [XmlElement(ElementName = "m1")]
        public long LanguageID { get; set; }
        [XmlElement(ElementName = "m2")]
        public string ShortName { get; set; }
        [XmlElement(ElementName = "m3")]
        public long MultiStringID { get; set; }
        [XmlElement(ElementName = "m4")]
        public int? Priority { get; set; }
        [XmlElement(ElementName = "m5")]
        public System.DateTime LastModified { get; set; }
        [XmlElement(ElementName = "m6")]
        public int IsTerminalLanguage { get; set; }

        [XmlIgnore]
        public override long ORMID { get { return this.LanguageID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrLanguageId; } set { this.SvrLanguageId = value; } }
        [XmlIgnore]
        public long SvrLanguageId { get; set; }
    }
}