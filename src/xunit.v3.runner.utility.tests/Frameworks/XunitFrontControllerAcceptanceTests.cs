using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class XunitFrontControllerAcceptanceTests
{
#if NETFRAMEWORK

	[CollectionDefinition(DisableParallelization = true)]
	public class CrashDetectionCollection { }

	[Collection(typeof(CrashDetectionCollection))]
	public class CrashDetection
	{
		[Fact]
		public async ValueTask CrashingTestStillReportsCompletion()
		{
			var code = /* lang=c#-test */ """
				using System;
				using System.Threading;
				using Xunit;

				public static class TestClass
				{
				    [Fact]
					public static void CrashingTest()
					{
						Environment.Exit(42);
					}
				}
				""";
			using var testAssembly = await CSharpAcceptanceTestV3Assembly.Create(code);

			var assemblyMetadata = AssemblyUtility.GetAssemblyMetadata(testAssembly.FileName);
			Assert.NotNull(assemblyMetadata);

			var projectAssembly = new XunitProjectAssembly(new XunitProject(), testAssembly.FileName, assemblyMetadata);
			projectAssembly.Project.Configuration.CrashDetectionSinkTimeout = 10;
			var frontController = XunitFrontController.Create(projectAssembly);
			Assert.NotNull(frontController);

			var messageSink = SpyMessageSink<ITestAssemblyFinished>.Create();
			var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());
			frontController.FindAndRun(messageSink, settings);

			if (!messageSink.Finished.WaitOne(30_000))
				throw new InvalidOperationException("Execution did not complete in time");

			// We don't look for an exact set of messages, because there's a race condition between the
			// message bus delivering messages back to the runner vs. the test process crashing.
			var assemblyStarting = Assert.Single(messageSink.Messages.OfType<ITestAssemblyStarting>());
			var assemblyFinished = Assert.Single(messageSink.Messages.OfType<ITestAssemblyFinished>());
			Assert.Equal(assemblyStarting.AssemblyUniqueID, assemblyFinished.AssemblyUniqueID);

			var errorMessage = Assert.Single(messageSink.Messages.OfType<IErrorMessage>());
			Assert.Equal(-1, errorMessage.ExceptionParentIndices.Single());
			Assert.Equal(typeof(TestPipelineException).SafeName(), errorMessage.ExceptionTypes.Single());
			Assert.Equal("Test process crashed with exit code 42.", errorMessage.Messages.Single());
			Assert.Null(errorMessage.StackTraces.Single());
		}
	}

#endif  // NETFRAMEWORK

	public class SourceInformation
	{
#if NETFRAMEWORK

		// v1 only supports back-filling source information via Cecil during discovery

		[Fact]
		public void Discovery_v1()
		{
			var assemblyFileName = Path.GetFullPath(Path.Combine(
				typeof(XunitFrontControllerAcceptanceTests).Assembly.Location,
				"..", "..", "..", "..", "..",
				"xunit.v1.tests", "bin",
#if DEBUG
				"Debug",
#else
				"Release",
#endif
				"net45", "xunit.v1.tests.dll"
			));

			if (!File.Exists(assemblyFileName))
				Assert.Skip($"Could not find test assembly '{assemblyFileName}'");

			var assemblyMetadata = AssemblyUtility.GetAssemblyMetadata(assemblyFileName);
			Assert.NotNull(assemblyMetadata);
			Assert.Equal(1, assemblyMetadata.XunitVersion);

			var projectAssembly = new XunitProjectAssembly(new XunitProject(), assemblyFileName, assemblyMetadata);
			projectAssembly.Configuration.IncludeSourceInformation = true;

			var frontController = XunitFrontController.Create(projectAssembly);
			Assert.NotNull(frontController);

			// Discovery

			var findSink = SpyMessageSink<IDiscoveryComplete>.Create();
			var findFilters = new XunitFilters();
			findFilters.AddIncludedClassFilter("Xunit1.TrueTests");
			var findOptions = TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration);
			var findSettings = new FrontControllerFindSettings(findOptions, findFilters);
			frontController.Find(findSink, findSettings);

			findSink.Finished.WaitOne();

			Assert.Collection(
				findSink.Messages.OfType<ITestCaseDiscovered>().OrderBy(d => d.TestCaseDisplayName),
				discovered => Assert.Equal("Xunit1.TrueTests.AssertTrue", discovered.TestCaseDisplayName),
				discovered => Assert.Equal("Xunit1.TrueTests.AssertTrueThrowsExceptionWhenFalse", discovered.TestCaseDisplayName)
			);
		}

		// v2 only supports back-filling source information via Cecil during discovery

		[Fact]
		public void Discovery_v2()
		{
			var assemblyFileName = Path.GetFullPath(Path.Combine(
				typeof(XunitFrontControllerAcceptanceTests).Assembly.Location,
				"..", "..", "..", "..", "..",
				"xunit.v2.tests", "bin",
#if DEBUG
				"Debug",
#else
				"Release",
#endif
				"net452", "xunit.v2.tests.dll"
			));

			if (!File.Exists(assemblyFileName))
				Assert.Skip($"Could not find test assembly '{assemblyFileName}'");

			var assemblyMetadata = AssemblyUtility.GetAssemblyMetadata(assemblyFileName);
			Assert.NotNull(assemblyMetadata);
			Assert.Equal(2, assemblyMetadata.XunitVersion);

			var projectAssembly = new XunitProjectAssembly(new XunitProject(), assemblyFileName, assemblyMetadata);
			projectAssembly.Configuration.IncludeSourceInformation = true;

			var frontController = XunitFrontController.Create(projectAssembly);
			Assert.NotNull(frontController);

			// Discovery

			var findSink = SpyMessageSink<IDiscoveryComplete>.Create();
			var findFilters = new XunitFilters();
			findFilters.AddIncludedClassFilter("AsyncAcceptanceTests");
			var findOptions = TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration);
			var findSettings = new FrontControllerFindSettings(findOptions, findFilters);
			frontController.Find(findSink, findSettings);

			findSink.Finished.WaitOne();

			Assert.Collection(
				findSink.Messages.OfType<ITestCaseDiscovered>().OrderBy(d => d.TestCaseDisplayName),
				discovered => Assert.Equal("AsyncAcceptanceTests.AsyncTaskTestsRunCorrectly", discovered.TestCaseDisplayName),
				discovered => Assert.Equal("AsyncAcceptanceTests.AsyncVoidTestsRunCorrectly", discovered.TestCaseDisplayName)
			);
		}

#endif  // NETFRAMEWORK

		[Fact]
		public void v3_Fact()
		{
			var assemblyFileName = typeof(DiscoveryStartingCompleteMessageSinkTests).Assembly.Location;
			var assemblyMetadata = AssemblyUtility.GetAssemblyMetadata(assemblyFileName);
			Assert.NotNull(assemblyMetadata);
			Assert.Equal(3, assemblyMetadata.XunitVersion);

			var projectAssembly = new XunitProjectAssembly(new XunitProject(), assemblyFileName, assemblyMetadata);
			projectAssembly.Configuration.IncludeSourceInformation = true;

			var frontController = XunitFrontController.Create(projectAssembly);
			Assert.NotNull(frontController);

			// Discovery

			var findSink = SpyMessageSink<IDiscoveryComplete>.Create();
			var findFilters = new XunitFilters();
			findFilters.AddIncludedClassFilter("DiscoveryStartingCompleteMessageSinkTests");
			var findOptions = TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration);
			var findSettings = new FrontControllerFindSettings(findOptions, findFilters);
			frontController.Find(findSink, findSettings);

			findSink.Finished.WaitOne();

			var serializedTestCases = new List<string>();

			Assert.Collection(
				findSink.Messages.OfType<ITestCaseDiscovered>().OrderBy(d => d.TestCaseDisplayName),
				discovered =>
				{
					serializedTestCases.Add(discovered.Serialization);

					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.NoTestCases", discovered.TestCaseDisplayName);
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.cs", Path.GetFileName(discovered.SourceFilePath));
					Assert.Equal(6, discovered.SourceLineNumber);
				},
				discovered =>
				{
					serializedTestCases.Add(discovered.Serialization);

					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.TwoTestCases", discovered.TestCaseDisplayName);
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.cs", Path.GetFileName(discovered.SourceFilePath));
					Assert.Equal(35, discovered.SourceLineNumber);
				}
			);

			// Execution

			var runSink = SpyMessageSink<ITestAssemblyFinished>.Create();
			var runOptions = TestFrameworkOptions.ForExecution(projectAssembly.Configuration);
			var runSettings = new FrontControllerRunSettings(runOptions, serializedTestCases);
			frontController.Run(runSink, runSettings);

			runSink.Finished.WaitOne();

			Assert.Collection(
				runSink.Messages.OfType<ITestCaseStarting>().OrderBy(d => d.TestCaseDisplayName),
				starting =>
				{
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.NoTestCases", starting.TestCaseDisplayName);
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.cs", Path.GetFileName(starting.SourceFilePath));
					Assert.Equal(6, starting.SourceLineNumber);
				},
				starting =>
				{
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.TwoTestCases", starting.TestCaseDisplayName);
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.cs", Path.GetFileName(starting.SourceFilePath));
					Assert.Equal(35, starting.SourceLineNumber);
				}
			);
		}

		[Fact]
		public void v3_Theory()
		{
			var assemblyFileName = typeof(DiscoveryStartingCompleteMessageSinkTests).Assembly.Location;
			var assemblyMetadata = AssemblyUtility.GetAssemblyMetadata(assemblyFileName);
			Assert.NotNull(assemblyMetadata);
			Assert.Equal(3, assemblyMetadata.XunitVersion);

			var projectAssembly = new XunitProjectAssembly(new XunitProject(), assemblyFileName, assemblyMetadata);
			projectAssembly.Configuration.IncludeSourceInformation = true;

			var frontController = XunitFrontController.Create(projectAssembly);
			Assert.NotNull(frontController);

			// Discovery

			var findSink = SpyMessageSink<IDiscoveryComplete>.Create();
			var findFilters = new XunitFilters();
			findFilters.AddIncludedClassFilter("MessageSplitMessageSinkTests");
			var findOptions = TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration);
			var findSettings = new FrontControllerFindSettings(findOptions, findFilters);
			frontController.Find(findSink, findSettings);

			findSink.Finished.WaitOne();

			var serializedTestCases = new List<string>();

			Assert.Collection(
				findSink.Messages.OfType<ITestCaseDiscovered>().OrderBy(d => d.TestCaseDisplayName),
				discovered =>
				{
					serializedTestCases.Add(discovered.Serialization);

					Assert.Equal("MessageSplitMessageSinkTests.DiagnosticMessages", discovered.TestCaseDisplayName);
					Assert.Equal("MessageSplitMessageSinkTests.cs", Path.GetFileName(discovered.SourceFilePath));
					Assert.Equal(13, discovered.SourceLineNumber);
				},
				discovered =>
				{
					serializedTestCases.Add(discovered.Serialization);

					Assert.Equal("MessageSplitMessageSinkTests.NonDiagnosticMessages", discovered.TestCaseDisplayName);
					Assert.Equal("MessageSplitMessageSinkTests.cs", Path.GetFileName(discovered.SourceFilePath));
					Assert.Equal(33, discovered.SourceLineNumber);
				}
			);

			// Execution

			var runSink = SpyMessageSink<ITestAssemblyFinished>.Create();
			var runOptions = TestFrameworkOptions.ForExecution(projectAssembly.Configuration);
			var runSettings = new FrontControllerRunSettings(runOptions, serializedTestCases);
			frontController.Run(runSink, runSettings);

			runSink.Finished.WaitOne();

			Assert.Collection(
				runSink.Messages.OfType<ITestCaseStarting>().OrderBy(d => d.TestCaseDisplayName),
				starting =>
				{
					Assert.Equal("MessageSplitMessageSinkTests.DiagnosticMessages", starting.TestCaseDisplayName);
					Assert.Equal("MessageSplitMessageSinkTests.cs", Path.GetFileName(starting.SourceFilePath));
					Assert.Equal(13, starting.SourceLineNumber);
				},
				starting =>
				{
					Assert.Equal("MessageSplitMessageSinkTests.NonDiagnosticMessages", starting.TestCaseDisplayName);
					Assert.Equal("MessageSplitMessageSinkTests.cs", Path.GetFileName(starting.SourceFilePath));
					Assert.Equal(33, starting.SourceLineNumber);
				}
			);
		}
	}
}
