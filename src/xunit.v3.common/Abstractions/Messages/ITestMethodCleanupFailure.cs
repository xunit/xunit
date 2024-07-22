namespace Xunit.Sdk;

/// <summary>
/// This message indicates that an error has occurred during test method cleanup.
/// </summary>
public interface ITestMethodCleanupFailure : ITestMethodMessage, IErrorMetadata
{ }
