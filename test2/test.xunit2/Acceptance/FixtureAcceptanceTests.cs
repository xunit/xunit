using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class FixtureAcceptanceTests
{
    public class ClassFixture : AcceptanceTest
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
                    Assert.Equal(typeof(TestClassException).FullName, failedMessage.ExceptionType);
                    Assert.Equal("A test class may only define a single public constructor.", failedMessage.Message);
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
                    Assert.Equal(typeof(TestClassException).FullName, failedMessage.ExceptionType);
                    Assert.Equal("A test class may only define a single public constructor.", failedMessage.Message);
                },
                message => Assert.IsAssignableFrom<ITestFinished>(message),
                message => Assert.IsAssignableFrom<ITestCaseFinished>(message),
                message => Assert.IsAssignableFrom<ITestMethodFinished>(message),

                message => Assert.IsAssignableFrom<ITestClassFinished>(message),
                message => Assert.IsAssignableFrom<ITestCollectionFinished>(message),
                message => Assert.IsAssignableFrom<ITestAssemblyFinished>(message)
            );
        }

        class ClassWithTooManyConstructors : IClassFixture<EmptyFixtureData>
        {
            public ClassWithTooManyConstructors() { }
            public ClassWithTooManyConstructors(int unused) { }

            [Fact]
            public void TestMethod1() { }

            [Fact]
            public void TestMethod2() { }
        }

        [Fact(Skip = "Need a test very much like this when we introduce collection support for IClassFixture<T>")]
        public void TestClassWithSameFixtureMoreThanOnceResultsInFailedTest()
        {
            //var messages = Run<ITestFailed>(typeof(ClassWithDuplicatedFixtureData));

            //var msg = Assert.Single(messages);
            //Assert.Equal(typeof(TestClassException).FullName, msg.ExceptionType);
            //Assert.Equal("Duplicate class fixtures were found for type FixtureAcceptanceTests+ClassFixture+EmptyFixtureData", msg.Message);
        }

        [Fact]
        public void TestClassWithExtraArgumentToConstructorResultsInFailedTest()
        {
            var messages = Run<ITestFailed>(typeof(ClassWithExtraCtorArg));

            var msg = Assert.Single(messages);
            Assert.Equal(typeof(TestClassException).FullName, msg.ExceptionType);
            Assert.Equal("The following constructor arguments did not have matching fixture data: Int32 arg1, String arg2", msg.Message);
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
            Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionType);
        }

        class ClassWithThrowingFixtureCtor : IClassFixture<ThrowingCtorFixture>
        {
            [Fact]
            public void TheTest() { }
        }

        class ThrowingCtorFixture
        {
            public ThrowingCtorFixture()
            {
                throw new DivideByZeroException();
            }
        }

        [Fact]
        public void TestClassWithThrowingFixtureDisposeResultsInFailedTest()
        {
            var messages = Run<IErrorMessage>(typeof(ClassWithThrowingFixtureDispose));

            var msg = Assert.Single(messages);
            Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionType);
        }

        class ClassWithThrowingFixtureDispose : IClassFixture<ThrowingDisposeFixture>
        {
            [Fact]
            public void TheTest() { }
        }

        class ThrowingDisposeFixture : IDisposable
        {
            public void Dispose()
            {
                throw new DivideByZeroException();
            }
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

        // What about class fixtures declared on the test collection? (TBD)

        class EmptyFixtureData { }
    }
}