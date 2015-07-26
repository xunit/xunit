using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A default implementation of <see cref="ITestFramework"/> that tracks objects to be
    /// disposed when the framework is disposed. The discoverer and executor are automatically
    /// tracked for disposal, since those interfaces mandate an implementation of <see cref="IDisposable"/>.
    /// </summary>
    public abstract class TestFramework : LongLivedMarshalByRefObject, ITestFramework
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestFramework"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        protected TestFramework(IMessageSink diagnosticMessageSink)
        {
            Guard.ArgumentNotNull("diagnosticMessageSink", diagnosticMessageSink);

            DiagnosticMessageSink = diagnosticMessageSink;
            DisposalTracker = new DisposalTracker();
            SourceInformationProvider = NullSourceInformationProvider.Instance;
        }

        /// <summary>
        /// Gets the message sink used to send diagnostic messages.
        /// </summary>
        public IMessageSink DiagnosticMessageSink { get; private set; }

        /// <summary>
        /// Gets the disposal tracker for the test framework.
        /// </summary>
        protected DisposalTracker DisposalTracker { get; set; }

        /// <inheritdoc/>
        public ISourceInformationProvider SourceInformationProvider { get; set; }

        /// <inheritdoc/>
        public async void Dispose()
        {
            // We want to immediately return before we call DisconnectAll, since we are in the list
            // of things that will be disconnected.
            await Task.Delay(1);

            ExtensibilityPointFactory.Dispose();
            DisposalTracker.Dispose();

            LongLivedMarshalByRefObject.DisconnectAll();
        }

        /// <summary>
        /// Override this method to provide the implementation of <see cref="ITestFrameworkDiscoverer"/>.
        /// </summary>
        /// <param name="assemblyInfo">The assembly that is being discovered.</param>
        /// <returns>Returns the test framework discoverer.</returns>
        protected abstract ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo);

        /// <summary>
        /// Override this method to provide the implementation of <see cref="ITestFrameworkExecutor"/>.
        /// </summary>
        /// <param name="assemblyName">The assembly that is being executed.</param>
        /// <returns>Returns the test framework executor.</returns>
        protected abstract ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName);

        /// <inheritdoc/>
        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assemblyInfo)
        {
            var discoverer = CreateDiscoverer(assemblyInfo);
            DisposalTracker.Add(discoverer);
            return discoverer;
        }

        /// <inheritdoc/>
        public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
        {
            var executor = CreateExecutor(assemblyName);
            DisposalTracker.Add(executor);
            return executor;
        }

        class NullSourceInformationProvider : ISourceInformationProvider
        {
            public static readonly NullSourceInformationProvider Instance = new NullSourceInformationProvider();

            public ISourceInformation GetSourceInformation(ITestCase testCase)
            {
                return new SourceInformation();
            }

            public void Dispose() { }
        }
    }
}
