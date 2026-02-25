using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class TestFailedTests
{
	partial class FromException
	{
		[Fact]
		public void CustomAssertionException()
		{
			var ex = new MyAssertionException();

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Assertion, failed.Cause);
		}

		interface IAssertionException
		{ }

		class MyAssertionException : Exception, IAssertionException
		{ }

		[Fact]
		public void CustomTimeoutException()
		{
			var ex = new MyTimeoutException();

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Timeout, failed.Cause);
		}

		interface ITestTimeoutException
		{ }

		class MyTimeoutException : Exception, ITestTimeoutException
		{ }

		[Fact]
		public void TimeoutExceptionTrumpsAssertionException()
		{
			var ex = new MyMultiException();

			var failed = TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null, null);

			Assert.Equal(FailureCause.Timeout, failed.Cause);
		}

		class MyMultiException : Exception, IAssertionException, ITestTimeoutException
		{ }
	}
}
