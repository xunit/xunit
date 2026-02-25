using Xunit;
using Xunit.Sdk;

partial class FixtureAcceptanceTests
{
	partial class ClassFixture : AcceptanceTestV3
	{
		// Native AOT reports this in the generator
		[Fact]
		public async ValueTask TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var messages = await RunAsync(typeof(ClassWithExtraCtorArg));

			Assert.Collection(
				messages,
				message => Assert.IsType<ITestAssemblyStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestCollectionStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestClassStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestMethodStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestCaseStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestClassConstructionStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestClassConstructionFinished>(message, exactMatch: false),
				message =>
				{
					var failedMessage = Assert.IsType<ITestFailed>(message, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 _1, String _3", failedMessage.Messages.Single());
				},
				message => Assert.IsType<ITestFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestCaseFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestMethodFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestClassFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestCollectionFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestAssemblyFinished>(message, exactMatch: false)
			);
		}

#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources

		class ClassWithExtraCtorArg(int _1, EmptyFixtureData _2, string _3) :
			IClassFixture<EmptyFixtureData>
		{
			[Fact]
			public void TheTest() { }
		}

#pragma warning restore xUnit1041 // Fixture arguments to test classes must have fixture sources
	}

	partial class CollectionFixture
	{
		// Native AOT reports these in the generator
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

		// Native AOT does not support generic collection definitions
		[Fact]
		public async ValueTask CollectionFixtureOnGenericTestClassAcceptsArgument()
		{
			var messages = await RunAsync<ITestPassed>(typeof(GenericTests));

			Assert.Equal(2, messages.Count);
		}

		[CollectionDefinition("generic collection")]
		public class GenericFixtureCollection<T> : ICollectionFixture<GenericFixture<T>> { }

		[Collection("generic collection")]
		abstract class GenericTestBase<T>(GenericFixture<T> fixture)
		{
			protected readonly GenericFixture<T> Fixture = fixture;
		}

		public class GenericArgument { }

		// TODO: Attributes are not being inherited because of GetCustomAttributesData
		class GenericTests(GenericFixture<GenericArgument> fixture) :
			GenericTestBase<GenericArgument>(fixture)
		{
			[Fact] public void Test1() => Assert.NotNull(Fixture);
			[Fact] public void Test2() { }
		}
	}

	partial class CollectionFixtureByTypeArgument
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
	}

#if !NETFRAMEWORK

	partial class CollectionFixtureGeneric
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
	}

#endif  // !NETFRAMEWORK

	public class Constructors : AcceptanceTestV3
	{
		// Native AOT reports this in the generator
		[Fact]
		public async ValueTask TestClassMustHaveSinglePublicConstructor()
		{
			var messages = await RunAsync(typeof(ClassWithTooManyConstructors));

			Assert.Collection(
				messages,
				message => Assert.IsType<ITestAssemblyStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestCollectionStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestClassStarting>(message, exactMatch: false),

				// TestMethod1
				message => Assert.IsType<ITestMethodStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestCaseStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestStarting>(message, exactMatch: false),
				message =>
				{
					var failedMessage = Assert.IsType<ITestFailed>(message, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failedMessage.Messages.Single());
				},
				message => Assert.IsType<ITestFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestCaseFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestMethodFinished>(message, exactMatch: false),

				// TestMethod2
				message => Assert.IsType<ITestMethodStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestCaseStarting>(message, exactMatch: false),
				message => Assert.IsType<ITestStarting>(message, exactMatch: false),
				message =>
				{
					var failedMessage = Assert.IsType<ITestFailed>(message, exactMatch: false);
					Assert.Equal(typeof(TestPipelineException).SafeName(), failedMessage.ExceptionTypes.Single());
					Assert.Equal("A test class may only define a single public constructor.", failedMessage.Messages.Single());
				},
				message => Assert.IsType<ITestFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestCaseFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestMethodFinished>(message, exactMatch: false),

				message => Assert.IsType<ITestClassFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestCollectionFinished>(message, exactMatch: false),
				message => Assert.IsType<ITestAssemblyFinished>(message, exactMatch: false)
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

	sealed class GenericFixture<T> : IAsyncLifetime, IDisposable
	{
		public GenericFixture() { }
		public void Dispose() { }
		public ValueTask InitializeAsync() => default;
		public ValueTask DisposeAsync() => default;
	}
}
