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
    public class MatchInfosLn : DatabaseBase, ILineObjectWithId<MatchInfosLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("matchinfos", false, "matchinfoid");
                
        public long MatchInfoId { get; set; }
        public DateTime? LastModified { get; set; }
        public MatchInfosValuesExternalState external_state { get; set; }

        public MatchInfosLn()
        {
            external_state = new MatchInfosValuesExternalState();
        }

        public void MergeFrom(MatchInfosLn objSource)
        {
            Debug.Assert(this.MatchInfoId == objSource.MatchInfoId);

            this.external_state.StatisticValues = objSource.external_state.StatisticValues;
            this.LastModified = objSource.LastModified;

            SetRelations();
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new NotImplementedException();
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.MatchInfoId = DbConvert.ToInt64(dr, "matchinfoid");
            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");

            MatchInfosValuesExternalState obj = LineSerializeHelper.StringToObject<MatchInfosValuesExternalState>(DbConvert.ToString(dr, "external_state"));
            this.external_state.StatisticValues = obj.StatisticValues;
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["matchinfoid"] = this.MatchInfoId;
            dr["LastModified"] = this.LastModified.Value;

            string sExternalState = LineSerializeHelper.ObjectToString<MatchInfosValuesExternalState>(this.external_state);
            dr["external_state"] = sExternalState;

            return dr;
        }

        public long Id
        {
            get { return this.MatchInfoId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.MatchInfos.ContainsKey(this.MatchInfoId); }
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

        //getters
        public long HomeTeamSuperId
        {
            get 
            {
                string value = this.external_state.StatisticValues.Where(x => x.Name == "COMPETITOR_1_BTR_SUPER_ID").Select(x => x.Value).FirstOrDefault();

                return (value == "") ? 0 : Int64.Parse(this.external_state.StatisticValues.Where(x => x.Name == "COMPETITOR_1_BTR_SUPER_ID").Select(x => x.Value).FirstOrDefault());
            }
        }

        public long AwayTeamSuperId
        {
            get
            {
                string value = this.external_state.StatisticValues.Where(x => x.Name == "COMPETITOR_2_BTR_SUPER_ID").Select(x => x.Value).FirstOrDefault();

                return (value == "") ? 0 : Int64.Parse(this.external_state.StatisticValues.Where(x => x.Name == "COMPETITOR_2_BTR_SUPER_ID").Select(x => x.Value).FirstOrDefault());
            }
        }
    }

    [XmlRoot("m2", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = false)]
    public class MatchInfosValuesExternalState
    {
        [XmlArrayItem("f")]
        public SyncList<StatisticValueSr> StatisticValues { get; set; }
    }

    public class MatchInfosDictionary : LineObjectDictionaryByIdBase<MatchInfosLn>
    {
    }
}
