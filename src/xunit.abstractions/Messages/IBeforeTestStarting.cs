namespace Xunit.Abstractions
{
    /// <summary>
    /// This message is sent during execution to indicate that the Before method of
    /// a <see cref="T:Xunit.Sdk.BeforeAfterTestAttribute"/> is about to execute.
    /// </summary>
    public interface IBeforeTestStarting : ITestMessage, IExecutionMessage
    {
        /// <summary>
        /// The fully qualified type name of the <see cref="T:Xunit.Sdk.BeforeAfterTestAttribute"/>.
        /// </summary>
        string AttributeName { get; }
    }
}