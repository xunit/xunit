using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Represents information about the current state of the test engine. It may be available at
/// various points during the execution pipeline, so consumers must always take care to ensure
/// that they check for <c>null</c> values from the various properties.
/// </summary>
public class TestContext
{
	static readonly AsyncLocal<TestContext?> local = new();
	static readonly HashSet<TestEngineStatus> validExecutionStatuses = new() { TestEngineStatus.Initializing, TestEngineStatus.Running, TestEngineStatus.CleaningUp };

	readonly List<string>? warnings;

	TestContext(
		_IMessageSink? diagnosticMessageSink,
		_IMessageSink? internalDiagnosticMessageSink,
		TestPipelineStage pipelineStage,
		CancellationToken cancellationToken,
		List<string>? warnings = null)
	{
		DiagnosticMessageSink = diagnosticMessageSink;
		InternalDiagnosticMessageSink = internalDiagnosticMessageSink;
		PipelineStage = pipelineStage;
		CancellationToken = cancellationToken;
		this.warnings = warnings;
	}

	/// <summary>
	/// Gets the cancellation token that is used to indicate that the test run should be
	/// aborted. Async tests should pass this along to any async functions that support
	/// cancellation tokens, to help speed up the cancellation process.
	/// </summary>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	/// Gets the current test context. If called outside of the text discovery or execution path,
	/// will return <c>null</c>. The current test context is a "snapshot in time" for when this
	/// property is called, so do not cache the instance across a single method boundary (or else
	/// you run the risk of having an out-of-date context).
	/// </summary>
	public static TestContext? Current => local.Value;

	internal _IMessageSink? DiagnosticMessageSink { get; set; }

	internal _IMessageSink? InternalDiagnosticMessageSink { get; set; }

	/// <summary>
	/// Gets the current test pipeline stage.
	/// </summary>
	public TestPipelineStage PipelineStage { get; private set; }

	/// <summary>
	/// Gets the current test, if the engine is currently in the process of running a test;
	/// will return <c>null</c> outside of the context of a test.
	/// </summary>
	public _ITest? Test { get; private set; }

	/// <summary>
	/// Gets the current test assembly, if the engine is currently in the process of running or
	/// discovering tests in assembly; will return <c>null</c> out of this context (this typically
	/// means the test framework itself is being created and initialized).
	/// </summary>
	[NotNullIfNotNull(nameof(TestCollection))]
	public _ITestAssembly? TestAssembly { get; private set; }

	/// <summary>
	/// Gets the current test engine status for the test assembly.
	/// </summary>
	[NotNullIfNotNull(nameof(TestAssembly))]
	public TestEngineStatus? TestAssemblyStatus { get; private set; }

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
	public TestResultState? TestState { get; private set; }

	/// <summary>
	/// Gets the current test engine status for the test. Will only be available when <see cref="Test"/>
	/// is not <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(Test))]
	public TestEngineStatus? TestStatus { get; private set; }

	/// <summary>
	/// Gets the set of warnings associated with the current test. Will only be available when <see cref="Test"/>
	/// is not <c>null</c>; will also return <c>null</c> if there have been no warnings issued.
	/// </summary>
	public IReadOnlyList<string>? Warnings =>
		warnings?.Count > 0 ? warnings : null;

	/// <summary>
	/// Adds a warning to the test result.
	/// </summary>
	/// <param name="message">The warning message to be reported</param>
	public void AddWarning(string message)
	{
		if (Test is null || warnings is null)
			SendDiagnosticMessage("Attempted to log a test warning message while not running a test (pipeline stage = {0}); message: {1}", PipelineStage, message);
		else
			warnings.Add(message);
	}

	/// <summary>
	/// Sends a diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="message">The message to send</param>
	public void SendDiagnosticMessage(string message)
	{
		if (DiagnosticMessageSink is not null)
			DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = message });
	}

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	public void SendDiagnosticMessage(
		string format,
		object? arg0)
	{
		if (DiagnosticMessageSink is not null)
			DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, arg0) });
	}

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	public void SendDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1)
	{
		if (DiagnosticMessageSink is not null)
			DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, arg0, arg1) });
	}

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	/// <param name="arg2">The value to replace {2} in the format string.</param>
	public void SendDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1,
		object? arg2)
	{
		if (DiagnosticMessageSink is not null)
			DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, arg0, arg1, arg2) });
	}

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public void SendDiagnosticMessage(
		string format,
		params object?[] args)
	{
		if (DiagnosticMessageSink is not null)
			DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, args) });
	}

	internal void SendInternalDiagnosticMessage(string message)
	{
		if (InternalDiagnosticMessageSink is not null)
			InternalDiagnosticMessageSink.OnMessage(new _InternalDiagnosticMessage { Message = message });
	}

	internal void SendInternalDiagnosticMessage(
		string format,
		object? arg0)
	{
		if (InternalDiagnosticMessageSink is not null)
			InternalDiagnosticMessageSink.OnMessage(new _InternalDiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, arg0) });
	}

	internal void SendInternalDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1)
	{
		if (InternalDiagnosticMessageSink is not null)
			InternalDiagnosticMessageSink.OnMessage(new _InternalDiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, arg0, arg1) });
	}

	internal void SendInternalDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1,
		object? arg2)
	{
		if (InternalDiagnosticMessageSink is not null)
			InternalDiagnosticMessageSink.OnMessage(new _InternalDiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, arg0, arg1, arg2) });
	}

	internal void SendInternalDiagnosticMessage(
		string format,
		params object?[] args)
	{
		if (InternalDiagnosticMessageSink is not null)
			InternalDiagnosticMessageSink.OnMessage(new _InternalDiagnosticMessage { Message = string.Format(CultureInfo.CurrentCulture, format, args) });
	}

	/// <summary>
	/// Sets the test context for test framework initialization. This is the moment before any specific assembly is
	/// being discovered or run. This is typically used by custom runners just before they create the test framework
	/// via a call to <see cref="ExtensibilityPointFactory.GetTestFramework"/>.
	/// </summary>
	/// <param name="diagnosticMessageSink">The optional message sink used to receive <see cref="_DiagnosticMessage"/>
	/// and <see cref="_InternalDiagnosticMessage"/> instances.</param>
	/// <param name="diagnosticMessages">A flag to indicate whether the user wants to receive diagnostic messages</param>
	/// <param name="internalDiagnosticMessages">A flag to indicate whether the user wants to receive internal diagnostic messages</param>
	public static void SetForInitialization(
		_IMessageSink? diagnosticMessageSink,
		bool diagnosticMessages,
		bool internalDiagnosticMessages) =>
			local.Value = new TestContext(diagnosticMessages ? diagnosticMessageSink : null, internalDiagnosticMessages ? diagnosticMessageSink : null, TestPipelineStage.Initialization, default);

	/// <summary>
	/// Sets the test context for execution of a test. This assumes an existing test context already exists from which
	/// it can pull the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="test">The test that is being executed</param>
	/// <param name="testStatus">The test status (valid values: <see cref="TestEngineStatus.Initializing"/>,
	/// <see cref="TestEngineStatus.Running"/>, and <see cref="TestEngineStatus.CleaningUp"/>)</param>
	/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
	/// <param name="testState">The state of the test (only required when <paramref name="testStatus"/>
	/// is <see cref="TestEngineStatus.CleaningUp"/>).</param>
	/// <param name="testOutputHelper">The test output helper that the test can use to write output. Must be passed
	/// when <paramref name="testStatus"/> is <see cref="TestEngineStatus.Initializing"/>; can be <c>null</c> for
	/// other statuses (as it will be pulled from the existing test context).</param>
	public static void SetForTest(
		_ITest test,
		TestEngineStatus testStatus,
		CancellationToken cancellationToken,
		TestResultState? testState = null,
		_ITestOutputHelper? testOutputHelper = null)
	{
		Guard.ArgumentNotNull(test);
		Guard.ArgumentEnumValid(testStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		if (testStatus == TestEngineStatus.CleaningUp)
			Guard.ArgumentNotNull(testState);

		if (Current.TestOutputHelper is null)
			Guard.ArgumentNotNull(testOutputHelper);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, TestPipelineStage.TestExecution, cancellationToken, Current.warnings ?? new())
		{
			Test = test,
			TestStatus = testStatus,
			TestOutputHelper = testOutputHelper ?? Current.TestOutputHelper,
			TestState = testState,

			TestCase = test.TestCase,
			TestCaseStatus = TestEngineStatus.Running,

			TestMethod = test.TestCase.TestMethod,
			TestMethodStatus = test.TestCase.TestMethod is null ? null : TestEngineStatus.Running,

			TestClass = test.TestCase.TestClass,
			TestClassStatus = test.TestCase.TestClass is null ? null : TestEngineStatus.Running,

			TestCollection = test.TestCase.TestCollection,
			TestCollectionStatus = TestEngineStatus.Running,

			TestAssembly = test.TestCase.TestCollection.TestAssembly,
			TestAssemblyStatus = TestEngineStatus.Running,
		};
	}

	/// <summary>
	/// Sets the test context for discovery or execution of a test assembly. This assumes an existing test context already exists
	/// from which it can pull the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="testAssembly">The test assembly that is being executed</param>
	/// <param name="testAssemblyStatus">The test assembly status</param>
	/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
	public static void SetForTestAssembly(
		_ITestAssembly testAssembly,
		TestEngineStatus testAssemblyStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testAssembly);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		var pipelineStage =
			testAssemblyStatus == TestEngineStatus.Discovering
				? TestPipelineStage.Discovery
				: TestPipelineStage.TestAssemblyExecution;

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, pipelineStage, cancellationToken)
		{
			TestAssembly = testAssembly,
			TestAssemblyStatus = testAssemblyStatus,
		};
	}

	/// <summary>
	/// Sets the test context for execution of a test case. This assumes an existing test context already exists from which
	/// it can pull the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="testCase">The test case that is being executed</param>
	/// <param name="testCaseStatus">The test case status (valid values: <see cref="TestEngineStatus.Initializing"/>,
	/// <see cref="TestEngineStatus.Running"/>, and <see cref="TestEngineStatus.CleaningUp"/>)</param>
	/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
	public static void SetForTestCase(
		_ITestCase testCase,
		TestEngineStatus testCaseStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testCase);
		Guard.ArgumentEnumValid(testCaseStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, TestPipelineStage.TestCaseExecution, cancellationToken)
		{
			TestCase = testCase,
			TestCaseStatus = testCaseStatus,

			TestMethod = testCase.TestMethod,
			TestMethodStatus = testCase.TestMethod is null ? null : TestEngineStatus.Running,

			TestClass = testCase.TestClass,
			TestClassStatus = testCase.TestClass is null ? null : TestEngineStatus.Running,

			TestCollection = testCase.TestCollection,
			TestCollectionStatus = TestEngineStatus.Running,

			TestAssembly = testCase.TestCollection.TestAssembly,
			TestAssemblyStatus = TestEngineStatus.Running,
		};
	}

	/// <summary>
	/// Sets the test context for execution of a test class. This assumes an existing test context already exists from which
	/// it can pull the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="testClass">The test class that is being executed</param>
	/// <param name="testClassStatus">The test class status (valid values: <see cref="TestEngineStatus.Initializing"/>,
	/// <see cref="TestEngineStatus.Running"/>, and <see cref="TestEngineStatus.CleaningUp"/>)</param>
	/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
	public static void SetForTestClass(
		_ITestClass testClass,
		TestEngineStatus testClassStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentEnumValid(testClassStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, TestPipelineStage.TestClassExecution, cancellationToken)
		{
			TestClass = testClass,
			TestClassStatus = testClassStatus,

			TestCollection = testClass.TestCollection,
			TestCollectionStatus = TestEngineStatus.Running,

			TestAssembly = testClass.TestCollection.TestAssembly,
			TestAssemblyStatus = TestEngineStatus.Running,
		};
	}

	/// <summary>
	/// Sets the test context for execution of a test collection. This assumes an existing test context already exists from which
	/// it can pull the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="testCollection">The test collection that is being executed</param>
	/// <param name="testCollectionStatus">The test collection status (valid values: <see cref="TestEngineStatus.Initializing"/>,
	/// <see cref="TestEngineStatus.Running"/>, and <see cref="TestEngineStatus.CleaningUp"/>)</param>
	/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
	public static void SetForTestCollection(
		_ITestCollection testCollection,
		TestEngineStatus testCollectionStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentEnumValid(testCollectionStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, TestPipelineStage.TestCollectionExecution, cancellationToken)
		{
			TestCollection = testCollection,
			TestCollectionStatus = testCollectionStatus,

			TestAssembly = testCollection.TestAssembly,
			TestAssemblyStatus = TestEngineStatus.Running,
		};
	}

	/// <summary>
	/// Sets the test context for execution of a test method. This assumes an existing test context already exists from which
	/// it can pull the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="testMethod">The test method that is being executed</param>
	/// <param name="testMethodStatus">The test method status (valid values: <see cref="TestEngineStatus.Initializing"/>,
	/// <see cref="TestEngineStatus.Running"/>, and <see cref="TestEngineStatus.CleaningUp"/>)</param>
	/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
	public static void SetForTestMethod(
		_ITestMethod testMethod,
		TestEngineStatus testMethodStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentEnumValid(testMethodStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, TestPipelineStage.TestMethodExecution, cancellationToken)
		{
			TestMethod = testMethod,
			TestMethodStatus = testMethodStatus,

			TestClass = testMethod.TestClass,
			TestClassStatus = TestEngineStatus.Running,

			TestCollection = testMethod.TestClass.TestCollection,
			TestCollectionStatus = TestEngineStatus.Running,

			TestAssembly = testMethod.TestClass.TestCollection.TestAssembly,
			TestAssemblyStatus = TestEngineStatus.Running,
		};
	}
}
