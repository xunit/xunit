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
/// The implementation of <see cref="ITestFrameworkDiscoverer"/> that supports discovery
/// of unit tests linked against xunit.v3.core.dll.
/// </summary>
public class XunitTestFrameworkDiscoverer : TestFrameworkDiscoverer<IXunitTestCase, IXunitTestClass>
{
	static readonly FactAttribute defaultFactAttribute = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	/// <param name="collectionFactory">The test collection factory used to look up test collections.</param>
	public XunitTestFrameworkDiscoverer(
		IXunitTestAssembly testAssembly,
		IXunitTestCollectionFactory? collectionFactory = null) :
			base(testAssembly)
	{
		TestAssembly = Guard.ArgumentNotNull(testAssembly);

		var collectionBehavior = testAssembly.CollectionBehavior;

		TestCollectionFactory =
			collectionFactory
				?? ExtensibilityPointFactory.GetXunitTestCollectionFactory(collectionBehavior?.CollectionFactoryType, testAssembly)
				?? new CollectionPerClassTestCollectionFactory(testAssembly);
	}

	/// <summary>
	/// Gets the mapping dictionary of fact attribute type to discoverer type. The key
	/// is a type that implements <see cref="IFactAttribute"/>; the value is the
	/// discoverer type, if known; <c>null</c> if not.
	/// </summary>
	protected Dictionary<Type, Type?> DiscovererTypeCache { get; } = [];

	/// <inheritdoc/>
	public new IXunitTestAssembly TestAssembly { get; }

	/// <summary>
	/// Gets the test collection factory that makes test collections.
	/// </summary>
	public IXunitTestCollectionFactory TestCollectionFactory { get; private set; }

	/// <inheritdoc/>
	protected override ValueTask<IXunitTestClass> CreateTestClass(Type @class) =>
		new(new XunitTestClass(@class, TestCollectionFactory.Get(@class)));

	/// <summary>
	/// Finds the tests on a test method.
	/// </summary>
	/// <param name="testMethod">The test method.</param>
	/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
	/// <param name="discoveryCallback">The callback that is called for each discovered test case.</param>
	/// <returns>Return <c>true</c> to continue test discovery, <c>false</c>, otherwise.</returns>
	protected virtual async ValueTask<bool> FindTestsForMethod(
		IXunitTestMethod testMethod,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<IXunitTestCase, ValueTask<bool>> discoveryCallback)
	{
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(discoveryCallback);

		var factAttributes = testMethod.FactAttributes;
		var factAttribute = factAttributes.FirstOrDefault();
		if (factAttribute is null)
			return true;

		if (factAttributes.Count > 1)
		{
			var message = string.Format(CultureInfo.CurrentCulture, "Test method '{0}.{1}' has multiple [Fact]-derived attributes", testMethod.TestClass.TestClassName, testMethod.MethodName);
			var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute!);
			await using var testCase = new ExecutionErrorTestCase(details.ResolvedTestMethod, details.TestCaseDisplayName, details.UniqueID, message);
			return await discoveryCallback(testCase);
		}

		var factAttributeType = factAttribute.GetType();

		if (!DiscovererTypeCache.TryGetValue(factAttributeType, out var discovererType))
		{
			var testCaseDiscovererAttribute = factAttributeType.GetCustomAttribute<XunitTestCaseDiscovererAttribute>();
			if (testCaseDiscovererAttribute is not null)
				discovererType = testCaseDiscovererAttribute.Type;

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
		IXunitTestClass testClass,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<IXunitTestCase, ValueTask<bool>> discoveryCallback)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(discoveryCallback);

		foreach (var method in testClass.Methods)
		{
			var testMethod = new XunitTestMethod(testClass, method, []);

			try
			{
				if (!await FindTestsForMethod(testMethod, discoveryOptions, discoveryCallback))
					return false;
			}
			catch (Exception ex)
			{
				var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, defaultFactAttribute);
				await using var errorTestCase = new ExecutionErrorTestCase(
					testMethod,
					details.TestCaseDisplayName,
					details.UniqueID,
					string.Format(CultureInfo.CurrentCulture, "Exception during discovery:{0}{1}", Environment.NewLine, ex.Unwrap())
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
			TestContext.Current.SendDiagnosticMessage("Discoverer type '{0}' could not be created or does not implement IXunitTestCaseDiscoverer: {1}", discovererType.SafeName(), ex.Unwrap());
			return null;
		}
	}

	/// <inheritdoc/>
	protected override Type[] GetExportedTypes() =>
		TestAssembly.Assembly.GetExportedTypes();
}
