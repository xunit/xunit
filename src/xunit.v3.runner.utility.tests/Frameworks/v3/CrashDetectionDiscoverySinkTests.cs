using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class CrashDetectionDiscoverySinkTests
{
	static readonly string assemblyFileName;
	static readonly XunitProjectAssembly projectAssembly;
	static readonly ITestProcessBase testProcess;
	static readonly ITestProcessBase testProcessWithExitCode;

	static CrashDetectionDiscoverySinkTests()
	{
#if XUNIT_AOT
		assemblyFileName = Path.Combine(AppContext.BaseDirectory, typeof(CrashDetectionExecutionSinkTests).Assembly.GetName().Name + ".dll").FindTestAssembly();
#else
		assemblyFileName = typeof(CrashDetectionExecutionSinkTests).Assembly.Location;
#endif
		var assemblyMetadata = AssemblyUtility.GetAssemblyMetadata(assemblyFileName);

		Assert.NotNull(assemblyMetadata);

		projectAssembly = new(new(), assemblyFileName, assemblyMetadata);

		testProcess = new SpyTestProcessBase();
		testProcessWithExitCode = new SpyTestProcessBaseWithExitCode { ExitCode = 42 };
	}

	[Fact]
	public static void WithoutStarting_WithoutComplete_SendsStarting_SendsError_SendsComplete()
	{
		var innerSink = SpyMessageSink.Capture();
		var sink = new CrashDetectionDiscoverySinkWithShortWait(projectAssembly, innerSink);
		var assemblyUniqueID = UniqueIDGenerator.ForAssembly(assemblyFileName, null);

		sink.OnProcessFinished(testProcess.TryGetExitCode());

		var assemblyName =
			assemblyFileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || assemblyFileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
				? Path.GetFileNameWithoutExtension(assemblyFileName)
				: Path.GetFileName(assemblyFileName);

		Assert.Collection(
			innerSink.Messages,
			msg =>
			{
				var starting = Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false);
				Assert.Equal(assemblyName, starting.AssemblyName);
				Assert.Equal(assemblyFileName, starting.AssemblyPath);
				Assert.Equal(assemblyUniqueID, starting.AssemblyUniqueID);
				Assert.Null(starting.ConfigFilePath);
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
				var complete = Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false);
				Assert.Equal(assemblyUniqueID, complete.AssemblyUniqueID);
				Assert.Equal(0, complete.TestCasesToRun);
			}
		);
	}

	[Fact]
	public static void WithStarting_WithoutComplete_SendsError_SendsComplete()
	{
		var innerSink = SpyMessageSink.Capture();
		var sink = new CrashDetectionDiscoverySinkWithShortWait(projectAssembly, innerSink);
		var starting = TestData.DiscoveryStarting();

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
				var complete = Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false);
				Assert.Equal(starting.AssemblyUniqueID, complete.AssemblyUniqueID);
				Assert.Equal(0, complete.TestCasesToRun);
			}
		);
	}

	[Fact]
	public static void WithStarting_WithComplete_SendsNothing()
	{
		var innerSink = SpyMessageSink.Capture();
		var sink = new CrashDetectionDiscoverySink(projectAssembly, innerSink);
		var starting = TestData.DiscoveryStarting();
		var complete = TestData.DiscoveryComplete();

		sink.OnMessage(starting);
		sink.OnMessage(complete);
		sink.OnProcessFinished(testProcessWithExitCode.TryGetExitCode());

		Assert.Collection(
			innerSink.Messages,
			msg => Assert.Same(starting, msg),
			msg => Assert.Same(complete, msg)
		);
	}

	class CrashDetectionDiscoverySinkWithShortWait(
		XunitProjectAssembly projectAssembly,
		IMessageSink innerSink) :
			CrashDetectionDiscoverySink(projectAssembly, innerSink)
	{
		protected override int FinishWaitMilliseconds => 10;
	}
}
