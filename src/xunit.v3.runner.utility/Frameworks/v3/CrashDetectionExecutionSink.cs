using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="IMessageSink"/> that detects when test assembly starting
/// and finished messages have been sent, and will send cleanup/error messages to the inner
/// sink if it appears that the test process crashed (failed to send one or both messages).
/// </summary>
/// <param name="projectAssembly">The test project assembly</param>
/// <param name="innerSink">The inner message sink</param>
public class CrashDetectionExecutionSink(
	XunitProjectAssembly projectAssembly,
	IMessageSink innerSink) :
		CrashDetectionSinkBase<ITestAssemblyStarting, ITestAssemblyFinished>(projectAssembly, innerSink)
{
	static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyTraits = new Dictionary<string, IReadOnlyCollection<string>>();

	/// <summary>
	/// Override this to set a constant time for testing purposes.
	/// </summary>
	protected virtual DateTimeOffset UtcNow =>
		DateTimeOffset.UtcNow;

	/// <inheritdoc/>
	protected override void SendFinish(string assemblyUniqueID) =>
		InnerSink.OnMessage(new TestAssemblyFinished
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = 0m,
			FinishTime = UtcNow,
			TestsFailed = 1,
			TestsNotRun = 0,
			TestsSkipped = 0,
			TestsTotal = 1,
		});

	/// <inheritdoc/>
	protected override void SendStart(string assemblyUniqueID) =>
		InnerSink.OnMessage(new TestAssemblyStarting
		{
			AssemblyName = ProjectAssembly.AssemblyDisplayName,
			AssemblyPath = ProjectAssembly.AssemblyFileName,
			AssemblyUniqueID = assemblyUniqueID,
			ConfigFilePath = ProjectAssembly.ConfigFileName,
			Seed = ProjectAssembly.Configuration.Seed,
			StartTime = UtcNow,
			TargetFramework = null,
			TestEnvironment = "<unknown>",
			TestFrameworkDisplayName = "<unknown>",
			Traits = EmptyTraits,
		});
}
