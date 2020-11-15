using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;
using Xunit.v3;

namespace Xunit.Sdk
{
	public class CulturedXunitTheoryTestCase : XunitTheoryTestCase
	{
		/// <summary/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public CulturedXunitTheoryTestCase() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="CulturedXunitTheoryTestCase"/> class.
		/// </summary>
		/// <param name="testAssemblyUniqueID">The test assembly unique ID.</param>
		/// <param name="testCollectionUniqueID">The test collection unique ID.</param>
		/// <param name="testClassUniqueID">The test class unique ID.</param>
		/// <param name="testMethodUniqueID">The test method unique ID.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/> messages.</param>
		/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
		/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
		/// <param name="testMethod">The method under test.</param>
		public CulturedXunitTheoryTestCase(
			string testAssemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID,
			string? testMethodUniqueID,
			_IMessageSink diagnosticMessageSink,
			TestMethodDisplay defaultMethodDisplay,
			TestMethodDisplayOptions defaultMethodDisplayOptions,
			ITestMethod testMethod,
			string culture)
				: base(testAssemblyUniqueID, testCollectionUniqueID, testClassUniqueID, testMethodUniqueID, diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
		{
			Initialize(culture);
		}

		public string Culture { get; private set; } = "<unset>";

		public override void Deserialize(IXunitSerializationInfo data)
		{
			base.Deserialize(data);

			Initialize(data.GetValue<string>("Culture"));
		}

		protected override string GetUniqueID() => $"{base.GetUniqueID()}[{Culture}]";

		void Initialize(string culture)
		{
			Culture = culture;

			Traits.Add("Culture", culture);

			DisplayName += $"[{culture}]";
		}

		public override Task<RunSummary> RunAsync(
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			object?[] constructorArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) =>
				new CulturedXunitTheoryTestCaseRunner(
					TestAssemblyUniqueID,
					TestCollectionUniqueID,
					TestClassUniqueID,
					TestMethodUniqueID,
					this,
					DisplayName,
					SkipReason,
					constructorArguments,
					diagnosticMessageSink,
					messageBus,
					aggregator,
					cancellationTokenSource
				).RunAsync();

		public override void Serialize(IXunitSerializationInfo data)
		{
			base.Serialize(data);

			data.AddValue("Culture", Culture);
		}
	}
}
