using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class XunitProjectAssemblyTests
{
	public class WithSettings
	{
		readonly ITestFrameworkDiscoveryOptions discoveryOptions;
		readonly ITestFrameworkExecutionOptions executionOptions;
		readonly XunitFilters filters;

		public WithSettings()
		{
			// Common options are set with opposite values in discovery vs. execution so that we
			// can tell when precedence is being followed.

			discoveryOptions = TestFrameworkOptions.ForDiscoveryFromSerialization("{}");
			discoveryOptions.SetCulture("ab-CD");
			discoveryOptions.SetDiagnosticMessages(true);
			discoveryOptions.SetIncludeSourceInformation(true);
			discoveryOptions.SetInternalDiagnosticMessages(true);
			discoveryOptions.SetMethodDisplay(TestMethodDisplay.Method);
			discoveryOptions.SetMethodDisplayOptions(TestMethodDisplayOptions.ReplaceUnderscoreWithSpace);
			discoveryOptions.SetPreEnumerateTheories(true);
			discoveryOptions.SetPrintMaxEnumerableLength(12);
			discoveryOptions.SetPrintMaxObjectDepth(34);
			discoveryOptions.SetPrintMaxObjectMemberCount(56);
			discoveryOptions.SetPrintMaxStringLength(78);
			discoveryOptions.SetSynchronousMessageReporting(true);

			executionOptions = TestFrameworkOptions.ForExecutionFromSerialization("{}");
			executionOptions.SetCulture("ef-GH");
			executionOptions.SetDiagnosticMessages(false);
			executionOptions.SetDisableParallelization(true);  // true instead of false because it's inverted in the result
			executionOptions.SetExplicitOption(ExplicitOption.On);
			executionOptions.SetFailSkips(false);
			executionOptions.SetFailTestsWithWarnings(false);
			executionOptions.SetInternalDiagnosticMessages(false);
			executionOptions.SetMaxParallelThreads(2112);
			executionOptions.SetParallelAlgorithm(ParallelAlgorithm.Aggressive);
			executionOptions.SetPrintMaxEnumerableLength(21);
			executionOptions.SetPrintMaxObjectDepth(43);
			executionOptions.SetPrintMaxObjectMemberCount(65);
			executionOptions.SetPrintMaxStringLength(87);
			executionOptions.SetSeed(2600);
			executionOptions.SetShowLiveOutput(false);
			executionOptions.SetStopOnTestFail(false);
			executionOptions.SetSynchronousMessageReporting(false);

			filters = new();
			filters.AddIncludedClassFilter("myClass");
		}

		[Fact]
		public void FindSettings()
		{
			var projectAssembly = TestData.XunitProjectAssembly<XunitProjectAssemblyTests>();
			var settings = new FrontControllerFindSettings(discoveryOptions, filters);
			settings.LaunchOptions.WaitForDebugger = true;

			var updatedAssembly = projectAssembly.WithSettings(settings);

			Assert.Equal("ab-CD", updatedAssembly.Configuration.Culture);
			Assert.True(updatedAssembly.Configuration.DiagnosticMessages);
			Assert.True(updatedAssembly.Configuration.IncludeSourceInformation);
			Assert.True(updatedAssembly.Configuration.InternalDiagnosticMessages);
			Assert.Equal(TestMethodDisplay.Method, updatedAssembly.Configuration.MethodDisplay);
			Assert.Equal(TestMethodDisplayOptions.ReplaceUnderscoreWithSpace, updatedAssembly.Configuration.MethodDisplayOptions);
			Assert.True(updatedAssembly.Configuration.PreEnumerateTheories);
			Assert.Equal(12, updatedAssembly.Configuration.PrintMaxEnumerableLength);
			Assert.Equal(34, updatedAssembly.Configuration.PrintMaxObjectDepth);
			Assert.Equal(56, updatedAssembly.Configuration.PrintMaxObjectMemberCount);
			Assert.Equal(78, updatedAssembly.Configuration.PrintMaxStringLength);
			Assert.True(updatedAssembly.Configuration.SynchronousMessageReporting);
			Assert.True(updatedAssembly.Project.Configuration.WaitForDebugger);
			Assert.Empty(updatedAssembly.TestCasesToRun);
			Assert.Same(filters, updatedAssembly.Configuration.Filters);
		}

		[Fact]
		public void FindAndRunSettings()
		{
			var projectAssembly = TestData.XunitProjectAssembly<XunitProjectAssemblyTests>();
			var settings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions, filters);
			settings.LaunchOptions.WaitForDebugger = true;

			var updatedAssembly = projectAssembly.WithSettings(settings);

			Assert.Equal("ef-GH", updatedAssembly.Configuration.Culture);
			Assert.False(updatedAssembly.Configuration.DiagnosticMessages);
			Assert.Equal(ExplicitOption.On, updatedAssembly.Configuration.ExplicitOption);
			Assert.False(updatedAssembly.Configuration.FailSkips);
			Assert.False(updatedAssembly.Configuration.FailTestsWithWarnings);
			Assert.True(updatedAssembly.Configuration.IncludeSourceInformation);
			Assert.False(updatedAssembly.Configuration.InternalDiagnosticMessages);
			Assert.Equal(2112, updatedAssembly.Configuration.MaxParallelThreads);
			Assert.Equal(ParallelAlgorithm.Aggressive, updatedAssembly.Configuration.ParallelAlgorithm);
			Assert.Equal(TestMethodDisplay.Method, updatedAssembly.Configuration.MethodDisplay);
			Assert.Equal(TestMethodDisplayOptions.ReplaceUnderscoreWithSpace, updatedAssembly.Configuration.MethodDisplayOptions);
			Assert.False(updatedAssembly.Configuration.ParallelizeAssembly);
			Assert.False(updatedAssembly.Configuration.ParallelizeTestCollections);
			Assert.True(updatedAssembly.Configuration.PreEnumerateTheories);
			Assert.Equal(21, updatedAssembly.Configuration.PrintMaxEnumerableLength);
			Assert.Equal(43, updatedAssembly.Configuration.PrintMaxObjectDepth);
			Assert.Equal(65, updatedAssembly.Configuration.PrintMaxObjectMemberCount);
			Assert.Equal(87, updatedAssembly.Configuration.PrintMaxStringLength);
			Assert.Equal(2600, updatedAssembly.Configuration.Seed);
			Assert.False(updatedAssembly.Configuration.ShowLiveOutput);
			Assert.False(updatedAssembly.Configuration.StopOnFail);
			Assert.False(updatedAssembly.Configuration.SynchronousMessageReporting);
			Assert.True(updatedAssembly.Project.Configuration.WaitForDebugger);
			Assert.Empty(updatedAssembly.TestCasesToRun);
			Assert.Same(filters, updatedAssembly.Configuration.Filters);
		}

		[Fact]
		public void RunSettings()
		{
			var projectAssembly = TestData.XunitProjectAssembly<XunitProjectAssemblyTests>();
			var settings = new FrontControllerRunSettings(executionOptions, ["test-1", "test-2"]);
			settings.LaunchOptions.WaitForDebugger = true;

			var updatedAssembly = projectAssembly.WithSettings(settings);

			Assert.Equal("ef-GH", updatedAssembly.Configuration.Culture);
			Assert.False(updatedAssembly.Configuration.DiagnosticMessages);
			Assert.Equal(ExplicitOption.On, updatedAssembly.Configuration.ExplicitOption);
			Assert.False(updatedAssembly.Configuration.FailSkips);
			Assert.False(updatedAssembly.Configuration.FailTestsWithWarnings);
			Assert.False(updatedAssembly.Configuration.InternalDiagnosticMessages);
			Assert.Equal(2112, updatedAssembly.Configuration.MaxParallelThreads);
			Assert.Equal(ParallelAlgorithm.Aggressive, updatedAssembly.Configuration.ParallelAlgorithm);
			Assert.False(updatedAssembly.Configuration.ParallelizeAssembly);
			Assert.False(updatedAssembly.Configuration.ParallelizeTestCollections);
			Assert.Equal(21, updatedAssembly.Configuration.PrintMaxEnumerableLength);
			Assert.Equal(43, updatedAssembly.Configuration.PrintMaxObjectDepth);
			Assert.Equal(65, updatedAssembly.Configuration.PrintMaxObjectMemberCount);
			Assert.Equal(87, updatedAssembly.Configuration.PrintMaxStringLength);
			Assert.Equal(2600, updatedAssembly.Configuration.Seed);
			Assert.False(updatedAssembly.Configuration.ShowLiveOutput);
			Assert.False(updatedAssembly.Configuration.StopOnFail);
			Assert.False(updatedAssembly.Configuration.SynchronousMessageReporting);
			Assert.Equal(["test-1", "test-2"], updatedAssembly.TestCasesToRun);
			Assert.True(updatedAssembly.Project.Configuration.WaitForDebugger);
		}
	}
}
