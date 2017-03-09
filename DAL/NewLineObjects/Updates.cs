using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public class UpdatesLn : ObjectBase
    {
        public readonly static TableSpecification TableSpec = new TableSpecification("Updates", true, "UpdateId");
        public override TableSpecification Table { get { return UpdatesLn.TableSpec; } }

        public new long UpdateId { get; protected set; }
        public DateTime Modified { get; protected set; }
        public eUpdateType UpdateType { get; protected set; }
        public string Description { get; protected set; }
        public string ProviderDescription { get; protected set; }
        //public string ProviderTag { get; protected set; }

        public UpdatesLn(eUpdateType UpdateType, string sDescription, string sProviderDescription)
        {
            this.Modified = DateTime.Now;
            this.Description = sDescription;
            this.ProviderDescription = sProviderDescription;
            //this.ProviderTag = sProviderTag;
        }

        public override void FillFromDataRow(DataRow dr)
        {
            this.UpdateId = DbConvert.ToInt64(dr, "UpdateId");
            this.Modified = DbConvert.ToDateTime(dr, "Modified");

            string sUpdateType = DbConvert.ToString(dr, "UpdateType");
            this.UpdateType = (eUpdateType) Enum.Parse(typeof (eUpdateType), sUpdateType, true);

            this.Description = DbConvert.ToString(dr, "Description");
            this.ProviderDescription = DbConvert.ToString(dr, "ProviderDescription");
            //this.ProviderTag = DbConvert.ToString(dr, "ProviderTag");
        }

        public override DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["UpdateId"] = this.UpdateId;
            dr["Modified"] = this.Modified;
            dr["UpdateType"] = this.UpdateType.ToString();
            dr["Description"] = this.Description;
            dr["ProviderDescription"] = this.ProviderDescription;
            dr["ProviderTag"] = string.Empty;// this.ProviderTag;

            return dr;
        }

        public override long ORMID { get { return this.UpdateId; } }

        public override void Insert(System.Data.IDbConnection conn, System.Data.IDbTransaction transaction)
        {
 	        base.Insert(conn, transaction);
            this.UpdateId = (long) m_objJustInsertedIdentity;
        }
    }
}
