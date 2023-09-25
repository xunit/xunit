#pragma warning disable CA1040 // This is intended as a marker interface

namespace Xunit.Sdk;

/// <summary>
/// Marker interface that must be implemented by test framework attributes, so
/// that the test framework attribute discoverer can find them.
/// </summary>
public interface ITestFrameworkAttribute
{ }
