using System;

namespace Xunit.v3;

/// <summary>
/// Used to decorate an assembly to allow the use of a custom test framework. May only be placed
/// at the assembly level, and only a single test framework is allowed.
/// </summary>
public interface ITestFrameworkAttribute
{
	/// <summary>
	/// Gets the framework type; must implement <see cref="ITestFramework"/>.
	/// </summary>
	Type FrameworkType { get; }
}
