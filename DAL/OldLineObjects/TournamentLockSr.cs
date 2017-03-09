using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.OldLineObjects
{
    [XmlType("TournamentLockSr")]
    public class TournamentLockSr : DatabaseBase
    {
        public override void FillFromDataRow(DataRow dr)
        {
            this.TournamentLockID = DbConvert.ToInt64(dr, "TournamentLockID");
            this.TournamentID = DbConvert.ToInt64(dr, "TournamentID");
            this.ToLockID = DbConvert.ToInt32(dr, "ToLockID");
        }

        public static TournamentLockSr CreateFromDataRow(LineBase lb, DataRow dr)
        {
            TournamentLockSr tls = new TournamentLockSr();

            tls.FillFromDataRow(dr);

            return tls;
        }

        public static List<TournamentLockSr> GetTournamentLockItemsByTournamentId(long lTournamentId)
        {
            List<TournamentLockSr> lItems = new List<TournamentLockSr>();

            using (DataTable dt = DataCopy.GetDataTable("SELECT * FROM TournamentLock WHERE TournamentID = {0}", lTournamentId))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    lItems.Add(CreateFromDataRow(null, dr));
                }
            }

            return lItems;
        }

        //protected static readonly TableSpec m_Table = new TableSpec("TournamentLock", "TournamentLockID", "Undefined");
        //[XmlIgnore]
        //public override TableSpec Table{get{return m_Table;}}

        [XmlElement(ElementName = "m1")]
        public long TournamentLockID { get; set; }
        [XmlElement(ElementName = "m2")]
        public long TournamentID { get; set; }
        [XmlElement(ElementName = "m3")]
        public long ToLockID { get; set; }

        [XmlIgnore]
        public long SvrTournamentLockID { get; set; }
        [XmlIgnore]
        public override long ORMID { get { return this.TournamentLockID; } }
        [XmlIgnore]
        public override long SvrID { get { return this.SvrTournamentLockID; } set { this.SvrTournamentLockID = value; } }
    }
}