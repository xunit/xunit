using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The implementation of <see cref="ITestFrameworkDiscoverer"/> that supports discovery
	/// of unit tests linked against xunit.v3.core.dll.
	/// </summary>
	public class XunitTestFrameworkDiscoverer : TestFrameworkDiscoverer
	{
		static readonly Type XunitTestCaseType = typeof(XunitTestCase);

		readonly string testFrameworkDisplayName;

		/// <summary>
		/// Gets the display name of the xUnit.net v3 test framework.
		/// </summary>
		public static readonly string DisplayName = $"xUnit.net v3 {ThisAssembly.AssemblyInformationalVersion}";

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
		/// </summary>
		/// <param name="assemblyInfo">The test assembly.</param>
		/// <param name="configFileName">The test configuration file.</param>
		/// <param name="sourceProvider">The source information provider.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="collectionFactory">The test collection factory used to look up test collections.</param>
		public XunitTestFrameworkDiscoverer(
			IAssemblyInfo assemblyInfo,
			string? configFileName,
			_ISourceInformationProvider sourceProvider,
			_IMessageSink diagnosticMessageSink,
			IXunitTestCollectionFactory? collectionFactory = null)
				: base(assemblyInfo, configFileName, sourceProvider, diagnosticMessageSink)
		{
			var collectionBehaviorAttribute = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
			var disableParallelization = collectionBehaviorAttribute != null && collectionBehaviorAttribute.GetNamedArgument<bool>("DisableTestParallelization");

			var testAssembly = new TestAssembly(assemblyInfo, configFileName);
			TestAssemblyUniqueID = FactDiscoverer.ComputeUniqueID(testAssembly);

			TestCollectionFactory =
				collectionFactory
				?? ExtensibilityPointFactory.GetXunitTestCollectionFactory(diagnosticMessageSink, collectionBehaviorAttribute, testAssembly)
				?? new CollectionPerClassTestCollectionFactory(testAssembly, diagnosticMessageSink);

			testFrameworkDisplayName = $"{DisplayName} [{TestCollectionFactory.DisplayName}, {(disableParallelization ? "non-parallel" : "parallel")}]";
		}

		/// <summary>
		/// Gets the mapping dictionary of fact attribute type to discoverer type. The key
		/// is a type that is (or derives from) <see cref="FactAttribute"/>; the value is the
		/// discoverer type, if known; <c>null</c> if not.
		/// </summary>
		protected Dictionary<Type, Type?> DiscovererTypeCache { get; } = new Dictionary<Type, Type?>();

		/// <inheritdoc/>
		public override string TestAssemblyUniqueID { get; }

		/// <summary>
		/// Gets the test collection factory that makes test collections.
		/// </summary>
		public IXunitTestCollectionFactory TestCollectionFactory { get; private set; }

		/// <inheritdoc/>
		public override string TestFrameworkDisplayName => testFrameworkDisplayName;

		/// <inheritdoc/>
		protected internal override ITestClass CreateTestClass(ITypeInfo @class) =>
			new TestClass(TestCollectionFactory.Get(@class), @class);

		internal ITestClass CreateTestClass(
			ITypeInfo @class,
			Guid testCollectionUniqueId)
		{
			// This method is called for special fact deserialization, to ensure that the test collection unique
			// ID lines up with the ones that will be deserialized through normal mechanisms.
			var discoveredTestCollection = TestCollectionFactory.Get(@class);
			var testCollection = new TestCollection(discoveredTestCollection.TestAssembly, discoveredTestCollection.CollectionDefinition, discoveredTestCollection.DisplayName, testCollectionUniqueId);
			return new TestClass(testCollection, @class);
		}

		/// <summary>
		/// Finds the tests on a test method.
		/// </summary>
		/// <param name="testCollectionUniqueID">The test collection unique ID.</param>
		/// <param name="testClassUniqueID">The test class unique ID.</param>
		/// <param name="testMethod">The test method.</param>
		/// <param name="messageBus">The message bus to report discovery messages to.</param>
		/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
		/// <returns>Return <c>true</c> to continue test discovery, <c>false</c>, otherwise.</returns>
		protected internal virtual bool FindTestsForMethod(
			string testCollectionUniqueID,
			string? testClassUniqueID,
			_ITestMethod testMethod,
			IMessageBus messageBus,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			var includeSerialization = discoveryOptions.IncludeSerializationOrDefault();
			var includeSourceInformation = discoveryOptions.IncludeSourceInformationOrDefault();
			var testMethodUniqueID = FactDiscoverer.ComputeUniqueID(testClassUniqueID, testMethod);
			var factAttributes = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).CastOrToList();
			if (factAttributes.Count > 1)
			{
				var message = $"Test method '{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}' has multiple [Fact]-derived attributes";
				var testCase = new ExecutionErrorTestCase(TestAssemblyUniqueID, testCollectionUniqueID, testClassUniqueID, testMethodUniqueID, DiagnosticMessageSink, TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod, message);
				return ReportDiscoveredTestCase(testCollectionUniqueID, testClassUniqueID, testMethodUniqueID, testCase, includeSerialization, includeSourceInformation, messageBus);
			}

			var factAttribute = factAttributes.FirstOrDefault();
			if (factAttribute == null)
				return true;

			var factAttributeType = (factAttribute as IReflectionAttributeInfo)?.Attribute.GetType();

			Type? discovererType = null;
			if (factAttributeType == null || !DiscovererTypeCache.TryGetValue(factAttributeType, out discovererType))
			{
				var testCaseDiscovererAttribute = factAttribute.GetCustomAttributes(typeof(XunitTestCaseDiscovererAttribute)).FirstOrDefault();
				if (testCaseDiscovererAttribute != null)
					discovererType = ExtensibilityPointFactory.TypeFromAttributeConstructor(testCaseDiscovererAttribute);

				if (factAttributeType != null)
					DiscovererTypeCache[factAttributeType] = discovererType;
			}

			if (discovererType == null)
				return true;

			var discoverer = GetDiscoverer(discovererType);
			if (discoverer == null)
				return true;

			foreach (var testCase in discoverer.Discover(discoveryOptions, testMethod, factAttribute))
				if (!ReportDiscoveredTestCase(testCollectionUniqueID, testClassUniqueID, testMethodUniqueID, testCase, includeSerialization, includeSourceInformation, messageBus))
					return false;

			return true;
		}

		/// <inheritdoc/>
		protected override bool FindTestsForType(
			string testCollectionUniqueID,
			string? testClassUniqueID,
			ITestClass testClass,
			IMessageBus messageBus,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			foreach (var method in testClass.Class.GetMethods(true))
			{
				var testMethod = new TestMethod(testClass, method);
				if (!FindTestsForMethod(testCollectionUniqueID, testClassUniqueID, testMethod, messageBus, discoveryOptions))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the test case discover instance for the given discoverer type. The instances are cached
		/// and reused, since they should not be stateful.
		/// </summary>
		/// <param name="discovererType">The discoverer type.</param>
		/// <returns>Returns the test case discoverer instance, if known; may return <c>null</c>
		/// when an error occurs (which is logged to the diagnostic message sink).</returns>
		protected IXunitTestCaseDiscoverer? GetDiscoverer(Type discovererType)
		{
			try
			{
				return ExtensibilityPointFactory.GetXunitTestCaseDiscoverer(DiagnosticMessageSink, discovererType);
			}
			catch (Exception ex)
			{
				DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Discoverer type '{discovererType.FullName}' could not be created or does not implement IXunitTestCaseDiscoverer: {ex.Unwrap()}" });
				return null;
			}
		}

		/// <inheritdoc/>
		protected override string Serialize(_ITestCase testCase)
		{
			Guard.ArgumentNotNull(nameof(testCase), testCase);

			if (testCase.GetType() == XunitTestCaseType)
			{
				var xunitTestCase = (XunitTestCase)testCase;
				var className = testCase.TestMethod?.TestClass?.Class?.Name;
				var methodName = testCase.TestMethod?.Method?.Name;
				if (className != null && methodName != null && (xunitTestCase.TestMethodArguments == null || xunitTestCase.TestMethodArguments.Length == 0))
					return $":F:{className.Replace(":", "::")}:{methodName.Replace(":", "::")}:{(int)xunitTestCase.DefaultMethodDisplay}:{(int)xunitTestCase.DefaultMethodDisplayOptions}:{xunitTestCase.TestMethod.TestClass.TestCollection.UniqueID:N}";
			}

			return base.Serialize(testCase);
		}
	}
}
