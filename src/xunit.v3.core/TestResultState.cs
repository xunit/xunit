using System;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

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
	/// Gets an immutable instance to indicates a test has a result.
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

	/// <summary/>
	public static TestResultState FromTestResult(_TestResultMessage testResult)
	{
		Guard.ArgumentNotNull(testResult);

		var result = new TestResultState { ExecutionTime = testResult.ExecutionTime };

		if (testResult is _TestPassed)
			result.Result = TestResult.Passed;
		else if (testResult is _TestSkipped)
			result.Result = TestResult.Skipped;
		else if (testResult is _TestNotRun)
			result.Result = TestResult.NotRun;
		else if (testResult is _TestFailed testFailed)
		{
			result.ExceptionMessages = testFailed.Messages;
			result.ExceptionParentIndices = testFailed.ExceptionParentIndices;
			result.ExceptionStackTraces = testFailed.StackTraces;
			result.ExceptionTypes = testFailed.ExceptionTypes;
			result.FailureCause = testFailed.Cause;
			result.Result = TestResult.Failed;
		}
		else
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown type: '{0}'", testResult.GetType().FullName), nameof(testResult));

		return result;
	}
}
