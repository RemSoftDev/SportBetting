using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLogger
{
    internal static class LoggerExtensions
    {
        public static bool Contains(this LoggingLevel[] levels, LoggingLevel level)
        {
            if(levels.Length == 0) return false;
            for (var i = 0; i < levels.Length; i++)
                if (levels[i] == level) return true;

            return false;
        }

        public static void ForEach<T>(this IEnumerable<T> ienumerable, Action<T> action)
        {
            foreach (var enumerable in ienumerable)
            {
                action(enumerable);
            }
        }
    }
}
