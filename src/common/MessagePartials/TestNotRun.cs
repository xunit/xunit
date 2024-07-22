using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestNotRun"/>.
/// </summary>
[JsonTypeID("test-not-run")]
sealed partial class TestNotRun : TestResultMessage, ITestNotRun
{ }
