using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportBetting.WPF.Prism.Database;
using SportRadar.DAL;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;

namespace MVVMTest.Services
{
    [TestClass]
    public class DatabaseTests
    {
        [TestMethod]
        [DeploymentItem("DatabaseResources\\PgSrbsClient.config")]
        [DeploymentItem("DatabaseResources\\CreateTestMatches.config")]
        public void ATestModeMatchesCreationTest()
        {
            ConfigurationManager.AppSettings["CreateDatabase"] = "1";
            StationSettingsUtils.m_sStartupPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseTests)).Location);

            string sDatabaseResourcesFolderName = StationSettingsUtils.StartupPath;
            string sTestMatchesFileName = Path.Combine(sDatabaseResourcesFolderName, "CreateTestMatches.config");
            string CreateTestMatchesStatement = System.IO.File.ReadAllText(sTestMatchesFileName);

            DatabaseManager.DropDatabase(false);
            DatabaseManager.EnsureDatabase(false);

            try
            {
                    DataCopy.ExecuteScalar(CreateTestMatchesStatement);
            }
            catch
            {
            }

            long count;
            var sql = "select count(" + MatchLn.TableSpec.IdentityNames.First() + ") FROM " + MatchLn.TableSpec.TableName + " where updateid = 71";
            long.TryParse(DataCopy.ExecuteScalar(sql).ToString(), out count);

            Assert.IsTrue(count > 0);

            DatabaseManager.DropDatabase(false);
        }

        [TestMethod]
        [DeploymentItem("DatabaseResources\\PgSrbsClient.config")]
        public void ClearDatabase()
        {
            ConfigurationManager.AppSettings["CreateDatabase"] = "1";
            StationSettingsUtils.m_sStartupPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(DatabaseTests)).Location);
            DatabaseManager.EnsureDatabase(false);
            DatabaseCache.EnsureDatabaseCache();
            LineSr.EnsureFromCache();

            UpdateFileEntrySr updateFileEntrySr = new UpdateFileEntrySr();
            updateFileEntrySr.DataSyncCacheType = eDataSyncCacheType.Statistic.ToString();

            UpdateFileEntrySr updateFileEntrySr2 = new UpdateFileEntrySr();
            updateFileEntrySr2.DataSyncCacheType = eDataSyncCacheType.Match.ToString();


            updateFileEntrySr.Save();
            updateFileEntrySr2.Save();
            long count;
            var sql = "select count(" + UpdateFileEntrySr.TableSpec.IdentityNames.First() + ") FROM " + UpdateFileEntrySr.TableSpec.TableName + " where DataSyncCacheType = '" + eDataSyncCacheType.Statistic.ToString() + "'";
            long.TryParse(DataCopy.ExecuteScalar(sql).ToString(), out count);

            Assert.IsTrue(count > 0);


            sql = "select count(" + UpdateFileEntrySr.TableSpec.IdentityNames.First() + ") FROM " + UpdateFileEntrySr.TableSpec.TableName + " where DataSyncCacheType = '" + eDataSyncCacheType.Match.ToString() + "'";
            long.TryParse(DataCopy.ExecuteScalar(sql).ToString(), out count);

            Assert.IsTrue(count > 0);


            DbManager.Instance.DeleteOldObjects();

            sql = "select count(" + UpdateFileEntrySr.TableSpec.IdentityNames.First() + ") FROM " + UpdateFileEntrySr.TableSpec.TableName + " where DataSyncCacheType = '" + eDataSyncCacheType.Statistic.ToString() + "'";

            long.TryParse(DataCopy.ExecuteScalar(sql).ToString(), out count);


            Assert.IsTrue(count > 0);

            sql = "select count(" + UpdateFileEntrySr.TableSpec.IdentityNames.First() + ") FROM " + UpdateFileEntrySr.TableSpec.TableName + " where DataSyncCacheType = '" + eDataSyncCacheType.Match.ToString() + "'";
            long.TryParse(DataCopy.ExecuteScalar(sql).ToString(), out count);
            Assert.AreEqual(0,count);

            DatabaseManager.DropDatabase(false);
        }

    }
}
