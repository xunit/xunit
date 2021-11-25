using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestInvokerTests
{
	public class Messages
	{
		[Fact]
		public static async void Messages_StaticTestMethod()
		{
			var messageBus = new SpyMessageBus();
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("StaticPassing", messageBus);

			await invoker.RunAsync();

			Assert.Empty(messageBus.Messages);
			Assert.True(invoker.BeforeTestMethodInvoked_Called);
			Assert.True(invoker.AfterTestMethodInvoked_Called);
		}

		[Fact]
		public static async void Messages_NonStaticTestMethod_NoDispose()
		{
			var messageBus = new SpyMessageBus();
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", messageBus, "Display Name");

			await invoker.RunAsync();

			Assert.Collection(
				messageBus.Messages,
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg)
			);
		}

		[Fact]
		public static async void Messages_NonStaticTestMethod_WithDispose()
		{
			var messageBus = new SpyMessageBus();
			var invoker = TestableTestInvoker.Create<DisposableClass>("Passing", messageBus, "Display Name");

			await invoker.RunAsync();

			Assert.Collection(
				messageBus.Messages,
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				msg => Assert.IsType<_TestClassDisposeStarting>(msg),
				msg => Assert.IsType<_TestClassDisposeFinished>(msg)
			);
		}

		[Fact]
		public static async void Messages_NonStaticTestMethod_WithDisposeAsync()
		{
			var messageBus = new SpyMessageBus();
			var invoker = TestableTestInvoker.Create<AsyncDisposableClass>("Passing", messageBus, "Display Name");

			await invoker.RunAsync();

			Assert.Collection(
				messageBus.Messages,
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				msg => Assert.IsType<_TestClassDisposeStarting>(msg),
				msg => Assert.IsType<_TestClassDisposeFinished>(msg)
			);
		}

		[Fact]
		public static async void Messages_NonStaticTestMethod_WithDisposeAndDisposeAsync()
		{
			var messageBus = new SpyMessageBus();
			var invoker = TestableTestInvoker.Create<BothDisposableClass>("Passing", messageBus, "Display Name");

			await invoker.RunAsync();

			Assert.Collection(
				messageBus.Messages,
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				msg => Assert.IsType<_TestClassDisposeStarting>(msg),
				msg => Assert.IsType<_TestClassDisposeFinished>(msg)
			);
		}
	}

	public class Execution
	{
		[Fact]
		public static async void Passing()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing");

			var result = await invoker.RunAsync();

			Assert.NotEqual(0m, result);
			Assert.Null(invoker.Aggregator.ToException());
		}

		[Fact]
		public static async void Failing()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Failing");

			var result = await invoker.RunAsync();

			Assert.NotEqual(0m, result);
			Assert.IsType<TrueException>(invoker.Aggregator.ToException());
		}

		[Fact]
		public static async ValueTask ClassCreationFailure_LogsExceptionIntoAggregator()
		{
			var invoker = TestableTestInvoker.Create<NonCreateableClass>("Passing");

			await invoker.RunAsync();

			var ex = invoker.Aggregator.ToException();
			Assert.IsType<MissingMethodException>(ex);
			Assert.Equal("Constructor on type 'TestInvokerTests+NonCreateableClass' not found.", ex.Message);
		}

		[Fact]
		public static async void TooManyParameterValues()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", testMethodArguments: new object[] { 42 });

			await invoker.RunAsync();

			var ex = Assert.IsType<InvalidOperationException>(invoker.Aggregator.ToException());
			Assert.Equal("The test method expected 0 parameter values, but 1 parameter value was provided.", ex.Message);
		}

		[Fact]
		public static async void NotEnoughParameterValues()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("FactWithParameter");

			await invoker.RunAsync();

			var ex = Assert.IsType<InvalidOperationException>(invoker.Aggregator.ToException());
			Assert.Equal("The test method expected 1 parameter value, but 0 parameter values were provided.", ex.Message);
		}
	}

	public class Cancellation
	{
		[Fact]
		public static async void CancellationRequested_DoesNotInvokeTestMethod()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Failing");
			invoker.TokenSource.Cancel();

			var result = await invoker.RunAsync();

			Assert.Equal(0m, result);
			Assert.Null(invoker.Aggregator.ToException());
			Assert.False(invoker.BeforeTestMethodInvoked_Called);
			Assert.False(invoker.AfterTestMethodInvoked_Called);
		}

		[Fact]
		public static async void CancellationRequested_DisposeCalledIfClassConstructed()
		{
			var classConstructed = false;

			bool cancelThunk(_MessageSinkMessage msg)
			{
				if (msg is _TestClassConstructionFinished)
					classConstructed = true;
				return !classConstructed;
			}

			var messageBus = new SpyMessageBus(cancelThunk);
			var invoker = TestableTestInvoker.Create<DisposableClass>("Passing", messageBus, "Display Name");

			await invoker.RunAsync();

			Assert.Collection(
				messageBus.Messages,
				msg => Assert.IsType<_TestClassConstructionStarting>(msg),
				msg => Assert.IsType<_TestClassConstructionFinished>(msg),
				msg => Assert.IsType<_TestClassDisposeStarting>(msg),
				msg => Assert.IsType<_TestClassDisposeFinished>(msg)
			);
		}
	}

	public class TestContextVisibility
	{
		[Fact]
		public async void Before_SeesInitializing()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", displayName: "Test display name");

			await invoker.RunAsync();

			var context = invoker.BeforeTestMethodInvoked_Context;
			Assert.NotNull(context);
			Assert.Equal(TestEngineStatus.Running, context.TestAssemblyStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCollectionStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestClassStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestMethodStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCaseStatus);
			Assert.Equal(TestEngineStatus.Initializing, context.TestStatus);
			var test = context.Test;
			Assert.NotNull(test);
			Assert.Equal("Test display name", test.DisplayName);
			Assert.Null(context.TestState);
		}

		[Fact]
		public async void Executing_SeesRunning()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", displayName: "Test display name");

			await invoker.RunAsync();

			var context = invoker.InvokeTestMethodAsync_Context;
			Assert.NotNull(context);
			Assert.Equal(TestEngineStatus.Running, context.TestAssemblyStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCollectionStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestClassStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestMethodStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCaseStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestStatus);
			var test = context.Test;
			Assert.NotNull(test);
			Assert.Equal("Test display name", test.DisplayName);
			Assert.Null(context.TestState);
		}

		[Fact]
		public async void After_Passing_SeesCleaningUp()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Passing", displayName: "Test display name");

			await invoker.RunAsync();

			var context = invoker.AfterTestMethodInvoked_Context;
			Assert.NotNull(context);
			Assert.Equal(TestEngineStatus.Running, context.TestAssemblyStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCollectionStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestClassStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestMethodStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCaseStatus);
			Assert.Equal(TestEngineStatus.CleaningUp, context.TestStatus);
			var test = context.Test;
			Assert.NotNull(test);
			Assert.Equal("Test display name", test.DisplayName);
			var testState = context.TestState;
			Assert.NotNull(testState);
			Assert.Null(testState.ExceptionMessages);
			Assert.Null(testState.ExceptionParentIndices);
			Assert.Null(testState.ExceptionStackTraces);
			Assert.Null(testState.ExceptionTypes);
			Assert.True(testState.ExecutionTime > 0m);
			Assert.Null(testState.FailureCause);
			Assert.Equal(TestResult.Passed, testState.Result);
		}

		[Fact]
		public async void After_Failing_SeesCleaningUp()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("Failing", displayName: "Test display name");

			await invoker.RunAsync();

			var context = invoker.AfterTestMethodInvoked_Context;
			Assert.NotNull(context);
			Assert.Equal(TestEngineStatus.Running, context.TestAssemblyStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCollectionStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestClassStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestMethodStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCaseStatus);
			Assert.Equal(TestEngineStatus.CleaningUp, context.TestStatus);
			var test = context.Test;
			Assert.NotNull(test);
			Assert.Equal("Test display name", test.DisplayName);
			var testState = context.TestState;
			Assert.NotNull(testState);
			Assert.Equal(
				"Assert.True() Failure" + Environment.NewLine +
				"Expected: True" + Environment.NewLine +
				"Actual:   False",
				Assert.Single(testState.ExceptionMessages!)
			);
			Assert.Equal(-1, Assert.Single(testState.ExceptionParentIndices!));
			Assert.Single(testState.ExceptionStackTraces!);
			Assert.Equal(typeof(TrueException).FullName, Assert.Single(testState.ExceptionTypes!));
			Assert.True(testState.ExecutionTime > 0m);
			Assert.Equal(FailureCause.Assertion, testState.FailureCause);
			Assert.Equal(TestResult.Failed, testState.Result);
		}

		[Fact]
		public async void After_Exception_SeesCleaningUp()
		{
			var invoker = TestableTestInvoker.Create<NonDisposableClass>("FactWithParameter", displayName: "Test display name");

			await invoker.RunAsync();

			var context = invoker.AfterTestMethodInvoked_Context;
			Assert.NotNull(context);
			Assert.Equal(TestEngineStatus.Running, context.TestAssemblyStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCollectionStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestClassStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestMethodStatus);
			Assert.Equal(TestEngineStatus.Running, context.TestCaseStatus);
			Assert.Equal(TestEngineStatus.CleaningUp, context.TestStatus);
			var test = context.Test;
			Assert.NotNull(test);
			Assert.Equal("Test display name", test.DisplayName);
			var testState = context.TestState;
			Assert.NotNull(testState);
			Assert.Equal("The test method expected 1 parameter value, but 0 parameter values were provided.", Assert.Single(testState.ExceptionMessages!));
			Assert.Equal(-1, Assert.Single(testState.ExceptionParentIndices!));
			Assert.Single(testState.ExceptionStackTraces!);
			Assert.Equal(typeof(InvalidOperationException).FullName, Assert.Single(testState.ExceptionTypes!));
			Assert.True(testState.ExecutionTime > 0m);
			Assert.Equal(FailureCause.Exception, testState.FailureCause);
			Assert.Equal(TestResult.Failed, testState.Result);
		}
	}

	class NonDisposableClass
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

		[Fact]
		public void FactWithParameter(int x) { }
	}

	class DisposableClass : IDisposable
	{
		public void Dispose() { }

		[Fact]
		public void Passing() { }
	}

	class AsyncDisposableClass : IAsyncDisposable
	{
		public ValueTask DisposeAsync() => default;

		[Fact]
		public void Passing() { }
	}

	class BothDisposableClass : IAsyncDisposable, IDisposable
	{
		public void Dispose() { }

		public ValueTask DisposeAsync() => default;

		[Fact]
		public void Passing() { }
	}

	class NonCreateableClass
	{
		// Unmatched constructor argument, class isn't createable
		public NonCreateableClass(int _)
		{ }

		[Fact]
		public void Passing() =>
			Assert.Fail("This test should never be run");
	}

	class TestableTestInvoker : TestInvoker<TestInvokerContext>
	{
		readonly IMessageBus messageBus;
		readonly _ITest test;
		readonly Type testClass;
		readonly MethodInfo testMethod;
		readonly object?[]? testMethodArguments;

		public readonly ExceptionAggregator Aggregator;
		public bool AfterTestMethodInvoked_Called;
		public TestContext? AfterTestMethodInvoked_Context;
		public bool BeforeTestMethodInvoked_Called;
		public TestContext? BeforeTestMethodInvoked_Context;
		public TestContext? InvokeTestMethodAsync_Context;
		public readonly CancellationTokenSource TokenSource;

		TestableTestInvoker(
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			this.test = test;
			this.messageBus = messageBus;
			this.testClass = testClass;
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
		{
			var testCase = Mocks.TestCase<TClassUnderTest>(methodName);
			var test = Mocks.Test(testCase, displayName, "test-id");

			return new TestableTestInvoker(
				test,
				messageBus ?? new SpyMessageBus(),
				typeof(TClassUnderTest),
				typeof(TClassUnderTest).GetMethod(methodName) ?? throw new ArgumentException($"Could not find method '{methodName}' in '{typeof(TClassUnderTest).FullName}'"),
				testMethodArguments,
				new ExceptionAggregator(),
				new CancellationTokenSource()
			);
		}

		protected override ValueTask AfterTestMethodInvokedAsync(TestInvokerContext ctxt)
		{
			AfterTestMethodInvoked_Called = true;
			AfterTestMethodInvoked_Context = TestContext.Current;
			return default;
		}

		protected override ValueTask BeforeTestMethodInvokedAsync(TestInvokerContext ctxt)
		{
			BeforeTestMethodInvoked_Called = true;
			BeforeTestMethodInvoked_Context = TestContext.Current;
			return default;
		}

		protected override ValueTask<decimal> InvokeTestMethodAsync(
			TestInvokerContext ctxt,
			object? testClassInstance)
		{
			InvokeTestMethodAsync_Context = TestContext.Current;

			return base.InvokeTestMethodAsync(ctxt, testClassInstance);
		}

		public ValueTask<decimal> RunAsync() =>
			RunAsync(new(test, testClass, Array.Empty<object>(), testMethod, testMethodArguments, messageBus, Aggregator, TokenSource));
	}
}
