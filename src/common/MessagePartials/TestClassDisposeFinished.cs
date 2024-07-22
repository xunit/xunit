using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestClassDisposeFinished"/>.
/// </summary>
[JsonTypeID("test-class-dispose-finished")]
sealed partial class TestClassDisposeFinished : TestMessage, ITestClassDisposeFinished
{ }
