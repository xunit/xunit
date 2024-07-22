namespace Xunit.Sdk;

/// <summary>
/// Base interface for all test messages. A test message is a message that is used to communicate
/// the status of discovery and/or execution of tests.
/// </summary>
public interface IMessageSinkMessage : IJsonSerializable
{ }
