using System;
using System.Collections.Generic;
using System.Data;

namespace SportRadar.DAL.CommonObjects
{
    public class StationAppConfigSr : DatabaseBase
    {
        public override void FillFromDataRow(DataRow dr)
        {
            this.StationAppConfigID = DbConvert.ToInt64(dr, "StationAppConfigID");
            this.PropertyName = DbConvert.ToString(dr, "PropertyName");
            this.ValueString = DbConvert.ToString(dr, "ValueString");
            this.ValueDateTime = DbConvert.ToDateTime(dr, "ValueDateTime");
            this.ValueDecimal = DbConvert.ToDecimal(dr, "ValueDecimal");
            this.ValueInt = DbConvert.ToInt32(dr, "ValueInt");
        }

        private static readonly TableSpecification TableSpec = new TableSpecification("StationAppConfig", true, "StationAppConfigID");
        private DateTime _valueDateTime = DataCopy.MIN_ALLOWED_DATE;
        public override TableSpecification Table { get { return StationAppConfigSr.TableSpec; } }

        public int ValueInt { get; private set; }

        public decimal ValueDecimal { get; private set; }

        [Obsolete("only from SetStationAppConfigValue()")]
        public void SetValue(string value)
        {
            ValueString = value;
        }       
        [Obsolete("only from SetStationAppConfigValue()")]
        public void SetValue(decimal value)
        {
            ValueDecimal = value;
        }       
        [Obsolete("only from SetStationAppConfigValue()")]
        public void SetValue(DateTime value)
        {
            ValueDateTime = value;
        }       
        [Obsolete("only from SetStationAppConfigValue()")]
        public void SetValue(int value)
        {
            ValueInt = value;
        }

        public DateTime ValueDateTime
        {
            get { return _valueDateTime; }
            private set { _valueDateTime = value; }
        }

        public string ValueString { get; private set; }

        public string PropertyName { get; private set; }

        public StationAppConfigSr(string name, int value)
        {
            PropertyName = name;
            ValueInt = value;
        }
        public StationAppConfigSr(string name, decimal value)
        {
            PropertyName = name;
            ValueDecimal = value;
        }
        public StationAppConfigSr(string name, string value)
        {
            PropertyName = name;
            ValueString = value;
        }
        public StationAppConfigSr(string name, DateTime value)
        {
            PropertyName = name;
            ValueDateTime = value;
        }
        public StationAppConfigSr()
        {
        }
        public static StationAppConfigSr CreateFromDataRow(DataRow dr)
        {
            StationAppConfigSr sac = new StationAppConfigSr();

            sac.FillFromDataRow(dr);

            return sac;
        }


        public override DataRow CreateDataRow(DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["StationAppConfigID"] = this.StationAppConfigID;
            dr["PropertyName"] = this.PropertyName;
            dr["ValueString"] = this.ValueString;
            dr["ValueDateTime"] = this.ValueDateTime;
            dr["ValueDecimal"] = this.ValueDecimal;
            dr["ValueInt"] = this.ValueInt;
            return dr;

        }


        public long StationAppConfigID { get; set; }
        public override long ORMID { get { return this.StationAppConfigID; } }


        public static StationAppConfigSr GetValueByName(string propertyName)
        {

            using (DataTable dt = DataCopy.GetDataTable("SELECT * FROM StationAppConfig WHERE propertyname = '{0}'",propertyName))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    StationAppConfigSr sac = StationAppConfigSr.CreateFromDataRow(dr);

                    return sac;
                }
            }

            return null;
        }

        public override void Insert(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction)
        {
            base.Insert(conn, transaction);
            this.StationAppConfigID = (long)m_objJustInsertedIdentity;
        }

        public static StationAppConfigSr LoadStationAppConfigByQuery(string sQuery)
        {
            using (DataTable dt = DataCopy.GetDataTable(sQuery))
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    StationAppConfigSr sac = StationAppConfigSr.CreateFromDataRow(dt.Rows[0]);

                    return sac;
                }
            }

            return null;
        }

        public static List<StationAppConfigSr> GetAllSettings()
        {
            List<StationAppConfigSr> lResult = new List<StationAppConfigSr>();

            using (DataTable dt = DataCopy.GetDataTable("SELECT * FROM StationAppConfig ORDER BY StationAppConfigID"))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    StationAppConfigSr sac = StationAppConfigSr.CreateFromDataRow(dr);

                    lResult.Add(sac);
                }
            }

            return lResult;
        }
    }
}
