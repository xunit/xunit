namespace Xunit.Sdk;

/// <summary>
/// Indicates that a test has passed.
/// </summary>
[JsonTypeID("test-passed")]
public sealed class TestPassed : TestResultMessage
{ }
