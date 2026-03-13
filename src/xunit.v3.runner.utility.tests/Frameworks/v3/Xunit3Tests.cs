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
		var newAssemblyPath = Assembly.AssemblyFileName.Replace("xunit.v3.runner.utility", "xunit.v3.assert");
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

		Assert.Throws<ArgumentNullException>("messageSink", () => xunit3.Run(null!, FrontControllerRunSettings.WithTestCaseIDs(ExecutionOptions, [])));
		Assert.Throws<ArgumentNullException>("settings", () => xunit3.Run(SpyMessageSink.Capture(), null!));
	}

	async ValueTask GathersAssemblyInformation(bool runInProcess)
	{
		var expectedUniqueID = UniqueIDGenerator.ForAssembly(
			Assembly.AssemblyFileName,
			Assembly.ConfigFileName
		);

		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, testProcessLauncher: GetTestProcessLauncher(runInProcess));

		Assert.False(xunit3.CanUseAppDomains);
#if NET472
		Assert.Equal(".NETFramework,Version=v4.7.2", xunit3.TargetFramework);
#elif NET8_0
		Assert.Equal(".NETCoreApp,Version=v8.0", xunit3.TargetFramework);
#elif NET9_0
		Assert.Equal(".NETCoreApp,Version=v9.0", xunit3.TargetFramework);
#else
#error Unknown target framework
#endif
		Assert.Equal(expectedUniqueID, xunit3.TestAssemblyUniqueID);
		Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(-dev)?(\+[0-9a-f]+)?)?", xunit3.TestFrameworkDisplayName);
	}

	[Theory]
	[InlineData(false)]
#if !XUNIT_AOT
	[InlineData(true)]
#endif
	public ValueTask GathersAssemblyInformation_Self(bool runInProcess) =>
		GathersAssemblyInformation(runInProcess);

	[Theory]
	[InlineData(false)]
#if !XUNIT_AOT
	[InlineData(true)]
#endif
	public ValueTask GathersAssemblyInformation_Other(bool runInProcess)
	{
		UseAssertTests();

		return GathersAssemblyInformation(runInProcess);
	}

	async ValueTask CanFindFilteredTestsAndRunThem_UsingFind_UsingRun(
		bool runInProcess,
		bool synchronousMessageReporting,
		string typeName,
		string methodName,
		string expectedFileName,
		int expectedLineNumber)
	{
		Assembly.Configuration.SynchronousMessageReporting = synchronousMessageReporting;

		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, testProcessLauncher: GetTestProcessLauncher(runInProcess));

		// Find
		var fullyQualifiedMethodName = typeName + "." + methodName;
		var filters = new XunitFilters();
		filters.AddIncludedMethodFilter(fullyQualifiedMethodName);

		var findMessageSink = SpyMessageSink<IDiscoveryComplete>.Create();
		xunit3.Find(findMessageSink, new FrontControllerFindSettings(DiscoveryOptions, filters));

		var findFinished = findMessageSink.Finished.WaitOne(60_000);
		Assert.True(findFinished, "Message sink did not see IDiscoveryComplete within 60 seconds");

		var testCases = findMessageSink.Messages.OfType<ITestCaseDiscovered>();
		var testCase = Assert.Single(testCases);
		Assert.Equal(fullyQualifiedMethodName, testCase.TestCaseDisplayName);
		Assert.Equal(expectedFileName, Path.GetFileName(testCase.SourceFilePath));
#if DEBUG
		Assert.Equal(expectedLineNumber, testCase.SourceLineNumber);
#else
		// We test for range here, because release PDBs can be slightly unpredictable
		Assert.InRange(testCase.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif

		// Run
		var runMessageSink = SpyMessageSink<ITestAssemblyFinished>.Create();
#if XUNIT_AOT
		Assert.Null(testCase.Serialization);
		xunit3.Run(runMessageSink, FrontControllerRunSettings.WithTestCaseIDs(ExecutionOptions, [testCase.UniqueID]));
#else
		Assert.NotNull(testCase.Serialization);
		xunit3.Run(runMessageSink, FrontControllerRunSettings.WithSerializedTestCases(ExecutionOptions, [testCase.Serialization]));
#endif

		var runFinished = runMessageSink.Finished.WaitOne(60_000);
		Assert.True(runFinished, "Message sink did not see ITestAssemblyFinished within 60 seconds");

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
#if !XUNIT_AOT
	[InlineData(true, false)]
	[InlineData(true, true)]
#endif
	public ValueTask CanFindFilteredTestsAndRunThem_UsingFind_UsingRun_Self(
		bool runInProcess,
		bool synchronousMessageReporting) =>
			CanFindFilteredTestsAndRunThem_UsingFind_UsingRun(runInProcess, synchronousMessageReporting, typeof(Xunit3Tests).SafeName(), nameof(GuardClauses_Ctor), "Xunit3Tests.cs", 28);

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
#if !XUNIT_AOT
	[InlineData(true, false)]
	[InlineData(true, true)]
#endif
	public ValueTask CanFindFilteredTestsAndRunThem_UsingFind_UsingRun_Other(
		bool runInProcess,
		bool synchronousMessageReporting)
	{
		UseAssertTests();

		return CanFindFilteredTestsAndRunThem_UsingFind_UsingRun(runInProcess, synchronousMessageReporting, "BooleanAssertsTests+True", "AssertTrue", "BooleanAssertsTests.cs", 56);
	}

	async ValueTask CanFindFilteredTestsAndRunThem_UsingFindAndRun(
		bool runInProcess,
		bool synchronousMessageReporting,
		string typeName,
		string methodName)
	{
		Assembly.Configuration.SynchronousMessageReporting = synchronousMessageReporting;

		var fullyQualifiedMethodName = typeName + "." + methodName;

		await using var xunit3 = Xunit3.ForDiscoveryAndExecution(Assembly, testProcessLauncher: GetTestProcessLauncher(runInProcess));
		var filters = new XunitFilters();
		filters.AddIncludedMethodFilter(fullyQualifiedMethodName);
		var messageSink = SpyMessageSink<ITestAssemblyFinished>.Create();
		xunit3.FindAndRun(messageSink, new FrontControllerFindAndRunSettings(DiscoveryOptions, ExecutionOptions, filters));

		var finished = messageSink.Finished.WaitOne(60_000);
		Assert.True(finished, "Message sink did not see IDiscoveryComplete within 60 seconds");

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
#if !XUNIT_AOT
	[InlineData(true, false)]
	[InlineData(true, true)]
#endif
	public ValueTask CanFindFilteredTestsAndRunThem_UsingFindAndRun_Self(
		bool runInProcess,
		bool synchronousMessageReporting) =>
			CanFindFilteredTestsAndRunThem_UsingFindAndRun(runInProcess, synchronousMessageReporting, typeof(Xunit3Tests).SafeName(), nameof(GuardClauses_Ctor));

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
#if !XUNIT_AOT
	[InlineData(true, false)]
	[InlineData(true, true)]
#endif
	public ValueTask CanFindFilteredTestsAndRunThem_UsingFindAndRun_Other(
		bool runInProcess,
		bool synchronousMessageReporting)
	{
		UseAssertTests();

		return CanFindFilteredTestsAndRunThem_UsingFindAndRun(runInProcess, synchronousMessageReporting, "BooleanAssertsTests+True", "AssertTrue");
	}

	static ITestProcessLauncher GetTestProcessLauncher(bool runInProcess) =>
		runInProcess
#if XUNIT_AOT
			? throw new NotSupportedException("Native AOT does not support in-process testing")
#else
			? InProcessTestProcessLauncher.Instance
#endif
			: LocalOutOfProcessTestProcessLauncher.Instance;
}
