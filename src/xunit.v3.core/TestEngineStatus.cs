namespace Xunit;

/// <summary>
/// Represents the current status of the execution of the test engine, with respect to
/// a phase in the execution pipeline (for example, engine status for a test collection vs.
/// test case vs. test).
/// </summary>
public enum TestEngineStatus
{
	/// <summary>
	/// The test engine is in the initialization phase of the given stage in the pipeline.
	/// </summary>
	Initializing = 1,

	/// <summary>
	/// The test engine is running the given state of the pipeline.
	/// </summary>
	Running,

	/// <summary>
	/// The test engine has run the given stage of the pipeline, and is currently doing clean up (f.e., Dispose).
	/// </summary>
	CleaningUp,

	/// <summary>
	/// The test engine is in the process of discovering tests.
	/// </summary>
	Discovering,
}
