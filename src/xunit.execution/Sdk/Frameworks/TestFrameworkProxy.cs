using System;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// This class proxies for the real implementation of <see cref="ITestFramework"/>, based on
    /// whether the user has overridden the choice via <see cref="TestFrameworkAttribute"/>. If
    /// no attribute is found, defaults to <see cref="XunitTestFramework"/>.
    /// </summary>
    public class TestFrameworkProxy : LongLivedMarshalByRefObject, ITestFramework
    {
        readonly ITestFramework testFramework;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFrameworkProxy"/> class.
        /// </summary>
        /// <param name="testAssembly">The test assembly.</param>
        /// <param name="sourceInformationProvider">The source information provider.</param>
        public TestFrameworkProxy(object testAssemblyObject, object sourceInformationProviderObject)
        {
            var testAssembly = (IAssemblyInfo)testAssemblyObject;
            var sourceInformationProvider = (ISourceInformationProvider)sourceInformationProviderObject;

            Type testFrameworkType = typeof(XunitTestFramework);

            try
            {
                var attr = testAssembly.GetCustomAttributes(typeof(TestFrameworkAttribute)).FirstOrDefault();
                if (attr != null)
                {
                    var ctorArgs = attr.GetConstructorArguments().Cast<string>().ToArray();
                    testFrameworkType = Reflector.GetType(ctorArgs[1], ctorArgs[0]);
                }
            }
            catch
            {
                // TODO: Log environmental error
            }

            try
            {
                testFramework = (ITestFramework)Activator.CreateInstance(testFrameworkType);
            }
            catch
            {
                // TODO: Log environmental error
                testFramework = new XunitTestFramework();
            }

            SourceInformationProvider = sourceInformationProvider;
        }

        /// <inheritdoc/>
        public ISourceInformationProvider SourceInformationProvider
        {
            set { testFramework.SourceInformationProvider = value; }
        }

        /// <inheritdoc/>
        public ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly)
        {
            return testFramework.GetDiscoverer(assembly);
        }

        /// <inheritdoc/>
        public ITestFrameworkExecutor GetExecutor(AssemblyName assemblyName)
        {
            return testFramework.GetExecutor(assemblyName);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            testFramework.Dispose();
        }
    }
}
