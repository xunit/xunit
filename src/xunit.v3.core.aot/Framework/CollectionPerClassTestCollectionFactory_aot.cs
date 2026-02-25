namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ICodeGenTestCollectionFactory"/> which creates a new test
/// collection for each test class that isn't decorated with <see cref="CollectionAttribute"/>
/// or <see cref="CollectionAttribute{TCollectionDefinition}"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
public class CollectionPerClassTestCollectionFactory(ICodeGenTestAssembly testAssembly) :
	TestCollectionFactoryBase(testAssembly)
{
	/// <inheritdoc/>
	public override string DisplayName => "collection-per-class";

	/// <inheritdoc/>
	protected override ICodeGenTestCollection GetDefaultTestCollection(Type testClass) =>
		new CodeGenTestCollection(
			beforeAfterTestAttributes: TestAssembly.BeforeAfterTestAttributes,
			classFixtureFactories: CodeGenHelper.EmptyFixtureFactories,
			collectionFixtureFactories: CodeGenHelper.EmptyFixtureFactories,
			disableParallelization: false,
			TestAssembly,
			testCollectionClass: null,
			CollectionAttribute.GetCollectionNameForType(testClass),
			traits: TestAssembly.Traits
		);
}
