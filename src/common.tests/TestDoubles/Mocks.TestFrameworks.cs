using System.Reflection;
using NSubstitute;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures mocks of test framework interfaces
public static partial class Mocks
{
	public static _ITestFramework TestFramework(
		_ITestFrameworkDiscoverer? discoverer = null,
		_ITestFrameworkExecutor? executor = null,
		string testFrameworkDisplayName = TestData.DefaultTestFrameworkDisplayName)
	{
		var result = Substitute.For<_ITestFramework, InterfaceProxy<_ITestFramework>>();

		discoverer ??= TestFrameworkDiscoverer();
		executor ??= TestFrameworkExecutor();

		result.TestFrameworkDisplayName.Returns(testFrameworkDisplayName);
		result.GetDiscoverer(Arg.Any<Assembly>()).Returns(discoverer);
		result.GetExecutor(Arg.Any<Assembly>()).Returns(executor);

		return result;
	}

	public static _ITestFrameworkDiscoverer TestFrameworkDiscoverer(_ITestAssembly? testAssembly = null)
	{
		var result = Substitute.For<_ITestFrameworkDiscoverer, InterfaceProxy<_ITestFrameworkDiscoverer>>();

		testAssembly ??= XunitTestAssembly();

		result.TestAssembly.Returns(testAssembly);

		return result;
	}

	public static _ITestFrameworkExecutor TestFrameworkExecutor() =>
		Substitute.For<_ITestFrameworkExecutor, InterfaceProxy<_ITestFrameworkExecutor>>();
}
