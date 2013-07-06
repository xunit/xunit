using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitTheoryTestCaseTests
{
    public class Run
    {
        [Fact]
        public void EnumeratesDataAtRuntimeAndExecutesOneTestForEachDataRow()
        {
            var testCase = TestableXunitTheoryTestCase.Create(typeof(ClassUnderTest), "TestWithData");
            var spy = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(spy);
            spy.Finished.WaitOne();

            var resultMessages = spy.Messages.OfType<ITestResultMessage>();
            Assert.Equal(2, resultMessages.Count());
            var passed = (ITestPassed)Assert.Single(resultMessages, msg => msg is ITestPassed);
            Assert.Equal("XunitTheoryTestCaseTests+Run+ClassUnderTest.TestWithData(x: 42, y: 21.12, z: \"Hello\")", passed.TestDisplayName);
            var failed = (ITestFailed)Assert.Single(resultMessages, msg => msg is ITestFailed);
            Assert.Equal("XunitTheoryTestCaseTests+Run+ClassUnderTest.TestWithData(x: 0, y: 0, z: \"World!\")", failed.TestDisplayName);
        }

        [Fact]
        public void DiscovererWhichThrowsReturnsASingleFailedTest()
        {
            var testCase = TestableXunitTheoryTestCase.Create(typeof(ClassUnderTest), "TestWithThrowingData");
            var spy = new SpyMessageSink<ITestCaseFinished>();

            testCase.Run(spy);
            spy.Finished.WaitOne();

            var resultMessages = spy.Messages.OfType<ITestResultMessage>();
            var failed = (ITestFailed)Assert.Single(resultMessages);
            Assert.Equal("XunitTheoryTestCaseTests+Run+ClassUnderTest.TestWithThrowingData", failed.TestDisplayName);
            Assert.Equal("System.DivideByZeroException : Attempted to divide by zero.", failed.Message);
            Assert.Contains("XunitTheoryTestCaseTests.Run.ClassUnderTest.get_ThrowingData()", failed.StackTrace);
        }

        class ClassUnderTest
        {
            public static IEnumerable<object[]> SomeData
            {
                get
                {
                    yield return new object[] { 42, 21.12, "Hello" };
                    yield return new object[] { 0, 0.0, "World!" };
                }
            }

            [Theory]
            [PropertyData("SomeData")]
            public void TestWithData(int x, double y, string z)
            {
                Assert.NotEqual(x, 0);
            }

            public static IEnumerable<object[]> ThrowingData
            {
                get
                {
                    throw new DivideByZeroException();
                }
            }

            [Theory]
            [PropertyData("ThrowingData")]
            public void TestWithThrowingData(int x) { }
        }

        class SpyMessage : ITestMessage { }

        public class TestableXunitTheoryTestCase : XunitTheoryTestCase
        {
            Action<IMessageSink> callback;
            SpyMessageSink<ITestMessage> sink = new SpyMessageSink<ITestMessage>();

            TestableXunitTheoryTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, Action<IMessageSink> callback = null)
                : base(new XunitTestCollection(), assembly, type, method, factAttribute)
            {
                this.callback = callback;
            }

            public List<ITestMessage> Messages
            {
                get { return sink.Messages; }
            }

            public static TestableXunitTheoryTestCase Create(Action<IMessageSink> callback = null)
            {
                var fact = Mocks.FactAttribute();
                var method = Mocks.MethodInfo();
                var type = Mocks.TypeInfo(methods: new[] { method });
                var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

                return new TestableXunitTheoryTestCase(assmInfo, type, method, fact, callback ?? (sink => sink.OnMessage(new SpyMessage())));
            }

            public static TestableXunitTheoryTestCase Create(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute)
            {
                return new TestableXunitTheoryTestCase(assembly, type, method, factAttribute);
            }

            public static TestableXunitTheoryTestCase Create(Type typeUnderTest, string methodName)
            {
                var methodUnderTest = typeUnderTest.GetMethod(methodName);
                var assembly = Reflector.Wrap(typeUnderTest.Assembly);
                var type = Reflector.Wrap(typeUnderTest);
                var method = Reflector.Wrap(methodUnderTest);
                var fact = Reflector.Wrap(CustomAttributeData.GetCustomAttributes(methodUnderTest)
                                                             .Single(cad => cad.AttributeType == typeof(TheoryAttribute)));
                return new TestableXunitTheoryTestCase(assembly, type, method, fact);
            }

            protected override IEnumerable<BeforeAfterTestAttribute> GetBeforeAfterAttributes(Type classUnderTest, MethodInfo methodUnderTest)
            {
                // Order by name so they are discovered in a predictable order, for these tests
                return base.GetBeforeAfterAttributes(classUnderTest, methodUnderTest).OrderBy(a => a.GetType().Name);
            }

            public bool Run(IMessageSink messageSink)
            {
                return Run(messageSink, new object[0], new ExceptionAggregator());
            }

            public void RunTests()
            {
                RunTests(sink, new object[0], new ExceptionAggregator());
            }

            protected override bool RunTests(IMessageSink messageSink, object[] constructorArguments, ExceptionAggregator aggregator)
            {
                if (callback == null)
                    return base.RunTests(messageSink, constructorArguments, aggregator);

                callback(messageSink);
                return true;
            }
        }
    }
}