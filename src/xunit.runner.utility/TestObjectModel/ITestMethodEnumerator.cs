using System;
using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// Represents the ability to enumerate and filter test methods.
    /// </summary>
    public interface ITestMethodEnumerator
    {
        /// <summary>
        /// Enumerates all test methods.
        /// </summary>
        IEnumerable<TestMethod> EnumerateTestMethods();

        /// <summary>
        /// Enumerates test methods which pass the given filter.
        /// </summary>
        /// <param name="filter">The test method filter.</param>
        IEnumerable<TestMethod> EnumerateTestMethods(Predicate<TestMethod> filter);
    }
}