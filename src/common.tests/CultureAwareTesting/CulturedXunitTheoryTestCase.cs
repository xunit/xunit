using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

public class CulturedXunitTheoryTestCase : XunitDelayEnumeratedTheoryTestCase
{
	string? culture;
	CultureInfo? originalCulture;
	CultureInfo? originalUICulture;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public CulturedXunitTheoryTestCase()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="CulturedXunitTheoryTestCase"/> class.
	/// </summary>
	/// <param name="culture">The culture to run the theory under.</param>
	/// <param name="testMethod">The test method this test case belongs to.</param>
	/// <param name="testCaseDisplayName">The display name for the test case.</param>
	/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
	/// <param name="explicit">Indicates whether the test case was marked as explicit.</param>
	/// <param name="skipTestWithoutData">Set to <c>true</c> to skip if the test has no data, rather than fail.</param>
	/// <param name="skipExceptions">The value from <see cref="IFactAttribute.SkipExceptions"/>.</param>
	/// <param name="skipReason">The value from <see cref="IFactAttribute.Skip"/></param>
	/// <param name="skipType">The value from <see cref="IFactAttribute.SkipType"/> </param>
	/// <param name="skipUnless">The value from <see cref="IFactAttribute.SkipUnless"/></param>
	/// <param name="skipWhen">The value from <see cref="IFactAttribute.SkipWhen"/></param>
	/// <param name="traits">The optional traits list.</param>
	/// <param name="sourceFilePath">The optional source file in where this test case originated.</param>
	/// <param name="sourceLineNumber">The optional source line number where this test case originated.</param>
	/// <param name="timeout">The optional timeout for the test case (in milliseconds).</param>
	public CulturedXunitTheoryTestCase(
		string culture,
		IXunitTestMethod testMethod,
		string testCaseDisplayName,
		string uniqueID,
		bool @explicit,
		bool skipTestWithoutData,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		Dictionary<string, HashSet<string>>? traits = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int? timeout = null)
			: base(testMethod, $"{testCaseDisplayName}[{culture}]", $"{uniqueID}[{culture}]", @explicit, skipTestWithoutData, skipExceptions, skipReason, skipType, skipUnless, skipWhen, traits, sourceFilePath, sourceLineNumber, timeout)
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
