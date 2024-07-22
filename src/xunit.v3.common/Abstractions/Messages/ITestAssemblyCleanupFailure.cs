namespace Xunit.Sdk;

/// <summary>
/// This message indicates that an error has occurred during test assembly cleanup.
/// </summary>
public interface ITestAssemblyCleanupFailure : ITestAssemblyMessage, IErrorMetadata
{ }
