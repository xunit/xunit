#pragma warning disable 1574    // XDOC reference to Xunit.Sdk.BeforeAfterTestAttribute will be fixed up post-compilation

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message is sent during execution to indicate that the After method of
    /// a <see cref="Xunit.Sdk.BeforeAfterTestAttribute"/> is about to execute.
    /// </summary>
    public interface IAfterTestStarting : ITestMessage
    {
        /// <summary>
        /// The fully qualified type name of the <see cref="Xunit.Sdk.BeforeAfterTestAttribute"/>.
        /// </summary>
        string AttributeName { get; }
    }
}