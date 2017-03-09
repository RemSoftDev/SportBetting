using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public sealed class GroupLn : ObjectBase, ILineObjectWithId<GroupLn>, IGroupLn
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(GroupLn));

        private GroupTournamentExternalState m_gtes = null;
        private GroupSportExternalState m_gses = null;

        public static readonly TableSpecification TableSpec = new TableSpecification("Groups", false, "GroupId");

        private ObjectStringDictionary m_diStrings = null;

        public static string GetKeyName(string sGroupType, long lServerId)
        {
            return string.Concat(sGroupType, KEY_SEPARATOR, lServerId);
        }

        public string KeyName { get { return GroupLn.GetKeyName(this.Type, this.SvrGroupId); } }

        public const string GROUP_TYPE_GROUP = "group";            // Group
        public const string GROUP_TYPE_GROUP_C = "group_category";   // Group Generated from Category
        public const string GROUP_TYPE_GROUP_T = "group_tournament"; // Group Generated from Tournament
        public const string GROUP_TYPE_SPORT = "group_sport";      // Group Generated from Sport
        public const string GROUP_TYPE_COUNTRY = "group_country";    // Group Generated from Country

        public long Id { get { return this.GroupId; } }
        public long GroupId { get; set; }
        public ObservableProperty<long?> ParentGroupId { get; set; }
        public long SvrGroupId { get; set; }
        public string Type { get; set; }
        public ObservableProperty<int> Sort { get; set; }
        public string DefaultName { get; set; }
        public string ExternalState { get; set; }
        public ObservableProperty<bool> Active { get; set; }

        public GroupTournamentExternalState GroupTournament
        {
            get
            {
                if (this.Type == GROUP_TYPE_GROUP_T && m_gtes == null)
                {
                    m_gtes = new GroupTournamentExternalState();
                }

                return m_gtes;
            }
        }

        public GroupSportExternalState GroupSport
        {
            get
            {
                if (this.Type == GROUP_TYPE_SPORT && m_gses == null)
                {
                    m_gses = new GroupSportExternalState();
                }

                return m_gses;
            }
        }

        public GroupLn()
            : base(true)
        {
            this.ChildGroups = new LineObjectList<GroupLn>();
        }

        public ObjectStringDictionary Strings
        {
            get
            {
                if (m_diStrings == null)
                {
                    Debug.Assert(this.GroupId != 0);

                    m_diStrings = LineSr.Instance.AllObjects.TaggedStrings.GetGroupStrings(this.GroupId);
                }

                return m_diStrings;
            }
        }

        public string GetDisplayName(string sLanguage)
        {
            TaggedStringLn tstr = this.Strings != null ? m_diStrings.SafelyGetString(sLanguage) : null;

#if DEBUG
            if (tstr == null)
            {

            }
#endif

            if (tstr != null)
                return tstr.Text;

            return string.Format("GROUP_{0}__{1}", this.Type.ToUpperInvariant(), this.GroupId.ToString("G"));
        }

        public GroupLn ParentGroup { get; protected set; }
        public LineObjectList<GroupLn> ChildGroups { get; protected set; }

        public override int GetHashCode()
        {
            return this.KeyName.GetHashCode();
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.DoesGroupExist(this.Type, this.SvrGroupId); }
        }

        private void EnsureExternalObjects()
        {
            string sExternalState = !string.IsNullOrEmpty(this.ExternalState) ? this.ExternalState : ObjectBase.EMPTY_EXTERNAL_STATE;

            if (this.Type == GROUP_TYPE_GROUP_T)
            {
                m_gtes = LineSerializeHelper.StringToObject<GroupTournamentExternalState>(sExternalState);
            }
            else if (this.Type == GROUP_TYPE_SPORT)
            {
                m_gses = LineSerializeHelper.StringToObject<GroupSportExternalState>(sExternalState);
            }
        }

        private void EnsureExternalState()
        {
            if (this.Type == GROUP_TYPE_GROUP_T)
            {
                this.ExternalState = LineSerializeHelper.ObjectToString<GroupTournamentExternalState>(this.GroupTournament);
            }
            else if (this.Type == GROUP_TYPE_SPORT)
            {
                this.ExternalState = LineSerializeHelper.ObjectToString<GroupSportExternalState>(this.GroupSport);
            }
        }

        public override void FillFromDataRow(DataRow dr)
        {
            this.GroupId = DbConvert.ToInt64(dr, "GroupId");
            this.ParentGroupId.Value = DbConvert.ToNullableInt64(dr, "ParentGroupId");
            this.SvrGroupId = DbConvert.ToInt64(dr, "SvrGroupId");
            this.UpdateId = DbConvert.ToInt64(dr, "UpdateId");
            this.Type = DbConvert.ToString(dr, "Type");
            this.Sort.Value = DbConvert.ToInt32(dr, "Sort");
            this.Active.Value = DbConvert.ToBool(dr, "Active");
            this.ExternalState = DbConvert.ToString(dr, "ExternalState");

            EnsureExternalObjects();
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            try
            {
                DataRow dr = dtSample.NewRow();

                EnsureExternalState();

                dr["GroupId"] = this.GroupId;
                DataCopy.SetNullableColumn(dr, "ParentGroupId", this.ParentGroupId.Value);
                dr["SvrGroupId"] = this.SvrGroupId;
                dr["UpdateId"] = this.UpdateId;
                dr["Type"] = this.Type;
                dr["Sort"] = this.Sort.Value;
                dr["Active"] = this.Active.Value;
                dr["ExternalState"] = this.ExternalState;

                return dr;
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "GroupLn.CreateDataRow() ERROR");
                throw;
            }
        }

        public void MergeFrom(GroupLn objSource)
        {
            ExcpHelper.ThrowIf<InvalidOperationException>(this.GroupId != objSource.GroupId, "Cannot merge objects with different Group Ids {0} => {1}", objSource, this);
            ExcpHelper.ThrowIf<InvalidOperationException>(this.Type != objSource.Type, "Cannot merge objects with different Group Types {0} => {1}", objSource, this);

            objSource.EnsureExternalState();

            this.ParentGroupId.Value = objSource.ParentGroupId.Value;
            this.Sort.Value = objSource.Sort.Value;
            this.ExternalState = objSource.ExternalState;

            if (this.ExternalState != null)
            {
                this.EnsureExternalObjects();
            }

            this.SetRelations();
        }

        public override void SetRelations()
        {
            try
            {
                //ExcpHelper.ThrowIf(!LineSr.Instance.AllObjects.RelatedStrings.ContainsGroupId(this.GroupId), "Cannot get strings for {0}", this);

                if (this.ParentGroupId.Value == null)
                {
                    this.ParentGroup = null;
                }
                else if (this.ParentGroup == null || this.ChangedProps.Contains(this.ParentGroupId))
                {
                    long lParentGroupId = (long)this.ParentGroupId.Value;

                    this.ParentGroup = LineSr.Instance.AllObjects.Groups.GetObject(lParentGroupId);
                    ExcpHelper.ThrowIf(this.ParentGroup == null, "Cannot get ParentGroup (ParentGroupId={0}) for {1}", this.ParentGroupId.Value, this);
                }
            }
            catch
            {
                throw;
            }
        }

        public override ISerializableObject Serialize()
        {
            EnsureExternalState();

            dynamic so = new SerializableObject(this.GetType());

            so.GroupId = this.GroupId;
            so.ParentGroupId = this.ParentGroupId.Value;
            so.SvrGroupId = this.SvrGroupId;
            so.Type = this.Type;
            so.Sort = this.Sort.Value;
            so.Active = this.Active.Value;
            so.ExternalState = this.ExternalState;

            return so;
        }

        public override void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.GroupId = dso.GroupId.Value;
            this.ParentGroupId.Value = dso.ParentGroupId.Value;
            this.SvrGroupId = dso.SvrGroupId.Value;
            this.Type = dso.Type.Value;
            this.Sort.Value = dso.Sort.Value;
            this.Active.Value = dso.Active.Value;
            this.ExternalState = dso.ExternalState.Value;

            EnsureExternalObjects();
        }

        public override string ToString()
        {
            string sLang = DalStationSettings.Instance.Language;

            return string.Format("GroupLn {{GroupId={0}, SvrGroupId={1}, Type='{2}', DisplayName({3})='{4}', Active={5}, IsNew={6}}}", this.GroupId, this.SvrGroupId, this.Type, sLang, this.GetDisplayName(sLang), this.Active.Value, this.IsNew);
        }

        public GroupVw GroupView
        {
            get
            {
                if (m_objView == null)
                {
                    m_objView = new GroupVw(this);
                }

                return m_objView as GroupVw;
            }
        }
    }

    [XmlRoot("state", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = true)]
    public class GroupTournamentExternalState
    {
        [XmlElement(ElementName = "sportId")]
        public long SportGroupId { get; set; }
        [XmlElement(ElementName = "countryId")]
        public long? CountryGroupId { get; set; }
        [XmlElement(ElementName = "mincomb")]
        public int? MinCombination { get; set; }
        [XmlElement(ElementName = "btrid")]
        public long? BtrTournamentId { get; set; }

        [XmlIgnore]
        public bool CountryGroupIdSpecified { get { return this.CountryGroupId != null; } }
        [XmlIgnore]
        public bool MinCombinationSpecified { get { return this.MinCombination != null; } }

        public override int GetHashCode()
        {
            int iHashCode = this.SportGroupId.GetHashCode();

            if (this.CountryGroupId != null)
            {
                iHashCode ^= this.CountryGroupId.GetHashCode();
            }

            if (this.MinCombination != null)
            {
                iHashCode ^= this.MinCombination.GetHashCode();
            }

            return iHashCode;
        }

        public override bool Equals(object obj)
        {
            GroupTournamentExternalState gtes = obj as GroupTournamentExternalState;

            return gtes != null && gtes.GetHashCode() == this.GetHashCode();
        }
    }

    [XmlRoot("state", Namespace = LineSerializeHelper.DEFAULT_NAMESPACE, IsNullable = true)]
    public class GroupSportExternalState
    {
        [XmlElement(ElementName = "desc")]
        public string SportDescriptor { get; set; }
        [XmlElement(ElementName = "btrsportId")]
        public long? BtrSportId { get; set; }

        public override int GetHashCode()
        {
            return this.SportDescriptor != null ? this.SportDescriptor.GetHashCode() : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            GroupSportExternalState gses = obj as GroupSportExternalState;

            return gses != null && gses.GetHashCode() == this.GetHashCode();
        }
    }

    public class GroupDictionary : LineObjectDictionaryByIdBase<GroupLn>
    {
        protected Dictionary<string, GroupLn> m_diKeyToGroup = new Dictionary<string, GroupLn>();

        public override GroupLn MergeLineObject(GroupLn objSource)
        {
            lock (m_oLocker)
            {
                var groupMerged = base.MergeLineObjectImp(objSource);

                m_diKeyToGroup[groupMerged.KeyName] = groupMerged;

                return groupMerged;
            }
        }

        public override void AddSafely(GroupLn objSource)
        {
            lock (m_oLocker)
            {
                AddSafelyImp(objSource);

                if (!m_diKeyToGroup.ContainsKey(objSource.KeyName))
                {
                    m_diKeyToGroup.Add(objSource.KeyName, objSource);
                }
            }
        }

        public override void AddStrictly(GroupLn objSource)
        {
            lock (m_oLocker)
            {
                AddStrictlyImp(objSource);

                ExcpHelper.ThrowIf(m_diKeyToGroup.ContainsKey(objSource.KeyName), "GroupDictionary.AddStrictly({1}) ERROR. Such object already exists in database.", m_type.Name, objSource);

                m_diKeyToGroup.Add(objSource.KeyName, objSource);
            }
        }

        public GroupLn SafelyGetGroupByKeyName(string sGroupType, long lServerId)
        {
            string sKeyName = GroupLn.GetKeyName(sGroupType, lServerId);

            lock (m_oLocker)
            {
                return m_diKeyToGroup.ContainsKey(sKeyName) ? m_diKeyToGroup[sKeyName] : null;
            }
        }
    }
}
