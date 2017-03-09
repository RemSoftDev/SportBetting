using System;
using System.Data;
using System.Diagnostics;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;
using SportRadar.Common.Windows;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("CompetitorSr")]
    public class CompetitorSr : DatabaseBase
    {
        public const int HOME_COMPETITOR = 1;
        public const int AWAY_COMPETITOR = 2;

        public override void FillFromDataRow(DataRow dr)
        {
            this.CompetitorID = DbConvert.ToInt64(dr, "CompetitorID");
            this.SvrCompetitorID = DbConvert.ToInt64(dr, "SvrCompetitorID");
            this.BtrCompetitorID = DbConvert.ToNullableInt64(dr, "BtrCompetitorID");
            this.IsLiveBet = DbConvert.ToBool(dr, "IsLiveBet");
            this.DefaultName = DbConvert.ToString(dr, "DefaultName");
            this.SportID = DbConvert.ToNullableInt64(dr, "SportID");
            this.MultiStringID = DbConvert.ToInt64(dr, "MultiStringID");
            this.CountryID = DbConvert.ToNullableInt64(dr, "CountryID");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");

            try
            {
                this.BtrCompetitorID = DbConvert.ToInt64(dr, "BtrCompetitorID");
            }
            catch
            {
            }
        }

        public static CompetitorSr CreateFromDataRow(LineBase lb, DataRow dr)
        {
            CompetitorSr csr = new CompetitorSr();

            csr.FillFromDataRow(dr);

            return csr;
        }

        [XmlElement(ElementName = "m1")]
        public long CompetitorID { get; set; }
        [XmlElement(ElementName = "m2", IsNullable = true)]
        public long? BtrCompetitorID { get; set; }
        [XmlElement(ElementName = "m3")]
        public bool IsLiveBet { get; set; }
        [XmlElement(ElementName = "m4")]
        public string DefaultName { get; set; }
        [XmlElement(ElementName = "m5", IsNullable = true)]
        public long? SportID { get; set; }
        [XmlElement(ElementName = "m6", IsNullable = true)]
        public long? MultiStringID { get; set; }
        [XmlElement(ElementName = "m7", IsNullable = true)]
        public long? CountryID { get; set; }
        [XmlElement(ElementName = "m8")]
        public System.DateTime LastModified { get; set; }

        [XmlIgnore]
        public override long ORMID { get { return this.CompetitorID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrCompetitorID; } set { this.SvrCompetitorID = value; } }
        [XmlIgnore]
        public long SvrCompetitorID { get; set; }
        [XmlIgnore]
        public long BtrLiveBetCompetitorID { get; set; }

        public override void Save(IDbConnection conn, IDbTransaction transaction)
        {
            ExcpHelper.ThrowIf(this.BtrCompetitorID == 0, "BtrCompetitorID is Invalid");

            if (this.IsModified)
            {
                base.Save(conn, transaction);

                if (this.CompetitorID == 0)
                {
                    object objCompetitorId = DataCopy.ExecuteScalar(conn, transaction, "SELECT CompetitorId FROM Competitor WHERE BtrCompetitorID = {0}", this.BtrCompetitorID);
                    this.CompetitorID = Convert.ToInt64(objCompetitorId);
                    Debug.Assert(this.CompetitorID > 0);
                }

                this.IsModified = false;
            }
        }

        // UI Props & Methods
        public override string ToString()
        {
            return string.Format("CompetitorSr {{ORMID = {0}, Name = '{1}'}}", this.CompetitorID, this.DefaultName);
        }
    }
}