using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFramework"/> that supports discovery and
    /// execution of unit tests linked against xunit2.dll.
    /// </summary>
    public class XunitTestFramework : LongLivedMarshalByRefObject, ITestFramework
    {
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
            LongLivedMarshalByRefObject.DisconnectAll();
        }

        /// <inheritdoc/>
        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new XunitTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider);
        }

        /// <inheritdoc/>
        public ITestFrameworkExecutor GetExecutor(string assemblyFileName)
        {
            return new XunitTestFrameworkExecutor(assemblyFileName, SourceInformationProvider);
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