using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified string in this instance. Quicker than IndexOf(string)
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="pattern">String to seek</param>
        /// <returns>The zero-based index position of value if that string is found, or -1 if it is not. If value is String.Empty, the return value is 0.</returns>
        public static int FastIndexOf(this string source, string pattern)
        {
            if (pattern == null) throw new ArgumentNullException();
            if (pattern.Length == 0) return 0;
            if (pattern.Length == 1) return source.IndexOf(pattern[0]);
            var limit = source.Length - pattern.Length + 1;
            if (limit < 1) return -1;

            var c0 = pattern[0];
            var c1 = pattern[1];

            var first = source.IndexOf(c0, 0, limit);
            while (first != -1)
            {

                if (source[first + 1] != c1)
                {
                    first = source.IndexOf(c0, ++first, limit - first);
                    continue;
                }

                var found = true;
                for (var j = 2; j < pattern.Length; j++)
                    if (source[first + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }

                if (found) return first;
                first = source.IndexOf(c0, ++first, limit - first);
            }
            return -1;
        }
    }
}
