//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using Xunit;
//using Xunit.Abstractions;
//using Xunit.Sdk;

//public class XunitTheoryTestCaseTests
//{
//    [Fact]
//    public void Monkey()
//    {
//        var testCase = TestableXunitTheoryTestCase.Create(typeof(ClassUnderTest), "TheTest");
//        var spy = new SpyMessageSink<ITestCaseFinished>();

//        testCase.Run(spy);
//        spy.Finished.WaitOne();

//        var msgTypes = spy.Messages.Select(msg => msg.GetType().FullName).ToList();

//        var passed = (ITestPassed)Assert.Single(spy.Messages, msg => msg is ITestPassed);
//        Assert.Equal<object>("?", passed.TestDisplayName);
//        var failed = (ITestFailed)Assert.Single(spy.Messages, msg => msg is ITestFailed);
//        Assert.Equal<object>("?", failed.TestDisplayName);
//    }

//    class ClassUnderTest
//    {
//        public static IEnumerable<object[]> SomeData
//        {
//            get
//            {
//                yield return new object[] { 42, 21.12, "Hello" };
//                yield return new object[] { 0, 0.0, "World!" };
//            }
//        }

//        [Theory]
//        [PropertyData("SomeData")]
//        public void TheTest(int x, double y, string z)
//        {
//            Assert.NotEqual(x, 0);
//        }
//    }

//    class SpyMessage : ITestMessage { }

//    public class TestableXunitTheoryTestCase : XunitTheoryTestCase
//    {
//        Action<IMessageSink> callback;
//        SpyMessageSink<ITestMessage> sink = new SpyMessageSink<ITestMessage>();

//        TestableXunitTheoryTestCase(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute, Action<IMessageSink> callback = null)
//            : base(assembly, type, method, factAttribute)
//        {
//            this.callback = callback;
//        }

//        public List<ITestMessage> Messages
//        {
//            get { return sink.Messages; }
//        }

//        public static TestableXunitTheoryTestCase Create(Action<IMessageSink> callback = null)
//        {
//            var fact = Mocks.FactAttribute();
//            var method = Mocks.MethodInfo();
//            var type = Mocks.TypeInfo(methods: new[] { method });
//            var assmInfo = Mocks.AssemblyInfo(types: new[] { type });

//            return new TestableXunitTheoryTestCase(assmInfo, type, method, fact, callback ?? (sink => sink.OnMessage(new SpyMessage())));
//        }

//        public static TestableXunitTheoryTestCase Create(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo factAttribute)
//        {
//            return new TestableXunitTheoryTestCase(assembly, type, method, factAttribute);
//        }

//        public static TestableXunitTheoryTestCase Create(Type typeUnderTest, string methodName)
//        {
//            var methodUnderTest = typeUnderTest.GetMethod(methodName);
//            var assembly = Reflector.Wrap(typeUnderTest.Assembly);
//            var type = Reflector.Wrap(typeUnderTest);
//            var method = Reflector.Wrap(methodUnderTest);
//            var fact = Reflector.Wrap(CustomAttributeData.GetCustomAttributes(methodUnderTest)
//                                                         .Single(cad => cad.AttributeType == typeof(TheoryAttribute)));
//            return new TestableXunitTheoryTestCase(assembly, type, method, fact);
//        }

//        protected override IEnumerable<BeforeAfterTestAttribute> GetBeforeAfterAttributes(Type classUnderTest, MethodInfo methodUnderTest)
//        {
//            // Order by name so they are discovered in a predictable order, for these tests
//            return base.GetBeforeAfterAttributes(classUnderTest, methodUnderTest).OrderBy(a => a.GetType().Name);
//        }

//        public void RunTests()
//        {
//            RunTests(sink);
//        }

//        protected override bool RunTests(IMessageSink messageSink)
//        {
//            if (callback == null)
//                return base.RunTests(messageSink);

//            callback(messageSink);
//            return true;
//        }
//    }
//}