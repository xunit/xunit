using NSubstitute;
using Xunit.v3;

// This file contains mocks of test framework interfaces.
public static partial class Mocks
{
	public static _ITestFramework TestFramework(
		_ITestFrameworkDiscoverer? discoverer = null,
		_ITestFrameworkExecutor? executor = null)
	{
		var result = Substitute.For<_ITestFramework, InterfaceProxy<_ITestFramework>>();

		discoverer ??= TestFrameworkDiscoverer();
		executor ??= TestFrameworkExecutor();

		result.GetDiscoverer(Arg.Any<_IAssemblyInfo>()).Returns(discoverer);
		result.GetExecutor(Arg.Any<_IReflectionAssemblyInfo>()).Returns(executor);

		return result;
	}

	public static _ITestFrameworkDiscoverer TestFrameworkDiscoverer(
		string targetFramework = DefaultTargetFramework,
		_ITestAssembly? testAssembly = null,
		string testFrameworkDisplayName = DefaultTestFrameworkDisplayName)
	{
		var result = Substitute.For<_ITestFrameworkDiscoverer, InterfaceProxy<_ITestFrameworkDiscoverer>>();

		testAssembly ??= TestAssembly();

		result.TargetFramework.Returns(targetFramework);
		result.TestAssembly.Returns(testAssembly);
		result.TestFrameworkDisplayName.Returns(testFrameworkDisplayName);

		return result;
	}

	public static _ITestFrameworkExecutor TestFrameworkExecutor() =>
		Substitute.For<_ITestFrameworkExecutor, InterfaceProxy<_ITestFrameworkExecutor>>();
}
