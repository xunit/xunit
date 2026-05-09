#nullable enable

using System;
using System.Collections.Generic;

namespace Xunit.Generators
{
	/// <summary>
	/// This class helpers create implementations of <see cref="IEquatable{T}"/> for generator result classes,
	/// by offering comparison functions that require <see cref="IEquatable{T}"/> values.
	/// </summary>
	/// <remarks>
	/// The <see cref="HashCodeHelper"/> class is a companion class, designed to help implement hash codes, since the
	/// compiler will suggest that all <see cref="IEquatable{T}"/> implementations also override
	/// <see cref="object.Equals(object?)"/> and <see cref="object.GetHashCode"/>.
	/// </remarks>
	internal static class ComparerHelper
	{
		/// <summary>
		/// Compare two values that are equatable.
		/// </summary>
		/// <typeparam name="T">The type of the items</typeparam>
		/// <param name="x">The first item</param>
		/// <param name="y">The second item</param>
		/// <returns>Returns <see langword="true"/> if the values are equal; <see langword="false"/> otherwise.</returns>
		public static bool Equal<T>(
			T? x,
			T? y)
				where T : IEquatable<T> =>
					x is null ? y is null : y != null && x.Equals(y);

		/// <summary>
		/// Compare two <see cref="Nullable{T}"/> values that are equatable.
		/// </summary>
		/// <typeparam name="T">The type of the items</typeparam>
		/// <param name="x">The first item</param>
		/// <param name="y">The second item</param>
		/// <returns>Returns <see langword="true"/> if the values are equal; <see langword="false"/> otherwise.</returns>
		public static bool Equal<T>(
			T? x,
			T? y)
				where T : struct, IEquatable<T> =>
					x is null ? y is null : y != null && x.Value.Equals(y.Value);

		/// <summary>
		/// Compare two string values via case-sensitive ordinal comparison.
		/// </summary>
		/// <param name="x">The first string</param>
		/// <param name="y">The second string</param>
		/// <returns>Returns <see langword="true"/> if the strings are equal; <see langword="false"/> otherwise.</returns>
		public static bool Equal(
			string? x,
			string? y) =>
				x is null ? y is null : x.Equals(y, StringComparison.Ordinal);

		/// <summary>
		/// Compares two read-only collections of equatable items.
		/// </summary>
		/// <typeparam name="T">The type of the items</typeparam>
		/// <param name="x">The first collection</param>
		/// <param name="y">The second collection</param>
		/// <returns>Returns <see langword="true"/> if the collections are equal; <see langword="false"/> otherwise.</returns>
		public static bool Equal<T>(
			IReadOnlyCollection<T?>? x,
			IReadOnlyCollection<T?>? y)
				where T : class, IEquatable<T>
		{
			if (x is null)
				return y is null;
			if (y is null)
				return false;
			if (x.Count != y.Count)
				return false;

			var xEnumerator = x.GetEnumerator();
			var yEnumerator = y.GetEnumerator();

			while (xEnumerator.MoveNext() && yEnumerator.MoveNext())
			{
				var xCurrent = xEnumerator.Current;
				var yCurrent = yEnumerator.Current;

				if (xCurrent is null)
				{
					if (yCurrent != null)
						return false;
					continue;
				}
				if (yCurrent is null)
					return false;
				if (!xCurrent.Equals(yCurrent))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Compares two dictionaries with equatable keys of equatable values.
		/// </summary>
		/// <typeparam name="TKey">The type of the key</typeparam>
		/// <typeparam name="TValue">The type of the value</typeparam>
		/// <param name="x">The first dictionary</param>
		/// <param name="y">The second dictionary</param>
		/// <returns>Returns <see langword="true"/> if the dictionaries are equal; <see langword="false"/> otherwise.</returns>
		public static bool Equal<TKey, TValue>(
			IReadOnlyDictionary<TKey, TValue>? x,
			IReadOnlyDictionary<TKey, TValue>? y)
				where TKey : IEquatable<TKey>
				where TValue : IEquatable<TValue>
		{
			if (x is null)
				return y is null;
			if (y is null)
				return false;
			if (x.Count != y.Count)
				return false;

			foreach (var xPair in x)
			{
				if (y.TryGetValue(xPair.Key, out var yValue))
					return false;
				if (!xPair.Value.Equals(yValue))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Compares two dictionaries with equatable keys of hashsets of equatable values.
		/// </summary>
		/// <typeparam name="TKey">The type of the key</typeparam>
		/// <typeparam name="TValue">The type of the value in the value hashset</typeparam>
		/// <param name="x">The first dictionary</param>
		/// <param name="y">The second dictionary</param>
		/// <returns>Returns <see langword="true"/> if the dictionaries are equal; <see langword="false"/> otherwise.</returns>
		public static bool Equal<TKey, TValue>(
			IReadOnlyDictionary<TKey, HashSet<TValue>>? x,
			IReadOnlyDictionary<TKey, HashSet<TValue>>? y)
				where TKey : IEquatable<TKey>
				where TValue : IEquatable<TValue>
		{
			if (x is null)
				return y is null;
			if (y is null)
				return false;
			if (x.Count != y.Count)
				return false;

			foreach (var xPair in x)
			{
				if (y.TryGetValue(xPair.Key, out var yValue) || yValue is null)
					return false;
				if (xPair.Value.Count != yValue.Count)
					return false;
				foreach (var value in xPair.Value)
					if (!yValue.Contains(value))
						return false;
			}

			return true;
		}
	}
}
