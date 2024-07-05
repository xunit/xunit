using System;
using System.Reflection;

namespace Xunit.Sdk;

/// <summary>
/// Represents a test assembly.
/// </summary>
/// <remarks>
/// Although most test frameworks will use an <see cref="Assembly"/> for the test assembly,
/// this is not a requirement at this layer. Assembly is just an abstraction that represents
/// a group of zero or more <see cref="ITestCollection"/>s.
/// </remarks>
public interface ITestAssembly : IAssemblyMetadata
{
	/// <summary>
	/// Returns the module version ID of the test assembly. Used as the basis for randomization.
	/// </summary>
	Guid ModuleVersionID { get; }
}
