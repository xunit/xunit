using Xunit;

public partial class DynamicSkipAcceptanceTests
{
	public partial class Skip
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest
		{
			[Fact]
			public void Unconditional()
			{
				Assert.Skip("This test was skipped");
			}
		}
	}

	public partial class SkipUnless
	{
#if XUNIT_AOT
		public
#endif
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

	public partial class SkipWhen
	{
#if XUNIT_AOT
		public
#endif
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

	public partial class SkipExceptions
	{
#if XUNIT_AOT
		public
#endif
		class MessagelessException : Exception
		{
			public override string Message => string.Empty;
		}

		// Despite the contract being for a non-null Message, there are Exception objects in the wild that
		// return null, so we must be prepared for them.
#if XUNIT_AOT
		public
#endif
		class NullMessageException : Exception
		{
			public override string Message => null!;
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest
		{
			public static TheoryData<Exception> ExceptionsData =
			[
				new NotImplementedException("The exception message"),
				new MessagelessException(),
				new NullMessageException(),
				new DivideByZeroException(),
			];

			[Theory(SkipExceptions = [typeof(NotImplementedException), typeof(MessagelessException), typeof(NullMessageException)])]
			[MemberData(nameof(ExceptionsData), DisableDiscoveryEnumeration = true)]
			public static void TestMethod(Exception ex) =>
				throw ex;
		}
	}
}
