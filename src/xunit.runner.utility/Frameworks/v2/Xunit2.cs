using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class be used to do discovery and execution of xUnit.net v2 tests
    /// using a reflection-based implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    public class Xunit2 : Xunit2Discoverer, IFrontController
    {
        readonly ITestFrameworkExecutor executor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2"/> class.
        /// </summary>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// tests to be discovered and run without locking assembly files on disk.</param>
        /// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
        /// will be automatically (randomly) generated</param>
        public Xunit2(ISourceInformationProvider sourceInformationProvider, string assemblyFileName, string configFileName = null, bool shadowCopy = true, string shadowCopyFolder = null)
            : base(sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder)
        {
#if !ANDROID
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
#else
            var assm = Assembly.Load(assemblyFileName);
            AssemblyName assemblyName = assm.GetName();
#endif
            executor = Framework.GetExecutor(assemblyName);
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
        {
            return executor.Deserialize(value);
        }

        /// <inheritdoc/>
        public override sealed void Dispose()
        {
            executor.SafeDispose();

            base.Dispose();
        }

        /// <summary>
        /// Starts the process of running all the xUnit.net v2 tests in the assembly.
        /// </summary>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="discoveryOptions">The options to be used during test discovery.</param>
        /// <param name="executionOptions">The options to be used during test execution.</param>
        public void Run(IMessageSink messageSink, XunitDiscoveryOptions discoveryOptions, XunitExecutionOptions executionOptions)
        {
            executor.RunAll(messageSink, discoveryOptions, executionOptions);
        }

        void ITestFrameworkExecutor.RunAll(IMessageSink messageSink, ITestFrameworkOptions discoveryOptions, ITestFrameworkOptions executionOptions)
        {
            executor.RunAll(messageSink, discoveryOptions, executionOptions);
        }

        /// <summary>
        /// Starts the process of running the selected xUnit.net v2 tests.
        /// </summary>
        /// <param name="testCases">The test cases to run; if null, all tests in the assembly are run.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="executionOptions">The options to be used during test execution.</param>
        public void Run(IEnumerable<ITestCase> testCases, IMessageSink messageSink, XunitExecutionOptions executionOptions)
        {
            executor.RunTests(testCases, messageSink, executionOptions);
        }

        void ITestFrameworkExecutor.RunTests(IEnumerable<ITestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            executor.RunTests(testCases, messageSink, options);
        }
    }
}