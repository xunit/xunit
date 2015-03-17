namespace Xunit.Abstractions
{
    /// <summary>
    /// This message is sent during execution to indicate that the After method of
    /// a <see cref="T:Xunit.Sdk.BeforeAfterTestAttribute"/> is about to execute.
    /// </summary>
    public interface IAfterTestStarting : ITestMessage, IExecutionMessage
    {
        /// <summary>
        /// The fully qualified type name of the <see cref="T:Xunit.Sdk.BeforeAfterTestAttribute"/>.
        /// </summary>
        string AttributeName { get; }
    }
}