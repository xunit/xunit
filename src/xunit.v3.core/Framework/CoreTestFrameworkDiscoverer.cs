using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base implementation derived from <see cref="TestFrameworkDiscoverer{TTestClass}"/> which contains common
/// code used for both reflection and native AOT test execution.
/// </summary>
/// <typeparam name="TTestClass">The test class object model type. Must derive from
/// <see cref="ICoreTestClass"/>.</typeparam>
/// <param name="testAssembly">The test assembly.</param>
public abstract class CoreTestFrameworkDiscoverer<TTestClass>(ICoreTestAssembly testAssembly) :
	TestFrameworkDiscoverer<TTestClass>(testAssembly)
		where TTestClass : class, ICoreTestClass
{
	/// <inheritdoc/>
	public override ValueTask Find(
		Func<ITestCase, ValueTask<bool>> callback,
		ITestFrameworkDiscoveryOptions discoveryOptions,
		Type[]? types = null,
		CancellationToken? cancellationToken = null)
	{
		SetEnvironment(EnvironmentVariables.PrintMaxEnumerableLength, discoveryOptions.PrintMaxEnumerableLength());
		SetEnvironment(EnvironmentVariables.PrintMaxObjectDepth, discoveryOptions.PrintMaxObjectDepth());
		SetEnvironment(EnvironmentVariables.PrintMaxObjectMemberCount, discoveryOptions.PrintMaxObjectMemberCount());
		SetEnvironment(EnvironmentVariables.PrintMaxStringLength, discoveryOptions.PrintMaxStringLength());

		return base.Find(callback, discoveryOptions, types, cancellationToken);
	}

	static void SetEnvironment(
		string environmentVariableName,
		int? value)
	{
		if (value.HasValue)
			Environment.SetEnvironmentVariable(environmentVariableName, value.Value.ToString(CultureInfo.InvariantCulture));
	}
}
