using System;
using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test collection from xUnit.net v3 based on reflection.
/// </summary>
/// <remarks>
/// Test collections form the basis of the parallelization in xUnit.net v3. Test cases
/// which are in the same test collection will not be run in parallel against sibling
/// tests, but will run in parallel against tests in other collections. They also provide
/// a level of shared context via <see cref="ICollectionFixture{TFixture}"/>.
/// </remarks>
public interface IXunitTestCollection : ITestCollection
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the test collection (and
	/// the test assembly).
	/// </summary>
	IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets a list of collection fixture types associated with the test collection.
	/// </summary>
	IReadOnlyCollection<Type> ClassFixtureTypes { get; }

	/// <summary>
	/// Gets the type that this collection definition derived from, if it derives from
	/// one. Untyped collections are possible when test classes are decorated
	/// using <see cref="CollectionAttribute(string)"/> and there is no test collection
	/// class declared with the same name.
	/// </summary>
	/// <remarks>
	/// This should only be used to execute a test collection. All reflection should be abstracted here
	/// instead for better testability.
	/// </remarks>
	Type? CollectionDefinition { get; }

	/// <summary>
	/// Gets a list of collection fixture types associated with the test collection.
	/// </summary>
	IReadOnlyCollection<Type> CollectionFixtureTypes { get; }

	/// <summary>
	/// Determines whether tests in this collection runs in parallel with any other collections.
	/// </summary>
	bool DisableParallelization { get; }

	/// <summary>
	/// Gets the test assembly this test collection belongs to.
	/// </summary>
	new IXunitTestAssembly TestAssembly { get; }

	/// <summary>
	/// Gets the test case orderer for the test collection, if present.
	/// </summary>
	ITestCaseOrderer? TestCaseOrderer { get; }
}
