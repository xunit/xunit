using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        XmlNode ToXml(XmlNode parentNode);
    }
}