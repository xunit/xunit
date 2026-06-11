using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class XunitTestAssemblyRunnerTests
{
	public static class Messages
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg =>
				{
					var starting = Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false);
					verifyTestAssemblyMessage(starting);
#if BUILD_X86 && NETFRAMEWORK
					Assert.Equal("xunit.v3.core.netfx.x86.tests", starting.AssemblyName);
#elif BUILD_X86 && NETCOREAPP
					Assert.Equal("xunit.v3.core.netcore.x86.tests", starting.AssemblyName);
#elif NETFRAMEWORK
					Assert.Equal("xunit.v3.core.netfx.tests", starting.AssemblyName);
#elif NETCOREAPP
					Assert.Equal("xunit.v3.core.netcore.tests", starting.AssemblyName);
#else
#error Unknown target platform
#endif
					Assert.Equal(typeof(ClassUnderTest).Assembly.Location, starting.AssemblyPath);
					Assert.Null(starting.ConfigFilePath);
					Assert.NotNull(starting.Seed);  // We don't know what the seed will be, we just know it will have one
#if NET472
					Assert.Equal(".NETFramework,Version=v4.7.2", starting.TargetFramework);
#elif NET8_0
					Assert.Equal(".NETCoreApp,Version=v8.0", starting.TargetFramework);
#else
#error Unknown target framework
#endif
					Assert.Matches($"^{IntPtr.Size * 8}-bit \\({Regex.Escape(RuntimeInformation.ProcessArchitecture.ToDisplayName())}\\) {Regex.Escape(RuntimeInformation.FrameworkDescription)} \\[collection-per-class, parallel \\(\\d+ threads\\)\\]$", starting.TestEnvironment);
					Assert.Matches("^xUnit.net v3 \\d+.\\d+.\\d+", starting.TestFrameworkDisplayName);
					// Trait comes from an assembly-level trait attribute on this test assembly
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => verifyTestAssemblyMessage(Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false))
			);

			static void verifyTestAssemblyMessage(ITestAssemblyMessage message) =>
				Assert.Equal("assembly-id", message.AssemblyUniqueID);
		}

		[Fact]
		public static async ValueTask StaticPassing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.StaticPassing));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask Failed()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal(-1, failed.ExceptionParentIndices.Single());
					Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaAttribute()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaAttribute));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...no invocation since it's skipped...
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaException));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("This isn't a good time", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaRegisteredException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaRegisteredException));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// ...invocation happens here...
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Dividing by zero is really tough", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask NotRun()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ExplicitTest));
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestNotRun>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class ClassUnderTest : IDisposable
		{
			public void Dispose() { }

			[Fact]
			public async Task Passing() => await Task.Yield();

			[Fact]
			public static void StaticPassing() { }

			[Fact]
			public void Failing() => Assert.True(false);

			[Fact(Skip = "Don't run me")]
			public void SkippedViaAttribute() { }

			[Fact]
			public void SkippedViaException() => Assert.Skip("This isn't a good time");

			[Fact(SkipExceptions = [typeof(DivideByZeroException)])]
			public void SkippedViaRegisteredException() => throw new DivideByZeroException("Dividing by zero is really tough");

			[Fact(Explicit = true)]
			public void ExplicitTest() => Assert.Fail("Should not run");
		}

		[Fact]
		public static async ValueTask AssemblyFixtureThrowingDuringInitialization()
		{
			var testAssembly = Mocks.XunitTestAssembly(assemblyFixtureTypes: [typeof(ThrowingInitFixture)]);
			var testCollection = TestData.XunitTestCollection(testAssembly);
			var testClass = TestData.XunitTestClass(typeof(ClassUnderTest), testCollection);
			var methodInfo = Guard.NotNull("Could not find method", typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.Passing)));
			var testMethod = TestData.XunitTestMethod(testClass, methodInfo);
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failed.ExceptionParentIndices);
					Assert.Equal(new string[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failed.ExceptionTypes);
					Assert.Equal(new string[] { $"Assembly fixture type '{typeof(ThrowingInitFixture).SafeName()}' threw in its constructor", "Attempted to divide by zero." }, failed.Messages);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingInitFixture
		{
			public ThrowingInitFixture() => throw new DivideByZeroException();
		}

		[Fact]
		public static async ValueTask AssemblyFixtureThrowingDuringCleanup()
		{
			var testAssembly = Mocks.XunitTestAssembly(assemblyFixtureTypes: [typeof(ThrowingDisposeFixture)]);
			var testCollection = TestData.XunitTestCollection(testAssembly);
			var testClass = TestData.XunitTestClass(typeof(ClassUnderTest), testCollection);
			var methodInfo = Guard.NotNull("Could not find method", typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.Passing)));
			var testMethod = TestData.XunitTestMethod(testClass, methodInfo);
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg =>
				{
					var failure = Assert.IsType<ITestAssemblyCleanupFailure>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failure.ExceptionParentIndices);
					Assert.Equal(new string[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failure.ExceptionTypes);
					Assert.Equal(new string[] { $"Assembly fixture type '{typeof(ThrowingDisposeFixture).SafeName()}' threw in Dispose", "Attempted to divide by zero." }, failure.Messages);
				},
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingDisposeFixture : IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();
		}
	}

	[Collection("Shared state in FixtureWithEvents")]
	public static class Fixtures
	{
		[Fact]
		public static async ValueTask FixtureEvents()
		{
			FixtureWithEvents.Events.Clear();

			var testAssembly = Mocks.XunitTestAssembly(assemblyFixtureTypes: [typeof(FixtureWithEvents)]);
			var testCollection = TestData.XunitTestCollection(testAssembly);
			var testClass = TestData.XunitTestClass<ClassUnderTest>(testCollection);
			var testMethod = TestData.XunitTestMethod(testClass, Guard.NotNull("Could not find ClassUnderTest.TestMethod", typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.TestMethod))));
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				FixtureWithEvents.Events,
				e => Assert.Equal("OnTestAssemblyStarting", e),
				e => Assert.Equal("OnTestAssemblyStartingAsync", e),

				e => Assert.Equal("OnTestCollectionStarting", e),
				e => Assert.Equal("OnTestCollectionStartingAsync", e),

				e => Assert.Equal("OnTestClassStarting", e),
				e => Assert.Equal("OnTestClassStartingAsync", e),

				e => Assert.Equal("OnTestMethodStarting", e),
				e => Assert.Equal("OnTestMethodStartingAsync", e),

				e => Assert.Equal("OnTestCaseStarting", e),
				e => Assert.Equal("OnTestCaseStartingAsync", e),

				e => Assert.Equal("OnTestStarting", e),
				e => Assert.Equal("OnTestStartingAsync", e),
				e => Assert.Equal("OnTestFinishedAsync", e),
				e => Assert.Equal("OnTestFinished", e),

				e => Assert.Equal("OnTestCaseFinishedAsync", e),
				e => Assert.Equal("OnTestCaseFinished", e),

				e => Assert.Equal("OnTestMethodFinishedAsync", e),
				e => Assert.Equal("OnTestMethodFinished", e),

				e => Assert.Equal("OnTestClassFinishedAsync", e),
				e => Assert.Equal("OnTestClassFinished", e),

				e => Assert.Equal("OnTestCollectionFinishedAsync", e),
				e => Assert.Equal("OnTestCollectionFinished", e),

				e => Assert.Equal("OnTestAssemblyFinishedAsync", e),
				e => Assert.Equal("OnTestAssemblyFinished", e)
			);
		}

		class ClassUnderTest
		{
			[Fact]
			public void TestMethod() { }
		}
	}

	public static class Run
	{
		[Fact]
		public static async ValueTask OrdererWithThrowingConstructor()
		{
			var testCollectionOrdererAttribute = Mocks.CustomAttributeData(typeof(TestCollectionOrdererAttribute), typeof(MyCtorThrowingOrderer));
			var assembly = new MockAssembly(testCollectionOrdererAttribute);
			var testAssembly = TestData.XunitTestAssembly(assembly);
			var testCollection = TestData.XunitTestCollection(testAssembly);
			var testClass = TestData.XunitTestClass(typeof(TestClassWithCtorThrowingOrderer), testCollection);
			var methodInfo = Guard.NotNull("Could not find method", typeof(TestClassWithCtorThrowingOrderer).GetMethod(nameof(TestClassWithCtorThrowingOrderer.Passing)));
			var testMethod = TestData.XunitTestMethod(testClass, methodInfo);
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestAssemblyRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageSink.Messages,
				msg => Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false),
				msg =>
				{
					var failure = Assert.IsType<ITestAssemblyCleanupFailure>(msg, exactMatch: false);
					Assert.Collection(
						failure.ExceptionTypes,
						type => Assert.Equal(typeof(TestPipelineException).SafeName(), type),
						type => Assert.Equal(typeof(DivideByZeroException).SafeName(), type)
					);
					Assert.Collection(
						failure.Messages,
						msg => Assert.Equal<object>($"Assembly-level test collection orderer '{typeof(MyCtorThrowingOrderer).FullName}' threw during construction", msg),
						msg => Assert.Equal("Attempted to divide by zero.", msg)
					);
				},
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		// Orderer attribute injected via mock
		class TestClassWithCtorThrowingOrderer
		{
			[Fact]
			public void Passing() { }
		}

		class MyCtorThrowingOrderer : ITestCollectionOrderer
		{
			public MyCtorThrowingOrderer() =>
				throw new DivideByZeroException();

			public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
				where TTestCollection : ITestCollection =>
					[];
		}
	}

	class TestableXunitTestAssemblyRunner(IXunitTestCase testCase) :
		XunitTestAssemblyRunner
	{
		public readonly SpyMessageSink MessageSink = SpyMessageSink.Capture();

		public ValueTask<RunSummary> RunAsync() =>
			Run(testCase.TestCollection.TestAssembly, [testCase], MessageSink, TestData.TestFrameworkExecutionOptions(), default);
	}

	class MockAssembly(CustomAttributeData customAttributeData) : Assembly
	{
		public override string Location => "/fake/path/to/test-assembly.dll";

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<Attribute>();
		public override IList<CustomAttributeData> GetCustomAttributesData() => [customAttributeData];
		public override AssemblyName GetName() => new("test-assembly");
	}
}
