using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestMethodRunnerTests
{
	public class Messages
	{
		[Fact]
		public static async ValueTask Passing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Null(runner.Aggregator.ToException());
			Assert.Collection(
				runner.MessageBus.Messages,
				msg =>
				{
					var starting = Assert.IsType<ITestMethodStarting>(msg, exactMatch: false);
					verifyTestMethodMessage(starting);
					Assert.Equal("Passing", starting.MethodName);
					// Trait comes from an assembly-level trait attribute on this test assembly
					var trait = Assert.Single(starting.Traits);
					Assert.Equal("Assembly", trait.Key);
					var value = Assert.Single(trait.Value);
					Assert.Equal("Trait", value);
				},
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => verifyTestMethodMessage(Assert.IsType<ITestMethodFinished>(msg, exactMatch: false))
			);

			static void verifyTestMethodMessage(ITestMethodMessage message)
			{
				Assert.Equal("assembly-id", message.AssemblyUniqueID);
				Assert.Equal("test-class-id", message.TestClassUniqueID);
				Assert.Equal("test-collection-id", message.TestCollectionUniqueID);
				Assert.Equal("test-method-id", message.TestMethodUniqueID);
			}
		}

		[Fact]
		public static async ValueTask StaticPassing()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.StaticPassing));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestPassed>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask Failed()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.Failing));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// Test method is invoked here
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
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaAttribute()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaAttribute));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Don't run me", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaException));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("This isn't a good time", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask SkippedViaRegisteredException()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.SkippedViaRegisteredException));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassConstructionFinished>(msg, exactMatch: false),
				// Test method is invoked here
				msg => Assert.IsType<ITestClassDisposeStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestClassDisposeFinished>(msg, exactMatch: false),
				msg =>
				{
					var skipped = Assert.IsType<ITestSkipped>(msg, exactMatch: false);
					Assert.Equal("Dividing by zero is really tough", skipped.Reason);
				},
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false)
			);
		}

		[Fact]
		public static async ValueTask NotRun()
		{
			var testCase = TestData.XunitTestCase<ClassUnderTest>(nameof(ClassUnderTest.ExplicitTest));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.Collection(
				runner.MessageBus.Messages,
				msg => Assert.IsType<ITestMethodStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestStarting>(msg, exactMatch: false),
				msg => Assert.IsType<ITestNotRun>(msg, exactMatch: false),
				msg => Assert.IsType<ITestFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestCaseFinished>(msg, exactMatch: false),
				msg => Assert.IsType<ITestMethodFinished>(msg, exactMatch: false)
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
	}

	public class TestCaseOrderer
	{
		[Theory]
		[InlineData(typeof(AssemblyLevel))]
		[InlineData(typeof(CollectionLevel))]
		[InlineData(typeof(ClassLevel))]
		[InlineData(typeof(MethodLevel))]
		public static async ValueTask UsesCustomTestOrderer(Type testClassType)
		{
			var testAssembly =
				testClassType == typeof(AssemblyLevel)
					? Mocks.XunitTestAssembly(testCaseOrderer: new CustomTestCaseOrderer())
					: TestData.XunitTestAssembly(testClassType.Assembly);
			var testCollection = new CollectionPerClassTestCollectionFactory(testAssembly).Get(testClassType);
			var testClass = TestData.XunitTestClass(testClassType, testCollection);
			var testMethod = TestData.XunitTestMethod(testClass, testClassType.GetMethod("Passing") ?? throw new InvalidOperationException("Passing method not found"));
			var testCase = TestData.XunitTestCase(testMethod);
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			Assert.IsType<CustomTestCaseOrderer>(runner.RunTestCases_TestCaseOrderer);
		}

		[Fact]
		public static async ValueTask OrdersTestCases()
		{
			var testAssembly = Mocks.XunitTestAssembly(testCaseOrderer: UnorderedTestCaseOrderer.Instance);
			var testCollection = Mocks.XunitTestCollection(testAssembly: testAssembly);
			var testClass = Mocks.XunitTestClass(testCollection: testCollection, testClassName: "test-class");
			var testMethod = Mocks.XunitTestMethod(testClass: testClass, methodName: "test-method");
			var testCase1 = Mocks.XunitTestCase(testMethod: testMethod, testCaseDisplayName: "test-case-1");
			var testCase2 = Mocks.XunitTestCase(testMethod: testMethod, testCaseDisplayName: "test-case-2");
			var testCase3 = Mocks.XunitTestCase(testMethod: testMethod, testCaseDisplayName: "test-case-3");
			var runner = new TestableXunitTestMethodRunner(testCase3, testCase1, testCase2);

			await runner.RunAsync();

			Assert.IsType<UnorderedTestCaseOrderer>(runner.RunTestCases_TestCaseOrderer);
			Assert.Collection(
				runner.RunTestCase__TestCasesRun,
				tc => Assert.Equal("test-case-3", tc.TestCaseDisplayName),
				tc => Assert.Equal("test-case-1", tc.TestCaseDisplayName),
				tc => Assert.Equal("test-case-2", tc.TestCaseDisplayName)
			);
		}

		[Fact]
		public static async ValueTask SettingTestCaseOrdererWithThrowingConstructorLogsDiagnosticMessage()
		{
			var spy = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = spy;
			var testCase = TestData.XunitTestCase<TestClassWithCtorThrowingTestCaseOrder>(nameof(MethodLevel.Passing));
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			var diagnosticMessage = Assert.Single(spy.Messages.Cast<IDiagnosticMessage>());
			Assert.StartsWith($"Class-level test case orderer '{typeof(MyCtorThrowingTestCaseOrderer).SafeName()}' for test class '{typeof(TestClassWithCtorThrowingTestCaseOrder).SafeName()}' threw 'System.DivideByZeroException' during construction: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		class AssemblyLevel  // Attribute injected via mock assembly
		{
			[Fact]
			public void Passing() { }
		}

		[TestCaseOrderer(typeof(CustomTestCaseOrderer))]
		public class CollectionLevelCollection { }

		[Collection(typeof(CollectionLevelCollection))]
		class CollectionLevel()
		{
			[Fact]
			public void Passing() { }
		}

		[TestCaseOrderer(typeof(CustomTestCaseOrderer))]
		class ClassLevel()
		{
			[Fact]
			public void Passing() { }
		}

		class MethodLevel()
		{
			[Fact]
			[TestCaseOrderer(typeof(CustomTestCaseOrderer))]
			public void Passing() { }
		}

		class CustomTestCaseOrderer : ITestCaseOrderer
		{
			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, ITestCase =>
					testCases;
		}

		[TestCaseOrderer(typeof(MyCtorThrowingTestCaseOrderer))]
		class TestClassWithCtorThrowingTestCaseOrder
		{
			[Fact]
			public void Passing() { }
		}

		class MyCtorThrowingTestCaseOrderer : ITestCaseOrderer
		{
			public MyCtorThrowingTestCaseOrderer()
			{
				throw new DivideByZeroException();
			}

			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, ITestCase =>
					[];
		}
	}

	public class SelfExecution
	{
		[Fact]
		public async ValueTask SupportsSelfExecutingTestCases()
		{
			var testMethod = TestData.XunitTestMethod<ClassUnderTest>(nameof(ClassUnderTest.Passing));
			var testCase = new SelfExecutingTestCase(testMethod);
			var runner = new TestableXunitTestMethodRunner(testCase);

			await runner.RunAsync();

			var skipped = Assert.Single(runner.MessageBus.Messages.OfType<ITestSkipped>());
			Assert.Equal("This is skipped via self-execution", skipped.Reason);
		}

		class ClassUnderTest
		{
			[Fact]
			public void Passing() { }
		}

		class SelfExecutingTestCase : XunitTestCase, ISelfExecutingXunitTestCase
		{
			[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
			public SelfExecutingTestCase()
			{ }

			public SelfExecutingTestCase(IXunitTestMethod testMethod) :
				base(testMethod, "Display Name", "Unique ID", @explicit: false)
			{ }

			public ValueTask<RunSummary> Run(
				ExplicitOption explicitOption,
				IMessageBus messageBus,
				object?[] constructorArguments,
				ExceptionAggregator aggregator,
				CancellationTokenSource cancellationTokenSource) =>
					new(XunitRunnerHelper.SkipTestCases(messageBus, cancellationTokenSource, [this], "This is skipped via self-execution"));
		}
	}

	class TestableXunitTestMethodRunner(params IXunitTestCase[] testCases) :
		XunitTestMethodRunner
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly SpyMessageBus MessageBus = new();

		public ValueTask<RunSummary> RunAsync() =>
			Run(
				testCases[0].TestMethod,
				testCases,
				ExplicitOption.Off,
				MessageBus,
				DefaultTestCaseOrderer.Instance,
				Aggregator,
				CancellationTokenSource,
				[]
			);

		public List<IXunitTestCase> RunTestCase__TestCasesRun = [];

		protected override ValueTask<RunSummary> RunTestCase(
			XunitTestMethodRunnerContext ctxt,
			IXunitTestCase testCase)
		{
			RunTestCase__TestCasesRun.Add(testCase);

			return base.RunTestCase(ctxt, testCase);
		}

		public ITestCaseOrderer? RunTestCases_TestCaseOrderer;

		protected override ValueTask<RunSummary> RunTestCases(
			XunitTestMethodRunnerContext ctxt,
			Exception? exception)
		{
			RunTestCases_TestCaseOrderer = ctxt.TestCaseOrderer;

			return base.RunTestCases(ctxt, exception);
		}
	}
}
