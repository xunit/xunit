using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents a set of filters for an <see cref="XunitProject"/>.
    /// </summary>
    public class XunitFilters
    {
        DateTimeOffset includeCacheDataDate;
        ChangeTrackingHashSet<string> includedMethods;
        List<Regex> includeMethodRegexFilters;
        HashSet<string> includeMethodStandardFilters;

        DateTimeOffset excludeCacheDataDate;
        ChangeTrackingHashSet<string> excludedMethods;
        List<Regex> excludeMethodRegexFilters;
        HashSet<string> excludeMethodStandardFilters;

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

        /// <summary>
        /// Filters the given method using the defined filter values.
        /// </summary>
        /// <param name="testCase">The test case to filter.</param>
        /// <returns>Returns <c>true</c> if the test case passed the filter; returns <c>false</c> otherwise.</returns>
        public bool Filter(ITestCase testCase)
        {
            SplitMethodFilters();

            if (!FilterIncludedMethodsAndClasses(testCase))
                return false;
            if (!FilterExcludedMethodsAndClasses(testCase))
                return false;
            if (!FilterIncludedTraits(testCase))
                return false;
            if (!FilterExcludedTraits(testCase))
                return false;
            if (!FilterIncludedNamespaces(testCase))
                return false;
            if (!FilterExcludedNamespaces(testCase))
                return false;
            return true;
        }

        bool FilterExcludedNamespaces(ITestCase testCase)
        {
            // No assemblies in the filter == everything is okay
            if (ExcludedNamespaces.Count == 0)
                return true;

            if (ExcludedNamespaces.Count != 0 && ExcludedNamespaces.Any(a => testCase.TestMethod.TestClass.Class.Name.StartsWith($"{a}.", StringComparison.Ordinal)))
                return false;
            return true;
        }

        bool FilterIncludedNamespaces(ITestCase testCase)
        {
            // No assemblies in the filter == everything is okay
            if (IncludedNamespaces.Count == 0)
                return true;

            if (IncludedNamespaces.Count != 0 && IncludedNamespaces.Any(a => testCase.TestMethod.TestClass.Class.Name.StartsWith($"{a}.", StringComparison.Ordinal)))
                return true;

            return false;
        }

        bool FilterExcludedMethodsAndClasses(ITestCase testCase)
        {
            // No methods or classes in the filter == everything is okay
            if (excludeMethodStandardFilters.Count == 0 && excludeMethodRegexFilters.Count == 0 && ExcludedClasses.Count == 0)
                return true;

            if (ExcludedClasses.Count != 0 && ExcludedClasses.Contains(testCase.TestMethod.TestClass.Class.Name))
                return false;

            var methodName = $"{testCase.TestMethod.TestClass.Class.Name}.{testCase.TestMethod.Method.Name}";

            if (excludeMethodStandardFilters.Count != 0 && excludeMethodStandardFilters.Contains(methodName))
                return false;

            if (excludeMethodRegexFilters.Count != 0)
                foreach (var regex in excludeMethodRegexFilters)
                    if (regex.IsMatch(methodName))
                        return false;

            return true;
        }

        bool FilterIncludedMethodsAndClasses(ITestCase testCase)
        {
            // No methods or classes in the filter == everything is okay
            if (includeMethodStandardFilters.Count == 0 && includeMethodRegexFilters.Count == 0 && IncludedClasses.Count == 0)
                return true;

            if (IncludedClasses.Count != 0 && IncludedClasses.Contains(testCase.TestMethod.TestClass.Class.Name))
                return true;

            var methodName = $"{testCase.TestMethod.TestClass.Class.Name}.{testCase.TestMethod.Method.Name}";

            if (includeMethodStandardFilters.Count != 0 && includeMethodStandardFilters.Contains(methodName))
                return true;

            if (includeMethodRegexFilters.Count != 0)
                foreach (var regex in includeMethodRegexFilters)
                    if (regex.IsMatch(methodName))
                        return true;

            return false;
        }

        bool FilterExcludedTraits(ITestCase testCase)
        {
            // No traits in the filter == everything is okay
            if (ExcludedTraits.Count == 0)
                return true;

            // No traits in the method == it's never excluded
            if (testCase.Traits.Count == 0)
                return true;

            foreach (var key in ExcludedTraits.Keys)
                foreach (var value in ExcludedTraits[key])
                    if (testCase.Traits.Contains(key, value, StringComparer.OrdinalIgnoreCase))
                        return false;

            return true;
        }

        bool FilterIncludedTraits(ITestCase testCase)
        {
            // No traits in the filter == everything is okay
            if (IncludedTraits.Count == 0)
                return true;

            // No traits in the method == it'll never match anything, don't try
            if (testCase.Traits.Count == 0)
                return false;

            foreach (var key in IncludedTraits.Keys)
                foreach (var value in IncludedTraits[key])
                    if (testCase.Traits.Contains(key, value, StringComparer.OrdinalIgnoreCase))
                        return true;

            return false;
        }

        void SplitMethodFilters()
        {
            this.SplitIncludeMethodFilters();
            this.SplitExcludeMethodFilters();
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

        string WildcardToRegex(string pattern)
            => $"^{Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".")}$";

        // This class wraps HashSet<T>, tracking the last mutation date, and using itself
        // as a lock for mutation (so that we can guarantee a stable data set when transferring
        // the data into caches).
        class ChangeTrackingHashSet<T> : ICollection<T>
        {
            HashSet<T> innerCollection;

            public ChangeTrackingHashSet(IEqualityComparer<T> comparer)
            {
                innerCollection = new HashSet<T>(comparer);
            }

            public int Count => innerCollection.Count;
            public bool IsReadOnly => false;

            public DateTimeOffset LastMutation { get; private set; } = DateTimeOffset.UtcNow;

            public void Add(T item)
            {
                lock (this)
                {
                    LastMutation = DateTimeOffset.UtcNow;
                    innerCollection.Add(item);
                }
            }

            public void Clear()
            {
                lock (this)
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
                lock (this)
                {
                    LastMutation = DateTimeOffset.UtcNow;
                    return innerCollection.Remove(item);
                }
            }
        }
    }
}
