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
				message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
				message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
				message => Assert.IsAssignableFrom<ITestClassStarting>(message),

				// TestMethod1
				message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
				message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message =>
				{
					var failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failedMessage.Messages.Single());
				},
				message => Assert.IsAssignableFrom<ITestFinished>(message),
				message => Assert.IsAssignableFrom<ITestCaseFinished>(message),
				message => Assert.IsAssignableFrom<ITestMethodFinished>(message),

				// TestMethod2
				message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
				message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message =>
				{
					var failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failedMessage.Messages.Single());
				},
				message => Assert.IsAssignableFrom<ITestFinished>(message),
				message => Assert.IsAssignableFrom<ITestCaseFinished>(message),
				message => Assert.IsAssignableFrom<ITestMethodFinished>(message),

				message => Assert.IsAssignableFrom<ITestClassFinished>(message),
				message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
				message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
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
				message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
				message => Assert.IsAssignableFrom<ITestCollectionStarting>(message),
				message => Assert.IsAssignableFrom<ITestClassStarting>(message),
				message => Assert.IsAssignableFrom<ITestMethodStarting>(message),
				message => Assert.IsAssignableFrom<ITestCaseStarting>(message),
				message => Assert.IsAssignableFrom<ITestStarting>(message),
				message =>
				{
					var failedMessage = Assert.IsAssignableFrom<ITestFailed>(message);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", failedMessage.Messages.Single());
				},
				message => Assert.IsAssignableFrom<ITestFinished>(message),
				message => Assert.IsAssignableFrom<ITestCaseFinished>(message),
				message => Assert.IsAssignableFrom<ITestMethodFinished>(message),
				message => Assert.IsAssignableFrom<ITestClassFinished>(message),
				message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
				message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
			);
		}

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

		class ClassWithExtraCtorArg : IClassFixture<EmptyFixtureData>
		{
			public ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) { }

			[Fact]
			public void TheTest() { }
		}

#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithMissingCtorArg));

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
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureCtor));

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
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithCtorAndThrowingFixtureCtor));

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
			var messages = await RunAsync<ITestClassCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

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
		public async ValueTask FixtureDataIsPassedToConstructorAndAvailableViaContext()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

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

		[Fact]
		public async ValueTask TestClassWithDefaultParameter()
		{
			var messages = await RunAsync(typeof(ClassWithDefaultCtorArg));

			Assert.Single(messages.OfType<ITestPassed>());
			var starting = Assert.Single(messages.OfType<ITestStarting>());
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

			Assert.Single(messages.OfType<ITestPassed>());
			var starting = Assert.Single(messages.OfType<ITestStarting>());
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

			Assert.Single(messages.OfType<ITestPassed>());
			var starting = Assert.Single(messages.OfType<ITestStarting>());
			Assert.Equal("FixtureAcceptanceTests+ClassFixture+ClassWithParamsArg.TheTest", starting.TestDisplayName);
		}

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

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

#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources

		[Fact]
		public async ValueTask ClassFixtureCanAcceptIMessageSink()
		{
			var messages = await RunForResultsAsync(typeof(ClassWithMessageSinkFixture));

			var passed = Assert.Single(messages.OfType<TestPassedWithDisplayName>());
			Assert.Equal("FixtureAcceptanceTests+ClassFixture+ClassWithMessageSinkFixture.MessageSinkWasInjected", passed.TestDisplayName);
		}

		class ClassWithMessageSinkFixture : IClassFixture<MessageSinkFixture>
		{
			readonly MessageSinkFixture fixture;

			public ClassWithMessageSinkFixture(MessageSinkFixture fixture) =>
				this.fixture = fixture;

			[Fact]
			public void MessageSinkWasInjected() =>
				Assert.NotNull(fixture.MessageSink);
		}

		class MessageSinkFixture
		{
			public MessageSinkFixture(IMessageSink messageSink) =>
				MessageSink = messageSink;

			public IMessageSink MessageSink { get; }
		}
	}

	public class AsyncClassFixture : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask FixtureDataShouldHaveBeenSetup()
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
		public async ValueTask ThrowingFixtureInitializeAsyncShouldResultInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureInitializeAsync));

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
			var messages = await RunAsync<ITestClassCleanupFailure>(typeof(ClassWithThrowingFixtureDisposeAsync));

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
			var messages = await RunAsync<ITestFailed>(typeof(TestClassWithCollectionFixture));

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
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", msg.Messages.Single());
		}

		[CollectionDefinition("Collection with empty fixture data")]
		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>, ICollectionFixture<object> { }

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

		[Collection("Collection with empty fixture data")]
		class ClassWithExtraCtorArg
		{
			public ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) { }

			[Fact]
			public void TheTest() { }
		}

#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithMissingCtorArg));

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
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureCtor));

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
			var messages = await RunAsync<ITestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

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
		public async ValueTask FixtureDataIsPassedToConstructorAndAvailableViaContext()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

		[Collection("Collection with empty fixture data")]
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

		[Fact]
		public async ValueTask FixtureDataIsSameInstanceAcrossClasses()
		{
			var results = await RunForResultsAsync([typeof(FixtureSaver1), typeof(FixtureSaver2)]);

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().OrderBy(p => p.TestDisplayName),
				passed => Assert.Equal("FixtureAcceptanceTests+CollectionFixture+FixtureSaver1.TheTest", passed.TestDisplayName),
				passed => Assert.Equal("FixtureAcceptanceTests+CollectionFixture+FixtureSaver2.TheTest", passed.TestDisplayName)
			);
			Assert.NotNull(FixtureSaver1.Fixture);
			Assert.Same(FixtureSaver1.Fixture, FixtureSaver2.Fixture);
		}

		[Collection("Collection with empty fixture data")]
		class FixtureSaver1
		{
			public static EmptyFixtureData? Fixture;

			public FixtureSaver1(EmptyFixtureData data) =>
				Fixture = data;

			[Fact]
			public void TheTest() { }
		}

		[Collection("Collection with empty fixture data")]
		class FixtureSaver2
		{
			public static EmptyFixtureData? Fixture;

			public FixtureSaver2(EmptyFixtureData data) =>
				Fixture = data;

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask ClassFixtureOnCollectionDecorationWorks()
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
		public async ValueTask ClassFixtureOnTestClassTakesPrecedenceOverClassFixtureOnCollection()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithCountedFixture));

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
			var messages = await RunAsync<ITestPassed>(typeof(GenericTests));

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
			public GenericTests(GenericFixture<GenericArgument> fixture) : base(fixture) { }

			[Fact] public void Test1() => Assert.NotNull(Fixture);
			[Fact] public void Test2() { }
		}

		[Fact]
		public async ValueTask CollectionFixtureCanAcceptIMessageSink()
		{
			var messages = await RunForResultsAsync(typeof(ClassWithMessageSinkFixture));

			var passed = Assert.Single(messages.OfType<TestPassedWithDisplayName>());
			Assert.Equal("FixtureAcceptanceTests+CollectionFixture+ClassWithMessageSinkFixture.MessageSinkWasInjected", passed.TestDisplayName);
		}

		[CollectionDefinition("collection with message sink fixture")]
		public class ClassWithMessageSinkFixtureCollection : ICollectionFixture<MessageSinkFixture> { }

		[Collection("collection with message sink fixture")]
		class ClassWithMessageSinkFixture
		{
			readonly MessageSinkFixture fixture;

			public ClassWithMessageSinkFixture(MessageSinkFixture fixture) =>
				this.fixture = fixture;

			[Fact]
			public void MessageSinkWasInjected() =>
				Assert.NotNull(fixture.MessageSink);
		}

		class MessageSinkFixture
		{
			public MessageSinkFixture(IMessageSink messageSink) =>
				MessageSink = messageSink;

			public IMessageSink MessageSink { get; }
		}
	}

	public class CollectionFixtureByTypeArgument : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassCannotBeDecoratedWithICollectionFixture()
		{
			var messages = await RunAsync<ITestFailed>(typeof(TestClassWithCollectionFixture));

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
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", msg.Messages.Single());
		}

		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>, ICollectionFixture<object> { }

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

		[Collection(typeof(CollectionWithEmptyFixtureData))]
		class ClassWithExtraCtorArg
		{
			public ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) { }

			[Fact]
			public void TheTest() { }
		}

#pragma warning restore xUnit1041

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithMissingCtorArg));

			Assert.Single(messages);
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
		class ClassWithMissingCtorArg
		{
			public ClassWithMissingCtorArg(EmptyFixtureData _) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureCtor));

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
			var messages = await RunAsync<ITestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingDisposeFixture' threw in Dispose", msg.Messages.First());
		}

		public class CollectionWithThrowingDispose : ICollectionFixture<ThrowingDisposeFixture> { }

		[Collection(typeof(CollectionWithThrowingDispose))]
		class ClassWithThrowingFixtureDispose
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask FixtureDataIsPassedToConstructorAndAvailableViaContext()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
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

		[Fact]
		public async ValueTask FixtureDataIsSameInstanceAcrossClasses()
		{
			var results = await RunForResultsAsync([typeof(FixtureSaver1), typeof(FixtureSaver2)]);

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().OrderBy(p => p.TestDisplayName),
				passed => Assert.Equal("FixtureAcceptanceTests+CollectionFixtureByTypeArgument+FixtureSaver1.TheTest", passed.TestDisplayName),
				passed => Assert.Equal("FixtureAcceptanceTests+CollectionFixtureByTypeArgument+FixtureSaver2.TheTest", passed.TestDisplayName)
			);
			Assert.NotNull(FixtureSaver1.Fixture);
			Assert.Same(FixtureSaver1.Fixture, FixtureSaver2.Fixture);
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
		class FixtureSaver1
		{
			public static EmptyFixtureData? Fixture;

			public FixtureSaver1(EmptyFixtureData data) =>
				Fixture = data;

			[Fact]
			public void TheTest() { }
		}

		[Collection(typeof(CollectionWithEmptyFixtureData))]
		class FixtureSaver2
		{
			public static EmptyFixtureData? Fixture;

			public FixtureSaver2(EmptyFixtureData data) =>
				Fixture = data;

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask ClassFixtureOnCollectionDecorationWorks()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy_ClassFixture));

			Assert.Single(messages);
		}

		public class CollectionWithClassFixture : IClassFixture<EmptyFixtureData> { }

		[Collection(typeof(CollectionWithClassFixture))]
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
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithCountedFixture));

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

#if !NETFRAMEWORK

	public class CollectionFixtureGeneric : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassCannotBeDecoratedWithICollectionFixture()
		{
			var messages = await RunAsync<ITestFailed>(typeof(TestClassWithCollectionFixture));

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
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", msg.Messages.Single());
		}

		public class CollectionWithEmptyFixtureData : ICollectionFixture<EmptyFixtureData>, ICollectionFixture<object> { }

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

		[Collection<CollectionWithEmptyFixtureData>]
		class ClassWithExtraCtorArg
		{
			public ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) { }

			[Fact]
			public void TheTest() { }
		}

#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithMissingCtorArg));

			Assert.Single(messages);
		}

		[Collection<CollectionWithEmptyFixtureData>]
		class ClassWithMissingCtorArg
		{
			public ClassWithMissingCtorArg(EmptyFixtureData _) { }

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureCtor));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingCtorFixture' threw in its constructor", msg.Messages.First());
		}

		public class CollectionWithThrowingCtor : ICollectionFixture<ThrowingCtorFixture> { }

		[Collection<CollectionWithThrowingCtor>]
		class ClassWithThrowingFixtureCtor
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingCollectionFixtureDisposeResultsInFailedTest()
		{
			var messages = await RunAsync<ITestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestPipelineException).SafeName(), exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).SafeName(), exceptionTypeName)
			);
			Assert.Equal("Collection fixture type 'FixtureAcceptanceTests+ThrowingDisposeFixture' threw in Dispose", msg.Messages.First());
		}

		public class CollectionWithThrowingDispose : ICollectionFixture<ThrowingDisposeFixture> { }

		[Collection<CollectionWithThrowingDispose>]
		class ClassWithThrowingFixtureDispose
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask FixtureDataIsPassedToConstructor()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy));

			Assert.Single(messages);
		}

		[Collection<CollectionWithEmptyFixtureData>]
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
			var results = await RunForResultsAsync([typeof(FixtureSaver1), typeof(FixtureSaver2)]);

			Assert.Collection(
				results.OfType<TestPassedWithDisplayName>().OrderBy(p => p.TestDisplayName),
				passed => Assert.Equal("FixtureAcceptanceTests+CollectionFixtureGeneric+FixtureSaver1.TheTest", passed.TestDisplayName),
				passed => Assert.Equal("FixtureAcceptanceTests+CollectionFixtureGeneric+FixtureSaver2.TheTest", passed.TestDisplayName)
			);
			Assert.NotNull(FixtureSaver1.Fixture);
			Assert.Same(FixtureSaver1.Fixture, FixtureSaver2.Fixture);
		}

		[Collection<CollectionWithEmptyFixtureData>]
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

		[Collection<CollectionWithEmptyFixtureData>]
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
		public async ValueTask ClassFixtureOnCollectionDecorationWorks()
		{
			var messages = await RunAsync<ITestPassed>(typeof(FixtureSpy_ClassFixture));

			Assert.Single(messages);
		}

		public class CollectionWithClassFixture : IClassFixture<EmptyFixtureData> { }

		[Collection<CollectionWithClassFixture>]
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
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithCountedFixture));

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
	}

#endif

	public class AsyncCollectionFixture : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassWithThrowingCollectionFixtureSetupAsyncResultsInFailedTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithThrowingFixtureInitializeAsync));

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
			var messages = await RunAsync<ITestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDisposeAsync));

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
			var results = await RunAsync<ITestPassed>([typeof(Fixture1), typeof(Fixture2)]);
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
