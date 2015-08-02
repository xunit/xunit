using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="IXunitTestCase"/> for xUnit v2 that supports tests decorated with
    /// both <see cref="FactAttribute"/> and <see cref="TheoryAttribute"/>.
    /// </summary>
    [DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {DisplayName}, skip = {SkipReason} \}")]
    public class XunitTestCase : TestMethodTestCase, IXunitTestCase
    {
        readonly IMessageSink diagnosticMessageSink;

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public XunitTestCase()
        {
            // No way for us to get access to the message sink on the execution deserialization path, but that should
            // be okay, because we assume all the issues were reported during discovery.
            diagnosticMessageSink = new NullMessageSink();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCase"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        public XunitTestCase(IMessageSink diagnosticMessageSink,
                             TestMethodDisplay defaultMethodDisplay,
                             ITestMethod testMethod,
                             object[] testMethodArguments = null)
            : base(defaultMethodDisplay, testMethod, testMethodArguments)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <summary>
        /// Gets the display name for the test case. Calls <see cref="TypeUtility.GetDisplayNameWithArguments"/>
        /// with the given base display name (which is itself either derived from <see cref="FactAttribute.DisplayName"/>,
        /// falling back to <see cref="TestMethodTestCase.BaseDisplayName"/>.
        /// </summary>
        /// <param name="factAttribute">The fact attribute the decorated the test case.</param>
        /// <param name="displayName">The base display name from <see cref="TestMethodTestCase.BaseDisplayName"/>.</param>
        /// <returns>The display name for the test case.</returns>
        protected virtual string GetDisplayName(IAttributeInfo factAttribute, string displayName)
            => TestMethod.Method.GetDisplayNameWithArguments(displayName, TestMethodArguments, MethodGenericTypes);

        /// <summary>
        /// Gets the skip reason for the test case. By default, pulls the skip reason from the
        /// <see cref="FactAttribute.Skip"/> property.
        /// </summary>
        /// <param name="factAttribute">The fact attribute the decorated the test case.</param>
        /// <returns>The skip reason, if skipped; <c>null</c>, otherwise.</returns>
        protected virtual string GetSkipReason(IAttributeInfo factAttribute)
            => factAttribute.GetNamedArgument<string>("Skip");

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();

            var factAttribute = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute)).First();
            var baseDisplayName = factAttribute.GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;

            DisplayName = GetDisplayName(factAttribute, baseDisplayName);
            SkipReason = GetSkipReason(factAttribute);

            foreach (var traitAttribute in TestMethod.Method.GetCustomAttributes(typeof(ITraitAttribute))
                                                            .Concat(TestMethod.TestClass.Class.GetCustomAttributes(typeof(ITraitAttribute))))
            {
                var discovererAttribute = traitAttribute.GetCustomAttributes(typeof(TraitDiscovererAttribute)).FirstOrDefault();
                if (discovererAttribute != null)
                {
                    var discoverer = ExtensibilityPointFactory.GetTraitDiscoverer(diagnosticMessageSink, discovererAttribute);
                    if (discoverer != null)
                        foreach (var keyValuePair in discoverer.GetTraits(traitAttribute))
                            Traits.Add(keyValuePair.Key, keyValuePair.Value);
                }
                else
                    diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Trait attribute on '{DisplayName}' did not have [TraitDiscoverer]"));
            }
        }

        /// <inheritdoc/>
        public virtual Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                 IMessageBus messageBus,
                                                 object[] constructorArguments,
                                                 ExceptionAggregator aggregator,
                                                 CancellationTokenSource cancellationTokenSource)
            => new XunitTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource).RunAsync();
    }
}