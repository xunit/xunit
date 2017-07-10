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
        static readonly ConcurrentDictionary<Tuple<Type, IMessageSink>, object> instances = new ConcurrentDictionary<Tuple<Type, IMessageSink>, object>();

        static object CreateInstance(IMessageSink diagnosticMessageSink, Type type, object[] ctorArgs)
        {
            ctorArgs = ctorArgs ?? new object[0];
            object result = null;

            try
            {
                var ctorArgsWithMessageSink = ctorArgs.Concat(new object[] { diagnosticMessageSink }).ToArray();
                result = Activator.CreateInstance(type, ctorArgsWithMessageSink);
            }
            catch (MissingMemberException)
            {
                try
                {
                    result = Activator.CreateInstance(type, ctorArgs);
                }
                catch (MissingMemberException)
                {
                    diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Could not find constructor for '{type.FullName}' with arguments type(s): {(string.Join(", ", ctorArgs.Select(a => a == null ? "(unknown)" : a.GetType().FullName)))}"));
                    throw;
                }
            }

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
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="type">The implementation type.</param>
        /// <param name="ctorArgs">The constructor arguments. Since diagnostic message sinks are optional,
        /// the code first looks for a type that takes the given arguments plus the message sink, and only
        /// falls back to the message sink-less constructor if none was found.</param>
        /// <returns>The instance of the type.</returns>
        public static TInterface Get<TInterface>(IMessageSink diagnosticMessageSink, Type type, object[] ctorArgs = null)
        {
            return (TInterface)instances.GetOrAdd(Tuple.Create(type, diagnosticMessageSink), () => CreateInstance(diagnosticMessageSink, type, ctorArgs));
        }

        /// <summary>
        /// Gets a data discoverer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="discovererType">The discoverer type</param>
        public static IDataDiscoverer GetDataDiscoverer(IMessageSink diagnosticMessageSink, Type discovererType)
        {
            return Get<IDataDiscoverer>(diagnosticMessageSink, discovererType);
        }

        /// <summary>
        /// Gets a data discoverer, as specified in a reflected <see cref="DataDiscovererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="dataDiscovererAttribute">The data discoverer attribute</param>
        /// <returns>The data discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static IDataDiscoverer GetDataDiscoverer(IMessageSink diagnosticMessageSink, IAttributeInfo dataDiscovererAttribute)
        {
            var args = dataDiscovererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var discovererType = SerializationHelper.GetType(args[1], args[0]);
            if (discovererType == null)
                return null;

            return GetDataDiscoverer(diagnosticMessageSink, discovererType);
        }

        /// <summary>
        /// Gets a test case orderer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="ordererType">The test case orderer type</param>
        public static ITestCaseOrderer GetTestCaseOrderer(IMessageSink diagnosticMessageSink, Type ordererType)
        {
            return Get<ITestCaseOrderer>(diagnosticMessageSink, ordererType);
        }

        /// <summary>
        /// Gets a test case orderer, as specified in a reflected <see cref="TestCaseOrdererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testCaseOrdererAttribute">The test case orderer attribute.</param>
        /// <returns>The test case orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITestCaseOrderer GetTestCaseOrderer(IMessageSink diagnosticMessageSink, IAttributeInfo testCaseOrdererAttribute)
        {
            var args = testCaseOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var ordererType = SerializationHelper.GetType(args[1], args[0]);
            if (ordererType == null)
                return null;

            return GetTestCaseOrderer(diagnosticMessageSink, ordererType);
        }

        /// <summary>
        /// Gets a test collection orderer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="ordererType">The test collection orderer type</param>
        public static ITestCollectionOrderer GetTestCollectionOrderer(IMessageSink diagnosticMessageSink, Type ordererType)
        {
            return Get<ITestCollectionOrderer>(diagnosticMessageSink, ordererType);
        }

        /// <summary>
        /// Gets a test collection orderer, as specified in a reflected <see cref="TestCollectionOrdererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testCollectionOrdererAttribute">The test collection orderer attribute.</param>
        /// <returns>The test collection orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITestCollectionOrderer GetTestCollectionOrderer(IMessageSink diagnosticMessageSink, IAttributeInfo testCollectionOrdererAttribute)
        {
            var args = testCollectionOrdererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var ordererType = SerializationHelper.GetType(args[1], args[0]);
            if (ordererType == null)
                return null;

            return GetTestCollectionOrderer(diagnosticMessageSink, ordererType);
        }

        /// <summary>
        /// Gets a test framework discoverer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="frameworkType">The test framework type discoverer type</param>
        public static ITestFrameworkTypeDiscoverer GetTestFrameworkTypeDiscoverer(IMessageSink diagnosticMessageSink, Type frameworkType)
        {
            return Get<ITestFrameworkTypeDiscoverer>(diagnosticMessageSink, frameworkType);
        }

        /// <summary>
        /// Gets a test framework discoverer, as specified in a reflected <see cref="TestFrameworkDiscovererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testFrameworkDiscovererAttribute">The test framework discoverer attribute</param>
        public static ITestFrameworkTypeDiscoverer GetTestFrameworkTypeDiscoverer(IMessageSink diagnosticMessageSink, IAttributeInfo testFrameworkDiscovererAttribute)
        {
            var args = testFrameworkDiscovererAttribute.GetConstructorArguments().Cast<string>().ToArray();
            var testFrameworkDiscovererType = SerializationHelper.GetType(args[1], args[0]);
            if (testFrameworkDiscovererType == null)
                return null;

            return GetTestFrameworkTypeDiscoverer(diagnosticMessageSink, testFrameworkDiscovererType);
        }

        /// <summary>
        /// Gets a trait discoverer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="traitDiscovererType">The trait discoverer type</param>
        public static ITraitDiscoverer GetTraitDiscoverer(IMessageSink diagnosticMessageSink, Type traitDiscovererType)
        {
            return Get<ITraitDiscoverer>(diagnosticMessageSink, traitDiscovererType);
        }

        /// <summary>
        /// Gets a trait discoverer, as specified in a reflected <see cref="TraitDiscovererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="traitDiscovererAttribute">The trait discoverer attribute.</param>
        /// <returns>The trait discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITraitDiscoverer GetTraitDiscoverer(IMessageSink diagnosticMessageSink, IAttributeInfo traitDiscovererAttribute)
        {
            var args = traitDiscovererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var discovererType = SerializationHelper.GetType(args[1], args[0]);
            if (discovererType == null)
                return null;

            return GetTraitDiscoverer(diagnosticMessageSink, discovererType);
        }

        /// <summary>
        /// Gets an xUnit.net v2 test discoverer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testCaseDiscovererType">The test case discoverer type</param>
        public static IXunitTestCaseDiscoverer GetXunitTestCaseDiscoverer(IMessageSink diagnosticMessageSink, Type testCaseDiscovererType)
        {
            return Get<IXunitTestCaseDiscoverer>(diagnosticMessageSink, testCaseDiscovererType);
        }

        /// <summary>
        /// Gets an xUnit.net v2 test collection factory.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testCollectionFactoryType">The test collection factory type</param>
        /// <param name="testAssembly">The test assembly under test</param>
        public static IXunitTestCollectionFactory GetXunitTestCollectionFactory(IMessageSink diagnosticMessageSink, Type testCollectionFactoryType, ITestAssembly testAssembly)
        {
            return Get<IXunitTestCollectionFactory>(diagnosticMessageSink, testCollectionFactoryType, new object[] { testAssembly });
        }

        /// <summary>
        /// Gets an xUnit.net v2 test collection factory, as specified in a reflected <see cref="CollectionBehaviorAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="collectionBehaviorAttribute">The collection behavior attribute.</param>
        /// <param name="testAssembly">The test assembly.</param>
        /// <returns>The collection factory.</returns>
        public static IXunitTestCollectionFactory GetXunitTestCollectionFactory(IMessageSink diagnosticMessageSink, IAttributeInfo collectionBehaviorAttribute, ITestAssembly testAssembly)
        {
            try
            {
                var testCollectionFactoryType = GetTestCollectionFactoryType(diagnosticMessageSink, collectionBehaviorAttribute);
                return GetXunitTestCollectionFactory(diagnosticMessageSink, testCollectionFactoryType, testAssembly);
            }
            catch
            {
                return new CollectionPerClassTestCollectionFactory(testAssembly, diagnosticMessageSink);
            }
        }

        static Type GetTestCollectionFactoryType(IMessageSink diagnosticMessageSink, IAttributeInfo collectionBehaviorAttribute)
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

            var result = SerializationHelper.GetType((string)ctorArgs[1], (string)ctorArgs[0]);
            if (result == null)
            {
                diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Unable to create test collection factory type '{ctorArgs[1]}, {ctorArgs[0]}'"));
                return typeof(CollectionPerClassTestCollectionFactory);
            }

            var resultTypeInfo = result.GetTypeInfo();
            if (!typeof(IXunitTestCollectionFactory).GetTypeInfo().IsAssignableFrom(resultTypeInfo))
            {
                diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Test collection factory type '{ctorArgs[1]}, {ctorArgs[0]}' does not implement IXunitTestCollectionFactory"));
                return typeof(CollectionPerClassTestCollectionFactory);
            }

            return result;
        }
    }
}
