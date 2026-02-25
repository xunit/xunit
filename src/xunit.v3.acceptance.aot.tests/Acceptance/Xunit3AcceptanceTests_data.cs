#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public partial class Xunit3AcceptanceTests
{
	public partial class AsyncLifetime
	{
#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncLifetime : IAsyncLifetime
		{
			protected readonly ITestOutputHelper output;

			public ClassWithAsyncLifetime(ITestOutputHelper output)
			{
				this.output = output;

				output.WriteLine("Constructor");
			}

			public virtual ValueTask InitializeAsync()
			{
				output.WriteLine("InitializeAsync");
				return default;
			}

			public virtual ValueTask DisposeAsync()
			{
				output.WriteLine("DisposeAsync");
				return default;
			}

			[Fact]
			public virtual void TheTest() =>
				output.WriteLine("Run Test");
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncDisposable : IAsyncDisposable, IDisposable
		{
			protected readonly ITestOutputHelper output;

			public ClassWithAsyncDisposable(ITestOutputHelper output)
			{
				this.output = output;

				output.WriteLine("Constructor");
			}

			public virtual void Dispose()
			{
				output.WriteLine("Dispose");
			}

			public virtual ValueTask DisposeAsync()
			{
				output.WriteLine("DisposeAsync");
				return default;
			}

			[Fact]
			public virtual void TheTest() =>
				output.WriteLine("Run Test");
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithDisposable : IDisposable
		{
			protected readonly ITestOutputHelper output;

			public ClassWithDisposable(ITestOutputHelper output)
			{
				this.output = output;

				output.WriteLine("Constructor");
			}

			public virtual void Dispose() =>
				output.WriteLine("Dispose");

			[Fact]
			public virtual void TheTest() =>
				output.WriteLine("Run Test");
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncLifetime_ThrowingCtor : ClassWithAsyncLifetime
		{
			public ClassWithAsyncLifetime_ThrowingCtor(ITestOutputHelper output) :
				base(output) =>
					throw new DivideByZeroException();
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncLifetime_ThrowingInitializeAsync(ITestOutputHelper output) :
			ClassWithAsyncLifetime(output)
		{
			public override async ValueTask InitializeAsync()
			{
				await base.InitializeAsync();

				throw new DivideByZeroException();
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncLifetime_ThrowingDisposeAsync(ITestOutputHelper output) :
			ClassWithAsyncLifetime(output)
		{
			public override async ValueTask DisposeAsync()
			{
				await base.DisposeAsync();

				throw new DivideByZeroException();
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncDisposable_ThrowingDisposeAsync(ITestOutputHelper output) :
			ClassWithAsyncDisposable(output)
		{
			public override async ValueTask DisposeAsync()
			{
				await base.DisposeAsync();

				throw new DivideByZeroException();
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithAsyncLifetime_FailingTest(ITestOutputHelper output) :
			ClassWithAsyncLifetime(output)
		{
			public override void TheTest()
			{
				base.TheTest();

				throw new DivideByZeroException();
			}
		}
	}

	public partial class ClassFailures
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_CtorFailure
		{
			public ClassUnderTest_CtorFailure() =>
				throw new DivideByZeroException();

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_DisposeFailure : IDisposable
		{
			public void Dispose() =>
				throw new DivideByZeroException();

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassUnderTest_FailingTestAndDisposeFailure : IDisposable
		{
			public void Dispose() =>
				throw new DivideByZeroException();

			[Fact]
			public void TheTest() =>
				Assert.Equal(2, 3);
		}
	}

	public partial class EndToEndMessageInspection
	{
#if XUNIT_AOT
		public
#endif
		class NoTestsClass
		{ }

#if XUNIT_AOT
		public
#endif
		class SinglePassingTestClass
		{
			[Fact]
			public void TestMethod() { }
		}
	}

	public partial class ErrorAggregation
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest
		{
			[Fact]
			public void EqualFailure() =>
				Assert.Equal(42, 40);

			[Fact]
			public void NotNullFailure() =>
				Assert.NotNull(null);
		}
	}

	public partial class ExplicitTests
	{
#if XUNIT_AOT
		public
#endif
		class ClassWithExplicitTest
		{
			[Fact]
			public void NonExplicitTest() =>
				Assert.True(true);

			[Fact(Explicit = true)]
			public void ExplicitTest() =>
				Assert.True(false);
		}
	}

	public partial class FailingTests
	{
#if XUNIT_AOT
		public
#endif
		class SingleFailingTestClass
		{
			[Fact]
			public void TestMethod() =>
				Assert.True(false);
		}

#if XUNIT_AOT
		public
#endif
		class SingleFailingValueTaskTestClass
		{
			[Fact]
			public async ValueTask TestMethod()
			{
				await Task.Delay(1, TestContext.Current.CancellationToken);
				Assert.True(false);
			}
		}
	}

	public partial class NonStartedTasks
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest
		{
			[Fact]
			public Task NonStartedTask() =>
				new(() => { Thread.Sleep(1000); });
		}
	}

	public partial class SkippedTests
	{
#if XUNIT_AOT
		public
#endif
		class SingleSkippedTestClass
		{
			[Fact(Skip = "This is a skipped test")]
			public void TestMethod() => Assert.True(false);
		}

		// Use theories for both to ensure that any "skip" logic in TheoryDiscoverer doesn't kick in for conditional skips
#if XUNIT_AOT
		public
#endif
		class ConditionallySkippedTestClass
		{
			public static bool Always => true;

			[Theory(Skip = "I am always skipped, conditionally", SkipWhen = nameof(Always))]
			[InlineData(false)]
			public void ConditionallyAlwaysSkipped(bool value) =>
				Assert.True(value);

			[Theory(Skip = "I am never skipped, conditionally", SkipUnless = nameof(Always))]
			[InlineData(false)]
			[InlineData(true)]
			public void ConditionallyNeverSkipped(bool value) =>
				Assert.True(value);
		}

#if XUNIT_AOT
		public
#endif
		class ConditionallySkippedTestsClass_UsingSkipType
		{
			[Fact(Skip = "I am always skipped, conditionally", SkipType = typeof(ConditionallySkippedTestClass), SkipWhen = nameof(ConditionallySkippedTestClass.Always))]
			public void ConditionallyAlwaysSkipped() =>
				Assert.True(false);

			[Fact(Skip = "I am never skipped, conditionally", SkipType = typeof(ConditionallySkippedTestClass), SkipUnless = nameof(ConditionallySkippedTestClass.Always))]
			public void ConditionallyNeverSkipped() { }
		}
	}

	public partial class StaticClassSupport
	{
#if XUNIT_AOT
		public
#endif
		static class StaticClassUnderTest
		{
			[Fact]
			public static void Passing() { }
		}
	}

	public partial class TestContextAccessor
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest(ITestContextAccessor accessor)
		{
			[Fact]
			public void Passing()
			{
				Assert.NotNull(accessor);
				Assert.Same(TestContext.Current, accessor.Current);
			}
		}
	}

	public partial class TestNonParallelOrdering
	{
		[CollectionDefinition("Parallel Ordered Collection")]
		[TestMethodOrderer(typeof(AlphabeticalMethodOrderer))]
		public class CollectionClass { }

		[Collection("Parallel Ordered Collection")]
#if XUNIT_AOT
		public
#endif
		class TestClassParallelCollection
		{
			[Fact]
			public void Test1() { }

			[Fact]
			public void Test2() { }
		}

		[CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
		[TestMethodOrderer(typeof(AlphabeticalMethodOrderer))]
		public class TestClassNonParallelCollectionDefinition { }

		[Collection("Non-Parallel Collection")]
#if XUNIT_AOT
		public
#endif
		class TestClassNonParallelCollection
		{
			[Fact]
			public void IShouldBeLast2() { }

			[Fact]
			public void IShouldBeLast1() { }
		}
	}

	public partial class TestOrdering
	{
		[CollectionDefinition("Ordered Collection")]
		[TestMethodOrderer(typeof(AlphabeticalMethodOrderer))]
		public class CollectionClass { }

		[Collection("Ordered Collection")]
#if XUNIT_AOT
		public
#endif
		class TestClassUsingCollection
		{
			[Fact]
			public void Test1() { }

			[Fact]
			public void Test3() { }

			[Fact]
			public void Test2() { }
		}

		[TestMethodOrderer(typeof(AlphabeticalMethodOrderer))]
#if XUNIT_AOT
		public
#endif
		class TestClassWithoutCollection
		{
			[Fact]
			public void Test1() { }

			[Fact]
			public void Test3() { }

			[Fact]
			public void Test2() { }
		}
	}

	public partial class TestOutput
	{
#if XUNIT_AOT
		public
#endif
		class ClassUnderTest : IDisposable
		{
			readonly ITestOutputHelper output;

			public ClassUnderTest(ITestOutputHelper output)
			{
				this.output = output;

				output.WriteLine("This is output in the constructor");
			}

			public void Dispose()
			{
				output.WriteLine("This is {0} in Dispose", "output");
			}

			[Fact]
			public void TestMethod() =>
				output.WriteLine("This is ITest output");
		}
	}

	public partial class Warnings
	{
#if XUNIT_AOT
		public
#endif
		sealed class ClassWithLegalWarnings : IDisposable
		{
			public ClassWithLegalWarnings() =>
				TestContext.Current.AddWarning("This is a warning message from the constructor");

			public void Dispose() =>
				TestContext.Current.AddWarning("This is a warning message from Dispose()");

			[Fact]
			public void Passing() =>
				TestContext.Current.AddWarning("This is a warning message from Passing()");

			[Fact]
			public void Failing()
			{
				TestContext.Current.AddWarning("This is a warning message from Failing()");
				Assert.True(false);
			}

			[Fact(Skip = "I never run")]
			public void Skipping() { }

			[Fact]
			public void SkippingDynamic()
			{
				TestContext.Current.AddWarning("This is a warning message from SkippingDynamic()");
				Assert.Skip("I decided not to run");
			}
		}

#if XUNIT_AOT
		public
#endif
		class FixtureWithIllegalWarning
		{
			public FixtureWithIllegalWarning() =>
				TestContext.Current.AddWarning("This is a warning from an illegal part of the pipeline");
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithIllegalWarnings : IClassFixture<FixtureWithIllegalWarning>
		{
			[Fact]
			public void Passing()
			{ }
		}
	}

	public class AlphabeticalMethodOrderer : ITestMethodOrderer
	{
		public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
			where TTestMethod : notnull, ITestMethod =>
				testMethods.OrderBy(tm => tm?.MethodName).ToList();
	}
}
