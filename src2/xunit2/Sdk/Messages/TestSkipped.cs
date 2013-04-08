using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestSkipped"/>.
    /// </summary>
    public class TestSkipped : TestResultMessage, ITestSkipped
    {
        /// <inheritdoc/>
        public string Reason { get; set; }
    }
}