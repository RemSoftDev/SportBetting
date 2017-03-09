using SportRadar.DAL.CommonObjects;
using System.Diagnostics;
using System.Data;

namespace SportRadar.DAL.NewLineObjects
{
    public class MultistringGroupLn : DatabaseBase, ILineObjectWithId<MultistringGroupLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("MultistringGroup", false, "MultiStringGroupID");

        public long MultiStringGroupId { get; set; }
        public string MultiStringGroupTag { get; set; }
        public MultistringGroupLn(long multiStringGroupId, string multiStringGroupTag)
        {
            this.MultiStringGroupId = multiStringGroupId;
            this.MultiStringGroupTag = multiStringGroupTag;
        }

        public MultistringGroupLn()
        {
        }

        public long Id
        {
            get { return this.MultiStringGroupId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.MultistringGroups.ContainsKey(this.MultiStringGroupId); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override string ToString()
        {
            return string.Format("LanguageLn {{MultiStringGroupID={0}, MultiStringGroupTag='{1}'}}", this.MultiStringGroupId, this.MultiStringGroupTag);
        }

        public void MergeFrom(MultistringGroupLn objSource)
        {
            Debug.Assert(this.MultiStringGroupId == objSource.MultiStringGroupId);

            this.MultiStringGroupTag = objSource.MultiStringGroupTag;

            SetRelations();
        }

        public void MergeFrom(ISerializableObject so)
        {
            throw new System.NotImplementedException();
        }

        public void NotifyPropertiesChanged()
        {
            throw new System.NotImplementedException();
        }

        public void UnsetPropertiesChanged()
        {
            throw new System.NotImplementedException();
        }

        public ISerializableObject Serialize()
        {
            throw new System.NotImplementedException();
        }

        public void Deserialize(ISerializableObject so)
        {
            throw new System.NotImplementedException();
        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.MultiStringGroupId = DbConvert.ToInt64(dr, "MultiStringGroupID");
            this.MultiStringGroupTag = DbConvert.ToString(dr, "MultiStringGroupTag");
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["MultiStringGroupID"] = this.MultiStringGroupId;
            dr["MultiStringGroupTag"] = this.MultiStringGroupTag;

            return dr;
        }


    }
    public class MultistringGroupDictionary : LineObjectDictionaryByIdBase<MultistringGroupLn>
    {
    }


}
