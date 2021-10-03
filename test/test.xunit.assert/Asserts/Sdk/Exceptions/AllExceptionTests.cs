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
			new Tuple<int, object?, Exception>(
				1,
				$"Multi-line{Environment.NewLine}ToString-print",
				new Exception($"Multi-line{Environment.NewLine}message")
			),
			new Tuple<int, object?, Exception>(3, 2, new Exception("Error 2")),
			new Tuple<int, object?, Exception>(5, new object(), new Exception("Error 3")),
			new Tuple<int, object?, Exception>(6, null, new NullReferenceException("Error 4"))
		};

		var ex = new AllException(6, errors);

		var expectedMessage =
			"Assert.All() Failure: 4 out of 6 items in the collection did not pass." + Environment.NewLine +
			"[1]: Item: Multi-line" + Environment.NewLine +
			"     ToString-print" + Environment.NewLine +
			"     System.Exception: Multi-line" + Environment.NewLine +
			"     message" + Environment.NewLine +
			"[3]: Item: 2" + Environment.NewLine +
			"     System.Exception: Error 2" + Environment.NewLine +
			"[5]: Item: System.Object" + Environment.NewLine +
			"     System.Exception: Error 3" + Environment.NewLine +
			"[6]: Item: " + Environment.NewLine +
			"     System.NullReferenceException: Error 4";

		Assert.Equal(expectedMessage, ex.Message);
	}
}
