using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Sdk;

public class CulturedXunitTheoryTestCaseRunner : XunitDelayEnumeratedTheoryTestCaseRunner
{
	readonly string culture;
	CultureInfo? originalCulture;
	CultureInfo? originalUICulture;

	public CulturedXunitTheoryTestCaseRunner(string culture) =>
		this.culture = culture;

	protected override ValueTask AfterTestCaseStartingAsync(XunitDelayEnumeratedTheoryTestCaseRunnerContext ctxt)
	{
		try
		{
			originalCulture = CultureInfo.CurrentCulture;
			originalUICulture = CultureInfo.CurrentUICulture;

			var cultureInfo = new CultureInfo(culture, useUserOverride: false);
			CultureInfo.CurrentCulture = cultureInfo;
			CultureInfo.CurrentUICulture = cultureInfo;
		}
		catch (Exception ex)
		{
			ctxt.Aggregator.Add(ex);
			return default;
		}

		return base.AfterTestCaseStartingAsync(ctxt);
	}

	protected override ValueTask BeforeTestCaseFinishedAsync(XunitDelayEnumeratedTheoryTestCaseRunnerContext ctxt)
	{
		if (originalUICulture is not null)
			CultureInfo.CurrentUICulture = originalUICulture;
		if (originalCulture is not null)
			CultureInfo.CurrentCulture = originalCulture;

		return base.BeforeTestCaseFinishedAsync(ctxt);
	}
}
