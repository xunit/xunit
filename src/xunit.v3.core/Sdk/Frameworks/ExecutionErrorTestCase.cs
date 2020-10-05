using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;

namespace Xunit.Sdk
{
	/// <summary>
	/// A simple implementation of <see cref="IXunitTestCase"/> that can be used to report an error
	/// rather than running a test.
	/// </summary>
	public class ExecutionErrorTestCase : XunitTestCase
	{
		string? errorMessage;

		/// <summary/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public ExecutionErrorTestCase()
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExecutionErrorTestCase"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/> messages.</param>
		/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
		/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
		/// <param name="testMethod">The test method.</param>
		/// <param name="errorMessage">The error message to report for the test.</param>
		public ExecutionErrorTestCase(
			IMessageSink diagnosticMessageSink,
			TestMethodDisplay defaultMethodDisplay,
			TestMethodDisplayOptions defaultMethodDisplayOptions,
			ITestMethod testMethod,
			string errorMessage)
				: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
		{
			ErrorMessage = Guard.ArgumentNotNull(nameof(errorMessage), errorMessage);
		}

		/// <summary>
		/// Gets the error message that will be display when the test is run.
		/// </summary>
		public string ErrorMessage
		{
			get => errorMessage ?? throw new InvalidOperationException($"Attempted to get ErrorMessage on an uninitialized '{GetType().FullName}' object");
			private set => errorMessage = Guard.ArgumentNotNull(nameof(ErrorMessage), value);
		}

		/// <inheritdoc/>
		public override Task<RunSummary> RunAsync(
			IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			object?[] constructorArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) =>
				new ExecutionErrorTestCaseRunner(this, messageBus, aggregator, cancellationTokenSource).RunAsync();

		/// <inheritdoc/>
		public override void Serialize(IXunitSerializationInfo info)
		{
			Guard.ArgumentNotNull(nameof(info), info);

			base.Serialize(info);

			info.AddValue("ErrorMessage", ErrorMessage);
		}

		/// <inheritdoc/>
		public override void Deserialize(IXunitSerializationInfo info)
		{
			Guard.ArgumentNotNull(nameof(info), info);

			base.Deserialize(info);

			ErrorMessage = info.GetValue<string>("ErrorMessage");
		}
	}
}
