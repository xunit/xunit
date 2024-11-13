using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Represents the current state of the test pipeline.
/// </summary>
public interface ITestContext
{
	/// <summary>
	/// Gets the attachments for the current test, if the engine is currently in the process of running a test;
	/// will return <c>null</c> outside of the context of a test.
	/// </summary>
	[NotNullIfNotNull(nameof(Test))]
	IReadOnlyDictionary<string, TestAttachment>? Attachments { get; }

	/// <summary>
	/// Gets the cancellation token that is used to indicate that the test run should be
	/// aborted. Async tests should pass this along to any async functions that support
	/// cancellation tokens, to help speed up the cancellation process.
	/// </summary>
	CancellationToken CancellationToken { get; }

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
	Dictionary<string, object?> KeyValueStorage { get; }

	/// <summary>
	/// Gets the current test pipeline stage.
	/// </summary>
	TestPipelineStage PipelineStage { get; }

	/// <summary>
	/// Gets the current test, if the engine is currently in the process of running a test;
	/// will return <c>null</c> outside of the context of a test.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="IXunitTest"/>.
	/// </remarks>
	ITest? Test { get; }

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
	ITestAssembly? TestAssembly { get; }

	/// <summary>
	/// Gets the current test engine status for the test assembly.
	/// </summary>
	[NotNullIfNotNull(nameof(TestAssembly))]
	TestEngineStatus? TestAssemblyStatus { get; }

	/// <summary>
	/// Gets the current test case, if the engine is currently in the process of running a
	/// test case; will return <c>null</c> outside of the context of a test case.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="IXunitTestCase"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(Test))]
	ITestCase? TestCase { get; }

	/// <summary>
	/// Gets the current test engine status for the test case. Will only be available when <see cref="TestCase"/>
	/// is not <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestCase))]
	TestEngineStatus? TestCaseStatus { get; }

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
	ITestClass? TestClass { get; }

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
	object? TestClassInstance { get; }

	/// <summary>
	/// Gets the current test engine status for the test class. Will only be available when <see cref="TestClass"/>
	/// is not <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestClass))]
	TestEngineStatus? TestClassStatus { get; }

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
	ITestCollection? TestCollection { get; }

	/// <summary>
	/// Gets the current test engine status for the test collection. Will only be available when
	/// <see cref="TestCollection"/> is not <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestCollection))]
	TestEngineStatus? TestCollectionStatus { get; }

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
	ITestMethod? TestMethod { get; }

	/// <summary>
	/// Gets the current test engine status for the test method. Will only be available when <see cref="TestMethod"/>
	/// is not <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(TestMethod))]
	TestEngineStatus? TestMethodStatus { get; }

	/// <summary>
	/// Gets the output helper, which can be used to add output to the test. Will only be
	/// available when <see cref="Test"/> is not <c>null</c>. Note that the value may still
	/// be <c>null</c> when <see cref="Test"/> is not <c>null</c>, if the test framework
	/// implementation does not provide output helper support.
	/// </summary>
	ITestOutputHelper? TestOutputHelper { get; }

	/// <summary>
	/// Gets the current state of the test. Will only be available after the test has finished running.
	/// </summary>
	TestResultState? TestState { get; }

	/// <summary>
	/// Gets the current test engine status for the test. Will only be available when <see cref="Test"/>
	/// is not <c>null</c>.
	/// </summary>
	[NotNullIfNotNull(nameof(Test))]
	TestEngineStatus? TestStatus { get; }

	/// <summary>
	/// Gets the set of warnings associated with the current test. Will only be available when <see cref="Test"/>
	/// is not <c>null</c>; will also return <c>null</c> if there have been no warnings issued.
	/// </summary>
	IReadOnlyList<string>? Warnings { get; }

	/// <summary>
	/// Adds an attachment that is a string value.
	/// </summary>
	/// <param name="name">The name of the attachment</param>
	/// <param name="value">The value of the attachment</param>
	void AddAttachment(
		string name,
		string value);

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
	void AddAttachment(
		string name,
		byte[] value,
		string mediaType = "application/octet-stream");

	/// <summary>
	/// Adds a warning to the test result.
	/// </summary>
	/// <param name="message">The warning message to be reported</param>
	void AddWarning(string message);

	/// <summary>
	/// Attempt to cancel the currently executing test, if one is executing. This will
	/// signal the <see cref="CancellationToken"/> for cancellation.
	/// </summary>
	void CancelCurrentTest();

	/// <summary>
	/// Gets a fixture that was attached to the test class. Will return <c>null</c> if there is
	/// no exact match for the requested fixture type, or if there is no test class (that is,
	/// if <see cref="TestClass"/> returns <c>null</c>).
	/// </summary>
	/// <remarks>
	/// This may be a fixture attached via <see cref="IClassFixture{TFixture}"/>, <see cref="ICollectionFixture{TFixture}"/>,
	/// or <see cref="AssemblyFixtureAttribute"/>.
	/// </remarks>
	/// <param name="fixtureType">The exact type of the fixture</param>
	/// <returns>The fixture, if available; <c>null</c>, otherwise</returns>
	ValueTask<object?> GetFixture(Type fixtureType);

	/// <summary>
	/// Sends a diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="message">The message to send</param>
	void SendDiagnosticMessage(string message);

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	void SendDiagnosticMessage(string format, object? arg0);

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	void SendDiagnosticMessage(string format, object? arg0, object? arg1);

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	/// <param name="arg2">The value to replace {2} in the format string.</param>
	void SendDiagnosticMessage(string format, object? arg0, object? arg1, object? arg2);

	/// <summary>
	/// Sends a formatted diagnostic message. Will only be visible if the end user has enabled diagnostic messages.
	/// See https://xunit.net/docs/configuration-files for configuration information.
	/// </summary>
	/// <param name="format">A composite format string.</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	void SendDiagnosticMessage(string format, params object?[] args);
}
