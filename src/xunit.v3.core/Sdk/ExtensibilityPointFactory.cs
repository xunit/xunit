using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Represents a caching factory for the types used for extensibility throughout the system.
	/// </summary>
	public static class ExtensibilityPointFactory
	{
		static DisposalTracker disposalTracker = new DisposalTracker();
		static readonly ConcurrentDictionary<(Type type, _IMessageSink diagnosticMessageSink), object?> instances = new ConcurrentDictionary<(Type type, _IMessageSink diagnosticMessageSink), object?>();

		static object? CreateInstance(
			_IMessageSink diagnosticMessageSink,
			Type type,
			object?[]? ctorArgs)
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
					diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Could not find constructor for '{type.FullName}' with arguments type(s): {(string.Join(", ", ctorArgs.Select(a => a == null ? "(unknown)" : a.GetType().FullName)))}" });
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
		public static async ValueTask DisposeAsync()
		{
			instances.Clear();
			await disposalTracker.DisposeAsync();
			disposalTracker = new DisposalTracker();
		}

		/// <summary>
		/// Gets an instance of the given type, casting it to <typeparamref name="TInterface"/>, using the provided
		/// constructor arguments. There is a single instance of a given type that is cached and reused,
		/// so classes retrieved from this factory must be stateless and thread-safe.
		/// </summary>
		/// <typeparam name="TInterface">The interface type.</typeparam>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="type">The implementation type.</param>
		/// <param name="ctorArgs">The constructor arguments. Since diagnostic message sinks are optional,
		/// the code first looks for a type that takes the given arguments plus the message sink, and only
		/// falls back to the message sink-less constructor if none was found.</param>
		/// <returns>The instance of the type.</returns>
		public static TInterface? Get<TInterface>(
			_IMessageSink diagnosticMessageSink,
			Type type,
			object?[]? ctorArgs = null)
				where TInterface : class
		{
			Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
			Guard.ArgumentNotNull(nameof(type), type);

			return (TInterface?)instances.GetOrAdd((type, diagnosticMessageSink), () => CreateInstance(diagnosticMessageSink, type, ctorArgs));
		}

		/// <summary>
		/// Gets a data discoverer.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="discovererType">The discoverer type</param>
		public static IDataDiscoverer? GetDataDiscoverer(
			_IMessageSink diagnosticMessageSink,
			Type discovererType) =>
				Get<IDataDiscoverer>(diagnosticMessageSink, discovererType);

		/// <summary>
		/// Gets a data discoverer, as specified in a reflected <see cref="DataDiscovererAttribute"/>.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="dataDiscovererAttribute">The data discoverer attribute</param>
		/// <returns>The data discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
		public static IDataDiscoverer? GetDataDiscoverer(
			_IMessageSink diagnosticMessageSink,
			_IAttributeInfo dataDiscovererAttribute)
		{
			Guard.ArgumentNotNull(nameof(dataDiscovererAttribute), dataDiscovererAttribute);

			var discovererType = TypeFromAttributeConstructor(dataDiscovererAttribute);
			if (discovererType == null)
				return null;

			return GetDataDiscoverer(diagnosticMessageSink, discovererType);
		}

		/// <summary>
		/// Gets a test case orderer.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="ordererType">The test case orderer type</param>
		public static ITestCaseOrderer? GetTestCaseOrderer(
			_IMessageSink diagnosticMessageSink,
			Type ordererType) =>
				Get<ITestCaseOrderer>(diagnosticMessageSink, ordererType);

		/// <summary>
		/// Gets a test case orderer, as specified in a reflected <see cref="TestCaseOrdererAttribute"/>.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="testCaseOrdererAttribute">The test case orderer attribute.</param>
		/// <returns>The test case orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
		public static ITestCaseOrderer? GetTestCaseOrderer(
			_IMessageSink diagnosticMessageSink,
			_IAttributeInfo testCaseOrdererAttribute)
		{
			Guard.ArgumentNotNull(nameof(testCaseOrdererAttribute), testCaseOrdererAttribute);

			var ordererType = TypeFromAttributeConstructor(testCaseOrdererAttribute);
			if (ordererType == null)
				return null;

			return GetTestCaseOrderer(diagnosticMessageSink, ordererType);
		}

		/// <summary>
		/// Gets a test collection orderer.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="ordererType">The test collection orderer type</param>
		public static ITestCollectionOrderer? GetTestCollectionOrderer(
			_IMessageSink diagnosticMessageSink,
			Type ordererType) =>
				Get<ITestCollectionOrderer>(diagnosticMessageSink, ordererType);

		/// <summary>
		/// Gets a test collection orderer, as specified in a reflected <see cref="TestCollectionOrdererAttribute"/>.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="testCollectionOrdererAttribute">The test collection orderer attribute.</param>
		/// <returns>The test collection orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
		public static ITestCollectionOrderer? GetTestCollectionOrderer(
			_IMessageSink diagnosticMessageSink,
			_IAttributeInfo testCollectionOrdererAttribute)
		{
			Guard.ArgumentNotNull(nameof(testCollectionOrdererAttribute), testCollectionOrdererAttribute);

			var ordererType = TypeFromAttributeConstructor(testCollectionOrdererAttribute);
			if (ordererType == null)
				return null;

			return GetTestCollectionOrderer(diagnosticMessageSink, ordererType);
		}

		/// <summary>
		/// Gets the test framework object for the given test assembly.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="testAssembly">The test assembly to get the test framework for</param>
		/// <param name="sourceInformationProvider">The optional source information provider</param>
		/// <returns>The test framework object</returns>
		public static _ITestFramework GetTestFramework(
			_IMessageSink diagnosticMessageSink,
			_IAssemblyInfo testAssembly,
			_ISourceInformationProvider? sourceInformationProvider = null)
		{
			// TODO Guard
			_ITestFramework result;

			var testFrameworkType = GetTestFrameworkType(diagnosticMessageSink, testAssembly);
			if (!typeof(_ITestFramework).IsAssignableFrom(testFrameworkType))
			{
				diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Test framework type '{testFrameworkType.FullName}' does not implement '{typeof(_ITestFramework).FullName}'; falling back to '{typeof(XunitTestFramework).FullName}'" });
				testFrameworkType = typeof(XunitTestFramework);
			}

			try
			{
				var ctorWithSink =
					testFrameworkType
						.GetConstructors()
						.FirstOrDefault(ctor =>
						{
							var paramInfos = ctor.GetParameters();
							return paramInfos.Length == 1 && paramInfos[0].ParameterType == typeof(_IMessageSink);
						});

				if (ctorWithSink != null)
					result = (_ITestFramework)ctorWithSink.Invoke(new object[] { diagnosticMessageSink });
				else
					result = (_ITestFramework)Activator.CreateInstance(testFrameworkType)!;
			}
			catch (Exception ex)
			{
				diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Exception thrown during test framework construction: {ex.Unwrap()}" });
				result = new XunitTestFramework(diagnosticMessageSink);
			}

			if (sourceInformationProvider != null)
				result.SourceInformationProvider = sourceInformationProvider;

			return result;
		}

		static Type GetTestFrameworkType(
			_IMessageSink diagnosticMessageSink,
			_IAssemblyInfo testAssembly)
		{
			try
			{
				var testFrameworkAttr = testAssembly.GetCustomAttributes(typeof(ITestFrameworkAttribute)).FirstOrDefault();
				if (testFrameworkAttr != null)
				{
					var discovererAttr = testFrameworkAttr.GetCustomAttributes(typeof(TestFrameworkDiscovererAttribute)).FirstOrDefault();
					if (discovererAttr != null)
					{
						var discoverer = GetTestFrameworkTypeDiscoverer(diagnosticMessageSink, discovererAttr);
						if (discoverer != null)
						{
							var discovererType = discoverer.GetTestFrameworkType(testFrameworkAttr);
							if (discovererType != null)
								return discovererType;
						}

						diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Unable to create custom test framework discoverer type from '{testFrameworkAttr.GetType().FullName}'" });
					}
					else
					{
						diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = "Assembly-level test framework attribute was not decorated with [TestFrameworkDiscoverer]" });
					}
				}
			}
			catch (Exception ex)
			{
				diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Exception thrown during test framework discoverer construction: {ex.Unwrap()}" });
			}

			return typeof(XunitTestFramework);
		}

		/// <summary>
		/// Gets a test framework discoverer.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="frameworkType">The test framework type discoverer type</param>
		public static ITestFrameworkTypeDiscoverer? GetTestFrameworkTypeDiscoverer(
			_IMessageSink diagnosticMessageSink,
			Type frameworkType) =>
				Get<ITestFrameworkTypeDiscoverer>(diagnosticMessageSink, frameworkType);

		/// <summary>
		/// Gets a test framework discoverer, as specified in a reflected <see cref="TestFrameworkDiscovererAttribute"/>.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="testFrameworkDiscovererAttribute">The test framework discoverer attribute</param>
		public static ITestFrameworkTypeDiscoverer? GetTestFrameworkTypeDiscoverer(
			_IMessageSink diagnosticMessageSink,
			_IAttributeInfo testFrameworkDiscovererAttribute)
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
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="traitDiscovererType">The trait discoverer type</param>
		public static ITraitDiscoverer? GetTraitDiscoverer(
			_IMessageSink diagnosticMessageSink,
			Type traitDiscovererType) =>
				Get<ITraitDiscoverer>(diagnosticMessageSink, traitDiscovererType);

		/// <summary>
		/// Gets a trait discoverer, as specified in a reflected <see cref="TraitDiscovererAttribute"/>.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="traitDiscovererAttribute">The trait discoverer attribute.</param>
		/// <returns>The trait discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
		public static ITraitDiscoverer? GetTraitDiscoverer(
			_IMessageSink diagnosticMessageSink,
			_IAttributeInfo traitDiscovererAttribute)
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
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="testCaseDiscovererType">The test case discoverer type</param>
		public static IXunitTestCaseDiscoverer? GetXunitTestCaseDiscoverer(
			_IMessageSink diagnosticMessageSink,
			Type testCaseDiscovererType) =>
				Get<IXunitTestCaseDiscoverer>(diagnosticMessageSink, testCaseDiscovererType);

		/// <summary>
		/// Gets an xUnit.net v3 test collection factory.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="testCollectionFactoryType">The test collection factory type</param>
		/// <param name="testAssembly">The test assembly under test</param>
		public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
			_IMessageSink diagnosticMessageSink,
			Type testCollectionFactoryType,
			_ITestAssembly testAssembly) =>
				Get<IXunitTestCollectionFactory>(diagnosticMessageSink, testCollectionFactoryType, new object[] { testAssembly });

		/// <summary>
		/// Gets an xUnit.net v3 test collection factory, as specified in a reflected <see cref="CollectionBehaviorAttribute"/>.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="collectionBehaviorAttribute">The collection behavior attribute.</param>
		/// <param name="testAssembly">The test assembly.</param>
		/// <returns>The collection factory.</returns>
		public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
			_IMessageSink diagnosticMessageSink,
			_IAttributeInfo? collectionBehaviorAttribute,
			_ITestAssembly testAssembly)
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

		static Type GetTestCollectionFactoryType(
			_IMessageSink diagnosticMessageSink,
			_IAttributeInfo? collectionBehaviorAttribute)
		{
			if (collectionBehaviorAttribute == null)
				return typeof(CollectionPerClassTestCollectionFactory);

			var ctorArgs = collectionBehaviorAttribute.GetConstructorArguments().CastOrToReadOnlyList();
			if (ctorArgs.Count == 0)
				return typeof(CollectionPerClassTestCollectionFactory);

			if (ctorArgs.Count == 1 && ctorArgs[0] is CollectionBehavior collectionBehavior)
			{
				if (collectionBehavior == CollectionBehavior.CollectionPerAssembly)
					return typeof(CollectionPerAssemblyTestCollectionFactory);

				return typeof(CollectionPerClassTestCollectionFactory);
			}
			else if (ctorArgs.Count == 1 && ctorArgs[0] is Type factoryType)
			{
				if (!typeof(IXunitTestCollectionFactory).IsAssignableFrom(factoryType))
					diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Test collection factory type '{factoryType.FullName}' does not implement IXunitTestCollectionFactory" });
				else
					return factoryType;
			}
			else if (ctorArgs.Count == 2 && ctorArgs[0] is string typeName && ctorArgs[1] is string assemblyName)
			{
				var result = SerializationHelper.GetType(assemblyName, typeName);
				if (result == null)
					diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Unable to create test collection factory type '{assemblyName}, {typeName}'" });
				else
				{
					if (!typeof(IXunitTestCollectionFactory).IsAssignableFrom(result))
						diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Test collection factory type '{assemblyName}, {typeName}' does not implement IXunitTestCollectionFactory" });
					else
						return result;
				}
			}
			else
				diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"[CollectionBehavior({ToQuotedString(ctorArgs[0])}, {ToQuotedString(ctorArgs[1])})] cannot have null argument values" });

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
		public static Type? TypeFromAttributeConstructor(_IAttributeInfo attribute)
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
		public static (string? typeName, string? assemblyName) TypeStringsFromAttributeConstructor(_IAttributeInfo attribute)
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
