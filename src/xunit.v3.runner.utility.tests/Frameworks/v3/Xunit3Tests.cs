using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.Sdk;

[Collection(typeof(Xunit3Tests))]
[CollectionDefinition(DisableParallelization = true)]
public class Xunit3Tests
{
	readonly XunitProjectAssembly Assembly;
	readonly ITestFrameworkDiscoveryOptions DiscoveryOptions = TestData.TestFrameworkDiscoveryOptions(includeSourceInformation: true);
	readonly ITestFrameworkExecutionOptions ExecutionOptions = TestData.TestFrameworkExecutionOptions();

	public Xunit3Tests()
	{
		Assembly = TestData.XunitProjectAssembly<Xunit3Tests>();
	}

	[Fact]
	public void GuardClauses_Ctor()
	{
		Assert.Throws<ArgumentNullException>("projectAssembly", () => Xunit3.ForDiscoveryAndExecution(null!));

		var assembly = new XunitProjectAssembly(new XunitProject(), "/this/file/does/not/exist.exe", new(3, ".NETCoreApp,Version=v6.0"));
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

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async ValueTask GathersAssemblyInformation(bool forceInProcess)
	{
		var expectedUniqueID = UniqueIDGenerator.ForAssembly(
			Assembly.AssemblyFileName,
			Assembly.ConfigFileName
		);

		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, forceInProcess: forceInProcess);

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

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async ValueTask CanFindFilteredTestsAndRunThem_UsingFind_UsingRun(bool forceInProcess)
	{
		var sourceInformationProvider = Substitute.For<ISourceInformationProvider, InterfaceProxy<ISourceInformationProvider>>();
		sourceInformationProvider.GetSourceInformation("Xunit3Tests", "GuardClauses_Ctor").Returns(("/path/to/source/file.cs", 2112));
		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, sourceInformationProvider, forceInProcess: forceInProcess);

		// Find
		var filters = new XunitFilters();
		filters.IncludedMethods.Add($"{typeof(Xunit3Tests).FullName}.{nameof(GuardClauses_Ctor)}");

		var findMessageSink = SpyMessageSink<IDiscoveryComplete>.Create();
		var findProcess = xunit3.Find(findMessageSink, new FrontControllerFindSettings(DiscoveryOptions, filters));
		if (forceInProcess)
			Assert.Null(findProcess);
		else
			Assert.NotNull(findProcess);

		var findFinished = findMessageSink.Finished.WaitOne(60_000);
		Assert.True(findFinished, "Message sink did not see _DiscoveryComplete within 60 seconds");

		var testCases = findMessageSink.Messages.OfType<ITestCaseDiscovered>();
		var testCase = Assert.Single(testCases);
		Assert.Equal("Xunit3Tests.GuardClauses_Ctor", testCase.TestCaseDisplayName);
		Assert.Equal("/path/to/source/file.cs", testCase.SourceFilePath);
		Assert.Equal(2112, testCase.SourceLineNumber);

		// Run
		var runMessageSink = SpyMessageSink<ITestAssemblyFinished>.Create();
		var runProcess = xunit3.Run(runMessageSink, new FrontControllerRunSettings(ExecutionOptions, [testCase.Serialization]));
		if (forceInProcess)
			Assert.Null(runProcess);
		else
			Assert.NotNull(runProcess);

		var runFinished = runMessageSink.Finished.WaitOne(60_000);
		Assert.True(runFinished, "Message sink did not see _TestAssemblyFinished within 60 seconds");

		var results = runMessageSink.Messages.OfType<ITestResultMessage>().ToList();
		var passed = Assert.Single(runMessageSink.Messages.OfType<ITestPassed>());
		Assert.Equal(testCase.TestCaseUniqueID, passed.TestCaseUniqueID);
		Assert.Empty(results.OfType<ITestFailed>());
		Assert.Empty(results.OfType<ITestSkipped>());
		Assert.Empty(results.OfType<ITestNotRun>());
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async ValueTask CanFindFilteredTestsAndRunThem_UsingFindAndRun(bool forceInProcess)
	{
		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, forceInProcess: forceInProcess);
		var filters = new XunitFilters();
		filters.IncludedMethods.Add($"{typeof(Xunit3Tests).FullName}.{nameof(GuardClauses_Ctor)}");
		var messageSink = SpyMessageSink<ITestAssemblyFinished>.Create();
		var process = xunit3.FindAndRun(messageSink, new FrontControllerFindAndRunSettings(DiscoveryOptions, ExecutionOptions, filters));

		if (forceInProcess)
			Assert.Null(process);
		else
			Assert.NotNull(process);

		var finished = messageSink.Finished.WaitOne(60_000);
		Assert.True(finished, "Message sink did not see _DiscoveryComplete within 60 seconds");

		var starting = Assert.Single(messageSink.Messages.OfType<ITestStarting>());
		Assert.Equal("Xunit3Tests.GuardClauses_Ctor", starting.TestDisplayName);
		var passed = Assert.Single(messageSink.Messages.OfType<ITestPassed>());
		Assert.Equal(starting.TestUniqueID, passed.TestUniqueID);
		Assert.Empty(messageSink.Messages.OfType<ITestFailed>());
		Assert.Empty(messageSink.Messages.OfType<ITestSkipped>());
		Assert.Empty(messageSink.Messages.OfType<ITestNotRun>());
	}
}
