using Xunit;
using Xunit.v3;

partial class TestData
{
	public static CollectionBehaviorAttribute CollectionBehaviorAttribute(CollectionBehavior collectionBehavior) =>
		new(collectionBehavior);

	public static CollectionBehaviorAttribute CollectionBehaviorAttribute(Type collectionFactoryType) =>
		new(collectionFactoryType);

	public static CollectionBehaviorAttribute<TCollectionFactory> CollectionBehaviorAttribute<TCollectionFactory>()
#if XUNIT_AOT
			where TCollectionFactory : ICodeGenTestCollectionFactory =>
#else
			where TCollectionFactory : IXunitTestCollectionFactory =>
#endif
				new();
}
