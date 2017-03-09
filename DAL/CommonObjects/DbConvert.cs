using System;
using System.Data;

namespace SportRadar.DAL.CommonObjects
{
    public static class DbConvert
    {
        public readonly static DateTime DATETIMENULL = new DateTime(1900, 01, 01);

        public static byte[] ToByteArray(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? (byte[])obj : null;
        }
        public static string ToString(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? (string)obj : string.Empty;
        }

        public static string ToString(DataRow dr, int iIdx)
        {
            object obj = dr[iIdx];

            return obj != DBNull.Value ? (string)obj : string.Empty;
        }

        public static short ToInt16(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? Convert.ToInt16(obj) : (short)0;
        }

        public static short? ToNullableInt16(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? (short?)Convert.ToInt16(obj) : null;
        }

        public static int ToInt32(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? Convert.ToInt32(obj) : 0;
        }

        public static int? ToNullableInt32(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? (int?)Convert.ToInt32(obj) : null;
        }

        public static long ToInt64(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? Convert.ToInt64(obj) : 0;
        }

        public static long? ToNullableInt64(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? (long?)Convert.ToInt64(obj) : null;
        }

        public static bool ToBool(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? Convert.ToInt32(obj) > 0 : false;
        }

        public static decimal ToDecimal(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? Convert.ToDecimal(obj) : 0;
        }

        public static decimal? ToNullableDecimal(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? (decimal?)Convert.ToDecimal(obj) : null;
        }

        public static DateTime ToDateTime(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? Convert.ToDateTime(obj) : DbConvert.DATETIMENULL;
        }

        public static DateTime? ToNullableDateTime(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return obj != DBNull.Value ? (DateTime?)Convert.ToDateTime(obj) : null;
        }

        public static DateTimeSr ToDateTimeSr(DataRow dr, string sColumnName)
        {
            object obj = dr[sColumnName.ToLowerInvariant()];

            return new DateTimeSr(obj != DBNull.Value ? Convert.ToDateTime(obj) : DbConvert.DATETIMENULL);
        }
    }
}