using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="_ITestFrameworkDiscoverer"/> that supports discovery
/// of unit tests linked against xunit.v3.core.dll.
/// </summary>
public class XunitTestFrameworkDiscoverer : TestFrameworkDiscoverer<IXunitTestCase>
{
	static readonly Lazy<_IReflectionAttributeInfo> defaultFactAttribute;
	readonly _ITestAssembly testAssembly;

	/// <summary>
	/// Gets the display name of the xUnit.net v3 test framework.
	/// </summary>
	public static readonly string DisplayName = string.Format(CultureInfo.CurrentCulture, "xUnit.net v3 {0}", ThisAssembly.AssemblyInformationalVersion);

	static XunitTestFrameworkDiscoverer()
	{
		[Fact]
		static _IReflectionAttributeInfo EmptyFact() =>
			Reflector.Wrap(CustomAttributeData.GetCustomAttributes(MethodBase.GetCurrentMethod()!).Single(cad => cad.AttributeType == typeof(FactAttribute)));

		defaultFactAttribute = new(() => EmptyFact());
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
	/// </summary>
	/// <param name="assemblyInfo">The test assembly.</param>
	/// <param name="configFileName">The test configuration file.</param>
	/// <param name="collectionFactory">The test collection factory used to look up test collections.</param>
	public XunitTestFrameworkDiscoverer(
		_IAssemblyInfo assemblyInfo,
		string? configFileName,
		IXunitTestCollectionFactory? collectionFactory = null)
			: base(assemblyInfo)
	{
		var collectionBehaviorAttribute = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
		var disableParallelization = collectionBehaviorAttribute is not null && collectionBehaviorAttribute.GetNamedArgument<bool>(nameof(CollectionBehaviorAttribute.DisableTestParallelization));

		testAssembly = new TestAssembly(assemblyInfo, configFileName);
		TestCollectionFactory =
			collectionFactory
				?? ExtensibilityPointFactory.GetXunitTestCollectionFactory(collectionBehaviorAttribute, testAssembly)
				?? new CollectionPerClassTestCollectionFactory(testAssembly);

		TestFrameworkDisplayName = string.Format(
			CultureInfo.CurrentCulture,
			"{0} [{1}, {2}]",
			DisplayName,
			TestCollectionFactory.DisplayName,
			disableParallelization ? "non-parallel" : "parallel"
		);
	}

	/// <summary>
	/// Gets the mapping dictionary of fact attribute type to discoverer type. The key
	/// is a type that is (or derives from) <see cref="FactAttribute"/>; the value is the
	/// discoverer type, if known; <c>null</c> if not.
	/// </summary>
	protected Dictionary<Type, Type?> DiscovererTypeCache { get; } = new();

	/// <inheritdoc/>
	public override _ITestAssembly TestAssembly => testAssembly;

	/// <summary>
	/// Gets the test collection factory that makes test collections.
	/// </summary>
	public IXunitTestCollectionFactory TestCollectionFactory { get; private set; }

	/// <inheritdoc/>
	public override string TestFrameworkDisplayName { get; }

	/// <inheritdoc/>
	protected override ValueTask<_ITestClass> CreateTestClass(_ITypeInfo @class) =>
		new(new TestClass(TestCollectionFactory.Get(@class), @class));

	/// <summary>
	/// Finds the tests on a test method.
	/// </summary>
	/// <param name="testMethod">The test method.</param>
	/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
	/// <param name="discoveryCallback">The callback that is called for each discovered test case.</param>
	/// <returns>Return <c>true</c> to continue test discovery, <c>false</c>, otherwise.</returns>
	protected virtual async ValueTask<bool> FindTestsForMethod(
		_ITestMethod testMethod,
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<IXunitTestCase, ValueTask<bool>> discoveryCallback)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(discoveryCallback);

		var factAttributes = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).CastOrToList();
		if (factAttributes.Count > 1)
		{
			var message = string.Format(CultureInfo.CurrentCulture, "Test method '{0}.{1}' has multiple [Fact]-derived attributes", testMethod.TestClass.Class.Name, testMethod.Method.Name);
			var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttributes[0]);
			await using var testCase = new ExecutionErrorTestCase(details.ResolvedTestMethod, details.TestCaseDisplayName, details.UniqueID, message);
			return await discoveryCallback(testCase);
		}

		var factAttribute = factAttributes.FirstOrDefault();
		if (factAttribute is null)
			return true;

		var factAttributeType = (factAttribute as _IReflectionAttributeInfo)?.Attribute.GetType();

		Type? discovererType = null;
		if (factAttributeType is null || !DiscovererTypeCache.TryGetValue(factAttributeType, out discovererType))
		{
			var testCaseDiscovererAttribute = factAttribute.GetCustomAttributes(typeof(XunitTestCaseDiscovererAttribute)).FirstOrDefault();
			if (testCaseDiscovererAttribute is not null)
				discovererType = ExtensibilityPointFactory.TypeFromAttributeConstructor(testCaseDiscovererAttribute);

			if (factAttributeType is not null)
				DiscovererTypeCache[factAttributeType] = discovererType;
		}

		if (discovererType is null)
			return true;

		var discoverer = GetDiscoverer(discovererType);
		if (discoverer is null)
			return true;

		foreach (var testCase in await discoverer.Discover(discoveryOptions, testMethod, factAttribute))
			if (!await discoveryCallback(testCase))
				return false;

		return true;
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> FindTestsForType(
		_ITestClass testClass,
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<IXunitTestCase, ValueTask<bool>> discoveryCallback)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(discoveryCallback);

		foreach (var method in testClass.Class.GetMethods(includePrivateMethods: true))
		{
			var testMethod = new TestMethod(testClass, method);

			try
			{
				if (!await FindTestsForMethod(testMethod, discoveryOptions, discoveryCallback))
					return false;
			}
			catch (Exception ex)
			{
				var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, defaultFactAttribute.Value);
				await using var errorTestCase = new ExecutionErrorTestCase(
					testMethod,
					details.TestCaseDisplayName,
					details.UniqueID,
					string.Format(CultureInfo.CurrentCulture, "Exception during discovery:{0}{1}", Environment.NewLine, ex)
				);
				await discoveryCallback(errorTestCase);
			}
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
	protected static IXunitTestCaseDiscoverer? GetDiscoverer(Type discovererType)
	{
		Guard.ArgumentNotNull(discovererType);

		try
		{
			return ExtensibilityPointFactory.GetXunitTestCaseDiscoverer(discovererType);
		}
		catch (Exception ex)
		{
			TestContext.Current?.SendDiagnosticMessage("Discoverer type '{0}' could not be created or does not implement IXunitTestCaseDiscoverer: {1}", discovererType.FullName, ex.Unwrap());
			return null;
		}
	}
}
