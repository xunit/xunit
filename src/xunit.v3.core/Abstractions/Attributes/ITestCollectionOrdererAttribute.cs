using System;

namespace Xunit.v3;

/// <summary>
/// Used to decorate an assembly, test collection, or test class to allow the use of a custom test collection orderer.
/// Only one may exist on a given element.
/// </summary>
public interface ITestCollectionOrdererAttribute
{
	/// <summary>
	/// Gets the orderer type. Must implement <see cref="ITestCollectionOrderer"/>.
	/// </summary>
	Type OrdererType { get; }
}
