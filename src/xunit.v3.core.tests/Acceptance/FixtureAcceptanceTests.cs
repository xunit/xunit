using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Xunit.v3;

public class FixtureAcceptanceTests
{
	public class Constructors : AcceptanceTestV3
	{
		[Fact]
		public async void TestClassMustHaveSinglePublicConstructor()
		{
			var messages = await RunAsync(typeof(ClassWithTooManyConstructors));

			Assert.Collection(
				messages,
				message => Assert.IsType<_TestAssemblyStarting>(message),
				message => Assert.IsType<_TestCollectionStarting>(message),
				message => Assert.IsType<_TestClassStarting>(message),

				// TestMethod1
				message => Assert.IsAssignableFrom<_TestMethodStarting>(message),
				message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message =>
				{
					var failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
					Assert.Equal(typeof(TestClassException).FullName, failedMessage.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failedMessage.Messages.Single());
				},
				message => Assert.IsAssignableFrom<ITestFinished>(message),
				message => Assert.IsAssignableFrom<ITestCaseFinished>(message),
				message => Assert.IsAssignableFrom<ITestMethodFinished>(message),

				// TestMethod2
				message => Assert.IsAssignableFrom<_TestMethodStarting>(message),
				message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message =>
				{
					var failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
					Assert.Equal(typeof(TestClassException).FullName, failedMessage.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failedMessage.Messages.Single());
				},
				message => Assert.IsAssignableFrom<ITestFinished>(message),
				message => Assert.IsAssignableFrom<ITestCaseFinished>(message),
				message => Assert.IsAssignableFrom<ITestMethodFinished>(message),

				message => Assert.IsType<_TestClassFinished>(message),
				message => Assert.IsType<_TestCollectionFinished>(message),
				message => Assert.IsType<_TestAssemblyFinished>(message)
			);
		}

		class ClassWithTooManyConstructors
		{
			public ClassWithTooManyConstructors() { }
			public ClassWithTooManyConstructors(int unused) { }

			[Fact]
			public void TestMethod1() { }

			[Fact]
			public void TestMethod2() { }
		}
	}

	public class ClassFixture : AcceptanceTestV3
	{
		[Fact]
		public async void TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestClassException).FullName, msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 arg1, String arg2", msg.Messages.Single());
		}

		class ClassWithExtraCtorArg : IClassFixture<EmptyFixtureData>
		{
			public ClassWithExtraCtorArg(int arg1, EmptyFixtureData fixture, string arg2) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithMissingCtorArg));

			Assert.Single(messages);
		}

		class ClassWithMissingCtorArg : IClassFixture<EmptyFixtureData>, IClassFixture<object>
		{
			public ClassWithMissingCtorArg(EmptyFixtureData fixture) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void TestClassWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureCtor));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		class ClassWithThrowingFixtureCtor : IClassFixture<ThrowingCtorFixture>
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void TestClassWithThrowingFixtureDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<_TestClassCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		class ClassWithThrowingFixtureDispose : IClassFixture<ThrowingDisposeFixture>
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void FixtureDataIsPassedToConstructor()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

		class FixtureSpy : IClassFixture<EmptyFixtureData>
		{
			public FixtureSpy(EmptyFixtureData data)
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void TestClassWithDefaultParameter()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithDefaultCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal("FixtureAcceptanceTests+ClassFixture+ClassWithDefaultCtorArg.TheTest", msg.Test.DisplayName);
		}

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

		[Fact]
		public async void TestClassWithOptionalParameter()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithOptionalCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal("FixtureAcceptanceTests+ClassFixture+ClassWithOptionalCtorArg.TheTest", msg.Test.DisplayName);
		}

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

		[Fact]
		public async void TestClassWithParamsParameter()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithParamsArg));

			var msg = Assert.Single(messages);
			Assert.Equal("FixtureAcceptanceTests+ClassFixture+ClassWithParamsArg.TheTest", msg.Test.DisplayName);
		}

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
	}

	public class AsyncClassFixture : AcceptanceTestV3
	{
		[Fact]
		public async void FixtureDataShouldHaveBeenSetup()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

		class Alpha { }
		class Beta { }

		/// <remarks>
		/// We include two class fixtures and test that each one is only initialised once.
		/// Regression testing for https://github.com/xunit/xunit/issues/869
		/// </remarks>
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

		class ThrowIfNotCompleted<T> : IAsyncLifetime
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

		[Fact]
		public async void ThrowingAsyncSetupShouldResultInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureSetup));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		class ClassWithThrowingFixtureSetup : IClassFixture<ThrowingSetup>
		{
			public ClassWithThrowingFixtureSetup(ThrowingSetup ignored) { }

			[Fact]
			public void TheTest() { }
		}

		class ThrowingSetup : IAsyncLifetime
		{
			public ValueTask InitializeAsync()
			{
				throw new DivideByZeroException();
			}

			public ValueTask DisposeAsync()
			{
				return default;
			}
		}

		[Fact]
		public async void TestClassWithThrowingFixtureAsyncDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<_TestClassCleanupFailure>(typeof(ClassWithThrowingFixtureDisposeAsync));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		class ClassWithThrowingFixtureDisposeAsync : IClassFixture<ThrowingDisposeAsync>
		{
			public ClassWithThrowingFixtureDisposeAsync(ThrowingDisposeAsync ignore) { }

			[Fact]
			public void TheTest() { }
		}

		class ThrowingDisposeAsync : IAsyncLifetime
		{
			public ValueTask InitializeAsync()
			{
				return default;
			}

			public ValueTask DisposeAsync()
			{
				throw new DivideByZeroException();
			}
		}

		[Fact]
		public async void TestClassWithThrowingFixtureAsyncDisposeResultsInFailedTest_Disposable()
		{
			var messages = await RunAsync<_TestClassCleanupFailure>(typeof(ClassWithThrowingFixtureDisposeAsync));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		class ClassWithThrowingFixtureDisposeAsync_Disposable : IClassFixture<ThrowingDisposeAsync_Disposable>
		{
			public ClassWithThrowingFixtureDisposeAsync_Disposable(ThrowingDisposeAsync_Disposable ignore) { }

			[Fact]
			public void TheTest() { }
		}

		class ThrowingDisposeAsync_Disposable : IAsyncDisposable
		{
			public ValueTask DisposeAsync()
			{
				throw new DivideByZeroException();
			}
		}
	}

	public class CollectionFixture : AcceptanceTestV3
	{
		[Fact]
		public async void TestClassCannotBeDecoratedWithICollectionFixture()
		{
			var messages = await RunAsync<ITestFailed>(typeof(TestClassWithCollectionFixture));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestClassException).FullName, msg.ExceptionTypes.Single());
			Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", msg.Messages.Single());
		}

		class TestClassWithCollectionFixture : ICollectionFixture<EmptyFixtureData>
		{
			[Fact]
			public void TestMethod() { }
		}

		[Fact]
		public async void TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestClassException).FullName, msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 arg1, String arg2", msg.Messages.Single());
		}

		[CollectionDefinition("Collection with empty fixture data")]
		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>
		{
		}

		[Collection("Collection with empty fixture data")]
		class ClassWithExtraCtorArg
		{
			public ClassWithExtraCtorArg(int arg1, EmptyFixtureData fixture, string arg2) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithMissingCtorArg));

			Assert.Single(messages);
		}

		[Collection("Collection with empty fixture data")]
		class ClassWithMissingCtorArg
		{
			public ClassWithMissingCtorArg(EmptyFixtureData fixture) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void TestClassWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureCtor));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[CollectionDefinition("Collection with throwing constructor")]
		public class CollectionWithThrowingCtor : ICollectionFixture<ThrowingCtorFixture>
		{
		}

		[Collection("Collection with throwing constructor")]
		class ClassWithThrowingFixtureCtor
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void TestClassWithThrowingCollectionFixtureDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<_TestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[CollectionDefinition("Collection with throwing dispose")]
		public class CollectionWithThrowingDispose : ICollectionFixture<ThrowingDisposeFixture>
		{
		}

		[Collection("Collection with throwing dispose")]
		class ClassWithThrowingFixtureDispose
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void FixtureDataIsPassedToConstructor()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

		[Collection("Collection with empty fixture data")]
		class FixtureSpy
		{
			public FixtureSpy(EmptyFixtureData data)
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void FixtureDataIsSameInstanceAcrossClasses()
		{
			await RunAsync<ITestPassed>(new[] { typeof(FixtureSaver1), typeof(FixtureSaver2) });

			Assert.Same(FixtureSaver1.Fixture, FixtureSaver2.Fixture);
		}

		class FixtureSaver1
		{
			public static EmptyFixtureData? Fixture;

			public FixtureSaver1(EmptyFixtureData data)
			{
				Fixture = data;
			}

			[Fact]
			public void TheTest() { }
		}

		class FixtureSaver2
		{
			public static EmptyFixtureData? Fixture;

			public FixtureSaver2(EmptyFixtureData data)
			{
				Fixture = data;
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void ClassFixtureOnCollectionDecorationWorks()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy_ClassFixture));

			Assert.Single(messages);
		}

		[CollectionDefinition("Collection with class fixture")]
		public class CollectionWithClassFixture : IClassFixture<EmptyFixtureData> { }

		[Collection("Collection with class fixture")]
		class FixtureSpy_ClassFixture
		{
			public FixtureSpy_ClassFixture(EmptyFixtureData data)
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async void ClassFixtureOnTestClassTakesPrecedenceOverClassFixtureOnCollection()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithCountedFixture));

			Assert.Single(messages);
		}

		[CollectionDefinition("Collection with counted fixture")]
		public class CollectionWithClassFixtureCounter : ICollectionFixture<CountedFixture> { }

		[Collection("Collection with counted fixture")]
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
	}

	public class AsyncCollectionFixture : AcceptanceTestV3
	{
		[Fact]
		public async void TestClassWithThrowingCollectionFixtureSetupAsyncResultsInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureSetupAsync));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[CollectionDefinition("Collection with throwing async setup")]
		public class CollectionWithThrowingSetupAsync : ICollectionFixture<ThrowingSetupAsync> { }

		[Collection("Collection with throwing async setup")]
		class ClassWithThrowingFixtureSetupAsync
		{
			[Fact]
			public void TheTest() { }
		}

		class ThrowingSetupAsync : IAsyncLifetime
		{
			public ValueTask InitializeAsync()
			{
				throw new DivideByZeroException();
			}

			public ValueTask DisposeAsync()
			{
				return default;
			}
		}

		[Fact]
		public async void TestClassWithThrowingCollectionFixtureDisposeAsyncResultsInFailedTest()
		{
			var messages = await RunAsync<_TestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureAsyncDispose));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[CollectionDefinition("Collection with throwing async Dispose")]
		public class CollectionWithThrowingAsyncDispose : ICollectionFixture<ThrowingDisposeAsync> { }

		[Collection("Collection with throwing async Dispose")]
		class ClassWithThrowingFixtureAsyncDispose
		{
			[Fact]
			public void TheTest() { }
		}

		class ThrowingDisposeAsync : IAsyncLifetime
		{
			public ValueTask InitializeAsync()
			{
				return default;
			}

			public ValueTask DisposeAsync()
			{
				throw new DivideByZeroException();
			}
		}

		[Fact]
		public async void TestClassWithThrowingCollectionFixtureDisposeAsyncResultsInFailedTest_Disposable()
		{
			var messages = await RunAsync<_TestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureAsyncDispose_Disposable));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[CollectionDefinition("Collection with throwing async Dispose (Disposable)")]
		public class CollectionWithThrowingAsyncDispose_Disposable : ICollectionFixture<ThrowingDisposeAsync_Disposable> { }

		[Collection("Collection with throwing async Dispose (Disposable)")]
		class ClassWithThrowingFixtureAsyncDispose_Disposable
		{
			[Fact]
			public void TheTest() { }
		}

		class ThrowingDisposeAsync_Disposable : IAsyncDisposable
		{
			public ValueTask DisposeAsync()
			{
				throw new DivideByZeroException();
			}
		}

		[Fact]
		public async void CollectionFixtureAsyncSetupShouldOnlyRunOnce()
		{
			var results = await RunAsync<ITestPassed>(new[] { typeof(Fixture1), typeof(Fixture2) });
			Assert.Equal(2, results.Count);
		}

		class Alpha { }
		class Beta { }

		/// <remarks>
		/// We include two class fixtures and test that each one is only initialised once.
		/// Regression testing for https://github.com/xunit/xunit/issues/869
		/// </remarks>
		[CollectionDefinition("Async once")]
		public class AsyncOnceCollection : ICollectionFixture<CountedAsyncFixture<Alpha>>, ICollectionFixture<CountedAsyncFixture<Beta>> { }

		[Collection("Async once")]
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

		[Collection("Async once")]
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

		class CountedAsyncFixture<T> : IAsyncLifetime
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
	}

	class EmptyFixtureData { }

	class ThrowingCtorFixture
	{
		public ThrowingCtorFixture()
		{
			throw new DivideByZeroException();
		}
	}

	class ThrowingDisposeFixture : IDisposable
	{
		public void Dispose()
		{
			throw new DivideByZeroException();
		}
	}

	class CountedFixture
	{
		static int counter = 0;

		public CountedFixture()
		{
			Identity = ++counter;
		}

		public readonly int Identity;
	}
}
