using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestInvokerTests
{
	public class RunAsync
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var invoker = TestableTestInvoker.Create<ClassUnderTest>(nameof(ClassUnderTest.Passing));

			var result = await invoker.RunAsync();

			Assert.NotEqual(TimeSpan.Zero, result);
			Assert.Null(invoker.Aggregator.ToException());
		}

		[Fact]
		public static async ValueTask Failing()
		{
			var invoker = TestableTestInvoker.Create<ClassUnderTest>(nameof(ClassUnderTest.Failing));

			var result = await invoker.RunAsync();

			Assert.NotEqual(TimeSpan.Zero, result);
			Assert.IsType<TrueException>(invoker.Aggregator.ToException());
		}

		[Fact]
		public static async ValueTask TooManyParameterValues()
		{
			var invoker = TestableTestInvoker.Create<ClassUnderTest>(nameof(ClassUnderTest.Passing), testMethodArguments: [42]);

			await invoker.RunAsync();

			var ex = Assert.IsType<InvalidOperationException>(invoker.Aggregator.ToException());
			Assert.Equal("The test method expected 0 parameter values, but 1 parameter value was provided.", ex.Message);
		}

		[Fact]
		public static async ValueTask NotEnoughParameterValues()
		{
			var invoker = TestableTestInvoker.Create<ClassUnderTest>(nameof(ClassUnderTest.FactWithParameter));

			await invoker.RunAsync();

			var ex = Assert.IsType<InvalidOperationException>(invoker.Aggregator.ToException());
			Assert.Equal("The test method expected 1 parameter value, but 0 parameter values were provided.", ex.Message);
		}

		[Fact]
		public static async ValueTask AsyncVoidProhibited()
		{
			var invoker = TestableTestInvoker.Create<ClassUnderTest>(nameof(ClassUnderTest.AsyncVoidFact));

			await invoker.RunAsync();

			var ex = Assert.IsType<InvalidOperationException>(invoker.Aggregator.ToException());
			Assert.Equal("Tests marked as 'async void' are no longer supported. Please convert to 'async Task' or 'async ValueTask'.", ex.Message);
		}

		[Fact]
		public static async ValueTask CancelledTestDoesNotRun()
		{
			var invoker = TestableTestInvoker.Create<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			invoker.TokenSource.Cancel();

			var result = await invoker.RunAsync();

			Assert.Equal(TimeSpan.Zero, result);
			Assert.Null(invoker.Aggregator.ToException());
		}

		[Fact]
		public static async ValueTask TestWithPreExistingErrorsDoesNotRun()
		{
			var invoker = TestableTestInvoker.Create<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			invoker.Aggregator.Add(new DivideByZeroException());

			var result = await invoker.RunAsync();

			Assert.Equal(TimeSpan.Zero, result);
		}

		class ClassUnderTest
		{
			[Fact]
			public static void StaticPassing() { }

			[Fact]
			public void Passing() { }

			[Fact]
			public void Failing()
			{
				Assert.True(false);
			}

#pragma warning disable xUnit1001 // Fact methods cannot have parameters

			[Fact]
			public void FactWithParameter(int _) { }

#pragma warning restore xUnit1001 // Fact methods cannot have parameters

			[Fact]
			public async void AsyncVoidFact() => await Task.Yield();
		}

	}

	class TestableTestInvoker : TestInvoker<TestInvokerContext<_ITest>, _ITest>
	{
		readonly IMessageBus messageBus;
		readonly _ITest test;
		readonly Type testClass;
		readonly object? testClassInstance;
		readonly MethodInfo testMethod;
		readonly object?[] testMethodArguments;

		public readonly ExceptionAggregator Aggregator;
		public readonly CancellationTokenSource TokenSource;

		TestableTestInvoker(
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			_ITest test,
			Type testClass,
			object? testClassInstance,
			MethodInfo testMethod,
			object?[] testMethodArguments)
		{
			this.test = test;
			this.messageBus = messageBus;
			this.testClass = testClass;
			this.testClassInstance = testClassInstance;
			this.testMethod = testMethod;
			this.testMethodArguments = testMethodArguments;

			Aggregator = aggregator;
			TokenSource = cancellationTokenSource;
		}

		public static TestableTestInvoker Create<TClassUnderTest>(
			string methodName,
			IMessageBus? messageBus = null,
			string displayName = "MockDisplayName",
			object?[]? testMethodArguments = null)
				where TClassUnderTest : new()
		{
			var test = TestData.XunitTest<TClassUnderTest>(methodName, testDisplayName: displayName, uniqueID: "test-id");

			return new TestableTestInvoker(
				messageBus ?? new SpyMessageBus(),
				new ExceptionAggregator(),
				new CancellationTokenSource(),
				test,
				typeof(TClassUnderTest),
				new TClassUnderTest(),
				test.TestMethod.Method,
				testMethodArguments ?? []
			);
		}

		public async ValueTask<TimeSpan> RunAsync()
		{
			await using var ctxt = new TestInvokerContext<_ITest>(ExplicitOption.Off, messageBus, Aggregator, TokenSource, test, testClass, testClassInstance, testMethod, testMethodArguments);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}
	}
}
