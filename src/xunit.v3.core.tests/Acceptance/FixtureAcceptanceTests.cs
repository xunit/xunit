using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.v3;

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
					Assert.Equal(typeof(TestClassException).FullName, failedMessage.ExceptionTypes.Single());
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
					Assert.Equal(typeof(TestClassException).FullName, failedMessage.ExceptionTypes.Single());
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
		public async ValueTask TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithExtraCtorArg));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestClassException).FullName, msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 arg1, String arg2", msg.Messages.Single());
		}

		class ClassWithExtraCtorArg : IClassFixture<EmptyFixtureData>
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithExtraCtorArg(int arg1, EmptyFixtureData fixture, string arg2) { }
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
			public ClassWithMissingCtorArg(EmptyFixtureData fixture) { }

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
				exceptionTypeName => Assert.Equal(typeof(TestClassException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
			);
			Assert.Equal("Class fixture type 'FixtureAcceptanceTests+ThrowingCtorFixture' threw in its constructor", msg.Messages.First());
		}

		class ClassWithThrowingFixtureCtor : IClassFixture<ThrowingCtorFixture>
		{
			public ClassWithThrowingFixtureCtor(ThrowingCtorFixture _) { }

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
				exceptionTypeName => Assert.Equal(typeof(TestFixtureCleanupException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
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
				exceptionTypeName => Assert.Equal(typeof(TestClassException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
			);
			Assert.Equal("Class fixture type 'FixtureAcceptanceTests+ThrowingInitializeAsyncFixture' threw in InitializeAsync", msg.Messages.First());
		}

		class ClassWithThrowingFixtureInitializeAsync : IClassFixture<ThrowingInitializeAsyncFixture>
		{
			public ClassWithThrowingFixtureInitializeAsync(ThrowingInitializeAsyncFixture ignored) { }

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
				exceptionTypeName => Assert.Equal(typeof(TestFixtureCleanupException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
			);
			Assert.Equal("Class fixture type 'FixtureAcceptanceTests+ThrowingDisposeAsyncFixture' threw in DisposeAsync", msg.Messages.First());
		}

		class ClassWithThrowingFixtureDisposeAsync : IClassFixture<ThrowingDisposeAsyncFixture>
		{
			public ClassWithThrowingFixtureDisposeAsync(ThrowingDisposeAsyncFixture ignore) { }

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
			Assert.Equal(typeof(TestClassException).FullName, msg.ExceptionTypes.Single());
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
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithExtraCtorArg(int arg1, EmptyFixtureData fixture, string arg2) { }
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
			public ClassWithMissingCtorArg(EmptyFixtureData fixture) { }

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
				exceptionTypeName => Assert.Equal(typeof(TestClassException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
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
				exceptionTypeName => Assert.Equal(typeof(TestFixtureCleanupException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
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
			await RunAsync<_TestPassed>(new[] { typeof(FixtureSaver1), typeof(FixtureSaver2) });

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
		public async ValueTask TestClassWithThrowingCollectionFixtureSetupAsyncResultsInFailedTest()
		{
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithThrowingFixtureInitializeAsync));

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestClassException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
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
				exceptionTypeName => Assert.Equal(typeof(TestFixtureCleanupException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
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
			var results = await RunAsync<_TestPassed>(new[] { typeof(Fixture1), typeof(Fixture2) });
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

	public class AssemblyFixture : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassWithExtraArgumentToConstructorResultsInFailedTest()
		{
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(EmptyFixtureData));
			var messages = await RunAsync<_TestFailed>(typeof(ClassWithExtraCtorArg), additionalAssemblyAttributes: assemblyAttribute);

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(TestClassException).FullName, msg.ExceptionTypes.Single());
			Assert.Equal("The following constructor parameters did not have matching fixture data: Int32 arg1, String arg2", msg.Messages.Single());
		}

		class ClassWithExtraCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithExtraCtorArg(int arg1, EmptyFixtureData fixture, string arg2) { }
#pragma warning restore xUnit1041

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithMissingArgumentToConstructorIsAcceptable()
		{
			var emptyFixtureAttribute = Mocks.AssemblyFixtureAttribute(typeof(EmptyFixtureData));
			var objectFixtureAttribute = Mocks.AssemblyFixtureAttribute(typeof(object));
			var messages = await RunAsync<_TestPassed>(typeof(ClassWithMissingCtorArg), additionalAssemblyAttributes: new[] { emptyFixtureAttribute, objectFixtureAttribute });

			Assert.Single(messages);
		}

		class ClassWithMissingCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithMissingCtorArg(EmptyFixtureData fixture) { }
#pragma warning restore xUnit1041

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureConstructorResultsInFailedTest()
		{
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(ThrowingCtorFixture));
			var messages = await RunAsync<_TestFailed>(typeof(PlainTestClass), additionalAssemblyAttributes: assemblyAttribute);

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestClassException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
			);
			Assert.Equal("Assembly fixture type 'FixtureAcceptanceTests+ThrowingCtorFixture' threw in its constructor", msg.Messages.First());
		}


		[Fact]
		public async ValueTask TestClassWithThrowingFixtureDisposeResultsInFailedTest()
		{
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(ThrowingDisposeFixture));
			var messages = await RunAsync<_TestAssemblyCleanupFailure>(typeof(PlainTestClass), additionalAssemblyAttributes: assemblyAttribute);

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestFixtureCleanupException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
			);
			Assert.Equal("Assembly fixture type 'FixtureAcceptanceTests+ThrowingDisposeFixture' threw in Dispose", msg.Messages.First());
		}

		class PlainTestClass
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask FixtureDataIsPassedToConstructor()
		{
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(EmptyFixtureData));
			var messages = await RunAsync<_TestPassed>(typeof(FixtureSpy), additionalAssemblyAttributes: assemblyAttribute);

			Assert.Single(messages);
		}

		class FixtureSpy
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public FixtureSpy(EmptyFixtureData data)
#pragma warning restore xUnit1041
			{
				Assert.NotNull(data);
			}

			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask TestClassWithDefaultParameter()
		{
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(EmptyFixtureData));
			var messages = await RunForResultsAsync<TestPassedWithDisplayName>(typeof(ClassWithDefaultCtorArg), additionalAssemblyAttributes: assemblyAttribute);

			var message = Assert.Single(messages);
			Assert.Equal("FixtureAcceptanceTests+AssemblyFixture+ClassWithDefaultCtorArg.TheTest", message.TestDisplayName);
		}

		class ClassWithDefaultCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithDefaultCtorArg(EmptyFixtureData fixture, int x = 0)
#pragma warning restore xUnit1041
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
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(EmptyFixtureData));
			var messages = await RunForResultsAsync<TestPassedWithDisplayName>(typeof(ClassWithOptionalCtorArg), additionalAssemblyAttributes: assemblyAttribute);

			var message = Assert.Single(messages);
			Assert.Equal("FixtureAcceptanceTests+AssemblyFixture+ClassWithOptionalCtorArg.TheTest", message.TestDisplayName);
		}

		class ClassWithOptionalCtorArg
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public ClassWithOptionalCtorArg(EmptyFixtureData fixture, [Optional] int x, [Optional] object y)
#pragma warning restore xUnit1041
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
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(EmptyFixtureData));
			var messages = await RunForResultsAsync<TestPassedWithDisplayName>(typeof(ClassWithParamsArg), additionalAssemblyAttributes: assemblyAttribute);

			var message = Assert.Single(messages);
			Assert.Equal("FixtureAcceptanceTests+AssemblyFixture+ClassWithParamsArg.TheTest", message.TestDisplayName);
		}

		class ClassWithParamsArg
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

	public class AsyncAssemblyFixture : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestClassWithThrowingFixtureInitializeAsyncResultsInFailedTest()
		{
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(ThrowingInitializeAsyncFixture));
			var messages = await RunAsync<_TestFailed>(typeof(PlainTestClass), additionalAssemblyAttributes: assemblyAttribute);

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestClassException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
			);
			Assert.Equal("Assembly fixture type 'FixtureAcceptanceTests+ThrowingInitializeAsyncFixture' threw in InitializeAsync", msg.Messages.First());
		}

		[Fact]
		public async ValueTask TestClassWithThrowingFixtureDisposeAsyncResultsInFailedTest()
		{
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(ThrowingDisposeAsyncFixture));
			var messages = await RunAsync<_TestAssemblyCleanupFailure>(typeof(PlainTestClass), additionalAssemblyAttributes: assemblyAttribute);

			var msg = Assert.Single(messages);
			Assert.Collection(
				msg.ExceptionTypes,
				exceptionTypeName => Assert.Equal(typeof(TestFixtureCleanupException).FullName, exceptionTypeName),
				exceptionTypeName => Assert.Equal(typeof(DivideByZeroException).FullName, exceptionTypeName)
			);
			Assert.Equal("Assembly fixture type 'FixtureAcceptanceTests+ThrowingDisposeAsyncFixture' threw in DisposeAsync", msg.Messages.First());
		}

		class PlainTestClass
		{
			[Fact]
			public void TheTest() { }
		}

		[Fact]
		public async ValueTask AssemblyFixtureAsyncSetupShouldOnlyRunOnce()
		{
			var alphaFixture = Mocks.AssemblyFixtureAttribute(typeof(CountedAsyncFixture<Alpha>));
			var betaFixture = Mocks.AssemblyFixtureAttribute(typeof(CountedAsyncFixture<Beta>));
			var results = await RunAsync<_TestPassed>(new[] { typeof(TestClass1), typeof(TestClass2) }, additionalAssemblyAttributes: new[] { alphaFixture, betaFixture });

			Assert.Equal(2, results.Count);
		}

		class Alpha { }
		class Beta { }

		[Collection("Assembly async once")]
		class TestClass1
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public TestClass1(CountedAsyncFixture<Alpha> alpha, CountedAsyncFixture<Beta> beta)
#pragma warning restore xUnit1041
			{
				Assert.Equal(1, alpha.Count);
				Assert.Equal(1, beta.Count);
			}

			[Fact]
			public void TheTest() { }
		}

		[Collection("Assembly async once")]
		class TestClass2
		{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
			public TestClass2(CountedAsyncFixture<Alpha> alpha, CountedAsyncFixture<Beta> beta)
#pragma warning restore xUnit1041
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

	public class FixtureComposition : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask ClassFixtureComposition()
		{
			var assemblyAttribute = Mocks.AssemblyFixtureAttribute(typeof(ComposedAssemblyFixture));
			var messageSink = SpyMessageSink.Capture();
			var messages = await RunAsync(typeof(TestClassWithClassFixtureComposition), diagnosticMessageSink: messageSink, additionalAssemblyAttributes: assemblyAttribute);

			Assert.Single(messages.OfType<_TestPassed>());
		}

		[CollectionDefinition(nameof(TestClassWithClassFixtureCompositionCollection))]
		public class TestClassWithClassFixtureCompositionCollection : ICollectionFixture<ComposedCollectionFixture>
		{ }

		[Collection(nameof(TestClassWithClassFixtureCompositionCollection))]
		class TestClassWithClassFixtureComposition : IClassFixture<ComposedClassFixture>
		{
			readonly ComposedAssemblyFixture assemblyFixture;
			readonly ComposedClassFixture classFixture;
			readonly ComposedCollectionFixture collectionFixture;
			readonly ITestContextAccessor testContextAccessor;

			public TestClassWithClassFixtureComposition(
				ComposedClassFixture classFixture,
				ComposedCollectionFixture collectionFixture,
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
				ComposedAssemblyFixture assemblyFixture,
#pragma warning restore xUnit1041
				ITestContextAccessor testContextAccessor)
			{
				this.classFixture = classFixture;
				this.collectionFixture = collectionFixture;
				this.assemblyFixture = assemblyFixture;
				this.testContextAccessor = testContextAccessor;
			}

			[Fact]
			public void TheTest()
			{
				Assert.NotNull(classFixture);
				Assert.NotNull(collectionFixture);
				Assert.NotNull(assemblyFixture);

				Assert.Same(collectionFixture, classFixture.CollectionFixture);
				Assert.Same(assemblyFixture, classFixture.AssemblyFixture);
				Assert.Same(assemblyFixture, collectionFixture.AssemblyFixture);

				var diagnosticMessageSink = classFixture.DiagnosticMessageSink;
				Assert.NotNull(diagnosticMessageSink);
				Assert.Same(diagnosticMessageSink, collectionFixture.DiagnosticMessageSink);
				Assert.Same(diagnosticMessageSink, assemblyFixture.DiagnosticMessageSink);

				Assert.NotNull(testContextAccessor);
				Assert.NotNull(classFixture.TestContextAccessor);
				Assert.NotNull(collectionFixture.TestContextAccessor);
				Assert.NotNull(assemblyFixture.TestContextAccessor);
				var testContext = testContextAccessor.Current;
				Assert.Same(testContext, classFixture.TestContextAccessor.Current);
				Assert.Same(testContext, collectionFixture.TestContextAccessor.Current);
				Assert.Same(testContext, assemblyFixture.TestContextAccessor.Current);
			}
		}

		class ComposedClassFixture
		{
			public ComposedClassFixture(
				ComposedCollectionFixture collectionFixture,
				ComposedAssemblyFixture assemblyFixture,
				_IMessageSink diagnosticMessageSink,
				ITestContextAccessor testContextAccessor)
			{
				CollectionFixture = collectionFixture;
				AssemblyFixture = assemblyFixture;
				DiagnosticMessageSink = diagnosticMessageSink;
				TestContextAccessor = testContextAccessor;
			}

			public ComposedAssemblyFixture AssemblyFixture { get; }
			public ComposedCollectionFixture CollectionFixture { get; }
			public _IMessageSink DiagnosticMessageSink { get; }
			public ITestContextAccessor TestContextAccessor { get; }
		}

		class ComposedCollectionFixture
		{
			public ComposedCollectionFixture(
				ComposedAssemblyFixture assemblyFixture,
				_IMessageSink diagnosticMessageSink,
				ITestContextAccessor testContextAccessor)
			{
				AssemblyFixture = assemblyFixture;
				DiagnosticMessageSink = diagnosticMessageSink;
				TestContextAccessor = testContextAccessor;
			}

			public ComposedAssemblyFixture AssemblyFixture { get; }
			public _IMessageSink DiagnosticMessageSink { get; }
			public ITestContextAccessor TestContextAccessor { get; }
		}

		class ComposedAssemblyFixture
		{
			public ComposedAssemblyFixture(
				_IMessageSink diagnosticMessageSink,
				ITestContextAccessor testContextAccessor)
			{
				DiagnosticMessageSink = diagnosticMessageSink;
				TestContextAccessor = testContextAccessor;
			}

			public _IMessageSink DiagnosticMessageSink { get; }
			public ITestContextAccessor TestContextAccessor { get; }
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

	class CountedFixture
	{
		static int counter = 0;

		public CountedFixture() => Identity = ++counter;

		public readonly int Identity;
	}
}
