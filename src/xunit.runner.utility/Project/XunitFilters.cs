using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents a set of filters for an <see cref="XunitProject"/>.
    /// </summary>
    public class XunitFilters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitFilters"/> class.
        /// </summary>
        public XunitFilters()
        {
            ExcludedTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            IncludedTraits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            IncludedMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the set of trait filters for tests to exclude.
        /// </summary>
        public Dictionary<string, List<string>> ExcludedTraits { get; private set; }

        /// <summary>
        /// Gets the set of trait filters for tests to include.
        /// </summary>
        public Dictionary<string, List<string>> IncludedTraits { get; private set; }

        /// <summary>
        /// Gets the set of method filters for tests to include.
        /// </summary>
        public HashSet<string> IncludedMethods { get; private set; }

        /// <summary>
        /// Filters the given method using the defined filter values.
        /// </summary>
        /// <param name="testCase">The test case to filter.</param>
        /// <returns>Returns <c>true</c> if the test case passed the filter; returns <c>false</c> otherwise.</returns>
        public bool Filter(ITestCase testCase)
        {
            if (!FilterIncludedMethods(testCase))
                return false;
            if (!FilterIncludedTraits(testCase))
                return false;
            if (!FilterExcludedTraits(testCase))
                return false;

            return true;
        }

        bool FilterIncludedMethods(ITestCase testCase)
        {
            // No names in the filter == everything is okay
            if (IncludedMethods.Count == 0)
                return true;

            var name = String.Format("{0}.{1}", testCase.TestMethod.TestClass.Class.Name, testCase.TestMethod.Method.Name);
            return IncludedMethods.Contains(name);
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
