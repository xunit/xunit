using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The implementation of <see cref="ITestFrameworkDiscoverer"/> that supports tests registered via
/// code generation.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
public class CodeGenTestFrameworkDiscoverer(ICodeGenTestAssembly testAssembly) :
	TestFrameworkDiscoverer<ICodeGenTestClass>(testAssembly)
{
	/// <summary>
	/// Gets the test assembly.
	/// </summary>
	public new ICodeGenTestAssembly TestAssembly { get; } =
		Guard.ArgumentNotNull(testAssembly);

	/// <inheritdoc/>
	protected override ValueTask<ICodeGenTestClass> CreateTestClass(Type @class) =>
		new(Guard.NotNull("Could not find test class registration for " + Guard.ArgumentNotNull(@class).SafeName(), RegisteredEngineConfig.GetTestClass(testAssembly, @class)));

	/// <inheritdoc/>
	protected override async ValueTask<bool> FindTestsForType(
		ICodeGenTestClass testClass,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		Func<ITestCase, ValueTask<bool>> discoveryCallback)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(discoveryCallback);

		foreach (var testCase in await RegisteredEngineConfig.GetTestCases(discoveryOptions, testClass, DisposalTracker))
			if (!await discoveryCallback(testCase))
				return false;

		return true;
	}

	/// <inheritdoc/>
	protected override Type[] GetExportedTypes() =>
		RegisteredEngineConfig.GetTestClassTypes();
}
