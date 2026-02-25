using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestPassed"/>.
/// </summary>
[JsonTypeID(TypeID)]
sealed partial class TestPassed : TestResultMessage, ITestPassed
{
	internal const string TypeID = "test-passed";
}
