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
	/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
	/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
	/// <param name="testMethod">The test method.</param>
	/// <param name="errorMessage">The error message to report for the test.</param>
	public ExecutionErrorTestCase(
		TestMethodDisplay defaultMethodDisplay,
		TestMethodDisplayOptions defaultMethodDisplayOptions,
		_ITestMethod testMethod,
		string errorMessage)
			: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
	{
		this.errorMessage = Guard.ArgumentNotNull(errorMessage);
	}

	/// <summary>
	/// Gets the error message that will be display when the test is run.
	/// </summary>
	public string ErrorMessage =>
		errorMessage ?? throw new InvalidOperationException($"Attempted to get {nameof(ErrorMessage)} on an uninitialized '{GetType().FullName}' object");

	/// <inheritdoc/>
	protected override void Deserialize(IXunitSerializationInfo info)
	{
		base.Deserialize(info);

		errorMessage = Guard.NotNull("Could not retrieve ErrorMessage from serialization", info.GetValue<string>("em"));
	}

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunAsync(
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) =>
			ExecutionErrorTestCaseRunner.Instance.RunAsync(this, messageBus, aggregator, cancellationTokenSource);

	/// <inheritdoc/>
	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("em", ErrorMessage);
	}
}
