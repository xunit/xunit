using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitTheoryTestCaseTests
{
    public class RunAsync
    {
        [Fact]
        public async void EnumeratesDataAtRuntimeAndExecutesOneTestForEachDataRow()
        {
            var testCase = TestableXunitTheoryTestCase.Create(typeof(ClassUnderTest), "TestWithData");
            var bus = new SpyMessageBus<ITestCaseFinished>();

            await testCase.RunAsync(bus);
            bus.Finished.WaitOne();

            var resultMessages = bus.Messages.OfType<ITestResultMessage>();
            Assert.Equal(2, resultMessages.Count());
            var passed = (ITestPassed)Assert.Single(resultMessages, msg => msg is ITestPassed);
            Assert.Equal("XunitTheoryTestCaseTests+RunAsync+ClassUnderTest.TestWithData(x: 42, y: " + 21.12.ToString(CultureInfo.CurrentCulture) + ", z: \"Hello\")", passed.TestDisplayName);
            var failed = (ITestFailed)Assert.Single(resultMessages, msg => msg is ITestFailed);
            Assert.Equal("XunitTheoryTestCaseTests+RunAsync+ClassUnderTest.TestWithData(x: 0, y: 0, z: \"World!\")", failed.TestDisplayName);
        }

        [Fact]
        public async void DiscovererWhichThrowsReturnsASingleFailedTest()
        {
            var testCase = TestableXunitTheoryTestCase.Create(typeof(ClassUnderTest), "TestWithThrowingData");
            var bus = new SpyMessageBus<ITestCaseFinished>();

            await testCase.RunAsync(bus);
            bus.Finished.WaitOne();

            var resultMessages = bus.Messages.OfType<ITestResultMessage>();
            var failed = (ITestFailed)Assert.Single(resultMessages);
            Assert.Equal("XunitTheoryTestCaseTests+RunAsync+ClassUnderTest.TestWithThrowingData", failed.TestDisplayName);
            Assert.Equal("System.DivideByZeroException", failed.ExceptionTypes.Single());
            Assert.Equal("Attempted to divide by zero.", failed.Messages.Single());
            Assert.Contains("XunitTheoryTestCaseTests.RunAsync.ClassUnderTest.get_ThrowingData()", failed.StackTraces.Single());
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
            [MemberData("SomeData")]
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
            [MemberData("ThrowingData")]
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

            public async Task<bool> RunAsync(IMessageBus messageBus)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                await RunAsync(messageBus, new object[0], new ExceptionAggregator(), cancellationTokenSource);
                return cancellationTokenSource.IsCancellationRequested;
            }

            public Task RunTestsAsync()
            {
                return RunTestsAsync(bus, new object[0], new ExceptionAggregator(), new CancellationTokenSource());
            }

            protected override Task RunTestsAsync(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            {
                if (callback == null)
                    return base.RunTestsAsync(messageBus, constructorArguments, aggregator, cancellationTokenSource);

                callback(messageBus);
                return Task.FromResult(0);
            }
        }
    }
}