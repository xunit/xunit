using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

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
		public async void AcceptanceTest()
		{
			var results = await RunAsync<ITestResultMessage>(typeof(ClassUnderTest));

			var result = Assert.Single(results);
			var skipResult = Assert.IsAssignableFrom<ITestSkipped>(result);
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
		public async void AcceptanceTest()
		{
			var results = await RunAsync(typeof(ClassUnderTest));

			var skipResult = Assert.Single(results.OfType<ITestSkipped>());
			Assert.Equal("Skipped", skipResult.TestMethod.Method.Name);
			Assert.Equal("This test was skipped", skipResult.Reason);
			var passResult = Assert.Single(results.OfType<ITestPassed>());
			Assert.Equal("Passed", passResult.TestMethod.Method.Name);
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
		public async void AcceptanceTest()
		{
			var results = await RunAsync(typeof(ClassUnderTest));

			var skipResult = Assert.Single(results.OfType<ITestSkipped>());
			Assert.Equal("Skipped", skipResult.TestMethod.Method.Name);
			Assert.Equal("This test was skipped", skipResult.Reason);
			var passResult = Assert.Single(results.OfType<ITestPassed>());
			Assert.Equal("Passed", passResult.TestMethod.Method.Name);
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
