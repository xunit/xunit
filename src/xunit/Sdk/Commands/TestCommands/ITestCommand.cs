using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Interface which represents the ability to invoke of a test method.
    /// </summary>
    public interface ITestCommand
    {
        /// <summary>
        /// Gets the display name of the test method.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Determines if the test runner infrastructure should create a new instance of the
        /// test class before running the test.
        /// </summary>
        bool ShouldCreateInstance { get; }

        /// <summary>
        /// Determines if the test should be limited to running a specific amount of time
        /// before automatically failing.
        /// </summary>
        /// <returns>The timeout value, in milliseconds; if zero, the test will not have
        /// a timeout.</returns>
        int Timeout { get; }

        /// <summary>
        /// Executes the test method.
        /// </summary>
        /// <param name="testClass">The instance of the test class</param>
        /// <returns>Returns information about the test run</returns>
        MethodResult Execute(object testClass);

        /// <summary>
        /// Creates the start XML to be sent to the callback when the test is about to start
        /// running.
        /// </summary>
        /// <returns>Return the <see cref="XmlNode"/> of the start node, or null if the test
        /// is known that it will not be running.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        XmlNode ToStartXml();
    }
}