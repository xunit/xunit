using System;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.Sdk;

public class Xunit3ArgumentFactoryTests
{
	public static readonly Version Version_0_2_999 = new(0, 2, 999);
	public static readonly Version Version_0_3_0 = new(0, 3, 0);

	public class ForFind
	{
		[Fact]
		public void DefaultOptions()
		{
			var options = TestData.TestFrameworkDiscoveryOptions();

			var arguments = Xunit3ArgumentFactory.ForFind(Version_0_3_0, options);

			var arg = Assert.Single(arguments);
			Assert.Equal("-automated", arg);
		}

		[Theory]
		[InlineData("", "invariant")]
		[InlineData("en-US", "en-US")]
		public void FullOptions(
			string culture,
			string expectedCulture)
		{
			// We ignore includeSourceInformation and synchronousMessageReporting because they're processed locally, not remotely
			var options = TestData.TestFrameworkDiscoveryOptions(
				culture: culture,
				diagnosticMessages: true,
				internalDiagnosticMessages: true,
				includeSourceInformation: true,
				methodDisplay: TestMethodDisplay.Method,
				methodDisplayOptions: TestMethodDisplayOptions.ReplacePeriodWithComma | TestMethodDisplayOptions.UseOperatorMonikers,
				preEnumerateTheories: true,
				synchronousMessageReporting: true
			);

			var arguments = Xunit3ArgumentFactory.ForFind(Version_0_3_0, options);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("sync", arg),
				arg => Assert.Equal("-culture", arg),
				arg => Assert.Equal(expectedCulture, arg),
				arg => Assert.Equal("-diagnostics", arg),
				arg => Assert.Equal("-internalDiagnostics", arg),
				arg => Assert.Equal("-methodDisplay", arg),
				arg => Assert.Equal("Method", arg),
				arg => Assert.Equal("-methodDisplayOptions", arg),
				arg => Assert.Equal("UseOperatorMonikers,ReplacePeriodWithComma", arg),
				arg => Assert.Equal("-preEnumerateTheories", arg)
			);
		}

		[Fact]
		public void DoesNotSendSyncParameterToAutomatedForOlderTestProjects()
		{
			var options = TestData.TestFrameworkDiscoveryOptions(synchronousMessageReporting: true);

			var arguments = Xunit3ArgumentFactory.ForFind(Version_0_2_999, options);

			var argument = Assert.Single(arguments);
			Assert.Equal("-automated", argument);
		}

		[Fact]
		public void AddConfigFile()
		{
			var options = TestData.TestFrameworkDiscoveryOptions();

			var arguments = Xunit3ArgumentFactory.ForFind(Version_0_3_0, options, configFileName: "/config/file/name.json");

			Assert.Collection(
				arguments,
				arg => Assert.Equal("/config/file/name.json", arg),
				arg => Assert.Equal("-automated", arg)
			);
		}

		[Fact]
		public void AddFilters()
		{
			var options = TestData.TestFrameworkDiscoveryOptions();
			var filters = new XunitFilters();
			filters.AddIncludedClassFilter("class1");
			filters.AddIncludedClassFilter("class2");
			filters.AddExcludedClassFilter("class3");
			filters.AddExcludedClassFilter("class4");
			filters.AddIncludedMethodFilter("method1");
			filters.AddIncludedMethodFilter("method2");
			filters.AddExcludedMethodFilter("method3");
			filters.AddExcludedMethodFilter("method4");
			filters.AddIncludedNamespaceFilter("namespace1");
			filters.AddIncludedNamespaceFilter("namespace2");
			filters.AddExcludedNamespaceFilter("namespace3");
			filters.AddExcludedNamespaceFilter("namespace4");
			filters.AddIncludedTraitFilter("trait1", "value1a");
			filters.AddIncludedTraitFilter("trait1", "value1b");
			filters.AddIncludedTraitFilter("trait2", "value2a");
			filters.AddIncludedTraitFilter("trait2", "value2b");
			filters.AddExcludedTraitFilter("trait3", "value3a");
			filters.AddExcludedTraitFilter("trait3", "value3b");
			filters.AddExcludedTraitFilter("trait4", "value4a");
			filters.AddExcludedTraitFilter("trait4", "value4b");

			var arguments = Xunit3ArgumentFactory.ForFind(Version_0_3_0, options, filters: filters);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),

				arg => Assert.Equal("-class", arg),
				arg => Assert.Equal("class1", arg),
				arg => Assert.Equal("-class", arg),
				arg => Assert.Equal("class2", arg),

				arg => Assert.Equal("-class-", arg),
				arg => Assert.Equal("class3", arg),
				arg => Assert.Equal("-class-", arg),
				arg => Assert.Equal("class4", arg),

				arg => Assert.Equal("-method", arg),
				arg => Assert.Equal("method1", arg),
				arg => Assert.Equal("-method", arg),
				arg => Assert.Equal("method2", arg),

				arg => Assert.Equal("-method-", arg),
				arg => Assert.Equal("method3", arg),
				arg => Assert.Equal("-method-", arg),
				arg => Assert.Equal("method4", arg),

				arg => Assert.Equal("-namespace", arg),
				arg => Assert.Equal("namespace1", arg),
				arg => Assert.Equal("-namespace", arg),
				arg => Assert.Equal("namespace2", arg),

				arg => Assert.Equal("-namespace-", arg),
				arg => Assert.Equal("namespace3", arg),
				arg => Assert.Equal("-namespace-", arg),
				arg => Assert.Equal("namespace4", arg),

				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("trait1=value1a", arg),
				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("trait1=value1b", arg),
				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("trait2=value2a", arg),
				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("trait2=value2b", arg),

				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("trait3=value3a", arg),
				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("trait3=value3b", arg),
				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("trait4=value4a", arg),
				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("trait4=value4b", arg)
			);
		}

		[Theory]
		[InlineData(ListOption.Classes)]
		[InlineData(ListOption.Full)]
		[InlineData(ListOption.Methods)]
		[InlineData(ListOption.Tests)]
		[InlineData(ListOption.Traits)]
		public void AddListOption(ListOption listOption)
		{
			var options = TestData.TestFrameworkDiscoveryOptions();

			var arguments = Xunit3ArgumentFactory.ForFind(Version_0_3_0, options, listOption: listOption);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("-list", arg),
				arg => Assert.Equal(listOption.ToString(), arg)
			);
		}

		[Fact]
		public void AddWaitForDebugger()
		{
			var options = TestData.TestFrameworkDiscoveryOptions();

			var arguments = Xunit3ArgumentFactory.ForFind(Version_0_3_0, options, waitForDebugger: true);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("-waitForDebugger", arg)
			);
		}
	}

	public class ForFindAndRun
	{
		[Fact]
		public void DefaultOptions()
		{
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();

			var arguments = Xunit3ArgumentFactory.ForFindAndRun(Version_0_3_0, discoveryOptions, executionOptions);

			var arg = Assert.Single(arguments);
			Assert.Equal("-automated", arg);
		}

		[Theory]
		[InlineData("", "invariant")]
		[InlineData("en-US", "en-US")]
		public void FullOptions(
			string culture,
			string expectedCulture)
		{
			// Execution options take priority, so we'll only provide unique-to-discovery options here
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(
				methodDisplay: TestMethodDisplay.Method,
				methodDisplayOptions: TestMethodDisplayOptions.ReplacePeriodWithComma | TestMethodDisplayOptions.UseOperatorMonikers,
				preEnumerateTheories: true
			);
			// We ignore synchronousMessageReporting because it's processed locally, not remotely
			var executionOptions = TestData.TestFrameworkExecutionOptions(
				culture: culture,
				diagnosticMessages: true,
				disableParallelization: true,
				explicitOption: ExplicitOption.On,
				failSkips: true,
				failTestsWithWarnings: true,
				internalDiagnosticMessages: true,
				maxParallelThreads: 42,
				parallelAlgorithm: ParallelAlgorithm.Conservative,
				seed: 2112,
				stopOnFail: true,
				synchronousMessageReporting: true
			);

			var arguments = Xunit3ArgumentFactory.ForFindAndRun(Version_0_3_0, discoveryOptions, executionOptions);

			Assert.Collection(
				arguments,
				arg => Assert.Equal(":2112", arg),
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("sync", arg),
				arg => Assert.Equal("-culture", arg),
				arg => Assert.Equal(expectedCulture, arg),
				arg => Assert.Equal("-diagnostics", arg),
				arg => Assert.Equal("-explicit", arg),
				arg => Assert.Equal("On", arg),
				arg => Assert.Equal("-failSkips", arg),
				arg => Assert.Equal("-failWarns", arg),
				arg => Assert.Equal("-internalDiagnostics", arg),
				arg => Assert.Equal("-maxThreads", arg),
				arg => Assert.Equal("42", arg),
				arg => Assert.Equal("-methodDisplay", arg),
				arg => Assert.Equal("Method", arg),
				arg => Assert.Equal("-methodDisplayOptions", arg),
				arg => Assert.Equal("UseOperatorMonikers,ReplacePeriodWithComma", arg),
				arg => Assert.Equal("-parallel", arg),
				arg => Assert.Equal("none", arg),
				arg => Assert.Equal("-parallelAlgorithm", arg),
				arg => Assert.Equal("Conservative", arg),
				arg => Assert.Equal("-preEnumerateTheories", arg),
				arg => Assert.Equal("-stopOnFail", arg)
			);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DoesNotSendSyncParameterToAutomatedForOlderTestProjects(bool discoverySynchronous)
		{
			// Either one turns it on, so test it both ways
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions(synchronousMessageReporting: discoverySynchronous);
			var executionOptions = TestData.TestFrameworkExecutionOptions(synchronousMessageReporting: !discoverySynchronous);

			var arguments = Xunit3ArgumentFactory.ForFindAndRun(Version_0_2_999, discoveryOptions, executionOptions);

			var argument = Assert.Single(arguments);
			Assert.Equal("-automated", argument);
		}

		[Fact]
		public void AddConfigFile()
		{
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();

			var arguments = Xunit3ArgumentFactory.ForFindAndRun(Version_0_3_0, discoveryOptions, executionOptions, configFileName: "/config/file/name.json");

			Assert.Collection(
				arguments,
				arg => Assert.Equal("/config/file/name.json", arg),
				arg => Assert.Equal("-automated", arg)
			);
		}

		[Fact]
		public void AddFilters()
		{
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();
			var filters = new XunitFilters();
			filters.AddIncludedClassFilter("class1");
			filters.AddIncludedClassFilter("class2");
			filters.AddExcludedClassFilter("class3");
			filters.AddExcludedClassFilter("class4");
			filters.AddIncludedMethodFilter("method1");
			filters.AddIncludedMethodFilter("method2");
			filters.AddExcludedMethodFilter("method3");
			filters.AddExcludedMethodFilter("method4");
			filters.AddIncludedNamespaceFilter("namespace1");
			filters.AddIncludedNamespaceFilter("namespace2");
			filters.AddExcludedNamespaceFilter("namespace3");
			filters.AddExcludedNamespaceFilter("namespace4");
			filters.AddIncludedTraitFilter("trait1", "value1a");
			filters.AddIncludedTraitFilter("trait1", "value1b");
			filters.AddIncludedTraitFilter("trait2", "value2a");
			filters.AddIncludedTraitFilter("trait2", "value2b");
			filters.AddExcludedTraitFilter("trait3", "value3a");
			filters.AddExcludedTraitFilter("trait3", "value3b");
			filters.AddExcludedTraitFilter("trait4", "value4a");
			filters.AddExcludedTraitFilter("trait4", "value4b");

			var arguments = Xunit3ArgumentFactory.ForFindAndRun(Version_0_3_0, discoveryOptions, executionOptions, filters: filters);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),

				arg => Assert.Equal("-class", arg),
				arg => Assert.Equal("class1", arg),
				arg => Assert.Equal("-class", arg),
				arg => Assert.Equal("class2", arg),

				arg => Assert.Equal("-class-", arg),
				arg => Assert.Equal("class3", arg),
				arg => Assert.Equal("-class-", arg),
				arg => Assert.Equal("class4", arg),

				arg => Assert.Equal("-method", arg),
				arg => Assert.Equal("method1", arg),
				arg => Assert.Equal("-method", arg),
				arg => Assert.Equal("method2", arg),

				arg => Assert.Equal("-method-", arg),
				arg => Assert.Equal("method3", arg),
				arg => Assert.Equal("-method-", arg),
				arg => Assert.Equal("method4", arg),

				arg => Assert.Equal("-namespace", arg),
				arg => Assert.Equal("namespace1", arg),
				arg => Assert.Equal("-namespace", arg),
				arg => Assert.Equal("namespace2", arg),

				arg => Assert.Equal("-namespace-", arg),
				arg => Assert.Equal("namespace3", arg),
				arg => Assert.Equal("-namespace-", arg),
				arg => Assert.Equal("namespace4", arg),

				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("trait1=value1a", arg),
				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("trait1=value1b", arg),
				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("trait2=value2a", arg),
				arg => Assert.Equal("-trait", arg),
				arg => Assert.Equal("trait2=value2b", arg),

				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("trait3=value3a", arg),
				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("trait3=value3b", arg),
				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("trait4=value4a", arg),
				arg => Assert.Equal("-trait-", arg),
				arg => Assert.Equal("trait4=value4b", arg)
			);
		}

		[Fact]
		public void AddWaitForDebugger()
		{
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();

			var arguments = Xunit3ArgumentFactory.ForFindAndRun(Version_0_3_0, discoveryOptions, executionOptions, waitForDebugger: true);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("-waitForDebugger", arg)
			);
		}
	}

	public class ForRun
	{
		[Fact]
		public void DefaultOptions()
		{
			var options = TestData.TestFrameworkExecutionOptions();

			var arguments = Xunit3ArgumentFactory.ForRun(Version_0_3_0, options, ["abc", "123"]);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("-run", arg),
				arg => Assert.Equal("abc", arg),
				arg => Assert.Equal("-run", arg),
				arg => Assert.Equal("123", arg)
			);
		}

		[Theory]
		[InlineData("", "invariant")]
		[InlineData("en-US", "en-US")]
		public void FullOptions(
			string culture,
			string expectedCulture)
		{
			// We ignore synchronousMessageReporting because it's processed locally, not remotely
			var options = TestData.TestFrameworkExecutionOptions(
				culture: culture,
				diagnosticMessages: true,
				disableParallelization: true,
				explicitOption: ExplicitOption.On,
				failSkips: true,
				failTestsWithWarnings: true,
				internalDiagnosticMessages: true,
				maxParallelThreads: 42,
				parallelAlgorithm: ParallelAlgorithm.Conservative,
				seed: 2112,
				stopOnFail: true,
				synchronousMessageReporting: true
			);

			var arguments = Xunit3ArgumentFactory.ForRun(Version_0_3_0, options, ["abc"]);

			Assert.Collection(
				arguments,
				arg => Assert.Equal(":2112", arg),
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("sync", arg),
				arg => Assert.Equal("-culture", arg),
				arg => Assert.Equal(expectedCulture, arg),
				arg => Assert.Equal("-diagnostics", arg),
				arg => Assert.Equal("-explicit", arg),
				arg => Assert.Equal("On", arg),
				arg => Assert.Equal("-failSkips", arg),
				arg => Assert.Equal("-failWarns", arg),
				arg => Assert.Equal("-internalDiagnostics", arg),
				arg => Assert.Equal("-maxThreads", arg),
				arg => Assert.Equal("42", arg),
				arg => Assert.Equal("-parallel", arg),
				arg => Assert.Equal("none", arg),
				arg => Assert.Equal("-parallelAlgorithm", arg),
				arg => Assert.Equal("Conservative", arg),
				arg => Assert.Equal("-run", arg),
				arg => Assert.Equal("abc", arg),
				arg => Assert.Equal("-stopOnFail", arg)
			);
		}

		[Fact]
		public void DoesNotSendSyncParameterToAutomatedForOlderTestProjects()
		{
			var options = TestData.TestFrameworkExecutionOptions(synchronousMessageReporting: true);

			var arguments = Xunit3ArgumentFactory.ForRun(Version_0_2_999, options, ["abc"]);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("-run", arg),
				arg => Assert.Equal("abc", arg)
			);
		}

		[Fact]
		public void AddConfigFile()
		{
			var options = TestData.TestFrameworkExecutionOptions();

			var arguments = Xunit3ArgumentFactory.ForRun(Version_0_3_0, options, ["abc"], configFileName: "/config/file/name.json");

			Assert.Collection(
				arguments,
				arg => Assert.Equal("/config/file/name.json", arg),
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("-run", arg),
				arg => Assert.Equal("abc", arg)
			);
		}

		[Fact]
		public void AddWaitForDebugger()
		{
			var options = TestData.TestFrameworkExecutionOptions();

			var arguments = Xunit3ArgumentFactory.ForRun(Version_0_3_0, options, ["abc"], waitForDebugger: true);

			Assert.Collection(
				arguments,
				arg => Assert.Equal("-automated", arg),
				arg => Assert.Equal("-run", arg),
				arg => Assert.Equal("abc", arg),
				arg => Assert.Equal("-waitForDebugger", arg)
			);
		}
	}
}
