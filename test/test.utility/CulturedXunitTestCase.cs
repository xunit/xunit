using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestUtility
{
    public class CulturedXunitTestCase : XunitTestCase
    {
        private string culture;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public CulturedXunitTestCase() { }

        public CulturedXunitTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod, string culture)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod)
        {
            Initialize(culture);
        }

        void Initialize(string culture)
        {
            this.culture = culture;

            Traits.Add("Culture", culture);

            DisplayName += $" [{culture}]";
        }

        protected override string GetUniqueID()
            => $"{base.GetUniqueID()} [{culture}]";

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            Initialize(data.GetValue<string>("Culture"));
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("Culture", culture);
        }

        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                        IMessageBus messageBus,
                                                        object[] constructorArguments,
                                                        ExceptionAggregator aggregator,
                                                        CancellationTokenSource cancellationTokenSource)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                return await base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }
    }
}
