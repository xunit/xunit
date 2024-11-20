using System.Globalization;
using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Sdk;

public class CulturedXunitTheoryTestCaseRunner(string culture) :
	XunitDelayEnumeratedTheoryTestCaseRunner
{
	CultureInfo? originalCulture;
	CultureInfo? originalUICulture;

	protected override ValueTask<bool> OnTestCaseFinished(
		XunitDelayEnumeratedTestCaseRunnerContext<IXunitDelayEnumeratedTestCase> ctxt,
		RunSummary summary)
	{
		ctxt.Aggregator.Run(() =>
		{
			if (originalUICulture is not null)
				CultureInfo.CurrentUICulture = originalUICulture;
			if (originalCulture is not null)
				CultureInfo.CurrentCulture = originalCulture;
		});

		return base.OnTestCaseFinished(ctxt, summary);
	}

	protected override async ValueTask<bool> OnTestCaseStarting(XunitDelayEnumeratedTestCaseRunnerContext<IXunitDelayEnumeratedTestCase> ctxt)
	{
		var result = await base.OnTestCaseStarting(ctxt);

		ctxt.Aggregator.Run(() =>
		{
			originalCulture = CultureInfo.CurrentCulture;
			originalUICulture = CultureInfo.CurrentUICulture;

			var cultureInfo = new CultureInfo(culture, useUserOverride: false);
			CultureInfo.CurrentCulture = cultureInfo;
			CultureInfo.CurrentUICulture = cultureInfo;
		});

		return result;
	}
}
