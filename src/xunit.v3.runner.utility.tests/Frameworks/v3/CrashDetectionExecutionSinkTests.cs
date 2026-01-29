using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class CrashDetectionExecutionSinkTests
{
	static readonly string assemblyFileName;
	static readonly XunitProjectAssembly projectAssembly;
	static readonly ITestProcessBase testProcess;
	static readonly ITestProcessBase testProcessWithExitCode;

	static CrashDetectionExecutionSinkTests()
	{
		assemblyFileName = typeof(CrashDetectionExecutionSinkTests).Assembly.Location;
		var assemblyMetadata = AssemblyUtility.GetAssemblyMetadata(assemblyFileName);

		Assert.NotNull(assemblyMetadata);

		projectAssembly = new(new(), assemblyFileName, assemblyMetadata);

		testProcess = Substitute.For<ITestProcess>();

		testProcessWithExitCode = Substitute.For<ITestProcessBase, ITestProcessWithExitCode>();
		((ITestProcessWithExitCode)testProcessWithExitCode).ExitCode.Returns(42);
	}

	[Fact]
	public static void WithoutStarting_WithoutFinished_SendsStarting_SendsError_SendsFinished()
	{
		var innerSink = SpyMessageSink.Capture();
		var now = DateTimeOffset.UtcNow;
		var sink = new CrashDetectionExecutionSinkWithFixedTime(projectAssembly, innerSink, now);
		var assemblyUniqueID = UniqueIDGenerator.ForAssembly(assemblyFileName, null);

		sink.OnProcessFinished(testProcess.TryGetExitCode());

		Assert.Collection(
			innerSink.Messages,
			msg =>
			{
				var starting = Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false);
				Assert.Equal(Path.GetFileNameWithoutExtension(assemblyFileName), starting.AssemblyName);
				Assert.Equal(assemblyFileName, starting.AssemblyPath);
				Assert.Equal(assemblyUniqueID, starting.AssemblyUniqueID);
				Assert.Null(starting.ConfigFilePath);
				Assert.Null(starting.Seed);
				Assert.Equal(now, starting.StartTime);
				Assert.Null(starting.TargetFramework);
				Assert.Equal("<unknown>", starting.TestEnvironment);
				Assert.Equal("<unknown>", starting.TestFrameworkDisplayName);
				Assert.Empty(starting.Traits);
			},
			msg =>
			{
				var error = Assert.IsType<IErrorMessage>(msg, exactMatch: false);
				Assert.Equal(-1, error.ExceptionParentIndices.Single());
				Assert.Equal(typeof(TestPipelineException).SafeName(), error.ExceptionTypes.Single());
				Assert.Equal("Test process crashed or communication channel was lost.", error.Messages.Single());
				Assert.Null(error.StackTraces.Single());
			},
			msg =>
			{
				// We don't know what the final totals are supposed to be, so we set it as 1 run, 1 failed
				var finished = Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false);
				Assert.Equal(assemblyUniqueID, finished.AssemblyUniqueID);
				Assert.Equal(0, finished.ExecutionTime);
				Assert.Equal(now, finished.FinishTime);
				Assert.Equal(1, finished.TestsFailed);
				Assert.Equal(0, finished.TestsNotRun);
				Assert.Equal(0, finished.TestsSkipped);
				Assert.Equal(1, finished.TestsTotal);
			}
		);
	}

	[Fact]
	public static void WithStarting_WithoutFinished_SendsError_SendsFinished()
	{
		var innerSink = SpyMessageSink.Capture();
		var now = DateTimeOffset.UtcNow;
		var sink = new CrashDetectionExecutionSinkWithFixedTime(projectAssembly, innerSink, now);
		var starting = TestData.TestAssemblyStarting();

		sink.OnMessage(starting);
		sink.OnProcessFinished(testProcessWithExitCode.TryGetExitCode());

		Assert.Collection(
			innerSink.Messages,
			msg => Assert.Same(starting, msg),
			msg =>
			{
				var error = Assert.IsType<IErrorMessage>(msg, exactMatch: false);
				Assert.Equal(-1, error.ExceptionParentIndices.Single());
				Assert.Equal(typeof(TestPipelineException).SafeName(), error.ExceptionTypes.Single());
				Assert.Equal("Test process crashed with exit code 42.", error.Messages.Single());
				Assert.Null(error.StackTraces.Single());
			},
			msg =>
			{
				var finished = Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false);
				Assert.Equal(starting.AssemblyUniqueID, finished.AssemblyUniqueID);
				Assert.Equal(0, finished.ExecutionTime);
				Assert.Equal(now, finished.FinishTime);
				Assert.Equal(1, finished.TestsFailed);
				Assert.Equal(0, finished.TestsNotRun);
				Assert.Equal(0, finished.TestsSkipped);
				Assert.Equal(1, finished.TestsTotal);
			}
		);
	}

	[Fact]
	public static void WithStarting_WithFinished_SendsNothing()
	{
		var innerSink = SpyMessageSink.Capture();
		var sink = new CrashDetectionExecutionSink(projectAssembly, innerSink);
		var starting = TestData.TestAssemblyStarting();
		var finished = TestData.TestAssemblyFinished();

		sink.OnMessage(starting);
		sink.OnMessage(finished);
		sink.OnProcessFinished(testProcessWithExitCode.TryGetExitCode());

		Assert.Collection(
			innerSink.Messages,
			msg => Assert.Same(starting, msg),
			msg => Assert.Same(finished, msg)
		);
	}

	class CrashDetectionExecutionSinkWithFixedTime(
		XunitProjectAssembly projectAssembly,
		IMessageSink innerSink,
		DateTimeOffset now) :
			CrashDetectionExecutionSink(projectAssembly, innerSink)
	{
		protected override DateTimeOffset UtcNow => now;

		protected override int FinishWaitMilliseconds => 10;
	}
}
