using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
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
        private readonly string culture;

        public CulturedXunitTestCase(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, string culture)
            : base(testCollection, assembly, type, method, factAttribute)
        {
            this.culture = culture;

            Traits.Add("Culture", culture);
        }

        protected CulturedXunitTestCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            culture = info.GetString("Culture");

            Traits.Add("Culture", culture);
        }

        public override string DisplayName
        {
            get { return String.Format("{0} [{1}]", base.DisplayName, culture); }
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Culture", culture);

            base.GetObjectData(info, context);
        }

        protected override async Task RunTestsOnMethodAsync(IMessageBus messageBus,
                                                            Type classUnderTest,
                                                            object[] constructorArguments,
                                                            MethodInfo methodUnderTest,
                                                            List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                            ExceptionAggregator aggregator,
                                                            CancellationTokenSource cancellationTokenSource)
        {
            var executionTime = 0M;
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                executionTime =
                    await RunTestWithArgumentsAsync(messageBus,
                                                    classUnderTest,
                                                    constructorArguments,
                                                    methodUnderTest,
                                                    null,
                                                    DisplayName,
                                                    beforeAfterAttributes,
                                                    aggregator,
                                                    cancellationTokenSource);
            }
            catch (Exception ex)
            {
                if (!messageBus.QueueMessage(new Xunit.Sdk.TestStarting(this, DisplayName)))
                    cancellationTokenSource.Cancel();
                else
                {
                    if (!messageBus.QueueMessage(new Xunit.Sdk.TestFailed(this, DisplayName, executionTime, null, ex.Unwrap())))
                        cancellationTokenSource.Cancel();
                }

                if (!messageBus.QueueMessage(new Xunit.Sdk.TestFinished(this, DisplayName, executionTime, null)))
                    cancellationTokenSource.Cancel();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }
    }
}
