namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test has finished executing.
/// </summary>
[JsonTypeID("test-finished")]
public sealed class TestFinished : TestResultMessage
{ }
