namespace Xunit.v3;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate that the developer wishes to have code
/// that runs during the test pipeline startup and shutdown (including both discovery and execution).
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class TestPipelineStartupAttribute(Type testPipelineStartupType) :
	Attribute
#if !XUNIT_AOT
	, ITestPipelineStartupAttribute
#endif
{
	/// <inheritdoc/>
	public Type TestPipelineStartupType { get; } = testPipelineStartupType;
}
