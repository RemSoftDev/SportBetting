using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.DAL.OldLineObjects;
using System.Xml.Serialization;
using System.IO;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using System.Data;
using System.Diagnostics;

namespace SportRadar.DAL.NewLineObjects
{
    public class ActiveTournamentLn : DatabaseBase, ILineObjectWithKey<ActiveTournamentLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("activetournaments", false, "Id");
        public string Id;
        public bool Active { get; set; }
        public decimal OddIncreaseDecrease { get; set; }
        public string Markets { get; set; }

        public ActiveTournamentLn()
        {
        }

        public string KeyName
        {
            get { return this.Id; }
        }

        public void NotifyPropertiesChanged()
        {
            throw new NotImplementedException();
        }

        public void UnsetPropertiesChanged()
        {
            throw new NotImplementedException();
        }

        public override void SetRelations()
        {
            base.SetRelations();
        }

        public ISerializableObject Serialize()
        {
            throw new NotImplementedException();
        }

        public void Deserialize(ISerializableObject so)
        {
            throw new NotImplementedException();
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.ActiveTournaments.ContainsKey(this.Id); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["Id"] = this.Id;
            dr["Active"] = this.Active;
            dr["OddIncreaseDecrease"] = this.OddIncreaseDecrease;
            dr["Markets"] = this.Markets;

            return dr;
        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.Id = DbConvert.ToString(dr, "Id");
            this.Active = DbConvert.ToBool(dr, "Active");
            this.OddIncreaseDecrease = DbConvert.ToDecimal(dr, "OddIncreaseDecrease");
            this.Markets = DbConvert.ToString(dr, "Markets");
        }

        public void MergeFrom(ActiveTournamentLn objSource)
        {
            Debug.Assert(this.Id == objSource.Id);

            this.Id = objSource.Id;
            this.Active = objSource.Active;
            this.OddIncreaseDecrease = objSource.OddIncreaseDecrease;
            this.Markets = objSource.Markets;

            SetRelations();
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new NotImplementedException();
        }
    }

    public class ActiveTournamentsDictionary : LineObjectDictionaryByKeyBase<ActiveTournamentLn>
    {
    }
}
