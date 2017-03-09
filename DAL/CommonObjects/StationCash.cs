using System;
using System.Collections.Generic;
using System.Data;

namespace SportRadar.DAL.CommonObjects
{
    public class StationCashSr : ObjectBase
    {
        public override void FillFromDataRow(DataRow dr)
        {
            this.StationCashID = DbConvert.ToInt64(dr,"StationCashID");
            this.Cash = DbConvert.ToDecimal(dr, "Cash");
            this.MoneyIn = DbConvert.ToBool(dr, "MoneyIn");
            this.OperationType = DbConvert.ToString(dr, "OperationType");
            this.OperatorID = DbConvert.ToString(dr, "OperatorID");
            this.CashCheckPoint = DbConvert.ToBool(dr, "CashCheckPoint");
            this.DateModified = DbConvert.ToDateTime(dr, "DateModified");        
        }

        public static StationCashSr CreateFromDataRow(DataRow dr)
        {
            StationCashSr scs = new StationCashSr();

            scs.FillFromDataRow(dr);

            return scs;
        }
        private static readonly TableSpecification TableSpec = new TableSpecification("StationCash", true, "StationCashID");
        public override TableSpecification Table { get { return TableSpec; } }

        public static List<StationCashSr> GetStationCashListByQuery(string sQuery, List<IDbDataParameter> lParams)
        {
            List<StationCashSr> lResult = new List<StationCashSr>();

            using (DataTable dt = DataCopy.GetDataTableWithSqlParams(sQuery, lParams))
            {
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        StationCashSr tqs = StationCashSr.CreateFromDataRow(dr);

                        lResult.Add(tqs);
                    }
            }

            return lResult;
        }

        public long StationCashID { get; set; }
        public override long ORMID { get { return this.StationCashID; } }
        public decimal Cash { get; set; }
        public bool MoneyIn { get; set; }
        public string OperationType { get; set; }
        public string OperatorID { get; set; }
        public bool CashCheckPoint { get; set; }
        public DateTime DateModified { get; set; }
    }
}
