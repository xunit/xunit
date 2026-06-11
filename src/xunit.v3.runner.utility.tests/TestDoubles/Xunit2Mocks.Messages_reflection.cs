#pragma warning disable xUnit3000 // These derive from the "wrong" version (v3 instead of v2) of LLMBRO, which is acceptable

using Xunit.Abstractions;
using ExceptionUtility = Xunit.Sdk.ExceptionUtility;
using LongLivedMarshalByRefObject = Xunit.Sdk.LongLivedMarshalByRefObject;

namespace Xunit.Runner.v2;

// This file manufactures mocks of test runner messages
partial class Xunit2Mocks
{
	public static IAfterTestFinished AfterTestFinished(
		ITest test,
		string attributeName) =>
			new MockAfterTestFinished(test)
			{
				AttributeName = attributeName,
			};

	class MockAfterTestFinished(ITest test) :
		_TestBase, IAfterTestFinished
	{
		public required string AttributeName { get; set; }

		public override ITest Test => test;
	}

	public static IAfterTestStarting AfterTestStarting(
		ITest test,
		string attributeName) =>
			new MockAfterTestStarting(test)
			{
				AttributeName = attributeName,
			};

	class MockAfterTestStarting(ITest test) :
		_TestBase, IAfterTestStarting
	{
		public required string AttributeName { get; set; }

		public override ITest Test => test;
	}

	public static IBeforeTestFinished BeforeTestFinished(
		ITest test,
		string attributeName) =>
			new MockBeforeTestFinished(test)
			{
				AttributeName = attributeName,
			};

	class MockBeforeTestFinished(ITest test) :
		_TestBase, IBeforeTestFinished
	{
		public required string AttributeName { get; set; }

		public override ITest Test => test;
	}

	public static IBeforeTestStarting BeforeTestStarting(
		ITest test,
		string attributeName) =>
			new MockBeforeTestStarting(test)
			{
				AttributeName = attributeName,
			};

	class MockBeforeTestStarting(ITest test) :
		_TestBase, IBeforeTestStarting
	{
		public required string AttributeName { get; set; }

		public override ITest Test => test;
	}

	public static IDiagnosticMessage DiagnosticMessage(string message) =>
		new MockDiagnosticMessage
		{
			Message = message,
		};

	class MockDiagnosticMessage : LongLivedMarshalByRefObject, IDiagnosticMessage
	{
		public required string Message { get; set; }
	}

	public static IErrorMessage ErrorMessage(
		Exception ex,
		ITestCase[]? testCases = null)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		return new MockErrorMessage
		{
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			Messages = messages,
			StackTraces = stackTraces,
			TestCases = testCases ?? [],
		};
	}

	class MockErrorMessage : LongLivedMarshalByRefObject, IErrorMessage
	{
		public required int[] ExceptionParentIndices { get; set; }
		public required string?[] ExceptionTypes { get; set; }
		public required string[] Messages { get; set; }
		public required string?[] StackTraces { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }
	}

	public static ITestAssemblyCleanupFailure TestAssemblyCleanupFailure(
		ITestAssembly testAssembly,
		Exception ex,
		ITestCase[]? testCases = null)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		return new MockTestAssemblyCleanupFailure(testAssembly)
		{
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			Messages = messages,
			StackTraces = stackTraces,
			TestCases = testCases ?? [],
		};
	}

	class MockTestAssemblyCleanupFailure(ITestAssembly testAssembly) :
		_TestAssemblyBase, ITestAssemblyCleanupFailure
	{
		public required int[] ExceptionParentIndices { get; set; }
		public required string?[] ExceptionTypes { get; set; }
		public required string[] Messages { get; set; }
		public required string?[] StackTraces { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }

		public override ITestAssembly TestAssembly => testAssembly;
	}

	public static ITestAssemblyFinished TestAssemblyFinished(
		decimal executionTime = 0m,
		ITestAssembly? testAssembly = null,
		ITestCase[]? testCases = null,
		int testsRun = 0,
		int testsFailed = 0,
		int testsSkipped = 0) =>
			new MockTestAssemblyFinished(testAssembly ?? TestAssembly())
			{
				ExecutionTime = executionTime,
				TestCases = testCases ?? [],
				TestsFailed = testsFailed,
				TestsRun = testsRun,
				TestsSkipped = testsSkipped,
			};

	class MockTestAssemblyFinished(ITestAssembly testAssembly) :
		_TestAssemblyBase, ITestAssemblyFinished
	{
		public required decimal ExecutionTime { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }
		public required int TestsFailed { get; set; }
		public required int TestsRun { get; set; }
		public required int TestsSkipped { get; set; }

		public override ITestAssembly TestAssembly => testAssembly;
	}

	public static ITestAssemblyStarting TestAssemblyStarting(
		DateTime? startTime = null,
		ITestAssembly? testAssembly = null,
		ITestCase[]? testCases = null,
		string testEnvironment = TestData.DefaultTestEnvironment,
		string testFrameworkDisplayName = TestData.DefaultTestFrameworkDisplayName) =>
			new MockTestAssemblyStarting(testAssembly ?? TestAssembly())
			{
				StartTime = startTime ?? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),
				TestCases = testCases ?? [],
				TestEnvironment = testEnvironment,
				TestFrameworkDisplayName = testFrameworkDisplayName,
			};

	class MockTestAssemblyStarting(ITestAssembly testAssembly) :
		_TestAssemblyBase, ITestAssemblyStarting
	{
		public required DateTime StartTime { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }
		public required string TestEnvironment { get; set; }
		public required string TestFrameworkDisplayName { get; set; }

		public override ITestAssembly TestAssembly => testAssembly;
	}

	public static ITestCaseCleanupFailure TestCaseCleanupFailure(
		ITestCase testCase,
		Exception ex)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		return new MockTestCaseCleanupFailure(testCase)
		{
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			Messages = messages,
			StackTraces = stackTraces,
		};
	}

	class MockTestCaseCleanupFailure(ITestCase testCase) :
		_TestCaseBase, ITestCaseCleanupFailure
	{
		public required int[] ExceptionParentIndices { get; set; }
		public required string?[] ExceptionTypes { get; set; }
		public required string[] Messages { get; set; }
		public required string?[] StackTraces { get; set; }

		public override ITestCase TestCase => testCase;
	}

	public static ITestCaseDiscoveryMessage TestCaseDiscoveryMessage(ITestCase testCase) =>
		new MockTestCaseDiscoveryMessage(testCase);

	class MockTestCaseDiscoveryMessage(ITestCase testCase) :
		_TestCaseBase, ITestCaseDiscoveryMessage
	{
		public override ITestCase TestCase => testCase;
	}

	public static ITestCaseFinished TestCaseFinished(
		ITestCase testCase,
		decimal executionTime = 0m,
		int testsRun = 0,
		int testsFailed = 0,
		int testsSkipped = 0) =>
			new MockTestCaseFinished(testCase)
			{
				ExecutionTime = executionTime,
				TestsFailed = testsFailed,
				TestsRun = testsRun,
				TestsSkipped = testsSkipped,
			};

	class MockTestCaseFinished(ITestCase testCase) :
		_TestCaseBase, ITestCaseFinished
	{
		public required decimal ExecutionTime { get; set; }
		public required int TestsFailed { get; set; }
		public required int TestsRun { get; set; }
		public required int TestsSkipped { get; set; }

		public override ITestCase TestCase => testCase;
	}

	public static ITestCaseStarting TestCaseStarting(ITestCase testCase) =>
		new MockTestCaseStarting(testCase);

	class MockTestCaseStarting(ITestCase testCase) :
		_TestCaseBase, ITestCaseStarting
	{
		public override ITestCase TestCase => testCase;
	}

	public static ITestClassCleanupFailure TestClassCleanupFailure(
		ITestClass testClass,
		Exception ex,
		ITestCase[]? testCases = null)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		return new MockTestClassCleanupFailure(testClass)
		{
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			Messages = messages,
			StackTraces = stackTraces,
			TestCases = testCases ?? [],
		};
	}

	class MockTestClassCleanupFailure(ITestClass testClass) :
		_TestClassBase, ITestClassCleanupFailure
	{
		public required int[] ExceptionParentIndices { get; set; }
		public required string?[] ExceptionTypes { get; set; }
		public required string[] Messages { get; set; }
		public required string?[] StackTraces { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }

		public override ITestClass TestClass => testClass;
	}

	public static ITestClassConstructionStarting TestClassConstructionStarting(ITest test) =>
		new MockTestClassConstructionStarting(test);

	class MockTestClassConstructionStarting(ITest test) :
		_TestBase, ITestClassConstructionStarting
	{
		public override ITest Test => test;
	}

	public static ITestClassConstructionFinished TestClassConstructionFinished(ITest test) =>
		new MockTestClassConstructionFinished(test);

	class MockTestClassConstructionFinished(ITest test) :
		_TestBase, ITestClassConstructionFinished
	{
		public override ITest Test => test;
	}

	public static ITestClassDisposeFinished TestClassDisposeFinished(ITest test) =>
		new MockTestClassDisposeFinished(test);

	class MockTestClassDisposeFinished(ITest test) :
		_TestBase, ITestClassDisposeFinished
	{
		public override ITest Test => test;
	}

	public static ITestClassDisposeStarting TestClassDisposeStarting(ITest test) =>
		new MockTestClassDisposeStarting(test);

	class MockTestClassDisposeStarting(ITest test) :
		_TestBase, ITestClassDisposeStarting
	{
		public override ITest Test => test;
	}

	public static ITestClassFinished TestClassFinished(
		ITestClass testClass,
		decimal executionTime = 0m,
		ITestCase[]? testCases = null,
		int testsFailed = 0,
		int testsRun = 0,
		int testsSkipped = 0) =>
			new MockTestClassFinished(testClass)
			{
				ExecutionTime = executionTime,
				TestCases = testCases ?? [],
				TestsFailed = testsFailed,
				TestsRun = testsRun,
				TestsSkipped = testsSkipped,
			};

	class MockTestClassFinished(ITestClass testClass) :
		_TestClassBase, ITestClassFinished
	{
		public required decimal ExecutionTime { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }
		public required int TestsFailed { get; set; }
		public required int TestsRun { get; set; }
		public required int TestsSkipped { get; set; }

		public override ITestClass TestClass => testClass;
	}

	public static ITestClassStarting TestClassStarting(
		ITestClass testClass,
		ITestCase[]? testCases = null) =>
			new MockTestClassStarting(testClass)
			{
				TestCases = testCases ?? [],
			};

	class MockTestClassStarting(ITestClass testClass) :
		_TestClassBase, ITestClassStarting
	{
		public required IEnumerable<ITestCase> TestCases { get; set; }

		public override ITestClass TestClass => testClass;
	}

	public static ITestCleanupFailure TestCleanupFailure(
		ITest test,
		Exception ex)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		return new MockTestCleanupFailure(test)
		{
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			Messages = messages,
			StackTraces = stackTraces,
		};
	}

	class MockTestCleanupFailure(ITest test) :
		_TestBase, ITestCleanupFailure
	{
		public required int[] ExceptionParentIndices { get; set; }
		public required string?[] ExceptionTypes { get; set; }
		public required string[] Messages { get; set; }
		public required string?[] StackTraces { get; set; }

		public override ITest Test => test;
	}

	public static ITestCollectionCleanupFailure TestCollectionCleanupFailure(
		ITestCollection collection,
		Exception ex,
		ITestCase[]? testCases = null)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		return new MockTestCollectionCleanupFailure(collection)
		{
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			Messages = messages,
			StackTraces = stackTraces,
			TestCases = testCases ?? [],
		};
	}

	class MockTestCollectionCleanupFailure(ITestCollection testCollection) :
		_TestCollectionBase, ITestCollectionCleanupFailure
	{
		public required int[] ExceptionParentIndices { get; set; }
		public required string?[] ExceptionTypes { get; set; }
		public required string[] Messages { get; set; }
		public required string?[] StackTraces { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }

		public override ITestCollection TestCollection => testCollection;
	}

	public static ITestCollectionFinished TestCollectionFinished(
		ITestCollection testCollection,
		int testsRun = 0,
		int testsFailed = 0,
		int testsSkipped = 0,
		decimal executionTime = 0m,
		ITestCase[]? testCases = null) =>
			new MockTestCollectionFinished(testCollection)
			{
				ExecutionTime = executionTime,
				TestCases = testCases ?? [],
				TestsFailed = testsFailed,
				TestsRun = testsRun,
				TestsSkipped = testsSkipped,
			};

	class MockTestCollectionFinished(ITestCollection testCollection) :
		_TestCollectionBase, ITestCollectionFinished
	{
		public required decimal ExecutionTime { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }
		public required int TestsFailed { get; set; }
		public required int TestsRun { get; set; }
		public required int TestsSkipped { get; set; }

		public override ITestCollection TestCollection => testCollection;
	}

	public static ITestCollectionStarting TestCollectionStarting(
		ITestCollection testCollection,
		ITestCase[]? testCases = null) =>
			new MockTestCollectionStarting(testCollection)
			{
				TestCases = testCases ?? [],
			};

	class MockTestCollectionStarting(ITestCollection testCollection) :
		_TestCollectionBase, ITestCollectionStarting
	{
		public required IEnumerable<ITestCase> TestCases { get; set; }

		public override ITestCollection TestCollection => testCollection;
	}

	public static ITestFailed TestFailed(
		ITest test,
		decimal executionTime,
		string output,
		Exception ex)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		return new MockTestFailed(test)
		{
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			ExecutionTime = executionTime,
			Output = output,
			Messages = messages,
			StackTraces = stackTraces,
		};
	}

	class MockTestFailed(ITest test) :
		_TestBase, ITestFailed
	{
		public required int[] ExceptionParentIndices { get; set; }
		public required string?[] ExceptionTypes { get; set; }
		public required decimal ExecutionTime { get; set; }
		public required string[] Messages { get; set; }
		public required string Output { get; set; }
		public required string?[] StackTraces { get; set; }

		public override ITest Test => test;
	}

	public static ITestFinished TestFinished(
		ITest test,
		decimal executionTime,
		string output) =>
			new MockTestFinished(test)
			{
				ExecutionTime = executionTime,
				Output = output,
			};

	class MockTestFinished(ITest test) :
		_TestBase, ITestFinished
	{
		public required decimal ExecutionTime { get; set; }
		public required string Output { get; set; }

		public override ITest Test => test;
	}

	public static ITestMethodCleanupFailure TestMethodCleanupFailure(
		ITestMethod testMethod,
		Exception ex,
		ITestCase[]? testCases = null)
	{
		var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		return new MockTestMethodCleanupFailure(testMethod)
		{
			ExceptionParentIndices = exceptionParentIndices,
			ExceptionTypes = exceptionTypes,
			Messages = messages,
			StackTraces = stackTraces,
			TestCases = testCases ?? [],
		};
	}

	class MockTestMethodCleanupFailure(ITestMethod testMethod) :
		_TestMethodBase, ITestMethodCleanupFailure
	{
		public required int[] ExceptionParentIndices { get; set; }
		public required string?[] ExceptionTypes { get; set; }
		public required string[] Messages { get; set; }
		public required string?[] StackTraces { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }

		public override ITestMethod TestMethod => testMethod;
	}

	public static ITestMethodFinished TestMethodFinished(
		ITestMethod testMethod,
		int testsRun,
		int testsFailed,
		int testsSkipped,
		decimal executionTime,
		ITestCase[]? testCases = null) =>
			new MockTestMethodFinished(testMethod)
			{
				ExecutionTime = executionTime,
				TestCases = testCases ?? [],
				TestsFailed = testsFailed,
				TestsRun = testsRun,
				TestsSkipped = testsSkipped,
			};

	class MockTestMethodFinished(ITestMethod testMethod) :
		_TestMethodBase, ITestMethodFinished
	{
		public required decimal ExecutionTime { get; set; }
		public required IEnumerable<ITestCase> TestCases { get; set; }
		public required int TestsFailed { get; set; }
		public required int TestsRun { get; set; }
		public required int TestsSkipped { get; set; }

		public override ITestMethod TestMethod => testMethod;
	}

	public static ITestMethodStarting TestMethodStarting(
		ITestMethod testMethod,
		ITestCase[]? testCases = null) =>
			new MockTestMethodStarting(testMethod)
			{
				TestCases = testCases ?? [],
			};

	class MockTestMethodStarting(ITestMethod testMethod) :
		_TestMethodBase, ITestMethodStarting
	{
		public required IEnumerable<ITestCase> TestCases { get; set; }

		public override ITestMethod TestMethod => testMethod;
	}

	public static ITestOutput TestOutput(
		ITest test,
		string output) =>
			new MockTestOutput(test)
			{
				Output = output,
			};

	class MockTestOutput(ITest test) :
		_TestBase, ITestOutput
	{
		public required string Output { get; set; }

		public override ITest Test => test;
	}

	public static ITestPassed TestPassed(
		ITest test,
		decimal executionTime,
		string output) =>
			new MockTestPassed(test)
			{
				ExecutionTime = executionTime,
				Output = output,
			};

	class MockTestPassed(ITest test) :
		_TestBase, ITestPassed
	{
		public required decimal ExecutionTime { get; set; }
		public required string Output { get; set; }

		public override ITest Test => test;
	}

	public static ITestSkipped TestSkipped(
		ITest test,
		string reason) =>
			new MockTestSkipped(test)
			{
				ExecutionTime = 0,
				Output = string.Empty,
				Reason = reason,
			};

	class MockTestSkipped(ITest test) :
		_TestBase, ITestSkipped
	{
		public required decimal ExecutionTime { get; set; }
		public required string Output { get; set; }
		public required string Reason { get; set; }

		public override ITest Test => test;
	}

	public static ITestStarting TestStarting(ITest test) =>
		new MockTestStarting(test);

	class MockTestStarting(ITest test) :
		_TestBase, ITestStarting
	{
		public override ITest Test => test;
	}

	abstract class _TestAssemblyBase : LongLivedMarshalByRefObject, ITestAssemblyMessage
	{
		public abstract ITestAssembly TestAssembly { get; }
	}

	abstract class _TestCollectionBase : _TestAssemblyBase, ITestCollectionMessage
	{
		public override ITestAssembly TestAssembly => TestCollection.TestAssembly;
		public abstract ITestCollection TestCollection { get; }
	}

	abstract class _TestClassBase : _TestCollectionBase, ITestCollectionMessage
	{
		public override ITestCollection TestCollection => TestClass.TestCollection;
		public abstract ITestClass TestClass { get; }
	}

	abstract class _TestMethodBase : _TestClassBase, ITestClassMessage
	{
		public override ITestClass TestClass => TestMethod.TestClass;
		public abstract ITestMethod TestMethod { get; }
	}

	abstract class _TestCaseBase : _TestMethodBase, ITestMethodMessage, IExecutionMessage
	{
		public abstract ITestCase TestCase { get; }
		public IEnumerable<ITestCase> TestCases => [TestCase];
		public override ITestMethod TestMethod => TestCase.TestMethod;
	}

	abstract class _TestBase : _TestCaseBase, ITestCaseMessage
	{
		public abstract ITest Test { get; }
		public override ITestCase TestCase => Test.TestCase;
	}
}
