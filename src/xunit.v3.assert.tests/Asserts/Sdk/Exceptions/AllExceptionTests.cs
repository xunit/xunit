using System;
using Xunit;
using Xunit.Sdk;

public class AllExceptionTests
{
	[Fact]
	public static void ReturnsAMessageForEachFailure()
	{
		var errors = new[]
		{
			new Tuple<int, string, Exception>(
				1,
				$"Multi-line{Environment.NewLine}item format",
				new Exception($"Multi-line{Environment.NewLine}exception message")
			),
			new Tuple<int, string, Exception>(3, ArgumentFormatter.Format(2), new Exception("Error 2")),
			new Tuple<int, string, Exception>(5, ArgumentFormatter.Format(new object()), new Exception("Error 3")),
			new Tuple<int, string, Exception>(16, ArgumentFormatter.Format(null), new NullReferenceException("Error 4"))
		};

		var ex = AllException.ForFailures(2112, errors);

		Assert.Equal(
			"Assert.All() Failure: 4 out of 2112 items in the collection did not pass." + Environment.NewLine +
			"[1]:  Item:  Multi-line" + Environment.NewLine +
			"             item format" + Environment.NewLine +
			"      Error: Multi-line" + Environment.NewLine +
			"             exception message" + Environment.NewLine +
			"[3]:  Item:  2" + Environment.NewLine +
			"      Error: Error 2" + Environment.NewLine +
			"[5]:  Item:  Object { }" + Environment.NewLine +
			"      Error: Error 3" + Environment.NewLine +
			"[16]: Item:  null" + Environment.NewLine +
			"      Error: Error 4",
			ex.Message
		);
	}
}
