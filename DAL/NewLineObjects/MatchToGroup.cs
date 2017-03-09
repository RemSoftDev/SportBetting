using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public class MatchToGroupLn : DatabaseBase, ILineObjectWithKey<MatchToGroupLn>, IRemovableLineObject<MatchToGroupLn>
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(MatchLn));
        public static readonly TableSpecification TableSpec = new TableSpecification("MatchToGroup", false, "MatchId", "GroupId");

        public long MatchId { get; set; }
        public long GroupId { get; set; }
        public string Type { get; set; }
        public int Sort { get; set; }

        public MatchToGroupLn()
        {
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override void FillFromDataRow(DataRow dr)
        {
            this.MatchId = DbConvert.ToInt64(dr, "MatchId");
            this.GroupId = DbConvert.ToInt64(dr, "GroupId");
            this.Type = DbConvert.ToString(dr, "Type");
            this.Sort = DbConvert.ToInt32(dr, "Sort");
            this.UpdateId = DbConvert.ToInt64(dr, "UpdateId");
        }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            DataRow dr = dtSample.NewRow();

            dr["MatchId"] = this.MatchId;
            dr["GroupId"] = this.GroupId;
            dr["Type"] = this.Type;
            dr["Sort"] = this.Sort;
            dr["UpdateId"] = this.UpdateId;

            return dr;
        }

        public string KeyName { get { return MatchToGroupLn.GetKeyName(this.MatchId, this.GroupId); } }
        public long RemoveId { get { return this.MatchId; } }

        public static string GetKeyName(long lMatchId, long lGroupId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(lMatchId);
            sb.Append(KEY_SEPARATOR);
            sb.Append(lGroupId);
            return sb.ToString();
        }

        public bool IsNew
        {
            get { return !DatabaseCache.Instance.AllObjects.MatchesToGroups.ContainsKey(this.KeyName); }
        }

        public void MergeFrom(MatchToGroupLn objSource)
        {
            Debug.Assert(this.MatchId == objSource.MatchId);
            Debug.Assert(this.GroupId == objSource.GroupId);
            Debug.Assert(this.Type == objSource.Type);

            this.Sort = objSource.Sort;
        }

        public void MergeFrom(ISerializableObject so)
        {
            dynamic dso = new SerializableObject(this.GetType());

            //Debug.Assert(this.MatchId == dso.MatchId.Value);
            //Debug.Assert(this.GroupId == dso.GroupId.Value);
            //Debug.Assert(this.Type == dso.Type.Value);

            this.Sort = dso.Sort.Value;

        }

        public void NotifyPropertiesChanged()
        {
            throw new NotImplementedException();
        }

        public void UnsetPropertiesChanged()
        {
            throw new NotImplementedException();
        }

        public ISerializableObject Serialize()
        {
            dynamic so = new SerializableObject(this.GetType());

            so.MatchId = this.MatchId;
            so.GroupId = this.GroupId;
            so.Type = this.Type;
            so.Sort = this.Sort;

            return so;
        }

        public  void Deserialize(ISerializableObject so)
        {
            dynamic dso = so;

            this.MatchId = dso.MatchId.Value;
            this.GroupId = dso.GroupId.Value;
            this.Type = dso.Type.Value;
            this.Sort = dso.Sort.Value;
        }

        public override string ToString()
        {
            return string.Format("MatchToGroupLn{{MatchId={0}, GroupId={1}, Type='{2}', Sort={3}, IsNew={4}}}", this.MatchId, this.GroupId, this.Type, this.Sort, this.IsNew);
        }
    }

    public sealed class SyncDictionaryOfLists<T> : SyncDictionary<long, SyncList<T>>
    {
        public void SafelyAddObjectToList(long lKeyObjectId, T objToList)
        {
            SyncList<T> lObjectsToList = null;

            lock (m_oLocker)
            {
                if (m_di.ContainsKey(lKeyObjectId))
                {
                    lObjectsToList = m_di[lKeyObjectId];
                }
                else
                {
                    lObjectsToList = new SyncList<T>();
                    m_di[lKeyObjectId] = lObjectsToList;
                }
            }

            lObjectsToList.SafelyAdd(objToList);
        }

        public void SafelyRemoveObjectFromList(long lKeyObjectId, T objToList)
        {
            SyncList<T> lObjectsToList = null;

            lock (m_oLocker)
            {
                if (m_di.ContainsKey(lKeyObjectId))
                {
                    lObjectsToList = m_di[lKeyObjectId];
                }
            }

            if (lObjectsToList != null)
            {
                lObjectsToList.Remove(objToList);
            }
        }

        public SyncList<T> GetObjectList(long lKeyObjectId)
        {
            lock (m_oLocker)
            {
                return m_di.ContainsKey(lKeyObjectId) ? m_di[lKeyObjectId] : null;
            }
        }
    }

    public sealed class MatchToGroupDictionary : LineObjectDictionaryByKeyBase<MatchToGroupLn>
    {
        private SyncDictionaryOfLists<GroupLn> m_diMatchToGroups = new SyncDictionaryOfLists<GroupLn>();
        private SyncDictionaryOfLists<MatchLn> m_diGroupToMatchs = new SyncDictionaryOfLists<MatchLn>();

        public override void Clear()
        {
            lock (m_oLocker)
            {
                m_di.Clear();
                m_diMatchToGroups.Clear();
                m_diGroupToMatchs.Clear();
            }
        }

        public SyncList<MatchLn> GetGroupMatches(long lGroupId)
        {
            return m_diGroupToMatchs.GetObjectList(lGroupId);
        }

        public SyncList<GroupLn> GetMatchGroups(long lMatchId)
        {
            return m_diMatchToGroups.GetObjectList(lMatchId);
        }

        public SyncList<MatchToGroupLn> RemoveByMatch(MatchLn match)
        {
            SyncList<MatchToGroupLn> lResult = new SyncList<MatchToGroupLn>();

            lock (m_oLocker)
            {
                SyncList<GroupLn> lGroups = m_diMatchToGroups.GetObjectList(match.MatchId);

                if (lGroups != null)
                    foreach (var group in lGroups)
                    {
                        string sKey = MatchToGroupLn.GetKeyName(match.MatchId, @group.GroupId);

                        if (m_di.ContainsKey(sKey))
                        {
                            MatchToGroupLn mtog = m_di[sKey];
                            m_di.Remove(sKey);

                            lResult.Add(mtog);

                            SyncList<MatchLn> lMatches = this.GetGroupMatches(@group.GroupId);

                            if (lMatches != null)
                            {
                                lMatches.Remove(match);
                            }
                        }
                    }
            }

            return lResult;
        }

        public override MatchToGroupLn MergeLineObject(MatchToGroupLn objSource)
        {
            MatchToGroupLn objMerged = base.MergeLineObject(objSource);

            GroupLn group = LineSr.Instance.AllObjects.Groups.GetObject(objMerged.GroupId);
            MatchLn match = LineSr.Instance.AllObjects.Matches.GetObject(objMerged.MatchId);

            Debug.Assert(group != null);
            Debug.Assert(match != null);

            m_diMatchToGroups.SafelyAddObjectToList(objMerged.MatchId, group);
            m_diGroupToMatchs.SafelyAddObjectToList(objMerged.GroupId, match);

            return objMerged;
        }
    }
}
