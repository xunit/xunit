using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Represents information about the current state of the test engine. It may be available at
/// various points during the execution pipeline, so consumers must always take care to ensure
/// that they check for <c>null</c> values from the various properties.
/// </summary>
public sealed class TestContext : ITestContext, IDisposable
{
	static readonly TestContext idleTestContext = new(null, null, null, TestPipelineStage.Unknown, default);
	static readonly AsyncLocal<TestContext?> local = new();
	static readonly HashSet<TestEngineStatus> validExecutionStatuses = [TestEngineStatus.Initializing, TestEngineStatus.Running, TestEngineStatus.CleaningUp];

	readonly Dictionary<string, TestAttachment>? attachments;
	IMessageSink? diagnosticMessageSink;
	readonly FixtureMappingManager? fixtures;
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
		FixtureMappingManager? fixtures = null,
		List<string>? warnings = null)
	{
		DiagnosticMessageSink = diagnosticMessageSink;
		InternalDiagnosticMessageSink = internalDiagnosticMessageSink;
		this.keyValueStorage = keyValueStorage;
		PipelineStage = pipelineStage;
		CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, testCancellationTokenSource.Token).Token;
		this.attachments = attachments;
		this.fixtures = fixtures;
		this.warnings = warnings;
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, TestAttachment>? Attachments => attachments;

	/// <inheritdoc/>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	/// Gets the current test context. If called outside of the text discovery or execution path,
	/// will return a test context that is in the <see cref="TestPipelineStage.Unknown"/> stage.
	/// The current test context is a "snapshot in time" for when this/ property is called, so do
	/// not cache the instance across a single method boundary (or else/ you run the risk of having
	/// an out-of-date context).
	/// </summary>
	public static ITestContext Current =>
		CurrentInternal;

	internal static TestContext CurrentInternal =>
		local.Value ?? idleTestContext;

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

	/// <inheritdoc/>
	public Dictionary<string, object?> KeyValueStorage =>
		keyValueStorage ?? throw new InvalidOperationException("Cannot get KeyValueStorage on the idle test context");

	/// <inheritdoc/>
	public TestPipelineStage PipelineStage { get; private set; }

	/// <inheritdoc/>
	public ITest? Test { get; private set; }

	/// <inheritdoc/>
	public ITestAssembly? TestAssembly { get; private set; }

	/// <inheritdoc/>
	public TestEngineStatus? TestAssemblyStatus { get; private set; }

	/// <inheritdoc/>
	public ITestCase? TestCase { get; private set; }

	/// <inheritdoc/>
	public TestEngineStatus? TestCaseStatus { get; private set; }

	/// <inheritdoc/>
	public ITestClass? TestClass { get; private set; }

	/// <inheritdoc/>
	public object? TestClassInstance { get; private set; }

	/// <inheritdoc/>
	public TestEngineStatus? TestClassStatus { get; private set; }

	/// <inheritdoc/>
	public ITestCollection? TestCollection { get; private set; }

	/// <inheritdoc/>
	public TestEngineStatus? TestCollectionStatus { get; private set; }

	/// <inheritdoc/>
	public ITestMethod? TestMethod { get; private set; }

	/// <inheritdoc/>
	public TestEngineStatus? TestMethodStatus { get; private set; }

	/// <inheritdoc/>
	public ITestOutputHelper? TestOutputHelper { get; private set; }

	/// <inheritdoc/>
	public TestResultState? TestState { get; private set; }

	/// <inheritdoc/>
	public TestEngineStatus? TestStatus { get; private set; }

	/// <inheritdoc/>
	public IReadOnlyList<string>? Warnings =>
		warnings?.Count > 0 ? warnings : null;

	/// <inheritdoc/>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public void AddWarning(string message)
	{
		if (Test is null || warnings is null)
			SendDiagnosticMessage("Attempted to log a test warning message while not running a test (pipeline stage = {0}); message: {1}", PipelineStage, message);
		else
			warnings.Add(message);
	}

	/// <inheritdoc/>
	public void CancelCurrentTest() =>
		testCancellationTokenSource.Cancel();

	/// <inheritdoc/>
	public void Dispose() =>
		testCancellationTokenSource.Dispose();

	/// <inheritdoc/>
	public ValueTask<object?> GetFixture(Type fixtureType)
	{
		if (fixtures is null)
			return new(null);

		return fixtures.GetFixture(fixtureType);
	}

	/// <inheritdoc/>
	public void SendDiagnosticMessage(string message)
		=> DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(message));

	/// <inheritdoc/>
	public void SendDiagnosticMessage(
		string format,
		object? arg0) =>
			DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(format, arg0));

	/// <inheritdoc/>
	public void SendDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1) =>
			DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(format, arg0, arg1));

	/// <inheritdoc/>
	public void SendDiagnosticMessage(
		string format,
		object? arg0,
		object? arg1,
		object? arg2) =>
			DiagnosticMessageSink?.OnMessage(new DiagnosticMessage(format, arg0, arg1, arg2));

	/// <inheritdoc/>
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

		if (testStatus == TestEngineStatus.CleaningUp)
			Guard.ArgumentNotNull(testState);

		if (CurrentInternal.TestOutputHelper is null)
			Guard.ArgumentNotNull(testOutputHelper);

		local.Value = new TestContext(CurrentInternal.DiagnosticMessageSink, CurrentInternal.InternalDiagnosticMessageSink, CurrentInternal.KeyValueStorage, TestPipelineStage.TestExecution, cancellationToken, CurrentInternal.attachments ?? [], CurrentInternal.fixtures, CurrentInternal.warnings ?? [])
		{
			Test = test,
			TestClassInstance = testClassInstance,
			TestOutputHelper = testOutputHelper ?? CurrentInternal.TestOutputHelper,
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

		var pipelineStage =
			testAssemblyStatus == TestEngineStatus.Discovering
				? TestPipelineStage.Discovery
				: TestPipelineStage.TestAssemblyExecution;

		local.Value = new TestContext(CurrentInternal.DiagnosticMessageSink, CurrentInternal.InternalDiagnosticMessageSink, CurrentInternal.KeyValueStorage, pipelineStage, cancellationToken)
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

		local.Value = new TestContext(CurrentInternal.DiagnosticMessageSink, CurrentInternal.InternalDiagnosticMessageSink, CurrentInternal.KeyValueStorage, TestPipelineStage.TestCaseExecution, cancellationToken, fixtures: CurrentInternal.fixtures)
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
	/// <param name="fixtures">The fixtures that are available to the test class</param>
	public static void SetForTestClass(
		ITestClass testClass,
		TestEngineStatus testClassStatus,
		CancellationToken cancellationToken,
		FixtureMappingManager? fixtures = null)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentEnumValid(testClassStatus, validExecutionStatuses);

		local.Value = new TestContext(CurrentInternal.DiagnosticMessageSink, CurrentInternal.InternalDiagnosticMessageSink, CurrentInternal.KeyValueStorage, TestPipelineStage.TestClassExecution, cancellationToken, fixtures: fixtures)
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

		local.Value = new TestContext(CurrentInternal.DiagnosticMessageSink, CurrentInternal.InternalDiagnosticMessageSink, CurrentInternal.KeyValueStorage, TestPipelineStage.TestCollectionExecution, cancellationToken)
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

		local.Value = new TestContext(CurrentInternal.DiagnosticMessageSink, CurrentInternal.InternalDiagnosticMessageSink, CurrentInternal.KeyValueStorage, TestPipelineStage.TestMethodExecution, cancellationToken, fixtures: CurrentInternal.fixtures)
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
