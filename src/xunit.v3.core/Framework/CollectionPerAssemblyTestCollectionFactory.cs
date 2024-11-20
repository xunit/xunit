using System;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="IXunitTestCollectionFactory"/> that creates a single
/// default test collection for the assembly, and places any tests classes which are not
/// decorated by <see cref="CollectionAttribute"/> or <see cref="CollectionAttribute{TCollectionDefinition}"/>
/// into the default test collection.
/// </summary>
public class CollectionPerAssemblyTestCollectionFactory : TestCollectionFactoryBase
{
	readonly XunitTestCollection defaultCollection;

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionPerAssemblyTestCollectionFactory" /> class.
	/// </summary>
	/// <param name="testAssembly">The assembly.</param>
	public CollectionPerAssemblyTestCollectionFactory(IXunitTestAssembly testAssembly) :
		base(testAssembly) =>
			defaultCollection = new XunitTestCollection(
				testAssembly,
				collectionDefinition: null,
				disableParallelization: false,
				"Test collection for " + TestAssembly.AssemblyName
			);

	/// <inheritdoc/>
	public override string DisplayName => "collection-per-assembly";

	/// <inheritdoc/>
	protected override IXunitTestCollection GetDefaultTestCollection(Type testClass) =>
		defaultCollection;
}
