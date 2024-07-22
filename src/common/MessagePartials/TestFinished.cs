using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestFinished"/>.
/// </summary>
[JsonTypeID("test-finished")]
sealed partial class TestFinished : TestResultMessage, ITestFinished
{ }
