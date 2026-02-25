using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestClassDisposeStarting"/>.
/// </summary>
[JsonTypeID(TypeID)]
sealed partial class TestClassDisposeStarting : TestMessage, ITestClassDisposeStarting
{
	internal const string TypeID = "test-class-dispose-starting";
}
