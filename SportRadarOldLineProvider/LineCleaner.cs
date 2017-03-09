using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.Common.Logs;
using SportRadar.Common.Windows;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.Connection;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;

namespace SportRadar.DAL.SportRadarOldLineProvider
{
    public static class LineCleaner
    {
        public const string LINE_CLEANER_THREAD_NAME = "LineCleanerThread";
        public const int    MIN_CLEANING_TIME_PERIOD = 60;
        private const int   ONE_SEC_IN_MILLISECONDS  = 1000;

        private static ILog m_logger = LogFactory.CreateLog(typeof(LineCleaner));

        public static void Run()
        {
            ThreadHelper.RunThread(LINE_CLEANER_THREAD_NAME, LineCleanerThread,ThreadPriority.BelowNormal);
        }

        /*
        static LineCleaner()
        {
            List<string> lFiellds = new List<string>();

            foreach (var table in DatabaseManager.Schema.TableStatements.Where(x => x.Statements.Any(s => s.Statement.ToLowerInvariant().Contains("references updates"))))
            {
                var column = "UpdateID";

                // There seems to be some inconsistencies in the naming scheme, where in some cases the referencing field is update_id rather than UpdateID
                if (table.Statements.Any(x => x.Statement.Contains("update_id")))
                {
                    column = "update_id";
                }

                string sSelectQuery = string.Format("(SELECT MIN({0}) FROM {1})", column, table.TableName);
                lFiellds.Add(sSelectQuery);
            }

            string sFields = string.Join(",\r\n", lFiellds.ToArray());

            string sQueryFormat = @"

";
        }

        */
        public static void RemoveExpiredMatches()
        {
            try
            {
                resultExpireDate = DateTime.Now.AddDays(-7);
                var foundMatches = new SortableObservableCollection<IMatchVw>();
                LineSr.Instance.SearchMatches(foundMatches, "", "EN", MatchFilter2, delegate(IMatchVw m1, IMatchVw m2) { return 0; });
                foreach (var foundMatch in foundMatches)
                {
                    LineSr.Instance.RemoveMatch(foundMatch.LineObject);
                }
                var ocMatchResults = new SortableObservableCollection<MatchResultVw>();
                LineSr.Instance.SearchResults(ocMatchResults, ResultFilter, delegate(MatchResultVw m1, MatchResultVw m2) { return 0; });
                foreach (var result in ocMatchResults)
                {
                    LineSr.Instance.RemoveResult(result.LineObject);
                }


            }
            catch (Exception excp)
            {
                m_logger.Excp(excp, "RemoveExpiredMatches() error.");
            }
        }

        private static bool ResultFilter(MatchResultLn result)
        {
            if (result.StartDate.Value != null)
            {
                return false;
            }
            if (result.StartDate.Value.LocalDateTime < resultExpireDate)
            {
                return true;
            }
            return false;
        }

        private static DateTime resultExpireDate = DateTime.Now.AddDays(-7);
        private static DateTime MatchExpireDate = DateTime.Now.AddDays(-1);
        private static DateTime VirtualExpireDate = DateTime.Now.AddHours(-1);
        private static bool MatchFilter2(MatchLn match)
        {
            if (match.EndDate.Value == null)
            {
                return false;
            }
            if (match.EndDate.Value.LocalDateTime < MatchExpireDate)
            {
                return true;
            }
            if (((match.SourceType == eServerSourceType.BtrVfl || match.SourceType == eServerSourceType.BtrVhc) && match.StartDate.Value.LocalDateTime < VirtualExpireDate))
            {
                return true;
            }
            return false;
        }

        /*
        public static void CleanDatabase()
        {
            var databaseName = ConnectionManager.DatabaseName;
            var connectionString = ConnectionManager.ConnectionString;
            IDbConnection connection = null;

            try
            {
                using (connection = new NpgsqlConnection(connectionString.Replace(databaseName, ConnectionManager.SystemDatabaseName)))
                {
                    connection.Open();

                    const string selectFrom = "SELECT {1} FROM {0}";
                    const string deleteFromUpdatesWhere = "DELETE FROM Updates WHERE {0}";
                    const string updateIdNotIn = " UpdateID NOT IN ({0}) ";

                    var searchClauses = new List<string>();

                    // For the sake of simplicity and to ensure atomicity, for now we'll use detection 
                    // on the reference field instead of manually specified property
                    foreach (var table in DatabaseManager.Schema.TableStatements.Where(x => x.Statements.Any(s => s.Statement.ToLowerInvariant().Contains("references updates"))))
                    {
                        var column = "UpdateID";
                        
                        // There seems to be some inconsistencies in the naming scheme, where in some cases the referencing field is update_id rather than UpdateID
                        if (table.Statements.Any(x => x.Statement.Contains("update_id")))
                        {
                            column = "update_id";
                        }

                        var selectQuery = string.Format(selectFrom, table.TableName, column);
                        var notInQuery = string.Format(updateIdNotIn, selectQuery);
                        searchClauses.Add(notInQuery);
                    }

                    var final = string.Format(deleteFromUpdatesWhere, string.Join("AND", searchClauses));

                    DataCopy.ExecuteScalar(connection, null, final);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to execute CleanDatabase command.", e);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        */

        private static void LineCleanerThread(ThreadContext tc)
        {
            while (true)
            {
                LineSr.Instance.ClearOldData();
                //CleanDatabase();

                int iLineCleanerPeriodInSec = Math.Max(DalStationSettings.Instance.LineCleanerPeriodInSec, MIN_CLEANING_TIME_PERIOD);

                Thread.Sleep(iLineCleanerPeriodInSec * ONE_SEC_IN_MILLISECONDS);
            }
        }
    }
}
