using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Indicates that a test has passed.
/// </summary>
[JsonTypeID("test-passed")]
public class _TestPassed : _TestResultMessage
{ }
