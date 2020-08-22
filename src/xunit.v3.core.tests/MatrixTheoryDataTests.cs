using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

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
	public async void InvokesTestsForDataMatrix()
	{
		var messages = await RunAsync(typeof(SampleUsage));

		Assert.Collection(
			messages.OfType<ITestPassed>().OrderBy(x => x.Test.DisplayName),
			passing => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"Hello\", y: 5)", passing.Test.DisplayName),
			passing => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"world!\", y: 6)", passing.Test.DisplayName)
		);
		Assert.Collection(
			messages.OfType<ITestFailed>().OrderBy(x => x.Test.DisplayName),
			failing => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"Hello\", y: 42)", failing.Test.DisplayName),
			failing => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"Hello\", y: 6)", failing.Test.DisplayName),
			failing => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"world!\", y: 42)", failing.Test.DisplayName),
			failing => Assert.Equal("MatrixTheoryDataTests+SampleUsage.MyTestMethod(x: \"world!\", y: 5)", failing.Test.DisplayName)
		);
	}

	class SampleUsage
	{
		public static int[] Numbers = { 42, 5, 6 };
		public static string[] Strings = { "Hello", "world!" };
		public static MatrixTheoryData<string, int> MatrixData = new MatrixTheoryData<string, int>(Strings, Numbers);

		[Theory]
		[MemberData(nameof(MatrixData))]
		public void MyTestMethod(string x, int y)
		{
			Assert.Equal(y, x.Length);
		}
	}

}
