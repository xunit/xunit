using System;

namespace Xunit.v3;

#if NETFRAMEWORK
/// <summary>
/// Implementation of <see cref="IXunitTestCollectionFactory"/> which creates a new test
/// collection for each test class that isn't decorated with <see cref="CollectionAttribute"/>.
/// </summary>
#else
/// <summary>
/// Implementation of <see cref="IXunitTestCollectionFactory"/> which creates a new test
/// collection for each test class that isn't decorated with <see cref="CollectionAttribute"/>
/// or <see cref="CollectionAttribute{TCollectionDefinition}"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
#endif
public class CollectionPerClassTestCollectionFactory(IXunitTestAssembly testAssembly) : TestCollectionFactoryBase(testAssembly)
{
	/// <inheritdoc/>
	public override string DisplayName => "collection-per-class";

	/// <inheritdoc/>
	protected override IXunitTestCollection GetDefaultTestCollection(Type testClass) =>
		new XunitTestCollection(
			TestAssembly,
			collectionDefinition: null,
			disableParallelization: false,
			CollectionAttribute.GetCollectionNameForType(testClass)
		);
}
