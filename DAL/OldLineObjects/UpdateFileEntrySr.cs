using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;

namespace SportRadar.DAL.OldLineObjects
{
    public sealed class UpdateFileEntrySr : DatabaseBase, ILineObject<UpdateFileEntrySr>
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(UpdateFileEntrySr));

        private static UpdateFileEntrySr m_ufeLastLocalization = null;
        private static UpdateFileEntrySr m_ufeLastLine = null;
        private static UpdateFileEntrySr m_ufeLastResource = null;
        private static UpdateFileEntrySr m_ufeStatLastLine = null;
        private static UpdateFileEntrySr m_ufeMetaLastLine = null;
        private static UpdateFileEntrySr m_ufeLiabLastLine = null;
        private static Dictionary<long, UpdateFileEntrySr> m_diUpdates = null;

        public static readonly TableSpecification TableSpec = new TableSpecification("UpdateFileEntry", true, "UpdateFileID");

        private static Object m_objLocker = new Object();

        public static UpdateFileEntrySr GetByFileId(long lFileId)
        {
            RefreshDbStoredUpdates();

            lock (m_objLocker)
            {
                return m_diUpdates != null && m_diUpdates.ContainsKey(lFileId) ? m_diUpdates[lFileId] : null;
            }
        }

        public static UpdateFileEntrySr GetLastUpdate(eDataSyncCacheType type)
        {
            RefreshDbStoredUpdates();

            lock (m_objLocker)
            {
                switch (type)
                {
                    case eDataSyncCacheType.String: return m_ufeLastLocalization;
                    case eDataSyncCacheType.Match: return m_ufeLastLine;
                    case eDataSyncCacheType.Statistic: return m_ufeStatLastLine;
                    case eDataSyncCacheType.ActiveTournaments: return null;
                    case eDataSyncCacheType.Resources: return m_ufeLastResource;
                    case eDataSyncCacheType.Metainfo: return m_ufeMetaLastLine;
                    case eDataSyncCacheType.Liability: return m_ufeLiabLastLine;

                    default:

                        Debug.Assert(false);
                        break;
                }
            }

            return null;
        }

        public static void SetLastUpdate(UpdateFileEntrySr ufe)
        {
            lock (m_objLocker)
            {
                eDataSyncCacheType dsct = (eDataSyncCacheType)Enum.Parse(typeof(eDataSyncCacheType), ufe.DataSyncCacheType, true);

                switch (dsct)
                {
                    case eDataSyncCacheType.String: m_ufeLastLocalization = ufe; break;
                    case eDataSyncCacheType.Match: m_ufeLastLine = ufe; break;
                    case eDataSyncCacheType.Statistic: m_ufeStatLastLine = ufe; break;
                    case eDataSyncCacheType.Resources: m_ufeLastResource = ufe; break;
                    case eDataSyncCacheType.Metainfo: m_ufeMetaLastLine = ufe; break;
                    case eDataSyncCacheType.ActiveTournaments: return;
                    case eDataSyncCacheType.Liability: m_ufeLiabLastLine = ufe; break;

                    default:

                        Debug.Assert(false);
                        break;
                }
            }
        }

        private static void RefreshByType(eDataSyncCacheType type)
        {
            using (DataTable dt = DataCopy.GetDataTable(ConnectionManager.GetConnection(), null, "SELECT * FROM UpdateFileEntry WHERE DataSyncCacheType = '{0}' ORDER BY DataSyncCacheID DESC", type))
            {
                CheckTime ct = new CheckTime();
                ct.AddEvent("UpdateFileEntry SQL Done (Count = {0})", dt.Rows.Count);

                if (dt.Rows.Count > 0)
                {
                    SetLastUpdate(UpdateFileEntrySr.CreateFromDataRow(dt.Rows[0]));
                }

                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        UpdateFileEntrySr ufe = UpdateFileEntrySr.CreateFromDataRow(dr);

                        if (m_diUpdates.ContainsKey(ufe.DataSyncCacheID))
                        {
                            m_diUpdates.Add(ufe.DataSyncCacheID, ufe);
                        }
                    }
                    catch (Exception excp)
                    {
                        m_logger.ErrorFormat("RefreshByType Row Exception:{0}\r\n{1}",excp, excp.Message, excp.StackTrace);
                    }
                }

                ct.AddEvent("RefreshByType({0}) filled up (Count = {1})", type, m_diUpdates.Count);
                ct.Info(m_logger);
            }
        }

        private static void RefreshDbStoredUpdates()
        {
            lock (m_objLocker)
            {
                if (m_diUpdates != null)
                {
                    return;
                }

                m_diUpdates = new Dictionary<long, UpdateFileEntrySr>();

                try
                {
                    RefreshByType(eDataSyncCacheType.String);
                    RefreshByType(eDataSyncCacheType.Match);
                    RefreshByType(eDataSyncCacheType.Metainfo);
                    RefreshByType(eDataSyncCacheType.Statistic);
                    RefreshByType(eDataSyncCacheType.Resources);
                    RefreshByType(eDataSyncCacheType.Liability);
                }
                catch (Exception excp)
                {
                    m_logger.ErrorFormat("GetDbStoredUpdates General Exception:{0}\r\n{1}",excp, excp.Message, excp.StackTrace);
                    m_diUpdates = null;
                }
            }
        }

        public static UpdateFileEntrySr CreateFromDataRow(DataRow dr)
        {
            UpdateFileEntrySr ufe = new UpdateFileEntrySr();

            ufe.FillFromDataRow(dr);

            return ufe;
        }

        public ObservablePropertyList ChangedProps { get; private set; }

        public override void FillFromDataRow(DataRow dr)
        {
            this.UpdateFileID = DbConvert.ToInt64(dr, "UpdateFileID");
            //this.UpdateId = DbConvert.ToInt64(dr, "UpdateId");
            this.DataSyncCacheID = DbConvert.ToInt64(dr, "DataSyncCacheID");
            this.DataSyncCacheType = DbConvert.ToString(dr, "DataSyncCacheType");
            this.FileName = DbConvert.ToString(dr, "FileName");
            this.Description = DbConvert.ToString(dr, "Description");
            this.CreateDate = DbConvert.ToDateTime(dr, "CreateDate");
        }

        public bool IsNew
        {
            get
            {
                throw new Exception("NOT OMPLEMENTED");
            }
        }

        public void MergeFrom(UpdateFileEntrySr ufe)
        {
            throw new Exception("NOT OMPLEMENTED");
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

        public ISerializableObject Serialize()
        {
            throw new NotImplementedException();
        }

        public void Deserialize(ISerializableObject so)
        {
            throw new NotImplementedException();
        }

        public long UpdateFileID { get; set; }
        public long DataSyncCacheID { get; set; }
        public string DataSyncCacheType { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }

        public override long ORMID { get { return this.UpdateFileID; } }

        public override TableSpecification Table { get { return UpdateFileEntrySr.TableSpec; } }

        public override DataRow CreateDataRow(DataTable dtSample)
        {
            return DataCopy.CreateDataRow(dtSample, this, new ErrorList());
        }
    }
}