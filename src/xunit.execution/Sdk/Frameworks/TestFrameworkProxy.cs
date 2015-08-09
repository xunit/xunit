using System;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// This class proxies for the real implementation of <see cref="ITestFramework"/>, based on
    /// whether the user has overridden the choice via <see cref="TestFrameworkAttribute"/>. If
    /// no attribute is found, defaults to <see cref="XunitTestFramework"/>.
    /// </summary>
    public class TestFrameworkProxy : LongLivedMarshalByRefObject, ITestFramework
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFrameworkProxy"/> class.
        /// </summary>
        /// <param name="testAssemblyObject">The test assembly (expected to implement <see cref="IAssemblyInfo"/>).</param>
        /// <param name="sourceInformationProviderObject">The source information provider (expected to implement <see cref="ISourceInformationProvider"/>).</param>
        /// <param name="diagnosticMessageSinkObject">The diagnostic message sink (expected to implement <see cref="IMessageSink"/>).</param>
        public TestFrameworkProxy(object testAssemblyObject, object sourceInformationProviderObject, object diagnosticMessageSinkObject)
        {
            var testAssembly = (IAssemblyInfo)testAssemblyObject;
            var sourceInformationProvider = (ISourceInformationProvider)sourceInformationProviderObject;
            var diagnosticMessageSink = new MessageSinkWrapper((IMessageSink)diagnosticMessageSinkObject);
            var testFrameworkType = GetTestFrameworkType(testAssembly, diagnosticMessageSink);
            InnerTestFramework = CreateInnerTestFramework(testFrameworkType, diagnosticMessageSink);
            SourceInformationProvider = sourceInformationProvider;
        }

        /// <summary>
        /// Gets the test framework that's being wrapped by the proxy.
        /// </summary>
        public ITestFramework InnerTestFramework { get; private set; }

        /// <inheritdoc/>
        public ISourceInformationProvider SourceInformationProvider
        {
            set { InnerTestFramework.SourceInformationProvider = value; }
        }

        static ITestFramework CreateInnerTestFramework(Type testFrameworkType, IMessageSink diagnosticMessageSink)
        {
            try
            {
                var ctorWithSink = testFrameworkType.GetTypeInfo().DeclaredConstructors
                                                                  .FirstOrDefault(ctor =>
                                                                  {
                                                                      var paramInfos = ctor.GetParameters();
                                                                      return paramInfos.Length == 1 && paramInfos[0].ParameterType == typeof(IMessageSink);
                                                                  });
                if (ctorWithSink != null)
                    return (ITestFramework)ctorWithSink.Invoke(new object[] { diagnosticMessageSink });

                return (ITestFramework)Activator.CreateInstance(testFrameworkType);
            }
            catch (Exception ex)
            {
                diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Exception thrown during test framework construction: {ex.Unwrap()}"));
                return new XunitTestFramework(diagnosticMessageSink);
            }
        }

        /// <inheritdoc/>
        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
        {
            return InnerTestFramework.GetDiscoverer(assembly);
        }

        /// <inheritdoc/>
        public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
        {
            return InnerTestFramework.GetExecutor(assemblyName);
        }

        static Type GetTestFrameworkType(IAssemblyInfo testAssembly, IMessageSink diagnosticMessageSink)
        {
            try
            {
                var testFrameworkAttr = testAssembly.GetCustomAttributes(typeof(ITestFrameworkAttribute)).FirstOrDefault();
                if (testFrameworkAttr != null)
                {
                    var discovererAttr = testFrameworkAttr.GetCustomAttributes(typeof(TestFrameworkDiscovererAttribute)).FirstOrDefault();
                    if (discovererAttr != null)
                    {
                        var discoverer = ExtensibilityPointFactory.GetTestFrameworkTypeDiscoverer(diagnosticMessageSink, discovererAttr);
                        if (discoverer != null)
                            return discoverer.GetTestFrameworkType(testFrameworkAttr);

                        var ctorArgs = discovererAttr.GetConstructorArguments().ToArray();
                        diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Unable to create custom test framework discoverer type '{ctorArgs[1]}, {ctorArgs[0]}'"));
                    }
                    else
                    {
                        diagnosticMessageSink.OnMessage(new DiagnosticMessage("Assembly-level test framework attribute was not decorated with [TestFrameworkDiscoverer]"));
                    }
                }
            }
            catch (Exception ex)
            {
                diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Exception thrown during test framework discoverer construction: {ex.Unwrap()}"));
            }

            return typeof(XunitTestFramework);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            InnerTestFramework.Dispose();
        }

        /// <summary>
        /// INTERNAL CLASS. DO NOT USE.
        /// </summary>
        public class MessageSinkWrapper : LongLivedMarshalByRefObject, IMessageSink
        {
            /// <summary/>
            public readonly IMessageSink InnerSink;

            /// <summary/>
            public MessageSinkWrapper(IMessageSink innerSink)
            {
                InnerSink = innerSink;
            }

            /// <summary/>
            public bool OnMessage(IMessageSinkMessage message)
            {
                return InnerSink.OnMessage(message);
            }
        }
    }
}
