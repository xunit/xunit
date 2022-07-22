#if XUNIT_SKIP

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.v3;

public class SkipAssertsTests
{
	public class Skip : AcceptanceTestV3
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("reason", () => Assert.Skip(null!));
		}

		[Fact]
		public async Task AcceptanceTest()
		{
			var results = await RunAsync(typeof(ClassUnderTest));

			var skipResult = Assert.Single(results.OfType<_TestSkipped>());
			Assert.Equal("This test was skipped", skipResult.Reason);
		}

		class ClassUnderTest
		{
			[Fact]
			public void Unconditional()
			{
				Assert.Skip("This test was skipped");
			}
		}
	}

	public class SkipUnless : AcceptanceTestV3
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("reason", () => Assert.SkipUnless(true, null!));
		}

		[Fact]
		public async Task AcceptanceTest()
		{
			var results = await RunAsync(typeof(ClassUnderTest));

			var skipResult = Assert.Single(results.OfType<_TestSkipped>());
			var skipMethodStarting = Assert.Single(results.OfType<_TestMethodStarting>().Where(s => s.TestMethodUniqueID == skipResult.TestMethodUniqueID));
			Assert.Equal("Skipped", skipMethodStarting.TestMethod);
			Assert.Equal("This test was skipped", skipResult.Reason);
			var passResult = Assert.Single(results.OfType<_TestPassed>());
			var passMethodStarting = results.OfType<_TestMethodStarting>().Where(ts => ts.TestMethodUniqueID == passResult.TestMethodUniqueID).Single();
			Assert.Equal("Passed", passMethodStarting.TestMethod);
		}

		class ClassUnderTest
		{
			[Fact]
			public void Skipped()
			{
				Assert.SkipUnless(false, "This test was skipped");
			}

			[Fact]
			public void Passed()
			{
				Assert.SkipUnless(true, "This test is not skipped");
			}
		}
	}

	public class SkipWhen : AcceptanceTestV3
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("reason", () => Assert.SkipWhen(true, null!));
		}

		[Fact]
		public async Task AcceptanceTest()
		{
			var results = await RunAsync(typeof(ClassUnderTest));

			var skipResult = Assert.Single(results.OfType<_TestSkipped>());
			var skipMethodStarting = Assert.Single(results.OfType<_TestMethodStarting>().Where(s => s.TestMethodUniqueID == skipResult.TestMethodUniqueID));
			Assert.Equal("Skipped", skipMethodStarting.TestMethod);
			Assert.Equal("This test was skipped", skipResult.Reason);
			var passResult = Assert.Single(results.OfType<_TestPassed>());
			var passMethodStarting = results.OfType<_TestMethodStarting>().Where(ts => ts.TestMethodUniqueID == passResult.TestMethodUniqueID).Single();
			Assert.Equal("Passed", passMethodStarting.TestMethod);
		}

		class ClassUnderTest
		{
			[Fact]
			public void Skipped()
			{
				Assert.SkipWhen(true, "This test was skipped");
			}

			[Fact]
			public void Passed()
			{
				Assert.SkipWhen(false, "This test is not skipped");
			}
		}
	}
}

#endif
