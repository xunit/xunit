namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ICodeGenTestCollectionFactory"/> that creates a single
/// default test collection for the assembly, and places any tests classes which are not
/// decorated by <see cref="CollectionAttribute"/> or <see cref="CollectionAttribute{TCollectionDefinition}"/>
/// into the default test collection.
/// </summary>
public class CollectionPerAssemblyTestCollectionFactory : TestCollectionFactoryBase
{
	readonly ICodeGenTestCollection defaultCollection;

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionPerAssemblyTestCollectionFactory" /> class.
	/// </summary>
	/// <param name="testAssembly">The assembly.</param>
	public CollectionPerAssemblyTestCollectionFactory(ICodeGenTestAssembly testAssembly) :
		base(testAssembly) =>
			defaultCollection = new CodeGenTestCollection(
				beforeAfterTestAttributes: TestAssembly.BeforeAfterTestAttributes,
				classFixtureFactories: CodeGenHelper.EmptyFixtureFactories,
				collectionFixtureFactories: CodeGenHelper.EmptyFixtureFactories,
				disableParallelization: false,
				TestAssembly,
				testCollectionClass: null,
				$"Test collection for {Guard.ArgumentNotNull(testAssembly).AssemblyName}",
				traits: TestAssembly.Traits
			);

	/// <inheritdoc/>
	public override string DisplayName => "collection-per-assembly";

	/// <inheritdoc/>
	protected override ICodeGenTestCollection GetDefaultTestCollection(Type testClass) =>
		defaultCollection;
}
