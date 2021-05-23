using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Xunit.Internal
{
	/// <summary>
	/// Extension methods for <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class EnumerableExtensions
	{
		static readonly Func<object, bool> notNullTest = x => x is not null;

		/// <summary>
		/// Returns <paramref name="source"/> as an array of <typeparamref name="T"/>, using a cast when
		/// available and <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> when not.
		/// </summary>
		[return: NotNullIfNotNull("source")]
		public static T[]? CastOrToArray<T>(this IEnumerable<T>? source) =>
			source == null ? null : source as T[] ?? source.ToArray();

		/// <summary>
		/// Returns <paramref name="source"/> as a <see cref="List{T}"/>, using a cast when
		/// available and <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/> when not.
		/// </summary>
		[return: NotNullIfNotNull("source")]
		public static List<T>? CastOrToList<T>(this IEnumerable<T>? source) =>
			source == null ? null : source as List<T> ?? source.ToList();

		/// <summary>
		/// Returns <paramref name="source"/> as an <see cref="IReadOnlyCollection{T}"/> of <typeparamref name="T"/>,
		/// using a cast when available and <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> when not.
		/// </summary>
		[return: NotNullIfNotNull("source")]
		public static IReadOnlyCollection<T>? CastOrToReadOnlyCollection<T>(this IEnumerable<T>? source) =>
			source == null ? null : source as IReadOnlyCollection<T> ?? source.ToArray();

		/// <summary>
		/// Returns <paramref name="source"/> as an <see cref="IReadOnlyList{T}"/> of <typeparamref name="T"/>,
		/// using a cast when available and <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> when not.
		/// </summary>
		[return: NotNullIfNotNull("source")]
		public static IReadOnlyList<T>? CastOrToReadOnlyList<T>(this IEnumerable<T>? source) =>
			source == null ? null : source as IReadOnlyList<T> ?? source.ToArray();

		/// <summary>
		/// Returns <paramref name="source"/> as an enumerable of <typeparamref name="T"/> with
		/// all the <c>null</c> items removed.
		/// </summary>
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
			where T : class =>
				source.Where((Func<T?, bool>)notNullTest)!;
	}
}
