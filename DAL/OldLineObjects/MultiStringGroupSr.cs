using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("MultiStringGroupSr")]
    public class MultiStringGroupSr : DatabaseBase
    {
        public const string TERMINAL_MULTISTRING_GROUP          = "TERMINAL_MSG"; // Terminal
        public const string OUTLET_MULTISTRING_GROUP            = "SHOP_MSG"; // Shop
        public const string TIP_TEXT_SHORT_MULTISTRING_GROUP    = "TipTextShort_MSG"; // Odd
        public const string TIP_TEXT_MULTISTRING_GROUP          = "TipText_MSG"; // Odd
        public const string BETDOMAIN_MULTISTRING_GROUP         = "BETDOMAIN_MSG"; // BetDomain

        public override void FillFromDataRow(DataRow dr)
        {
            this.MultiStringGroupID = DbConvert.ToInt64(dr, "MultiStringGroupID");
            this.MultiStringGroupTag = DbConvert.ToString(dr, "MultiStringGroupTag");
            this.Comment = DbConvert.ToString(dr, "Comment");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
            this.SvrMultiStringGroupID = DbConvert.ToInt64(dr, "SvrMultiStringGroupID");
        }

        //protected static TableSpec m_Table = new TableSpec("MultiStringGroup", "MultiStringGroupID", "SvrMultiStringGroupID");
        //public override TableSpec Table { get { return m_Table; } }

        [XmlElement(ElementName = "m1")]
        public long MultiStringGroupID { get; set; }
        [XmlElement(ElementName = "m2")]
        public string MultiStringGroupTag { get; set; }
        [XmlElement(ElementName = "m3")]
        public string Comment { get; set; }
        [XmlElement(ElementName = "m4")]
        public System.DateTime LastModified { get; set; }

        [XmlIgnore]
        public override long ORMID { get { return this.MultiStringGroupID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrMultiStringGroupID; } set { this.SvrMultiStringGroupID = value; } }
        [XmlIgnore]
        public long SvrMultiStringGroupID { get; set; }
    }
}