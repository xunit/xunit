using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test has finished executing.
/// </summary>
[JsonTypeID("test-finished")]
public class _TestFinished : _TestResultMessage
{ }
