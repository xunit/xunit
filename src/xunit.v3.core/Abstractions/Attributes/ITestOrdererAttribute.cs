using System;

namespace Xunit.v3;

/// <summary>
/// Base interface for all attributes which specify a test orderer (at some level).
/// </summary>
public interface ITestOrdererAttribute
{
	/// <summary>
	/// Gets the orderer type.
	/// </summary>
	Type OrdererType { get; }
}
