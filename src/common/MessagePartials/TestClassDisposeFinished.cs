using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestClassDisposeFinished"/>.
/// </summary>
[JsonTypeID(TypeID)]
sealed partial class TestClassDisposeFinished : TestMessage, ITestClassDisposeFinished
{
	internal const string TypeID = "test-class-dispose-finished";
}
