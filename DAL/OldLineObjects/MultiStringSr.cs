using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("MultiStringSr")]
    public class MultiStringSr : DatabaseBase
    {
        public override void FillFromDataRow(DataRow dr)
        {
            this.MultiStringID = DbConvert.ToInt64(dr, "MultiStringID");
            this.SvrMultiStringID = DbConvert.ToInt64(dr, "SvrMultiStringID");
            this.MultiStringTag = DbConvert.ToString(dr, "MultiStringTag");
            this.Comment = DbConvert.ToString(dr, "Comment");
            this.MultiStringGroupID = DbConvert.ToNullableInt64(dr, "MultiStringGroupID");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
        }

        public static MultiStringSr CreateFromDataRow(DataRow dr)
        {
            MultiStringSr ms = new MultiStringSr();

            ms.FillFromDataRow(dr);

            return ms;
        }

        public static MultiStringSr GetByMultiStringTag(IDbConnection connection, IDbTransaction transaction, string sMultiStringTag)
        {
            using (DataTable dt = DataCopy.GetDataTable(connection, transaction, "SELECT * FROM MultiString WHERE MultiStringTag = '{0}'", sMultiStringTag))
            {
                return dt != null && dt.Rows != null && dt.Rows.Count > 0 ? CreateFromDataRow(dt.Rows[0]) : null;
            }
        }

        //protected static TableSpec m_Table = new TableSpec("MultiString", "MultiStringID", "SvrMultiStringID");
        //[XmlIgnore]
        //public override TableSpec Table { get { return m_Table; } }

        [XmlElement(ElementName = "m1")]
        public long MultiStringID { get; set; }
        [XmlElement(ElementName = "m2")]
        public string MultiStringTag { get; set; }
        [XmlElement(ElementName = "m3")]
        public string Comment { get; set; }
        [XmlElement(ElementName = "m4")]
        public long? MultiStringGroupID { get; set; }
        [XmlElement(ElementName = "m5")]
        public System.DateTime LastModified { get; set; }

        [XmlIgnore]
        public override long ORMID { get { return this.MultiStringID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrMultiStringID; } set { this.SvrMultiStringID = value; } }
        [XmlIgnore]
        public long SvrMultiStringID { get; set; }
    }
}