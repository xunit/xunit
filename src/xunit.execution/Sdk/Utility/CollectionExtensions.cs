using System.Linq;
using System.Collections.Generic;

namespace Xunit.Sdk
{
    internal static class CollectionExtensions
    {
        public static List<T> CastOrToList<T>(this IEnumerable<T> source)
        {
            return source as List<T> ?? source.ToList();
        }

        public static T[] CastOrToArray<T>(this IEnumerable<T> source)
        {
            return source as T[] ?? source.ToArray();
        }
    }
}
