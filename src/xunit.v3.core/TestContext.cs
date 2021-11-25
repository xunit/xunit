using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// Represents information about the current state of the test engine. It may be available at
	/// various points during the execution pipeline, so consumers must always take care to ensure
	/// that they check for <c>null</c> values from the various properties.
	/// </summary>
	public class TestContext
	{
		static readonly AsyncLocal<TestContext?> local = new();

		TestContext(
			_ITestAssembly testAssembly,
			TestEngineStatus testAssemblyStatus,
			CancellationToken cancellationToken)
		{
			TestAssembly = testAssembly;
			TestAssemblyStatus = testAssemblyStatus;
			CancellationToken = cancellationToken;
		}

		/// <summary>
		/// Gets the cancellation token that is used to indicate that the test run should be
		/// aborted. Async tests should pass this along to any async functions that support
		/// cancellation tokens, to help speed up the cancellation process.
		/// </summary>
		public CancellationToken CancellationToken { get; }

		/// <summary/>
		public static TestContext? Current => local.Value;

		/// <summary>
		/// Gets the current test, if the engine is currently in the process of running a test;
		/// will return <c>null</c> outside of the context of a test.
		/// </summary>
		public _ITest? Test { get; private set; }

		/// <summary>
		/// Gets the current test assembly.
		/// </summary>
		public _ITestAssembly TestAssembly { get; private set; }

		/// <summary>
		/// Gets the current test engine status for the test assembly.
		/// </summary>
		public TestEngineStatus TestAssemblyStatus { get; private set; }

		/// <summary>
		/// Gets the current test case, if the engine is currently in the process of running a
		/// test case; will return <c>null</c> outside of the context of a test case.
		/// </summary>
		[NotNullIfNotNull(nameof(Test))]
		public _ITestCase? TestCase { get; private set; }

		/// <summary>
		/// Gets the current test engine status for the test case. Will only be available when <see cref="TestCase"/>
		/// is not <c>null</c>.
		/// </summary>
		[NotNullIfNotNull(nameof(TestCase))]
		public TestEngineStatus? TestCaseStatus { get; private set; }

		/// <summary>
		/// Gets the current test method, if the engine is currently in the process of running
		/// a test class; will return <c>null</c> outside of the context of a test class. Note that
		/// not all test framework implementations require that tests be based on classes, so this
		/// value may be <c>null</c> even if <see cref="TestCase"/> is not <c>null</c>.
		/// </summary>
		[NotNullIfNotNull(nameof(TestMethod))]
		public _ITestClass? TestClass { get; private set; }

		/// <summary>
		/// Gets the current test engine status for the test class. Will only be available when <see cref="TestClass"/>
		/// is not <c>null</c>.
		/// </summary>
		[NotNullIfNotNull(nameof(TestClass))]
		public TestEngineStatus? TestClassStatus { get; private set; }

		/// <summary>
		/// Gets the current test collection, if the engine is currently in the process of running
		/// a test collection; will return <c>null</c> outside of the context of a test collection.
		/// </summary>
		[NotNullIfNotNull(nameof(TestClass))]
		[NotNullIfNotNull(nameof(TestCase))]
		public _ITestCollection? TestCollection { get; private set; }

		/// <summary>
		/// Gets the current test engine status for the test collection. Will only be available when
		/// <see cref="TestCollection"/> is not <c>null</c>.
		/// </summary>
		[NotNullIfNotNull(nameof(TestCollection))]
		public TestEngineStatus? TestCollectionStatus { get; private set; }

		/// <summary>
		/// Gets the output helper, which can be used to add output to the test. Will only be
		/// available when <see cref="Test"/> is not <c>null</c>. Note that the value may still
		/// be <c>null</c> when <see cref="Test"/> is not <c>null</c>, if the test framework
		/// implementation does not provide output helper support.
		/// </summary>
		public _ITestOutputHelper? TestOutputHelper { get; private set; }

		/// <summary>
		/// Gets the current test method, if the engine is currently in the process of running
		/// a test method; will return <c>null</c> outside of the context of a test method. Note that
		/// not all test framework implementations require that tests be based on methods, so this
		/// value may be <c>null</c> even if <see cref="TestCase"/> is not <c>null</c>.
		/// </summary>
		public _ITestMethod? TestMethod { get; private set; }

		/// <summary>
		/// Gets the current test engine status for the test method. Will only be available when <see cref="TestMethod"/>
		/// is not <c>null</c>.
		/// </summary>
		[NotNullIfNotNull(nameof(TestMethod))]
		public TestEngineStatus? TestMethodStatus { get; private set; }

		/// <summary>
		/// Gets the current state of the test. Will only be available when <see cref="TestStatus"/>
		/// is <see cref="TestEngineStatus.CleaningUp"/>.
		/// </summary>
		public TestState? TestState { get; private set; }

		/// <summary>
		/// Gets the current test engine status for the test. Will only be available when <see cref="Test"/>
		/// is not <c>null</c>.
		/// </summary>
		[NotNullIfNotNull(nameof(Test))]
		public TestEngineStatus? TestStatus { get; private set; }

		internal static void SetForTest(
			_ITest test,
			TestEngineStatus testStatus,
			CancellationToken cancellationToken,
			TestState? testState = null,
			_ITestOutputHelper? testOutputHelper = null)
		{
			Guard.ArgumentNotNull(test);

			local.Value = new TestContext(test.TestCase.TestCollection.TestAssembly, TestEngineStatus.Running, cancellationToken)
			{
				Test = test,
				TestStatus = testStatus,
				TestOutputHelper = testOutputHelper ?? Current?.TestOutputHelper,
				TestState = testState,

				TestCase = test.TestCase,
				TestCaseStatus = TestEngineStatus.Running,

				TestMethod = test.TestCase.TestMethod,
				TestMethodStatus = test.TestCase.TestMethod == null ? null : TestEngineStatus.Running,

				TestClass = test.TestCase.TestMethod?.TestClass,
				TestClassStatus = test.TestCase.TestMethod?.TestClass == null ? null : TestEngineStatus.Running,

				TestCollection = test.TestCase.TestCollection,
				TestCollectionStatus = TestEngineStatus.Running,
			};
		}

		internal static void SetForTestAssembly(
			_ITestAssembly testAssembly,
			TestEngineStatus testAssemblyStatus,
			CancellationToken cancellationToken)
		{
			Guard.ArgumentNotNull(testAssembly);

			local.Value = new TestContext(testAssembly, testAssemblyStatus, cancellationToken);
		}

		internal static void SetForTestCase(
			_ITestCase testCase,
			TestEngineStatus testCaseStatus,
			CancellationToken cancellationToken)
		{
			Guard.ArgumentNotNull(testCase);

			local.Value = new TestContext(testCase.TestCollection.TestAssembly, TestEngineStatus.Running, cancellationToken)
			{
				TestCase = testCase,
				TestCaseStatus = testCaseStatus,

				TestMethod = testCase.TestMethod,
				TestMethodStatus = testCase.TestMethod == null ? null : TestEngineStatus.Running,

				TestClass = testCase.TestMethod?.TestClass,
				TestClassStatus = testCase.TestMethod?.TestClass == null ? null : TestEngineStatus.Running,

				TestCollection = testCase.TestCollection,
				TestCollectionStatus = TestEngineStatus.Running,
			};
		}

		internal static void SetForTestClass(
			_ITestClass testClass,
			TestEngineStatus testClassStatus,
			CancellationToken cancellationToken)
		{
			Guard.ArgumentNotNull(testClass);

			local.Value = new TestContext(testClass.TestCollection.TestAssembly, TestEngineStatus.Running, cancellationToken)
			{
				TestClass = testClass,
				TestClassStatus = testClassStatus,

				TestCollection = testClass.TestCollection,
				TestCollectionStatus = TestEngineStatus.Running,
			};
		}

		internal static void SetForTestCollection(
			_ITestCollection testCollection,
			TestEngineStatus testCollectionStatus,
			CancellationToken cancellationToken)
		{
			Guard.ArgumentNotNull(testCollection);

			local.Value = new TestContext(testCollection.TestAssembly, TestEngineStatus.Running, cancellationToken)
			{
				TestCollection = testCollection,
				TestCollectionStatus = testCollectionStatus,
			};
		}

		internal static void SetForTestMethod(
			_ITestMethod testMethod,
			TestEngineStatus testMethodStatus,
			CancellationToken cancellationToken)
		{
			Guard.ArgumentNotNull(testMethod);

			local.Value = new TestContext(testMethod.TestClass.TestCollection.TestAssembly, TestEngineStatus.Running, cancellationToken)
			{
				TestMethod = testMethod,
				TestMethodStatus = testMethodStatus,

				TestClass = testMethod.TestClass,
				TestClassStatus = TestEngineStatus.Running,

				TestCollection = testMethod.TestClass.TestCollection,
				TestCollectionStatus = TestEngineStatus.Running,
			};
		}
	}
}
