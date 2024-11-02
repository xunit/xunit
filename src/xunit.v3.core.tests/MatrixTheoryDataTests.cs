using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class MatrixTheoryDataTests : AcceptanceTestV3
{
	[Fact]
	public void GuardClauses()
	{
		var nonEmptyData = new[] { new object() };

		Assert.Throws<ArgumentNullException>("dimension1", () => new MatrixTheoryData<object?, object?>(null!, nonEmptyData));
		Assert.Throws<ArgumentNullException>("dimension2", () => new MatrixTheoryData<object?, object?>(nonEmptyData, null!));

		var emptyData = Array.Empty<object>();

		Assert.Throws<ArgumentException>("dimension1", () => new MatrixTheoryData<object?, object?>(emptyData, nonEmptyData));
		Assert.Throws<ArgumentException>("dimension2", () => new MatrixTheoryData<object?, object?>(nonEmptyData, emptyData));
	}

	[Fact]
	public async ValueTask InvokesTestsForDataMatrix()
	{
		var messages = await RunAsync(typeof(SampleUsage));

		Assert.Collection(
			messages.OfType<ITestPassed>().Select(passed => messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == passed.TestUniqueID).TestDisplayName).OrderBy(x => x),
			displayName => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"Hello\", y: 5)", displayName),
			displayName => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"world!\", y: 6)", displayName)
		);
		Assert.Collection(
			messages.OfType<ITestFailed>().Select(failed => messages.OfType<ITestStarting>().Single(ts => ts.TestUniqueID == failed.TestUniqueID).TestDisplayName).OrderBy(x => x),
			displayName => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"Hello\", y: 42)", displayName),
			displayName => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"Hello\", y: 6)", displayName),
			displayName => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"world!\", y: 42)", displayName),
			displayName => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"world!\", y: 5)", displayName)
		);
	}

	class SampleUsage
	{
		public static int[] Numbers = { 42, 5, 6 };
		public static string[] Strings = { "Hello", "world!" };
		public static MatrixTheoryData<string, int> MatrixData = new(Strings, Numbers);

		[Theory]
		[MemberData(nameof(MatrixData))]
		public void MyTestMethod(string x, int y)
		{
			Assert.Equal(y, x.Length);
		}
	}
}
