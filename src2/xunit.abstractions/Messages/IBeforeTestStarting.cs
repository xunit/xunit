#pragma warning disable 1574    // XDOC reference to Xunit.Sdk.BeforeAfterTestAttribute will be fixed up post-compilation

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message is sent during execution to indicate that the Before method of
    /// a <see cref="Xunit.Sdk.BeforeAfterTestAttribute"/> is about to execute.
    /// </summary>
    public interface IBeforeTestStarting : ITestMessage
    {
        /// <summary>
        /// The fully qualified type name of the <see cref="Xunit.Sdk.BeforeAfterTestAttribute"/>.
        /// </summary>
        string AttributeName { get; }

        /// <summary>
        /// The test case that this message applies to. 
        /// </summary>
        ITestCase TestCase { get; }

        /// <summary>
        /// The display name to be used for this test. 
        /// </summary>
        string TestDisplayName { get; }
    }
}