#pragma warning disable CA1019 // The attribute arguments are always read via reflection

using System;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Used to decorate an assembly to allow the use of a custom <see cref="_ITestFramework"/>.
/// </summary>
[TestFrameworkDiscoverer(typeof(TestFrameworkTypeDiscoverer))]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class TestFrameworkAttribute : Attribute, ITestFrameworkAttribute
{
	/// <summary>
	/// Initializes an instance of <see cref="TestFrameworkAttribute"/>.
	/// </summary>
	/// <param name="typeName">The fully qualified type name of the test framework
	/// (f.e., 'Xunit.Sdk.XunitTestFramework')</param>
	/// <param name="assemblyName">The name of the assembly that the test framework type
	/// is located in, without file extension (f.e., 'xunit.v3.core')</param>
	public TestFrameworkAttribute(
		string typeName,
		string assemblyName)
	{ }

	/// <summary>
	/// Initializes an instance of <see cref="TestFrameworkAttribute"/>.
	/// </summary>
	/// <param name="frameworkType">The framework type</param>
	public TestFrameworkAttribute(Type frameworkType)
	{ }
}
