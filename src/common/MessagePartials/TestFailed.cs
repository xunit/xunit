using System;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestFailed"/>.
/// </summary>
[JsonTypeID("test-failed")]
sealed partial class TestFailed : TestResultMessage, ITestFailed
{
	/// <summary>
	/// Creates a new <see cref="ITestFailed"/> constructed from an <see cref="Exception"/> object.
	/// </summary>
	/// <param name="ex">The exception to use</param>
	/// <param name="assemblyUniqueID">The unique ID of the assembly</param>
	/// <param name="testCollectionUniqueID">The unique ID of the test collectioon</param>
	/// <param name="testClassUniqueID">The (optional) unique ID of the test class</param>
	/// <param name="testMethodUniqueID">The (optional) unique ID of the test method</param>
	/// <param name="testCaseUniqueID">The unique ID of the test case</param>
	/// <param name="testUniqueID">The unique ID of the test</param>
	/// <param name="executionTime">The execution time of the test (may be <c>null</c> if the test wasn't executed)</param>
	/// <param name="output">The (optional) output from the test</param>
	/// <param name="warnings">The (optional) warnings that were recorded during test execution</param>
	/// <param name="finishTime">The time when the test finished executing; defaults to <see cref="DateTimeOffset.UtcNow"/></param>
	public static ITestFailed FromException(
		Exception ex,
		string assemblyUniqueID,
		string testCollectionUniqueID,
		string? testClassUniqueID,
		string? testMethodUniqueID,
		string testCaseUniqueID,
		string testUniqueID,
		decimal executionTime,
		string? output,
		string[]? warnings,
		DateTimeOffset? finishTime = null)
	{
		Guard.ArgumentNotNull(ex);
		Guard.ArgumentNotNull(assemblyUniqueID);
		Guard.ArgumentNotNull(testCollectionUniqueID);
		Guard.ArgumentNotNull(testCaseUniqueID);
		Guard.ArgumentNotNull(testUniqueID);

		var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

		return new TestFailed
		{
			AssemblyUniqueID = assemblyUniqueID,
			Cause = errorMetadata.Cause,
			ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
			ExceptionTypes = errorMetadata.ExceptionTypes,
			ExecutionTime = executionTime,
			FinishTime = finishTime ?? DateTimeOffset.UtcNow,
			Messages = errorMetadata.Messages,
			Output = output ?? string.Empty,
			StackTraces = errorMetadata.StackTraces,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestUniqueID = testUniqueID,
			Warnings = warnings,
		};
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(Cause), Cause);
		serializer.SerializeIntArray(nameof(ExceptionParentIndices), ExceptionParentIndices);
		serializer.SerializeStringArray(nameof(ExceptionTypes), ExceptionTypes);
		serializer.SerializeStringArray(nameof(Messages), Messages);
		serializer.SerializeStringArray(nameof(StackTraces), StackTraces);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} types={1} messages={2}", base.ToString(), ToDisplayString(ExceptionTypes), ToDisplayString(Messages));
}
