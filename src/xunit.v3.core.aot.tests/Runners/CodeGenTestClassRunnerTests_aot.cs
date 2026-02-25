using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CodeGenTestClassRunnerTests
{
	public class ClassFixtures
	{
		[Fact]
		public static async ValueTask FixtureCreationFailure()
		{
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(classFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingCtorFixture)] = async _ => new ThrowingCtorFixture()
			});
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestClassStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// ...no invocation because of the startup failure...
				msg =>
				{
					var failed = Assert.IsType<ITestFailed>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failed.ExceptionParentIndices);
					Assert.Equal(new string[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failed.ExceptionTypes);
					Assert.Equal(new string[] { $"Class fixture type '{typeof(ThrowingCtorFixture).SafeName()}' threw in its constructor", "Attempted to divide by zero." }, failed.Messages);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
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
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(classFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingDisposeFixture)] = async _ => new ThrowingDisposeFixture()
			});
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
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
				msg =>
				{
					var failure = Assert.IsType<ITestClassCleanupFailure>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failure.ExceptionParentIndices);
					Assert.Equal(new[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failure.ExceptionTypes);
					Assert.Equal(new[] { $"Class fixture type '{typeof(ThrowingDisposeFixture).SafeName()}' threw in Dispose", "Attempted to divide by zero." }, failure.Messages);
				},
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingDisposeFixture : IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();
		}

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(classFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingDisposeWithAsyncDisposeFixture)] = async _ => new ThrowingDisposeWithAsyncDisposeFixture()
			});
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestClassRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
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
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingDisposeWithAsyncDisposeFixture : IAsyncDisposable, IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();

			public ValueTask DisposeAsync() => default;
		}
	}

	class TestableCodeGenTestClassRunner(params ICodeGenTestCase[] testCases) :
		CodeGenTestClassRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public FixtureMappingManager CollectionFixtureMappingManager = new("[Unit Test] Test Collection", TestData.EmptyFixtureFactories);
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();

		public ValueTask<RunSummary> RunAsync() =>
			Run(
				testCases[0].TestClass,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				CancellationTokenSource,
				CollectionFixtureMappingManager
			);
	}
}
