using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="IXunitTestCase"/> for xUnit v3 that supports test methods decorated with
/// <see cref="FactAttribute"/>. Test methods decorated with derived attributes may use this as a base class
/// to build from.
/// </summary>
[DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {TestCaseDisplayName}, skip = {SkipReason} \}")]
public class XunitTestCase : TestMethodTestCase, IXunitTestCase
{
	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="testCaseDisplayName">The display name for the test case.</param>
	/// <param name="uniqueID">The unique ID for the test case.</param>
	/// <param name="explicit">Indicates whether the test case was marked as explicit.</param>
	/// <param name="skipReason">The optional reason for skipping the test.</param>
	/// <param name="traits">The optional traits list.</param>
	/// <param name="testMethodArguments">The optional arguments for the test method.</param>
	/// <param name="sourceFilePath">The optional source file in where this test case originated.</param>
	/// <param name="sourceLineNumber">The optional source line number where this test case originated.</param>
	/// <param name="timeout">The optional timeout for the test case (in milliseconds).</param>
	public XunitTestCase(
		_ITestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		bool @explicit,
		string? skipReason = null,
		Dictionary<string, List<string>>? traits = null,
		object?[]? testMethodArguments = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int? timeout = null)
			: base(testMethod, testCaseDisplayName, uniqueID, skipReason, traits, testMethodArguments, sourceFilePath, sourceLineNumber)
	{
		Explicit = @explicit;
		Timeout = timeout ?? 0;
	}

	/// <inheritdoc/>
	public bool Explicit { get; private set; }

	/// <inheritdoc/>
	public int Timeout { get; private set; }

	/// <inheritdoc/>
	protected override void Deserialize(IXunitSerializationInfo info)
	{
		base.Deserialize(info);

		Explicit = info.GetValue<bool>("ex");
		Timeout = info.GetValue<int>("to");
	}

	/// <inheritdoc/>
	public virtual ValueTask<RunSummary> RunAsync(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(constructorArguments);
		Guard.ArgumentNotNull(cancellationTokenSource);

		return XunitTestCaseRunner.Instance.RunAsync(
			this,
			messageBus,
			aggregator,
			cancellationTokenSource,
			TestCaseDisplayName,
			SkipReason,
			explicitOption,
			constructorArguments,
			TestMethodArguments
		);
	}

	/// <inheritdoc/>
	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("ex", Explicit);
		info.AddValue("to", Timeout);
	}
}
