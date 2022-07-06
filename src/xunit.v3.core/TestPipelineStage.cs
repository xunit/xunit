namespace Xunit;

/// <summary>
/// A flag to indicate which part of the test pipeline you're in. Retrieved from an
/// instance of <see cref="TestContext"/>.
/// </summary>
public enum TestPipelineStage
{
	/// <summary>
	/// Indicates an unknown state of the test pipeline, or being outside of the test pipeline.
	/// </summary>
	Unknown,

	/// <summary>
	/// Indicates that the test pipeline is still in the initialization phase and hasn't begun work.
	/// </summary>
	Initialization,

	/// <summary>
	/// Indicates that tests are currently being discovered.
	/// </summary>
	Discovery,

	/// <summary>
	/// Indicates that the test pipeline is executing a test assembly.
	/// </summary>
	TestAssemblyExecution,

	/// <summary>
	/// Indicates that the test pipeline is executing a test collection.
	/// </summary>
	TestCollectionExecution,

	/// <summary>
	/// Indicates that the test pipeline is executing a test class.
	/// </summary>
	TestClassExecution,

	/// <summary>
	/// Indicates that the test pipeline is executing a test method.
	/// </summary>
	TestMethodExecution,

	/// <summary>
	/// Indicates that the test pipeline is executing a test case.
	/// </summary>
	TestCaseExecution,

	/// <summary>
	/// Indicates that the test pipeline is executing a test.
	/// </summary>
	TestExecution,
}
