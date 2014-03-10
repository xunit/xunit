using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFramework"/> that supports discovery and
    /// execution of unit tests linked against xunit.core.dll, using xunit.execution.dll.
    /// </summary>
    public class XunitTestFramework : LongLivedMarshalByRefObject, ITestFramework
    {
        List<IDisposable> toDispose = new List<IDisposable>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFramework"/> class.
        /// </summary>
        public XunitTestFramework()
        {
            SourceInformationProvider = new NullSourceInformationProvider();
        }

        /// <inheritdoc/>
        public ISourceInformationProvider SourceInformationProvider { get; set; }

        /// <inheritdoc/>
        public async void Dispose()
        {
            // We want to immediately return before we call DisconnectAll, since we are in the list
            // of things that will be disconnected.
            await Task.Delay(1);

            toDispose.ForEach(x => x.Dispose());

            LongLivedMarshalByRefObject.DisconnectAll();
        }

        /// <inheritdoc/>
        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assemblyInfo)
        {
            var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider);
            toDispose.Add(discoverer);
            return discoverer;
        }

        /// <inheritdoc/>
        public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
        {
            var executor = new XunitTestFrameworkExecutor(assemblyName, SourceInformationProvider);
            toDispose.Add(executor);
            return executor;
        }

        class NullSourceInformationProvider : ISourceInformationProvider
        {
            public ISourceInformation GetSourceInformation(ITestCase testCase)
            {
                return new SourceInformation();
            }

            public void Dispose() { }
        }
    }
}