using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using Npgsql;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.Common.Xml;
using SportRadar.DAL;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.CommonObjects;

namespace SportBetting.WPF.Prism.Database
{
    public class DbManager : IDatabaseManager
    {
        private static DbManager _dbManager;

        public static DbManager Instance
        {
            get { return _dbManager ?? (_dbManager = new DbManager()); }
        }

        public string Version
        {
            get
            {
                var version = "";
                try
                {
                    using (var conn2 = ConnectionManager.GetConnection())
                    {
                        version = (string)DataCopy.ExecuteScalar(conn2, null, "Select version from version where id = 1");
                    }
                }
                catch (Exception)
                {

                }
                return version;


            }
        }

        public void SetVesrion(string version)
        {
            try
            {
                using (var conn2 = ConnectionManager.GetConnection())
                {
                    DataCopy.ExecuteScalar(conn2, null, "Insert into version (id,version) values (1,'{0}')", version);
                }
            }
            catch (Exception)
            {

            }
        }

        private static ILog m_logger = LogFactory.CreateLog(typeof(DbManager));

        public void EnsureDatabase(bool isTestMode)
        {
            DatabaseManager.EnsureDatabase(isTestMode);
            if (isTestMode)
            {
                CreateTestMatches();
            }
            DatabaseCache.EnsureDatabaseCache();
            LineSr.EnsureFromCache();


        }

        public void DropDatabase(bool isTestMode)
        {
            DatabaseManager.DropDatabase(isTestMode);
        }

        public bool DeleteOldObjects()
        {
            try
            {
                using (IDbConnection conn = new NpgsqlConnection(DalStationSettings.Instance.ConnectionString.Replace(DalStationSettings.Instance.DatabaseName, ConnectionManager.SystemDatabaseName)))
                {
                    conn.Open();
                    if (DatabaseManager.DoesDatabaseExist(conn, DalStationSettings.Instance.DatabaseName) == DatabaseManager.eExistResult.Exists)
                        if (DatabaseManager.OneOfRequiredTablesExist() != DatabaseManager.eExistResult.DoesNotExist)
                        {
                            ExcpHelper.ThrowIf(DatabaseManager.DeleteFromTables(), "DeleteOldObjects() ERROR. Cannot clear tables.");
                            ExcpHelper.ThrowIf(!DatabaseManager.DropTables(), "DeleteOldObjects() ERROR. Cannot drop tables.");
                        }
                        else
                        {
                            using (var conn2 = ConnectionManager.GetConnection())
                            {

                                //foreach (var sqlDelete in DatabaseManager.Schema.DeleteFromTablesStatement.Split(';'))
                                //{
                                //    DataCopy.ExecuteScalar(conn2, null, sqlDelete);
                                //}

                                DataCopy.ExecuteScalar(conn2, null, "Delete From UpdateFileEntry Where DataSyncCacheType not in('" + eDataSyncCacheType.Resources + "','" + eDataSyncCacheType.Statistic + "')");


                                foreach (var sqlDelete in DatabaseManager.Schema.DropTablesStatement.Split(';'))
                                {
                                    DataCopy.ExecuteScalar(conn2, null, sqlDelete);
                                }


                            }



                        }
                }

                return true;
            }
            catch (Exception e)
            {
                m_logger.Error(e.Message, e);
            }

            return false;
        }

        public void CreateTestMatches()
        {
            string sDatabaseResourcesFolderName = StationSettingsUtils.StartupPath;
            string sTestMatchesFileName = Path.Combine(sDatabaseResourcesFolderName, "DatabaseResources\\CreateTestMatches.config");


            string CreateTestMatchesStatement = System.IO.File.ReadAllText(sTestMatchesFileName);

            //XmlDocument TestMatchesDocument = new XmlDocument();
            //TestMatchesDocument.Load(sTestMatchesFileName);

            //string CreateTestMatchesStatement = XmlHelper.GetElementInnerText(TestMatchesDocument, "configuration/createTestSoccerMatch");

            try
            {
                using (var conn2 = ConnectionManager.GetConnection())
                {
                    using (IDbTransaction transaction = conn2.BeginTransaction())
                    {
                        DataCopy.ExecuteScalar(conn2, transaction, CreateTestMatchesStatement);
                        transaction.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                m_logger.Error(e.Message, e);
            }
        }
    }
}
