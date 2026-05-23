#nullable enable

#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;

namespace Xunit.Generators
{
	/// <summary>
	/// This class helps create hash codes for objects implementing <see cref="object.GetHashCode"/>.
	/// </summary>
	/// <remarks>
	/// This is a companion class to <see cref="ComparerHelper"/> in that it implements computing hash
	/// codes for all the types that can be compared.
	/// </remarks>
	internal sealed partial class HashCodeHelper
	{
		int result;

		HashCodeHelper(int start) =>
			result = start;

		/// <summary>
		/// Gets the current hash code from an instance of <see cref="HashCodeHelper"/>.
		/// </summary>
		public static implicit operator int(HashCodeHelper hasher) =>
			(hasher ?? throw new ArgumentNullException(nameof(hasher))).result;

		/// <summary>
		/// Starts a new instance of <see cref="HashCodeHelper"/> based on the hash code provided from
		/// a base class. Useful when your class derives from another.
		/// </summary>
		/// <param name="start">The hash code retrieved from calling <c>base.GetHashCode</c> inside
		/// your implementation of <see cref="object.GetHashCode"/>.</param>
		/// <returns>The new <see cref="HashCodeHelper"/> instance</returns>
		public static HashCodeHelper Extend(int start) =>
			new HashCodeHelper(start);

		/// <summary>
		/// Starts a new instance of <see cref="HashCodeHelper"/>, when you are the not derived from a base
		/// class.
		/// </summary>
		/// <returns>The new <see cref="HashCodeHelper"/> instance</returns>
		public static HashCodeHelper Start() =>
			new HashCodeHelper(64827692);

		/// <summary>
		/// Gets the current hash code.
		/// </summary>
		public int ToInt32() =>
			result;

		/// <summary>
		/// Adds the hash code for a value into the hasher result.
		/// </summary>
		/// <param name="value">The object</param>
		public HashCodeHelper With<T>(T? value)
			where T : IEquatable<T> =>
				WithObject(value);

		/// <summary>
		/// Adds the hash code for a <see cref="Nullable{T}"/> value into the hasher result.
		/// </summary>
		/// <param name="value">The object</param>
		public HashCodeHelper With<T>(T? value)
			where T : struct, IEquatable<T> =>
				WithObject(value ?? (object?)null);

		/// <summary>
		/// Adds the hash code for a string into the hasher result.
		/// </summary>
		/// <param name="value">The string</param>
		// This is here so strings don't come in as collections
		public HashCodeHelper With(string? value) =>
			WithObject(value);

		/// <summary>
		/// Adds the hash code for an collection into the hasher result, by adding all the
		/// individual items into the hash code.
		/// </summary>
		/// <typeparam name="T">The item type in the collection</typeparam>
		/// <param name="collection">The collection</param>
		public HashCodeHelper With<T>(IReadOnlyCollection<T?>? collection)
			where T : class, IEquatable<T>
		{
			WithObject("collection");

			if (collection != null)
				foreach (var value in collection)
					WithObject(value);

			return this;
		}

		/// <summary>
		/// Adds the hash code for a dictionary into the hasher result, by adding all the keys
		/// and all the values into the hash code.
		/// </summary>
		/// <typeparam name="TKey">The key type</typeparam>
		/// <typeparam name="TValue">The value type</typeparam>
		/// <param name="dictionary">The dictionary</param>
		public HashCodeHelper With<TKey, TValue>(IReadOnlyDictionary<TKey, TValue>? dictionary)
			where TKey : IEquatable<TKey>
			where TValue : IEquatable<TValue>
		{
			WithObject("dictionary");

			if (dictionary != null)
				foreach (var kvp in dictionary)
				{
					WithObject(kvp.Key);
					WithObject(kvp.Value);
				}

			return this;
		}

		/// <summary>
		/// Adds the hash code for a dictionary into the hasher result, by adding all the keys
		/// and all the hashed values into the hash code.
		/// </summary>
		/// <typeparam name="TKey">The key type</typeparam>
		/// <typeparam name="TValue">The value type</typeparam>
		/// <param name="dictionary">The dictionary</param>
		public HashCodeHelper With<TKey, TValue>(IReadOnlyDictionary<TKey, HashSet<TValue>>? dictionary)
			where TKey : IEquatable<TKey>
			where TValue : IEquatable<TValue>
		{
			WithObject("traits");

			if (dictionary != null)
				foreach (var kvp in dictionary)
				{
					WithObject(kvp.Key);
					foreach (var value in kvp.Value)
						With(value);
				}

			return this;
		}

		/// <summary>
		/// Adds the hash code for a dictionary of dictionaries, where the inner value is a hashset.
		/// </summary>
		/// <typeparam name="TOuterKey">The key type for the outer dictionary</typeparam>
		/// <typeparam name="TKey">The key type for the inner dictionary</typeparam>
		/// <typeparam name="TValue">The value type for the inner dictionary</typeparam>
		/// <param name="dictionary">The dictionary</param>
		public HashCodeHelper With<TOuterKey, TKey, TValue>(Dictionary<TOuterKey, Dictionary<TKey, HashSet<TValue>>>? dictionary)
			where TOuterKey : IEquatable<TOuterKey>
			where TKey : IEquatable<TKey>
			where TValue : IEquatable<TValue>
		{
			WithObject("dictionary-of-dictionaries");

			if (dictionary != null)
				foreach (var kvp in dictionary)
				{
					WithObject(kvp.Key);
					With(kvp.Value);
				}

			return this;
		}

		HashCodeHelper WithObject(object? value)
		{
			result = result * -1521134295 + (value?.GetHashCode() ?? 0);
			return this;
		}
	}
}
