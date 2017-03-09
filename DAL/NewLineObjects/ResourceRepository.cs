using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using System.Diagnostics;
using System.Data;

namespace SportRadar.DAL.NewLineObjects
{
    public enum eResourceType
    {
        COUNTRY_FLAGS = 0,
        TOURNAMENT_FLAGS = 1,
        CONFIG = 2
    }

    public class ResourceRepositoryLn : DatabaseBase, ILineObjectWithId<ResourceRepositoryLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("ResourseRepository", false, "ResourceId");

        public long ResourceId { get; set; }
        public eResourceType ResourceType { get; set; }
        public string MimeType { get; set; }
        public string Data { get; set; }

        public ResourceRepositoryLn()
        {
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.ResourceId = DbConvert.ToInt64(dr, "ResourceId");
            this.ResourceType = (eResourceType)Enum.Parse(typeof(eResourceType), DbConvert.ToString(dr, "ResourceType"));
            this.MimeType = DbConvert.ToString(dr, "MimeType");
            this.Data = DbConvert.ToString(dr, "Data");
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["ResourceId"] = this.ResourceId;
            dr["ResourceType"] = this.ResourceType;
            dr["MimeType"] = this.MimeType;
            dr["Data"] = this.Data;

            return dr;
        }

        public long Id
        {
            get { return this.ResourceId; }
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.Resources.ContainsKey(this.ResourceId); }
        }

        public void MergeFrom(ResourceRepositoryLn objSource)
        {
            Debug.Assert(this.ResourceId == objSource.ResourceId);

            this.ResourceType= objSource.ResourceType;
            this.MimeType = objSource.MimeType;
            this.Data = objSource.Data;

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

    public class ResourceRepositoryDictionary : LineObjectDictionaryByIdBase<ResourceRepositoryLn>
    {
    }
}
