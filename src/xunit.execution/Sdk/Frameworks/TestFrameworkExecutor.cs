using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A reusable implementation of <see cref="ITestFrameworkExecutor"/> which contains the basic behavior
    /// for running tests.
    /// </summary>
    /// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
    /// derive from <see cref="ITestCase"/>.</typeparam>
    public abstract class TestFrameworkExecutor<TTestCase> : LongLivedMarshalByRefObject, ITestFrameworkExecutor
        where TTestCase : ITestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFrameworkExecutor{TTestCase}"/> class.
        /// </summary>
        /// <param name="assemblyName">Name of the test assembly.</param>
        /// <param name="sourceInformationProvider">The source line number information provider.</param>
        protected TestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider)
        {
            DisposalTracker = new DisposalTracker();
            SourceInformationProvider = sourceInformationProvider;

#if !WIN8_STORE || WINDOWS_PHONE_APP || WINDOWS_PHONE
            var assembly = Assembly.Load(assemblyName);
#else
            var assembly = Assembly.Load(assemblyName.Name);
#endif
            AssemblyInfo = Reflector.Wrap(assembly);
        }

        /// <summary>
        /// Gets the assembly information of the assembly under test.
        /// </summary>
        protected IAssemblyInfo AssemblyInfo { get; set; }

        /// <summary>
        /// Gets the disposal tracker for the test framework discoverer.
        /// </summary>
        protected DisposalTracker DisposalTracker { get; set; }

        /// <summary>
        /// Gets the source information provider.
        /// </summary>
        protected ISourceInformationProvider SourceInformationProvider { get; set; }

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
            Guard.ArgumentNotNull("messageSink", messageSink);
            Guard.ArgumentNotNull("discoveryOptions", discoveryOptions);
            Guard.ArgumentNotNull("executionOptions", executionOptions);

            var discoverySink = new TestDiscoveryVisitor();

            using (var discoverer = CreateDiscoverer())
            {
                discoverer.Find(false, discoverySink, discoveryOptions);
                discoverySink.Finished.WaitOne();
            }

            RunTestCases(discoverySink.TestCases.Cast<TTestCase>(), messageSink, executionOptions);
        }

        /// <inheritdoc/>
        public virtual void RunTests(IEnumerable<ITestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions executionOptions)
        {
            Guard.ArgumentNotNull("testCases", testCases);
            Guard.ArgumentNotNull("messageSink", messageSink);
            Guard.ArgumentNotNull("executionOptions", executionOptions);

            RunTestCases(testCases.Cast<TTestCase>(), messageSink, executionOptions);
        }

        /// <summary>
        /// Override to run test cases.
        /// </summary>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="messageSink">The message sink to report run status to.</param>
        /// <param name="executionOptions">The user's requested execution options.</param>
        protected abstract void RunTestCases(IEnumerable<TTestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions executionOptions);
    }
}