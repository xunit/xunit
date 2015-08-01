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
#if !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !DOTNETCORE
            config = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif
            TestAssembly = new TestAssembly(AssemblyInfo, config, assemblyName.Version);
        }

        /// <summary>
        /// Gets the test assembly that contains the test.
        /// </summary>
        protected TestAssembly TestAssembly { get; set; }

        /// <inheritdoc/>
        protected override ITestFrameworkDiscoverer CreateDiscoverer()
            => new XunitTestFrameworkDiscoverer(AssemblyInfo, SourceInformationProvider, DiagnosticMessageSink);

        /// <inheritdoc/>
        protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            using (var assemblyRunner = new XunitTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
                await assemblyRunner.RunAsync();
        }
    }
}
