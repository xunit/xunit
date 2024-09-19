#if NETFRAMEWORK

using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.Sdk;

public class Xunit2AcceptanceTests
{
	public class Find
	{
		[Fact]
		public async ValueTask NoTestMethods()
		{
			using var assm = await CSharpAcceptanceTestV2Assembly.Create(code: "");
			var controller = TestableXunit2.Create(assm.FileName, null, true);
			using var sink = SpyMessageSink<IDiscoveryComplete>.Create();
			var settings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());

			controller.Find(sink, settings);
			sink.Finished.WaitOne();

			Assert.IsAssignableFrom<IDiscoveryStarting>(sink.Messages.First());
			Assert.DoesNotContain(sink.Messages, msg => msg is ITestCaseDiscovered);
		}

		public class CSharp
		{
			[Fact]
			public async ValueTask FactAcceptanceTest()
			{
				var code = @"
using System;
using Xunit;

namespace Namespace1
{
	public class Class1
	{
		[Fact]
		[Trait(""Name!"", ""Value!"")]
		public void Trait() { }

		[Fact(Skip=""Skipping"")]
		public void Skipped() { }

		[Fact(DisplayName=""Custom Test Name"")]
		public void CustomName() { }
	}
}

namespace Namespace2
{
	public class OuterClass
	{
		public class Class2
		{
			[Fact]
			public void TestMethod() { }
		}
	}
}";

				using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
				var sourceInformationProvider = Substitute.For<ISourceInformationProvider, InterfaceProxy<ISourceInformationProvider>>();
				sourceInformationProvider.GetSourceInformation("Namespace1.Class1", "Trait").Returns(new SourceInformation("/path/to/source/file.cs", 2112));
				var controller = TestableXunit2.Create(assembly.FileName, shadowCopy: true, sourceInformationProvider: sourceInformationProvider);
				var sink = new TestDiscoverySink();
				var settings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions(includeSourceInformation: true));

				controller.Find(sink, settings);
				sink.Finished.WaitOne();

				Assert.Collection(
					sink.TestCases.OrderBy(tc => tc.TestCaseDisplayName),
					testCase => Assert.Equal("Custom Test Name", testCase.TestCaseDisplayName),
					testCase =>
					{
						Assert.Equal("Namespace1.Class1.Skipped", testCase.TestCaseDisplayName);
						Assert.Equal("Skipping", testCase.SkipReason);
					},
					testCase =>
					{
						Assert.Equal("Namespace1.Class1.Trait", testCase.TestCaseDisplayName);
						Assert.Equal("/path/to/source/file.cs", testCase.SourceFilePath);
						Assert.Equal(2112, testCase.SourceLineNumber);
						var key = Assert.Single(testCase.Traits.Keys);
						Assert.Equal("Name!", key);
						var value = Assert.Single(testCase.Traits[key]);
						Assert.Equal("Value!", value);
					},
					testCase =>
					{
						Assert.Equal("Namespace2.OuterClass+Class2.TestMethod", testCase.TestCaseDisplayName);
						Assert.StartsWith(":F:Namespace2.OuterClass+Class2:TestMethod:1:0:", testCase.Serialization);
						Assert.Null(testCase.SkipReason);
						Assert.Equal("Namespace2.OuterClass+Class2", testCase.TestClassName);
						Assert.Equal("Namespace2", testCase.TestClassNamespace);
						Assert.Equal("OuterClass+Class2", testCase.TestClassSimpleName);
						Assert.Equal("TestMethod", testCase.TestMethodName);
					}
				);
			}

			[Fact]
			public async ValueTask TheoryAcceptanceTest()
			{
				var code = @"
using System;
using Xunit;

public class TestClass
{
	[Theory]
	[InlineData]
	[InlineData(42)]
	[InlineData(42, ""Hello world"")]
	public void TestMethod(int x) { }
}";

				using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var sink = new TestDiscoverySink();
				var settings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());

				controller.Find(sink, settings);
				sink.Finished.WaitOne();

				Assert.Collection(
					sink.TestCases.OrderBy(tc => tc.TestCaseDisplayName),
					testCase => Assert.Contains("TestClass.TestMethod(x: ???)", testCase.TestCaseDisplayName),
					testCase => Assert.Contains("TestClass.TestMethod(x: 42)", testCase.TestCaseDisplayName),
					testCase => Assert.Contains("TestClass.TestMethod(x: 42, ???: \"Hello world\")", testCase.TestCaseDisplayName)
				);
			}
		}

		public class FSharp
		{
			[Fact]
			public async ValueTask FactAcceptanceTest()
			{
				var code = @"
module FSharpTests

open Xunit

[<Fact>]
[<Trait(""Name!"", ""Value!"")>]
let Trait() =
	Assert.True(true)

[<Fact(Skip = ""Skipping"")>]
let Skipped() =
	Assert.True(false)

[<Fact(DisplayName=""Custom Test Name"")>]
let CustomName() =
	Assert.True(true)
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var sink = new TestDiscoverySink();
				var settings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());

				controller.Find(sink, settings);

				sink.Finished.WaitOne();

				Assert.Collection(
					sink.TestCases.OrderBy(tc => tc.TestCaseDisplayName),
					testCase => Assert.Equal("Custom Test Name", testCase.TestCaseDisplayName),
					testCase =>
					{
						Assert.Equal("FSharpTests.Skipped", testCase.TestCaseDisplayName);
						Assert.Equal("Skipping", testCase.SkipReason);
					},
					testCase =>
					{
						Assert.Equal("FSharpTests.Trait", testCase.TestCaseDisplayName);
						Assert.Collection(testCase.Traits,
							kvp =>
							{
								Assert.Equal("Name!", kvp.Key);
								Assert.Equal("Value!", kvp.Value.Single());
							}
						);
					}
				);
			}

			[Fact]
			public async ValueTask TheoryAcceptanceTest()
			{
				var code = @"
module FSharpTests

open Xunit

[<Theory>]
[<InlineData>]
[<InlineData(42)>]
[<InlineData(42, ""Hello world"")>]
let TestMethod (x:int) =
	Assert.True(true)
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var sink = new TestDiscoverySink();
				var settings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());

				controller.Find(sink, settings);
				sink.Finished.WaitOne();

				Assert.Collection(
					sink.TestCases.OrderBy(tc => tc.TestCaseDisplayName),
					testCase => Assert.Equal("FSharpTests.TestMethod(x: ???)", testCase.TestCaseDisplayName),
					testCase => Assert.Equal("FSharpTests.TestMethod(x: 42)", testCase.TestCaseDisplayName),
					testCase => Assert.Equal("FSharpTests.TestMethod(x: 42, ???: \"Hello world\")", testCase.TestCaseDisplayName)
				);
			}

			[Theory]
			[InlineData("async")]
			[InlineData("task")]
			public async ValueTask SupportsAsyncReturningMethods(string blockType)
			{
				string code = @$"
module FSharpTests

open Xunit

[<Fact>]
let AsyncFailing() =
	{blockType} {{
		do! Async.Sleep(10)
		Assert.Fail(""Make sure things waited"")
	}}
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var sink = SpyMessageSink<ITestAssemblyFinished>.Create();
				var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());

				controller.FindAndRun(sink, settings);
				sink.Finished.WaitOne();

				var failures = sink.Messages.OfType<ITestFailed>();
				var failure = Assert.Single(failures);
				var starts = sink.Messages.OfType<ITestStarting>();
				var start = Assert.Single(starts, s => s.TestUniqueID == failure.TestUniqueID);
				Assert.Equal("FSharpTests.AsyncFailing", start.TestDisplayName);
				Assert.Equal("Make sure things waited", failure.Messages.Single());
			}

			[Theory]
			[InlineData("async")]
			[InlineData("task")]
			public async ValueTask SupportsTimeoutOnAsyncReturningMethods(string blockType)
			{
				string code = @$"
module FSharpTests

open System.Threading.Tasks
open Xunit

[<Fact(Timeout = 3000)>]
let TestMethod() =
	{blockType} {{
		do! Async.Sleep(10)
		Assert.Fail(""Make sure things waited"")
	}}
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var sink = SpyMessageSink<ITestAssemblyFinished>.Create();
				var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());

				controller.FindAndRun(sink, settings);
				sink.Finished.WaitOne();

				var failures = sink.Messages.OfType<ITestFailed>();
				var failure = Assert.Single(failures);
				var starts = sink.Messages.OfType<ITestStarting>();
				var start = Assert.Single(starts, s => s.TestUniqueID == failure.TestUniqueID);
				Assert.Equal("FSharpTests.TestMethod", start.TestDisplayName);
				Assert.Equal("Make sure things waited", failure.Messages.Single());
			}
		}
	}

	public class FindAndRun
	{
		[Fact]
		public async ValueTask NoTestMethods()
		{
			using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code: "");
			var controller = TestableXunit2.Create(assembly.FileName, null, true);
			var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());
			using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();

			controller.FindAndRun(sink, settings);
			sink.Finished.WaitOne();

			Assert.Empty(sink.Messages.OfType<ITestPassed>());
			Assert.Empty(sink.Messages.OfType<ITestFailed>());
			Assert.Empty(sink.Messages.OfType<ITestSkipped>());
		}

		[Fact]
		public async ValueTask DoesNotRunTestsWhenExplicitOnly()
		{
			var code = @"
using System;
using Xunit;

public class TestClass
{
	[Theory]
	[InlineData(42)]
	[InlineData(2112)]
	public void TestMethod(int x) { Assert.Equal(2112, x); }
}";

			using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
			var controller = TestableXunit2.Create(assembly.FileName, null, true);
			var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions(explicitOption: ExplicitOption.Only));
			using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();

			controller.FindAndRun(sink, settings);
			sink.Finished.WaitOne();

			Assert.Empty(sink.Messages.OfType<ITestPassed>());
			Assert.Empty(sink.Messages.OfType<ITestFailed>());
			Assert.Empty(sink.Messages.OfType<ITestSkipped>());
			var notRunTests = sink.Messages.OfType<ITestNotRun>();
			var notRunTestDisplayNames = notRunTests.Select(t => sink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == t.TestUniqueID).TestDisplayName);
			Assert.Collection(
				notRunTestDisplayNames.OrderBy(x => x),
				displayName => Assert.Equal("TestClass.TestMethod(x: 2112)", displayName),
				displayName => Assert.Equal("TestClass.TestMethod(x: 42)", displayName)
			);
		}

		public class CSharp
		{
			[Fact]
			public async ValueTask FactAcceptanceTest()
			{
				var code = @"
using System;
using Xunit;

public class TestClass
{
	[Fact]
	public void TestMethod() { Assert.True(false); }
}";

				using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());
				using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();

				controller.FindAndRun(sink, settings);
				sink.Finished.WaitOne();

				Assert.Empty(sink.Messages.OfType<ITestPassed>());
				Assert.Empty(sink.Messages.OfType<ITestSkipped>());
				var failedTest = Assert.Single(sink.Messages.OfType<ITestFailed>());
				var failedMetadata = sink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failedTest.TestUniqueID);
				Assert.Equal("TestClass.TestMethod", failedMetadata.TestDisplayName);
			}

			[Fact]
			public async ValueTask TheoryAcceptanceTest()
			{
				var code = @"
using System;
using Xunit;

public class TestClass
{
	[Theory]
	[InlineData(42)]
	[InlineData(2112)]
	public void TestMethod(int x) { Assert.Equal(2112, x); }
}";

				using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());
				using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();

				controller.FindAndRun(sink, settings);
				sink.Finished.WaitOne();

				Assert.Empty(sink.Messages.OfType<ITestSkipped>());
				var passedTest = Assert.Single(sink.Messages.OfType<ITestPassed>());
				var passedMetadata = sink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == passedTest.TestUniqueID);
				Assert.Equal("TestClass.TestMethod(x: 2112)", passedMetadata.TestDisplayName);
				var failedTest = Assert.Single(sink.Messages.OfType<ITestFailed>());
				var failedMetadata = sink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failedTest.TestUniqueID);
				Assert.Equal("TestClass.TestMethod(x: 42)", failedMetadata.TestDisplayName);
			}

			[Fact]
			public async ValueTask AsyncAcceptanceTest()
			{
				var code = @"
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass
{
	[Fact]
	public async void AsyncVoid()
	{
		await Task.Delay(10);
		Assert.True(false);
	}

	[Fact]
	public async Task AsyncTask()
	{
		await Task.Delay(10);
		Assert.True(false);
	}
}";

				using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
				var executionOptions = TestData.TestFrameworkExecutionOptions();
				var settings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions);
				using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();

				controller.FindAndRun(sink, settings);
				sink.Finished.WaitOne();

				Assert.Empty(sink.Messages.OfType<ITestPassed>());
				Assert.Empty(sink.Messages.OfType<ITestSkipped>());
				var failedTests =
					sink.Messages
						.OfType<ITestFailed>()
						.Select(f => sink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == f.TestUniqueID).TestDisplayName);
				Assert.Collection(
					failedTests.OrderBy(name => name),
					name => Assert.Equal("TestClass.AsyncTask", name),
					name => Assert.Equal("TestClass.AsyncVoid", name)
				);
			}
		}

		public class FSharp
		{
			[Fact]
			public async ValueTask FactAcceptanceTest()
			{
				var code = @"
module FSharpTests

open Xunit

[<Fact>]
let TestMethod() =
	Assert.True(false)
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());
				using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();

				controller.FindAndRun(sink, settings);
				sink.Finished.WaitOne();

				Assert.Empty(sink.Messages.OfType<ITestPassed>());
				Assert.Empty(sink.Messages.OfType<ITestSkipped>());
				var failedTest = Assert.Single(sink.Messages.OfType<ITestFailed>());
				var failedMetadata = sink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failedTest.TestUniqueID);
				Assert.Equal("FSharpTests.TestMethod", failedMetadata.TestDisplayName);
			}

			[Fact]
			public async ValueTask TheoryAcceptanceTest()
			{
				var code = @"
module FSharpTests

open Xunit

[<Theory>]
[<InlineData(42)>]
[<InlineData(2112)>]
let TestMethod(x : int) =
	Assert.Equal(2112, x)
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());
				using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();

				controller.FindAndRun(sink, settings);
				sink.Finished.WaitOne();

				Assert.Empty(sink.Messages.OfType<ITestSkipped>());
				var passedTest = Assert.Single(sink.Messages.OfType<ITestPassed>());
				var passedMetadata = sink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == passedTest.TestUniqueID);
				Assert.Equal("FSharpTests.TestMethod(x: 2112)", passedMetadata.TestDisplayName);
				var failedTest = Assert.Single(sink.Messages.OfType<ITestFailed>());
				var failedMetadata = sink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failedTest.TestUniqueID);
				Assert.Equal("FSharpTests.TestMethod(x: 42)", failedMetadata.TestDisplayName);
			}

			[Fact]
			public async ValueTask AsyncAcceptanceTest()
			{
				var code = @"
module FSharpTests

open Xunit

[<Fact>]
let AsyncFailing() =
	async {
		do! Async.Sleep(10)
		Assert.True(false)
	}
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();
				var settings = new FrontControllerFindAndRunSettings(TestData.TestFrameworkDiscoveryOptions(), TestData.TestFrameworkExecutionOptions());

				controller.FindAndRun(sink, settings);
				sink.Finished.WaitOne();

				var failures = sink.Messages.OfType<ITestFailed>();
				var failure = Assert.Single(failures);
				var failureStarting = sink.Messages.OfType<ITestStarting>().Single(s => s.TestUniqueID == failure.TestUniqueID);
				Assert.Equal("FSharpTests.AsyncFailing", failureStarting.TestDisplayName);
			}
		}
	}

	public class Run
	{
		[Fact]
		public async ValueTask NoTestMethods()
		{
			using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code: "");
			var controller = TestableXunit2.Create(assembly.FileName, null, true);
			var settings = new FrontControllerRunSettings(TestData.TestFrameworkExecutionOptions(), []);
			using var sink = SpyMessageSink<ITestAssemblyFinished>.Create();

			controller.Run(sink, settings);
			sink.Finished.WaitOne();

			Assert.Empty(sink.Messages.OfType<ITestPassed>());
			Assert.Empty(sink.Messages.OfType<ITestFailed>());
			Assert.Empty(sink.Messages.OfType<ITestSkipped>());
		}

		[Fact]
		public async ValueTask DoesNotRunTestsWhenExplicitOnly()
		{
			var code = @"
using System;
using Xunit;

public class TestClass
{
	[Theory]
	[InlineData(42)]
	[InlineData(2112)]
	public void TestMethod(int x) { Assert.Equal(2112, x); }
}";

			using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
			var controller = TestableXunit2.Create(assembly.FileName, null, true);
			var findSettings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());
			using var discoverySink = SpyMessageSink<IDiscoveryComplete>.Create();

			controller.Find(discoverySink, findSettings);
			discoverySink.Finished.WaitOne();

			using var executionSink = SpyMessageSink<ITestAssemblyFinished>.Create();
			var serializedTestCases = discoverySink.Messages.OfType<ITestCaseDiscovered>().Select(tcdm => tcdm.Serialization!).ToArray();
			Assert.All(serializedTestCases, serializedTestCase => Assert.NotNull(serializedTestCase));
			var runSettings = new FrontControllerRunSettings(TestData.TestFrameworkExecutionOptions(explicitOption: ExplicitOption.Only), serializedTestCases);

			controller.Run(executionSink, runSettings);
			executionSink.Finished.WaitOne();

			Assert.Empty(executionSink.Messages.OfType<ITestPassed>());
			Assert.Empty(executionSink.Messages.OfType<ITestFailed>());
			Assert.Empty(executionSink.Messages.OfType<ITestSkipped>());
			var notRunTests = executionSink.Messages.OfType<ITestNotRun>();
			var notRunTestDisplayNames = notRunTests.Select(t => executionSink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == t.TestUniqueID).TestDisplayName);
			Assert.Collection(
				notRunTestDisplayNames.OrderBy(x => x),
				displayName => Assert.Equal("TestClass.TestMethod(x: 2112)", displayName),
				displayName => Assert.Equal("TestClass.TestMethod(x: 42)", displayName)
			);
		}

		public class CSharp
		{
			[Fact]
			public async ValueTask FactAcceptanceTest()
			{
				var code = @"
using System;
using Xunit;

public class TestClass
{
	[Fact]
	public void TestMethod() { Assert.True(false); }
}";

				using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var findSettings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());
				using var discoverySink = SpyMessageSink<IDiscoveryComplete>.Create();

				controller.Find(discoverySink, findSettings);
				discoverySink.Finished.WaitOne();

				using var executionSink = SpyMessageSink<ITestAssemblyFinished>.Create();
				var serializedTestCases = discoverySink.Messages.OfType<ITestCaseDiscovered>().Select(tcdm => tcdm.Serialization!).ToArray();
				Assert.All(serializedTestCases, serializedTestCase => Assert.NotNull(serializedTestCase));
				var runSettings = new FrontControllerRunSettings(TestData.TestFrameworkExecutionOptions(), serializedTestCases);

				controller.Run(executionSink, runSettings);
				executionSink.Finished.WaitOne();

				Assert.Empty(executionSink.Messages.OfType<ITestPassed>());
				Assert.Empty(executionSink.Messages.OfType<ITestSkipped>());
				var failedTest = Assert.Single(executionSink.Messages.OfType<ITestFailed>());
				var failedMetadata = executionSink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failedTest.TestUniqueID);
				Assert.Equal("TestClass.TestMethod", failedMetadata.TestDisplayName);
			}

			[Fact]
			public async ValueTask TheoryAcceptanceTest()
			{
				var code = @"
using System;
using Xunit;

public class TestClass
{
	[Theory]
	[InlineData(42)]
	[InlineData(2112)]
	public void TestMethod(int x) { Assert.Equal(2112, x); }
}";

				using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var findSettings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());
				using var discoverySink = SpyMessageSink<IDiscoveryComplete>.Create();

				controller.Find(discoverySink, findSettings);
				discoverySink.Finished.WaitOne();

				using var executionSink = SpyMessageSink<ITestAssemblyFinished>.Create();
				var serializedTestCases = discoverySink.Messages.OfType<ITestCaseDiscovered>().Select(tcdm => tcdm.Serialization!).ToArray();
				Assert.All(serializedTestCases, serializedTestCase => Assert.NotNull(serializedTestCase));
				var runSettings = new FrontControllerRunSettings(TestData.TestFrameworkExecutionOptions(), serializedTestCases);

				controller.Run(executionSink, runSettings);
				executionSink.Finished.WaitOne();

				Assert.Empty(executionSink.Messages.OfType<ITestSkipped>());
				var passedTest = Assert.Single(executionSink.Messages.OfType<ITestPassed>());
				var passedMetadata = executionSink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == passedTest.TestUniqueID);
				Assert.Equal("TestClass.TestMethod(x: 2112)", passedMetadata.TestDisplayName);
				var failedTest = Assert.Single(executionSink.Messages.OfType<ITestFailed>());
				var failedMetadata = executionSink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failedTest.TestUniqueID);
				Assert.Equal("TestClass.TestMethod(x: 42)", failedMetadata.TestDisplayName);
			}
		}

		public class FSharp
		{
			[Fact]
			public async ValueTask FactAcceptanceTest()
			{
				var code = @"
module FSharpTests

open Xunit

[<Fact>]
let TestMethod() =
	Assert.True(false)
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var findSettings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());
				using var discoverySink = SpyMessageSink<IDiscoveryComplete>.Create();

				controller.Find(discoverySink, findSettings);
				discoverySink.Finished.WaitOne();

				using var executionSink = SpyMessageSink<ITestAssemblyFinished>.Create();
				var serializedTestCases = discoverySink.Messages.OfType<ITestCaseDiscovered>().Select(tcdm => tcdm.Serialization!).ToArray();
				Assert.All(serializedTestCases, serializedTestCase => Assert.NotNull(serializedTestCase));
				var runSettings = new FrontControllerRunSettings(TestData.TestFrameworkExecutionOptions(), serializedTestCases);

				controller.Run(executionSink, runSettings);
				executionSink.Finished.WaitOne();

				Assert.Empty(executionSink.Messages.OfType<ITestPassed>());
				Assert.Empty(executionSink.Messages.OfType<ITestSkipped>());
				var failedTest = Assert.Single(executionSink.Messages.OfType<ITestFailed>());
				var failedMetadata = executionSink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failedTest.TestUniqueID);
				Assert.Equal("FSharpTests.TestMethod", failedMetadata.TestDisplayName);
			}

			[Fact]
			public async ValueTask TheoryAcceptanceTest()
			{
				var code = @"
module FSharpTests

open Xunit

[<Theory>]
[<InlineData(42)>]
[<InlineData(2112)>]
let TestMethod(x : int) =
	Assert.Equal(2112, x)
";

				using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code.Replace("\t", "    "));
				var controller = TestableXunit2.Create(assembly.FileName, null, true);
				var findSettings = new FrontControllerFindSettings(TestData.TestFrameworkDiscoveryOptions());
				using var discoverySink = SpyMessageSink<IDiscoveryComplete>.Create();

				controller.Find(discoverySink, findSettings);
				discoverySink.Finished.WaitOne();

				using var executionSink = SpyMessageSink<ITestAssemblyFinished>.Create();
				var serializedTestCases = discoverySink.Messages.OfType<ITestCaseDiscovered>().Select(tcdm => tcdm.Serialization!).ToArray();
				Assert.All(serializedTestCases, serializedTestCase => Assert.NotNull(serializedTestCase));
				var runSettings = new FrontControllerRunSettings(TestData.TestFrameworkExecutionOptions(), serializedTestCases);

				controller.Run(executionSink, runSettings);
				executionSink.Finished.WaitOne();

				Assert.Empty(executionSink.Messages.OfType<ITestSkipped>());
				var passedTest = Assert.Single(executionSink.Messages.OfType<ITestPassed>());
				var passedMetadata = executionSink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == passedTest.TestUniqueID);
				Assert.Equal("FSharpTests.TestMethod(x: 2112)", passedMetadata.TestDisplayName);
				var failedTest = Assert.Single(executionSink.Messages.OfType<ITestFailed>());
				var failedMetadata = executionSink.Messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failedTest.TestUniqueID);
				Assert.Equal("FSharpTests.TestMethod(x: 42)", failedMetadata.TestDisplayName);
			}
		}
	}

	class TestableXunit2
	{
		public static IFrontController Create(
			string assemblyFileName,
			string? configFileName = null,
			bool shadowCopy = true,
			AppDomainSupport appDomainSupport = AppDomainSupport.Required,
			ISourceInformationProvider? sourceInformationProvider = null)
		{
			var project = new XunitProject();
			var metadata = new AssemblyMetadata(2, ".NETFramework,Version=v4.7.2");
			var projectAssembly = new XunitProjectAssembly(project, assemblyFileName, metadata) { ConfigFileName = configFileName };
			projectAssembly.Configuration.AppDomain = appDomainSupport;
			projectAssembly.Configuration.ShadowCopy = shadowCopy;

			return Xunit2.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, NullMessageSink.Instance);
		}
	}
}

#endif
