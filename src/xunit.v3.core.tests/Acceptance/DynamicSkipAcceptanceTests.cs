using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class DynamicSkipAcceptanceTests
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

			var skipResult = Assert.Single(results.OfType<ITestSkipped>());
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

			var skipResult = Assert.Single(results.OfType<ITestSkipped>());
			var skipMethodStarting = Assert.Single(results.OfType<ITestMethodStarting>(), s => s.TestMethodUniqueID == skipResult.TestMethodUniqueID);
			Assert.Equal("Skipped", skipMethodStarting.MethodName);
			Assert.Equal("This test was skipped", skipResult.Reason);
			var passResult = Assert.Single(results.OfType<ITestPassed>());
			var passMethodStarting = results.OfType<ITestMethodStarting>().Where(ts => ts.TestMethodUniqueID == passResult.TestMethodUniqueID).Single();
			Assert.Equal("Passed", passMethodStarting.MethodName);
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

			var skipResult = Assert.Single(results.OfType<ITestSkipped>());
			var skipMethodStarting = Assert.Single(results.OfType<ITestMethodStarting>(), s => s.TestMethodUniqueID == skipResult.TestMethodUniqueID);
			Assert.Equal("Skipped", skipMethodStarting.MethodName);
			Assert.Equal("This test was skipped", skipResult.Reason);
			var passResult = Assert.Single(results.OfType<ITestPassed>());
			var passMethodStarting = results.OfType<ITestMethodStarting>().Where(ts => ts.TestMethodUniqueID == passResult.TestMethodUniqueID).Single();
			Assert.Equal("Passed", passMethodStarting.MethodName);
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

	public class SkipExceptions : AcceptanceTestV3
	{
		[Theory]
		[InlineData(typeof(NotImplementedException))]
		[InlineData(typeof(NotSupportedException))]
		public async ValueTask WithMessage(Type exceptionType)
		{
			ClassUnderTest.ExceptionToThrow = Activator.CreateInstance(exceptionType, ["The exception message"]) as Exception;

			var results = await RunForResultsAsync(typeof(ClassUnderTest));

			Assert.Empty(results.OfType<TestPassedWithDisplayName>());
			Assert.Empty(results.OfType<TestFailedWithDisplayName>());
			Assert.Empty(results.OfType<TestNotRunWithDisplayName>());
			var skipResult = Assert.Single(results.OfType<TestSkippedWithDisplayName>());
			Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(ClassUnderTest.TestMethod)}", skipResult.TestDisplayName);
			Assert.Equal("The exception message", skipResult.Reason);
		}

		[Fact]
		public async ValueTask WithoutMessage()
		{
			ClassUnderTest.ExceptionToThrow = new MessagelessException();

			var results = await RunForResultsAsync(typeof(ClassUnderTest));

			Assert.Empty(results.OfType<TestPassedWithDisplayName>());
			Assert.Empty(results.OfType<TestFailedWithDisplayName>());
			Assert.Empty(results.OfType<TestNotRunWithDisplayName>());
			var skipResult = Assert.Single(results.OfType<TestSkippedWithDisplayName>());
			Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(ClassUnderTest.TestMethod)}", skipResult.TestDisplayName);
			Assert.Equal($"Exception of type '{typeof(MessagelessException).FullName}' was thrown", skipResult.Reason);
		}

		[Fact]
		public async ValueTask NonSkippedException()
		{
			ClassUnderTest.ExceptionToThrow = new DivideByZeroException();

			var results = await RunForResultsAsync(typeof(ClassUnderTest));

			Assert.Empty(results.OfType<TestPassedWithDisplayName>());
			Assert.Empty(results.OfType<TestSkippedWithDisplayName>());
			Assert.Empty(results.OfType<TestNotRunWithDisplayName>());
			var failedResult = Assert.Single(results.OfType<TestFailedWithDisplayName>());
			Assert.Equal($"{typeof(ClassUnderTest).FullName}.{nameof(ClassUnderTest.TestMethod)}", failedResult.TestDisplayName);
			Assert.Equal(typeof(DivideByZeroException).FullName, failedResult.ExceptionTypes.Single());
		}

		class MessagelessException : Exception
		{
			public override string Message => string.Empty;
		}

		class ClassUnderTest
		{
			public static Exception? ExceptionToThrow;

			[Fact(SkipExceptions = [typeof(NotImplementedException), typeof(NotSupportedException), typeof(MessagelessException)])]
			public static void TestMethod()
			{
				if (ExceptionToThrow is not null)
					throw ExceptionToThrow;
			}
		}
	}
}
