using System;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class LifetimeCommandTests
    {
        [Fact]
        public void CreatesNewInstanceWhenPassedNull()
        {
            StubCommand innerCommand = new StubCommand();
            MethodInfo method = typeof(StubCommand).GetMethod("Execute");
            LifetimeCommand command = new LifetimeCommand(innerCommand, Reflector.Wrap(method));

            command.Execute(null);

            Assert.NotNull(innerCommand.TestClass);
        }

        [Fact]
        public void ConstructorThrowsTestNotCalledDisposeNotCalled()
        {
            MethodInfo method = typeof(SpyWithConstructorThrow).GetMethod("PassedTest");
            IMethodInfo wrappedMethod = Reflector.Wrap(method);
            TestCommand testCommand = new FactCommand(wrappedMethod);
            LifetimeCommand command = new LifetimeCommand(testCommand, wrappedMethod);
            SpyWithConstructorThrow.Reset();

            Record.Exception(() => command.Execute(null));

            Assert.Equal(1, SpyWithConstructorThrow.ctorCalled);
            Assert.Equal(0, SpyWithConstructorThrow.testCalled);
            Assert.Equal(0, SpyWithConstructorThrow.disposeCalled);
        }

        [Fact]
        public void ConstructorThrowsTargetInvocationExceptionIsUnwrappedAndRethrown()
        {
            MethodInfo method = typeof(SpyWithConstructorThrow).GetMethod("PassedTest");
            IMethodInfo wrappedMethod = Reflector.Wrap(method);
            FactCommand factCommand = new FactCommand(wrappedMethod);
            LifetimeCommand command = new LifetimeCommand(factCommand, wrappedMethod);
            SpyWithConstructorThrow.Reset();

            Exception ex = Record.Exception(() => command.Execute(null));

            Assert.IsType<InvalidOperationException>(ex);
        }

        [Fact]
        public void DoesNotCreateNewInstanceWhenPassedExistingInstance()
        {
            StubCommand innerCommand = new StubCommand();
            MethodInfo method = typeof(StubCommand).GetMethod("Execute");
            LifetimeCommand command = new LifetimeCommand(innerCommand, Reflector.Wrap(method));
            object instance = new object();

            command.Execute(instance);

            Assert.Same(instance, innerCommand.TestClass);
        }

        [Fact]
        public void DisposeThrowsTestCalled()
        {
            MethodInfo method = typeof(SpyWithDisposeThrow).GetMethod("PassedTest");
            IMethodInfo wrappedMethod = Reflector.Wrap(method);
            TestCommand testCommand = new FactCommand(wrappedMethod);
            LifetimeCommand command = new LifetimeCommand(testCommand, wrappedMethod);
            SpyWithDisposeThrow.Reset();

            Record.Exception(() => command.Execute(new SpyWithDisposeThrow()));

            Assert.Equal(1, SpyWithDisposeThrow.ctorCalled);
            Assert.Equal(1, SpyWithDisposeThrow.testCalled);
            Assert.Equal(1, SpyWithDisposeThrow.disposeCalled);
        }

        [Fact]
        public void DuringTestThrowsDisposeCalled()
        {
            MethodInfo method = typeof(SpyWithTestThrow).GetMethod("FailedTest");
            IMethodInfo wrappedMethod = Reflector.Wrap(method);
            TestCommand testCommand = new FactCommand(wrappedMethod);
            LifetimeCommand command = new LifetimeCommand(testCommand, wrappedMethod);
            SpyWithTestThrow.Reset();

            Record.Exception(() => command.Execute(new SpyWithTestThrow()));

            Assert.Equal(1, SpyWithTestThrow.ctorCalled);
            Assert.Equal(1, SpyWithTestThrow.testCalled);
            Assert.Equal(1, SpyWithTestThrow.disposeCalled);
        }

        class StubCommand : ITestCommand
        {
            public object TestClass;

            public string DisplayName
            {
                get { return null; }
            }

            public bool ShouldCreateInstance
            {
                get { return true; }
            }

            public int Timeout
            {
                get { return 0; }
            }

            public MethodResult Execute(object testClass)
            {
                TestClass = testClass;
                return new SkipResult(null, null, null, null, null);
            }

            public XmlNode ToStartXml()
            {
                return null;
            }
        }

        internal class SpyWithConstructorThrow : FixtureSpy
        {
            public static int testCalled;

            public SpyWithConstructorThrow()
            {
                throw new InvalidOperationException("Constructor Failed");
            }

            public void PassedTest()
            {
                testCalled++;
            }

            public static new void Reset()
            {
                FixtureSpy.Reset();

                testCalled = 0;
            }
        }

        internal class SpyWithDisposeThrow : FixtureSpy
        {
            public static int testCalled;

            public override void Dispose()
            {
                base.Dispose();
                throw new InvalidOperationException("Dispose Failed");
            }

            public static new void Reset()
            {
                FixtureSpy.Reset();

                testCalled = 0;
            }

            public void PassedTest()
            {
                testCalled++;
            }
        }

        internal class SpyWithTestThrow : FixtureSpy
        {
            public static int testCalled;

            public void FailedTest()
            {
                testCalled++;
                throw new InvalidOperationException("Dispose Failed");
            }

            public static new void Reset()
            {
                FixtureSpy.Reset();

                testCalled = 0;
            }
        }
    }
}
