using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;
using System.Diagnostics;
using System.Data;
using SportRadar.Common.Collections;

namespace SportRadar.DAL.NewLineObjects
{
    public enum eAssignmentType
    {
        COUNTRY = 0,
        TOURNAMENT = 1,
        T_CONFIG = 2
    }

    public class ResourceAssignmentLn : DatabaseBase, ILineObjectWithKey<ResourceAssignmentLn>
    {
        public static readonly TableSpecification TableSpec = new TableSpecification("ResourceAssignment", false, "ObjectId", "ObjectClass");

        public string AssignmentKey { get; set; }

        public long ResourceId { get; set; }
        public long ObjectId  { get; set; }
        public eAssignmentType ResourceType { get; set; }
        public bool Active  { get; set; }


        public string KeyName
        {
            get { return ResourceAssignmentLn.GetKeyName(this.ResourceType.ToString(), this.ObjectId); }
        }

        public static string GetKeyName(string sResourceType, long lObjectId)
        {
            return string.Concat(sResourceType, KEY_SEPARATOR, lObjectId);
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.ResourceAssignments.ContainsKey(this.KeyName); }
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public ResourceAssignmentLn()
        {
        }

        public override void FillFromDataRow(System.Data.DataRow dr)
        {
            this.ResourceId = DbConvert.ToInt64(dr, "ResourceId");
            this.ResourceType = (eAssignmentType)Enum.Parse(typeof(eAssignmentType), DbConvert.ToString(dr, "ObjectClass"));
            this.ObjectId = DbConvert.ToInt64(dr, "ObjectId");
            this.Active = DbConvert.ToBool(dr, "Active");
        }

        public override System.Data.DataRow CreateDataRow(System.Data.DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["ResourceId"] = this.ResourceId;
            dr["ObjectId"] = this.ObjectId;
            dr["Active"] = this.Active;
            dr["ObjectClass"] = this.ResourceType;

            return dr;
        }

        public void MergeFrom(ResourceAssignmentLn objSource)
        {
            Debug.Assert(this.ResourceType == objSource.ResourceType);
            Debug.Assert(this.ObjectId == objSource.ObjectId);

            this.ResourceId = objSource.ResourceId;
            this.Active = objSource.Active;

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

    public class ResourceAssignmentDictionary : LineObjectDictionaryByKeyBase<ResourceAssignmentLn>
    {
    }
}
