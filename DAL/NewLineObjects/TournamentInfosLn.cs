using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using System.Diagnostics;
using System.Data;
using SportRadar.DAL.OldLineObjects;
using System.Xml.Serialization;
using System.IO;
using SportRadar.Common.Collections;

namespace SportRadar.DAL.NewLineObjects
{
    public class TournamentInfosLn : DatabaseBase, ILineObjectWithId<TournamentInfosLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("tournamentinfos", false, "tournamentinfoid");

        public long TournamentInfoId { get; set; }
        public DateTime? LastModified { get; set; }

        public TournamentInfosValuesExternalState external_state { get; set; }

        public TournamentInfosLn()
        {
            external_state = new TournamentInfosValuesExternalState();
        }

        public void MergeFrom(TournamentInfosLn objSource)
        {
            Debug.Assert(this.TournamentInfoId == objSource.TournamentInfoId);

            this.external_state.CompetitorsContainer = objSource.external_state.CompetitorsContainer;
            this.LastModified = objSource.LastModified;

            SetRelations();
        }

        protected CompetitorInfoSr[] CompetitorInfos { get; set; }

        public void MergeFrom(ISerializableObject so)
        {
            throw new NotImplementedException();
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.TournamentInfoId = DbConvert.ToInt64(dr, "tournamentinfoid");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");

            TournamentInfosValuesExternalState obj = LineSerializeHelper.StringToObject<TournamentInfosValuesExternalState>(DbConvert.ToString(dr, "external_state"));
            this.external_state.CompetitorsContainer = obj.CompetitorsContainer;
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["tournamentinfoid"] = this.TournamentInfoId;
            dr["LastModified"] = this.LastModified;

            string sExternalState = LineSerializeHelper.ObjectToString<TournamentInfosValuesExternalState>(this.external_state);
            dr["external_state"] = sExternalState;

            return dr;
        }

        public long Id
        {
            get { return this.TournamentInfoId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.TournamentInfos.ContainsKey(this.TournamentInfoId); }
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
    }

    [XmlRoot("m2", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = false)]
    public class TournamentInfosValuesExternalState
    {
        public CompetitorInfoSr[] CompetitorsContainer { get; set; }
    }

    public class TournamentInfosDictionary : LineObjectDictionaryByIdBase<TournamentInfosLn>
    {
    }
}
