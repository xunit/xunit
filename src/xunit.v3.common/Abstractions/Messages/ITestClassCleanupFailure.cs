namespace Xunit.Sdk;

/// <summary>
/// This message indicates that an error has occurred during test class cleanup.
/// </summary>
public interface ITestClassCleanupFailure : ITestClassMessage, IErrorMetadata
{ }
