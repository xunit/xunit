using System;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Used to decorate an assembly, test collection, or test class to allow
/// the use of a custom test case orderer.
/// </summary>
/// <param name="ordererType">The orderer type; must implement <see cref="ITestCaseOrderer"/></param>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class TestCaseOrdererAttribute(Type ordererType) : Attribute, ITestCaseOrdererAttribute
{
	/// <inheritdoc/>
	public Type OrdererType { get; } = ordererType;
}
