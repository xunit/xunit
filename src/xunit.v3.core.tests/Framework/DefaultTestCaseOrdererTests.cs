using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class DefaultTestCaseOrdererTests
{
	static readonly ITestCase[] TestCases =
	[
		Mocks.TestCase(uniqueID: "42e76f9f-5ad6-4004-b723-25fdfdda07c2"),
		Mocks.TestCase(uniqueID: "258c89d4-16c6-46dd-be77-3d6bb0584224"),
		Mocks.TestCase(uniqueID: "e596e94a-445b-44d1-aec4-f71027e971f5"),
		Mocks.TestCase(uniqueID: "8451aead-c51c-41bc-ab80-b72b443f8860"),
		Mocks.TestCase(uniqueID: "6ebfe3f1-8657-4522-b20f-fa2275e07e76"),
		Mocks.TestCase(uniqueID: "d5624513-6f42-4229-8276-eba1c1fe5320"),
	];

	[Fact]
	public static void OrderIsStable()
	{
		var orderer = DefaultTestCaseOrderer.Instance;

		var result1 = orderer.OrderTestCases(TestCases);
		var result2 = orderer.OrderTestCases(TestCases);
		var result3 = orderer.OrderTestCases(TestCases);

		Assert.Equal(result1, result2);
		Assert.Equal(result2, result3);
	}

	[Fact]
	public static void OrderIsUnpredictable()
	{
		var orderer = DefaultTestCaseOrderer.Instance;

		var result = orderer.OrderTestCases(TestCases);

		Assert.NotEqual(TestCases, result);
	}
}
