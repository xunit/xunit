using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
	public class CulturedXunitTheoryTestCaseRunner : XunitTheoryTestCaseRunner
	{
		readonly string culture;
		CultureInfo? originalCulture;
		CultureInfo? originalUICulture;

		public CulturedXunitTheoryTestCaseRunner(
			CulturedXunitTheoryTestCase culturedXunitTheoryTestCase,
			string displayName,
			string? skipReason,
			object?[] constructorArguments,
			IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
				: base(culturedXunitTheoryTestCase, displayName, skipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource)
		{
			culture = culturedXunitTheoryTestCase.Culture;
		}

		protected override Task AfterTestCaseStartingAsync()
		{
			try
			{
				originalCulture = CultureInfo.CurrentCulture;
				originalUICulture = CultureInfo.CurrentUICulture;

				var cultureInfo = new CultureInfo(culture);
				CultureInfo.CurrentCulture = cultureInfo;
				CultureInfo.CurrentUICulture = cultureInfo;
			}
			catch (Exception ex)
			{
				Aggregator.Add(ex);
				return Task.FromResult(0);
			}

			return base.AfterTestCaseStartingAsync();
		}

		protected override Task BeforeTestCaseFinishedAsync()
		{
			if (originalUICulture != null)
				CultureInfo.CurrentUICulture = originalUICulture;
			if (originalCulture != null)
				CultureInfo.CurrentCulture = originalCulture;

			return base.BeforeTestCaseFinishedAsync();
		}
	}
}
