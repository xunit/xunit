using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class CoreTestRunnerTests
{
	public class Run
	{
		[Fact]
		public async ValueTask StaticallySkipped()
		{
			var test = Mocks.CoreTest(testCase: Mocks.CoreTestCase(skipReason: "Don't run me"));
			var runner = new TestableCoreTestRunner(test);

			var result = await runner.RunAsync();

			validateSkippedResult(result);
			var skipped = Assert.Single(runner.MessageBus.Messages.OfType<ITestSkipped>());
			Assert.Equal("Don't run me", skipped.Reason);
		}

		[Fact]
		public async ValueTask DynamicallySkipped_ViaMessageToken()
		{
			var runner = new TestableCoreTestRunner() { InvokeTest__Lambda = () => Assert.Skip("Don't run me") };

			var result = await runner.RunAsync();

			validateSkippedResult(result);
			var skipped = Assert.Single(runner.MessageBus.Messages.OfType<ITestSkipped>());
			Assert.Equal("Don't run me", skipped.Reason);
		}

		// The contract for Exception.Message is non-null, but some exceptions break the contract anyway
		[Fact]
		public async ValueTask DynamicallySkipped_ViaSkipException_NullMessage()
		{
			var test = Mocks.CoreTest(testCase: Mocks.CoreTestCase(skipExceptions: [typeof(MySkipException)]));
			var runner = new TestableCoreTestRunner(test) { InvokeTest__Lambda = () => throw new MySkipException(null) };

			var result = await runner.RunAsync();

			validateSkippedResult(result);
			var skipped = Assert.Single(runner.MessageBus.Messages.OfType<ITestSkipped>());
			Assert.Equal($"Exception of type '{typeof(MySkipException).SafeName()}' was thrown", skipped.Reason);
		}

		[Fact]
		public async ValueTask DynamicallySkipped_ViaSkipException_EmptyMessage()
		{
			var test = Mocks.CoreTest(testCase: Mocks.CoreTestCase(skipExceptions: [typeof(MySkipException)]));
			var runner = new TestableCoreTestRunner(test) { InvokeTest__Lambda = () => throw new MySkipException(string.Empty) };

			var result = await runner.RunAsync();

			validateSkippedResult(result);
			var skipped = Assert.Single(runner.MessageBus.Messages.OfType<ITestSkipped>());
			Assert.Equal($"Exception of type '{typeof(MySkipException).SafeName()}' was thrown", skipped.Reason);
		}

		[Fact]
		public async ValueTask DynamicallySkipped_ViaSkipException_NonEmptyMessage()
		{
			var test = Mocks.CoreTest(testCase: Mocks.CoreTestCase(skipExceptions: [typeof(MySkipException)]));
			var runner = new TestableCoreTestRunner(test) { InvokeTest__Lambda = () => throw new MySkipException("Don't run me") };

			var result = await runner.RunAsync();

			Assert.Equal(1, result.Total);
			Assert.Equal(0, result.Failed);
			Assert.Equal(0, result.NotRun);
			Assert.Equal(1, result.Skipped);
			var skipped = Assert.Single(runner.MessageBus.Messages.OfType<ITestSkipped>());
			Assert.Equal("Don't run me", skipped.Reason);
		}

		static void validateSkippedResult(RunSummary result)
		{
			Assert.Equal(1, result.Total);
			Assert.Equal(0, result.Failed);
			Assert.Equal(0, result.NotRun);
			Assert.Equal(1, result.Skipped);
		}

		[Fact]
		public async ValueTask CallsBeforeAfterAttributes()
		{
			var attr = new SpyBeforeAfterAttribute();
			var runner = new TestableCoreTestRunner(beforeAfterTestAttributes: [attr]);

			await runner.RunAsync();

			Assert.Collection(
				attr.Operations,
				op => Assert.Equal("Before", op),
				op => Assert.Equal("After", op)
			);
		}

		class MySkipException(string? message) :
			Exception
		{
			public override string Message => message!;
		}
	}

	class TestableCoreTestRunner(
		ICoreTest? test = null,
		IReadOnlyCollection<SpyBeforeAfterAttribute>? beforeAfterTestAttributes = null) :
			CoreTestRunner<TestableCoreTestRunner.TestableContext, ICoreTest, SpyBeforeAfterAttribute>
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly IReadOnlyCollection<SpyBeforeAfterAttribute> BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [];
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();
		public readonly ICoreTest Test = test ?? Mocks.CoreTest();

		public object? CreateTestClassInstance__ReturnValue_Instance = null;
		public SynchronizationContext? CreateTestClassInstance__ReturnValue_SyncContext = null;
		public ExecutionContext? CreateTestClassInstance__ReturnValue_ExecutionContext = null;

		protected override ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(TestableContext ctxt) =>
			new((CreateTestClassInstance__ReturnValue_Instance, CreateTestClassInstance__ReturnValue_SyncContext, CreateTestClassInstance__ReturnValue_ExecutionContext));

		public object? InvokeTest_TestClassInstance;
		public Action? InvokeTest__Lambda = null;
		public TimeSpan InvokeTest__ReturnValue = TimeSpan.Zero;

		protected override ValueTask<TimeSpan> InvokeTest(
			TestableContext ctxt,
			object? testClassInstance)
		{
			InvokeTest_TestClassInstance = testClassInstance;

			InvokeTest__Lambda?.Invoke();

			return new(InvokeTest__ReturnValue);
		}

		public bool IsTestClassCreatable__ReturnValue = true;

		protected override bool IsTestClassCreatable(TestableContext ctxt) =>
			IsTestClassCreatable__ReturnValue;

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestableContext(
				Test,
				BeforeAfterTestAttributes,
				MessageBus,
				Test.TestCase.SkipReason,
				Test.TestCase.SkipReason,
				ExplicitOption.Off,
				Aggregator,
				CancellationTokenSource
			);
			await ctxt.InitializeAsync();

			return await Run(ctxt);
		}

		public class TestableContext(
			ICoreTest test,
			IReadOnlyCollection<SpyBeforeAfterAttribute> beforeAfterTestAttributes,
			IMessageBus messageBus,
			string? skipReason,
			string? runtimeSkipReason,
			ExplicitOption explicitOption,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) :
				CoreTestRunnerContext<ICoreTest, SpyBeforeAfterAttribute>(test, messageBus, skipReason, explicitOption, aggregator, cancellationTokenSource)
		{
			protected override IReadOnlyCollection<SpyBeforeAfterAttribute> BeforeAfterTestAttributes { get; set; } =
				beforeAfterTestAttributes;

			public override ValueTask<TimeSpan> InvokeTest(object? testClassInstance) =>
				new(TimeSpan.Zero);

			public override void RunAfter(SpyBeforeAfterAttribute attribute) =>
				attribute.Operations.Add("After");

			public override void RunBefore(SpyBeforeAfterAttribute attribute) =>
				attribute.Operations.Add("Before");

			protected override string? GetRuntimeSkipReason() =>
				runtimeSkipReason;
		}
	}

	// There is no contractual requirement for TBeforeAfterAttribute other than "notnull", so rather
	// than using the real attribute (which differs in AOT vs. non-AOT), we just make a simple
	// container for operations and record them in our testable context.
	class SpyBeforeAfterAttribute
	{
		public readonly List<string> Operations = [];
	}
}
