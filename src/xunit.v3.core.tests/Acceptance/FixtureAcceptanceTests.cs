#pragma warning disable IDE0290  // Lots of things in here can't use primary constructors

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class FixtureAcceptanceTests
{
	public class Constructors : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassMustHaveSinglePublicConstructor()
		{
			var messages = await RunAsync(typeof(ClassWithTooManyConstructors));

			Assert.Collection(
				messages,
				message => Assert.IsType<_TestAssemblyStarting>(message),
				message => Assert.IsType<_TestCollectionStarting>(message),
				message => Assert.IsType<_TestClassStarting>(message),

				// TestMethod1
				message => Assert.IsType<_TestMethodStarting>(message),
				message => Assert.IsType<_TestCaseStarting>(message),
				message => Assert.IsType<_TestStarting>(message),
				message =>
				{
					var failedMessage = Assert.IsType<_TestFailed>(message);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failedMessage.Messages.Single());
				},
				message => Assert.IsType<_TestFinished>(message),
				message => Assert.IsType<_TestCaseFinished>(message),
				message => Assert.IsType<_TestMethodFinished>(message),

				// TestMethod2
				message => Assert.IsType<_TestMethodStarting>(message),
				message => Assert.IsType<_TestCaseStarting>(message),
				message => Assert.IsType<_TestStarting>(message),
				message =>
				{
					var failedMessage = Assert.IsType<_TestFailed>(message);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failedMessage.Messages.Single());
				},
				message => Assert.IsType<_TestFinished>(message),
				message => Assert.IsType<_TestCaseFinished>(message),
				message => Assert.IsType<_TestMethodFinished>(message),

				message => Assert.IsType<_TestClassFinished>(message),
				message => Assert.IsType<_TestCollectionFinished>(message),
				message => Assert.IsType<_TestAssemblyFinished>(message)
			);
		}

		class ClassWithTooManyConstructors
		{
			public ClassWithTooManyConstructors() { }

			public ClassWithTooManyConstructors(int _) { }

			[Fact]
			public void TestMethod1() { }

			[Fact]
			public void TestMethod2() { }
		}
	}

	public class ClassFixture : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var messages = await RunAsync(typeof(ClassWithExtraCtorArg));

			Assert.Collection(
				messages,
				message => Assert.IsType<_TestAssemblyStarting>(message),
				message => Assert.IsType<_TestCollectionStarting>(message),
				message => Assert.IsType<_TestClassStarting>(message),
				message => Assert.IsType<_TestMethodStarting>(message),
				message => Assert.IsType<_TestCaseStarting>(message),
				message => Assert.IsType<_TestStarting>(message),
				message =>
				{
					var failedMessage = Assert.IsType<_TestFailed>(message);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", failedMessage.Messages.Single());
				},
				message => Assert.IsType<_TestFinished>(message),
				message => Assert.IsType<_TestCaseFinished>(message),
				message => Assert.IsType<_TestMethodFinished>(message),
				message => Assert.IsType<_TestClassFinished>(message),
				message => Assert.IsType<_TestCollectionFinished>(message),
				message => Assert.IsType<_TestAssemblyFinished>(message)
			);
		}

		class ClassWithExtraCtorArg : IClassFixture<EmptyFixtureData>
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) { }
#pragma warning restore xUnit1041

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<_TestPassed>(typeof(ClassWithMissingCtorArg));

			Assert.Single(messages);
		}

		class ClassWithMissingCtorArg : IClassFixture<EmptyFixtureData>, IClassFixture<object>
		{
			public ClassWithMissingCtorArg(EmptyFixtureData _) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithoutCtorWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithThrowingFixtureCtor));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Class fixture type 'FixtureAcceptanceTests+ThrowingCtorFixture' threw in its constructor", msg.Messages.First());
		}

		class ClassWithThrowingFixtureCtor : IClassFixture<ThrowingCtorFixture>
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithCtorWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithCtorAndThrowingFixtureCtor));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Class fixture type 'FixtureAcceptanceTests+ThrowingCtorFixture' threw in its constructor", msg.Messages.First());
		}

		class ClassWithCtorAndThrowingFixtureCtor : IClassFixture<ThrowingCtorFixture>
		{
			public ClassWithCtorAndThrowingFixtureCtor(ThrowingCtorFixture _) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<_TestClassCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Class fixture type 'FixtureAcceptanceTests+ThrowingDisposeFixture' threw in Dispose", msg.Messages.First());
		}

		class ClassWithThrowingFixtureDispose : IClassFixture<ThrowingDisposeFixture>
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask FixtureDataIsPassedToConstructor()
		{
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy));

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
		public async ValueTask TestClassWithDefaultParameter()
		{
			var messages = await RunAsync(typeof(ClassWithDefaultCtorArg));

			Assert.Single(messages.OfType<_TestPassed>());
			var starting = Assert.Single(messages.OfType<_TestStarting>());
			Assert.Equal("FixtureAcceptanceTests+ClassFixture+ClassWithDefaultCtorArg.TheTest", starting.TestDisplayName);
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
		public async ValueTask TestClassWithOptionalParameter()
		{
			var messages = await RunAsync(typeof(ClassWithOptionalCtorArg));

			Assert.Single(messages.OfType<_TestPassed>());
			var starting = Assert.Single(messages.OfType<_TestStarting>());
			Assert.Equal("FixtureAcceptanceTests+ClassFixture+ClassWithOptionalCtorArg.TheTest", starting.TestDisplayName);
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
		public async ValueTask TestClassWithParamsParameter()
		{
			var messages = await RunAsync(typeof(ClassWithParamsArg));

			Assert.Single(messages.OfType<_TestPassed>());
			var starting = Assert.Single(messages.OfType<_TestStarting>());
			Assert.Equal("FixtureAcceptanceTests+ClassFixture+ClassWithParamsArg.TheTest", starting.TestDisplayName);
		}

		class ClassWithParamsArg : IClassFixture<EmptyFixtureData>
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithParamsArg(EmptyFixtureData fixture, params object[] x)
#pragma warning restore xUnit1041
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
		public async ValueTask FixtureDataShouldHaveBeenSetup()
		{
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy));

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
		public async ValueTask ThrowingFixtureInitializeAsyncShouldResultInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithThrowingFixtureInitializeAsync));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Class fixture type 'FixtureAcceptanceTests+ThrowingInitializeAsyncFixture' threw in InitializeAsync", msg.Messages.First());
		}

		class ClassWithThrowingFixtureInitializeAsync : IClassFixture<ThrowingInitializeAsyncFixture>
		{
			public ClassWithThrowingFixtureInitializeAsync(ThrowingInitializeAsyncFixture _) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureAsyncDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<_TestClassCleanupFailure>(typeof(ClassWithThrowingFixtureDisposeAsync));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Class fixture type 'FixtureAcceptanceTests+ThrowingDisposeAsyncFixture' threw in DisposeAsync", msg.Messages.First());
		}

		class ClassWithThrowingFixtureDisposeAsync : IClassFixture<ThrowingDisposeAsyncFixture>
		{
			public ClassWithThrowingFixtureDisposeAsync(ThrowingDisposeAsyncFixture _) { }

			[Fact]
			public void TheTest() { }
		}
	}

	public class CollectionFixture : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassCannotBeDecoratedWithICollectionFixture()
		{
			var messages = await RunAsync<_TestFailed>(typeof(TestClassWithCollectionFixture));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", msg.Messages.Single());
		}

		class TestClassWithCollectionFixture : ICollectionFixture<EmptyFixtureData>
		{
			[Fact]
			public void TestMethod() { }
		}

		[Fact]
		public async ValueTask TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", msg.Messages.Single());
		}

		[CollectionDefinition("Collection with empty fixture data")]
		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>
		{
		}

		[Collection("Collection with empty fixture data")]
		class ClassWithExtraCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) { }
#pragma warning restore xUnit1041

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<_TestPassed>(typeof(ClassWithMissingCtorArg));

			Assert.Single(messages);
		}

		[Collection("Collection with empty fixture data")]
		class ClassWithMissingCtorArg
		{
			public ClassWithMissingCtorArg(EmptyFixtureData _) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithThrowingFixtureCtor));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingCtorFixture' threw in its constructor", msg.Messages.First());
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
		public async ValueTask TestClassWithThrowingCollectionFixtureDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<_TestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingDisposeFixture' threw in Dispose", msg.Messages.First());
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
		public async ValueTask FixtureDataIsPassedToConstructor()
		{
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy));

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
		public async ValueTask FixtureDataIsSameInstanceAcrossClasses()
		{
			await RunAsync<_TestPassed>([typeof(FixtureSaver1), typeof(FixtureSaver2)]);

			Assert.Same(FixtureSaver1.Fixture, FixtureSaver2.Fixture);
		}

		class FixtureSaver1
		{
			public static EmptyFixtureData? Fixture;

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSaver1(EmptyFixtureData data)
#pragma warning restore xUnit1041
			{
				Fixture = data;
			}

			[Fact]
			public void TheTest() { }
		}

		class FixtureSaver2
		{
			public static EmptyFixtureData? Fixture;

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSaver2(EmptyFixtureData data)
#pragma warning restore xUnit1041
			{
				Fixture = data;
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask ClassFixtureOnCollectionDecorationWorks()
		{
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy_ClassFixture));

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
		public async ValueTask ClassFixtureOnTestClassTakesPrecedenceOverClassFixtureOnCollection()
		{
			var messages = await RunAsync<_TestPassed>(typeof(ClassWithCountedFixture));

			Assert.Single(messages);
		}

		class CountedFixture
		{
			static int counter = 0;

			public CountedFixture() => Identity = ++counter;

			public readonly int Identity;
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

		[Fact]
		public async ValueTask CollectionFixtureOnGenericTestClassAcceptsArgument()
		{
			var messages = await RunAsync<_TestPassed>(typeof(GenericTests));

			Assert.Equal(2, messages.Count);
		}

		[CollectionDefinition("generic collection")]
		public class GenericFixtureCollection<T> : ICollectionFixture<GenericFixture<T>> { }

		[Collection("generic collection")]
		abstract class GenericTestBase<T>
		{
			protected GenericTestBase(GenericFixture<T> fixture) => Fixture = fixture;
			protected readonly GenericFixture<T> Fixture;
		}

		public class GenericArgument { }

		class GenericTests : GenericTestBase<GenericArgument>
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public GenericTests(GenericFixture<GenericArgument> fixture) : base(fixture) { }
#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources
			[Fact] public void Test1() => Assert.NotNull(Fixture);
			[Fact] public void Test2() { }
		}
	}

	public class CollectionFixtureByTypeArgument : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassCannotBeDecoratedWithICollectionFixture()
		{
			var messages = await RunAsync<_TestFailed>(typeof(TestClassWithCollectionFixture));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", msg.Messages.Single());
		}

		class TestClassWithCollectionFixture : ICollectionFixture<EmptyFixtureData>
		{
			[Fact]
			public void TestMethod() { }
		}

		[Fact]
		public async ValueTask TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", msg.Messages.Single());
		}

		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>
		{
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
		class ClassWithExtraCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) { }
#pragma warning restore xUnit1041

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<_TestPassed>(typeof(ClassWithMissingCtorArg));

			Assert.Single(messages);
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
		class ClassWithMissingCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithMissingCtorArg(EmptyFixtureData _) { }
#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithThrowingFixtureCtor));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingCtorFixture' threw in its constructor", msg.Messages.First());
		}

		public class CollectionWithThrowingCtor : ICollectionFixture<ThrowingCtorFixture>
		{
		}

		[Collection(typeof(CollectionWithThrowingCtor))]
		class ClassWithThrowingFixtureCtor
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingCollectionFixtureDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<_TestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingDisposeFixture' threw in Dispose", msg.Messages.First());
		}

		public class CollectionWithThrowingDispose : ICollectionFixture<ThrowingDisposeFixture>
		{
		}

		[Collection(typeof(CollectionWithThrowingDispose))]
		class ClassWithThrowingFixtureDispose
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask FixtureDataIsPassedToConstructor()
		{
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
		class FixtureSpy
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSpy(EmptyFixtureData data)
#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask FixtureDataIsSameInstanceAcrossClasses()
		{
			await RunAsync<_TestPassed>([typeof(FixtureSaver1), typeof(FixtureSaver2)]);

			Assert.Same(FixtureSaver1.Fixture, FixtureSaver2.Fixture);
		}

		class FixtureSaver1
		{
			public static EmptyFixtureData? Fixture;

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSaver1(EmptyFixtureData data)
#pragma warning restore xUnit1041
			{
				Fixture = data;
			}

			[Fact]
			public void TheTest() { }
		}

		class FixtureSaver2
		{
			public static EmptyFixtureData? Fixture;

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSaver2(EmptyFixtureData data)
#pragma warning restore xUnit1041
			{
				Fixture = data;
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask ClassFixtureOnCollectionDecorationWorks()
		{
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy_ClassFixture));

			Assert.Single(messages);
		}

		public class CollectionWithClassFixture : IClassFixture<EmptyFixtureData> { }

		[Collection(typeof(CollectionWithClassFixture))]
		class FixtureSpy_ClassFixture
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSpy_ClassFixture(EmptyFixtureData data)
#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask ClassFixtureOnTestClassTakesPrecedenceOverClassFixtureOnCollection()
		{
			var messages = await RunAsync<_TestPassed>(typeof(ClassWithCountedFixture));

			Assert.Single(messages);
		}

		class CountedFixture
		{
			static int counter = 0;

			public CountedFixture() => Identity = ++counter;

			public readonly int Identity;
		}

		public class CollectionWithClassFixtureCounter : ICollectionFixture<CountedFixture> { }

		[Collection(typeof(CollectionWithClassFixtureCounter))]
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

	public class CollectionFixtureGeneric : AcceptanceTestV3
	{
#if !NETFRAMEWORK

		[Fact]
		public async ValueTask TestClassCannotBeDecoratedWithICollectionFixture()
		{
			var messages = await RunAsync<_TestFailed>(typeof(TestClassWithCollectionFixture));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).", msg.Messages.Single());
		}

		class TestClassWithCollectionFixture : ICollectionFixture<EmptyFixtureData>
		{
			[Fact]
			public void TestMethod() { }
		}

		[Fact]
		public async ValueTask TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", msg.Messages.Single());
		}

		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>
		{
		}

		[Collection<CollectionWithEmptyFixtureData>]
		class ClassWithExtraCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) { }
#pragma warning restore xUnit1041

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<_TestPassed>(typeof(ClassWithMissingCtorArg));

			Assert.Single(messages);
		}

		[Collection<CollectionWithEmptyFixtureData>]
		class ClassWithMissingCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithMissingCtorArg(EmptyFixtureData _) { }
#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithThrowingFixtureCtor));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingCtorFixture' threw in its constructor", msg.Messages.First());
		}

		public class CollectionWithThrowingCtor : ICollectionFixture<ThrowingCtorFixture>
		{
		}

		[Collection<CollectionWithThrowingCtor>]
		class ClassWithThrowingFixtureCtor
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingCollectionFixtureDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<_TestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingDisposeFixture' threw in Dispose", msg.Messages.First());
		}

		public class CollectionWithThrowingDispose : ICollectionFixture<ThrowingDisposeFixture>
		{
		}

		[Collection<CollectionWithThrowingDispose>]
		class ClassWithThrowingFixtureDispose
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask FixtureDataIsPassedToConstructor()
		{
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
		class FixtureSpy
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSpy(EmptyFixtureData data)
#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask FixtureDataIsSameInstanceAcrossClasses()
		{
			await RunAsync<_TestPassed>([typeof(FixtureSaver1), typeof(FixtureSaver2)]);

			Assert.Same(FixtureSaver1.Fixture, FixtureSaver2.Fixture);
		}

		class FixtureSaver1
		{
			public static EmptyFixtureData? Fixture;

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSaver1(EmptyFixtureData data)
#pragma warning restore xUnit1041
			{
				Fixture = data;
			}

			[Fact]
			public void TheTest() { }
		}

		class FixtureSaver2
		{
			public static EmptyFixtureData? Fixture;

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSaver2(EmptyFixtureData data)
#pragma warning restore xUnit1041
			{
				Fixture = data;
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask ClassFixtureOnCollectionDecorationWorks()
		{
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy_ClassFixture));

			Assert.Single(messages);
		}

		public class CollectionWithClassFixture : IClassFixture<EmptyFixtureData> { }

		[Collection<CollectionWithClassFixture>]
		class FixtureSpy_ClassFixture
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSpy_ClassFixture(EmptyFixtureData data)
#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask ClassFixtureOnTestClassTakesPrecedenceOverClassFixtureOnCollection()
		{
			var messages = await RunAsync<_TestPassed>(typeof(ClassWithCountedFixture));

			Assert.Single(messages);
		}

		class CountedFixture
		{
			static int counter = 0;

			public CountedFixture() => Identity = ++counter;

			public readonly int Identity;
		}

		public class CollectionWithClassFixtureCounter : ICollectionFixture<CountedFixture> { }

		[Collection<CollectionWithClassFixtureCounter>]
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
#endif
	}

	public class AsyncCollectionFixture : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassWithThrowingCollectionFixtureSetupAsyncResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithThrowingFixtureInitializeAsync));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingInitializeAsyncFixture' threw in InitializeAsync", msg.Messages.First());
		}

		[CollectionDefinition("Collection with throwing InitializeAsync")]
		public class CollectionWithThrowingInitializeAsync : ICollectionFixture<ThrowingInitializeAsyncFixture> { }

		[Collection("Collection with throwing InitializeAsync")]
		class ClassWithThrowingFixtureInitializeAsync
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingCollectionFixtureDisposeAsyncResultsInFailedTest()
		{
			var messages = await RunAsync<_TestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDisposeAsync));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingDisposeAsyncFixture' threw in DisposeAsync", msg.Messages.First());
		}

		[CollectionDefinition("Collection with throwing DisposeAsync")]
		public class CollectionWithThrowingDisposeAsync : ICollectionFixture<ThrowingDisposeAsyncFixture> { }

		[Collection("Collection with throwing DisposeAsync")]
		class ClassWithThrowingFixtureDisposeAsync
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask CollectionFixtureAsyncSetupShouldOnlyRunOnce()
		{
			var results = await RunAsync<_TestPassed>([typeof(Fixture1), typeof(Fixture2)]);
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
		public ThrowingCtorFixture() => throw new DivideByZeroException();
	}

	class ThrowingInitializeAsyncFixture : IAsyncLifetime
	{
		public ValueTask DisposeAsync() => default;

		public ValueTask InitializeAsync() => throw new DivideByZeroException();
	}

	class ThrowingDisposeFixture : IDisposable
	{
		public void Dispose() => throw new DivideByZeroException();
	}

	class ThrowingDisposeAsyncFixture : IAsyncDisposable
	{
		public ValueTask DisposeAsync() => throw new DivideByZeroException();
	}

	sealed class GenericFixture<T> : IAsyncLifetime, IDisposable
	{
		public GenericFixture() { }
		public void Dispose() { }
		public ValueTask InitializeAsync() => default;
		public ValueTask DisposeAsync() => default;
	}
}
