using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CodeGenTestAssemblyRunnerTests
{
	public class Messages : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask Passing()
		{
			var messages = await RunAsync("CodeGenTestAssemblyRunnerTests+Messages+Passing");

			var assemblyID = default(string);
			Assert.Collection(
				messages,
				msg => Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false),
				msg => Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false),
				msg =>
				{
					var starting = Assert.IsType<ITestAssemblyStarting>(msg, exactMatch: false);
					assemblyID = starting.AssemblyUniqueID;
					Assert.Equal("xunit.v3.acceptance.aot.tests", starting.AssemblyName);
					Assert.Null(starting.ConfigFilePath);
					Assert.NotNull(starting.Seed);  // We don't know what the seed will be, we just know it will have one
					Assert.Equal(".NETCoreApp,Version=v9.0", starting.TargetFramework);
					Assert.Matches($"^{IntPtr.Size * 8}-bit \\({Regex.Escape(RuntimeInformation.ProcessArchitecture.ToDisplayName())}\\) {Regex.Escape(RuntimeInformation.FrameworkDescription)} \\[collection-per-class, parallel \\(\\d+ threads\\)\\]$", starting.TestEnvironment);
					Assert.Matches("^xUnit.net v3 \\d+.\\d+.\\d+", starting.TestFrameworkDisplayName);
					Assert.Empty(starting.Traits);
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
				msg =>
				{
					var finished = Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false);
					Assert.Equal(assemblyID, finished.AssemblyUniqueID);
				}
			);
		}

		[Fact]
		public static async ValueTask StaticPassing()
		{
			var messages = await RunAsync("CodeGenTestAssemblyRunnerTests+Messages+StaticPassing");

			Assert.Collection(
				messages,
				msg => Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false),
				msg => Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false),
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
			var messages = await RunAsync("CodeGenTestAssemblyRunnerTests+Messages+Failed");

			Assert.Collection(
				messages,
				msg => Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false),
				msg => Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false),
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
			var messages = await RunAsync("CodeGenTestAssemblyRunnerTests+Messages+SkippedViaAttribute");

			Assert.Collection(
				messages,
				msg => Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false),
				msg => Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false),
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
			var messages = await RunAsync("CodeGenTestAssemblyRunnerTests+Messages+SkippedViaException");

			Assert.Collection(
				messages,
				msg => Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false),
				msg => Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false),
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
			var messages = await RunAsync("CodeGenTestAssemblyRunnerTests+Messages+SkippedViaRegisteredException");

			Assert.Collection(
				messages,
				msg => Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false),
				msg => Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false),
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
			var messages = await RunAsync("CodeGenTestAssemblyRunnerTests+Messages+NotRun");

			Assert.Collection(
				messages,
				msg => Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false),
				msg => Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false),
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
	}

	public class AssemblyFixtures
	{
		[Fact]
		public static async ValueTask FixtureCreationFailure()
		{
			var testAssembly = Mocks.CodeGenTestAssembly(assemblyFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingCtorFixture)] = async _ => new ThrowingCtorFixture()
			});
			var testCollection = Mocks.CodeGenTestCollection(testAssembly: testAssembly);
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(testCollection: testCollection);
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestAssemblyRunner(testCase);

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
					Assert.Equal(new string[] { $"Assembly fixture type '{typeof(ThrowingCtorFixture).SafeName()}' threw in its constructor", "Attempted to divide by zero." }, failed.Messages);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class MyTestClass { }

		class ThrowingCtorFixture
		{
			public ThrowingCtorFixture() => throw new DivideByZeroException();
		}

		[Fact]
		public static async ValueTask FixtureDisposeFailure()
		{
			var testAssembly = Mocks.CodeGenTestAssembly(assemblyFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingDisposeFixture)] = async _ => new ThrowingDisposeFixture()
			});
			var testCollection = Mocks.CodeGenTestCollection(testAssembly: testAssembly);
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(testCollection: testCollection);
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestAssemblyRunner(testCase);

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

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testAssembly = Mocks.CodeGenTestAssembly(assemblyFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingDisposeWithAsyncDisposeFixture)] = async _ => new ThrowingDisposeWithAsyncDisposeFixture()
			});
			var testCollection = Mocks.CodeGenTestCollection(testAssembly: testAssembly);
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(testCollection: testCollection);
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestAssemblyRunner(testCase);

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
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestAssemblyFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingDisposeWithAsyncDisposeFixture : IAsyncDisposable, IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();

			public ValueTask DisposeAsync() => default;
		}
	}

	class TestableCodeGenTestAssemblyRunner(ICodeGenTestCase testCase) :
		CodeGenTestAssemblyRunner
	{
		public readonly SpyMessageSink MessageSink = SpyMessageSink.Capture();

		public ValueTask<RunSummary> RunAsync() =>
			Run(testCase.TestCollection.TestAssembly, [testCase], MessageSink, TestData.TestFrameworkExecutionOptions(), default);
	}
}
