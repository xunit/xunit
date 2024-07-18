using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A simple implementation of <see cref="IXunitTestCase"/> that can be used to report an error
/// rather than running a test.
/// </summary>
public class ExecutionErrorTestCase : XunitTestCase
{
	string? errorMessage;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public ExecutionErrorTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="ExecutionErrorTestCase"/> class.
	/// </summary>
	/// <param name="testMethod">The test method.</param>
	/// <param name="testCaseDisplayName">The display name for the test case.</param>
	/// <param name="uniqueID">The unique ID for the test case.</param>
	/// <param name="errorMessage">The error message to report for the test.</param>
	public ExecutionErrorTestCase(
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		string errorMessage) :
			base(testMethod, testCaseDisplayName, uniqueID, @explicit: false) =>
				this.errorMessage = Guard.ArgumentNotNull(errorMessage);

	/// <summary>
	/// Gets the error message that will be displayed when the test is run.
	/// </summary>
	public string ErrorMessage =>
		this.ValidateNullablePropertyValue(errorMessage, nameof(ErrorMessage));

	/// <inheritdoc/>
	protected override void Deserialize(IXunitSerializationInfo info)
	{
		base.Deserialize(info);

		errorMessage = Guard.NotNull("Could not retrieve ErrorMessage from serialization", info.GetValue<string>("em"));
	}

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunAsync(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) =>
			ExecutionErrorTestCaseRunner.Instance.RunAsync(this, messageBus, aggregator.Clone(), cancellationTokenSource);

	/// <inheritdoc/>
	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("em", ErrorMessage);
	}
}
