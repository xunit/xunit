namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a catastrophic error has occurred.
/// </summary>
public interface IErrorMessage : IMessageSinkMessage, IErrorMetadata
{ }
