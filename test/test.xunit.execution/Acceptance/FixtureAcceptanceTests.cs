using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class FixtureAcceptanceTests
{
    public class Constructors : AcceptanceTestV2
    {
        [Fact]
        public void TestClassMustHaveSinglePublicConstructor()
        {
            var messages = Run(typeof(ClassWithTooManyConstructors));

            Assert.Collection(messages,
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
                    Assert.Equal(typeof(TestClassException).FullName, failedMessage.ExceptionTypes.Single());
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
                    Assert.Equal(typeof(TestClassException).FullName, failedMessage.ExceptionTypes.Single());
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
            public ClassWithTooManyConstructors(int unused) { }

            [Fact]
            public void TestMethod1() { }

            [Fact]
            public void TestMethod2() { }
        }
    }

    public class ClassFixture : AcceptanceTestV2
    {
        [Fact]
        public void TestClassWithExtraArgumentToConstructorResultsInFailedTest()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithExtraCtorArg));

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
        public void TestClassWithMissingArgumentToConstructorIsAcceptable()
        {
            var messages = Run<ITestPassed>(typeof(ClassWithMissingCtorArg));

            var msg = Assert.Single(messages);
        }

        class ClassWithMissingCtorArg : IClassFixture<EmptyFixtureData>, IClassFixture<object>
        {
            public ClassWithMissingCtorArg(EmptyFixtureData fixture) { }

            [Fact]
            public void TheTest() { }
        }

        [Fact]
        public void TestClassWithThrowingFixtureConstructorResultsInFailedTest()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithThrowingFixtureCtor));

            var msg = Assert.Single(messages);
            Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
        }

        class ClassWithThrowingFixtureCtor : IClassFixture<ThrowingCtorFixture>
        {
            [Fact]
            public void TheTest() { }
        }

        [Fact]
        public void TestClassWithThrowingFixtureDisposeResultsInFailedTest()
        {
            var messages = Run<ITestClassCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

            var msg = Assert.Single(messages);
            Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
        }

        class ClassWithThrowingFixtureDispose : IClassFixture<ThrowingDisposeFixture>
        {
            [Fact]
            public void TheTest() { }
        }

        [Fact]
        public void FixtureDataIsPassedToConstructor()
        {
            var messages = Run<ITestPassed>(typeof(FixtureSpy));

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
    }

    public class AsyncClassFixture : AcceptanceTestV2
    {
        [Fact]
        public void FixtureDataShouldHaveBeenSetup()
        {
            var messages = Run<ITestPassed>(typeof(FixtureSpy));

            Assert.Single(messages);
        }
        
        class FixtureSpy : IClassFixture<ThrowIfNotCompleted>
        {
            public FixtureSpy(ThrowIfNotCompleted data)
            {
                Assert.True(data.SetupComplete);
            }

            [Fact]
            public void TheTest() { }
        }
        
        class ThrowIfNotCompleted : IAsyncFixture
        {
            public Task SetupAsync()
            {
                SetupComplete = true;
                return Task.FromResult(0);
            }

            public Task TeardownAsync()
            {
                return Task.FromResult(0);
            }

            public bool SetupComplete = false;
        }

        [Fact]
        public void ThrowingAsyncSetupShouldResultInFailedTest()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithThrowingFixtureSetup));

            var msg = Assert.Single(messages);
            Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
        }

        class ClassWithThrowingFixtureSetup : IClassFixture<ThrowingSetup>
        {
            public ClassWithThrowingFixtureSetup(ThrowingSetup ignored)
            {
                
            }   
            [Fact]
            public void TheTest() { }
        }

        class ThrowingSetup :IAsyncFixture{
            public Task SetupAsync()
            {
                throw new DivideByZeroException();
            }

            public Task TeardownAsync()
            {
                return Task.FromResult(0);
            }
        }

        [Fact]
        public void TestClassWithThrowingFixtureAsyncTeardownResultsInFailedTest()
        {
            var messages = Run<ITestClassCleanupFailure>(typeof(ClassWithThrowingFixtureTeardownAsync));

            var msg = Assert.Single(messages);
            Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
        }

        class ClassWithThrowingFixtureTeardownAsync : IClassFixture<ThrowingTeardownAsync>
        {
            public ClassWithThrowingFixtureTeardownAsync(ThrowingTeardownAsync ignore)
            {
                
            }

            [Fact]
            public void TheTest() { }
        }

        class ThrowingTeardownAsync : IAsyncFixture
        {
            public Task SetupAsync()
            {
                return Task.FromResult(0);
            }

            public Task TeardownAsync()
            {
                throw new DivideByZeroException();
            }
        }
    }

    public class CollectionFixture : AcceptanceTestV2
    {
        [Fact]
        public void TestClassCannotBeDecoratedWithICollectionFixture()
        {
            var messages = Run<ITestFailed>(typeof(TestClassWithCollectionFixture));

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
        public void TestClassWithExtraArgumentToConstructorResultsInFailedTest()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithExtraCtorArg));

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
        public void TestClassWithMissingArgumentToConstructorIsAcceptable()
        {
            var messages = Run<ITestPassed>(typeof(ClassWithMissingCtorArg));

            var msg = Assert.Single(messages);
        }

        [Collection("Collection with empty fixture data")]
        class ClassWithMissingCtorArg
        {
            public ClassWithMissingCtorArg(EmptyFixtureData fixture) { }

            [Fact]
            public void TheTest() { }
        }

        [Fact]
        public void TestClassWithThrowingFixtureConstructorResultsInFailedTest()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithThrowingFixtureCtor));

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
        public void TestClassWithThrowingCollectionFixtureDisposeResultsInFailedTest()
        {
            var messages = Run<ITestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureDispose));

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
        public void FixtureDataIsPassedToConstructor()
        {
            var messages = Run<ITestPassed>(typeof(FixtureSpy));

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
        public void FixtureDataIsSameInstanceAcrossClasses()
        {
            Run<ITestPassed>(new[] { typeof(FixtureSaver1), typeof(FixtureSaver2) });

            Assert.Same(FixtureSaver1.Fixture, FixtureSaver2.Fixture);
        }

        class FixtureSaver1
        {
            public static EmptyFixtureData Fixture;

            public FixtureSaver1(EmptyFixtureData data)
            {
                Fixture = data;
            }

            [Fact]
            public void TheTest() { }
        }

        class FixtureSaver2
        {
            public static EmptyFixtureData Fixture;

            public FixtureSaver2(EmptyFixtureData data)
            {
                Fixture = data;
            }

            [Fact]
            public void TheTest() { }
        }

        [Fact]
        public void ClassFixtureOnCollectionDecorationWorks()
        {
            var messages = Run<ITestPassed>(typeof(FixtureSpy_ClassFixture));

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
        public void ClassFixtureOnTestClassTakesPrecedenceOverClassFixtureOnCollection()
        {
            var messages = Run<ITestPassed>(typeof(ClassWithCountedFixture));

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

    public class AsyncCollectionFixture : AcceptanceTestV2
    {
        //Throwing setup
        [Fact]
        public void TestClassWithThrowingCollectionFixtureSetupAsyncResultsInFailedTest()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithThrowingFixtureSetupAsync));

            var msg = Assert.Single(messages);
            Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
        }

        [CollectionDefinition("Collection with throwing async setup")]
        public class CollectionWithThrowingSetupAsync : ICollectionFixture<ThrowingSetupAsync>
        {
        }

        [Collection("Collection with throwing async setup")]
        class ClassWithThrowingFixtureSetupAsync
        {
            [Fact]
            public void TheTest() { }
        }

        //Throwing Teardown
        [Fact]
        public void TestClassWithThrowingCollectionFixtureTeardownAsyncResultsInFailedTest()
        {
            var messages = Run<ITestCollectionCleanupFailure>(typeof(ClassWithThrowingFixtureAsyncTeardown));

            var msg = Assert.Single(messages);
            Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
        }

        [CollectionDefinition("Collection with throwing async teardown")]
        public class CollectionWithThrowingAsyncTeardown : ICollectionFixture<ThrowingTeardownAsync>
        {
            public CollectionWithThrowingAsyncTeardown()
            {

            }
        }

        [Collection("Collection with throwing async teardown")]
        class ClassWithThrowingFixtureAsyncTeardown
        {
            [Fact]
            public void TheTest() { }
        }

        class ThrowingSetupAsync : IAsyncFixture
        {
            public Task SetupAsync()
            {
                throw new DivideByZeroException();
            }

            public Task TeardownAsync()
            {
                return Task.FromResult(0);
            }
        }

        class ThrowingTeardownAsync : IAsyncFixture
        {
            public Task SetupAsync()
            {
                return Task.FromResult(0);
            }

            public Task TeardownAsync()
            {
                throw new DivideByZeroException();
            }
        }

        [Fact]
        public void CollectionFixtureAsyncSetupShouldOnlyRunOnce()
        {
            var results = Run<ITestPassed> (new[] {typeof (Fixture1), typeof (Fixture2)});
            Assert.Equal(2, results.Count);
        }

        [CollectionDefinition("Async once")]
        public class AsyncOnceCollection: ICollectionFixture<CountedAsyncFixture> { }

        [Collection("Async once")]
        class Fixture1
        {
            public Fixture1(CountedAsyncFixture fixture)
            {
                Assert.Equal(1, fixture.Count);
            }
            [Fact]
            public void TheTest() { }
        }

        [Collection("Async once")]
        class Fixture2
        {
            public Fixture2(CountedAsyncFixture fixture)
            {
                Assert.Equal(1, fixture.Count);
            }
            [Fact]
            public void TheTest() { }
        }

        class CountedAsyncFixture : IAsyncFixture
        {
            public int Count = 0;
            public Task SetupAsync()
            {
                Count += 1;
                return Task.FromResult(0);
            }

            public Task TeardownAsync()
            {
                return Task.FromResult(0);
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