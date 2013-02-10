using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    // REVIEW: Is ITestCase the right abstraction to use for this discovery?

    /// <summary>
    /// Represents a provider which gives source line information for a test case. Generally
    /// consumed by an implementation of <see cref="ITestFramework"/> during Find operations.
    /// </summary>
    public interface ISourceInformationProvider
    {
        /// <summary>
        /// Returns the source information for a test case.
        /// </summary>
        /// <param name="testCase">The test case to retrieve information for.</param>
        /// <returns>The source information, with null string and int values when the information is not available.
        /// Note: the tuple itself should never be null, only the interior data values inside the tuple.</returns>
        Tuple<string, int?> GetSourceInformation(ITestCase testCase);
    }
}
