using System;
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
        List<Regex> includedMethodRegexes;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitFilters"/> class.
        /// </summary>
        public XunitFilters()
        {
            ExcludedTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            IncludedTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            IncludedClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IncludedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            includedMethodRegexes = new List<Regex>();
            IncludedNameSpaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
        /// Gets the set of method filters for test classes to include.
        /// </summary>
        public HashSet<string> IncludedClasses { get; }

        /// <summary>
        /// Gets the set of method filters for tests to include.
        /// </summary>
        public HashSet<string> IncludedMethods { get; }

        /// <summary>
        /// Add an include method filter. 
        /// It can either be a fully-qualified method name, or a wildcard pattern.
        /// </summary>
        public void AddIncludedMethod(string methodNameOrWildcard)
        {
            if (methodNameOrWildcard.Contains("*"))
            {
                var regexPattern = WildcardToRegex(methodNameOrWildcard);
                var regex = new Regex(regexPattern);
                includedMethodRegexes.Add(regex);
            }
            else
            {
                IncludedMethods.Add(methodNameOrWildcard);
            }
        }

        /// <summary>
        /// Checks whether there are any included method filters.
        /// </summary>
        public bool HasIncludedMethods()
        {
            return IncludedMethods.Count != 0 || includedMethodRegexes.Count != 0; 
        }

        /// <summary>
        /// Converts a wildcard to a regex.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to convert.</param>
        /// <returns>A regex equivalent of the given wildcard.</returns>
        public string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
                 Replace("\\*", ".*").
                 Replace("\\?", ".") + "$";
        }

        /// <summary>
        /// Gets the set of assembly filters for tests to include.
        /// </summary>
        public HashSet<string> IncludedNameSpaces { get; }

        /// <summary>
        /// Filters the given method using the defined filter values.
        /// </summary>
        /// <param name="testCase">The test case to filter.</param>
        /// <returns>Returns <c>true</c> if the test case passed the filter; returns <c>false</c> otherwise.</returns>
        public bool Filter(ITestCase testCase)
        {
            if (!FilterIncludedMethodsAndClasses(testCase))
                return false;
            if (!FilterIncludedTraits(testCase))
                return false;
            if (!FilterExcludedTraits(testCase))
                return false;
            if (!FilterIncludedNameSpaces(testCase))
                return false;

            return true;
        }

        bool FilterIncludedNameSpaces(ITestCase testCase)
        {
            // No assemblies in the filter == everything is okay
            if (IncludedNameSpaces.Count == 0)
                return true;

            if (IncludedNameSpaces.Count != 0 && IncludedNameSpaces.Any(a => testCase.TestMethod.TestClass.Class.Name.StartsWith($"{a}.", StringComparison.Ordinal)))
                return true;

            return false;
        }

        bool FilterIncludedMethodsAndClasses(ITestCase testCase)
        {
            // No methods or classes in the filter == everything is okay
            if (!HasIncludedMethods() && IncludedClasses.Count == 0)
                return true;

            if (IncludedClasses.Count != 0 && IncludedClasses.Contains(testCase.TestMethod.TestClass.Class.Name))
                return true;

            var testCaseMethod = $"{testCase.TestMethod.TestClass.Class.Name}.{testCase.TestMethod.Method.Name}";
            if (IncludedMethods.Count != 0 && IncludedMethods.Contains(testCaseMethod))
                return true;

            if (includedMethodRegexes != null && FilterIncludedMethodWildcards(testCaseMethod))
                return true;

            return false;
        }

        bool FilterIncludedMethodWildcards(string testCaseMethod)
        {
            foreach(var regex in includedMethodRegexes)
            {
                if (regex.IsMatch(testCaseMethod))
                {
                    return true;
                }
            }
            return false;
        }

        bool FilterExcludedTraits(ITestCase testCase)
        {
            // No traits in the filter == everything is okay
            if (ExcludedTraits.Count == 0)
                return true;

            // No traits in the method == it's always safe from exclusion
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
    }
}
