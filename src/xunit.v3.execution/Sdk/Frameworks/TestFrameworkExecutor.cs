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
        /// <param name="diagnosticMessageSink">The message sink to report diagnostic messages to.</param>
        protected TestFrameworkExecutor(AssemblyName assemblyName,
                                        ISourceInformationProvider sourceInformationProvider,
                                        IMessageSink diagnosticMessageSink)
        {
            DisposalTracker = new DisposalTracker();
            SourceInformationProvider = sourceInformationProvider;
            DiagnosticMessageSink = diagnosticMessageSink;

            var assembly = Assembly.Load(assemblyName);

            AssemblyInfo = Reflector.Wrap(assembly);
        }

        /// <summary>
        /// Gets the assembly information of the assembly under test.
        /// </summary>
        protected IAssemblyInfo AssemblyInfo { get; set; }

        /// <summary>
        /// Gets the message sink to send diagnostic messages to.
        /// </summary>
        protected IMessageSink DiagnosticMessageSink { get; set; }

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
        public virtual void RunAll(IMessageSink executionMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
        {
            Guard.ArgumentNotNull("executionMessageSink", executionMessageSink);
            Guard.ArgumentNotNull("discoveryOptions", discoveryOptions);
            Guard.ArgumentNotNull("executionOptions", executionOptions);

            var discoverySink = new TestDiscoveryVisitor();

            using (var discoverer = CreateDiscoverer())
            {
                discoverer.Find(false, discoverySink, discoveryOptions);
                discoverySink.Finished.WaitOne();
            }

            RunTestCases(discoverySink.TestCases.Cast<TTestCase>(), executionMessageSink, executionOptions);
        }

        /// <inheritdoc/>
        public virtual void RunTests(IEnumerable<ITestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            Guard.ArgumentNotNull("testCases", testCases);
            Guard.ArgumentNotNull("executionMessageSink", executionMessageSink);
            Guard.ArgumentNotNull("executionOptions", executionOptions);

            RunTestCases(testCases.Cast<TTestCase>(), executionMessageSink, executionOptions);
        }

        /// <summary>
        /// Override to run test cases.
        /// </summary>
        /// <param name="testCases">The test cases to be run.</param>
        /// <param name="executionMessageSink">The message sink to report run status to.</param>
        /// <param name="executionOptions">The user's requested execution options.</param>
        protected abstract void RunTestCases(IEnumerable<TTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions);
    }
}
