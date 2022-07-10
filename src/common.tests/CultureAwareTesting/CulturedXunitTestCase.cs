using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

public class CulturedXunitTestCase : XunitTestCase
{
	string? culture;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public CulturedXunitTestCase()
	{ }

	public CulturedXunitTestCase(
		TestMethodDisplay defaultMethodDisplay,
		TestMethodDisplayOptions defaultMethodDisplayOptions,
		_ITestMethod testMethod,
		string culture,
		object?[]? testMethodArguments = null,
		Dictionary<string, List<string>>? traits = null,
		string? displayName = null)
			: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments, null, null, traits, null, null, displayName)
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

	public override async ValueTask<RunSummary> RunAsync(
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		var originalCulture = CultureInfo.CurrentCulture;
		var originalUICulture = CultureInfo.CurrentUICulture;

		try
		{
			var cultureInfo = new CultureInfo(Culture, useUserOverride: false);
			CultureInfo.CurrentCulture = cultureInfo;
			CultureInfo.CurrentUICulture = cultureInfo;

			return await base.RunAsync(explicitOption, messageBus, constructorArguments, aggregator, cancellationTokenSource);
		}
		finally
		{
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUICulture;
		}
	}

	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("cul", Culture);
	}
}
