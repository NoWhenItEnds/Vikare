using System;
using System.Collections.Generic;
using System.Linq;

namespace Vikare.Utilities.Extensions
{
    /// <summary> Additional helper methods for working with collections. </summary>
    public static class CollectionExtensions
    {
        /// <summary> Gets a random element from an IEnumerable collection. </summary>
        /// <typeparam name="T"> The type of the elements in the collection. </typeparam>
        /// <param name="source"> The source collection. </param>
        /// <returns> A random element, or the default value of T (probably null) if the collection is empty. </returns>
        public static T? GetRandomElement<T>(this IEnumerable<T> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            // Materialise once so we don't enumerate separately.
            IList<T> list = source as IList<T> ?? source.ToList();
            return list.Count > 0 ? list[Random.Shared.Next(list.Count)] : default(T);
        }


        /// <summary> Convert the useless array .ToString into something usable. </summary>
        /// <param name="array"> The input array. </param>
        /// <returns> A readable string of all the elements in an array. </returns>
        public static String ToArrayString(this String[] array)
        {
            return $"[{string.Join(", ", array)}]";
        }
    }
}
