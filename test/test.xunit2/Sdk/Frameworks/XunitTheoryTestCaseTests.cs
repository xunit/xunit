using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
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
            var bus = new SpyMessageBus<ITestCaseFinished>();

            testCase.Run(bus);
            bus.Finished.WaitOne();

            var resultMessages = bus.Messages.OfType<ITestResultMessage>();
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
            var bus = new SpyMessageBus<ITestCaseFinished>();

            testCase.Run(bus);
            bus.Finished.WaitOne();

            var resultMessages = bus.Messages.OfType<ITestResultMessage>();
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

        class SpyMessage : IMessageSinkMessage
        {
            public void Dispose() { }
        }

        public class TestableXunitTheoryTestCase : XunitTheoryTestCase
        {
            Action<IMessageBus> callback;
            SpyMessageBus<IMessageSinkMessage> bus = new SpyMessageBus<IMessageSinkMessage>();

            TestableXunitTheoryTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, Action<IMessageBus> callback = null)
                : base(new XunitTestCollection(), assembly, type, method, factAttribute)
            {
                this.callback = callback;
            }

            public List<IMessageSinkMessage> Messages
            {
                get { return bus.Messages; }
            }

            public static TestableXunitTheoryTestCase Create(Action<IMessageBus> callback = null)
            {
                var fact = Mocks.FactAttribute();
                var method = Mocks.MethodInfo();
                var type = Mocks.TypeInfo(methods: new[] { method });
                var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

                return new TestableXunitTheoryTestCase(assmInfo, type, method, fact, callback ?? (sink => sink.QueueMessage(new SpyMessage())));
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

            public bool Run(IMessageBus messageBus)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                Run(messageBus, new object[0], new ExceptionAggregator(), cancellationTokenSource);
                return cancellationTokenSource.IsCancellationRequested;
            }

            public void RunTests()
            {
                RunTests(bus, new object[0], new ExceptionAggregator(), new CancellationTokenSource());
            }

            protected override void RunTests(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            {
                if (callback == null)
                    base.RunTests(messageBus, constructorArguments, aggregator, cancellationTokenSource);
                else
                    callback(messageBus);
            }
        }
    }
}