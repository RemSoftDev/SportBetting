using System;
using System.Windows;


namespace JHLib
{
    /// <summary>
    /// A static class for holding misc mathematical utilities
    /// </summary>
    public static class MathLib
    {
        #region IsInRange
        /// <summary>
        /// Return true if this value (x) is within the range denoted by the two parameters X1,x2 regardless of their order or whether they're negative.
        /// </summary>
        /// <param name="x">this numeric value to compare against the range limits</param>
        /// <param name="x1">one range limit (may be greater or less than the other limit)</param>
        /// <param name="x2">the other range limit</param>
        /// <returns></returns>
        public static bool IsInRange(this double x, double x1, double x2)
        {
            double lowerLimit = Math.Min(x1, x2);
            double upperLimit = Math.Max(x1, x2);
            return (x >= lowerLimit && x <= upperLimit);
        }
        #endregion

        #region IsEssentiallyEqual

        public static bool IsEssentiallyEqual(Size sizeValue1, Size sizeValue2)
        {
            bool bResult = false;
            // If both are Empty, then consider them equal
            if (sizeValue1.IsEmpty)
            {
                bResult = sizeValue2.IsEmpty;
            }
            else if (sizeValue2.IsEmpty)
            {
                bResult = sizeValue1.IsEmpty;
            }
            else
            {
                bResult = IsEssentiallyEqual(sizeValue1.Width, sizeValue2.Width) && IsEssentiallyEqual(sizeValue1.Height, sizeValue2.Height);
            }
            return bResult;
        }

        /// <summary>
        /// Return true if the two numeric values are essentially equal in value,
        /// ie within about Double.Epsilon of each other.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if they're within Double.Epsilon of each other</returns>
        public static bool IsEssentiallyEqual(this double x, double y)
        {
            return Math.Abs(x - y) <= Double.Epsilon;
            //double epsilon = 0.0001;
            //return (y - epsilon) < x && x < (y + epsilon);
        }
        #endregion

        #region IsEssentiallyEqualToOrLessThan
        /// <summary>
        /// Return true if the first numeric values is less than the second, or they're essentially equal,
        /// ie within about 0.0001 of each other.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if x is equal to or less than y</returns>
        public static bool IsEssentiallyEqualToOrLessThan(this double x, double y)
        {
            double epsilon = 0.0001;
            return x < (y + epsilon);
        }
        #endregion

        #region IsEssentiallyEqualToOrGreaterThan
        /// <summary>
        /// Return true if the first numeric values is greater than the second, or they're essentially equal,
        /// ie within about 0.0001 of each other.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if x is equal to or greater than y</returns>
        public static bool IsEssentiallyEqualToOrGreaterThan(this double x, double y)
        {
            double epsilon = 0.0001;
            return x > (y - epsilon);
        }
        #endregion

        #region GetDecimalPlaces
        /// <summary>
        /// An extension method that counts and returns the number of decimal places in this Double value
        /// </summary>
        public static int GetDecimalPlaces(this double dbValue)
        {
            const int MAX_DECIMAL_PLACES = 10;
            double THRESHOLD = Math.Pow(0.1, 10);
            if (dbValue == 0.0)
            {
                return 0;
            }
            int nDecimal = 0;
            while (dbValue - Math.Floor(dbValue) > THRESHOLD && nDecimal < MAX_DECIMAL_PLACES)
            {
                dbValue *= 10.0;
                nDecimal++;
            }
            return nDecimal;
        }
        #endregion

        #region RoundUpToNearestUnit, roundDownToNearestUnit
        /// <summary>
        /// An extension method that rounds this double value up to the nearest value in dbUnit steps.
        /// </summary>
        /// <param name="dbUnit">the step-size to round to</param>
        /// <returns>dbValue rounded up to the nearest dbUnit step</returns>
        public static double RoundUpToNearestUnit(this double dbValue, double dbUnit)
        {
            const int ROUND_DP = 10;
            double dbValInUnit = dbValue / dbUnit;
            dbValInUnit = Math.Round(dbValInUnit, ROUND_DP);
            dbValInUnit = Math.Ceiling(dbValInUnit);
            return (dbValInUnit * dbUnit);
        }

        /// <summary>
        /// An extension method that rounds this double value down to the nearest value in dbUnit steps.
        /// </summary>
        /// <param name="dbUnit">the step-size to round to</param>
        /// <returns>dbValue rounded down to the nearest dbUnit step</returns>
        public static double RoundDownToNearestUnit(this double dbValue, double dbUnit)
        {
            const int ROUND_DP = 10;
            double dbValInUnit = dbValue / dbUnit;
            dbValInUnit = Math.Round(dbValInUnit, ROUND_DP);
            dbValInUnit = Math.Floor(dbValInUnit);
            return (dbValInUnit * dbUnit);
        }
        #endregion
    }
}
