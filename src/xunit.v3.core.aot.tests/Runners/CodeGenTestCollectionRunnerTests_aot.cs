using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CodeGenTestCollectionRunnerTests
{
	public class CollectionFixtures
	{
		[Fact]
		public static async ValueTask FixtureCreationFailure()
		{
			var testCollection = Mocks.CodeGenTestCollection(collectionFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingCtorFixture)] = async _ => new ThrowingCtorFixture()
			});
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(testCollection: testCollection);
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestCollectionStarting>(msg, exactMatch: false),
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
					Assert.Equal(new string[] { $"Collection fixture type '{typeof(ThrowingCtorFixture).SafeName()}' threw in its constructor", "Attempted to divide by zero." }, failed.Messages);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
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
			var testCollection = Mocks.CodeGenTestCollection(collectionFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingDisposeFixture)] = async _ => new ThrowingDisposeFixture()
			});
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(testCollection: testCollection);
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
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
				msg =>
				{
					var failure = Assert.IsType<ITestCollectionCleanupFailure>(msg, exactMatch: false);
					Assert.Equal([-1, 0], failure.ExceptionParentIndices);
					Assert.Equal(new[] { typeof(TestPipelineException).SafeName(), typeof(DivideByZeroException).SafeName() }, failure.ExceptionTypes);
					Assert.Equal(new[] { $"Collection fixture type '{typeof(ThrowingDisposeFixture).SafeName()}' threw in Dispose", "Attempted to divide by zero." }, failure.Messages);
				},
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingDisposeFixture : IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();
		}

		[Fact]
		public static async ValueTask IAsyncDisposableIsPreferredOverIDisposable()
		{
			var testCollection = Mocks.CodeGenTestCollection(collectionFixtureFactories: new Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>()
			{
				[typeof(ThrowingDisposeWithAsyncDisposeFixture)] = async _ => new ThrowingDisposeWithAsyncDisposeFixture()
			});
			var testClass = Mocks.CodeGenTestClass<MyTestClass>(testCollection: testCollection);
			var testMethod = Mocks.CodeGenTestMethod(testClass: testClass);
			var testCase = Mocks.CodeGenTestCase(testMethod: testMethod);
			var runner = new TestableCodeGenTestCollectionRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
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
				msg => Assert.IsType<ITestCollectionFinished>(msg, exactMatch: false)
			);
		}

		class ThrowingDisposeWithAsyncDisposeFixture : IAsyncDisposable, IDisposable
		{
			public void Dispose() => throw new DivideByZeroException();

			public ValueTask DisposeAsync() => default;
		}
	}

	class TestableCodeGenTestCollectionRunner(params ICodeGenTestCase[] testCases) :
		CodeGenTestCollectionRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public FixtureMappingManager AssemblyFixtureMappingManager = new("[Unit Test] Test Assembly", TestData.EmptyFixtureFactories);
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();

		public ValueTask<RunSummary> RunAsync() =>
			Run(
				testCases[0].TestCollection,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				Aggregator,
				CancellationTokenSource,
				AssemblyFixtureMappingManager
			);
	}
}
