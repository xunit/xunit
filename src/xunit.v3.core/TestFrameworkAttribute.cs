using System;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Used to decorate an assembly to allow the use of a custom test framework.
/// </summary>
/// <param name="frameworkType">The framework type; must implement <see cref="ITestFramework"/></param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class TestFrameworkAttribute(Type frameworkType) : Attribute, ITestFrameworkAttribute
{
	/// <inheritdoc/>
	public Type FrameworkType { get; } = frameworkType;
}
