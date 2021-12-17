using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// Represents a test case which runs multiple tests for theory data, either because the
	/// data was not enumerable or because the data was not serializable.
	/// </summary>
	[Serializable]
	public class XunitDelayEnumeratedTheoryTestCase : XunitTestCase
	{
		/// <inheritdoc/>
		protected XunitDelayEnumeratedTheoryTestCase(
			SerializationInfo info,
			StreamingContext context) :
				base(info, context)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitDelayEnumeratedTheoryTestCase"/> class.
		/// </summary>
		/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
		/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
		/// <param name="testMethod">The method under test.</param>
		/// <param name="skipReason">The optional reason for skipping the test; if not provided, will be read from the <see cref="FactAttribute"/>.</param>
		/// <param name="traits">The optional traits list; if not provided, will be read from trait attributes.</param>
		/// <param name="timeout">The optional timeout (in milliseconds); if not provided, will be read from the <see cref="FactAttribute"/>.</param>
		/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
		public XunitDelayEnumeratedTheoryTestCase(
			TestMethodDisplay defaultMethodDisplay,
			TestMethodDisplayOptions defaultMethodDisplayOptions,
			_ITestMethod testMethod,
			string? skipReason = null,
			Dictionary<string, List<string>>? traits = null,
			int? timeout = null,
			string? uniqueID = null)
				: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, null, skipReason, traits, timeout, uniqueID, null)
		{ }

		/// <inheritdoc/>
		public override ValueTask<RunSummary> RunAsync(
			IMessageBus messageBus,
			object?[] constructorArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) =>
				new XunitDelayEnumeratedTheoryTestCaseRunner(
					this,
					TestCaseDisplayName,
					SkipReason,
					constructorArguments,
					messageBus,
					aggregator,
					cancellationTokenSource
				).RunAsync();
	}
}
