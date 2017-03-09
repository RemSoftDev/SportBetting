using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("CategorySr")]
    public class CategorySr : ObjectBase
    {
        public override void FillFromDataRow(DataRow dr)
        {
            this.CategoryID = DbConvert.ToInt64(dr, "CategoryID");
            this.SvrCategoryID = DbConvert.ToInt64(dr, "SvrCategoryID");
            this.MultiStringID = DbConvert.ToInt64(dr, "MultiStringID");
            this.DefaultName = DbConvert.ToString(dr, "DefaultName");
            this.Sort = DbConvert.ToInt32(dr, "Sort");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
        }

        public static CategorySr CreateFromDataRow(LineBase lb, DataRow dr)
        {
            CategorySr cs = new CategorySr();

            cs.FillFromDataRow(dr);

            return cs;
        }

        [XmlElement(ElementName = "m1")]
        public long CategoryID { get; set; }
        [XmlElement(ElementName = "m2")]
        public long MultiStringID { get; set; }
        [XmlElement(ElementName = "m3")]
        public string DefaultName { get; set; }
        [XmlElement(ElementName = "m4")]
        public int Sort { get; set; }
        [XmlElement(ElementName = "m5")]
        public System.DateTime LastModified { get; set; }

        [XmlIgnore]
        public override long ORMID { get { return this.CategoryID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrCategoryID; } set { this.SvrCategoryID = value; } }
        [XmlIgnore]
        public long SvrCategoryID { get; set; }
    }
}