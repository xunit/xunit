#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

using System.Runtime.InteropServices;
using Xunit;
using Xunit.Sdk;

public partial class FixtureAcceptanceTests
{
	public partial class AsyncClassFixture
	{
		/// <remarks>
		/// We include two class fixtures and test that each one is only initialised once.
		/// Regression testing for https://github.com/xunit/xunit/issues/869
		/// </remarks>
#if XUNIT_AOT
		public
#endif
		class FixtureSpy : IClassFixture<ThrowIfNotCompleted<Alpha>>, IClassFixture<ThrowIfNotCompleted<Beta>>
		{
			public FixtureSpy(ThrowIfNotCompleted<Alpha> alpha, ThrowIfNotCompleted<Beta> beta)
			{
				Assert.Equal(1, alpha.SetupCalls);
				Assert.Equal(1, beta.SetupCalls);
			}

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		sealed class ThrowIfNotCompleted<T> : IAsyncLifetime
		{
			public ValueTask InitializeAsync()
			{
				++SetupCalls;
				return default;
			}

			public ValueTask DisposeAsync()
			{
				return default;
			}

			public int SetupCalls = 0;
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureInitializeAsync(ThrowingInitializeAsyncFixture _) :
			IClassFixture<ThrowingInitializeAsyncFixture>
		{
			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureDisposeAsync(ThrowingDisposeAsyncFixture _) :
			IClassFixture<ThrowingDisposeAsyncFixture>
		{
			[Fact]
			public void TheTest() { }
		}
	}

	public partial class AsyncCollectionFixture
	{
		/// <remarks>
		/// We include two class fixtures and test that each one is only initialised once.
		/// Regression testing for https://github.com/xunit/xunit/issues/869
		/// </remarks>
		[CollectionDefinition]
		public class AsyncOnceCollection : ICollectionFixture<CountedAsyncFixture<Alpha>>, ICollectionFixture<CountedAsyncFixture<Beta>> { }

		[Collection(typeof(AsyncOnceCollection))]
#if XUNIT_AOT
		public
#endif
		class Fixture1
		{
			public Fixture1(CountedAsyncFixture<Alpha> alpha, CountedAsyncFixture<Beta> beta)
			{
				Assert.Equal(1, alpha.Count);
				Assert.Equal(1, beta.Count);
			}

			[Fact]
			public void TheTest() { }
		}

		[Collection(typeof(AsyncOnceCollection))]
#if XUNIT_AOT
		public
#endif
		class Fixture2
		{
			public Fixture2(CountedAsyncFixture<Alpha> alpha, CountedAsyncFixture<Beta> beta)
			{
				Assert.Equal(1, alpha.Count);
				Assert.Equal(1, beta.Count);
			}

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		sealed class CountedAsyncFixture<T> : IAsyncLifetime
		{
			public int Count = 0;
			public ValueTask InitializeAsync()
			{
				Count += 1;
				return default;
			}

			public ValueTask DisposeAsync()
			{
				return default;
			}
		}

		[CollectionDefinition]
		public class CollectionWithThrowingInitializeAsync : ICollectionFixture<ThrowingInitializeAsyncFixture> { }

		[Collection(typeof(CollectionWithThrowingInitializeAsync))]
#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureInitializeAsync
		{
			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition]
		public class CollectionWithThrowingDisposeAsync : ICollectionFixture<ThrowingDisposeAsyncFixture> { }

		[Collection(typeof(CollectionWithThrowingDisposeAsync))]
#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureDisposeAsync
		{
			[Fact]
			public void TheTest() { }
		}
	}

	public partial class ClassFixture
	{
#if XUNIT_AOT
		public
#endif
		class ClassWithMissingCtorArg(EmptyFixtureData _) :
			IClassFixture<EmptyFixtureData>, IClassFixture<object>
		{
			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureCtor : IClassFixture<ThrowingCtorFixture>
		{
			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithCtorAndThrowingFixtureCtor(FixtureAcceptanceTests.ThrowingCtorFixture _) :
			IClassFixture<ThrowingCtorFixture>
		{
			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureDispose : IClassFixture<ThrowingDisposeFixture>
		{
			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class FixtureSpy : IClassFixture<EmptyFixtureData>
		{
			readonly EmptyFixtureData data;

			public FixtureSpy(EmptyFixtureData data)
			{
				Assert.NotNull(data);

				this.data = data;
			}

			[Fact]
			public async ValueTask TheTest()
			{
				Assert.Same(data, await TestContext.Current.GetFixture(typeof(EmptyFixtureData)));
				Assert.Same(data, await TestContext.Current.GetFixture<EmptyFixtureData>());
			}
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithDefaultCtorArg : IClassFixture<EmptyFixtureData>
		{
			public ClassWithDefaultCtorArg(EmptyFixtureData fixture, int x = 0)
			{
				Assert.NotNull(fixture);
				Assert.Equal(0, x);
			}

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithOptionalCtorArg : IClassFixture<EmptyFixtureData>
		{
			public ClassWithOptionalCtorArg(EmptyFixtureData fixture, [Optional] int x, [Optional] object y)
			{
				Assert.NotNull(fixture);
				Assert.Equal(0, x);
				Assert.Null(y);
			}

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithParamsArg : IClassFixture<EmptyFixtureData>
		{
			public ClassWithParamsArg(EmptyFixtureData fixture, params object[] x)
			{
				Assert.NotNull(fixture);
				Assert.Empty(x);
			}

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithMessageSinkFixture(ClassFixture.MessageSinkFixture fixture) :
			IClassFixture<MessageSinkFixture>
		{
			[Fact]
			public void MessageSinkWasInjected() =>
				Assert.NotNull(fixture.MessageSink);
		}

#if XUNIT_AOT
		public
#endif
		class MessageSinkFixture(IMessageSink messageSink)
		{
			public IMessageSink MessageSink { get; } = messageSink;
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithSkippedTests : IClassFixture<ThrowingCtorFixture>
		{
			[Fact(Skip = "Do not run me")]
			public void Skipped() { }
		}

#if XUNIT_AOT
		public
#endif
		class ClassWithSkippedTests_WithConstructor(ThrowingCtorFixture fixture) :
			IClassFixture<ThrowingCtorFixture>
		{
			[Fact(Skip = "Do not run me")]
			public void Skipped() { }
		}
	}

	public partial class CollectionFixture
	{

		[CollectionDefinition("Collection with empty fixture data")]
		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>, ICollectionFixture<object>
		{ }

		[Collection("Collection with empty fixture data")]
#if XUNIT_AOT
		public
#endif
		class ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3)
		{
			[Fact]
			public void TheTest() { }
		}

		[Collection("Collection with empty fixture data")]
#if XUNIT_AOT
		public
#endif
		class ClassWithMissingCtorArg(EmptyFixtureData _)
		{
			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition("Collection with throwing constructor")]
		public class CollectionWithThrowingCtor : ICollectionFixture<ThrowingCtorFixture>
		{ }

		[Collection("Collection with throwing constructor")]
#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureCtor
		{
			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition("Collection with throwing dispose")]
		public class CollectionWithThrowingDispose : ICollectionFixture<ThrowingDisposeFixture>
		{ }

		[Collection("Collection with throwing dispose")]
#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureDispose
		{
			[Fact]
			public void TheTest() { }
		}

		[Collection("Collection with empty fixture data")]
#if XUNIT_AOT
		public
#endif
		class FixtureSpy
		{
			readonly EmptyFixtureData data;

			public FixtureSpy(EmptyFixtureData data)
			{
				Assert.NotNull(data);

				this.data = data;
			}

			[Fact]
			public async ValueTask TheTest()
			{
				Assert.Same(data, await TestContext.Current.GetFixture(typeof(EmptyFixtureData)));
				Assert.Same(data, await TestContext.Current.GetFixture<EmptyFixtureData>());
			}
		}

		[CollectionDefinition("Collection with static counting fixture data")]
		public class CollectionWithStaticCountingFixtureData : ICollectionFixture<StaticCountingFixtureData>, ICollectionFixture<object>
		{ }

		[Collection("Collection with static counting fixture data")]
#if XUNIT_AOT
		public
#endif
		class FixtureSaver1(StaticCountingFixtureData data)
		{
			[Fact]
			public void TheTest() =>
				TestContext.Current.TestOutputHelper?.WriteLine("FixtureSaver1: Counter value is {0}", data.Counter);
		}

		[Collection("Collection with static counting fixture data")]
#if XUNIT_AOT
		public
#endif
		class FixtureSaver2(StaticCountingFixtureData data)
		{
			[Fact]
			public void TheTest() =>
				TestContext.Current.TestOutputHelper?.WriteLine("FixtureSaver2: Counter value is {0}", data.Counter);
		}

#if XUNIT_AOT
		public
#endif
		class StaticCountingFixtureData
		{
			static int counter = 0;

			public int Counter { get; } = ++counter;
		}

		[CollectionDefinition("Collection with class fixture")]
		public class CollectionWithClassFixture : IClassFixture<EmptyFixtureData> { }

		[Collection("Collection with class fixture")]
#if XUNIT_AOT
		public
#endif
		class FixtureSpy_ClassFixture
		{
			public FixtureSpy_ClassFixture(EmptyFixtureData data) =>
				Assert.NotNull(data);

			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition("Collection with counted fixture")]
		public class CollectionWithClassFixtureCounter : ICollectionFixture<CountedFixture>
		{ }

		[Collection("Collection with counted fixture")]
#if XUNIT_AOT
		public
#endif
		class ClassWithCountedFixture : IClassFixture<CountedFixture>
		{
			public ClassWithCountedFixture(CountedFixture fixture)
			{
				// 1 == test collection, 2 == test class
				Assert.Equal(2, fixture.Identity);
			}

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class CountedFixture
		{
			static int counter = 0;

			public CountedFixture() => Identity = ++counter;

			public readonly int Identity;
		}

		[CollectionDefinition("collection with message sink fixture")]
		public class ClassWithMessageSinkFixtureCollection : ICollectionFixture<MessageSinkFixture>
		{ }

		[Collection("collection with message sink fixture")]
#if XUNIT_AOT
		public
#endif
		class ClassWithMessageSinkFixture(MessageSinkFixture fixture)
		{
			[Fact]
			public void MessageSinkWasInjected() =>
				Assert.NotNull(fixture.MessageSink);
		}

#if XUNIT_AOT
		public
#endif
		class MessageSinkFixture(IMessageSink messageSink)
		{
			public IMessageSink MessageSink { get; } = messageSink;
		}

		[CollectionDefinition("Class with skipped tests")]
		public class ClassWithSkippedTestsCollection : ICollectionFixture<ThrowingCtorFixture>
		{ }

		[Collection("Class with skipped tests")]
#if XUNIT_AOT
		public
#endif
		class ClassWithSkippedTests
		{
			[Fact(Skip = "Do not run me")]
			public void Skipped() { }
		}

		[CollectionDefinition("Class with skipped tests and constructor")]
		public class ClassWithSkippedTestsCollection_WithConstructor : ICollectionFixture<ThrowingCtorFixture>
		{ }

		[Collection("Class with skipped tests and constructor")]
#if XUNIT_AOT
		public
#endif
		class ClassWithSkippedTests_WithConstructor(ThrowingCtorFixture fixture)
		{
			[Fact(Skip = "Do not run me")]
			public void Skipped() { }
		}
	}

	public partial class CollectionFixtureByTypeArgument
	{
		[CollectionDefinition]
		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>, ICollectionFixture<object>
		{ }

		[Collection(typeof(CollectionWithEmptyFixtureData))]
#if XUNIT_AOT
		public
#endif
		class ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3)
		{
			[Fact]
			public void TheTest() { }
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
#if XUNIT_AOT
		public
#endif
		class ClassWithMissingCtorArg(EmptyFixtureData _)
		{
			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition]
		public class CollectionWithThrowingCtor : ICollectionFixture<ThrowingCtorFixture>
		{ }

		[Collection(typeof(CollectionWithThrowingCtor))]
#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureCtor
		{
			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition]
		public class CollectionWithThrowingDispose : ICollectionFixture<ThrowingDisposeFixture>
		{ }

		[Collection(typeof(CollectionWithThrowingDispose))]
#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureDispose
		{
			[Fact]
			public void TheTest() { }
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
#if XUNIT_AOT
		public
#endif
		class FixtureSpy
		{
			readonly EmptyFixtureData data;

			public FixtureSpy(EmptyFixtureData data)
			{
				Assert.NotNull(data);

				this.data = data;
			}

			[Fact]
			public async ValueTask TheTest()
			{
				Assert.Same(data, await TestContext.Current.GetFixture(typeof(EmptyFixtureData)));
				Assert.Same(data, await TestContext.Current.GetFixture<EmptyFixtureData>());
			}
		}

		[CollectionDefinition]
		public class CollectionWithStaticCountingFixtureData : ICollectionFixture<StaticCountingFixtureData>, ICollectionFixture<object>
		{ }

		[Collection(typeof(CollectionWithStaticCountingFixtureData))]
#if XUNIT_AOT
		public
#endif
		class FixtureSaver1(StaticCountingFixtureData data)
		{
			[Fact]
			public void TheTest() =>
				TestContext.Current.TestOutputHelper?.WriteLine("FixtureSaver1: Counter value is {0}", data.Counter);
		}

		[Collection(typeof(CollectionWithStaticCountingFixtureData))]
#if XUNIT_AOT
		public
#endif
		class FixtureSaver2(StaticCountingFixtureData data)
		{
			[Fact]
			public void TheTest() =>
				TestContext.Current.TestOutputHelper?.WriteLine("FixtureSaver2: Counter value is {0}", data.Counter);
		}

#if XUNIT_AOT
		public
#endif
		class StaticCountingFixtureData
		{
			static int counter = 0;

			public int Counter { get; } = ++counter;
		}

		[CollectionDefinition]
		public class CollectionWithClassFixture : IClassFixture<EmptyFixtureData>
		{ }

		[Collection(typeof(CollectionWithClassFixture))]
#if XUNIT_AOT
		public
#endif
		class FixtureSpy_ClassFixture
		{
			public FixtureSpy_ClassFixture(EmptyFixtureData data)
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition]
		public class CollectionWithClassFixtureCounter : ICollectionFixture<CountedFixture>
		{ }

		[Collection(typeof(CollectionWithClassFixtureCounter))]
#if XUNIT_AOT
		public
#endif
		class ClassWithCountedFixture : IClassFixture<CountedFixture>
		{
			public ClassWithCountedFixture(CountedFixture fixture)
			{
				// 1 == test collection, 2 == test class
				Assert.Equal(2, fixture.Identity);
			}

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class CountedFixture
		{
			static int counter = 0;

			public CountedFixture() => Identity = ++counter;

			public readonly int Identity;
		}
	}

#if !NETFRAMEWORK

	public partial class CollectionFixtureGeneric
	{
		[CollectionDefinition]
		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>, ICollectionFixture<object> { }

		[Collection<CollectionWithEmptyFixtureData>]
#if XUNIT_AOT
		public
#endif
		class ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3)
		{
			[Fact]
			public void TheTest() { }
		}

		[Collection<CollectionWithEmptyFixtureData>]
#if XUNIT_AOT
		public
#endif
		class ClassWithMissingCtorArg(EmptyFixtureData _)
		{
			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition]
		public class CollectionWithThrowingCtor : ICollectionFixture<ThrowingCtorFixture> { }

		[Collection<CollectionWithThrowingCtor>]
#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureCtor
		{
			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition]
		public class CollectionWithThrowingDispose : ICollectionFixture<ThrowingDisposeFixture> { }

		[Collection<CollectionWithThrowingDispose>]
#if XUNIT_AOT
		public
#endif
		class ClassWithThrowingFixtureDispose
		{
			[Fact]
			public void TheTest() { }
		}

		[Collection<CollectionWithEmptyFixtureData>]
#if XUNIT_AOT
		public
#endif
		class FixtureSpy
		{
			public FixtureSpy(EmptyFixtureData data)
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition]
		public class CollectionWithStaticCountingFixtureData : ICollectionFixture<StaticCountingFixtureData>, ICollectionFixture<object>
		{ }

		[Collection(typeof(CollectionWithStaticCountingFixtureData))]
#if XUNIT_AOT
		public
#endif
		class FixtureSaver1(StaticCountingFixtureData data)
		{
			[Fact]
			public void TheTest() =>
				TestContext.Current.TestOutputHelper?.WriteLine("FixtureSaver1: Counter value is {0}", data.Counter);
		}

		[Collection(typeof(CollectionWithStaticCountingFixtureData))]
#if XUNIT_AOT
		public
#endif
		class FixtureSaver2(StaticCountingFixtureData data)
		{
			[Fact]
			public void TheTest() =>
				TestContext.Current.TestOutputHelper?.WriteLine("FixtureSaver2: Counter value is {0}", data.Counter);
		}

#if XUNIT_AOT
		public
#endif
		class StaticCountingFixtureData
		{
			static int counter = 0;

			public int Counter { get; } = ++counter;
		}

		[CollectionDefinition]
		public class CollectionWithClassFixture : IClassFixture<EmptyFixtureData> { }

		[Collection<CollectionWithClassFixture>]
#if XUNIT_AOT
		public
#endif
		class FixtureSpy_ClassFixture
		{
			public FixtureSpy_ClassFixture(EmptyFixtureData data)
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[CollectionDefinition]
		public class CollectionWithClassFixtureCounter : ICollectionFixture<CountedFixture> { }

		[Collection<CollectionWithClassFixtureCounter>]
#if XUNIT_AOT
		public
#endif
		class ClassWithCountedFixture : IClassFixture<CountedFixture>
		{
			public ClassWithCountedFixture(CountedFixture fixture)
			{
				// 1 == test collection, 2 == test class
				Assert.Equal(2, fixture.Identity);
			}

			[Fact]
			public void TheTest() { }
		}

#if XUNIT_AOT
		public
#endif
		class CountedFixture
		{
			static int counter = 0;

			public CountedFixture() => Identity = ++counter;

			public readonly int Identity;
		}
	}

#endif  // !NETFRAMEWORK

#if XUNIT_AOT
	public
#endif
	sealed class Alpha
	{ }

#if XUNIT_AOT
	public
#endif
	sealed class Beta
	{ }

#if XUNIT_AOT
	public
#endif
	class EmptyFixtureData
	{ }

#if XUNIT_AOT
	public
#endif
	sealed class ThrowingCtorFixture
	{
		public ThrowingCtorFixture() => throw new DivideByZeroException();
	}

#if XUNIT_AOT
	public
#endif
	sealed class ThrowingInitializeAsyncFixture : IAsyncLifetime
	{
		public ValueTask DisposeAsync() => default;

		public ValueTask InitializeAsync() => throw new DivideByZeroException();
	}

#if XUNIT_AOT
	public
#endif
	sealed class ThrowingDisposeFixture : IDisposable
	{
		public void Dispose() => throw new DivideByZeroException();
	}

#if XUNIT_AOT
	public
#endif
	sealed class ThrowingDisposeAsyncFixture : IAsyncDisposable
	{
		public ValueTask DisposeAsync() => throw new DivideByZeroException();
	}
}
