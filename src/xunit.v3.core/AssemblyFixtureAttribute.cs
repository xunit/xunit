#pragma warning disable CA1019 // The attribute arguments are always read via reflection
#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System;

namespace Xunit;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate per-assembly fixture data. An instance of
/// the fixture data is initialized before any test in the assembly are run (including
/// <see cref="IAsyncLifetime.InitializeAsync"/> if it's implemented). After all the tests in the
/// assembly have been run, it is cleaned up by calling <see cref="IAsyncDisposable.DisposeAsync"/>
/// if it's implemented, or it falls back to <see cref="IDisposable.Dispose"/> if that's implemented.
/// Assembly fixtures must have a public parameterless constructor. To gain access to the fixture data
/// from inside the test, a constructor argument should be added to the test class which exactly
/// matches the fixture type.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class AssemblyFixtureAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="AssemblyFixtureAttribute"/> class.
	/// </summary>
	/// <param name="assemblyFixtureType">The assembly fixture class type</param>
	public AssemblyFixtureAttribute(Type assemblyFixtureType)
	{ }
}
