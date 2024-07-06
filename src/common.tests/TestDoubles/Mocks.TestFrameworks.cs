using System.Reflection;
using NSubstitute;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures mocks of test framework interfaces
public static partial class Mocks
{
	public static ITestFramework TestFramework(
		ITestFrameworkDiscoverer? discoverer = null,
		ITestFrameworkExecutor? executor = null,
		string testFrameworkDisplayName = TestData.DefaultTestFrameworkDisplayName)
	{
		var result = Substitute.For<ITestFramework, InterfaceProxy<ITestFramework>>();

		discoverer ??= TestFrameworkDiscoverer();
		executor ??= TestFrameworkExecutor();

		result.TestFrameworkDisplayName.Returns(testFrameworkDisplayName);
		result.GetDiscoverer(Arg.Any<Assembly>()).Returns(discoverer);
		result.GetExecutor(Arg.Any<Assembly>()).Returns(executor);

		return result;
	}

	public static ITestFrameworkDiscoverer TestFrameworkDiscoverer(ITestAssembly? testAssembly = null)
	{
		var result = Substitute.For<ITestFrameworkDiscoverer, InterfaceProxy<ITestFrameworkDiscoverer>>();

		testAssembly ??= XunitTestAssembly();

		result.TestAssembly.Returns(testAssembly);

		return result;
	}

	public static ITestFrameworkExecutor TestFrameworkExecutor() =>
		Substitute.For<ITestFrameworkExecutor, InterfaceProxy<ITestFrameworkExecutor>>();
}
