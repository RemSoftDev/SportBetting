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
    public class LiabilityLn : DatabaseBase, ILineObjectWithKey<LiabilityLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("conffactor", false, "CFKey");
        public string CFKey;
        public decimal factor { get; set; }
        public decimal livefactor { get; set; }

        public LiabilityLn()
        {
        }

        public string KeyName
        {
            get { return this.CFKey; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.Liabilities.ContainsKey(this.CFKey); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["CFKey"] = this.CFKey;
            dr["factor"] = this.factor;
            dr["livefactor"] = this.livefactor;

            return dr;
        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.CFKey = DbConvert.ToString(dr, "CFKey");
            this.factor = DbConvert.ToDecimal(dr, "factor");
            this.livefactor = DbConvert.ToDecimal(dr, "livefactor");
        }

        public void MergeFrom(LiabilityLn objSource)
        {
            Debug.Assert(this.CFKey == objSource.CFKey);

            this.CFKey = objSource.CFKey;
            this.factor = objSource.factor;
            this.livefactor = objSource.livefactor;

            SetRelations();
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new NotImplementedException();
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

    public class LiabilitiesDictionary : LineObjectDictionaryByKeyBase<LiabilityLn>
    {
    }
}
