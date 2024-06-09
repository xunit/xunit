using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.Sdk;
using Xunit.v3;

public class Xunit3Tests
{
	readonly XunitProjectAssembly Assembly;
	readonly _ITestFrameworkDiscoveryOptions DiscoveryOptions = TestData.TestFrameworkDiscoveryOptions();
	readonly _ITestFrameworkExecutionOptions ExecutionOptions = TestData.TestFrameworkExecutionOptions();

	public Xunit3Tests()
	{
		Assembly = new XunitProjectAssembly(new XunitProject())
		{
			Assembly = typeof(Xunit3Tests).Assembly,
			AssemblyFileName = typeof(Xunit3Tests).Assembly.Location,
			AssemblyMetadata = AssemblyUtility.GetAssemblyMetadata(typeof(Xunit3Tests).Assembly.Location),
		};
	}

	[Fact]
	public void GuardClauses_Ctor()
	{
		Assert.Throws<ArgumentNullException>("projectAssembly", () => Xunit3.ForDiscoveryAndExecution(null!));

		var assembly = new XunitProjectAssembly(new XunitProject()) { AssemblyFileName = "/this/file/does/not/exist.exe", AssemblyMetadata = new(3, ".NETCoreApp,Version=v6.0") };
		var argEx = Assert.Throws<ArgumentException>("projectAssembly.AssemblyFileName", () => Xunit3.ForDiscoveryAndExecution(assembly));
		Assert.StartsWith("File not found: /this/file/does/not/exist.exe", argEx.Message);
	}

	[Fact]
	public async ValueTask GuardClauses_Find()
	{
		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly);

		Assert.Throws<ArgumentNullException>("messageSink", () => xunit3.Find(null!, new FrontControllerFindSettings(DiscoveryOptions)));
		Assert.Throws<ArgumentNullException>("settings", () => xunit3.Find(SpyMessageSink.Capture(), null!));
	}

	[Fact]
	public async ValueTask GathersAssemblyInformation()
	{
		var expectedUniqueID = UniqueIDGenerator.ForAssembly(
			Assembly.AssemblyDisplayName,
			Assembly.AssemblyFileName,
			Assembly.ConfigFileName
		);

		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly);

		Assert.False(xunit3.CanUseAppDomains);
#if NET472
		Assert.Equal(".NETFramework,Version=v4.7.2", xunit3.TargetFramework);
#elif NET6_0
		Assert.Equal(".NETCoreApp,Version=v6.0", xunit3.TargetFramework);
#else
#error Unknown target framework
#endif
		Assert.Equal(expectedUniqueID, xunit3.TestAssemblyUniqueID);
		Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(-dev)?(\+[0-9a-f]+)?)?", xunit3.TestFrameworkDisplayName);
	}

	[Fact]
	public async ValueTask CanFindFilteredTestsAndRunThem_UsingFind_UsingRun()
	{
		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly);

		// Find
		var filters = new XunitFilters();
		filters.IncludedMethods.Add($"{typeof(Xunit3Tests).FullName}.{nameof(GuardClauses_Ctor)}");

		var findMessageSink = SpyMessageSink<_DiscoveryComplete>.Create();
		var findProcess = xunit3.Find(findMessageSink, new FrontControllerFindSettings(DiscoveryOptions, filters));
		Assert.NotNull(findProcess);

		var findFinished = findMessageSink.Finished.WaitOne(60_000);
		Assert.True(findFinished, "Message sink did not see _DiscoveryComplete within 60 seconds");

		var testCases = findMessageSink.Messages.OfType<_TestCaseDiscovered>();
		var testCase = Assert.Single(testCases);
		Assert.Equal("Xunit3Tests.GuardClauses_Ctor", testCase.TestCaseDisplayName);

		// Run
		var runMessageSink = SpyMessageSink<_TestAssemblyFinished>.Create();
		var runProcess = xunit3.Run(runMessageSink, new FrontControllerRunSettings(ExecutionOptions, [testCase.Serialization]));
		Assert.NotNull(runProcess);

		var runFinished = runMessageSink.Finished.WaitOne(60_000);
		Assert.True(runFinished, "Message sink did not see _TestAssemblyFinished within 60 seconds");

		var results = runMessageSink.Messages.OfType<_TestResultMessage>().ToList();
		var passed = Assert.Single(runMessageSink.Messages.OfType<_TestPassed>());
		Assert.Equal(testCase.TestCaseUniqueID, passed.TestCaseUniqueID);
		Assert.Empty(results.OfType<_TestFailed>());
		Assert.Empty(results.OfType<_TestSkipped>());
		Assert.Empty(results.OfType<_TestNotRun>());
	}

	[Fact]
	public async ValueTask CanFindFilteredTestsAndRunThem_UsingFindAndRun()
	{
		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly);
		var filters = new XunitFilters();
		filters.IncludedMethods.Add($"{typeof(Xunit3Tests).FullName}.{nameof(GuardClauses_Ctor)}");
		var messageSink = SpyMessageSink<_TestAssemblyFinished>.Create();
		var process = xunit3.FindAndRun(messageSink, new FrontControllerFindAndRunSettings(DiscoveryOptions, ExecutionOptions, filters));
		Assert.NotNull(process);

		var finished = messageSink.Finished.WaitOne(60_000);
		Assert.True(finished, "Message sink did not see _DiscoveryComplete within 60 seconds");

		var starting = Assert.Single(messageSink.Messages.OfType<_TestStarting>());
		Assert.Equal("Xunit3Tests.GuardClauses_Ctor", starting.TestDisplayName);
		var passed = Assert.Single(messageSink.Messages.OfType<_TestPassed>());
		Assert.Equal(starting.TestUniqueID, passed.TestUniqueID);
		Assert.Empty(messageSink.Messages.OfType<_TestFailed>());
		Assert.Empty(messageSink.Messages.OfType<_TestSkipped>());
		Assert.Empty(messageSink.Messages.OfType<_TestNotRun>());
	}
}
