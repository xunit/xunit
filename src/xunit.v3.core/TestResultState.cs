using System;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Represents information about the current state of a test after it has run.
/// </summary>
public class TestResultState
{
	TestResultState()
	{ }

	/// <summary>
	/// Gets the message(s) of the exception(s). This value is only available
	/// when <see cref="Result"/> is <see cref="TestResult.Failed"/>.
	/// </summary>
	public string[]? ExceptionMessages { get; private set; }

	/// <summary>
	/// Gets the parent exception index(es) for the exception(s); a -1 indicates
	/// that the exception in question has no parent. This value is only available
	/// when <see cref="Result"/> is <see cref="TestResult.Failed"/>.
	/// </summary>
	public int[]? ExceptionParentIndices { get; private set; }

	/// <summary>
	/// Gets the stack trace(s) of the exception(s). This value is only available
	/// when <see cref="Result"/> is <see cref="TestResult.Failed"/>.
	/// </summary>
	public string?[]? ExceptionStackTraces { get; private set; }

	/// <summary>
	/// Gets the fully-qualified type name(s) of the exception(s). This value is
	/// only available when <see cref="Result"/> is <see cref="TestResult.Failed"/>.
	/// </summary>
	public string?[]? ExceptionTypes { get; private set; }

	/// <summary>
	/// Gets the amount of time the test ran, in seconds. The value may be <c>0</c> if no
	/// test code was run (for example, a statically skipped test). Note that the value may
	/// be a partial value because of further timing being done while cleaning up.
	/// </summary>
	public decimal? ExecutionTime { get; private set; }

	/// <summary>
	/// Gets a value which indicates what the cause of the test failure was. This value is only
	/// available when <see cref="Result"/> is <see cref="TestResult.Failed"/>.
	/// </summary>
	public FailureCause? FailureCause { get; private set; }

	/// <summary>
	/// Returns the result from the test run.
	/// </summary>
	public TestResult Result { get; private set; }

	/// <summary>
	/// Create a not run test result.
	/// </summary>
	/// <param name="executionTime">The optional execution time</param>
	public static TestResultState ForNotRun(decimal? executionTime = null) =>
		new() { ExecutionTime = executionTime ?? 0m, Result = TestResult.NotRun };

	/// <summary>
	/// Create a passing test result.
	/// </summary>
	/// <param name="executionTime">The optional execution time</param>
	public static TestResultState ForPassed(decimal? executionTime = null) =>
		new() { ExecutionTime = executionTime ?? 0m, Result = TestResult.Passed };

	/// <summary>
	/// Create a skipped test result.
	/// </summary>
	/// <param name="executionTime">The optional execution time</param>
	public static TestResultState ForSkipped(decimal? executionTime = null) =>
		new() { ExecutionTime = executionTime ?? 0m, Result = TestResult.Skipped };

	/// <summary>
	/// Creates an instance based on the presence or absence of an exception. If the exception
	/// is <c>null</c>, then it will be for <see cref="TestResult.Passed"/>; otherwise, it will
	/// be for <see cref="TestResult.Failed"/>;
	/// </summary>
	/// <param name="executionTime">The time spent executing the test</param>
	/// <param name="exception">The exception, if the test failed</param>
	public static TestResultState FromException(
		decimal executionTime,
		Exception? exception)
	{
		var result = new TestResultState { ExecutionTime = executionTime };

		if (exception is null)
			result.Result = TestResult.Passed;
		else
		{
			var errorMetadata = ExceptionUtility.ExtractMetadata(exception);

			result.ExceptionMessages = errorMetadata.Messages;
			result.ExceptionParentIndices = errorMetadata.ExceptionParentIndices;
			result.ExceptionStackTraces = errorMetadata.StackTraces;
			result.ExceptionTypes = errorMetadata.ExceptionTypes;
			result.FailureCause = errorMetadata.Cause;
			result.Result = TestResult.Failed;
		}

		return result;
	}

	/// <summary>
	/// Creates an instance based on inspecting the type identity of the
	/// <paramref name="testResult"/> instance.
	/// </summary>
	/// <param name="testResult">The test result</param>
	public static TestResultState FromTestResult(ITestResultMessage testResult)
	{
		Guard.ArgumentNotNull(testResult);

		var result = new TestResultState { ExecutionTime = testResult.ExecutionTime };

		if (testResult is ITestPassed)
			result.Result = TestResult.Passed;
		else if (testResult is ITestSkipped)
			result.Result = TestResult.Skipped;
		else if (testResult is ITestNotRun)
			result.Result = TestResult.NotRun;
		else if (testResult is ITestFailed testFailed)
		{
			result.ExceptionMessages = testFailed.Messages;
			result.ExceptionParentIndices = testFailed.ExceptionParentIndices;
			result.ExceptionStackTraces = testFailed.StackTraces;
			result.ExceptionTypes = testFailed.ExceptionTypes;
			result.FailureCause = testFailed.Cause;
			result.Result = TestResult.Failed;
		}
		else
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown type: '{0}'", testResult.GetType().SafeName()), nameof(testResult));

		return result;
	}
}
