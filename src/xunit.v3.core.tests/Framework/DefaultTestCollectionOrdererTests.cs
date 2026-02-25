using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class DefaultTestCollectionOrdererTests
{
	static readonly ITestCollection[] TestCollections =
	[
		Mocks.TestCollection(uniqueID: "42e76f9f-5ad6-4004-b723-25fdfdda07c2"),
		Mocks.TestCollection(uniqueID: "258c89d4-16c6-46dd-be77-3d6bb0584224"),
		Mocks.TestCollection(uniqueID: "e596e94a-445b-44d1-aec4-f71027e971f5"),
		Mocks.TestCollection(uniqueID: "8451aead-c51c-41bc-ab80-b72b443f8860"),
		Mocks.TestCollection(uniqueID: "6ebfe3f1-8657-4522-b20f-fa2275e07e76"),
		Mocks.TestCollection(uniqueID: "d5624513-6f42-4229-8276-eba1c1fe5320"),
	];

	[Fact]
	public static void OrderIsStable()
	{
		var orderer = DefaultTestCollectionOrderer.Instance;

		var result1 = orderer.OrderTestCollections(TestCollections);
		var result2 = orderer.OrderTestCollections(TestCollections);
		var result3 = orderer.OrderTestCollections(TestCollections);

		Assert.Equal(result1, result2);
		Assert.Equal(result2, result3);
	}

	[Fact]
	public static void OrderIsUnpredictable()
	{
		var orderer = DefaultTestCollectionOrderer.Instance;

		var result = orderer.OrderTestCollections(TestCollections);

		Assert.NotEqual(TestCollections, result);
	}
}
