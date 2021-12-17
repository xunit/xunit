using System;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Represents a factory for the types used for extensibility throughout the system.
	/// </summary>
	public static class ExtensibilityPointFactory
	{
		static object? CreateInstance(
			Type type,
			object?[]? ctorArgs,
			_IMessageSink? diagnosticMessageSink = null)
		{
			ctorArgs ??= new object[0];
			object? result = null;

			try
			{
				// TODO: For legacy reasons, we support constructors that accept diagnostic message sink as a first parameter.
				var ctorArgsWithMessageSink = ctorArgs.Concat(new object[] { diagnosticMessageSink ?? TestContext.Current?.DiagnosticMessageSink ?? _NullMessageSink.Instance }).ToArray();
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
					TestContext.Current?.SendDiagnosticMessage(
						"Could not find constructor for '{0}' with arguments type(s): {1}",
						type.FullName,
						string.Join(", ", ctorArgs.Select(a => a == null ? "(unknown)" : a.GetType().FullName))
					);
					throw;
				}
			}

			return result;
		}

		/// <summary>
		/// Gets an instance of the given type, casting it to <typeparamref name="TInterface"/>, using the provided
		/// constructor arguments. Support for legacy constructors which have a first parameter of type <see cref="_IMessageSink"/>
		/// are still supported, though it's strongly encouraged that classes use <see cref="TestContext.SendDiagnosticMessage(string)"/>
		/// or <see cref="TestContext.SendDiagnosticMessage(string, object?[])"/> instead.
		/// </summary>
		/// <typeparam name="TInterface">The interface type.</typeparam>
		/// <param name="type">The implementation type.</param>
		/// <param name="ctorArgs">The constructor arguments. Since diagnostic message sinks are optional,
		/// the code first looks for a type that takes the given arguments plus the message sink, and only
		/// falls back to the message sink-less constructor if none was found.</param>
		/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="_DiagnosticMessage"/> messages.
		/// If one isn't provided, but the constructor requires one, then a null message sink will be provided.</param>
		/// <returns>The instance of the type.</returns>
		public static TInterface? Get<TInterface>(
			Type type,
			object?[]? ctorArgs = null,
			_IMessageSink? diagnosticMessageSink = null)
				where TInterface : class
		{
			Guard.ArgumentNotNull(type);

			return (TInterface?)CreateInstance(type, ctorArgs, diagnosticMessageSink);
		}

		/// <summary>
		/// Gets a data discoverer.
		/// </summary>
		/// <param name="discovererType">The discoverer type</param>
		public static IDataDiscoverer? GetDataDiscoverer(Type discovererType) =>
			Get<IDataDiscoverer>(discovererType);

		/// <summary>
		/// Gets a data discoverer, as specified in a reflected <see cref="DataDiscovererAttribute"/>.
		/// </summary>
		/// <param name="dataDiscovererAttribute">The data discoverer attribute</param>
		/// <returns>The data discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
		public static IDataDiscoverer? GetDataDiscoverer(_IAttributeInfo dataDiscovererAttribute)
		{
			Guard.ArgumentNotNull(dataDiscovererAttribute);

			var discovererType = TypeFromAttributeConstructor(dataDiscovererAttribute);
			if (discovererType == null)
				return null;

			return GetDataDiscoverer(discovererType);
		}

		/// <summary>
		/// Gets a test case orderer.
		/// </summary>
		/// <param name="ordererType">The test case orderer type</param>
		public static ITestCaseOrderer? GetTestCaseOrderer(Type ordererType) =>
			Get<ITestCaseOrderer>(ordererType);

		/// <summary>
		/// Gets a test case orderer, as specified in a reflected <see cref="TestCaseOrdererAttribute"/>.
		/// </summary>
		/// <param name="testCaseOrdererAttribute">The test case orderer attribute.</param>
		/// <returns>The test case orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
		public static ITestCaseOrderer? GetTestCaseOrderer(_IAttributeInfo testCaseOrdererAttribute)
		{
			Guard.ArgumentNotNull(testCaseOrdererAttribute);

			var ordererType = TypeFromAttributeConstructor(testCaseOrdererAttribute);
			if (ordererType == null)
				return null;

			return GetTestCaseOrderer(ordererType);
		}

		/// <summary>
		/// Gets a test collection orderer.
		/// </summary>
		/// <param name="ordererType">The test collection orderer type</param>
		public static ITestCollectionOrderer? GetTestCollectionOrderer(Type ordererType) =>
			Get<ITestCollectionOrderer>(ordererType);

		/// <summary>
		/// Gets a test collection orderer, as specified in a reflected <see cref="TestCollectionOrdererAttribute"/>.
		/// </summary>
		/// <param name="testCollectionOrdererAttribute">The test collection orderer attribute.</param>
		/// <returns>The test collection orderer, if the type is loadable; <c>null</c>, otherwise.</returns>
		public static ITestCollectionOrderer? GetTestCollectionOrderer(_IAttributeInfo testCollectionOrdererAttribute)
		{
			Guard.ArgumentNotNull(testCollectionOrdererAttribute);

			var ordererType = TypeFromAttributeConstructor(testCollectionOrdererAttribute);
			if (ordererType == null)
				return null;

			return GetTestCollectionOrderer(ordererType);
		}

		/// <summary>
		/// Gets the test framework object for the given test assembly.
		/// </summary>
		/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="internalDiagnosticMessageSink">The optional message sink which receives internal <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="testAssembly">The test assembly to get the test framework for</param>
		/// <returns>The test framework object</returns>
		public static _ITestFramework GetTestFramework(
			_IMessageSink? diagnosticMessageSink,
			_IMessageSink? internalDiagnosticMessageSink,
			_IAssemblyInfo testAssembly)
		{
			_ITestFramework result;

			var testFrameworkType = GetTestFrameworkType(diagnosticMessageSink, testAssembly);
			if (!typeof(_ITestFramework).IsAssignableFrom(testFrameworkType))
			{
				if (diagnosticMessageSink != null)
					diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Test framework type '{testFrameworkType.FullName}' does not implement '{typeof(_ITestFramework).FullName}'; falling back to '{typeof(XunitTestFramework).FullName}'" });

				testFrameworkType = typeof(XunitTestFramework);
			}

			try
			{
				var ctorWithSink = default(ConstructorInfo);
				var ctorWithSinks = default(ConstructorInfo);
				var messageSinkType = typeof(_IMessageSink);

				foreach (var ctor in testFrameworkType.GetConstructors())
				{
					var paramInfos = ctor.GetParameters();

					// Legacy style, with just diagnostic message sink
					if (paramInfos.Length == 1 && paramInfos[0].ParameterType == messageSinkType)
						ctorWithSink = ctor;

					// v3 style, with both diagnostic message sink and internal diagnostic message sink
					if (paramInfos.Length == 2 && paramInfos[0].ParameterType == messageSinkType && paramInfos[1].ParameterType == messageSinkType)
						ctorWithSinks = ctor;

					if (ctorWithSink != null && ctorWithSinks != null)
						break;
				}

				if (ctorWithSinks != null)
					result = (_ITestFramework)ctorWithSinks.Invoke(new object?[] { diagnosticMessageSink, internalDiagnosticMessageSink });
				else if (ctorWithSink != null)
					result = (_ITestFramework)ctorWithSink.Invoke(new object?[] { diagnosticMessageSink });
				else
					result = (_ITestFramework)Activator.CreateInstance(testFrameworkType)!;
			}
			catch (Exception ex)
			{
				if (diagnosticMessageSink != null)
					diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Exception thrown during test framework construction; falling back to default test framework: {ex.Unwrap()}" });

				result = new XunitTestFramework(diagnosticMessageSink, internalDiagnosticMessageSink);
			}

			return result;
		}

		static Type GetTestFrameworkType(
			_IMessageSink? diagnosticMessageSink,
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

						if (diagnosticMessageSink != null)
							diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Unable to create custom test framework discoverer type from '{testFrameworkAttr.GetType().FullName}'" });
					}
					else
					{
						if (diagnosticMessageSink != null)
							diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = "Assembly-level test framework attribute was not decorated with [TestFrameworkDiscoverer]" });
					}
				}
			}
			catch (Exception ex)
			{
				if (diagnosticMessageSink != null)
					diagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Exception thrown during test framework discoverer construction: {ex.Unwrap()}" });
			}

			return typeof(XunitTestFramework);
		}

		/// <summary>
		/// Gets a test framework discoverer.
		/// </summary>
		/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="frameworkType">The test framework type discoverer type</param>
		public static ITestFrameworkTypeDiscoverer? GetTestFrameworkTypeDiscoverer(
			_IMessageSink? diagnosticMessageSink,
			Type frameworkType) =>
				Get<ITestFrameworkTypeDiscoverer>(frameworkType, diagnosticMessageSink: diagnosticMessageSink);

		/// <summary>
		/// Gets a test framework discoverer, as specified in a reflected <see cref="TestFrameworkDiscovererAttribute"/>.
		/// </summary>
		/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="testFrameworkDiscovererAttribute">The test framework discoverer attribute</param>
		public static ITestFrameworkTypeDiscoverer? GetTestFrameworkTypeDiscoverer(
			_IMessageSink? diagnosticMessageSink,
			_IAttributeInfo testFrameworkDiscovererAttribute)
		{
			Guard.ArgumentNotNull(testFrameworkDiscovererAttribute);

			var testFrameworkDiscovererType = TypeFromAttributeConstructor(testFrameworkDiscovererAttribute);
			if (testFrameworkDiscovererType == null)
				return null;

			return GetTestFrameworkTypeDiscoverer(diagnosticMessageSink, testFrameworkDiscovererType);
		}

		/// <summary>
		/// Gets a trait discoverer.
		/// </summary>
		/// <param name="traitDiscovererType">The trait discoverer type</param>
		public static ITraitDiscoverer? GetTraitDiscoverer(Type traitDiscovererType) =>
			Get<ITraitDiscoverer>(traitDiscovererType);

		/// <summary>
		/// Gets a trait discoverer, as specified in a reflected <see cref="TraitDiscovererAttribute"/>.
		/// </summary>
		/// <param name="traitDiscovererAttribute">The trait discoverer attribute.</param>
		/// <returns>The trait discoverer, if the type is loadable; <c>null</c>, otherwise.</returns>
		public static ITraitDiscoverer? GetTraitDiscoverer(_IAttributeInfo traitDiscovererAttribute)
		{
			Guard.ArgumentNotNull(traitDiscovererAttribute);

			var discovererType = TypeFromAttributeConstructor(traitDiscovererAttribute);
			if (discovererType == null)
				return null;

			return GetTraitDiscoverer(discovererType);
		}

		/// <summary>
		/// Gets an xUnit.net v3 test discoverer.
		/// </summary>
		/// <param name="testCaseDiscovererType">The test case discoverer type</param>
		public static IXunitTestCaseDiscoverer? GetXunitTestCaseDiscoverer(Type testCaseDiscovererType) =>
			Get<IXunitTestCaseDiscoverer>(testCaseDiscovererType);

		/// <summary>
		/// Gets an xUnit.net v3 test collection factory.
		/// </summary>
		/// <param name="testCollectionFactoryType">The test collection factory type</param>
		/// <param name="testAssembly">The test assembly under test</param>
		public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
			Type testCollectionFactoryType,
			_ITestAssembly testAssembly) =>
				Get<IXunitTestCollectionFactory>(testCollectionFactoryType, new object[] { testAssembly });

		/// <summary>
		/// Gets an xUnit.net v3 test collection factory, as specified in a reflected <see cref="CollectionBehaviorAttribute"/>.
		/// </summary>
		/// <param name="collectionBehaviorAttribute">The collection behavior attribute.</param>
		/// <param name="testAssembly">The test assembly.</param>
		/// <returns>The collection factory.</returns>
		public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
			_IAttributeInfo? collectionBehaviorAttribute,
			_ITestAssembly testAssembly)
		{
			try
			{
				var testCollectionFactoryType = GetTestCollectionFactoryType(collectionBehaviorAttribute);
				return GetXunitTestCollectionFactory(testCollectionFactoryType, testAssembly);
			}
			catch
			{
				return null;
			}
		}

		static Type GetTestCollectionFactoryType(_IAttributeInfo? collectionBehaviorAttribute)
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
					TestContext.Current?.SendDiagnosticMessage("Test collection factory type '{0}' does not implement IXunitTestCollectionFactory", factoryType.FullName);
				else
					return factoryType;
			}
			else if (ctorArgs.Count == 2 && ctorArgs[0] is string typeName && ctorArgs[1] is string assemblyName)
			{
				var result = SerializationHelper.GetType(assemblyName, typeName);
				if (result == null)
					TestContext.Current?.SendDiagnosticMessage("Unable to create test collection factory type '{0}, {1}'", assemblyName, typeName);
				else
				{
					if (!typeof(IXunitTestCollectionFactory).IsAssignableFrom(result))
						TestContext.Current?.SendDiagnosticMessage("Test collection factory type '{0}, {1}' does not implement IXunitTestCollectionFactory", assemblyName, typeName);
					else
						return result;
				}
			}
			else
				TestContext.Current?.SendDiagnosticMessage("[CollectionBehavior({0}, {1})] cannot have null argument values", ToQuotedString(ctorArgs[0]), ToQuotedString(ctorArgs[1]));

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
			Guard.ArgumentNotNull(attribute);

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
			Guard.ArgumentNotNull(attribute);

			var ctorArgs = attribute.GetConstructorArguments().ToArray();
			if (ctorArgs.Length == 1 && ctorArgs[0] is Type type)
				return (type.FullName, type.Assembly.FullName);

			if (ctorArgs.Length == 2 && ctorArgs[0] is string typeName && ctorArgs[1] is string assemblyName)
				return (typeName, assemblyName);

			return (null, null);
		}
	}
}
