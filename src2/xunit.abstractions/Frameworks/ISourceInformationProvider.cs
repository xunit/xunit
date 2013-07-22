namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a provider which gives source line information for a test case. Generally
    /// consumed by an implementation of <see cref="ITestFrameworkDiscoverer"/> during Find operations.
    /// </summary>
    public interface ISourceInformationProvider
    {
        /// <summary>
        /// Returns the source information for a test case.
        /// </summary>
        /// <param name="testCase">The test case to retrieve information for.</param>
        /// <returns>The source information, with null string and int values when the information is not available.
        /// Note: return value should never be <c>null</c>, only the interior data values inside.</returns>
        SourceInformation GetSourceInformation(ITestCase testCase);
    }
}