using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Entities
{
    /// <summary>
    /// Represents a range of values limited with the From and To 
    /// properties and a flag IncludeBounds indicating whether to 
    /// include boundaries into the range calculation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Range<T> where T : IComparable<T>
    {
        public Range()
        {
            IncludeBounds = true;
        }

        public Range(T from, T to, bool includeBounds = true)
        {
            this.From = from;
            this.To = to;
            this.IncludeBounds = includeBounds;
        }

        /// <summary>
        /// Lowest range boundary
        /// </summary>
        public T From { get; set; }
        /// <summary>
        /// Highest range boundary
        /// </summary>
        public T To { get; set; }

        /// <summary>
        /// Whether to include boundaries in the range calculation
        /// </summary>
        public bool IncludeBounds { get; set; }

        /// <summary>
        /// Returns true, if the specified value is in range.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsInRange(T value)
        {
            if (IncludeBounds)
            {
                return value.CompareTo(this.From) >= 0 && value.CompareTo(this.To) <= 0;
            }
            else
            {
                return value.CompareTo(this.From) > 0 && value.CompareTo(this.To) < 0;
            }
        }
    }
}
