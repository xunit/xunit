namespace Xunit.Sdk;

/// <summary>
/// This message indicates that an error has occurred during test cleanup.
/// </summary>
public interface ITestCleanupFailure : ITestMessage, IErrorMetadata
{ }
