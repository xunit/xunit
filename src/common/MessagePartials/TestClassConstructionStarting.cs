using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestClassConstructionStarting"/>.
/// </summary>
[JsonTypeID(TypeID)]
sealed partial class TestClassConstructionStarting : TestMessage, ITestClassConstructionStarting
{
	internal const string TypeID = "test-class-construction-starting";
}
