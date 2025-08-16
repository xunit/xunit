using Xunit;
using Xunit.v3;

public class XunitTestFrameworkTests
{
	public class TestFrameworkDisplayName
	{
		[Fact(Skip = "Temporarily skipped for MTPv2")]
		public static void Defaults()
		{
			var framework = new XunitTestFramework();

			Assert.Matches(@"xUnit.net v3 \d+\.\d+\.\d+(-pre\.\d+(-dev)?(\+[0-9a-f]+)?)?", framework.TestFrameworkDisplayName);
		}
	}
}
