using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using Npgsql;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.Common.Xml;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.Connection
{
    public static class DatabaseManager
    {
        private static ILog m_logger = LogFactory.CreateLog(typeof(DatabaseManager));

        private static DatabaseSchema m_ds = null;
        // ALTER DATABASE "SRBS_Client" RENAME TO "SRBS_Client_BAK"

        public enum eExistResult
        {
            DoesNotExist = 0,
            Exists = 1,
            Error = 2
        }

        public static eExistResult DoesDatabaseExist(IDbConnection conn, string sDatabaseName)
        {
            try
            {
                object objCount = DataCopy.ExecuteScalar(conn, null, DatabaseManager.Schema.DatabaseExistsStatement, sDatabaseName.ToLowerInvariant());
                int iCount = Convert.ToInt32(objCount);

                m_logger.InfoFormat("DatabaseManager.DoesDatabaseExist(DatabaseName='{0}') result = {1}", sDatabaseName, iCount > 0 ? "Exists" : "DOES NOT Exist");

                return iCount > 0 ? eExistResult.Exists : eExistResult.DoesNotExist;
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "DoesDatabaseExist(DatabaseName='{0}') ERROR", sDatabaseName);
            }

            return eExistResult.Error;
        }

        public static eExistResult OneOfRequiredTablesExist()
        {
            using (var conn = ConnectionManager.GetConnection())
            {
                return OneOfRequiredTablesExist(conn, null);
            }
        }

        public static bool DeleteFromTables()
        {

            bool bResult = false;

            ConnectionManager.ProcessTransaction(delegate(IDbConnection conn, IDbTransaction transaction)
            {
                try
                {
                    DataCopy.ExecuteScalar(conn, transaction, DatabaseManager.Schema.DeleteFromTablesStatement);
                    bResult = true;
                }
                catch (Exception excp)
                {
                    m_logger.Error(ExcpHelper.FormatException(excp, "DeleteFromTables() ERROR"),excp);
                    throw;
                }
            });

            return bResult;
        }

        public static eExistResult OneOfRequiredTablesExist(IDbConnection conn, IDbTransaction transaction)
        {
            try
            {
                string[] arrRequiredTables = DatabaseManager.Schema.RequiredTables.Split(new char[] { ',', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string sRequiredTableName in arrRequiredTables)
                {
                    eExistResult err = DoesTableExist(conn, sRequiredTableName);
                    ExcpHelper.ThrowIf(err == eExistResult.Error, "Cannot recognize if table '{0}' exists.", sRequiredTableName);

                    if (err == eExistResult.Exists)
                    {
                        return eExistResult.Exists;
                    }
                }

                return eExistResult.DoesNotExist;
            }
            catch (Exception excp)
            {
                m_logger.Error(ExcpHelper.FormatException(excp, "OneOfRequiredTablesExist() ERROR"),excp);
            }

            return eExistResult.Error;
        }

        public static bool DropTables()
        {

            bool bResult = false;

            ConnectionManager.ProcessTransaction(delegate(IDbConnection conn, IDbTransaction transaction)
            {
                try
                {
                    DataCopy.ExecuteScalar(conn, transaction, DatabaseManager.Schema.DropTablesStatement);
                    bResult = true;
                }
                catch (Exception excp)
                {
                    m_logger.Error(ExcpHelper.FormatException(excp, "DropTables() ERROR"),excp);
                    throw;
                }
            });

            return bResult;
        }
        public static void DropDatabase(bool isTestMode)
        {

            string sDatabaseName = ConfigurationManager.AppSettings["database_name"];
            string sConnectionString = ConfigurationManager.AppSettings["database_connection_string"];

            if (isTestMode)
            {
                sConnectionString.Replace(sDatabaseName, "test_" + sDatabaseName);
                sDatabaseName = "test_" + sDatabaseName;
            }

            try
            {
                using (IDbConnection conn = new NpgsqlConnection(sConnectionString.Replace(sDatabaseName, ConnectionManager.SystemDatabaseName)))
                {
                    //conn.Open();

                    eExistResult eerDatabase = DatabaseManager.DoesDatabaseExist(conn, sDatabaseName);
                    ExcpHelper.ThrowIf(eerDatabase == eExistResult.Error, "Cannot recognize if database '{0}' exists.", sDatabaseName);

                    if (eerDatabase == eExistResult.Exists)
                    {
                        try
                        {
                            DataCopy.ExecuteScalar(conn, null, "select pg_terminate_backend(procpid) from pg_stat_activity where datname='{0}';", sDatabaseName);
                            using (IDbConnection conn2 = new NpgsqlConnection(sConnectionString.Replace(sDatabaseName, ConnectionManager.SystemDatabaseName)))
                            {
                                DataCopy.ExecuteScalar(conn2, null, DatabaseManager.Schema.DeleteDatabaseStatement, sDatabaseName);
                            }
                            m_logger.InfoFormat("Database '{0}' Successfully deleted", sDatabaseName);
                        }
                        catch (Exception excp)
                        {
                            m_logger.Error(ExcpHelper.FormatException(excp, "EnsureDatabase() ERROR - cannot delete {0} Database '{1}'", ConnectionManager.Dialect, sDatabaseName),excp);
                            throw;
                        }
                    }
                }
            }
            catch (System.Exception excpInner)
            {
                string sError = string.Format("Cannot verify PostGreSQL Server. Either Server is not installed or invalid priveleges ({0}).\r\n{1}\r\n{2}", sConnectionString, excpInner.Message, excpInner.StackTrace);
                //MessageBox.Show(sError, "Station Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception(sError, excpInner);
            }
        }

        public static eExistResult DoesTableExist(IDbConnection conn, string sTableName)
        {
            try
            {
                object objCount = DataCopy.ExecuteScalar(conn, null, DatabaseManager.Schema.TableExistsStatement, sTableName.ToLowerInvariant());
                int iCount = Convert.ToInt32(objCount);

                m_logger.InfoFormat("DatabaseManager.DoesTableExist(TableName='{0}') result = {1}", sTableName, iCount > 0 ? "Exists" : "DOES NOT Exist");

                return iCount > 0 ? eExistResult.Exists : eExistResult.DoesNotExist;
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "DoesTableExist(TableName='{0}') ERROR", sTableName);
            }

            return eExistResult.Error;
        }

        /***
         *  Try time to time reconect to database if lucks return true , else false
         *  timeout - ms
        ***/
        public static bool WaitForDatabaseServiceStarts(int timeout)
        {
            for (int i = 0; i < 10; i++)
            {
                if (!ConnectionManager.IsDbServerAccesable())
                {
                    Thread.Sleep(timeout / 10);
                    continue;
                }
                return true;
            }
            return false;
        }

        public static void EnsureDatabase(bool isTestMode = false)
        {
            if (DalStationSettings.Instance.CreateDatabase)
            {
                EnsureDatabaseImp(isTestMode);
            }
        }

        private static void EnsureDatabaseImp(bool isTestMode)
        {
            string sDatabaseName = "";
            string sConnectionString = "";
            if (isTestMode)
            {
                sDatabaseName = "test_" + ConnectionManager.DatabaseName;
                sConnectionString = ConnectionManager.ConnectionString.Replace(ConnectionManager.DatabaseName, sDatabaseName);
                ConnectionManager.DatabaseName = sDatabaseName;
                ConnectionManager.ConnectionString = sConnectionString;
            }
            else
            {
                sDatabaseName = ConnectionManager.DatabaseName;
                sConnectionString = ConnectionManager.ConnectionString;
            }
            

            try
            {
                int counter = 0;
                while (true)
                {
                    try
                    {
                        using (IDbConnection conn = new NpgsqlConnection(sConnectionString.Replace(sDatabaseName, ConnectionManager.SystemDatabaseName)))
                        {
                            conn.Open();
                        }
                        break;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1000);
                        counter++;
                        if (counter > 15)
                            throw;
                    }

                }

                using (IDbConnection conn = new NpgsqlConnection(sConnectionString.Replace(sDatabaseName, ConnectionManager.SystemDatabaseName)))
                {
                    conn.Open();

                    eExistResult eerDatabase = DatabaseManager.DoesDatabaseExist(conn, sDatabaseName);
                    ExcpHelper.ThrowIf(eerDatabase == eExistResult.Error, "Cannot recognize if database '{0}' exists.", sDatabaseName);

                    if (eerDatabase == eExistResult.DoesNotExist)
                    {
                        try
                        {
                            DataCopy.ExecuteScalar(conn, null, DatabaseManager.Schema.CreateDatabaseStatement, sDatabaseName);
                            m_logger.InfoFormat("Database '{0}' Successfully created", sDatabaseName);
                        }
                        catch (Exception excp)
                        {
                            m_logger.Excp(excp, "EnsureDatabase() ERROR - cannot create {0} Database '{1}'", ConnectionManager.Dialect, sDatabaseName);
                            throw;
                        }
                    }
                }

                foreach (TableStatement ts in DatabaseManager.Schema.TableStatements)
                {
                    using (IDbConnection conn = ConnectionManager.GetConnection())
                    {

                        eExistResult eerTable = DoesTableExist(conn, ts.TableName);
                        ExcpHelper.ThrowIf(eerTable == eExistResult.Error, "Cannot recognize if table '{0}' exists.", ts.TableName);

                        foreach (SqlStatement statement in ts.Statements)
                        {
                            if (statement.SqlExecuteType == eSqlExecuteType.Always || // Always
                                (eerTable == eExistResult.Exists && statement.SqlExecuteType == eSqlExecuteType.Exists) || // Table exists
                                (eerTable == eExistResult.DoesNotExist && statement.SqlExecuteType == eSqlExecuteType.DoesNotExist)) // Table does not exist
                            {
                                try
                                {
                                    DataCopy.ExecuteScalar(statement.Statement);
                                    m_logger.InfoFormat("{0} Successfully executed", statement);
                                }
                                catch (Exception excp)
                                {
                                    m_logger.Excp(excp, "EnsureDatabase() ERROR - cannot execute {0} for {1} Database '{2}'", statement, ConnectionManager.Dialect, sDatabaseName);

                                    throw;
                                }
                            }
                            else
                            {
                                m_logger.InfoFormat("{0} is skipped. Table {1}.", statement, eerTable);
                            }
                        }
                    }
                }

            }
            catch (System.Exception excpInner)
            {
                string sError = string.Format("Cannot verify Server. Either Server is not installed or invalid priveleges ({0}).\r\n{1}\r\n{2}", sConnectionString, excpInner.Message, excpInner.StackTrace);
                //MessageBox.Show(sError, "Station Start Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception(sError, excpInner);
            }
        }

        public static string GetManifestResourceStringByName(string sResourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
#if DEBUG
            string[] arrNames = asm.GetManifestResourceNames();
#endif
            using (Stream s = asm.GetManifestResourceStream(sResourceName))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static DatabaseSchema Schema
        {
            get
            {
                if (m_ds == null)
                {
                    string sDatabaseResourcesFolderName = StationSettingsUtils.StartupPath;
                    string sConfigFileName = Path.Combine(sDatabaseResourcesFolderName, DalStationSettings.Instance.DatabaseSchemaConfig);

                    m_ds = new DatabaseSchema(DalStationSettings.Instance.DatabaseName, sConfigFileName);
                    m_ds.Initialize();
                }

                return m_ds;
            }
        }
    }

    public enum eSqlExecuteType
    {
        None = 0,
        DoesNotExist = 1,
        Exists = 2,
        Always = 3
    }

    public class SqlStatement
    {
        public TableStatement ParentStatement { get; protected set; }
        public eSqlExecuteType SqlExecuteType { get; protected set; }
        public string Statement { get; protected set; }

        public SqlStatement(TableStatement tsParent, XmlNode nodeStatement)
        {
            this.ParentStatement = tsParent;
            this.SqlExecuteType = (eSqlExecuteType)Enum.Parse(typeof(eSqlExecuteType), XmlHelper.GetElementInnerText(nodeStatement, "@type"));
            this.Statement = nodeStatement.InnerText;
        }

        public override string ToString()
        {
            return string.Format("SqlStatement {{TableName='{0}'; SqlExecuteType={1};}}", this.ParentStatement.TableName, this.SqlExecuteType);
        }
    }

    public class SqlStatementList : List<SqlStatement>
    {
        public SqlStatementList(TableStatement tsParent)
        {
            XmlNodeList xnlSqlStatements = tsParent.Node.SelectNodes("statement");

            foreach (XmlNode nodeStatement in xnlSqlStatements)
            {
                this.Add(new SqlStatement(tsParent, nodeStatement));
            }
        }
    }

    public class TableStatement
    {
        public XmlNode Node { get; protected set; }
        public string TableName { get; protected set; }
        public SqlStatementList Statements { get; protected set; }

        public TableStatement(XmlNode nodeTable)
        {
            this.Node = nodeTable;
            this.TableName = XmlHelper.GetElementInnerText(nodeTable, "@name");

            this.Statements = new SqlStatementList(this);
        }

        public override string ToString()
        {
            return string.Format("TableStatement {{ TableName='{0}'; StatementsCount={1};}}", this.TableName, this.Statements.Count);
        }
    }

    public class TableStatementList : List<TableStatement>
    {
        public TableStatementList(XmlNode nodeTables)
        {
            XmlNodeList xnlTables = nodeTables.SelectNodes("configuration/tables/table");

            foreach (XmlNode nodeSqlStatement in xnlTables)
            {
                this.Add(new TableStatement(nodeSqlStatement));
            }
        }
    }

    public class DatabaseSchema
    {
        private static readonly ILog m_logger = LogFactory.CreateLog(typeof(DatabaseSchema));
        private string _dropTablesStatement;


        public DatabaseSchema(string sDatabaseName, string sConfigFileName)
        {
            this.DatabaseName = sDatabaseName;
            this.ConfigFileName = sConfigFileName;
        }

        public void Initialize()
        {
            try
            {
                ExcpHelper.ThrowIf(!File.Exists(this.ConfigFileName), "Database Configuration File does not exist: '{0}'", this.ConfigFileName);

                this.ConfigDocument = new XmlDocument();
                this.ConfigDocument.Load(this.ConfigFileName);

                this.CreateDatabaseStatement = XmlHelper.GetElementInnerText(this.ConfigDocument, "configuration/createDatabase");
                this.DeleteDatabaseStatement = XmlHelper.GetElementInnerText(this.ConfigDocument, "configuration/deleteDatabase");
                this.DatabaseExistsStatement = XmlHelper.GetElementInnerText(this.ConfigDocument, "configuration/databaseExists");
                this.TableExistsStatement = XmlHelper.GetElementInnerText(this.ConfigDocument, "configuration/tableExists");
                this.RequiredTables = XmlHelper.GetElementInnerText(this.ConfigDocument, "configuration/requiredTables");
                this.DeleteFromTablesStatement = XmlHelper.GetElementInnerText(this.ConfigDocument, "configuration/deleteFromTables");
                this.DropTablesStatement = XmlHelper.GetElementInnerText(this.ConfigDocument, "configuration/dropTables");

                this.TableStatements = new TableStatementList(this.ConfigDocument);
            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "DatabaseSchema.Initialize (DatabaseName='{0}', ConfigFileName='{1}') ERROR", this.DatabaseName, this.ConfigFileName);

                this.ConfigDocument = null;
                this.TableExistsStatement = null;
                throw;
            }
        }

        public string DropTablesStatement
        {
            get { return _dropTablesStatement; }
            set { _dropTablesStatement = value; }
        }

        public string DeleteFromTablesStatement { get; set; }

        public string RequiredTables { get; set; }

        public string DatabaseName { get; protected set; }
        public string ConfigFileName { get; protected set; }
        public XmlDocument ConfigDocument { get; protected set; }
        public TableStatementList TableStatements { get; protected set; }

        public string CreateDatabaseStatement { get; protected set; }
        public string DeleteDatabaseStatement { get; protected set; }
        public string DatabaseExistsStatement { get; protected set; }
        public string TableExistsStatement { get; protected set; }
    }
}