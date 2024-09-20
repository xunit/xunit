using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.Sdk;
using Xunit.v3;

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

	void UseAssertTests()
	{
		// Make sure we're not relying on Assembly.GetEntryAssembly()
#if BUILD_X86
		var newAssemblyPath = Assembly.AssemblyFileName.Replace("xunit.v3.runner.utility.x86.tests", "xunit.v3.assert.x86.tests");
#else
		var newAssemblyPath = Assembly.AssemblyFileName.Replace("xunit.v3.runner.utility.tests", "xunit.v3.assert.tests");
#endif

		Assert.NotEqual(newAssemblyPath, Assembly.AssemblyFileName);
		Assembly.AssemblyFileName = newAssemblyPath;
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

	[Fact]
	public async ValueTask GuardClauses_FindAndRun()
	{
		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly);

		Assert.Throws<ArgumentNullException>("messageSink", () => xunit3.FindAndRun(null!, new FrontControllerFindAndRunSettings(DiscoveryOptions, ExecutionOptions)));
		Assert.Throws<ArgumentNullException>("settings", () => xunit3.FindAndRun(SpyMessageSink.Capture(), null!));
	}

	[Fact]
	public async ValueTask GuardClauses_Run()
	{
		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly);

		Assert.Throws<ArgumentNullException>("messageSink", () => xunit3.Run(null!, new FrontControllerRunSettings(ExecutionOptions, [])));
		Assert.Throws<ArgumentNullException>("settings", () => xunit3.Run(SpyMessageSink.Capture(), null!));
	}

	async ValueTask GathersAssemblyInformation(bool runInProcess)
	{
		var expectedUniqueID = UniqueIDGenerator.ForAssembly(
			Assembly.AssemblyFileName,
			Assembly.ConfigFileName
		);

		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, testProcessLauncher: runInProcess ? InProcessTestProcessLauncher.Instance : LocalOutOfProcessTestProcessLauncher.Instance);

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
	public ValueTask GathersAssemblyInformation_Self(bool runInProcess) =>
		GathersAssemblyInformation(runInProcess);

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public ValueTask GathersAssemblyInformation_Other(bool runInProcess)
	{
		UseAssertTests();

		return GathersAssemblyInformation(runInProcess);
	}

	async ValueTask CanFindFilteredTestsAndRunThem_UsingFind_UsingRun(
		bool runInProcess,
		bool synchronousMessageReporting,
		string typeName,
		string methodName)
	{
		Assembly.Configuration.SynchronousMessageReporting = synchronousMessageReporting;

		var sourceInformationProvider = Substitute.For<ISourceInformationProvider, InterfaceProxy<ISourceInformationProvider>>();
		sourceInformationProvider.GetSourceInformation(typeName, methodName).Returns(new SourceInformation("/path/to/source/file.cs", 2112));
		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, sourceInformationProvider, testProcessLauncher: runInProcess ? InProcessTestProcessLauncher.Instance : LocalOutOfProcessTestProcessLauncher.Instance);

		// Find
		var fullyQualifiedMethodName = typeName + "." + methodName;
		var filters = new XunitFilters();
		filters.AddIncludedMethodFilter(fullyQualifiedMethodName);

		var findMessageSink = SpyMessageSink<IDiscoveryComplete>.Create();
		xunit3.Find(findMessageSink, new FrontControllerFindSettings(DiscoveryOptions, filters));

		var findFinished = findMessageSink.Finished.WaitOne(60_000);
		Assert.True(findFinished, "Message sink did not see _DiscoveryComplete within 60 seconds");

		var testCases = findMessageSink.Messages.OfType<ITestCaseDiscovered>();
		var testCase = Assert.Single(testCases);
		Assert.Equal(fullyQualifiedMethodName, testCase.TestCaseDisplayName);
		Assert.Equal("/path/to/source/file.cs", testCase.SourceFilePath);
		Assert.Equal(2112, testCase.SourceLineNumber);

		// Run
		var runMessageSink = SpyMessageSink<ITestAssemblyFinished>.Create();
		xunit3.Run(runMessageSink, new FrontControllerRunSettings(ExecutionOptions, [testCase.Serialization]));

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
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public ValueTask CanFindFilteredTestsAndRunThem_UsingFind_UsingRun_Self(
		bool runInProcess,
		bool synchronousMessageReporting) =>
			CanFindFilteredTestsAndRunThem_UsingFind_UsingRun(runInProcess, synchronousMessageReporting, typeof(Xunit3Tests).SafeName(), nameof(GuardClauses_Ctor));

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public ValueTask CanFindFilteredTestsAndRunThem_UsingFind_UsingRun_Other(
		bool runInProcess,
		bool synchronousMessageReporting)
	{
		UseAssertTests();

		return CanFindFilteredTestsAndRunThem_UsingFind_UsingRun(runInProcess, synchronousMessageReporting, "BooleanAssertsTests+True", "AssertTrue");
	}

	async ValueTask CanFindFilteredTestsAndRunThem_UsingFindAndRun(
		bool runInProcess,
		bool synchronousMessageReporting,
		string typeName,
		string methodName)
	{
		Assembly.Configuration.SynchronousMessageReporting = synchronousMessageReporting;

		var fullyQualifiedMethodName = typeName + "." + methodName;

		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, testProcessLauncher: runInProcess ? InProcessTestProcessLauncher.Instance : LocalOutOfProcessTestProcessLauncher.Instance);
		var filters = new XunitFilters();
		filters.AddIncludedMethodFilter(fullyQualifiedMethodName);
		var messageSink = SpyMessageSink<ITestAssemblyFinished>.Create();
		xunit3.FindAndRun(messageSink, new FrontControllerFindAndRunSettings(DiscoveryOptions, ExecutionOptions, filters));

		var finished = messageSink.Finished.WaitOne(60_000);
		Assert.True(finished, "Message sink did not see _DiscoveryComplete within 60 seconds");

		var starting = Assert.Single(messageSink.Messages.OfType<ITestStarting>());
		Assert.Equal(fullyQualifiedMethodName, starting.TestDisplayName);
		var passed = Assert.Single(messageSink.Messages.OfType<ITestPassed>());
		Assert.Equal(starting.TestUniqueID, passed.TestUniqueID);
		Assert.Empty(messageSink.Messages.OfType<ITestFailed>());
		Assert.Empty(messageSink.Messages.OfType<ITestSkipped>());
		Assert.Empty(messageSink.Messages.OfType<ITestNotRun>());
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public ValueTask CanFindFilteredTestsAndRunThem_UsingFindAndRun_Self(
		bool runInProcess,
		bool synchronousMessageReporting) =>
			CanFindFilteredTestsAndRunThem_UsingFindAndRun(runInProcess, synchronousMessageReporting, typeof(Xunit3Tests).SafeName(), nameof(GuardClauses_Ctor));

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public ValueTask CanFindFilteredTestsAndRunThem_UsingFindAndRun_Other(
		bool runInProcess,
		bool synchronousMessageReporting)
	{
		UseAssertTests();

		return CanFindFilteredTestsAndRunThem_UsingFindAndRun(runInProcess, synchronousMessageReporting, "BooleanAssertsTests+True", "AssertTrue");
	}
}
