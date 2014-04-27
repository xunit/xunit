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
        static readonly DisposalTracker disposalTracker = new DisposalTracker();
        static readonly ConcurrentDictionary<Type, object> instances = new ConcurrentDictionary<Type, object>();

        private static object CreateInstance(Type type, object[] ctorArgs)
        {
            var result = Activator.CreateInstance(type, ctorArgs ?? new object[0]);

            var disposable = result as IDisposable;
            if (disposable != null)
                disposalTracker.Add(disposable);

            return result;
        }

        /// <summary>
        /// Disposes the instances that are contained in the cache.
        /// </summary>
        public static void Dispose()
        {
            instances.Clear();
            disposalTracker.Dispose();
        }

        /// <summary>
        /// Gets an instance of the given type, casting it to <see cref="TInterface"/>, using the provided
        /// constructor arguments. There is a single instance of a given type that is cached and reused,
        /// so classes retrieved from this factory must be stateless and thread-safe.
        /// </summary>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <param name="type">The implementation type.</param>
        /// <param name="ctorArgs">The constructor arguments.</param>
        /// <returns>The instance of the type.</returns>
        public static TInterface Get<TInterface>(Type type, object[] ctorArgs = null)
        {
            return (TInterface)instances.GetOrAdd(type, () => CreateInstance(type, ctorArgs));
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
        /// Gets a trait discoverer.
        /// </summary>
        public static ITraitDiscoverer GetTraitDiscoverer(Type discovererType)
        {
            return Get<ITraitDiscoverer>(discovererType);
        }

        /// <summary>
        /// Gets an xUnit.net v2 test discoverer.
        /// </summary>
        public static IXunitTestCaseDiscoverer GetXunitTestCaseDiscoverer(Type discovererType)
        {
            return Get<IXunitTestCaseDiscoverer>(discovererType);
        }

        /// <summary>
        /// Gets an xUnit.net v2 test collection factory.
        /// </summary>
        public static IXunitTestCollectionFactory GetXunitTestCollectionFactory(Type factoryType, IAssemblyInfo assemblyInfo)
        {
            return Get<IXunitTestCollectionFactory>(factoryType, new[] { assemblyInfo });
        }
    }
}
