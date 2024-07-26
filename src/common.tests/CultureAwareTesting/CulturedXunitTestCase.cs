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
		string culture,
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		bool @explicit,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		Dictionary<string, HashSet<string>>? traits = null,
		object?[]? testMethodArguments = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int? timeout = null) :
			base(
				testMethod,
				$"{testCaseDisplayName}[{culture}]",
				$"{uniqueID}[{culture}]",
				@explicit,
				skipReason,
				skipType,
				skipUnless,
				skipWhen,
				traits,
				testMethodArguments,
				sourceFilePath,
				sourceLineNumber,
				timeout
			)
	{
		this.culture = Guard.ArgumentNotNull(culture);

		Traits.Add("Culture", Culture);
	}

	public string Culture =>
		this.ValidateNullablePropertyValue(culture, nameof(Culture));

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
