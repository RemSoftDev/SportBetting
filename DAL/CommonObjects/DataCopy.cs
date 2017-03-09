using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using SportRadar.Common.Logs;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.Connection;
using SportRadar.Common.Windows;

namespace SportRadar.DAL.CommonObjects
{
    public enum eFileSyncResult
    {
        Failed = 0,
        Succeeded = 1,
        Skipped = 2
    }

    public enum eUpdateType
    {
        Initialize = 0,
        PreMatches = 1,
        LiveBet = 2,
        Other = 3,
        VirtualBet = 4,

    }

    public sealed class DataCopyTables : IDisposable
    {
        public DataTable InsertDataTable { get; private set; }
        public DataTable UpdateDataTable { get; private set; }

        private static Dictionary<string, DataTable> m_diEmptyDataTables = new Dictionary<string, DataTable>();
        private static object m_oLocker = new object();

        public static DataCopyTables GetDataCopyTables(IDbConnection conn, IDbTransaction transaction, string sTableName)
        {
            lock (m_oLocker)
            {
                DataTable dt = GetEmptyDataTableByName(conn, transaction, sTableName);

                DataCopyTables dct = new DataCopyTables();

                lock (m_oLocker)
                {
                    dct.InsertDataTable = dt.Clone();
                    dct.UpdateDataTable = dt.Clone();
                }

                return dct;
            }
        }

        public static DataTable GetEmptyDataTableByName(IDbConnection conn, IDbTransaction transaction, string sTableName)
        {
            lock (m_oLocker)
            {
                if (!m_diEmptyDataTables.ContainsKey(sTableName))
                {
                    DataTable dt = GetEmptyDataTableByNameImp(conn, transaction, sTableName);
                    m_diEmptyDataTables.Add(sTableName, dt);

                    return dt;
                }

                return m_diEmptyDataTables[sTableName].Clone();
            }
        }

        private static DataTable GetEmptyDataTableByNameImp(IDbConnection conn, IDbTransaction transaction, string sTableName)
        {
            string sFormat = null;

            switch (ConnectionManager.Dialect)
            {
                case DatabaseDialect.MsSql: sFormat = "SELECT TOP 0 * FROM {0}"; break;
                case DatabaseDialect.MySql: sFormat = "SELECT * FROM {0} LIMIT 0"; break;
                case DatabaseDialect.PgSql: sFormat = "SELECT * FROM {0} _LIMIT_ 0"; break;
                case DatabaseDialect.LtSql: sFormat = "SELECT * FROM {0} LIMIT 0"; break;

                default:

                    Debug.Assert(false);
                    break;
            }

            DataTable dt = DataCopy.GetDataTable(conn, transaction, sFormat, sTableName);
            //dt = dt.Clone();

            return dt;
        }

        public void Dispose()
        {
            this.InsertDataTable.Dispose();
            this.UpdateDataTable.Dispose();
        }
    }

    public class DataCopy
    {
#if DEBUG
        public const int MAX_ERROR_LIST_COUNT = 100;
#endif
        public static DateTime MIN_ALLOWED_DATE = new DateTime(1999, 1, 1);

        private static SportRadar.Common.Logs.ILog m_logger = LogFactory.CreateLog(typeof(DataCopy));

        protected UpdateStatistics m_uss = new UpdateStatistics();

        protected IDbConnection m_conn = null;
        protected IDbTransaction m_transaction = null;

        protected string m_sLiveBet = null;

        protected ErrorList m_elInfo = new ErrorList();
        protected ErrorList m_elWarnings = new ErrorList();

        protected Dictionary<string, IdentityCache> m_diIdentities = new Dictionary<string, IdentityCache>();
        protected Dictionary<string, List<object>> m_diInserted = new Dictionary<string, List<object>>();
        protected Dictionary<string, List<object>> m_diUpdated = new Dictionary<string, List<object>>();

        protected static object m_objLocker = new object();

        public delegate void DelegateDataProgressBar(int? totalUpdates);

        public static event DelegateDataProgressBar UpdateProgressBarEvent = null;
        public static event DelegateDataProgressBar UpdateLanguagesEvent = null;

        public static void UpdateLanguages()
        {
            if (UpdateLanguagesEvent != null)
            {
                try
                {
                    DataCopy.UpdateLanguagesEvent(null);
                }
                catch (Exception excp)
                {
                    m_logger.ErrorFormat("Event DataCopy.UpdateLanguagesEvent() ERROR:\r\n{0}\r\n{1}",excp, excp.Message, excp.StackTrace);
                }
            }
        }

        public static void UpdateProgressBar(int? total)
        {
            if (DataCopy.UpdateProgressBarEvent != null)
            {
                try
                {
                    DataCopy.UpdateProgressBarEvent(total);
                }
                catch (Exception excp)
                {
                    m_logger.ErrorFormat("Event DataCopy.UpdateProgressBarEvent({0}) ERROR:\r\n{1}\r\n{2}", excp, total, excp.Message, excp.StackTrace);
                }
            }
        }

        public DataCopy(IDbConnection conn, IDbTransaction transaction, string sLiveBet)
        {
            Debug.Assert(conn != null);
            Debug.Assert(transaction != null);

            m_conn = conn;
            m_transaction = transaction;

            m_sLiveBet = sLiveBet;
        }

        public IDbConnection Connection
        {
            get { return m_conn; }
        }

        public IDbTransaction Transaction
        {
            get { return m_transaction; }
        }

        public UpdateStatistics Statistics
        {
            get { return m_uss; }
        }

        public ErrorList Info
        {
            get { return m_elInfo; }
        }

        public ErrorList Warnings
        {
            get { return m_elWarnings; }
        }

        public IdentityCache GetIdentityCache(string sTableName)
        {
            return m_diIdentities.ContainsKey(sTableName) ? m_diIdentities[sTableName] : null;
        }

        public List<object> GetInsertedCache(string sTableName)
        {
            return m_diInserted.ContainsKey(sTableName) ? m_diInserted[sTableName] : null;
        }

        public List<object> GetUpdatedCache(string sTableName)
        {
            return m_diUpdated.ContainsKey(sTableName) ? m_diUpdated[sTableName] : null;
        }

        public static DataTable GetDataTableWithSqlParams(string sQuery, List<IDbDataParameter> lParams)
        {
            using (var conn = ConnectionManager.GetConnection())
            {
                return GetDataTableWithSqlParams(conn, null, sQuery, lParams);
            }
        }

        public static DataTable GetDataTableWithSqlParams(IDbConnection dcn, IDbTransaction dtn, string sQuery, List<IDbDataParameter> lParams)
        {
            try
            {
                sQuery = CheckQueryByDialect(sQuery);

                using (IDbCommand cmd = SqlObjectFactory.CreateDbCommand(dcn, dtn, sQuery))
                {
                    cmd.CommandTimeout = 300;

                    foreach (IDbDataParameter prm in lParams)
                    {
                        cmd.Parameters.Add(prm);
                    }

                    using (IDisposable dsp = SqlObjectFactory.CreateDbAdapter(cmd) as IDisposable)
                    {
                        IDbDataAdapter da = dsp as IDbDataAdapter;

                        using (DataSet ds = new DataSet())
                        {
                            da.Fill(ds);

                            DataTable dtResult = ds.Tables[0];

                            ds.Tables.Clear();

                            return dtResult;
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.ErrorFormat("GetDataTableWithSqlParams() ERROR:{0}\r\n{1}\r\nfor query:\r\n{2}", excp, excp.Message,
                                     excp.StackTrace, sQuery);
            }

            return null;
        }

        public static DataTable GetDataTable(string sFormatQuery, params object[] args)
        {
            using (var conn = ConnectionManager.GetConnection())
            {
                return GetDataTable(conn, null, sFormatQuery, args);
            }
        }

        public static string CheckQueryByDialect(string sQuery)
        {
            switch (ConnectionManager.Dialect)
            {
                case DatabaseDialect.MsSql:

                    sQuery = Regex.Replace(sQuery, @"\b_Set_\b", "Set", RegexOptions.IgnoreCase);
                    return sQuery;

                case DatabaseDialect.MySql:

                    sQuery = sQuery.Replace("[", string.Empty);
                    sQuery = sQuery.Replace("]", string.Empty);
                    sQuery = Regex.Replace(sQuery, @"\bMatch\b", "`Match`", RegexOptions.IgnoreCase);
                    sQuery = Regex.Replace(sQuery, @"\bSet\b", "`Set`", RegexOptions.IgnoreCase);
                    sQuery = Regex.Replace(sQuery, @"\b_Set_\b", "Set", RegexOptions.IgnoreCase);
                    return sQuery;

                case DatabaseDialect.PgSql:

                    sQuery = sQuery.Replace("[", string.Empty);
                    sQuery = sQuery.Replace("]", string.Empty);
                    sQuery = Regex.Replace(sQuery, @"\bLimit\b", "\"Limit\"", RegexOptions.IgnoreCase);
                    sQuery = Regex.Replace(sQuery, @"\b_Limit_\b", "Limit", RegexOptions.IgnoreCase);
                    sQuery = Regex.Replace(sQuery, @"\b_Set_\b", "Set", RegexOptions.IgnoreCase);
                    return sQuery;

                case DatabaseDialect.LtSql:

                    sQuery = sQuery.Replace("[", string.Empty);
                    sQuery = sQuery.Replace("]", string.Empty);
                    sQuery = Regex.Replace(sQuery, @"\b_Set_\b", "Set", RegexOptions.IgnoreCase);
                    return sQuery;

                default:

                    Debug.Assert(false);
                    break;
            }

            return string.Empty;
        }

        private static void EnsureConnection(IDbConnection dcn)
        {
            if (dcn.State != ConnectionState.Open)
            {
                dcn.Open();
            }
        }

        public static DataTable GetDataTable(IDbConnection dcn, IDbTransaction dtn, string sFormatQuery,
                                             params object[] args)
        {
            string sQuery = sFormatQuery;

            try
            {
                EnsureConnection(dcn);

                sQuery = string.Format(sFormatQuery, args);
                sQuery = CheckQueryByDialect(sQuery);

                using (IDbCommand cmd = SqlObjectFactory.CreateDbCommand(dcn, dtn, sQuery))
                {
                    using (IDisposable dsp = SqlObjectFactory.CreateDbAdapter(cmd) as IDisposable)
                    {
                        IDbDataAdapter da = dsp as IDbDataAdapter;

                        using (DataSet ds = new DataSet())
                        {
                            da.Fill(ds);

                            DataTable dtResult = ds.Tables[0];

                            ds.Tables.Clear();

                            return dtResult;
                        }
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "GetDataTable() ERROR:\r\nQuery:\r\n{0}\r\n", sQuery);
            }

            return new DataTable();
        }

        public static object ExecuteScalar(string sQuery, params object[] args)
        {
            using (var conn = ConnectionManager.GetConnection())
            {
                return ExecuteScalar(conn, null, sQuery, args);

            }
        }

        public static object ExecuteScalar(IDbConnection conn, IDbTransaction transaction, string sFormatQuery,
                                           params object[] args)
        {
            string sQuery = sFormatQuery;

            try
            {
                EnsureConnection(conn);

                sQuery = string.Format(sFormatQuery, args);
                sQuery = CheckQueryByDialect(sQuery);

                using (IDbCommand cmd = SqlObjectFactory.CreateDbCommand(conn, transaction, sQuery))
                {
                    cmd.CommandTimeout = 300;

                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "ExecuteScalar() ERROR:\r\nQuery:\r\n{0}\r\n", sQuery);
                return null;
            }
        }

        public static object ExecuteScalarWithSqlParams(string sQuery, List<IDbDataParameter> lParams)
        {
            using (var conn = ConnectionManager.GetConnection())
            {
                return ExecuteScalarWithSqlParams(conn, null, sQuery, lParams);

            }
        }

        public static object ExecuteScalarWithSqlParams(IDbConnection conn, IDbTransaction transaction, string sQuery,
                                                        List<IDbDataParameter> lParams)
        {
            try
            {
                EnsureConnection(conn);

                sQuery = CheckQueryByDialect(sQuery);

                using (IDbCommand cmd = SqlObjectFactory.CreateDbCommand(conn, transaction, sQuery))
                {
                    cmd.CommandTimeout = 300;

                    foreach (IDbDataParameter prm in lParams)
                    {
                        cmd.Parameters.Add(prm);
                    }

                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception excp)
            {
                m_logger.ErrorFormat("ExecuteScalarWithSqlParams() ERROR:{0}\r\n{1}", excp, excp.Message, excp.StackTrace);
                throw;
            }
        }

        public static void SetNullableColumn(DataRow dr, string sColumnName, object oValue)
        {
            if (oValue == null)
            {
                dr[sColumnName.ToLowerInvariant()] = DBNull.Value;
            }
            else
            {
                dr[sColumnName.ToLowerInvariant()] = oValue;
            }
        }

        public static DataRow CreateDataRow(DataTable dtParent, object objSource, ErrorList lMissedColumns)
        {
            Type type = objSource.GetType();

            try
            {
                DataRow dr = dtParent.NewRow();

                for (int i = 0; i < dtParent.Columns.Count; i++)
                {
                    DataColumn dc = dtParent.Columns[i];

                    string sColumnName = dc.ColumnName;

                    PropertyInfo pi = type.GetProperty(sColumnName,
                                                       BindingFlags.Public | BindingFlags.Instance |
                                                       BindingFlags.IgnoreCase);

                    if (pi != null)
                    {
                        object oValue = pi.GetValue(objSource, null);

                        dr[sColumnName] = oValue == null ? DBNull.Value : oValue;

                        if (dc.DataType == typeof(DateTime) && dr[sColumnName] != DBNull.Value)
                        {
                            // Let's check each DateTime
                            DateTime dtValue = (DateTime)dr[sColumnName];

                            if (dtValue < MIN_ALLOWED_DATE)
                            {
                                // Let's correct to minimum allowed time
                                dr[sColumnName] = MIN_ALLOWED_DATE;
                            }
                        }

                        continue;
                    }

                    // Add missed column to list
                    if (!lMissedColumns.Contains(sColumnName))
                    {
                        lMissedColumns.Add(sColumnName);
                    }
                }

                return dr;
            }
            catch (Exception excp)
            {
                m_logger.ErrorFormat("Cannot create DataRow for object of type {0}:\r\n{1}\r\n{2}", excp, type, excp.Message,
                                     excp.StackTrace);
            }

            return null;
        }

        protected static long GetIdentity(PropertyInfo piIdentity, object obj)
        {
            return (long)piIdentity.GetValue(obj, null);
        }

        public static IDbCommand GenerateInsertCommand(IDbConnection connection, IDbTransaction transaction, DataTable dt, TableSpecification ts)
        {
            string sTemplate = @"
INSERT INTO
    [{0}] ({1})
VALUES
    ({2})
";

            IDbCommand dbc = SqlObjectFactory.CreateDbCommand(connection, transaction, string.Empty);
            dbc.CommandTimeout = 1000;

            List<string> lColStrings = new List<string>();
            List<string> lSetStrings = new List<string>();

            foreach (DataColumn dc in dt.Columns)
            {
                if (ts.IsAutoGeneratedIdentity && ts.IdentityNames.Contains(dc.ColumnName.ToLowerInvariant()))
                {
                    continue;
                }

                switch (ConnectionManager.Dialect)
                {
                    case DatabaseDialect.MsSql:
                    case DatabaseDialect.PgSql:

                        lColStrings.Add(string.Format("[{0}]", dc.ColumnName));
                        lSetStrings.Add(string.Format("@{0}", dc.ColumnName));
                        dbc.Parameters.Add(SqlObjectFactory.CreateParameter(dc.ColumnName, null, dc.ColumnName));

                        break;

                    case DatabaseDialect.MySql:
                    case DatabaseDialect.LtSql:

                        lColStrings.Add(dc.ColumnName);
                        lSetStrings.Add(string.Format("@{0}", dc.ColumnName));
                        dbc.Parameters.Add(SqlObjectFactory.CreateParameter(dc.ColumnName, null, dc.ColumnName));
                        break;

                    default:

                        Debug.Assert(false);
                        break;
                }
            }

            string sCmdQuery = string.Format(sTemplate, ts.TableName, string.Join(",", lColStrings.ToArray()),
                                             string.Join(",", lSetStrings.ToArray()));
            dbc.CommandText = DataCopy.CheckQueryByDialect(sCmdQuery);

            return dbc;
        }

        public static IDbCommand GenerateUpdateCommand(IDbConnection connection, IDbTransaction transaction, DataTable dt, TableSpecification ts)
        {
            string sTemplate = @"
UPDATE
    [{0}]
_SET_
{1}
WHERE
{2}
";

            IDbCommand dbc = SqlObjectFactory.CreateDbCommand(connection, transaction, string.Empty);
            dbc.CommandTimeout = 1000;

            List<string> lSetStrings = new List<string>();
            List<string> lIdentityStrings = new List<string>();

            //sIdentityName = sIdentityName.ToLowerInvariant();

            foreach (DataColumn dc in dt.Columns)
            {
                if (ts.IdentityNames.Contains(dc.ColumnName.ToLowerInvariant()))
                {
                    lIdentityStrings.Add(string.Format("    [{0}] = @{0}", dc.ColumnName));
                }
                else
                {
                    lSetStrings.Add(string.Format("    [{0}] = @{0}", dc.ColumnName));
                }

                dbc.Parameters.Add(SqlObjectFactory.CreateParameter(dc.ColumnName, null, dc.ColumnName));
            }

            string sCmdQuery = string.Format(sTemplate, ts.TableName, string.Join(",\r\n", lSetStrings.ToArray()),
                                             string.Join("\r\nAND\r\n", lIdentityStrings.ToArray()));
            dbc.CommandText = DataCopy.CheckQueryByDialect(sCmdQuery);

            return dbc;
        }

        private bool DeleteObjects<T>(LineObjectCollection<T> locLineObjects, TableSpecification ts) where T : IRemovableLineObject<T>
        {
            // DK - At the moment this method works ONLY with BIGINT type primary keys

            if (locLineObjects != null)
            {
                IdentityList il = new IdentityList();

                try
                {
                    foreach (IRemovableLineObject<T> obj in locLineObjects.Values)
                    {
                        il.AddUnique(obj.RemoveId);
                    }

                    DataCopy.ExecuteScalar(m_conn, m_transaction, "DELETE FROM {0} WHERE {1} IN ({2})", ts.TableName, ts.IdentityNames[0], il.FormatIds());

                    UpdateStatistic us = m_uss.EnsureStatistic(ts.TableName);

                    us.DeleteCount = il.Count;

                    return true;
                }
                catch (Exception excp)
                {
                    m_logger.Excp(excp, "DeleteObjects<{0}> ERROR. ObjectIds: {1}", typeof(T), il.FormatIds());
                    throw;
                }
            }

            return false;
        }

        public void InsertOrUpdate<T>(LineObjectCollection<T> locLineObjects, TableSpecification ts, UpdatesLn updatesLn) where T : ILineObject<T>
        {
            CheckTime ct = new CheckTime(false, "InsertOrUpdate for '{0}' entered", ts.TableName);

            List<object> lInserted = new List<object>();
            List<object> lUpdated = new List<object>();

            m_diInserted.Add(ts.TableName, lInserted);
            m_diUpdated.Add(ts.TableName, lUpdated);

            if (locLineObjects == null)
            {
                return;
            }

            UpdateStatistic us = m_uss.EnsureStatistic(ts.TableName);

            string sInfo = string.Format("{0} table [{1}] {2};  ", m_sLiveBet, ts.TableName, locLineObjects);

#if DEBUG
            int iInsertCount = 0;
            int iUpdateCount = 0;
#endif

            try
            {
                    ct.AddEvent("Empty DataTables created.");

                    foreach (string sKey in locLineObjects.Keys)
                    {
                        using (DataCopyTables dct = DataCopyTables.GetDataCopyTables(m_conn, m_transaction, ts.TableName))
                        {

                            T obj = locLineObjects[sKey];

                            obj.UpdateId = updatesLn.UpdateId;

                            if (obj.IsNew)
                            {
                                DataRow drNew = obj.CreateDataRow(dct.InsertDataTable);
                                dct.InsertDataTable.Rows.Add(drNew);
                                lInserted.Add(obj);
                            }
                            else
                            {
                                DataRow drNew = obj.CreateDataRow(dct.UpdateDataTable);
                                dct.UpdateDataTable.Rows.Add(drNew);
                                lUpdated.Add(obj);
                            }

#if DEBUG
                    iInsertCount = dct.InsertDataTable.Rows.Count;
                    iUpdateCount = dct.UpdateDataTable.Rows.Count;
#endif


                            if (dct.InsertDataTable.Rows.Count > 0)
                            {
                                using (IDbCommand cmdInsert = GenerateInsertCommand(m_conn, m_transaction, dct.InsertDataTable, ts))
                                {
                                    using (IDisposable dsp = SqlObjectFactory.CreateDbAdapter(null) as IDisposable)
                                    {
                                        IDbDataAdapter daInsert = dsp as IDbDataAdapter;
                                        Debug.Assert(daInsert != null);

                                        daInsert.InsertCommand = cmdInsert;

                                        dct.InsertDataTable.AcceptChanges();

                                        foreach (DataRow dr in dct.InsertDataTable.Rows)
                                        {
                                            dr.SetAdded();
                                        }

                                        using (DataSet ds = new DataSet())
                                        {
                                            ds.Tables.Add(dct.InsertDataTable);
                                            daInsert.Update(ds);
                                        }
                                    }
                                }

                                us.InsertCount = dct.InsertDataTable.Rows.Count;
                                ct.AddEvent("Insert completed ({0})", dct.InsertDataTable.Rows.Count);
                            }

                            if (dct.UpdateDataTable.Rows.Count > 0)
                            {
                                using (IDbCommand cmdUpdate = GenerateUpdateCommand(m_conn, m_transaction, dct.UpdateDataTable, ts))
                                {
                                    using (IDisposable dsp = SqlObjectFactory.CreateDbAdapter(null) as IDisposable)
                                    {
                                        IDbDataAdapter daUpdate = dsp as IDbDataAdapter;
                                        Debug.Assert(daUpdate != null);

                                        daUpdate.UpdateCommand = cmdUpdate;

                                        dct.UpdateDataTable.AcceptChanges();

                                        foreach (DataRow dr in dct.UpdateDataTable.Rows)
                                        {
                                            dr.SetModified();
                                        }

                                        using (DataSet ds = new DataSet())
                                        {
                                            ds.Tables.Add(dct.UpdateDataTable);
                                            daUpdate.Update(ds);
                                        }
                                    }
                                }

                                us.UpdateCount = dct.UpdateDataTable.Rows.Count;
                                ct.AddEvent("Update completed ({0})", dct.UpdateDataTable.Rows.Count);
                            }
                            ct.AddEvent("Insert/Update filled up (I.Cnt={0}; U.Cnt={1})", dct.InsertDataTable.Rows.Count, dct.UpdateDataTable.Rows.Count);


                        }
                    }



                    

                //Debug.Assert(us.Count == arrObjects.Length);

                //m_elInfo.AddFormat("{0} Result: Succeeded;  Inserted: {1};  Updated: {2};  Skipped; {3}", sInfo, us.InsertCount, us.UpdateCount, us.SkipCount);
            }
            catch (Exception excp)
            {
                m_elInfo.AddFormat("{0} Result: FAILED; Inserted: {1};  Updated: {2};", sInfo, us.InsertCount, us.UpdateCount);

#if DEBUG
                if (typeof(T) == typeof(TaggedStringLn))
                {
                    FindDuplucates(locLineObjects);
                }

                int iCount = 0;
                string sObjectList = string.Format("ERROR objects (Count={0})\r\n", locLineObjects.Count);

                foreach (T obj in locLineObjects.Values)
                {
                    sObjectList += obj.ToString() + "\r\n";

                    if (++iCount > MAX_ERROR_LIST_COUNT)
                    {
                        sObjectList += string.Format("And More {0} objects not listed", locLineObjects.Count - iCount);
                        break;
                    }
                }


                m_logger.Error(sObjectList,excp);
#endif
                ExcpHelper.ThrowUp(excp, "ERROR InsertOrUpdate() for {0}", locLineObjects);
            }
            finally
            {
                ct.AddEvent("InsertOrUpdate for '{0}' completed", ts.TableName);
                ct.Info(m_logger);
            }
        }

#if DEBUG

        private static void FindDuplucates<T>(LineObjectCollection<T> locLineObjects) where T : ILineObject<T>
        {
            Dictionary<string, T> di = new Dictionary<string, T>();

            foreach (T tObj in locLineObjects.Values)
            {
                TaggedStringLn str = tObj as TaggedStringLn;
                Debug.Assert(str != null);

                string sKey = string.Format("{0}*{1}*{2}", str.Category, str.Tag, str.Language);
                if (!di.ContainsKey(sKey))
                {
                    di.Add(sKey, tObj);
                }
                else
                {
                    m_logger.ErrorFormat("Duplicates found:\r\n{0}\r\n{1}",new Exception(), di[sKey], tObj);
                }
            }
        }

#endif
        /*
        private static bool DeleteMatches(IDbConnection conn, IDbTransaction transaction)
        {
            LineObjectCollection<MatchLn> locMatches = LineSr.Instance.ObjectsToRemove.GetLineObjectCollection<MatchLn>();

            if (locMatches != null && locMatches.Count > 0)
            {
                IdentityList il = new IdentityList();

                foreach (MatchLn mtch in locMatches.Values)
                {
                    il.AddUnique(mtch.MatchId);
                }

                string sMatchIds = il.FormatIds();

                DataCopy.ExecuteScalar(conn, transaction, DELETE_MATCH_ODDS_QUERY, sMatchIds);
                DataCopy.ExecuteScalar(conn, transaction, DELETE_MATCH_BETDOMAIN_QUERY, sMatchIds);
                DataCopy.ExecuteScalar(conn, transaction, DELETE_MATCH_TO_GROUP_QUERY, sMatchIds);
                DataCopy.ExecuteScalar(conn, transaction, DELETE_LIVE_MATCH_INFO_QUERY, sMatchIds);
                DataCopy.ExecuteScalar(conn, transaction, DELETE_MATCHES_QUERY, sMatchIds);

                return true;
            }

            return false;
        }
        */

        public static eFileSyncResult UpdateDatabase(IDbConnection conn, eUpdateType eut, string sProviderDescription, UpdateStatistics us)
        {
            eFileSyncResult fsr = eFileSyncResult.Failed;

            DateTime dtStart = DateTime.Now;
            string sErrorString = string.Empty;

            DataCopy dc = null;

            string sUpdateDescription = eut.ToString();

            CheckTime ct = new CheckTime("UpdateAll({0}) Entered", sUpdateDescription);

            DictionaryOfLineObjectCollectionLight dlocToModify = LineSr.Instance.NewOrChangedObjects;
            DictionaryOfLineObjectCollectionLight dlocToDelete = LineSr.Instance.ObjectsToRemove;

            try
            {
                using (IDbTransaction transaction = conn.BeginTransaction())
                {
                    dc = new DataCopy(conn, transaction, sUpdateDescription);

                    try
                    {
                        UpdatesLn updatesLn = new UpdatesLn(eut, string.Format("Saving {0} and Deleting {1} line tables", dlocToModify.Count, dlocToDelete.Count), sProviderDescription);

                        updatesLn.Save(conn, transaction);
                        Debug.Assert(updatesLn.UpdateId > 0);

                        // Insert or Update objects
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<TaggedStringLn>(), TaggedStringLn.TableSpec, updatesLn);

                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<TimeTypeLn>(), TimeTypeLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<ScoreTypeLn>(), ScoreTypeLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<BetTypeLn>(), BetTypeLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<BetDomainTypeLn>(), BetDomainTypeLn.TableSpec, updatesLn);

                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<GroupLn>(), GroupLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<CompetitorLn>(), CompetitorLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<MatchLn>(), MatchLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<CompetitorToOutrightLn>(), CompetitorToOutrightLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<LiveMatchInfoLn>(), LiveMatchInfoLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<MatchResultLn>(), MatchResultLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<MatchToGroupLn>(), MatchToGroupLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<BetDomainLn>(), BetDomainLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<ResourceRepositoryLn>(), ResourceRepositoryLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<ResourceAssignmentLn>(), ResourceAssignmentLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<CompetitorInfosLn>(), CompetitorInfosLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<MatchInfosLn>(), MatchInfosLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<TournamentInfosLn>(), TournamentInfosLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<OddLn>(), OddLn.TableSpec, updatesLn);
                        //dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<LiabilityLn>(), LiabilityLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<LanguageLn>(), LanguageLn.TableSpec, updatesLn);
                        dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<MultistringGroupLn>(), MultistringGroupLn.TableSpec, updatesLn);
                        //dc.InsertOrUpdate(dlocToModify.GetLineObjectCollection<TournamentMatchLocksLn>(), TournamentMatchLocksLn.TableSpec, updatesLn);

                        // Delete Objects
                        dc.DeleteObjects(dlocToDelete.GetLineObjectCollection<OddLn>(), OddLn.TableSpec);
                        dc.DeleteObjects(dlocToDelete.GetLineObjectCollection<BetDomainLn>(), BetDomainLn.TableSpec);
                        dc.DeleteObjects(dlocToDelete.GetLineObjectCollection<MatchToGroupLn>(), MatchToGroupLn.TableSpec);
                        dc.DeleteObjects(dlocToDelete.GetLineObjectCollection<LiveMatchInfoLn>(), LiveMatchInfoLn.TableSpec);
                        dc.DeleteObjects(dlocToDelete.GetLineObjectCollection<CompetitorToOutrightLn>(), CompetitorToOutrightLn.TableSpec);
                        dc.DeleteObjects(dlocToDelete.GetLineObjectCollection<MatchLn>(), MatchLn.TableSpec);

                        if (dc.Statistics.IsInsrtedOrUpdatedOrDeleted)
                        {
                            // DK: If You like to test data import then uncomment line bellow:
                            // throw new SystemException("Developer Exception: Always rollback transaction for test purposes");

                            transaction.Commit();
                            fsr = eFileSyncResult.Succeeded;
                            ct.AddEvent("Commited");
                        }
                        else
                        {
                            transaction.Rollback();
                            fsr = eFileSyncResult.Skipped;
                            ct.AddEvent("Rolled Back");
                        }

                        if (us != null)
                        {
                            us.Append(dc.Statistics);
                        }
                    }
                    catch (Exception excp)
                    {
                        transaction.Rollback();
                        m_logger.Excp(excp, "UpdateAllWithSqlBulkCopy General Transaction Exception");
                    }
                    finally
                    {
                        DateTime dtEnd = DateTime.Now;

                        if (fsr != eFileSyncResult.Skipped)
                        {
                            // DK: Let's write to log ONLY Succeeded or Failed file syncrhonization info.

                            string sInfo = @"
{7} UpdateAllWithSqlBulkCopy({0}) {1}

{2}


{3}

Start: {4};  
  End: {5};  
 Time: {6};
";

                            m_logger.InfoFormat(
                                sInfo, // Format
                                sProviderDescription, // Provider Description
                                fsr, // Result
                                sErrorString, // Errors if any
                                dc.Statistics, // Statistic
                                dtStart,
                                dtEnd,
                                dtEnd - dtStart, // Time 
                                sUpdateDescription); // LiveBet or PreMatch
                        }
                        else
                        {
                            m_logger.InfoFormat("{0} UpdateAllWithSqlBulkCopy Skipped. Time: {1}", sUpdateDescription, dtEnd - dtStart);
                        }

                        if (fsr == eFileSyncResult.Succeeded)
                        {
                        }

                        ct.AddEvent("UpdateAll({0}) completed", sUpdateDescription);
                        ct.Info(m_logger);
                    }
                }
                //                }
            }
            catch (Exception excp)
            {
                sErrorString += ExcpHelper.FormatException(excp, "UpdateAllWithSqlBulkCopy General Exception");
                m_logger.Error(sErrorString,excp);
            }

            return fsr;
        }
    }
}