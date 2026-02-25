using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestNotRun"/>.
/// </summary>
[JsonTypeID(TypeID)]
sealed partial class TestNotRun : TestResultMessage, ITestNotRun
{
	internal const string TypeID = "test-not-run";
}
