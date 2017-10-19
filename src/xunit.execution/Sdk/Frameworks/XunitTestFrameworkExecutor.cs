using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFrameworkExecutor"/> that supports execution
    /// of unit tests linked against xunit.core.dll, using xunit.execution.dll.
    /// </summary>
    public class XunitTestFrameworkExecutor : TestFrameworkExecutor<IXunitTestCase>
    {
        readonly Lazy<XunitTestFrameworkDiscoverer> discoverer;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkExecutor"/> class.
        /// </summary>
        /// <param name="assemblyName">Name of the test assembly.</param>
        /// <param name="sourceInformationProvider">The source line number information provider.</param>
        /// <param name="diagnosticMessageSink">The message sink to report diagnostic messages to.</param>
        public XunitTestFrameworkExecutor(AssemblyName assemblyName,
                                          ISourceInformationProvider sourceInformationProvider,
                                          IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            string config = null;
#if NET452
            config = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif
            TestAssembly = new TestAssembly(AssemblyInfo, config, assemblyName.Version);
            discoverer = new Lazy<XunitTestFrameworkDiscoverer>(() => new XunitTestFrameworkDiscoverer(AssemblyInfo, SourceInformationProvider, DiagnosticMessageSink));
        }

        /// <summary>
        /// Gets the test assembly that contains the test.
        /// </summary>
        protected TestAssembly TestAssembly { get; set; }

        /// <inheritdoc/>
        protected override ITestFrameworkDiscoverer CreateDiscoverer()
            => discoverer.Value;

        /// <inheritdoc/>
        public override ITestCase Deserialize(string value)
        {
            if (value.Length > 3 && value.StartsWith(":F:"))
            {
                // Format from TestCaseDescriptorFactory: ":F:{typeName}:{methodName}:{defaultMethodDisplay}"
                var parts = value.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 3)
                {
                    var typeInfo = discoverer.Value.AssemblyInfo.GetType(parts[1]);
                    var testClass = discoverer.Value.CreateTestClass(typeInfo);
                    var methodInfo = testClass.Class.GetMethod(parts[2], true);
                    var testMethod = new TestMethod(testClass, methodInfo);
                    var defaultMethodDisplay = (TestMethodDisplay)int.Parse(parts[3]);
                    return new XunitTestCase(DiagnosticMessageSink, defaultMethodDisplay, testMethod);
                }
            }

            return base.Deserialize(value);
        }

        /// <inheritdoc/>
        protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            using (var assemblyRunner = new XunitTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
                await assemblyRunner.RunAsync();
        }
    }
}
