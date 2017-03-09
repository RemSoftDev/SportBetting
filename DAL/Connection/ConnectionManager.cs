using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Npgsql;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using SportRadar.Common.Windows;

namespace SportRadar.DAL.Connection
{
    /// <summary>
    /// Supported databases dialects enumerator
    /// </summary>
    public enum DatabaseDialect
    {
        Undefined = -1,
        MsSql = 0, // Microsoft SQL Server
        MySql = 1, // MySql
        LtSql = 2, // SQLite
        PgSql = 3  // PostGreSql
    };

    public static class ConnectionManager
    {
        private static readonly ILog m_Logger = SportRadar.Common.Logs.LogFactory.CreateLog(typeof(ConnectionManager));
        private static ConcurrentDictionary<int, IDbConnection> m_diConnections = new ConcurrentDictionary<int, IDbConnection>();
        private static object m_objLocker = new object();
        private static object m_objConnectionLocker = new object();

        private static DatabaseDialect m_dd = DatabaseDialect.Undefined;
        private static string m_sDatabaseName = DalStationSettings.Instance.DatabaseName;
        private static string m_sConnectionString = DalStationSettings.Instance.ConnectionString;
        static object _connLocker = new object();
        public static ConcurrentDictionary<int, IDbConnection> Connections
        {
            get
            {
                lock (_connLocker)
                {
                    return m_diConnections;
                }
            }
        }

        static ConnectionManager()
        {
            ThreadHelper.ThreadCompleted += ThreadHelper_ThreadCompleted;
        }

        static void ThreadHelper_ThreadCompleted(ThreadContext tc)
        {
            CloseConnection(tc.ManagedThreadId);
        }

        public static void SetConnection(DatabaseDialect ddNew, string sNewDatabaseName, string sConnectionString)
        {
            ConnectionManager.CloseAll();

            m_dd = ddNew;

            string sOldDatabaseName = GetDatabaseName(sConnectionString);

            m_sDatabaseName = sNewDatabaseName;
            m_sConnectionString = sConnectionString.Replace(sOldDatabaseName, sNewDatabaseName);
        }

        public static string ConnectionString { get { return m_sConnectionString; } set { m_sConnectionString = value; } }
        public static string DatabaseName { get { return m_sDatabaseName; } set { m_sDatabaseName = value; } }

        public static string SystemDatabaseName
        {
            get
            {
                switch (ConnectionManager.Dialect)
                {
                    case DatabaseDialect.MsSql: return "master";

#if MySQL
                case DatabaseDialect.MySql: return string.Empty;
#endif

                    case DatabaseDialect.PgSql: return "postgres";

#if SQLite
                case DatabaseDialect.LtSql: return string.Empty;
#endif

                    default:

                        Debug.Assert(false);
                        break;
                }

                return string.Empty;
            }
        }

        private static IDbConnection CreateDbConnection()
        {
            switch (ConnectionManager.Dialect)
            {
                case DatabaseDialect.MsSql: return new SqlConnection(m_sConnectionString);

#if MySQL
                case DatabaseDialect.MySql: return new MySqlConnection(m_sConnectionString);
#endif

                case DatabaseDialect.PgSql:
                    var connection = new NpgsqlConnection(m_sConnectionString);
                    connection.Disposed += connection_Disposed;
                    return connection;

#if SQLite
                case DatabaseDialect.LtSql: return new SQLiteConnection(sConnectionString);
#endif

                default:

                    Debug.Assert(false);
                    break;
            }

            return null;
        }

        static void connection_Disposed(object sender, EventArgs e)
        {
            var listToRemove = new List<int>();
            foreach (var mDiConnection in Connections)
            {
                try
                {
                    if (mDiConnection.Value.State == ConnectionState.Closed)
                        listToRemove.Add(mDiConnection.Key);

                }
                catch (Exception ex)
                {

                }
            }
            foreach (var i in listToRemove)
            {
                IDbConnection value;
                Connections.TryRemove(i, out value);
            }
        }

        public static long GetLastInsertId(IDbConnection conn, IDbTransaction transaction, string sIdentityName)
        {
            switch (ConnectionManager.Dialect)
            {
#if MySQL
                case DatabaseDialect.MySql:

                    object objLastInsertId = DataCopy.ExecuteScalar(conn, transaction, "SELECT last_insert_id()");
                    return Convert.ToInt64(objLastInsertId);
#endif

                case DatabaseDialect.MsSql:
                case DatabaseDialect.PgSql:

                    object objLastInsertId = DataCopy.ExecuteScalar(conn, transaction, "SELECT lastval()");
                    return Convert.ToInt64(objLastInsertId);
#if SQLite
                case DatabaseDialect.LtSql:
#endif

                default:

                    Debug.Assert(false);
                    break;
            }

            return 0;
        }

        public static DatabaseDialect Dialect
        {
            get
            {
                lock (m_objLocker)
                {
                    if (m_dd == DatabaseDialect.Undefined)
                    {
                        try
                        {
                            m_dd = (DatabaseDialect)Enum.Parse(typeof(DatabaseDialect), DalStationSettings.Instance.DatabaseDialect);
                        }
                        catch
                        {
                            m_dd = DatabaseDialect.Undefined;

                            throw new System.Exception("SQL Error: database_dialect is UNDEFINED!!! Please specify line in App.Config '<add key=\"database_dialect\" value=\"MsSql\"/>' for Microsoft SQL or corresponding other dialect (MsSql, MySql, LtSql, PgSql)");
                        }
                    }

                    return m_dd;
                }
            }
        }

        public static IDbConnection GetConnection()
        {
            int iManagedThreadId = Thread.CurrentThread.ManagedThreadId;

            lock (m_objConnectionLocker)
            {
                try
                {
                    IDbConnection conn = Connections.ContainsKey(iManagedThreadId) ? Connections[iManagedThreadId] : null;

                    if (conn != null)
                    {
                        try
                        {
                            if (conn.State != ConnectionState.Open)
                            {
                                conn.Dispose();
                                conn = null;
                            }
                        }
                        catch
                        {
                            conn = null;
                        }
                    }

                    if (conn == null)
                    {
                        conn = ConnectionManager.CreateDbConnection();
                        conn.Open();

                        Connections[iManagedThreadId] = conn;
                    }

                    //m_Logger.Debug(ConnectionManager.InfoImp());

                    return conn;
                }
                catch (Exception excp)
                {
                    CloseAll();
                    m_Logger.ErrorFormat("GetConnection(ManagedThreadID = {0}) ERROR:\r\n{1}\r\n{2}", excp, iManagedThreadId, excp.Message, excp.StackTrace);
                    return GetConnectionSecondTry(); // try to get connection 
                }

            }
        }

        public static IDbConnection GetConnectionSecondTry()
        {
            int iManagedThreadId = Thread.CurrentThread.ManagedThreadId;

            lock (m_objConnectionLocker)
            {
                try
                {
                    IDbConnection conn = Connections.ContainsKey(iManagedThreadId) ? Connections[iManagedThreadId] : null;

                    if (conn != null)
                    {
                        try
                        {
                            if (conn.State != ConnectionState.Open)
                            {
                                conn.Dispose();
                                conn = null;
                            }
                        }
                        catch
                        {
                            conn = null;
                        }
                    }

                    if (conn == null)
                    {
                        conn = ConnectionManager.CreateDbConnection();

                        conn.Open();

                        Connections[iManagedThreadId] = conn;
                    }

                    //m_Logger.Debug(ConnectionManager.InfoImp());

                    return conn;
                }
                catch (Exception excp)
                {

                    m_Logger.ErrorFormat("GetConnection(ManagedThreadID = {0}) ERROR:\r\n{1}\r\n{2}", excp, iManagedThreadId, excp.Message, excp.StackTrace);
                    throw;
                }

            }
        }

        public delegate void DelegateProcessTransaction(IDbConnection conn, IDbTransaction transaction);

        public static void ProcessTransaction(DelegateProcessTransaction dpt)
        {
            DateTime dtStart = DateTime.Now;

            try
            {
                using (IDbConnection conn = GetConnection())
                {
                    ProcessTransaction(conn, dpt);
                }
            }
            catch (Exception excp3)
            {
                m_Logger.ErrorFormat("ProcessTransaction({0}) General ERROR:\r\n{1}\r\n{2}", excp3, dpt.Method, excp3.Message, excp3.StackTrace);
                throw;
            }
        }

        public static void ProcessTransaction(IDbConnection conn, DelegateProcessTransaction dpt)
        {
            DateTime dtStart = DateTime.Now;

            try
            {
                // Check Stack to find nested transaction
                StackTrace st = new StackTrace();

                StackFrame[] sfArray = st.GetFrames();

                MethodBase mbThis = sfArray[0].GetMethod();

                for (int i = 1; i < sfArray.Length; i++)
                {
                    MethodBase mbStack = sfArray[i].GetMethod();

                    // If nested transaction detected then throw exception
                    ExcpHelper.ThrowIf(mbThis == mbStack, "Incorrect Operation: ProcessTransaction been called inside other ProcessTransaction. We do not support nested transactions:\r\n{0}\r\n", st);
                }

                // Delcare Transaction
                IDbTransaction transaction = null;

                try
                {
                    transaction = conn.BeginTransaction();

                    // Here we execute nHibernate statements
                    dpt(conn, transaction);

                    transaction.Commit();

                    DateTime dtEnd = DateTime.Now;
                    m_Logger.DebugFormat("ProcessTransaction({0}) Succeeded at {1} (Transaction Time: {2})", dpt.Method, dtEnd, dtEnd - dtStart);
                }
                catch (System.Exception excp1)
                {
                    m_Logger.ErrorFormat("ProcessTransaction({0}) ERROR:\r\n{1}\r\n{2}", excp1, dpt.Method, excp1.Message, excp1.StackTrace);

                    if (transaction != null)
                    {
                        try
                        {
                            transaction.Rollback();
                            m_Logger.WarnFormat("ProcessTransaction({0}) is ROLLED BACK", dpt.Method);
                        }
                        catch (Exception excp2)
                        {
                            m_Logger.ErrorFormat("ProcessTransaction({0}) Rollback Exception:\r\n{1}\r\n{2}", excp2,dpt.Method, excp2.Message, excp2.StackTrace);
                        }
                    }
                }
            }
            catch (Exception excp3)
            {
                m_Logger.ErrorFormat("ProcessTransaction({0}) General ERROR:\r\n{1}\r\n{2}", excp3,dpt.Method, excp3.Message, excp3.StackTrace);
                throw;
            }
        }

        public static string GetDatabaseName(string sConnectionString)
        {
            if (!string.IsNullOrEmpty(sConnectionString))
            {
                const int PAIR_COUNT = 2;

                string sDbParamName = string.Empty;

                switch (ConnectionManager.Dialect)
                {
                    case DatabaseDialect.MySql:
                    case DatabaseDialect.MsSql:
                    case DatabaseDialect.PgSql:

                        sDbParamName = "database";
                        break;

#if SQLite
                        sDbParamName = "Data Source";
                        break;
#endif

                    default:

                        Debug.Assert(false);
                        break;
                }

                Debug.Assert(!string.IsNullOrEmpty(sDbParamName));

                string[] arrSettings = m_sConnectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string sSetting in arrSettings)
                {
                    if (sSetting.StartsWith(sDbParamName, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] arrPair = sSetting.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        ExcpHelper.ThrowIf(arrPair.Length != PAIR_COUNT, "GetDatabaseName() ERROR. Incorrect pait count ({0}) in '{1}'", arrPair.Length, sSetting);

                        return arrPair[1];
                    }
                }
            }

            return null;
        }

        public static bool IsDbServerAccesable()
        {
            IDbConnection conn = null;
            switch (ConnectionManager.Dialect)
            {
                case DatabaseDialect.MsSql: conn = new SqlConnection(m_sConnectionString);
                    break;

#if MySQL
                case DatabaseDialect.MySql: conn = new MySqlConnection(m_sConnectionString);
#endif

                    break;
                case DatabaseDialect.PgSql:
                    var t = m_sConnectionString.Replace(m_sDatabaseName, "postgres");
                    conn = new NpgsqlConnection(t);
                    break;

#if SQLite
                case DatabaseDialect.LtSql: return new SQLiteConnection(sConnectionString);
                    break;
#endif

                default:

                    Debug.Assert(false);
                    break;
            }
            try
            {
                conn.Open();
                if (conn.State != ConnectionState.Open)
                {
                    conn.Dispose();
                    return false;
                }
                else
                {
                    conn.Close();
                    conn.Dispose();
                    return true;
                }
            }
            catch (Exception excp)
            {
                m_Logger.Excp(excp, "IsDbServerAccesable(Dialect = {0}) ERROR", ConnectionManager.Dialect);
            }

            return false;
        }


        public static void CloseConnection()
        {
            int iManagedThreadId = Thread.CurrentThread.ManagedThreadId;
            CloseConnection(iManagedThreadId);

        }
        public static void CloseConnection(int threadId)
        {
            int iManagedThreadId = threadId;

            lock (m_objConnectionLocker)
            {
                try
                {
                    if (Connections.ContainsKey(iManagedThreadId))
                    {
                        IDbConnection conn = Connections[iManagedThreadId];
                        try
                        {
                            conn.Dispose();
                        }
                        catch
                        {
                        }

                        IDbConnection value;
                        Connections.TryRemove(iManagedThreadId, out value);
                    }
                }
                catch (Exception excp)
                {
                    m_Logger.ErrorFormat("CloseConnection(ManagedThreadID = {0}) ERROR:\r\n{1}\r\n{2}",excp, iManagedThreadId, excp.Message, excp.StackTrace);
                }
            }
        }

        public static bool HasConnection()
        {
            int iManagedThreadId = Thread.CurrentThread.ManagedThreadId;

            lock (m_objConnectionLocker)
            {
                try
                {
                    return Connections.ContainsKey(iManagedThreadId);
                }
                catch (Exception excp)
                {
                    m_Logger.ErrorFormat("HasConnection(ManagedThreadID = {0}) ERROR:\r\n{1}\r\n{2}", excp, iManagedThreadId, excp.Message, excp.StackTrace);
                }
            }

            return false;
        }

        public static void CloseAll()
        {
            lock (m_objConnectionLocker)
            {
                foreach (IDbConnection conn in Connections.Values)
                {
                    try
                    {
                        conn.Dispose();
                    }
                    catch
                    {
                    }
                }

                Connections.Clear();
            }
        }

        public static string Info()
        {
            lock (m_objConnectionLocker)
            {
                return InfoImp();
            }
        }

        private static string InfoImp()
        {
            List<string> lConnection = new List<string>();

            foreach (int iThreadId in Connections.Keys)
            {
                lConnection.Add(iThreadId.ToString("G"));
            }

            return string.Format("ConnectionManager {{Count = {0}, Threads = {1}}}", lConnection.Count, string.Join(",", lConnection.ToArray()));
        }
    }
}