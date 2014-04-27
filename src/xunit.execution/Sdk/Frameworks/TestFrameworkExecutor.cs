using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public abstract class TestFrameworkExecutor<TTestCase> : LongLivedMarshalByRefObject, ITestFrameworkExecutor
        where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFrameworkExecutor"/> class.
        /// </summary>
        /// <param name="assemblyName">Name of the test assembly.</param>
        /// <param name="sourceInformationProvider">The source line number information provider.</param>
        public TestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider)
        {
            DisposalTracker = new DisposalTracker();

            SourceInformationProvider = sourceInformationProvider;

            var assembly = Assembly.Load(assemblyName);
            AssemblyInfo = Reflector.Wrap(assembly);
        }

        /// <summary>
        /// Gets the assembly information of the assembly under test.
        /// </summary>
        protected IAssemblyInfo AssemblyInfo { get; private set; }

        /// <summary>
        /// Gets the disposal tracker for the test framework discoverer.
        /// </summary>
        protected DisposalTracker DisposalTracker { get; private set; }

        /// <summary>
        /// Gets the source information provider.
        /// </summary>
        protected ISourceInformationProvider SourceInformationProvider { get; private set; }

        /// <summary>
        /// Override to create a test framework discoverer that can be used to discover
        /// tests when the user asks to run all test.
        /// </summary>
        /// <returns>The test framework discoverer</returns>
        protected abstract ITestFrameworkDiscoverer CreateDiscoverer();

        /// <inheritdoc/>
        public virtual ITestCase Deserialize(string value)
        {
            return SerializationHelper.Deserialize<ITestCase>(value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DisposalTracker.Dispose();
        }

        /// <inheritdoc/>
        public virtual void RunAll(IMessageSink messageSink, ITestFrameworkOptions discoveryOptions, ITestFrameworkOptions executionOptions)
        {
            var discoverySink = new TestDiscoveryVisitor();

            using (var discoverer = CreateDiscoverer())
            {
                discoverer.Find(false, discoverySink, discoveryOptions);
                discoverySink.Finished.WaitOne();
            }

            RunTests(discoverySink.TestCases, messageSink, executionOptions);

        }

        /// <inheritdoc/>
        public virtual void RunTests(IEnumerable<ITestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions executionOptions)
        {
            Guard.ArgumentNotNull("testCases", testCases);
            Guard.ArgumentNotNull("messageSink", messageSink);
            Guard.ArgumentNotNull("executionOptions", executionOptions);

            RunTestCases(testCases, messageSink, executionOptions);
        }

        protected abstract void RunTestCases(IEnumerable<ITestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions executionOptions);
    }
}