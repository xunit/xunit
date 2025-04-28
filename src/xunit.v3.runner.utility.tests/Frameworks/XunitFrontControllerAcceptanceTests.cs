using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class XunitFrontControllerAcceptanceTests
{
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
				discovered =>
				{
					Assert.Equal("Xunit1.TrueTests.AssertTrue", discovered.TestCaseDisplayName);
					Assert.Equal("TrueTests.cs", Path.GetFileName(discovered.SourceFilePath));
#if DEBUG
					Assert.Equal(10, discovered.SourceLineNumber);
#else
					// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
					Assert.InRange(discovered.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif
				},
				discovered =>
				{
					Assert.Equal("Xunit1.TrueTests.AssertTrueThrowsExceptionWhenFalse", discovered.TestCaseDisplayName);
					Assert.Equal("TrueTests.cs", Path.GetFileName(discovered.SourceFilePath));
#if DEBUG
					Assert.Equal(16, discovered.SourceLineNumber);
#else
					// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
					Assert.InRange(discovered.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif
				}
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
				discovered =>
				{
					Assert.Equal("AsyncAcceptanceTests.AsyncTaskTestsRunCorrectly", discovered.TestCaseDisplayName);
					Assert.Equal("AsyncAcceptanceTests.cs", Path.GetFileName(discovered.SourceFilePath));
#if DEBUG
					Assert.Equal(10, discovered.SourceLineNumber);
#else
					// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
					Assert.InRange(discovered.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif
				},
				discovered =>
				{
					Assert.Equal("AsyncAcceptanceTests.AsyncVoidTestsRunCorrectly", discovered.TestCaseDisplayName);
					Assert.Equal("AsyncAcceptanceTests.cs", Path.GetFileName(discovered.SourceFilePath));
#if DEBUG
					Assert.Equal(20, discovered.SourceLineNumber);
#else
					// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
					Assert.InRange(discovered.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif
				}
			);
		}

#endif  // NETFRAMEWORK

		[Fact]
		public void v3()
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
#if DEBUG
					Assert.Equal(9, discovered.SourceLineNumber);
#else
					// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
					Assert.InRange(discovered.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif
				},
				discovered =>
				{
					serializedTestCases.Add(discovered.Serialization);

					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.TwoTestCases", discovered.TestCaseDisplayName);
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.cs", Path.GetFileName(discovered.SourceFilePath));
#if DEBUG
					Assert.Equal(38, discovered.SourceLineNumber);
#else
					// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
					Assert.InRange(discovered.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif
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
#if DEBUG
					Assert.Equal(9, starting.SourceLineNumber);
#else
					// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
					Assert.InRange(starting.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif
				},
				starting =>
				{
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.TwoTestCases", starting.TestCaseDisplayName);
					Assert.Equal("DiscoveryStartingCompleteMessageSinkTests.cs", Path.GetFileName(starting.SourceFilePath));
#if DEBUG
					Assert.Equal(38, starting.SourceLineNumber);
#else
					// We test for range here, because release PDBs can be slightly unpredictable, especially on Mono
					Assert.InRange(starting.SourceLineNumber ?? -1, 1, 0xFEEFED);
#endif
				}
			);
		}
	}
}
