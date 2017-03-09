using System;
using System.Collections.Generic;
using System.Data;
using SportRadar.Common.Logs;

namespace SportRadar.DAL.CommonObjects
{
    public class IdentityCache : Dictionary<long, long>
    {
        public const long ERROR_IDENTITY = -1;

        protected static SportRadar.Common.Logs.ILog m_logger = LogFactory.CreateLog(typeof(IdentityCache));

        protected string m_sTableName = null;
        protected string m_sLocalIdColumnName = null;
        protected string m_sSvrIdColumnName = null;

        protected DataCopy m_dsbc = null;

        protected long m_lMaxLocalIdentity = 0;

        internal protected IdentityCache(DataCopy dsbc, string sTableName, string sLocalIdColumnName, string sSvrIdColumnName)
        {
            m_dsbc = dsbc;

            m_sTableName = sTableName;
            m_sLocalIdColumnName = sLocalIdColumnName;
            m_sSvrIdColumnName = sSvrIdColumnName;
        }

        public long MaxLocalIdentity { get { return m_lMaxLocalIdentity; } }

        public void UpdateMaxLocalIdentity()
        {
            if (!string.IsNullOrEmpty(m_sLocalIdColumnName))
            {
                m_lMaxLocalIdentity = IdentityCache.GetMaxLocalIdentity(m_dsbc.Connection, m_dsbc.Transaction, m_sTableName, m_sLocalIdColumnName);
            }
        }

        public static long GetMaxLocalIdentity(IDbConnection conn, IDbTransaction transaction, string sTableName, string sLocalIdColumnName)
        {
            try
            {
                object objMax = DataCopy.ExecuteScalar(conn, transaction, "SELECT MAX({0}) FROM {1}", sLocalIdColumnName, sTableName);

                return objMax == DBNull.Value ? 0 : Convert.ToInt64(objMax);
            }
            catch (Exception excp)
            {
                m_logger.ErrorFormat("GetMaxLocalIdentity({0}, {1}) ERROR:{2}\r\n{3}",excp, sTableName, sLocalIdColumnName, excp.Message, excp.StackTrace);

                string sError = string.Format("Cannot get local MAX Identity for [{0}][{1}]", sLocalIdColumnName, sTableName);

                throw new System.Exception(sError);
            }
        }

        public void FillFromDataRow(DataRow dr, ErrorList elDuplicates)
        {
            long lLocId = Convert.ToInt64(dr[m_sLocalIdColumnName]);
            long lSvrId = Convert.ToInt64(dr[m_sSvrIdColumnName]);

            if (!this.ContainsKey(lSvrId))
            {
                this.Add(lSvrId, lLocId);
            }
            else if (elDuplicates != null)
            {
                elDuplicates.AddUnique(lSvrId.ToString("G"));
            }
        }

        public void FillFromDataRows(IEnumerable<DataRow> DataRows)
        {
            foreach (DataRow dr in DataRows)
            {
                FillFromDataRow(dr, null);
            }
        }

        public void Fill(bool bUseMaxLocalIdentity)
        {
            const int SHOW_DUPLICATE_LIMIT = 10;

            if (string.IsNullOrEmpty(m_sLocalIdColumnName) || string.IsNullOrEmpty(m_sSvrIdColumnName))
            {
                return;
            }

            try
            {
                using (DataTable dt = DataCopy.GetDataTable(m_dsbc.Connection, m_dsbc.Transaction, "SELECT {0}, {1} FROM {2} WHERE {0} > {3}", m_sLocalIdColumnName, m_sSvrIdColumnName, m_sTableName, bUseMaxLocalIdentity ? m_lMaxLocalIdentity : ERROR_IDENTITY))
                    //using (DataTable dt = DataCopy.GetDataTable(m_dsbc.Dialect, m_dsbc.Connection, m_dsbc.Transaction, "SELECT {0}, {1} FROM {2}", m_sLocalIdColumnName, m_sSvrIdColumnName, m_sTableName))
                {
                    //this.Clear();

                    ErrorList elDuplicates = new ErrorList();

                    foreach (DataRow dr in dt.Rows)
                    {
                        FillFromDataRow(dr, elDuplicates);
                    }

                    if (elDuplicates.Count > 0)
                    {
                        m_dsbc.Warnings.AddFormat("Duplicate Server ID(s) found for Table: [{0}],  [{1}]: {2}", m_sTableName, m_sSvrIdColumnName, elDuplicates.ToFormatStringWithLimit(", ", SHOW_DUPLICATE_LIMIT));
                    }

                    //                    this.Trace();
                }
            }
            catch (Exception excp)
            {
                string sError = string.Format("Error FillIdentityCache ({0}, {1}, {2}, {3})", m_sTableName, m_sLocalIdColumnName, m_sSvrIdColumnName, m_lMaxLocalIdentity);

                m_logger.ErrorFormat("{0}:\r\n{1}\r\n{2}", excp, sError, excp.Message, excp.StackTrace);

                throw new System.Exception(sError);
            }
        }

        public long GetLocalIdSafely(long lSvrId)
        {
            try
            {
                return GetLocalId(lSvrId);
            }
            catch
            {
            }

            return 0;
        }

        public long? GetNullableLocalIdSafely(long? lSvrId)
        {
            try
            {
                return GetNullableLocalId(lSvrId);
            }
            catch
            {
            }

            return 0;
        }

        public long GetLocalId(long lSvrId)
        {
            if (lSvrId == 0)
            {
                return 0;
            }

            if (this.ContainsKey(lSvrId))
            {
                return this[lSvrId];
            }

            string sError = string.Format("ERROR: Cannot find Local ID (Table: [{0}]; [{1}] => [{2}];  SvrID: {3};) (Total records in cache: {4})", m_sTableName, m_sSvrIdColumnName, m_sLocalIdColumnName, lSvrId, this.Count);

            m_logger.Warn(sError);

            throw new System.Exception(sError);
        }

        public long? GetNullableLocalId(long? lSvrId)
        {
            if (lSvrId == null || lSvrId == 0)
            {
                return null;
            }

            long lIdentity = (long)lSvrId;

            if (this.ContainsKey(lIdentity))
            {
                return this[lIdentity];
            }

            string sError = string.Format("ERROR: Cannot find Local ID (Table: [{0}]; [{1}] => [{2}];  SvrID: {3};) (Total records in cache: {4})", m_sTableName, m_sSvrIdColumnName, m_sLocalIdColumnName, lSvrId, this.Count);

            m_logger.Warn(sError);

            throw new System.Exception(sError);
        }

        public void Trace()
        {
            try
            {
                string sInfo = string.Format("\r\nIdentityCache [{0}] rows({1}):\r\n", m_sTableName, this.Count);

                foreach (KeyValuePair<long, long> kvp in this)
                {
                    sInfo += string.Format("{0}, {1}\r\n", kvp.Key, kvp.Value);
                }

                Console.WriteLine(sInfo);
            }
            catch (Exception excp)
            {
                m_logger.ErrorFormat("TestEdintity ERROR:{0}\r\n{1}",excp, excp.Message, excp.StackTrace);
            }
        }
    }
}