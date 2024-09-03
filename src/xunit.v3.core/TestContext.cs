using System;
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
public sealed class TestContext : IDisposable
{
	static readonly TestContext idleTestContext = new(null, null, null, TestPipelineStage.Unknown, default);
	static readonly AsyncLocal<TestContext?> local = new();
	static readonly HashSet<TestEngineStatus> validExecutionStatuses = [TestEngineStatus.Initializing, TestEngineStatus.Running, TestEngineStatus.CleaningUp];

	readonly Dictionary<string, TestAttachment>? attachments;
	IMessageSink? diagnosticMessageSink;
	IMessageSink? internalDiagnosticMessageSink;
	readonly Dictionary<string, object?>? keyValueStorage;
	readonly CancellationTokenSource testCancellationTokenSource = new();
	readonly List<string>? warnings;

	TestContext(
		IMessageSink? diagnosticMessageSink,
		IMessageSink? internalDiagnosticMessageSink,
		Dictionary<string, object?>? keyValueStorage,
		TestPipelineStage pipelineStage,
		CancellationToken cancellationToken,
		Dictionary<string, TestAttachment>? attachments = null,
		List<string>? warnings = null)
	{
		DiagnosticMessageSink = diagnosticMessageSink;
		InternalDiagnosticMessageSink = internalDiagnosticMessageSink;
		this.keyValueStorage = keyValueStorage;
		PipelineStage = pipelineStage;
		CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, testCancellationTokenSource.Token).Token;
		this.attachments = attachments;
		this.warnings = warnings;
	}

	/// <summary>
	/// Gets the attachments for the current test, if the engine is currently in the process of running a test;
	/// will return <c>null</c> outside of the context of a test.
	/// </summary>
	[NotNullIfNotNull(nameof(Test))]
	public IReadOnlyDictionary<string, TestAttachment>? Attachments => attachments;

	/// <summary>
	/// Gets the cancellation token that is used to indicate that the test run should be
	/// aborted. Async tests should pass this along to any async functions that support
	/// cancellation tokens, to help speed up the cancellation process.
	/// </summary>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	/// Gets the current test context. If called outside of the text discovery or execution path,
	/// will return a test context that is in the <see cref="TestPipelineStage.Unknown"/> stage.
	/// The current test context is a "snapshot in time" for when this/ property is called, so do
	/// not cache the instance across a single method boundary (or else/ you run the risk of having
	/// an out-of-date context).
	/// </summary>
	public static TestContext Current => local.Value ?? idleTestContext;

	internal IMessageSink? DiagnosticMessageSink
	{
		get => diagnosticMessageSink;
		set
		{
			if (ReferenceEquals(this, idleTestContext))
				throw new InvalidOperationException("Cannot set DiagnosticMessageSink on the idle test context");

			diagnosticMessageSink = value;
		}
	}

	internal IMessageSink? InternalDiagnosticMessageSink
	{
		get => internalDiagnosticMessageSink;
		set
		{
			if (ReferenceEquals(this, idleTestContext))
				throw new InvalidOperationException("Cannot set InternalDiagnosticMessageSink on the idle test context");

			internalDiagnosticMessageSink = value;
		}
	}

	/// <summary>
	/// Stores key/value pairs that are available across all stages of the pipeline. Can be used
	/// to communicate between extensions at different execution stages, in both directions, as
	/// a single storage container is used for the entire pipeline.
	/// </summary>
	/// <remarks>
	/// This storage system is purely for communication between extension points. The values in here
	/// are thrown away after the pipeline execution is complete. It is strongly recommend that
	/// extensions either prefix their key names or use guaranteed unique IDs like GUIDs, to prevent
	/// collisions with other extension authors.
	/// </remarks>
	public Dictionary<string, object?> KeyValueStorage =>
		keyValueStorage ?? throw new InvalidOperationException("Cannot get KeyValueStorage on the idle test context");

	/// <summary>
	/// Gets the current test pipeline stage.
	/// </summary>
	public TestPipelineStage PipelineStage { get; private set; }

	/// <summary>
	/// Gets the current test, if the engine is currently in the process of running a test;
	/// will return <c>null</c> outside of the context of a test.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="IXunitTest"/>.
	/// </remarks>
	public ITest? Test { get; private set; }

	/// <summary>
	/// Gets the current test assembly, if the engine is currently in the process of running or
	/// discovering tests in assembly; will return <c>null</c> out of this context (this typically
	/// means the test framework itself is being created and initialized).
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="IXunitTestAssembly"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(TestCollection))]
	public ITestAssembly? TestAssembly { get; private set; }

	/// <summary>
	/// Gets the current test engine status for the test assembly.
	/// </summary>
	[NotNullIfNotNull(nameof(TestAssembly))]
	public TestEngineStatus? TestAssemblyStatus { get; private set; }

	/// <summary>
	/// Gets the current test case, if the engine is currently in the process of running a
	/// test case; will return <c>null</c> outside of the context of a test case.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="IXunitTestCase"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(Test))]
	public ITestCase? TestCase { get; private set; }

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
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="IXunitTestClass"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(TestMethod))]
	public ITestClass? TestClass { get; private set; }

	/// <summary>
	/// Gets the instance of the test class; will return <c>null</c> outside of the context of
	/// a test. Static test methods do not create test class instances, so this will always be <c>null</c>
	/// for static test methods.
	/// </summary>
	/// <remarks>
	/// This value will only be available when <see cref="PipelineStage"/> is <see cref="TestPipelineStage.TestExecution"/>
	/// and <see cref="TestStatus"/> is <see cref="TestEngineStatus.Running"/>, and only after the test class has been
	/// created. It will become <c>null</c> again immediately after the test class has been disposed.
	/// </remarks>
	public object? TestClassInstance { get; private set; }

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
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="IXunitTestCollection"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(TestClass))]
	[NotNullIfNotNull(nameof(TestCase))]
	public ITestCollection? TestCollection { get; private set; }

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
	public ITestOutputHelper? TestOutputHelper { get; private set; }

	/// <summary>
	/// Gets the current test method, if the engine is currently in the process of running
	/// a test method; will return <c>null</c> outside of the context of a test method. Note that
	/// not all test framework implementations require that tests be based on methods, so this
	/// value may be <c>null</c> even if <see cref="TestCase"/> is not <c>null</c>.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="IXunitTestMethod"/>.
	/// </remarks>
	public ITestMethod? TestMethod { get; private set; }

	/// <summary>
	/// Gets the current test engine status for the test method. Will only be available when <see cref="TestMethod"/>
	/// is not <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestMethod))]
	public TestEngineStatus? TestMethodStatus { get; private set; }

	/// <summary>
	/// Gets the current state of the test. Will only be available after the test has finished running.
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
	/// Adds an attachment that is a string value.
	/// </summary>
	/// <param name="name">The name of the attachment</param>
	/// <param name="value">The value of the attachment</param>
	public void AddAttachment(
		string name,
		string value)
	{
		Guard.ArgumentNotNull(name);
		Guard.ArgumentNotNull(value);

		if (Test is null || attachments is null)
			SendDiagnosticMessage("Attempted to add an attachment while not running a test (pipeline stage = {0}); name: {1}", PipelineStage, name);
		else
			lock (attachments)
			{
				if (attachments.ContainsKey(name))
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Attempted to add an attachment with an existing name: '{0}'", name));
				else
					attachments.Add(name, TestAttachment.Create(value));
			}
	}

	/// <summary>
	/// Adds an attachment that is a binary value (represented by a byte array and media type).
	/// </summary>
	/// <param name="name">The name of the attachment</param>
	/// <param name="value">The value of the attachment</param>
	/// <param name="mediaType">The media type of the attachment; defaults to "application/octet-stream"</param>
	/// <remarks>
	/// The <paramref name="mediaType"/> value must be in the MIME "type/subtype" form, and does not support
	/// parameter values. The subtype is allowed to have a single "+" to denote specialization of the
	/// subtype (i.e., "application/xhtml+xml"). For more information on media types, see
	/// <see href="https://datatracker.ietf.org/doc/html/rfc2045#section-5.1"/>.
	/// </remarks>
	public void AddAttachment(
		string name,
		byte[] value,
		string mediaType = "application/octet-stream")
	{
		Guard.ArgumentNotNull(name);
		Guard.ArgumentNotNull(value);

		if (Test is null || attachments is null)
			SendDiagnosticMessage("Attempted to add an attachment while not running a test (pipeline stage = {0}); name: {1}", PipelineStage, name);
		else
			lock (attachments)
			{
				attachments[name] =
					!attachments.ContainsKey(name)
						? TestAttachment.Create(value, mediaType)
						: throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Attempted to add an attachment with an existing name: '{0}'", name));
			}
	}

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
	/// Attempt to cancel the currently executing test, if one is executing. This will
	/// signal the <see cref="CancellationToken"/> for cancellation.
	/// </summary>
	public void CancelCurrentTest() =>
		testCancellationTokenSource.Cancel();

	/// <inheritdoc/>
	public void Dispose() =>
		testCancellationTokenSource.Dispose();

	/// <summary>
	/// Sends a diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="message">The message to send</param>
	public void SendDiagnosticMessage(string message)
		=> DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(message));

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	public void SendDiagnosticMessage(
		string format,
		object? arg0) =>
			DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(format, arg0));

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
		object? arg1) =>
			DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(format, arg0, arg1));

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
		object? arg2) =>
			DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(format, arg0, arg1, arg2));

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public void SendDiagnosticMessage(
		string format,
		params object?[] args) =>
			DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(format, args));

	internal void SendInternalDiagnosticMessage(string message) =>
		InternalDiagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage(message));

	internal void SendInternalDiagnosticMessage(
		string format,
		object? arg0) =>
			InternalDiagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage(format, arg0));

	internal void SendInternalDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1) =>
			InternalDiagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage(format, arg0, arg1));

	internal void SendInternalDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1,
		object? arg2) =>
			InternalDiagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage(format, arg0, arg1, arg2));

	internal void SendInternalDiagnosticMessage(
		string format,
		params object?[] args) =>
			InternalDiagnosticMessageSink?.OnMessage(new InternalDiagnosticMessage(format, args));

	/// <summary>
	/// Sets the test context for test framework initialization. This is the moment before any specific assembly is
	/// being discovered or run. This is typically used by custom runners just before they create the test framework
	/// via a call to <see cref="ExtensibilityPointFactory.GetTestFramework"/>.
	/// </summary>
	/// <param name="diagnosticMessageSink">The optional message sink used to receive <see cref="IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> instances.</param>
	/// <param name="diagnosticMessages">A flag to indicate whether the user wants to receive diagnostic messages</param>
	/// <param name="internalDiagnosticMessages">A flag to indicate whether the user wants to receive internal diagnostic messages</param>
	public static void SetForInitialization(
		IMessageSink? diagnosticMessageSink,
		bool diagnosticMessages,
		bool internalDiagnosticMessages) =>
			local.Value = new TestContext(diagnosticMessages ? diagnosticMessageSink : null, internalDiagnosticMessages ? diagnosticMessageSink : null, [], TestPipelineStage.Initialization, default);

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
	/// <param name="testClassInstance">The instance of the test class (may be <c>null</c> for static class and
	/// before the test class has been created)</param>
	public static void SetForTest(
		ITest test,
		TestEngineStatus testStatus,
		CancellationToken cancellationToken,
		TestResultState? testState = null,
		ITestOutputHelper? testOutputHelper = null,
		object? testClassInstance = null)
	{
		Guard.ArgumentNotNull(test);
		Guard.ArgumentEnumValid(testStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		if (testStatus == TestEngineStatus.CleaningUp)
			Guard.ArgumentNotNull(testState);

		if (Current.TestOutputHelper is null)
			Guard.ArgumentNotNull(testOutputHelper);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, Current.KeyValueStorage, TestPipelineStage.TestExecution, cancellationToken, Current.attachments ?? [], Current.warnings ?? [])
		{
			Test = test,
			TestClassInstance = testClassInstance,
			TestOutputHelper = testOutputHelper ?? Current.TestOutputHelper,
			TestState = testState,
			TestStatus = testStatus,

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
		ITestAssembly testAssembly,
		TestEngineStatus testAssemblyStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testAssembly);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		var pipelineStage =
			testAssemblyStatus == TestEngineStatus.Discovering
				? TestPipelineStage.Discovery
				: TestPipelineStage.TestAssemblyExecution;

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, Current.KeyValueStorage, pipelineStage, cancellationToken)
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
		ITestCase testCase,
		TestEngineStatus testCaseStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testCase);
		Guard.ArgumentEnumValid(testCaseStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, Current.KeyValueStorage, TestPipelineStage.TestCaseExecution, cancellationToken)
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
		ITestClass testClass,
		TestEngineStatus testClassStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentEnumValid(testClassStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, Current.KeyValueStorage, TestPipelineStage.TestClassExecution, cancellationToken)
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
		ITestCollection testCollection,
		TestEngineStatus testCollectionStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentEnumValid(testCollectionStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, Current.KeyValueStorage, TestPipelineStage.TestCollectionExecution, cancellationToken)
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
		ITestMethod testMethod,
		TestEngineStatus testMethodStatus,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentEnumValid(testMethodStatus, validExecutionStatuses);
		Guard.NotNull("TestContext.Current must be non-null", Current);

		local.Value = new TestContext(Current.DiagnosticMessageSink, Current.InternalDiagnosticMessageSink, Current.KeyValueStorage, TestPipelineStage.TestMethodExecution, cancellationToken)
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
