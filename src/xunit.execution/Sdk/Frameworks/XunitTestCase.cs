using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        static ConcurrentDictionary<string, IEnumerable<IAttributeInfo>> assemblyTraitAttributeCache = new ConcurrentDictionary<string, IEnumerable<IAttributeInfo>>(StringComparer.OrdinalIgnoreCase);
        static ConcurrentDictionary<string, IEnumerable<IAttributeInfo>> typeTraitAttributeCache = new ConcurrentDictionary<string, IEnumerable<IAttributeInfo>>(StringComparer.OrdinalIgnoreCase);

        int timeout;

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public XunitTestCase()
        {
            // No way for us to get access to the message sink on the execution deserialization path, but that should
            // be okay, because we assume all the issues were reported during discovery.
            DiagnosticMessageSink = new NullMessageSink();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCase"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        [Obsolete("Please call the constructor which takes TestMethodDisplayOptions")]
        public XunitTestCase(IMessageSink diagnosticMessageSink,
                             TestMethodDisplay defaultMethodDisplay,
                             ITestMethod testMethod,
                             object[] testMethodArguments = null)
            : this(diagnosticMessageSink, defaultMethodDisplay, TestMethodDisplayOptions.None, testMethod, testMethodArguments) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestCase"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        public XunitTestCase(IMessageSink diagnosticMessageSink,
                             TestMethodDisplay defaultMethodDisplay,
                             TestMethodDisplayOptions defaultMethodDisplayOptions,
                             ITestMethod testMethod,
                             object[] testMethodArguments = null)
            : base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        {
            DiagnosticMessageSink = diagnosticMessageSink;
        }

        /// <summary>
        /// Gets the message sink used to report <see cref="IDiagnosticMessage"/> messages.
        /// </summary>
        protected IMessageSink DiagnosticMessageSink { get; }

        /// <inheritdoc/>
        public int Timeout
        {
            get
            {
                EnsureInitialized();
                return timeout;
            }
            protected set
            {
                EnsureInitialized();
                timeout = value;
            }
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

        /// <summary>
        /// Gets the timeout for the test case. By default, pulls the skip reason from the
        /// <see cref="FactAttribute.Timeout"/> property.
        /// </summary>
        /// <param name="factAttribute">The fact attribute the decorated the test case.</param>
        /// <returns>The timeout in milliseconds, if set; 0, if unset.</returns>
        protected virtual int GetTimeout(IAttributeInfo factAttribute)
            => factAttribute.GetNamedArgument<int>("Timeout");

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();

            var factAttribute = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute)).First();
            var baseDisplayName = factAttribute.GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;

            DisplayName = GetDisplayName(factAttribute, baseDisplayName);
            SkipReason = GetSkipReason(factAttribute);
            Timeout = GetTimeout(factAttribute);

            foreach (var traitAttribute in GetTraitAttributesData(TestMethod))
            {
                var discovererAttribute = traitAttribute.GetCustomAttributes(typeof(TraitDiscovererAttribute)).FirstOrDefault();
                if (discovererAttribute != null)
                {
                    var discoverer = ExtensibilityPointFactory.GetTraitDiscoverer(DiagnosticMessageSink, discovererAttribute);
                    if (discoverer != null)
                        foreach (var keyValuePair in discoverer.GetTraits(traitAttribute))
                            Traits.Add(keyValuePair.Key, keyValuePair.Value);
                }
                else
                    DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Trait attribute on '{DisplayName}' did not have [TraitDiscoverer]"));
            }
        }

        static IEnumerable<IAttributeInfo> GetCachedTraitAttributes(IAssemblyInfo assembly)
            => assemblyTraitAttributeCache.AddOrGet(assembly.Name, () => assembly.GetCustomAttributes(typeof(ITraitAttribute)));

        static IEnumerable<IAttributeInfo> GetCachedTraitAttributes(ITypeInfo type)
            => typeTraitAttributeCache.AddOrGet(type.Name, () => type.GetCustomAttributes(typeof(ITraitAttribute)));

        static IEnumerable<IAttributeInfo> GetTraitAttributesData(ITestMethod testMethod)
        {
            return GetCachedTraitAttributes(testMethod.TestClass.Class.Assembly)
                  .Concat(testMethod.Method.GetCustomAttributes(typeof(ITraitAttribute)))
                  .Concat(GetCachedTraitAttributes(testMethod.TestClass.Class));
        }

        /// <inheritdoc/>
        public virtual Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                 IMessageBus messageBus,
                                                 object[] constructorArguments,
                                                 ExceptionAggregator aggregator,
                                                 CancellationTokenSource cancellationTokenSource)
            => new XunitTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource).RunAsync();

        /// <inheritdoc/>
        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("Timeout", Timeout);
        }

        /// <inheritdoc/>
        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            Timeout = data.GetValue<int>("Timeout");
        }
    }
}
