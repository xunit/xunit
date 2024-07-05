using System;

namespace Xunit.v3;

/// <summary>
/// An attribute used to decorate classes which implement <see cref="IFactAttribute"/>,
/// to indicate how test cases should be discovered.
/// </summary>
/// <param name="type">The type of the discoverer; must implement <see cref="IXunitTestCaseDiscoverer"/>.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class XunitTestCaseDiscovererAttribute(Type type) : Attribute
{
	/// <summary>
	/// Gets the type of the test case discoverer.
	/// </summary>
	public Type Type { get; } = type;
}
