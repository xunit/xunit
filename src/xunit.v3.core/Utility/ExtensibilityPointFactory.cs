using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

// Extensibility point factory helper methods, and factories that don't fit in any other categorization

/// <summary>
/// Represents a factory for the types used for extensibility throughout the system.
/// </summary>
public static partial class ExtensibilityPointFactory
{
	static object? CreateInstance(
		Type type,
		object?[]? ctorArgs)
	{
		ctorArgs ??= [];

		try
		{
			return Activator.CreateInstance(type, ctorArgs);
		}
		catch (MissingMemberException)
		{
			if (ctorArgs.Length == 0)
				TestContext.Current.SendDiagnosticMessage("Could not find empty constructor for '{0}'", type.SafeName());
			else
				TestContext.Current.SendDiagnosticMessage(
					"Could not find constructor for '{0}' with arguments type(s): {1}",
					type.SafeName(),
					ctorArgs.Select(a => a?.GetType()).ToCommaSeparatedList()
				);

			throw;
		}
	}

	/// <summary>
	/// Gets an instance of the given type, casting it to <typeparamref name="TInterface"/>, using the provided
	/// constructor arguments.
	/// </summary>
	/// <typeparam name="TInterface">The interface type.</typeparam>
	/// <param name="type">The implementation type.</param>
	/// <param name="ctorArgs">The constructor arguments. Since diagnostic message sinks are optional,
	/// the code first looks for a type that takes the given arguments plus the message sink, and only
	/// falls back to the message sink-less constructor if none was found.</param>
	/// <returns>The instance of the type.</returns>
	public static TInterface? Get<TInterface>(
		Type? type,
		object?[]? ctorArgs = null)
			where TInterface : class =>
				type is not null
					? CreateInstance(type, ctorArgs) as TInterface
					: default;

	/// <summary>
	/// Gets the test framework object for the given test assembly. It is important that callers to this function have
	/// called <see cref="TestContext.SetForInitialization"/> before calling this, so that the test framework and
	/// any ancillary helper classes have access to the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="testAssembly">The test assembly to get the test framework for</param>
	/// <returns>The test framework object</returns>
	public static ITestFramework GetTestFramework(Assembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		var testFrameworkAttribute = testAssembly.GetMatchingCustomAttributes(typeof(ITestFrameworkAttribute)).FirstOrDefault() as ITestFrameworkAttribute;
		var testFrameworkType = testFrameworkAttribute?.FrameworkType ?? typeof(XunitTestFramework);
		if (!typeof(ITestFramework).IsAssignableFrom(testFrameworkType))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Test framework type '{0}' does not implement '{1}'", testFrameworkType.SafeName(), typeof(ITestFramework).SafeName()));

		var obj =
			Activator.CreateInstance(testFrameworkType)
				?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Failed to create instance of test framework type '{0}'", testFrameworkType.SafeName()));

		return
			obj is ITestFramework result
				? result
				: throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Test framework type '{0}' could not be cast to '{1}'", testFrameworkType.SafeName(), typeof(ITestFramework).SafeName()));
	}

	/// <summary>
	/// Gets an xUnit.net v3 test discoverer.
	/// </summary>
	/// <param name="testCaseDiscovererType">The test case discoverer type</param>
	public static IXunitTestCaseDiscoverer? GetXunitTestCaseDiscoverer(Type testCaseDiscovererType) =>
		Get<IXunitTestCaseDiscoverer>(Guard.ArgumentNotNull(testCaseDiscovererType));

	/// <summary>
	/// Gets an xUnit.net v3 test collection factory.
	/// </summary>
	/// <param name="testCollectionFactoryType">The test collection factory type</param>
	/// <param name="testAssembly">The test assembly under test</param>
	public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
		Type? testCollectionFactoryType,
		IXunitTestAssembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		if (testCollectionFactoryType is not null && !typeof(IXunitTestCollectionFactory).IsAssignableFrom(testCollectionFactoryType))
		{
			TestContext.Current.SendDiagnosticMessage("Test collection factory type '{0}' does not implement IXunitTestCollectionFactory", testCollectionFactoryType.SafeName());
			testCollectionFactoryType = null;
		}

		return Get<IXunitTestCollectionFactory>(testCollectionFactoryType, [testAssembly]);
	}
}
