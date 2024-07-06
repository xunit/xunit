using System;

namespace Xunit.v3;

/// <summary>
/// Used to decorate an assembly, test collection, or test class to allow the use of a custom test case orderer.
/// Only one may exist on a given element.
/// </summary>
public interface ITestCaseOrdererAttribute
{
	/// <summary>
	/// Gets the orderer type. Must implement <see cref="ITestCaseOrderer"/>.
	/// </summary>
	Type OrdererType { get; }
}
