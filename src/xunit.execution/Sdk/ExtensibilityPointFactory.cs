using System;
using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents a caching factory for the types used for extensibility throughout the system.
    /// </summary>
    public static class ExtensibilityPointFactory
    {
        static readonly ConcurrentDictionary<Type, object> instances = new ConcurrentDictionary<Type, object>();

        private static TInterface Get<TInterface>(Type type, object[] ctorArgs = null)
        {
            return (TInterface)instances.GetOrAdd(type, () => Activator.CreateInstance(type, ctorArgs ?? new object[0]));
        }

        /// <summary>
        /// Gets a data discoverer.
        /// </summary>
        public static IDataDiscoverer GetDataDiscoverer(Type discovererType)
        {
            return Get<IDataDiscoverer>(discovererType);
        }

        /// <summary>
        /// Gets a test case orderer.
        /// </summary>
        public static ITestCaseOrderer GetTestCaseOrderer(Type ordererType)
        {
            return Get<ITestCaseOrderer>(ordererType);
        }

        /// <summary>
        /// Gets a test collection factory.
        /// </summary>
        public static IXunitTestCollectionFactory GetTestCollectionFactory(Type factoryType, IAssemblyInfo assemblyInfo)
        {
            return Get<IXunitTestCollectionFactory>(factoryType, new[] { assemblyInfo });
        }

        /// <summary>
        /// Gets a trait discoverer.
        /// </summary>
        public static ITraitDiscoverer GetTraitDiscoverer(Type discovererType)
        {
            return Get<ITraitDiscoverer>(discovererType);
        }

        /// <summary>
        /// Gets an xUnit.net v2 test discoverer.
        /// </summary>
        public static IXunitDiscoverer GetXunitDiscoverer(Type discovererType)
        {
            return Get<IXunitDiscoverer>(discovererType);
        }
    }
}
