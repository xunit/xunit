using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents a set of filters for an <see cref="XunitProject"/>.
	/// </summary>
	public partial class XunitFilters
	{
		DateTimeOffset includeCacheDataDate;
		readonly ChangeTrackingHashSet<string> includedMethods;
		List<Regex>? includeMethodRegexFilters;
		HashSet<string>? includeMethodStandardFilters;

		DateTimeOffset excludeCacheDataDate;
		readonly ChangeTrackingHashSet<string> excludedMethods;
		List<Regex>? excludeMethodRegexFilters;
		HashSet<string>? excludeMethodStandardFilters;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitFilters"/> class.
		/// </summary>
		public XunitFilters()
		{
			ExcludedTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			IncludedTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			ExcludedClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IncludedClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			excludedMethods = new ChangeTrackingHashSet<string>(StringComparer.OrdinalIgnoreCase);
			includedMethods = new ChangeTrackingHashSet<string>(StringComparer.OrdinalIgnoreCase);
			ExcludedNamespaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IncludedNamespaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Gets the set of trait filters for tests to exclude.
		/// </summary>
		public Dictionary<string, List<string>> ExcludedTraits { get; }

		/// <summary>
		/// Gets the set of trait filters for tests to include.
		/// </summary>
		public Dictionary<string, List<string>> IncludedTraits { get; }

		/// <summary>
		/// Gets the set of class filters for test classes to exclude.
		/// </summary>
		public HashSet<string> ExcludedClasses { get; }

		/// <summary>
		/// Gets the set of class filters for test classes to include.
		/// </summary>
		public HashSet<string> IncludedClasses { get; }

		/// <summary>
		/// Gets the set of method filters for tests to exclude.
		/// </summary>
		public ICollection<string> ExcludedMethods => excludedMethods;

		/// <summary>
		/// Gets the set of method filters for tests to include.
		/// </summary>
		public ICollection<string> IncludedMethods => includedMethods;

		/// <summary>
		/// Gets the set of assembly filters for tests to exclude.
		/// </summary>
		public HashSet<string> ExcludedNamespaces { get; }

		/// <summary>
		/// Gets the set of assembly filters for tests to include.
		/// </summary>
		public HashSet<string> IncludedNamespaces { get; }

		void SplitMethodFilters()
		{
			SplitIncludeMethodFilters();
			SplitExcludeMethodFilters();
		}

		void SplitIncludeMethodFilters()
		{
			if (includeCacheDataDate >= includedMethods.LastMutation)
				return;

			lock (includedMethods)
			{
				if (includeCacheDataDate >= includedMethods.LastMutation)
					return;

				var standardFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var regexFilters = new List<Regex>();

				foreach (var filter in IncludedMethods)
					if (filter.Contains("*") || filter.Contains("?"))
						regexFilters.Add(new Regex(WildcardToRegex(filter)));
					else
						standardFilters.Add(filter);

				includeMethodStandardFilters = standardFilters;
				includeMethodRegexFilters = regexFilters;
				includeCacheDataDate = includedMethods.LastMutation;
			}
		}

		void SplitExcludeMethodFilters()
		{
			if (excludeCacheDataDate >= excludedMethods.LastMutation)
				return;

			lock (excludedMethods)
			{
				if (excludeCacheDataDate >= excludedMethods.LastMutation)
					return;

				var standardFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var regexFilters = new List<Regex>();

				foreach (var filter in ExcludedMethods)
					if (filter.Contains("*") || filter.Contains("?"))
						regexFilters.Add(new Regex(WildcardToRegex(filter)));
					else
						standardFilters.Add(filter);

				excludeMethodStandardFilters = standardFilters;
				excludeMethodRegexFilters = regexFilters;
				excludeCacheDataDate = excludedMethods.LastMutation;
			}
		}

		string WildcardToRegex(string pattern) =>
			$"^{Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".")}$";

		// This class wraps HashSet<T>, tracking the last mutation date, and using itself
		// as a lock for mutation (so that we can guarantee a stable data set when transferring
		// the data into caches).
		class ChangeTrackingHashSet<T> : ICollection<T>
		{
			readonly HashSet<T> innerCollection;

			public ChangeTrackingHashSet(IEqualityComparer<T> comparer)
			{
				innerCollection = new HashSet<T>(comparer);
			}

			public int Count => innerCollection.Count;

			public bool IsReadOnly => false;

			public DateTimeOffset LastMutation { get; private set; } = DateTimeOffset.UtcNow;

			public void Add(T item)
			{
				lock (innerCollection)
				{
					LastMutation = DateTimeOffset.UtcNow;
					innerCollection.Add(item);
				}
			}

			public void Clear()
			{
				lock (innerCollection)
				{
					LastMutation = DateTimeOffset.UtcNow;
					innerCollection.Clear();
				}
			}

			public bool Contains(T item) => innerCollection.Contains(item);
			public void CopyTo(T[] array, int arrayIndex) => innerCollection.CopyTo(array, arrayIndex);
			public IEnumerator<T> GetEnumerator() => innerCollection.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => innerCollection.GetEnumerator();

			public bool Remove(T item)
			{
				lock (innerCollection)
				{
					LastMutation = DateTimeOffset.UtcNow;
					return innerCollection.Remove(item);
				}
			}
		}
	}
}
