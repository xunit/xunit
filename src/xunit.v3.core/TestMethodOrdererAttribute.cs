using System;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Used to decorate an assembly, test collection, or test class to allow the use of a custom test method orderer.
/// </summary>
/// <param name="ordererType">The orderer type; must implement <see cref="ITestMethodOrderer"/></param>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class TestMethodOrdererAttribute(Type ordererType) : Attribute, ITestMethodOrdererAttribute
{
	/// <inheritdoc/>
	public Type OrdererType { get; } = ordererType;
}
