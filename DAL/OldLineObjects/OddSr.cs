using System;
using System.ComponentModel;
using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("OddSr")]
    public class OddSr : DatabaseBase
    {
        public const int LIVEBET_STATUS_ACTIVE = 1;
        public const int LIVEBET_STATUS_FORBIDDEN = 2;
        public const int LIVEBET_STATUS_INACTIVE = 3;

        protected BetDomainSr m_betDomainSr = null;

        public OddSr()
        {

        }

        public OddSr(BetDomainSr bds)
        {
            m_betDomainSr = bds;
        }

        public override void FillFromDataRow(DataRow dr)
        {
            this.OddID = DbConvert.ToInt64(dr, "OddID");
            this.SvrOddID = DbConvert.ToInt64(dr, "SvrOddID");
            this.Value = DbConvert.ToDecimal(dr, "Value");
            this.ScanCode = DbConvert.ToString(dr, "ScanCode");
            this.Calculated = DbConvert.ToBool(dr, "Calculated");
            this.Won = DbConvert.ToBool(dr, "Won");
            this.Sort = DbConvert.ToInt32(dr, "Sort");
            this.UserID = DbConvert.ToInt64(dr, "UserID");
            this.MultiStringID = DbConvert.ToInt64(dr, "MultiStringID");
            this.OddTag = DbConvert.ToString(dr, "OddTag");
            this.MultiStringID2 = DbConvert.ToInt64(dr, "MultiStringID2");
            this.BetDomainID = DbConvert.ToInt64(dr, "BetDomainID");
            this.Active = DbConvert.ToBool(dr, "Active");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
            this.IsLiveBet = DbConvert.ToBool(dr, "IsLiveBet");
            this.Status = DbConvert.ToInt32(dr, "Status");
            this.IsLocked = DbConvert.ToBool(dr, "IsLocked");
        }

        public static OddSr CreateFromDataRow(LineBase lb, BetDomainSr bds, DataRow dr)
        {
            OddSr os = new OddSr();//DataCopy.CreateFromDataRow<OddSr>(dr);

            os.m_betDomainSr = bds;

            os.FillFromDataRow(dr);

            return os;
        }

        [XmlElement(ElementName = "m1")]
        public long OddID { get; set; }
        [XmlElement(ElementName = "m2")]
        public decimal? Value { get; set; }
        [XmlElement(ElementName = "m3")]
        public string ScanCode { get; set; }
        [XmlElement(ElementName = "m4")]
        public bool Calculated { get; set; }
        [XmlElement(ElementName = "m5")]
        public bool Won { get; set; }
        [XmlElement(ElementName = "m6")]
        public int? Sort { get; set; }
        [XmlElement(ElementName = "m7")]
        public long UserID { get; set; }
        [XmlElement(ElementName = "m8", IsNullable = true)]
        public long? MultiStringID { get; set; }
        [XmlElement(ElementName = "m9")]
        public string OddTag { get; set; }
        [XmlElement(ElementName = "m10", IsNullable = true)]
        public long? MultiStringID2 { get; set; }
        [XmlElement(ElementName = "m11")]
        public long? BetDomainID { get; set; }
        [XmlElement(ElementName = "m12")]
        public bool? Active { get; set; }
        [XmlElement(ElementName = "m13")]
        public System.DateTime LastModified { get; set; }
        [XmlElement(ElementName = "m14")]
        public bool? IsLiveBet { get; set; }
        [XmlElement(ElementName = "m15")]
        public bool IsLocked { get; set; }
        [XmlElement(ElementName = "m16")]
        public int? Status { get; set; }

        [XmlIgnore]
        public decimal OldValue { get; set; }

        [XmlIgnore]
        public override long ORMID { get { return this.OddID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrOddID; } set { this.SvrOddID = value; } }
        [XmlIgnore]
        public long SvrOddID { get; set; }

        // UI Props & Methods

        public void Save(IDbConnection conn, IDbTransaction transaction, DateTime dtLastModified)
        {
            if (this.IsModified)
            {
                this.LastModified = dtLastModified;
                base.Save(conn, transaction);

                if (this.OddID == 0)
                {
                    this.OddID = ConnectionManager.GetLastInsertId(conn, transaction, "Odd");
                }

                this.IsModified = false;
            }
        }
    }
}