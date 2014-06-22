using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestUtility
{
    [Serializable]
    public class CulturedXunitTestCase : XunitTestCase
    {
        private string culture;

        public CulturedXunitTestCase(ITestMethod testMethod, string culture)
            : base(testMethod)
        {
            Initialize(culture);
        }

        protected CulturedXunitTestCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            culture = info.GetString("Culture");

            Traits.Add("Culture", culture);
        }

        void Initialize(string culture)
        {
            this.culture = culture;

            Traits.Add("Culture", culture);

            DisplayName += String.Format(" [{0}]", culture);
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Culture", culture);

            base.GetObjectData(info, context);
        }

        public override async Task<RunSummary> RunAsync(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                return await base.RunAsync(messageBus, constructorArguments, aggregator, cancellationTokenSource);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }
    }
}
