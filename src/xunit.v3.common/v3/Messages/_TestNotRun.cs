namespace Xunit.v3;

/// <summary>
/// This message indicates that a test was not run because it was excluded (either because
/// it was marked as explicit and explicit tests weren't run, or because it was marked as
/// not explicit as only explicit tests were run).
/// </summary>
public class _TestNotRun : _TestResultMessage
{ }
