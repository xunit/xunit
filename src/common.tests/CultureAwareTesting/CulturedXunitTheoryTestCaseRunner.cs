using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Sdk
{
	public class CulturedXunitTheoryTestCaseRunner : XunitDelayEnumeratedTheoryTestCaseRunner
	{
		readonly string culture;
		CultureInfo? originalCulture;
		CultureInfo? originalUICulture;

		public CulturedXunitTheoryTestCaseRunner(
			CulturedXunitTheoryTestCase culturedXunitTheoryTestCase,
			string displayName,
			string? skipReason,
			object?[] constructorArguments,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
				: base(culturedXunitTheoryTestCase, displayName, skipReason, constructorArguments, messageBus, aggregator, cancellationTokenSource)
		{
			culture = culturedXunitTheoryTestCase.Culture;
		}

		protected override ValueTask AfterTestCaseStartingAsync()
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
				Aggregator.Add(ex);
				return default;
			}

			return base.AfterTestCaseStartingAsync();
		}

		protected override ValueTask BeforeTestCaseFinishedAsync()
		{
			if (originalUICulture != null)
				CultureInfo.CurrentUICulture = originalUICulture;
			if (originalCulture != null)
				CultureInfo.CurrentCulture = originalCulture;

			return base.BeforeTestCaseFinishedAsync();
		}
	}
}
