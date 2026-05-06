using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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


        /// <summary> Combines two dictionaries. </summary>
        /// <typeparam name="TKey"> The type used as the dictionary's key. </typeparam>
        /// <typeparam name="TValue"> The type used as the dictionary's value. </typeparam>
        /// <param name="master"> The master dictionary to add records to. </param>
        /// <param name="other"> The incoming dictionary. </param>
        /// <returns> The joined dictionary. </returns>
        public static Dictionary<TKey, TValue> Add<TKey, TValue>(this Dictionary<TKey, TValue> master, Dictionary<TKey, TValue> other)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>(master);
            foreach (KeyValuePair<TKey, TValue> item in other)
            {
                Boolean isSuccessful = result.TryAdd(item.Key, item.Value);
                if(!isSuccessful)
                {
                    GD.PushError($"Unable to add <'{item.Key?.ToString()}', '{item.Value?.ToString()}'> to collection. A key sharing the same name already exists.");
                }
            }
            return result;
        }


        /// <summary> Convert the useless array .ToString into something usable. </summary>
        /// <param name="array"> The input array. </param>
        /// <returns> A readable string of all the elements in an array. </returns>
        public static String ToArrayString(this String[] array) // TODO - Extend to use T.
        {
            return $"[{string.Join(", ", array)}]";
        }
    }
}
