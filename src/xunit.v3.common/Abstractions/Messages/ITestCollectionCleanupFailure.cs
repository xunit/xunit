namespace Xunit.Sdk;

/// <summary>
/// This message indicates that an error has occurred during test collection cleanup.
/// </summary>
public interface ITestCollectionCleanupFailure : ITestCollectionMessage, IErrorMetadata
{ }
