using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Interface that represents a single test result.
    /// </summary>
    public interface ITestResult
    {
        /// <summary>
        /// The amount of time spent in execution
        /// </summary>
        double ExecutionTime { get; }

        /// <summary>
        /// Converts the test result into XML that is consumed by the test runners.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <returns>The newly created XML node.</returns>
        XmlNode ToXml(XmlNode parentNode);
    }
}