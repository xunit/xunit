using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test assembly from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTestAssembly : ITestAssembly
{
	/// <summary>
	/// Gets the assembly of this test assembly.
	/// </summary>
	/// <remarks>
	/// This should only be used to execute a test assembly. All reflection should be abstracted here
	/// instead for better testability.
	/// </remarks>
	Assembly Assembly { get; }

	/// <summary>
	/// Gets a list of fixture types associated with the test assembly.
	/// </summary>
	IReadOnlyCollection<Type> AssemblyFixtureTypes { get; }

	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the test assembly.
	/// </summary>
	IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the collection behavior associated with the assembly, if present.
	/// </summary>
	ICollectionBehaviorAttribute? CollectionBehavior { get; }

	/// <summary>
	/// Gets the collection definitions attached to the test assembly, by collection name.
	/// </summary>
	IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)> CollectionDefinitions { get; }

	/// <summary>
	/// Gets the target framework the test assembly was compiled against. Will be in a
	/// form like ".NETFramework,Version=v4.7.2" or ".NETCoreApp,Version=v6.0".
	/// </summary>
	string TargetFramework { get; }

	/// <summary>
	/// Gets the test case orderer for the test assembly, if present.
	/// </summary>
	ITestCaseOrderer? TestCaseOrderer { get; }

	/// <summary>
	/// Gets the test collection orderer for the test assembly, if present.
	/// </summary>
	ITestCollectionOrderer? TestCollectionOrderer { get; }

	/// <summary>
	/// Gets the assembly version.
	/// </summary>
	Version Version { get; }
}
