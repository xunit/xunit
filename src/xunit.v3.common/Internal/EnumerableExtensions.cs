using System.Collections.Generic;
using System.Linq;

namespace Xunit.Internal
{
	/// <summary>
	/// Extension methods for <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Returns <paramref name="source"/> as a <see cref="List{T}"/>, using a cast when
		/// available and <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/> when not.
		/// </summary>
		public static List<T> CastOrToList<T>(this IEnumerable<T> source) =>
			source as List<T> ?? source.ToList();

		/// <summary>
		/// Returns <paramref name="source"/> as an array of <typeparamref name="T"/>, using a cast when
		/// available and <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> when not.
		/// </summary>
		public static T[] CastOrToArray<T>(this IEnumerable<T> source) =>
			source as T[] ?? source.ToArray();

		/// <summary>
		/// Returns <paramref name="source"/> as an enumerable of <typeparamref name="T"/> with
		/// all the <c>null</c> items removed.
		/// </summary>
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
			where T : class =>
				source.Where(item => item is not null).Cast<T>();
	}
}
