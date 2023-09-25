#pragma warning disable CA1019 // The attribute arguments are always read via reflection

using System;

namespace Xunit.Sdk;

/// <summary>
/// An attribute used to decorate classes which derive from <see cref="FactAttribute"/>,
/// to indicate how test cases should be discovered.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class XunitTestCaseDiscovererAttribute : Attribute
{
	/// <summary>
	/// Initializes an instance of the <see cref="XunitTestCaseDiscovererAttribute"/> class.
	/// The discoverer class must implement <see cref="IXunitTestCaseDiscoverer" />.
	/// </summary>
	/// <param name="typeName">The fully qualified type name of the discoverer
	/// (f.e., 'Xunit.Sdk.FactDiscoverer').</param>
	/// <param name="assemblyName">The name of the assembly that the discoverer type
	/// is located in, without file extension (f.e., 'xunit.v3.core')</param>
	public XunitTestCaseDiscovererAttribute(
		string typeName,
		string assemblyName)
	{ }

	/// <summary>
	/// Initializes an instance of the <see cref="XunitTestCaseDiscovererAttribute"/> class.
	/// The discoverer class must implement <see cref="IXunitTestCaseDiscoverer" />.
	/// </summary>
	/// <param name="type">The type of the discoverer</param>
	public XunitTestCaseDiscovererAttribute(Type type)
	{ }
}
