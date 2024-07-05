using System;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Used to decorate an assembly to allow the use of a custom test collection orderer.
/// </summary>
/// <param name="ordererType">The orderer type; must implement <see cref="ITestCollectionOrderer"/></param>
[AttributeUsage(AttributeTargets.Assembly, Inherited = true, AllowMultiple = false)]
public sealed class TestCollectionOrdererAttribute(Type ordererType) : Attribute, ITestCollectionOrdererAttribute
{
	/// <inheritdoc/>
	public Type OrdererType { get; } = ordererType;
}
