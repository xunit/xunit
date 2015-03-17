namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an error has occurred in the execution process. 
    /// </summary>
    public interface IErrorMessage : IMessageSinkMessage, IFailureInformation, IExecutionMessage
    {
    }
}