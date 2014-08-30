using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
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
        static readonly TypeInfo testAssemblyTypeInfo = typeof(ITestAssembly).GetTypeInfo();

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
        /// Gets an instance of the given type, casting it to <typeparamref name="TInterface"/>, using the provided
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
        /// Gets a data discoverer, as specified in a reflected <see cref="DataDiscovererAttribute"/>.
        /// </summary>
        /// <param name="dataDiscovererAttribute">The data discoverer attribute</param>
        /// <returns>The data discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static IDataDiscoverer GetDataDiscoverer(IAttributeInfo dataDiscovererAttribute)
        {
            var args = dataDiscovererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var discovererType = Reflector.GetType(args[1], args[0]);
            if (discovererType == null)
                return null;

            return GetDataDiscoverer(discovererType);
        }

        /// <summary>
        /// Gets a test case orderer.
        /// </summary>
        public static ITestCaseOrderer GetTestCaseOrderer(Type ordererType)
        {
            return Get<ITestCaseOrderer>(ordererType);
        }

        /// <summary>
        /// Gets a test case orderer, as specified in a reflected <see cref="TestCaseOrdererAttribute"/>.
        /// </summary>
        /// <param name="testCaseOrdererAttribute">The test case orderer attribute.</param>
        /// <returns>The test case orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITestCaseOrderer GetTestCaseOrderer(IAttributeInfo testCaseOrdererAttribute)
        {
            var args = testCaseOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var ordererType = Reflector.GetType(args[1], args[0]);
            if (ordererType == null)
                return null;

            return GetTestCaseOrderer(ordererType);
        }

        /// <summary>
        /// Gets a test collection orderer.
        /// </summary>
        public static ITestCollectionOrderer GetTestCollectionOrderer(Type ordererType)
        {
            return Get<ITestCollectionOrderer>(ordererType);
        }

        /// <summary>
        /// Gets a test collection orderer, as specified in a reflected <see cref="TestCollectionOrdererAttribute"/>.
        /// </summary>
        /// <param name="testCollectionOrdererAttribute">The test collection orderer attribute.</param>
        /// <returns>The test collection orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITestCollectionOrderer GetTestCollectionOrderer(IAttributeInfo testCollectionOrdererAttribute)
        {
            var args = testCollectionOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var ordererType = Reflector.GetType(args[1], args[0]);
            if (ordererType == null)
                return null;

            return GetTestCollectionOrderer(ordererType);
        }

        /// <summary>
        /// Gets a test framework discoverer.
        /// </summary>
        public static ITestFrameworkTypeDiscoverer GetTestFrameworkTypeDiscoverer(Type frameworkType)
        {
            return Get<ITestFrameworkTypeDiscoverer>(frameworkType);
        }

        /// <summary>
        /// Gets a test framework discoverer, as specified in a reflected <see cref="TestFrameworkDiscovererAttribute"/>.
        /// </summary>
        public static ITestFrameworkTypeDiscoverer GetTestFrameworkTypeDiscoverer(IAttributeInfo testFrameworkAttribute)
        {
            var args = testFrameworkAttribute.GetConstructorArguments().Cast<string>().ToArray();
            var testFrameworkDiscovererType = Reflector.GetType(args[1], args[0]);
            if (testFrameworkDiscovererType == null)
                return null;

            return GetTestFrameworkTypeDiscoverer(testFrameworkDiscovererType);
        }

        /// <summary>
        /// Gets a trait discoverer.
        /// </summary>
        public static ITraitDiscoverer GetTraitDiscoverer(Type discovererType)
        {
            return Get<ITraitDiscoverer>(discovererType);
        }

        /// <summary>
        /// Gets a trait discoverer, as specified in a reflected <see cref="TraitDiscovererAttribute"/>.
        /// </summary>
        /// <param name="traitDiscovererAttribute">The trait discoverer attribute.</param>
        /// <returns>The trait discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITraitDiscoverer GetTraitDiscoverer(IAttributeInfo traitDiscovererAttribute)
        {
            var args = traitDiscovererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var discovererType = Reflector.GetType(args[1], args[0]);
            if (discovererType == null)
                return null;

            return GetTraitDiscoverer(discovererType);
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
        public static IXunitTestCollectionFactory GetXunitTestCollectionFactory(Type factoryType, ITestAssembly testAssembly)
        {
            return Get<IXunitTestCollectionFactory>(factoryType, new[] { testAssembly });
        }

        /// <summary>
        /// Gets an xUnit.net v2 test collection factory, as specified in a reflected <see cref="CollectionBehaviorAttribute"/>.
        /// </summary>
        /// <param name="collectionBehaviorAttribute">The collection behavior attribute.</param>
        /// <param name="testAssembly">The test assembly.</param>
        /// <returns>The collection factory.</returns>
        public static IXunitTestCollectionFactory GetXunitTestCollectionFactory(IAttributeInfo collectionBehaviorAttribute, ITestAssembly testAssembly)
        {
            return GetXunitTestCollectionFactory(GetTestCollectionFactoryType(collectionBehaviorAttribute), testAssembly);
        }

        static Type GetTestCollectionFactoryType(IAttributeInfo collectionBehaviorAttribute)
        {
            if (collectionBehaviorAttribute == null)
                return typeof(CollectionPerClassTestCollectionFactory);

            var ctorArgs = collectionBehaviorAttribute.GetConstructorArguments().ToList();
            if (ctorArgs.Count == 0)
                return typeof(CollectionPerClassTestCollectionFactory);

            if (ctorArgs.Count == 1)
            {
                if ((CollectionBehavior)ctorArgs[0] == CollectionBehavior.CollectionPerAssembly)
                    return typeof(CollectionPerAssemblyTestCollectionFactory);

                return typeof(CollectionPerClassTestCollectionFactory);
            }

            var result = Reflector.GetType((string)ctorArgs[1], (string)ctorArgs[0]);
            if (result == null || !IsCompatibleTestCollectionFactory(result))
                return typeof(CollectionPerClassTestCollectionFactory);

            return result;
        }

        private static bool IsCompatibleTestCollectionFactory(Type result)
        {
            var resultTypeInfo = result.GetTypeInfo();

            if (!typeof(IXunitTestCollectionFactory).GetTypeInfo().IsAssignableFrom(resultTypeInfo))
                return false;

            return resultTypeInfo.DeclaredConstructors
                                 .Any(ctor =>
                                      {
                                          var parameters = ctor.GetParameters();
                                          if (parameters.Length != 1)
                                              return false;

                                          return testAssemblyTypeInfo.IsAssignableFrom(parameters[0].ParameterType.GetTypeInfo());
                                      });
        }
    }
}
