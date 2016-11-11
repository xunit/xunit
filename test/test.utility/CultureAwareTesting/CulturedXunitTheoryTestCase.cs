using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TestUtility
{
    public class CulturedXunitTheoryTestCase : XunitTheoryTestCase
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public CulturedXunitTheoryTestCase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CulturedXunitTheoryTestCase"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
        /// <param name="testMethod">The method under test.</param>
        public CulturedXunitTheoryTestCase(IMessageSink diagnosticMessageSink,
                                           TestMethodDisplay defaultMethodDisplay,
                                           TestMethodDisplayOptions defaultMethodDisplayOptions,
                                           ITestMethod testMethod,
                                           string culture)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
        {
            Initialize(culture);
        }

        public string Culture { get; private set; }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            Initialize(data.GetValue<string>("Culture"));
        }

        protected override string GetUniqueID()
            => $"{base.GetUniqueID()}[{Culture}]";

        void Initialize(string culture)
        {
            Culture = culture;

            Traits.Add("Culture", culture);

            DisplayName += $"[{culture}]";
        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                  IMessageBus messageBus,
                                                  object[] constructorArguments,
                                                  ExceptionAggregator aggregator,
                                                  CancellationTokenSource cancellationTokenSource)
            => new CulturedXunitTheoryTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource).RunAsync();

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("Culture", Culture);
        }
    }
}
