using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Npgsql;
using SportRadar.DAL.Connection;

namespace SportRadar.DAL.CommonObjects
{
    public static class SqlObjectFactory
    {
        public static IDbCommand CreateDbCommand(IDbConnection dc, IDbTransaction dt, string sQuery)
        {
            const int COMMAND_TIMEOUT = 10000;

            switch (ConnectionManager.Dialect)
            {
                case DatabaseDialect.MsSql:

                    SqlConnection connMsSql = dc as SqlConnection;
                    SqlTransaction trcnMsSql = dt as SqlTransaction;

                    Debug.Assert(connMsSql != null);

                    SqlCommand cmdMsSql = new SqlCommand(sQuery, connMsSql, trcnMsSql);
                    cmdMsSql.CommandTimeout = COMMAND_TIMEOUT;

                    return cmdMsSql;

#if MySql
                case DatabaseDialect.MySql:

                    MySqlConnection connMySql = dc as MySqlConnection;
                    MySqlTransaction trcnMySql = dt as MySqlTransaction;

                    Debug.Assert(connMySql != null);

                    MySqlCommand cmdMySql = new MySqlCommand(sQuery, connMySql, trcnMySql);
                    cmdMySql.CommandTimeout = COMMAND_TIMEOUT;

                    return cmdMySql;
#endif
                case DatabaseDialect.PgSql:

                    NpgsqlConnection connPgSql = dc as NpgsqlConnection;
                    NpgsqlTransaction trcnPgSql = dt as NpgsqlTransaction;

                    Debug.Assert(connPgSql != null);

                    NpgsqlCommand cmdPgSql = new NpgsqlCommand(sQuery, connPgSql, trcnPgSql);
                    cmdPgSql.CommandTimeout = COMMAND_TIMEOUT;

                    return cmdPgSql;

#if SQLite
                case DatabaseDialect.LtSql:

                    SQLiteConnection  connLtSql = dc as SQLiteConnection;
                    SQLiteTransaction trcnLtSql = dt as SQLiteTransaction;

                    Debug.Assert(connLtSql != null);

                    SQLiteCommand cmdLtSql = new SQLiteCommand(sQuery, connLtSql, trcnLtSql);
                    cmdLtSql.CommandTimeout = COMMAND_TIMEOUT;

                    return cmdLtSql;
#endif

                default:

                    Debug.Assert(false);
                    break;
            }

            return null;
        }


        public static IDbDataAdapter CreateDbAdapter(IDbCommand dcSelect)
        {
            switch (ConnectionManager.Dialect)
            {
                case DatabaseDialect.MsSql:

                    SqlCommand cmdMsSql = dcSelect as SqlCommand;
                    //Debug.Assert(cmdMsSql != null);

                    return new SqlDataAdapter(cmdMsSql);

#if MySql
                case DatabaseDialect.MySql:

                    MySqlCommand cmdMySql = dcSelect as MySqlCommand;
                    //Debug.Assert(cmdMsSql != null);

                    return new MySqlDataAdapter(cmdMySql);
#endif
                case DatabaseDialect.PgSql:

                    NpgsqlCommand cmdPgSql = dcSelect as NpgsqlCommand;
                    //Debug.Assert(cmdMsSql != null);

                    return new NpgsqlDataAdapter(cmdPgSql);

#if SQLite
                case DatabaseDialect.LtSql:

                    SQLiteCommand cmdLtSql = dcSelect as SQLiteCommand;
                    Debug.Assert(cmdLtSql != null);

                    return new SQLiteDataAdapter(cmdLtSql);
#endif
                default:

                    Debug.Assert(false);
                    break;
            }

            return null;
        }

        public static IDbDataParameter CreateParameter(string sParameterName, object objValue, string sSourceColumn)
        {
            switch (ConnectionManager.Dialect)
            {
                case DatabaseDialect.MsSql:

                    SqlParameter prmMsSql = new SqlParameter();

                    prmMsSql.ParameterName = sParameterName;
                    prmMsSql.Value = objValue;
                    prmMsSql.SourceColumn = sSourceColumn;

                    return prmMsSql;

#if MySql
                case DatabaseDialect.MySql:

                    MySqlParameter prmMySql = new MySqlParameter();

                    prmMySql.ParameterName = sParameterName;
                    prmMySql.Value = objValue;
                    prmMySql.SourceColumn = sSourceColumn;

                    return prmMySql;
#endif

                case DatabaseDialect.PgSql:

                    NpgsqlParameter prmPgSql = new NpgsqlParameter();

                    prmPgSql.ParameterName = sParameterName;
                    prmPgSql.Value = objValue;
                    prmPgSql.SourceColumn = sSourceColumn;

                    return prmPgSql;

#if SQLite
                case DatabaseDialect.LtSql:

                    SQLiteParameter prmLtSql = new SQLiteParameter();

                    prmLtSql.ParameterName = sParameterName;
                    prmLtSql.Value = objValue;
                    prmLtSql.SourceColumn  = sSourceColumn;

                    return prmLtSql;
#endif
                default:

                    Debug.Assert(false);
                    break;
            }

            return null;
        }
    }
}