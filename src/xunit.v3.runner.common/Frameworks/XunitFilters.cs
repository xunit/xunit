using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents a set of filters for an <see cref="XunitProject"/>.
	/// </summary>
	public class XunitFilters
	{
		DateTimeOffset includedMethodCacheLastUpdated;
		List<Regex> includedMethodRegexFilters = new();
		HashSet<string> includedMethodStandardFilters = new();
		readonly ChangeTrackingHashSet<string> includedMethods = new(StringComparer.OrdinalIgnoreCase);

		DateTimeOffset excludedMethodCacheLastUpdated;
		List<Regex> excludedMethodRegexFilters = new();
		HashSet<string> excludedMethodStandardFilters = new();
		readonly ChangeTrackingHashSet<string> excludedMethods = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets a flag which indicates if the filter list is empty (no filters).
		/// </summary>
		public bool Empty =>
			ExcludedClasses.Count == 0 &&
			ExcludedMethods.Count == 0 &&
			ExcludedNamespaces.Count == 0 &&
			ExcludedTraits.Count == 0 &&
			IncludedClasses.Count == 0 &&
			IncludedMethods.Count == 0 &&
			IncludedNamespaces.Count == 0 &&
			IncludedTraits.Count == 0;

		/// <summary>
		/// Gets the set of class filters for test classes to exclude.
		/// </summary>
		public HashSet<string> ExcludedClasses { get; } = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the set of method filters for tests to exclude.
		/// </summary>
		public ICollection<string> ExcludedMethods => excludedMethods;

		/// <summary>
		/// Gets the set of assembly filters for tests to exclude.
		/// </summary>
		public HashSet<string> ExcludedNamespaces { get; } = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the set of trait filters for tests to exclude.
		/// </summary>
		public Dictionary<string, List<string>> ExcludedTraits { get; } = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the set of class filters for test classes to include.
		/// </summary>
		public HashSet<string> IncludedClasses { get; } = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the set of method filters for tests to include.
		/// </summary>
		public ICollection<string> IncludedMethods => includedMethods;

		/// <summary>
		/// Gets the set of assembly filters for tests to include.
		/// </summary>
		public HashSet<string> IncludedNamespaces { get; } = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the set of trait filters for tests to include.
		/// </summary>
		public Dictionary<string, List<string>> IncludedTraits { get; } = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Filters the given method using the defined filter values.
		/// </summary>
		/// <param name="testCase">The test case to filter.</param>
		/// <returns>Returns <c>true</c> if the test case passed the filter; returns <c>false</c> otherwise.</returns>
		public bool Filter(_TestCaseDiscovered testCase)
		{
			Guard.ArgumentNotNull(nameof(testCase), testCase);

			SplitFilters();

			if (!FilterIncludedNamespaces(testCase))
				return false;
			if (!FilterExcludedNamespaces(testCase))
				return false;
			if (!FilterIncludedClasses(testCase))
				return false;
			if (!FilterExcludedClasses(testCase))
				return false;
			if (!FilterIncludedMethods(testCase))
				return false;
			if (!FilterExcludedMethods(testCase))
				return false;
			if (!FilterIncludedTraits(testCase))
				return false;
			if (!FilterExcludedTraits(testCase))
				return false;
			return true;
		}

		bool FilterExcludedClasses(_TestCaseDiscovered testCase)
		{
			// No filters == pass
			if (ExcludedClasses.Count == 0)
				return true;

			// No class == pass
			if (testCase.TestClassWithNamespace == null)
				return true;

			// Exact match == do not pass
			if (ExcludedClasses.Contains(testCase.TestClassWithNamespace))
				return false;

			return true;
		}

		bool FilterExcludedMethods(_TestCaseDiscovered testCase)
		{
			// No filters == pass
			if (excludedMethodRegexFilters.Count == 0 && excludedMethodStandardFilters.Count == 0)
				return true;

			// No method == pass
			if (testCase.TestMethod == null)
				return true;

			var methodName = $"{testCase.TestClassWithNamespace}.{testCase.TestMethod}";

			// Standard exact match == do not pass
			if (excludedMethodStandardFilters.Contains(methodName) == true)
				return false;

			// Regex match == do not pass
			foreach (var regex in excludedMethodRegexFilters)
				if (regex.IsMatch(methodName))
					return false;

			return true;
		}

		bool FilterExcludedNamespaces(_TestCaseDiscovered testCase)
		{
			// No filters == pass
			if (ExcludedNamespaces.Count == 0)
				return true;

			// No namespace == pass
			if (testCase.TestNamespace == null)
				return true;

			// Exact match or starts-with match == do not pass
			if (ExcludedNamespaces.Any(ns => testCase.TestNamespace.Equals(ns, StringComparison.OrdinalIgnoreCase) || testCase.TestNamespace.StartsWith($"{ns}.", StringComparison.OrdinalIgnoreCase)))
				return false;

			return true;
		}

		bool FilterExcludedTraits(_TestCaseDiscovered testCase)
		{
			// No filters == pass
			if (ExcludedTraits.Count == 0)
				return true;

			// No traits in the test case == pass
			if (testCase.Traits.Count == 0)
				return true;

			foreach (var kvp in ExcludedTraits)
				foreach (var value in kvp.Value)
					if (testCase.Traits.Contains(kvp.Key, value, StringComparer.OrdinalIgnoreCase))
						return false;

			return true;
		}

		bool FilterIncludedClasses(_TestCaseDiscovered testCase)
		{
			// No filters == pass
			if (IncludedClasses.Count == 0)
				return true;

			// No class == do not pass
			if (testCase.TestClassWithNamespace == null)
				return false;

			// Exact match == pass
			if (IncludedClasses.Contains(testCase.TestClassWithNamespace))
				return true;

			return false;
		}

		bool FilterIncludedMethods(_TestCaseDiscovered testCase)
		{
			// No filters == pass
			if (includedMethodRegexFilters.Count == 0 && includedMethodStandardFilters.Count == 0)
				return true;

			// No method == do not pass
			if (testCase.TestMethod == null)
				return false;

			var methodName = $"{testCase.TestClassWithNamespace}.{testCase.TestMethod}";

			// Standard exact match == pass
			if (includedMethodStandardFilters.Contains(methodName))
				return true;

			// Regex match == pass
			foreach (var regex in includedMethodRegexFilters)
				if (regex.IsMatch(methodName))
					return true;

			return false;
		}

		bool FilterIncludedNamespaces(_TestCaseDiscovered testCase)
		{
			// No filters == pass
			if (IncludedNamespaces.Count == 0)
				return true;

			// No namespace == do not pass
			if (testCase.TestNamespace == null)
				return false;

			// Exact match or starts-with match == pass
			if (IncludedNamespaces.Any(ns => testCase.TestNamespace.Equals(ns, StringComparison.OrdinalIgnoreCase) || testCase.TestNamespace.StartsWith($"{ns}.", StringComparison.OrdinalIgnoreCase)))
				return true;

			return false;
		}

		bool FilterIncludedTraits(_TestCaseDiscovered testCase)
		{
			// No filters == pass
			if (IncludedTraits.Count == 0)
				return true;

			// No traits in the test case == do not pass
			if (testCase.Traits.Count == 0)
				return false;

			foreach (var kvp in IncludedTraits)
				foreach (var value in kvp.Value)
					if (testCase.Traits.Contains(kvp.Key, value, StringComparer.OrdinalIgnoreCase))
						return true;

			return false;
		}

		void SplitFilters()
		{
			lock (includedMethods)
				SplitFilters(includedMethods, ref includedMethodCacheLastUpdated, ref includedMethodStandardFilters, ref includedMethodRegexFilters);

			lock (excludedMethods)
				SplitFilters(excludedMethods, ref excludedMethodCacheLastUpdated, ref excludedMethodStandardFilters, ref excludedMethodRegexFilters);
		}

		void SplitFilters(
			ChangeTrackingHashSet<string> toSplit,
			ref DateTimeOffset lastCacheUpdate,
			ref HashSet<string> standardFilters,
			ref List<Regex> regexFilters)
		{
			if (lastCacheUpdate >= toSplit.LastMutation)
				return;

			standardFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			regexFilters = new List<Regex>();

			foreach (var filter in toSplit)
				if (filter.Contains("*") || filter.Contains("?"))
					regexFilters.Add(new Regex(WildcardToRegex(filter), RegexOptions.IgnoreCase));
				else
					standardFilters.Add(filter);

			lastCacheUpdate = includedMethods.LastMutation;
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
