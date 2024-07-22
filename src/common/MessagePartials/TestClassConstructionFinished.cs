using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestClassConstructionFinished"/>.
/// </summary>
[JsonTypeID("test-class-construction-finished")]
sealed partial class TestClassConstructionFinished : TestMessage, ITestClassConstructionFinished
{ }
