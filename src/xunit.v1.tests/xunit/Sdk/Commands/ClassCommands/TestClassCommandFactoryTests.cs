using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TestClassCommandFactoryTests
    {
        [Fact]
        public void AbstractTestClassReturnsNull()
        {
            ITestClassCommand command = TestClassCommandFactory.Make(typeof(AbstractTestClass));

            Assert.Null(command);
        }

        [Fact]
        public void NoTestMethodsShouldReturnNull()
        {
            Type type = typeof(StubClass);
            ITestClassCommand command = TestClassCommandFactory.Make(type);

            Assert.Null(command);
        }

        [Fact]
        public void RunWithClassReturnsTypeToRunWith()
        {
            ITestClassCommand command = TestClassCommandFactory.Make(typeof(MyRunWithTestClass));

            Assert.IsType<MyRunWith>(command);
            Assert.Equal(typeof(MyRunWithTestClass), command.TypeUnderTest.Type);
        }

        [Fact]
        public void RunWithForInvalidTestClassCommandReturnsNull()
        {
            ITestClassCommand command = TestClassCommandFactory.Make(typeof(MyInvalidRunWithTestClass));

            Assert.Null(command);
        }

        [Fact]
        public void StubTestClassMakesTestClassCommand()
        {
            Type testClassType = typeof(StubTestClass);
            ITestClassCommand command = TestClassCommandFactory.Make(testClassType);

            Assert.IsType<TestClassCommand>(command);
            Assert.Equal(typeof(StubTestClass), command.TypeUnderTest.Type);
        }

        [Fact]
        public void AllStagesOfTestLifetimeExistOnSameThread()
        {
            Type testClassType = typeof(ThreadLifetimeSpy);
            ITestClassCommand command = TestClassCommandFactory.Make(testClassType);
            ThreadFixtureSpy.Reset();
            ThreadLifetimeSpy.Reset();

            TestClassCommandRunner.Execute(command, null, null, null);

            // The fixture data may take place on a different thread from the test, but that's
            // an acceptable limitation, as the fixture should have no knowledge of the test
            // class that it's attached to. This means that fixtures cannot use thread local
            // storage, but there's no reason for them to need that anyway, as their own data
            // remains the same throughout all the tests for a given class.

            Assert.NotEqual(-1, ThreadFixtureSpy.CtorThreadId);
            Assert.Equal(ThreadFixtureSpy.CtorThreadId, ThreadFixtureSpy.DisposeThreadId);

            Assert.NotEqual(-1, ThreadLifetimeSpy.CtorThreadId);
            Assert.Equal(ThreadLifetimeSpy.CtorThreadId, ThreadLifetimeSpy.DisposeThreadId);
            Assert.Equal(ThreadLifetimeSpy.CtorThreadId, ThreadLifetimeSpy.TestThreadId);
        }

        internal class ThreadFixtureSpy : IDisposable
        {
            public static int CtorThreadId = -1;
            public static int DisposeThreadId = -1;

            public ThreadFixtureSpy()
            {
                CtorThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            public void Dispose()
            {
                DisposeThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            public static void Reset()
            {
                CtorThreadId = -1;
                DisposeThreadId = -1;
            }
        }

        internal class ThreadLifetimeSpy : IUseFixture<ThreadFixtureSpy>, IDisposable
        {
            public static int CtorThreadId = -1;
            public static int DisposeThreadId = -1;
            public static int TestThreadId = -1;

            public ThreadLifetimeSpy()
            {
                CtorThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            [Fact]
            public void TestMethod()
            {
                TestThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            public void Dispose()
            {
                DisposeThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            public static void Reset()
            {
                CtorThreadId = -1;
                DisposeThreadId = -1;
                TestThreadId = -1;
            }

            public void SetFixture(ThreadFixtureSpy data) { }
        }

        internal abstract class AbstractTestClass
        {
            [Fact]
            public void TestMethod() { }
        }

        internal class MyInvalidRunWith { }

        [RunWith(typeof(MyInvalidRunWith))]
        internal class MyInvalidRunWithTestClass { }

        internal class MyRunWith : ITestClassCommand
        {
            ITypeInfo typeUnderTest;

            public object ObjectUnderTest
            {
                get { return null; }
            }

            public ITypeInfo TypeUnderTest
            {
                get { return typeUnderTest; }
                set { typeUnderTest = value; }
            }

            public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
            {
                throw new NotImplementedException();
            }

            public Exception ClassFinish()
            {
                throw new NotImplementedException();
            }

            public Exception ClassStart()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IMethodInfo> EnumerateTestMethods()
            {
                throw new NotImplementedException();
            }

            public bool IsTestMethod(IMethodInfo testMethod)
            {
                throw new NotImplementedException();
            }
        }

        [RunWith(typeof(MyRunWith))]
        internal class MyRunWithTestClass { }

        internal class StubClass
        {
            public void NonTestMethod() { }
        }

        internal class StubTestClass
        {
            public void NonTestMethod() { }

            [Fact]
            public void TestMethod() { }
        }
    }
}
