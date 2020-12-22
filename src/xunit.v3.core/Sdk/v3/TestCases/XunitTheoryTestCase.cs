using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Runner.v2;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// Represents a test case which runs multiple tests for theory data, either because the
	/// data was not enumerable or because the data was not serializable.
	/// </summary>
	public class XunitTheoryTestCase : XunitTestCase
	{
		/// <summary/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public XunitTheoryTestCase()
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTheoryTestCase"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
		/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
		/// <param name="testMethod">The method under test.</param>
		/// <param name="uniqueID">The unique ID for the test case (only used to override default behavior in testing scenarios)</param>
		public XunitTheoryTestCase(
			_IMessageSink diagnosticMessageSink,
			TestMethodDisplay defaultMethodDisplay,
			TestMethodDisplayOptions defaultMethodDisplayOptions,
			_ITestMethod testMethod,
			string? uniqueID = null)
				: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, uniqueID: uniqueID)
		{ }

		/// <inheritdoc/>
		public override Task<RunSummary> RunAsync(
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			object?[] constructorArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) =>
				new XunitTheoryTestCaseRunner(
					this,
					DisplayName,
					SkipReason,
					constructorArguments,
					diagnosticMessageSink,
					messageBus,
					aggregator,
					cancellationTokenSource
				).RunAsync();
	}
}
