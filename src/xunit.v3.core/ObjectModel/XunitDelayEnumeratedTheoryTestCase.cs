using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test case which runs multiple tests for theory data, either because the
/// data was not enumerable or because the data was not serializable.
/// </summary>
public class XunitDelayEnumeratedTheoryTestCase : XunitTestCase, IXunitDelayEnumeratedTestCase
{
	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitDelayEnumeratedTheoryTestCase()
	{ }

	/// <summary>
	/// Gets a flag which indicates whether a theory without data is skipped rather than failed.
	/// </summary>
	public bool SkipTestWithoutData { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitDelayEnumeratedTheoryTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="testCaseDisplayName">The display name for the test case.</param>
	/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
	/// <param name="explicit">Indicates whether the test case was marked as explicit.</param>
	/// <param name="skipTestWithoutData">Set to <c>true</c> to skip if the test has no data, rather than fail.</param>
	/// <param name="skipReason">The value from <see cref="IFactAttribute.Skip"/></param>
	/// <param name="skipType">The value from <see cref="IFactAttribute.SkipType"/> </param>
	/// <param name="skipUnless">The value from <see cref="IFactAttribute.SkipUnless"/></param>
	/// <param name="skipWhen">The value from <see cref="IFactAttribute.SkipWhen"/></param>
	/// <param name="traits">The optional traits list.</param>
	/// <param name="sourceFilePath">The optional source file in where this test case originated.</param>
	/// <param name="sourceLineNumber">The optional source line number where this test case originated.</param>
	/// <param name="timeout">The optional timeout for the test case (in milliseconds).</param>
	public XunitDelayEnumeratedTheoryTestCase(
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		bool @explicit,
		bool skipTestWithoutData,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		Dictionary<string, HashSet<string>>? traits = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int? timeout = null) :
			base(
				testMethod,
				testCaseDisplayName,
				uniqueID,
				@explicit,
				skipReason,
				skipType,
				skipUnless,
				skipWhen,
				traits,
				testMethodArguments: null,
				sourceFilePath,
				sourceLineNumber,
				timeout
			) =>
				SkipTestWithoutData = skipTestWithoutData;

	/// <inheritdoc/>
	protected override void Deserialize(IXunitSerializationInfo info)
	{
		base.Deserialize(info);

		SkipTestWithoutData = info.GetValue<bool>("swd");
	}

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunAsync(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) =>
			XunitDelayEnumeratedTheoryTestCaseRunner.Instance.RunAsync(
				this,
				messageBus,
				aggregator.Clone(),
				cancellationTokenSource,
				TestCaseDisplayName,
				SkipReason,
				explicitOption,
				constructorArguments
			);

	/// <inheritdoc/>
	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("swd", SkipTestWithoutData);
	}
}
