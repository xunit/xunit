using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.v3;

public class RecordTests
{
	public class MethodsWithoutReturnValues
	{
		[Fact]
		public void Exception()
		{
			static void testCode() => throw new InvalidOperationException();

			var ex = Record.Exception(testCode);

			Assert.NotNull(ex);
			Assert.IsType<InvalidOperationException>(ex);
		}

		[Fact]
		public void NoException()
		{
			static void testCode()
			{ }

			var ex = Record.Exception(testCode);

			Assert.Null(ex);
		}

		[Fact]
		public void SkipExceptionEscapes()
		{
			static void testCode() => Assert.Skip("This is a skipped test");

			try
			{
				Record.Exception(testCode);
				Assert.Fail("The exception should not be caught");
			}
			catch (Exception ex)
			{
				Assert.Equal(DynamicSkipToken.Value + "This is a skipped test", ex.Message);
			}
		}
	}

	public class MethodsReturningTask
	{
		[Fact]
		public async ValueTask Exception()
		{
			static Task testCode() => Task.Run(() => throw new InvalidOperationException(), TestContext.Current.CancellationToken);

			var ex = await Record.ExceptionAsync(testCode);

			Assert.NotNull(ex);
			Assert.IsType<InvalidOperationException>(ex);
		}

		[Fact]
		public async ValueTask NoException()
		{
			static Task testCode() => Task.Run(() => { }, TestContext.Current.CancellationToken);

			var ex = await Record.ExceptionAsync(testCode);

			Assert.Null(ex);
		}

		[Fact]
		public async ValueTask SkipExceptionEscapes()
		{
			static Task testCode() => Task.Run(() => Assert.Skip("This is a skipped test"), TestContext.Current.CancellationToken);

			try
			{
				await Record.ExceptionAsync(testCode);
				Assert.Fail("The exception should not be caught");
			}
			catch (Exception ex)
			{
				Assert.Equal(DynamicSkipToken.Value + "This is a skipped test", ex.Message);
			}
		}
	}

	public class MethodsWithReturnValues
	{
		[Fact]
		public void GuardClause()
		{
			static object testCode() => Task.Run(() => { }, TestContext.Current.CancellationToken);

			var ex = Record.Exception(() => Record.Exception(testCode));

			Assert.IsType<InvalidOperationException>(ex);
			Assert.Equal("You must call Assert.ThrowsAsync, Assert.DoesNotThrowAsync, or Record.ExceptionAsync when testing async code.", ex.Message);
		}

		[Fact]
		public void Exception()
		{
			var accessor = new StubAccessor();

			var ex = Record.Exception(() => accessor.FailingProperty);

			Assert.NotNull(ex);
			Assert.IsType<InvalidOperationException>(ex);
		}

		[Fact]
		public void NoException()
		{
			var accessor = new StubAccessor();

			var ex = Record.Exception(() => accessor.SuccessfulProperty);

			Assert.Null(ex);
		}

		[Fact]
		public void SkipExceptionEscapes()
		{
			var accessor = new StubAccessor();

			try
			{
				Record.Exception(() => accessor.SkippedProperty);
				Assert.Fail("The exception should not be caught");
			}
			catch (Exception ex)
			{
				Assert.Equal(DynamicSkipToken.Value + "This is a skipped test", ex.Message);
			}
		}

#pragma warning disable CA1822 // Mark members as static

		class StubAccessor
		{
			public int SuccessfulProperty { get; set; }

			public int FailingProperty
			{
				get { throw new InvalidOperationException(); }
			}

			public int SkippedProperty
			{
				get { Assert.Skip("This is a skipped test"); return 42; }
			}
		}

#pragma warning restore CA1822 // Mark members as static
	}
}
