using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

partial class ExtensibilityPointFactory
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
	/// Gets an xUnit.net v3 test discoverer.
	/// </summary>
	/// <param name="testCaseDiscovererType">The test case discoverer type</param>
	public static IXunitTestCaseDiscoverer? GetXunitTestCaseDiscoverer(Type testCaseDiscovererType) =>
		Get<IXunitTestCaseDiscoverer>(Guard.ArgumentNotNull(testCaseDiscovererType));

	/// <summary>
	/// Please call <see cref="RegisteredEngineConfig.GetTestCollectionFactory"/>.
	/// This method will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call RegisteredEngineConfig.GetTestCollectionFactory. This method will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IXunitTestCollectionFactory? GetXunitTestCollectionFactory(
		Type? testCollectionFactoryType,
		IXunitTestAssembly testAssembly) =>
			RegisteredEngineConfig.GetTestCollectionFactory(testAssembly);
}
