using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class XunitTestFramework : LongLivedMarshalByRefObject, ITestFramework
    {
        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new XunitTestFrameworkDiscoverer(assemblyInfo);
        }

        public ITestFrameworkExecutor GetExecutor(string assemblyFileName)
        {
            return new XunitTestFrameworkExecutor(assemblyFileName);
        }

        public void Dispose() { }
    }
}