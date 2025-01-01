using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

public class CulturedXunitTestCase : XunitTestCase
{
	string? culture;
	CultureInfo? originalCulture;
	CultureInfo? originalUICulture;

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
		Type[]? skipExceptions = null,
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
				skipExceptions,
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

	public override void PostInvoke()
	{
		if (originalCulture is not null)
			CultureInfo.CurrentCulture = originalCulture;
		if (originalUICulture is not null)
			CultureInfo.CurrentUICulture = originalUICulture;

		base.PostInvoke();
	}

	public override void PreInvoke()
	{
		base.PreInvoke();

		originalCulture = CultureInfo.CurrentCulture;
		originalUICulture = CultureInfo.CurrentUICulture;

		var cultureInfo = new CultureInfo(Culture, useUserOverride: false);
		CultureInfo.CurrentCulture = cultureInfo;
		CultureInfo.CurrentUICulture = cultureInfo;
	}

	protected override void Serialize(IXunitSerializationInfo info)
	{
		base.Serialize(info);

		info.AddValue("cul", Culture);
	}
}
