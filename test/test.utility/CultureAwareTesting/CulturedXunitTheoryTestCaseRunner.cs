#if NET452

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestUtility
{
    public class CulturedXunitTheoryTestCaseRunner : XunitTheoryTestCaseRunner
    {
        string culture;
        CultureInfo originalCulture;
        CultureInfo originalUICulture;

        public CulturedXunitTheoryTestCaseRunner(CulturedXunitTheoryTestCase culturedXunitTheoryTestCase,
                                                 string displayName,
                                                 string skipReason,
                                                 object[] constructorArguments,
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
                originalCulture = Thread.CurrentThread.CurrentCulture;
                originalUICulture = Thread.CurrentThread.CurrentUICulture;

                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
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
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            if (originalUICulture != null)
                Thread.CurrentThread.CurrentCulture = originalCulture;

            return base.BeforeTestCaseFinishedAsync();
        }
    }
}

#endif