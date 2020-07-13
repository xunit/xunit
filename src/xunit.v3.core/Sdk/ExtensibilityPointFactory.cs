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
        static readonly ConcurrentDictionary<(Type type, IMessageSink diagnosticMessageSink), object?> instances = new ConcurrentDictionary<(Type type, IMessageSink diagnosticMessageSink), object?>();

        static object? CreateInstance(IMessageSink diagnosticMessageSink, Type type, object?[]? ctorArgs)
        {
            ctorArgs ??= new object[0];
            object? result = null;

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

            if (result is IDisposable disposable)
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
        public static TInterface? Get<TInterface>(IMessageSink diagnosticMessageSink, Type type, object?[]? ctorArgs = null)
            where TInterface : class
        {
            Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
            Guard.ArgumentNotNull(nameof(type), type);

            return (TInterface?)instances.GetOrAdd((type, diagnosticMessageSink), () => CreateInstance(diagnosticMessageSink, type, ctorArgs));
        }

        /// <summary>
        /// Gets a data discoverer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="discovererType">The discoverer type</param>
        public static IDataDiscoverer? GetDataDiscoverer(IMessageSink diagnosticMessageSink, Type discovererType) =>
            Get<IDataDiscoverer>(diagnosticMessageSink, discovererType);

        /// <summary>
        /// Gets a data discoverer, as specified in a reflected <see cref="DataDiscovererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="dataDiscovererAttribute">The data discoverer attribute</param>
        /// <returns>The data discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static IDataDiscoverer? GetDataDiscoverer(IMessageSink diagnosticMessageSink, IAttributeInfo dataDiscovererAttribute)
        {
            Guard.ArgumentNotNull(nameof(dataDiscovererAttribute), dataDiscovererAttribute);

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
        public static ITestCaseOrderer? GetTestCaseOrderer(IMessageSink diagnosticMessageSink, Type ordererType) =>
            Get<ITestCaseOrderer>(diagnosticMessageSink, ordererType);

        /// <summary>
        /// Gets a test case orderer, as specified in a reflected <see cref="TestCaseOrdererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testCaseOrdererAttribute">The test case orderer attribute.</param>
        /// <returns>The test case orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITestCaseOrderer? GetTestCaseOrderer(IMessageSink diagnosticMessageSink, IAttributeInfo testCaseOrdererAttribute)
        {
            Guard.ArgumentNotNull(nameof(testCaseOrdererAttribute), testCaseOrdererAttribute);

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
        public static ITestCollectionOrderer? GetTestCollectionOrderer(IMessageSink diagnosticMessageSink, Type ordererType) =>
            Get<ITestCollectionOrderer>(diagnosticMessageSink, ordererType);

        /// <summary>
        /// Gets a test collection orderer, as specified in a reflected <see cref="TestCollectionOrdererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testCollectionOrdererAttribute">The test collection orderer attribute.</param>
        /// <returns>The test collection orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITestCollectionOrderer? GetTestCollectionOrderer(IMessageSink diagnosticMessageSink, IAttributeInfo testCollectionOrdererAttribute)
        {
            Guard.ArgumentNotNull(nameof(testCollectionOrdererAttribute), testCollectionOrdererAttribute);

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
        public static ITestFrameworkTypeDiscoverer? GetTestFrameworkTypeDiscoverer(IMessageSink diagnosticMessageSink, Type frameworkType) =>
            Get<ITestFrameworkTypeDiscoverer>(diagnosticMessageSink, frameworkType);

        /// <summary>
        /// Gets a test framework discoverer, as specified in a reflected <see cref="TestFrameworkDiscovererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testFrameworkDiscovererAttribute">The test framework discoverer attribute</param>
        public static ITestFrameworkTypeDiscoverer? GetTestFrameworkTypeDiscoverer(IMessageSink diagnosticMessageSink, IAttributeInfo testFrameworkDiscovererAttribute)
        {
            Guard.ArgumentNotNull(nameof(testFrameworkDiscovererAttribute), testFrameworkDiscovererAttribute);

            var testFrameworkDiscovererType = TypeFromAttributeConstructor(testFrameworkDiscovererAttribute);
            if (testFrameworkDiscovererType == null)
                return null;

            return GetTestFrameworkTypeDiscoverer(diagnosticMessageSink, testFrameworkDiscovererType);
        }

        /// <summary>
        /// Gets a trait discoverer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="traitDiscovererType">The trait discoverer type</param>
        public static ITraitDiscoverer? GetTraitDiscoverer(IMessageSink diagnosticMessageSink, Type traitDiscovererType) =>
            Get<ITraitDiscoverer>(diagnosticMessageSink, traitDiscovererType);

        /// <summary>
        /// Gets a trait discoverer, as specified in a reflected <see cref="TraitDiscovererAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="traitDiscovererAttribute">The trait discoverer attribute.</param>
        /// <returns>The trait discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
        public static ITraitDiscoverer? GetTraitDiscoverer(IMessageSink diagnosticMessageSink, IAttributeInfo traitDiscovererAttribute)
        {
            Guard.ArgumentNotNull(nameof(traitDiscovererAttribute), traitDiscovererAttribute);

            var discovererType = TypeFromAttributeConstructor(traitDiscovererAttribute);
            if (discovererType == null)
                return null;

            return GetTraitDiscoverer(diagnosticMessageSink, discovererType);
        }

        /// <summary>
        /// Gets an xUnit.net v3 test discoverer.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testCaseDiscovererType">The test case discoverer type</param>
        public static IXunitTestCaseDiscoverer? GetXunitTestCaseDiscoverer(IMessageSink diagnosticMessageSink, Type testCaseDiscovererType) =>
            Get<IXunitTestCaseDiscoverer>(diagnosticMessageSink, testCaseDiscovererType);

        /// <summary>
        /// Gets an xUnit.net v3 test collection factory.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="testCollectionFactoryType">The test collection factory type</param>
        /// <param name="testAssembly">The test assembly under test</param>
        public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
            IMessageSink diagnosticMessageSink,
            Type testCollectionFactoryType,
            ITestAssembly testAssembly) =>
                Get<IXunitTestCollectionFactory>(diagnosticMessageSink, testCollectionFactoryType, new object[] { testAssembly });

        /// <summary>
        /// Gets an xUnit.net v3 test collection factory, as specified in a reflected <see cref="CollectionBehaviorAttribute"/>.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="collectionBehaviorAttribute">The collection behavior attribute.</param>
        /// <param name="testAssembly">The test assembly.</param>
        /// <returns>The collection factory.</returns>
        public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
            IMessageSink diagnosticMessageSink,
            IAttributeInfo? collectionBehaviorAttribute,
            ITestAssembly testAssembly)
        {
            try
            {
                var testCollectionFactoryType = GetTestCollectionFactoryType(diagnosticMessageSink, collectionBehaviorAttribute);
                return GetXunitTestCollectionFactory(diagnosticMessageSink, testCollectionFactoryType, testAssembly);
            }
            catch
            {
                return null;
            }
        }

        static Type GetTestCollectionFactoryType(IMessageSink diagnosticMessageSink, IAttributeInfo? collectionBehaviorAttribute)
        {
            if (collectionBehaviorAttribute == null)
                return typeof(CollectionPerClassTestCollectionFactory);

            var ctorArgs = collectionBehaviorAttribute.GetConstructorArguments().ToList();
            if (ctorArgs.Count == 0)
                return typeof(CollectionPerClassTestCollectionFactory);

            if (ctorArgs.Count == 1)
            {
                if (ctorArgs[0] is CollectionBehavior collectionBehavior && collectionBehavior == CollectionBehavior.CollectionPerAssembly)
                    return typeof(CollectionPerAssemblyTestCollectionFactory);

                return typeof(CollectionPerClassTestCollectionFactory);
            }

            if (!(ctorArgs[0] is string typeName) || !(ctorArgs[1] is string assemblyName))
                diagnosticMessageSink.OnMessage(new DiagnosticMessage($"[CollectionBehavior({ToQuotedString(ctorArgs[0])}, {ToQuotedString(ctorArgs[1])})] cannot have null argument values"));
            else
            {
                var result = SerializationHelper.GetType(assemblyName, typeName);
                if (result == null)
                    diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Unable to create test collection factory type '{assemblyName}, {typeName}'"));
                else
                {
                    var resultTypeInfo = result.GetTypeInfo();
                    if (!typeof(IXunitTestCollectionFactory).GetTypeInfo().IsAssignableFrom(resultTypeInfo))
                        diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Test collection factory type '{ctorArgs[1]}, {ctorArgs[0]}' does not implement IXunitTestCollectionFactory"));
                    else
                        return result;
                }
            }

            return typeof(CollectionPerClassTestCollectionFactory);
        }

        static string ToQuotedString(object? value)
        {
            if (value == null)
                return "null";

            if (value is string stringValue)
                return "\"" + stringValue + "\"";

            // We expect values to be strings here, so hopefully we never hit this
            return value.ToString()!;
        }

        /// <summary>
        /// Gets the type from an attribute constructor, assuming it supports one or both
        /// of the following construtor forms:
        /// - ctor(Type type)
        /// - ctor(string typeName, string assemblyName)
        /// </summary>
        /// <param name="attribute">The attribute to get the type from</param>
        /// <returns>The type, if it exists; <c>null</c>, otherwise</returns>
        public static Type? TypeFromAttributeConstructor(IAttributeInfo attribute)
        {
            Guard.ArgumentNotNull(nameof(attribute), attribute);

            var ctorArgs = attribute.GetConstructorArguments().ToArray();
            if (ctorArgs.Length == 1 && ctorArgs[0] is Type type)
                return type;

            if (ctorArgs.Length == 2 && ctorArgs[0] is string typeName && ctorArgs[1] is string assemblyName)
                return SerializationHelper.GetType(assemblyName, typeName);

            return null;
        }

        /// <summary>
        /// Gets the type from an attribute constructor, assuming it supports one or both
        /// of the following construtor forms:
        /// - ctor(Type type)
        /// - ctor(string typeName, string assemblyName)
        /// </summary>
        /// <param name="attribute">The attribute to get the type from</param>
        /// <returns>The type, if it exists; <c>null</c>, otherwise</returns>
        public static (string? typeName, string? assemblyName) TypeStringsFromAttributeConstructor(IAttributeInfo attribute)
        {
            Guard.ArgumentNotNull(nameof(attribute), attribute);

            var ctorArgs = attribute.GetConstructorArguments().ToArray();
            if (ctorArgs.Length == 1 && ctorArgs[0] is Type type)
                return (type.FullName, type.Assembly.FullName);

            if (ctorArgs.Length == 2 && ctorArgs[0] is string typeName && ctorArgs[1] is string assemblyName)
                return (typeName, assemblyName);

            return (null, null);
        }
    }
}
