#pragma warning disable 1574    // XDOC reference to Xunit.Sdk.BeforeAfterTestAttribute will be fixed up post-compilation

namespace Xunit.Abstractions
{
    /// <summary>
    /// This message is sent during execution to indicate that the Before method of
    /// a <see cref="Xunit.Sdk.BeforeAfterTestAttribute"/> has completed executing.
    /// </summary>
    public interface IBeforeTestFinished : ITestMessage
    {
        /// <summary>
        /// The fully qualified type name of the <see cref="Xunit.Sdk.BeforeAfterTestAttribute"/>.
        /// </summary>
        string AttributeName { get; }
    }
}