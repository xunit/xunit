using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestClassDisposeStarting"/>.
/// </summary>
[JsonTypeID("test-class-dispose-starting")]
sealed partial class TestClassDisposeStarting : TestMessage, ITestClassDisposeStarting
{ }
