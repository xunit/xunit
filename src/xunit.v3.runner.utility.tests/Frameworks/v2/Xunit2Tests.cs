#if NETFRAMEWORK

using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.Common;
using Xunit.v3;

public class Xunit2Tests
{
	public class EnumerateTests
	{
		[Fact]
		public async void NoTestMethods_ByAssembly()
		{
			using var assm = await CSharpAcceptanceTestV2Assembly.Create(code: "");
			var controller = new TestableXunit2(assm.FileName, null, true);
			using var sink = SpyMessageSink<_DiscoveryComplete>.Create();

			controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: _TestFrameworkOptions.ForDiscovery());

			sink.Finished.WaitOne();

			Assert.IsType<_DiscoveryStarting>(sink.Messages.First());
			Assert.False(sink.Messages.Any(msg => msg is ITestCaseDiscoveryMessage));
		}

		[Fact]
		public async void NoTestMethods_ByType()
		{
			using var assm = await CSharpAcceptanceTestV2Assembly.Create(code: "");
			var controller = new TestableXunit2(assm.FileName, null, true);
			using var sink = SpyMessageSink<_DiscoveryComplete>.Create();

			controller.Find(typeName: "foo", includeSourceInformation: false, messageSink: sink, discoveryOptions: _TestFrameworkOptions.ForDiscovery());

			sink.Finished.WaitOne();

			Assert.IsType<_DiscoveryStarting>(sink.Messages.First());
			Assert.False(sink.Messages.Any(msg => msg is ITestCaseDiscoveryMessage));
		}

		[Fact]
		public async void SingleTestMethod()
		{
			var code = @"
using Xunit;

public class Foo
{
	[Fact]
	public void Bar() { }
}";

			using var assm = await CSharpAcceptanceTestV2Assembly.Create(code);
			var controller = new TestableXunit2(assm.FileName, null, true);
			using var sink = SpyMessageSink<_DiscoveryComplete>.Create();

			controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: _TestFrameworkOptions.ForDiscovery());

			sink.Finished.WaitOne();

			var testCase = sink.Messages.OfType<ITestCaseDiscoveryMessage>().Single().TestCase;
			Assert.Equal("Foo.Bar", testCase.DisplayName);
		}
	}

	public class CSharp
	{
		[Fact]
		public async void FactAcceptanceTest()
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
			var controller = new TestableXunit2(assembly.FileName, null, true);
			using var sink = SpyMessageSink<_DiscoveryComplete>.Create();

			controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: _TestFrameworkOptions.ForDiscovery());

			sink.Finished.WaitOne();
			var testCases = sink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(tcdm => tcdm.TestCase).ToArray();

			Assert.Equal(4, testCases.Length);

			var traitTest = Assert.Single(testCases, tc => tc.DisplayName == "Namespace1.Class1.Trait");
			var key = Assert.Single(traitTest.Traits.Keys);
			Assert.Equal("Name!", key);
			var value = Assert.Single(traitTest.Traits[key]);
			Assert.Equal("Value!", value);

			var skipped = Assert.Single(testCases, tc => tc.DisplayName == "Namespace1.Class1.Skipped");
			Assert.Equal("Skipping", skipped.SkipReason);

			Assert.Single(testCases, tc => tc.DisplayName == "Custom Test Name");
			Assert.Single(testCases, tc => tc.DisplayName == "Namespace2.OuterClass+Class2.TestMethod");
		}

		[Fact]
		public async void TheoryWithInlineData()
		{
			var code = @"
using System;
using Xunit;

public class TestClass
{
	[Theory]
	[InlineData]
	[InlineData(42)]
	[InlineData(42, 21.12)]
	public void TestMethod(int x) { }
}";

			using var assembly = await CSharpAcceptanceTestV2Assembly.Create(code);
			var controller = new TestableXunit2(assembly.FileName, null, true);
			using var sink = SpyMessageSink<_DiscoveryComplete>.Create();

			controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: _TestFrameworkOptions.ForDiscovery());

			sink.Finished.WaitOne();
			var testCaseNames = sink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(tcdm => tcdm.TestCase.DisplayName).ToArray();

			Assert.Equal(3, testCaseNames.Length);

			Assert.Contains("TestClass.TestMethod(x: ???)", testCaseNames);
			Assert.Contains("TestClass.TestMethod(x: 42)", testCaseNames);
			Assert.Contains($"TestClass.TestMethod(x: 42, ???: {21.12})", testCaseNames);
		}
	}

	public class FSharp
	{
		[Fact]
		public async void FactAcceptanceTest()
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

			using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code);
			var controller = new TestableXunit2(assembly.FileName, null, true);
			var sink = new TestDiscoverySink();

			controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: _TestFrameworkOptions.ForDiscovery());
			sink.Finished.WaitOne();

			Assert.Collection(
				sink.TestCases.OrderBy(tc => tc.DisplayName),
				testCase => Assert.Equal("Custom Test Name", testCase.DisplayName),
				testCase =>
				{
					Assert.Equal("FSharpTests.Skipped", testCase.DisplayName);
					Assert.Equal("Skipping", testCase.SkipReason);
				},
				testCase =>
				{
					Assert.Equal("FSharpTests.Trait", testCase.DisplayName);
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
		public async void TheoryWithInlineData()
		{
			var code = @"
module FSharpTests

open Xunit

[<Theory>]
[<InlineData>]
[<InlineData(42)>]
[<InlineData(42, 21.12)>]
let TestMethod (x:int) =
    Assert.True(true)
";

			using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code);
			var controller = new TestableXunit2(assembly.FileName, null, true);
			var sink = new TestDiscoverySink();

			controller.Find(includeSourceInformation: false, messageSink: sink, discoveryOptions: _TestFrameworkOptions.ForDiscovery());
			sink.Finished.WaitOne();

			Assert.Collection(
				sink.TestCases.OrderBy(tc => tc.DisplayName),
				testCase => Assert.Equal("FSharpTests.TestMethod(x: ???)", testCase.DisplayName),
				testCase => Assert.Equal("FSharpTests.TestMethod(x: 42)", testCase.DisplayName),
				testCase => Assert.Equal("FSharpTests.TestMethod(x: 42, ???: 21.12)", testCase.DisplayName)
			);
		}

		[Fact]
		public async void SupportsAsyncReturningMethods()
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

			using var assembly = await FSharpAcceptanceTestV2Assembly.Create(code);
			var controller = new TestableXunit2(assembly.FileName, null, true);
			using var sink = SpyMessageSink<_TestAssemblyFinished>.Create();

			controller.RunAll(sink, discoveryOptions: _TestFrameworkOptions.ForDiscovery(), executionOptions: _TestFrameworkOptions.ForExecution());
			sink.Finished.WaitOne();

			var failures = sink.Messages.OfType<ITestFailed>();
			var failure = Assert.Single(failures);
			Assert.Equal("FSharpTests.AsyncFailing", failure.TestCase.DisplayName);
		}
	}

	class TestableXunit2 : Xunit2
	{
		public TestableXunit2(
			string assemblyFileName,
			string? configFileName = null,
			bool shadowCopy = true,
			AppDomainSupport appDomainSupport = AppDomainSupport.Required)
				: base(new _NullMessageSink(), appDomainSupport, _NullSourceInformationProvider.Instance, assemblyFileName, configFileName, shadowCopy)
		{ }
	}
}

#endif
