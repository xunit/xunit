using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a set of filters for an <see cref="XunitProject"/>.
/// </summary>
public class XunitFilters
{
	DateTimeOffset includedMethodCacheLastUpdated;
	List<Regex> includedMethodRegexFilters = [];
	HashSet<string> includedMethodStandardFilters = [];
	readonly ChangeTrackingHashSet<string> includedMethods = new(StringComparer.OrdinalIgnoreCase);

	DateTimeOffset excludedMethodCacheLastUpdated;
	List<Regex> excludedMethodRegexFilters = [];
	HashSet<string> excludedMethodStandardFilters = [];
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
	public Dictionary<string, HashSet<string>> ExcludedTraits { get; } = new(StringComparer.OrdinalIgnoreCase);

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
	public Dictionary<string, HashSet<string>> IncludedTraits { get; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Filters the given method using the defined filter values.
	/// </summary>
	/// <param name="testCase">The test case to filter.</param>
	/// <returns>Returns <c>true</c> if the test case passed the filter; returns <c>false</c> otherwise.</returns>
	public bool Filter(ITestCaseMetadata testCase)
	{
		Guard.ArgumentNotNull(testCase);

		SplitFilters();

		return
			FilterIncludedNamespaces(testCase) &&
			FilterExcludedNamespaces(testCase) &&
			FilterIncludedClasses(testCase) &&
			FilterExcludedClasses(testCase) &&
			FilterIncludedMethods(testCase) &&
			FilterExcludedMethods(testCase) &&
			FilterIncludedTraits(testCase) &&
			FilterExcludedTraits(testCase);
	}

	bool FilterExcludedClasses(ITestCaseMetadata testCase)
	{
		// No filters == pass
		if (ExcludedClasses.Count == 0)
			return true;

		// No class == pass
		if (testCase.TestClassName is null)
			return true;

		// Exact match == do not pass
		return !ExcludedClasses.Contains(testCase.TestClassName);
	}

	bool FilterExcludedMethods(ITestCaseMetadata testCase)
	{
		// No filters == pass
		if (excludedMethodRegexFilters.Count == 0 && excludedMethodStandardFilters.Count == 0)
			return true;

		// No method == pass
		if (testCase.TestMethodName is null)
			return true;

		var methodName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", testCase.TestClassName, testCase.TestMethodName);

		// Standard exact match == do not pass
		if (excludedMethodStandardFilters.Contains(methodName))
			return false;

		// Regex match == do not pass
		foreach (var regex in excludedMethodRegexFilters)
			if (regex.IsMatch(methodName))
				return false;

		return true;
	}

	bool FilterExcludedNamespaces(ITestCaseMetadata testCase)
	{
		// No filters == pass
		if (ExcludedNamespaces.Count == 0)
			return true;

		// No namespace == pass
		if (testCase.TestClassNamespace is null)
			return true;

		// Exact match or starts-with match == do not pass
		return !ExcludedNamespaces.Any(ns => testCase.TestClassNamespace.Equals(ns, StringComparison.OrdinalIgnoreCase) || testCase.TestClassNamespace.StartsWith(ns + ".", StringComparison.OrdinalIgnoreCase));
	}

	bool FilterExcludedTraits(ITestCaseMetadata testCase)
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

	bool FilterIncludedClasses(ITestCaseMetadata testCase)
	{
		// No filters == pass
		if (IncludedClasses.Count == 0)
			return true;

		// No class == do not pass
		if (testCase.TestClassName is null)
			return false;

		// Exact match == pass
		return IncludedClasses.Contains(testCase.TestClassName);
	}

	bool FilterIncludedMethods(ITestCaseMetadata testCase)
	{
		// No filters == pass
		if (includedMethodRegexFilters.Count == 0 && includedMethodStandardFilters.Count == 0)
			return true;

		// No method == do not pass
		if (testCase.TestMethodName is null)
			return false;

		var methodName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", testCase.TestClassName, testCase.TestMethodName);

		// Standard exact match == pass
		if (includedMethodStandardFilters.Contains(methodName))
			return true;

		// Regex match == pass
		foreach (var regex in includedMethodRegexFilters)
			if (regex.IsMatch(methodName))
				return true;

		return false;
	}

	bool FilterIncludedNamespaces(ITestCaseMetadata testCase)
	{
		// No filters == pass
		if (IncludedNamespaces.Count == 0)
			return true;

		// No namespace == do not pass
		if (testCase.TestClassNamespace is null)
			return false;

		// Exact match or starts-with match == pass
		return IncludedNamespaces.Any(ns => testCase.TestClassNamespace.Equals(ns, StringComparison.OrdinalIgnoreCase) || testCase.TestClassNamespace.StartsWith(ns + ".", StringComparison.OrdinalIgnoreCase));
	}

	bool FilterIncludedTraits(ITestCaseMetadata testCase)
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
		regexFilters = [];

		foreach (var filter in toSplit)
			if (filter.Contains("*") || filter.Contains("?"))
				regexFilters.Add(new Regex(WildcardToRegex(filter), RegexOptions.IgnoreCase));
			else
				standardFilters.Add(filter);

		lastCacheUpdate = includedMethods.LastMutation;
	}

	static string WildcardToRegex(string pattern) =>
		string.Format(CultureInfo.InvariantCulture, "^{0}$", Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", "."));

	// This class wraps HashSet<T>, tracking the last mutation date, and using itself
	// as a lock for mutation (so that we can guarantee a stable data set when transferring
	// the data into caches).
	sealed class ChangeTrackingHashSet<T>(IEqualityComparer<T> comparer) :
		ICollection<T>
	{
		readonly HashSet<T> innerCollection = new(comparer);

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

		public bool Contains(T item) =>
			innerCollection.Contains(item);

		public void CopyTo(
			T[] array,
			int arrayIndex) =>
				innerCollection.CopyTo(array, arrayIndex);

		public IEnumerator<T> GetEnumerator() =>
			innerCollection.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			innerCollection.GetEnumerator();

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
