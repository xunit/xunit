using System;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class _TestFailedTests
{
	public class Cause
	{
		[Fact]
		public void GuardClause()
		{
			var ex = Record.Exception(() => new _TestFailed { Cause = (FailureCause)2112 });

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("Cause", argEx.ParamName);
			Assert.StartsWith("Cause is not a valid value from Xunit.v3.FailureCause", argEx.Message);
		}

		[Fact]
		public void DefaultFailureCauseIsException()
		{
			var failed = new _TestFailed();

			Assert.Equal(FailureCause.Exception, failed.Cause);
		}

		[Theory]
		[MemberData(nameof(CauseValues))]
		public void CanOverrideCause(FailureCause cause)
		{
			var failed = new _TestFailed { Cause = cause };

			Assert.Equal(cause, failed.Cause);
		}

		public static TheoryData<FailureCause> CauseValues = new TheoryData<FailureCause> { FailureCause.Assertion, FailureCause.Exception, FailureCause.Timeout };
	}

	public class FromException
	{
		[Fact]
		public void NonAssertionException()
		{
			var ex = new DivideByZeroException();

			var failed = _TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null);

			Assert.Equal(FailureCause.Exception, failed.Cause);
		}

		[Fact]
		public void BuiltInAssertionException()
		{
			var ex = new EqualException(42, 2112);

			var failed = _TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null);

			Assert.Equal(FailureCause.Assertion, failed.Cause);
		}

		[Fact]
		public void CustomAssertionException()
		{
			var ex = new MyAssertionException();

			var failed = _TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null);

			Assert.Equal(FailureCause.Assertion, failed.Cause);
		}

		interface IAssertionException
		{ }

		class MyAssertionException : Exception, IAssertionException
		{ }

		[Fact]
		public void BuiltInTimeoutException()
		{
			var ex = new TestTimeoutException(2112);

			var failed = _TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null);

			Assert.Equal(FailureCause.Timeout, failed.Cause);
		}

		[Fact]
		public void CustomTimeoutException()
		{
			var ex = new MyTimeoutException();

			var failed = _TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null);

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

			var failed = _TestFailed.FromException(ex, "asm-id", "coll-id", "class-id", "method-id", "case-id", "test-id", 21.12M, null);

			Assert.Equal(FailureCause.Timeout, failed.Cause);
		}

		class MyMultiException : Exception, IAssertionException, ITestTimeoutException
		{ }
	}
}
