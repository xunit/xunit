using System;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Used to decorate an assembly or test collection to allow the use of a custom test class orderer.
/// </summary>
/// <param name="ordererType">The orderer type; must implement <see cref="ITestClassOrderer"/></param>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class TestClassOrdererAttribute(Type ordererType) : Attribute, ITestClassOrdererAttribute
{
	/// <inheritdoc/>
	public Type OrdererType { get; } = ordererType;
}
