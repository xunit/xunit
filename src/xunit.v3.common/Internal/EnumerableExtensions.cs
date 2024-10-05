using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class EnumerableExtensions
{
	static readonly Func<object, bool> notNullTest = x => x is not null;

	/// <summary>
	/// Returns <paramref name="source"/> as an array of <typeparamref name="T"/>, using a cast when
	/// available and <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> when not.
	/// </summary>
	[return: NotNullIfNotNull(nameof(source))]
	public static T[]? CastOrToArray<T>(this IEnumerable<T>? source) =>
		source is null ? null : source as T[] ?? source.ToArray();

	/// <summary>
	/// Returns <paramref name="source"/> as a <see cref="List{T}"/>, using a cast when
	/// available and <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/> when not.
	/// </summary>
	[return: NotNullIfNotNull(nameof(source))]
	public static List<T>? CastOrToList<T>(this IEnumerable<T>? source) =>
		source is null ? null : source as List<T> ?? source.ToList();

	/// <summary>
	/// Returns <paramref name="source"/> as an <see cref="IReadOnlyCollection{T}"/> of <typeparamref name="T"/>,
	/// using a cast when available and <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> when not.
	/// </summary>
	[return: NotNullIfNotNull(nameof(source))]
	public static IReadOnlyCollection<T>? CastOrToReadOnlyCollection<T>(this IEnumerable<T>? source) =>
		source is null ? null : source as IReadOnlyCollection<T> ?? source.ToArray();

	/// <summary>
	/// Returns <paramref name="source"/> as an <see cref="IReadOnlyList{T}"/> of <typeparamref name="T"/>,
	/// using a cast when available and <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> when not.
	/// </summary>
	[return: NotNullIfNotNull(nameof(source))]
	public static IReadOnlyList<T>? CastOrToReadOnlyList<T>(this IEnumerable<T>? source) =>
		source is null ? null : source as IReadOnlyList<T> ?? source.ToArray();

	/// <summary>
	/// Enumerates all values in a collection, calling the callback for each.
	/// </summary>
	public static void ForEach<T>(
		this IEnumerable<T> source,
		Action<T> callback)
	{
		Guard.ArgumentNotNull(source);
		Guard.ArgumentNotNull(callback);

		foreach (var value in source)
			callback(value);
	}

	/// <summary>
	/// Returns <paramref name="source"/> as an enumerable of <typeparamref name="T"/> with
	/// all the <c>null</c> items removed.
	/// </summary>
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
		where T : class =>
			source.Where((Func<T?, bool>)notNullTest)!;

	/// <summary>
	/// Returns <paramref name="source"/> as an enumerable of <typeparamref name="T"/> with
	/// all the <c>null</c> items removed.
	/// </summary>
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
		where T : struct =>
			source.Where(x => x.HasValue).Select(x => x!.Value);

	/// <summary>
	/// Returns <paramref name="source"/> with all the <c>null</c> or whitespace-only strings removed.
	/// </summary>
	public static IEnumerable<string> WhereNotNullOrWhitespace(this IEnumerable<string?> source) =>
		source.Where(s => !string.IsNullOrWhiteSpace(s))!;

}
