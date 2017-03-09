using System;
using System.Globalization;

namespace SportRadar.Common
{
    /// <summary>
    /// Die Klasse soll eine Sammlung aller Datums- und Uhrzeitfunktionen sein
    /// Author: GMA
    /// Datum: 09.05.2007
    /// </summary>
    public class DateTimeUtils
    {
        public static DateTime DATETIMENULL = new DateTime(1900, 01, 01); //TODO: must be fixed : null must be null (after refactoring database)
        public static DateTime DATETIME1700 = new DateTime(1700, 01, 01);

        public static string DisplayNormalDate(DateTime dt, CultureInfo ci)
        {
            if (ci == null)
                return dt.ToString("d", CultureInfo.CurrentCulture);
            return dt.ToString("d", ci);
        }
        public static string DisplayTimeShort(DateTime dt, CultureInfo ci)
        {
            if (ci == null)
                return dt.ToString("t", CultureInfo.CurrentCulture);
            return dt.ToString("t", ci);
        }

    }
}
