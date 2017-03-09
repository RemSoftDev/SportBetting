using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Extensions
{
    public static class ExtensionMethods
    {
        public static string GetDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            return value.ToString();
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate. 
        /// Null elements are left out. If source
        /// is null, empty array is returned
        /// </summary>
        /// <param name="source">
        /// An System.Collections.Generic.IEnumerable to filter
        /// </param>
        /// <param name="predicate">
        /// A function to test each element for a condition.
        /// </param>
        /// <returns></returns>
        public static IEnumerable<TSource> WhereIgnoreNull<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            //return (source ?? new TSource[0]).Where(predicate);

            if (source == null)
            {
                return new TSource[0];
            }
            else
            {
                return source.Where(item => item != null && predicate(item));
            }
        }

        /// <summary>
        /// Returns true if the source collection is null or empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || source.Count() == 0;
        }

        /// <summary>
        /// Distincts elements in the source collection by a specified field
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source.GroupBy(x => keySelector(x)).Select(x => x.First());
        }

        /// <summary>
        /// Null elements are left out. If source
        /// is null, empty array is returned
        /// </summary>
        /// <param name="source">
        /// An System.Collections.Generic.IEnumerable to filter
        /// </param>
        /// <returns></returns>
        public static IEnumerable<TSource> IgnoreNull<TSource>(
            this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                return new TSource[0];
            }
            else
            {
                return source.Where(item => item != null);
            }
        }

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// Null elements in the source are ignored.
        /// If source collection is null, empty array is returned
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> SelectIgnoreNull<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                return new TResult[0];
            }
            else
            {
                return source.Where(item => item != null).Select(selector);
            }
        }

        /// <summary>
        /// Adds elements to the end of the same array (an existing array is resized).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="newItems"></param>
        /// <returns></returns>
        public static T[] Add<T>(this T[] items, params T[] newItems)
        {
            if (items == null)
            {
                return newItems;
            }

            if (newItems == null || newItems.Length <= 0)
            {
                return items;
            }

            int oldLen = items.Length;
            Array.Resize<T>(ref items, oldLen + newItems.Length);

            for (int i = oldLen, j = 0; i < items.Length && j < newItems.Length; i++, j++)
            {
                items[i] = newItems[j];
            }

            return items;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

    }
}
