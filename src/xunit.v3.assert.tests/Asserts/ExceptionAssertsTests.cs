using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class ExceptionAssertsTests
{
	public class Throws_NonGeneric
	{
		public class WithAction
		{
			[Fact]
			public static void GuardClauses()
			{
				void testCode() { }

				Assert.Throws<ArgumentNullException>("exceptionType", () => Assert.Throws(null!, testCode));
				Assert.Throws<ArgumentNullException>("testCode", () => Assert.Throws(typeof(Exception), default(Action)!));
			}

			[Fact]
			public static void NoExceptionThrown()
			{
				void testCode() { }

				var ex = Record.Exception(() => Assert.Throws(typeof(ArgumentException), testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static void CorrectExceptionThrown()
			{
				void testCode() => throw new ArgumentException();

				Assert.Throws(typeof(ArgumentException), testCode);
			}

			[Fact]
			public static void IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				void testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws(typeof(ArgumentException), testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static void DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				void testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws(typeof(ArgumentException), testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}

		public class WithFunc
		{
			[Fact]
			public static void GuardClauses()
			{
				object testCode() => 42;

				Assert.Throws<ArgumentNullException>("exceptionType", () => Assert.Throws(null!, testCode));
				Assert.Throws<ArgumentNullException>("testCode", () => Assert.Throws(typeof(Exception), default(Func<object>)!));
			}

			[Fact]
			public static void ProtectsAgainstAccidentalTask()
			{
				static object testCode() => Task.FromResult(42);

				var ex = Record.Exception(() => Assert.Throws(typeof(Exception), testCode));

				Assert.IsType<InvalidOperationException>(ex);
				Assert.Equal("You must call Assert.ThrowsAsync when testing async code", ex.Message);
			}

#if XUNIT_VALUETASK
			[Fact]
			public static void ProtectsAgainstAccidentalValueTask()
			{
				static object testCode() => default(ValueTask);

				var ex = Record.Exception(() => Assert.Throws(typeof(Exception), testCode));

				Assert.IsType<InvalidOperationException>(ex);
				Assert.Equal("You must call Assert.ThrowsAsync when testing async code", ex.Message);
			}
#endif

			[Fact]
			public static void NoExceptionThrown()
			{
				object testCode() => 42;

				var ex = Record.Exception(() => Assert.Throws(typeof(ArgumentException), testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static void CorrectExceptionThrown()
			{
				object testCode() => throw new ArgumentException();

				Assert.Throws(typeof(ArgumentException), testCode);
			}

			[Fact]
			public static void IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				object testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws(typeof(ArgumentException), testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static void DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				object testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws(typeof(ArgumentException), testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}
	}

	public class Throws_Generic
	{
		public class WithAction
		{
			[Fact]
			public static void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("testCode", () => Assert.Throws<ArgumentException>(default(Action)!));
			}

			[Fact]
			public static void NoExceptionThrown()
			{
				void testCode() { }

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static void CorrectExceptionThrown()
			{
				void testCode() => throw new ArgumentException();

				Assert.Throws<ArgumentException>(testCode);
			}

			[Fact]
			public static void IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				void testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static void DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				void testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}

		public class WithFunc
		{
			[Fact]
			public static void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("testCode", () => Assert.Throws<ArgumentException>(default(Func<object>)!));
			}

			[Fact]
			public static void ProtectsAgainstAccidentalTask()
			{
				static object testCode() => Task.FromResult(42);

				var ex = Record.Exception(() => Assert.Throws<Exception>(testCode));

				Assert.IsType<InvalidOperationException>(ex);
				Assert.Equal("You must call Assert.ThrowsAsync when testing async code", ex.Message);
			}

#if XUNIT_VALUETASK
			[Fact]
			public static void ProtectsAgainstAccidentalValueTask()
			{
				static object testCode() => default(ValueTask);

				var ex = Record.Exception(() => Assert.Throws<Exception>(testCode));

				Assert.IsType<InvalidOperationException>(ex);
				Assert.Equal("You must call Assert.ThrowsAsync when testing async code", ex.Message);
			}
#endif

			[Fact]
			public static void NoExceptionThrown()
			{
				object testCode() => 42;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static void CorrectExceptionThrown()
			{
				object testCode() => throw new ArgumentException();

				Assert.Throws<ArgumentException>(testCode);
			}

			[Fact]
			public static void IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				object testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static void DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				object testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}
	}

	public class Throws_Generic_ArgumentException
	{
		public class WithAction
		{
			[Fact]
			public static void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("testCode", () => Assert.Throws<ArgumentException>(default(Action)!));
			}

			[Fact]
			public static void NoExceptionThrown()
			{
				void testCode() { }

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static void CorrectExceptionThrown()
			{
				void testCode() => throw new ArgumentException("Hello world", "paramName");

				Assert.Throws<ArgumentException>("paramName", testCode);
			}

			[Fact]
			public static void IncorrectParameterName()
			{
				void testCode() => throw new ArgumentException("Hello world", "paramName1");

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName2", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Incorrect parameter name" + Environment.NewLine +
					"Exception: typeof(System.ArgumentException)" + Environment.NewLine +
					"Expected:  \"paramName2\"" + Environment.NewLine +
					"Actual:    \"paramName1\"",
					ex.Message
				);
			}

			[Fact]
			public static void IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				void testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static void DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				void testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}

		public class WithFunc
		{
			[Fact]
			public static void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("testCode", () => Assert.Throws<ArgumentException>(default(Func<object>)!));
			}

			[Fact]
			public static void NoExceptionThrown()
			{
				object testCode() => 42;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static void CorrectExceptionThrown()
			{
				object testCode() => throw new ArgumentException("Hello world", "paramName");

				Assert.Throws<ArgumentException>("paramName", testCode);
			}

			[Fact]
			public static void IncorrectParameterName()
			{
				object testCode() => throw new ArgumentException("Hello world", "paramName1");

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName2", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Incorrect parameter name" + Environment.NewLine +
					"Exception: typeof(System.ArgumentException)" + Environment.NewLine +
					"Expected:  \"paramName2\"" + Environment.NewLine +
					"Actual:    \"paramName1\"",
					ex.Message
				);
			}

			[Fact]
			public static void IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				object testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static void DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				object testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.Throws<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}
	}

	public class ThrowsAny
	{
		public class WithAction
		{
			[Fact]
			public static void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("testCode", () => Assert.ThrowsAny<ArgumentException>(default(Action)!));
			}

			[Fact]
			public static void NoExceptionThrown()
			{
				void testCode() { }

				var ex = Record.Exception(() => Assert.ThrowsAny<ArgumentException>(testCode));

				Assert.IsType<ThrowsAnyException>(ex);
				Assert.Equal(
					"Assert.ThrowsAny() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static void CorrectExceptionThrown()
			{
				void testCode() => throw new ArgumentException();

				Assert.ThrowsAny<ArgumentException>(testCode);
			}

			[Fact]
			public static void IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				void testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.ThrowsAny<ArgumentException>(testCode));

				Assert.IsType<ThrowsAnyException>(ex);
				Assert.Equal(
					"Assert.ThrowsAny() Failure: Exception type was not compatible" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static void DerivedExceptionThrown()
			{
				void testCode() => throw new ArgumentNullException();

				Assert.ThrowsAny<ArgumentException>(testCode);
			}
		}

		public class WithFunc
		{
			[Fact]
			public static void GuardClause()
			{
				Assert.Throws<ArgumentNullException>("testCode", () => Assert.ThrowsAny<ArgumentException>(default(Func<object>)!));
			}

			[Fact]
			public static void ProtectsAgainstAccidentalTask()
			{
				static object testCode() => Task.FromResult(42);

				var ex = Record.Exception(() => Assert.ThrowsAny<Exception>(testCode));

				Assert.IsType<InvalidOperationException>(ex);
				Assert.Equal("You must call Assert.ThrowsAnyAsync when testing async code", ex.Message);
			}

#if XUNIT_VALUETASK
			[Fact]
			public static void ProtectsAgainstAccidentalValueTask()
			{
				static object testCode() => default(ValueTask);

				var ex = Record.Exception(() => Assert.ThrowsAny<Exception>(testCode));

				Assert.IsType<InvalidOperationException>(ex);
				Assert.Equal("You must call Assert.ThrowsAnyAsync when testing async code", ex.Message);
			}
#endif

			[Fact]
			public static void NoExceptionThrown()
			{
				object testCode() => 42;

				var ex = Record.Exception(() => Assert.ThrowsAny<ArgumentException>(testCode));

				Assert.IsType<ThrowsAnyException>(ex);
				Assert.Equal(
					"Assert.ThrowsAny() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static void CorrectExceptionThrown()
			{
				object testCode() => throw new ArgumentException();

				Assert.ThrowsAny<ArgumentException>(testCode);
			}

			[Fact]
			public static void IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				object testCode() => throw thrown;

				var ex = Record.Exception(() => Assert.ThrowsAny<ArgumentException>(testCode));

				Assert.IsType<ThrowsAnyException>(ex);
				Assert.Equal(
					"Assert.ThrowsAny() Failure: Exception type was not compatible" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static void DerivedExceptionThrown()
			{
				object testCode() => throw new ArgumentNullException();

				Assert.ThrowsAny<ArgumentException>(testCode);
			}
		}
	}

	public class ThrowsAnyAsync
	{
		public class WithTask
		{
			[Fact]
			public static async Task GuardClause()
			{
				await Assert.ThrowsAsync<ArgumentNullException>("testCode", () => Assert.ThrowsAnyAsync<ArgumentException>(default(Func<Task>)!));
			}

			[Fact]
			public static async Task NoExceptionThrown()
			{
				Task testCode() => Task.FromResult(42);

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAnyAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsAnyException>(ex);
				Assert.Equal(
					"Assert.ThrowsAny() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static async Task CorrectExceptionThrown()
			{
				Task testCode() => throw new ArgumentException();

				await Assert.ThrowsAnyAsync<ArgumentException>(testCode);
			}

			[Fact]
			public static async Task IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				Task testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAnyAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsAnyException>(ex);
				Assert.Equal(
					"Assert.ThrowsAny() Failure: Exception type was not compatible" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static async Task DerivedExceptionThrown()
			{
				Task testCode() => throw new ArgumentNullException();

				await Assert.ThrowsAnyAsync<ArgumentException>(testCode);
			}
		}

#if XUNIT_VALUETASK
		public class WithValueTask
		{
			[Fact]
			public static async ValueTask GuardClause()
			{
				async ValueTask testCode()
				{
					await Assert.ThrowsAnyAsync<ArgumentException>(default(Func<ValueTask>)!);
				}

				await Assert.ThrowsAsync<ArgumentNullException>("testCode", testCode);
			}

			[Fact]
			public static async ValueTask NoExceptionThrown()
			{
				ValueTask testCode() => default(ValueTask);

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAnyAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsAnyException>(ex);
				Assert.Equal(
					"Assert.ThrowsAny() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static async ValueTask CorrectExceptionThrown()
			{
				ValueTask testCode() => throw new ArgumentException();

				await Assert.ThrowsAnyAsync<ArgumentException>(testCode);
			}

			[Fact]
			public static async ValueTask IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				ValueTask testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAnyAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsAnyException>(ex);
				Assert.Equal(
					"Assert.ThrowsAny() Failure: Exception type was not compatible" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static async ValueTask DerivedExceptionThrown()
			{
				ValueTask testCode() => throw new ArgumentNullException();

				await Assert.ThrowsAnyAsync<ArgumentException>(testCode);
			}
		}
#endif
	}

	public class ThrowsAsync
	{
		public class WithTask
		{
			[Fact]
			public static async Task GuardClause()
			{
				await Assert.ThrowsAsync<ArgumentNullException>("testCode", () => Assert.ThrowsAsync<ArgumentException>(default(Func<Task>)!));
			}

			[Fact]
			public static async Task NoExceptionThrown()
			{
				Task testCode() => Task.FromResult(42);

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static async Task CorrectExceptionThrown()
			{
				Task testCode() => throw new ArgumentException();

				await Assert.ThrowsAsync<ArgumentException>(testCode);
			}

			[Fact]
			public static async Task IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				Task testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static async Task DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				Task testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}

#if XUNIT_VALUETASK
		public class WithValueTask
		{
			[Fact]
			public static async ValueTask GuardClause()
			{
				async ValueTask testCode()
				{
					await Assert.ThrowsAsync<ArgumentException>(default(Func<ValueTask>)!);
				}

				await Assert.ThrowsAsync<ArgumentNullException>("testCode", testCode);
			}

			[Fact]
			public static async ValueTask NoExceptionThrown()
			{
				ValueTask testCode() => default(ValueTask);

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static async ValueTask CorrectExceptionThrown()
			{
				ValueTask testCode() => throw new ArgumentException();

				await Assert.ThrowsAsync<ArgumentException>(testCode);
			}

			[Fact]
			public static async ValueTask IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				ValueTask testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static async ValueTask DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				ValueTask testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>(testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}
#endif
	}

	public class ThrowsAsync_ArgumentException
	{
		public class WithTask
		{
			[Fact]
			public static async Task GuardClause()
			{
				await Assert.ThrowsAsync<ArgumentNullException>("testCode", () => Assert.ThrowsAsync<ArgumentException>("paramName", default(Func<Task>)!));
			}

			[Fact]
			public static async Task NoExceptionThrown()
			{
				Task testCode() => Task.FromResult(42);

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static async Task CorrectExceptionThrown()
			{
				Task testCode() => throw new ArgumentException("Hello world", "paramName");

				await Assert.ThrowsAsync<ArgumentException>("paramName", testCode);
			}

			[Fact]
			public static async Task IncorrectParameterName()
			{
				Task testCode() => throw new ArgumentException("Hello world", "paramName1");

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName2", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Incorrect parameter name" + Environment.NewLine +
					"Exception: typeof(System.ArgumentException)" + Environment.NewLine +
					"Expected:  \"paramName2\"" + Environment.NewLine +
					"Actual:    \"paramName1\"",
					ex.Message
				);
			}

			[Fact]
			public static async Task IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				Task testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static async Task DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				Task testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}

#if XUNIT_VALUETASK
		public class WithValueTask
		{
			[Fact]
			public static async ValueTask GuardClause()
			{
				async ValueTask testCode()
				{
					await Assert.ThrowsAsync<ArgumentException>("paramName", default(Func<ValueTask>)!);
				}

				await Assert.ThrowsAsync<ArgumentNullException>("testCode", testCode);
			}

			[Fact]
			public static async ValueTask NoExceptionThrown()
			{
				ValueTask testCode() => default(ValueTask);

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: No exception was thrown" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)",
					ex.Message
				);
				Assert.Null(ex.InnerException);
			}

			[Fact]
			public static async ValueTask CorrectExceptionThrown()
			{
				ValueTask testCode() => throw new ArgumentException("Hello world", "paramName");

				await Assert.ThrowsAsync<ArgumentException>("paramName", testCode);
			}

			[Fact]
			public static async ValueTask IncorrectParameterName()
			{
				ValueTask testCode() => throw new ArgumentException("Hello world", "paramName1");

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName2", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Incorrect parameter name" + Environment.NewLine +
					"Exception: typeof(System.ArgumentException)" + Environment.NewLine +
					"Expected:  \"paramName2\"" + Environment.NewLine +
					"Actual:    \"paramName1\"",
					ex.Message
				);
			}

			[Fact]
			public static async ValueTask IncorrectExceptionThrown()
			{
				var thrown = new DivideByZeroException();
				ValueTask testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.DivideByZeroException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}

			[Fact]
			public static async ValueTask DerivedExceptionThrown()
			{
				var thrown = new ArgumentNullException();
				ValueTask testCode() => throw thrown;

				var ex = await Record.ExceptionAsync(() => Assert.ThrowsAsync<ArgumentException>("paramName", testCode));

				Assert.IsType<ThrowsException>(ex);
				Assert.Equal(
					"Assert.Throws() Failure: Exception type was not an exact match" + Environment.NewLine +
					"Expected: typeof(System.ArgumentException)" + Environment.NewLine +
					"Actual:   typeof(System.ArgumentNullException)",
					ex.Message
				);
				Assert.Same(thrown, ex.InnerException);
			}
		}
#endif
	}
}
