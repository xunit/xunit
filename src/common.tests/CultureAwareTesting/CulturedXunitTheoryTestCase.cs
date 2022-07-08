using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

public class CulturedXunitTheoryTestCase : XunitDelayEnumeratedTheoryTestCase
{
	string? culture;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public CulturedXunitTheoryTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="CulturedXunitTheoryTestCase"/> class.
	/// </summary>
	/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
	/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
	/// <param name="testMethod">The method under test.</param>
	public CulturedXunitTheoryTestCase(
		TestMethodDisplay defaultMethodDisplay,
		TestMethodDisplayOptions defaultMethodDisplayOptions,
		_ITestMethod testMethod,
		string culture)
			: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
	{
		this.culture = Guard.ArgumentNotNull(culture);

		Traits.Add("Culture", Culture);

		var cultureDisplay = $"[{Culture}]";
		TestCaseDisplayName += cultureDisplay;
		UniqueID += cultureDisplay;
	}

	public string Culture =>
		culture ?? throw new InvalidOperationException($"Attempted to get {nameof(Culture)} on an uninitialized '{GetType().FullName}' object");

	protected override void Deserialize(IXunitSerializationInfo info)
	{
		base.Deserialize(info);

		culture = Guard.NotNull("Could not retrieve Culture from serialization", info.GetValue<string>("cul"));
	}

	public override ValueTask<RunSummary> RunAsync(
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) =>
			new CulturedXunitTheoryTestCaseRunner(Culture).RunAsync(
				this,
				messageBus,
				aggregator,
				cancellationTokenSource,
				TestCaseDisplayName,
				SkipReason,
				constructorArguments,
				TestMethodArguments
			);
	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("cul", Culture);
	}
}
