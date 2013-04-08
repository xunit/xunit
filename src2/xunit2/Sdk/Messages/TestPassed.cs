using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestPassed"/>.
    /// </summary>
    public class TestPassed : TestResultMessage, ITestPassed
    {
    }
}