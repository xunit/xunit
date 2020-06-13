using System;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class BeforeAfterCommandTests
    {
        [Fact]
        public void VerifyBeforeAfterTestAttributeCalledOnce()
        {
            MethodInfo method = typeof(SimpleTestFixtureSpy).GetMethod("PassedTest");
            BeforeAfterCommand command = new BeforeAfterCommand(new FactCommand(Reflector.Wrap(method)), method);
            SimpleTestFixtureSpy.Reset();

            ITestResult result = command.Execute(new SimpleTestFixtureSpy());

            Assert.Equal(1, BeforeAfterSpyAttribute.beforeTestCount);
            Assert.Equal(1, BeforeAfterSpyAttribute.afterTestCount);
            Assert.Equal("ctor beforetest test aftertest ", SimpleTestFixtureSpy.callOrder);
            Assert.IsType<PassedResult>(result);
        }

        [Fact]
        public void MethodUnderTestProvidedToBeforeAfter()
        {
            MethodInfo methodInfo = typeof(InstrumentedTestClass).GetMethod("PassedTest");
            StubTestCommand stub = new StubTestCommand();
            BeforeAfterCommand command = new BeforeAfterCommand(stub, methodInfo);
            InstrumentedTestClass.Reset();

            command.Execute(new InstrumentedTestClass());

            Assert.Same(methodInfo, BeforeAfterSpyAttribute.beforeMethod);
            Assert.Same(methodInfo, BeforeAfterSpyAttribute.afterMethod);
        }

        [Fact]
        public void BeforeTestThrows()
        {
            MethodInfo methodInfo = typeof(InstrumentedTestClass).GetMethod("PassedTest");
            StubTestCommand stub = new StubTestCommand();
            BeforeAfterCommand command = new BeforeAfterCommand(stub, methodInfo);
            InstrumentedTestClass.Reset();
            BeforeAfterSpyAttribute.beforeTestThrowCount = 1;

            Assert.Throws<Exception>(() => command.Execute(new InstrumentedTestClass()));

            Assert.Equal(1, BeforeAfterSpyAttribute.beforeTestCount);
            Assert.Equal(0, stub.ExecuteCount);
            Assert.Equal(0, BeforeAfterSpyAttribute.afterTestCount);
        }

        [Fact]
        public void AfterTestThrows()
        {
            MethodInfo methodInfo = typeof(InstrumentedTestClass).GetMethod("PassedTest");
            StubTestCommand stub = new StubTestCommand();
            BeforeAfterCommand command = new BeforeAfterCommand(stub, methodInfo);
            InstrumentedTestClass.Reset();
            BeforeAfterSpyAttribute.afterTestThrowCount = 1;

            Assert.Throws<AfterTestException>(() => command.Execute(new InstrumentedTestClass()));

            Assert.Equal(1, BeforeAfterSpyAttribute.beforeTestCount);
            Assert.Equal(1, stub.ExecuteCount);
            Assert.Equal(1, BeforeAfterSpyAttribute.afterTestCount);
        }

        [Fact]
        public void MultipleBeforeAfterTestAttributesAllCalled()
        {
            MethodInfo methodInfo = typeof(BeforeAfterDoubleSpy).GetMethod("PassedTest");
            StubTestCommand stub = new StubTestCommand();
            BeforeAfterCommand command = new BeforeAfterCommand(stub, methodInfo);
            BeforeAfterDoubleSpy.Reset();

            command.Execute(new BeforeAfterDoubleSpy());

            Assert.Equal(2, BeforeAfterSpyAttribute.beforeTestCount);
            Assert.Equal(1, stub.ExecuteCount);
            Assert.Equal(2, BeforeAfterSpyAttribute.afterTestCount);
        }

        [Fact]
        public void MultipleBeforeTestsSecondThrows()
        {
            MethodInfo methodInfo = typeof(MultipleAttributeSpy).GetMethod("PassedTest");
            StubTestCommand stub = new StubTestCommand();
            BeforeAfterCommand command = new BeforeAfterCommand(stub, methodInfo);
            BeforeAfterSpyAttribute.Reset();
            BeforeAfterSpyAttribute.beforeTestThrowCount = 2;

            Assert.Throws<Exception>(() => command.Execute(new MultipleAttributeSpy()));

            Assert.Equal(2, BeforeAfterSpyAttribute.beforeTestCount);
            Assert.Equal(0, stub.ExecuteCount);
            Assert.Equal(1, BeforeAfterSpyAttribute.afterTestCount);
        }

        [Fact]
        public void MultipleAfterTestsSecondThrows()
        {
            MethodInfo methodInfo = typeof(MultipleAttributeSpy).GetMethod("PassedTest");
            StubTestCommand stub = new StubTestCommand();
            BeforeAfterCommand command = new BeforeAfterCommand(stub, methodInfo);
            BeforeAfterSpyAttribute.Reset();
            BeforeAfterSpyAttribute.afterTestThrowCount = 2;

            AfterTestException ex = Assert.Throws<AfterTestException>(() => command.Execute(new MultipleAttributeSpy()));

            Assert.Equal(3, BeforeAfterSpyAttribute.beforeTestCount);
            Assert.Equal(1, stub.ExecuteCount);
            Assert.Equal(3, BeforeAfterSpyAttribute.afterTestCount);
            Assert.Equal(2, ex.AfterExceptions.Count);
        }

        [Fact]
        public void BeforeThrowsAfterThrowsShouldResultInBeforeException()
        {
            MethodInfo methodInfo = typeof(MultipleAttributeSpy).GetMethod("PassedTest");
            StubTestCommand stub = new StubTestCommand();
            BeforeAfterCommand command = new BeforeAfterCommand(stub, methodInfo);
            BeforeAfterSpyAttribute.Reset();
            BeforeAfterSpyAttribute.beforeTestThrowCount = 2;
            BeforeAfterSpyAttribute.afterTestThrowCount = 1;

            Assert.Throws<Exception>(() => command.Execute(new MultipleAttributeSpy()));
        }

        [Fact]
        public void TestThrowsAfterThrowsShouldResultInTestException()
        {
            MethodInfo methodInfo = typeof(MultipleAttributeSpy).GetMethod("PassedTest");
            StubTestCommand stub = new StubTestCommand { ThrowsOnExecute = true };
            BeforeAfterCommand command = new BeforeAfterCommand(stub, methodInfo);
            BeforeAfterSpyAttribute.Reset();
            BeforeAfterSpyAttribute.afterTestThrowCount = 1;

            Assert.Throws<Exception>(() => command.Execute(new InstrumentedTestClass()));
        }

        class StubTestCommand : ITestCommand
        {
            public int ExecuteCount;
            public bool ThrowsOnExecute = false;

            public string DisplayName
            {
                get { return ""; }
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
                ++ExecuteCount;

                if (ThrowsOnExecute)
                    throw new Exception();

                return new PassedResult(null, null, null, null);
            }

            public XmlNode ToStartXml()
            {
                return null;
            }
        }

        internal class BeforeAfterDoubleSpy : IDisposable
        {
            public static int ctorCounter;
            public static int disposeCounter;
            public static int testCounter;

            public BeforeAfterDoubleSpy()
            {
                ctorCounter++;
            }

            public void Dispose()
            {
                disposeCounter++;
            }

            [BeforeAfterSpy, BeforeAfterSpy]
            public void PassedTest()
            {
                testCounter++;
            }

            public static void Reset()
            {
                ctorCounter = 0;
                testCounter = 0;
                disposeCounter = 0;

                BeforeAfterSpyAttribute.Reset();
            }
        }

        internal class BeforeAfterSpyAttribute : BeforeAfterTestAttribute
        {
            public static MethodInfo afterMethod;
            public static int afterTestCount;
            public static int afterTestThrowCount;
            public static MethodInfo beforeMethod;
            public static int beforeTestCount;
            public static int beforeTestThrowCount;

            public override void After(MethodInfo methodUnderTest)
            {
                afterTestCount++;
                afterMethod = methodUnderTest;
                FixtureSpy.callOrder += "aftertest ";

                if (afterTestCount >= afterTestThrowCount)
                    throw new Exception();
            }

            public override void Before(MethodInfo methodUnderTest)
            {
                beforeTestCount++;
                beforeMethod = methodUnderTest;
                FixtureSpy.callOrder += "beforetest ";

                if (beforeTestCount >= beforeTestThrowCount)
                    throw new Exception();
            }

            public static void Reset()
            {
                afterTestCount = 0;
                beforeTestCount = 0;

                afterTestThrowCount = int.MaxValue;
                beforeTestThrowCount = int.MaxValue;
            }
        }

        internal class InstrumentedTestClass : IDisposable
        {
            public static int ctorCounter;
            public static int disposeCounter;

            public InstrumentedTestClass()
            {
                ctorCounter++;
            }

            public void Dispose()
            {
                disposeCounter++;
            }

            [BeforeAfterSpy]
            public void PassedTest() { }

            public static void Reset()
            {
                ctorCounter = 0;
                disposeCounter = 0;

                BeforeAfterSpyAttribute.Reset();
            }
        }

        internal class MultipleAttributeSpy
        {
            [BeforeAfterSpy, BeforeAfterSpy, BeforeAfterSpy]
            public void PassedTest() { }
        }

        internal class SimpleTestFixtureSpy : FixtureSpy
        {
            [BeforeAfterSpy]
            public void PassedTest()
            {
                callOrder += "test ";
            }

            public new static void Reset()
            {
                FixtureSpy.Reset();
                BeforeAfterSpyAttribute.Reset();
            }
        }
    }
}
