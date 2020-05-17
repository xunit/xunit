using System.Collections.Generic;
using System.Linq;

namespace Xunit.Sdk
{
    static class CollectionExtensions
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
