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
using SportRadar.DAL.NewLineObjects;
using System.Xml.Serialization;
using System.IO;
using SportRadar.Common.Collections;

namespace SportRadar.DAL.NewLineObjects
{
    public class CompetitorInfosLn : DatabaseBase, ILineObjectWithId<CompetitorInfosLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("competitorinfos", false, "competitorinfoid");

        public CompetitorInfosLn()
        {
            external_state = new CompetitorInfosValuesExternalState();
        }

        public long CompetitorInfoId { get; set; }
        public long SuperBtrId { get; set; }
        public string TshirtHome { get; set; }
        public string TshirtAway { get; set; }

        public CompetitorInfosValuesExternalState external_state { get; set; }

        public ObservableProperty<string> ExternalState { get; set; }
        
        //public string StatisticData { get; set; } //better to save statistics in db as string?
        public DateTime? LastModified { get; set; }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.SuperBtrId = DbConvert.ToInt64(dr, "SuperBtrId");
            this.CompetitorInfoId = DbConvert.ToInt64(dr, "CompetitorInfoId");

            this.LastModified = DbConvert.ToDateTime(dr, "LastModified");
            this.TshirtAway = DbConvert.ToString(dr, "TshirtAway");
            this.TshirtHome = DbConvert.ToString(dr, "TshirtHome");

            CompetitorInfosValuesExternalState obj = LineSerializeHelper.StringToObject<CompetitorInfosValuesExternalState>(DbConvert.ToString(dr, "external_state"));
            this.external_state.StatisticValues = obj.StatisticValues;
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["SuperBtrId"] = this.SuperBtrId;
            dr["tshirthome"] = this.TshirtHome;
            dr["tshirtaway"] = this.TshirtAway;
            dr["LastModified"] = this.LastModified;
            dr["CompetitorInfoId"] = this.CompetitorInfoId;

            string sExternalState = LineSerializeHelper.ObjectToString<CompetitorInfosValuesExternalState>(this.external_state);
            dr["external_state"] = sExternalState;

            return dr;
        }

        public long Id
        {
            get { return this.SuperBtrId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.CompetitorInfos.ContainsKey(this.SuperBtrId); }
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

        public void MergeFrom(CompetitorInfosLn objSource)
        {
            Debug.Assert(this.SuperBtrId == objSource.SuperBtrId);

            this.TshirtAway = objSource.TshirtAway;
            this.TshirtHome = objSource.TshirtHome;
            this.external_state.StatisticValues = objSource.external_state.StatisticValues;
            this.LastModified= objSource.LastModified;

            SetRelations();
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new NotImplementedException();
        }
    }

    [XmlRoot("matchinfo", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = false)]
    public class CompetitorInfosValuesExternalState
    {
        [XmlArray("m2")]
        [XmlArrayItem("f")]
        public SyncList<StatisticValueSr> StatisticValues { get; set; }
    }

    public class CompetitorInfosDictionary : LineObjectDictionaryByIdBase<CompetitorInfosLn>
    {
    }
}
