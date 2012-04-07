using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Moq;
using Xunit;

public class TestClassTests
{
    public class Constructor
    {
        [Fact]
        public void NullTestMethodListThrows()
        {
            Exception ex = Record.Exception(() => new TestClass("typeName", null));

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void SetsTestClassOnTestMethods()
        {
            TestMethod testMethod = new TestMethod(null, null, null);

            var testClass = new TestClass("typeName", new[] { testMethod });

            Assert.Same(testClass, testMethod.TestClass);
        }
    }

    public class EnumerateTestMethods
    {
        [Fact]
        public void Unfiltered()
        {
            TestMethod[] tests = new[]
            {
                new TestMethod("method1", null, null),
                new TestMethod("method2", null, null),
                new TestMethod("method3", null, null)
            };
            TestClass testClass = new TestClass("foo", tests);

            var results = testClass.EnumerateTestMethods();

            Assert.Contains(tests[0], results);
            Assert.Contains(tests[1], results);
            Assert.Contains(tests[2], results);
        }

        [Fact]
        public void NullFilterThrows()
        {
            TestClass testClass = new TestClass("foo", new TestMethod[0]);

            Exception ex = Record.Exception(() => testClass.EnumerateTestMethods(null).ToList());

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void FilterWithTruePredicate()
        {
            TestMethod[] tests = new[]
            {
                new TestMethod("method1", null, null),
                new TestMethod("method2", null, null),
                new TestMethod("method3", null, null)
            };
            TestClass testClass = new TestClass("foo", tests);

            var results = testClass.EnumerateTestMethods(testMethod => true);

            Assert.Contains(tests[0], results);
            Assert.Contains(tests[1], results);
            Assert.Contains(tests[2], results);
        }

        [Fact]
        public void FilterWithFalsePredicate()
        {
            TestMethod[] tests = new[]
            {
                new TestMethod("method1", null, null),
                new TestMethod("method2", null, null),
                new TestMethod("method3", null, null)
            };
            TestClass testClass = new TestClass("foo", tests);

            var results = testClass.EnumerateTestMethods(testMethod => false);

            Assert.Empty(results);
        }
    }

    public class Run
    {
        [Fact]
        public void NullTestMethodsThrows()
        {
            var callback = new Mock<ITestMethodRunnerCallback>();
            var testClass = new TestClass("typeName", new TestMethod[0]);

            Assert.Throws<ArgumentNullException>(() => testClass.Run(null, callback.Object));
        }

        [Fact]
        public void EmptyTestMethodsThrows()
        {
            var callback = new Mock<ITestMethodRunnerCallback>();
            var testClass = new TestClass("typeName", new TestMethod[0]);

            Assert.Throws<ArgumentException>(() => testClass.Run(new TestMethod[0], callback.Object));
        }

        [Fact]
        public void NullCallbackThrows()
        {
            var testMethod = new TestMethod(null, null, null);
            var testClass = new TestClass("typeName", new[] { testMethod });

            Assert.Throws<ArgumentNullException>(() => testClass.Run(new[] { testMethod }, null));
        }

        [Fact]
        public void TestMethodNotForThisTestClassThrows()
        {
            var callback = new Mock<ITestMethodRunnerCallback>();
            var testMethod = new TestMethod(null, null, null);
            var testClass = new TestClass("typeName", new TestMethod[0]);

            Assert.Throws<ArgumentException>(() => testClass.Run(new[] { testMethod }, callback.Object));
        }

        [Fact]
        public void CallsTestRunnerWithTestList()
        {
            var wrapper = new Mock<IExecutorWrapper>();
            var callback = new Mock<ITestMethodRunnerCallback>();
            var testMethod1 = new TestMethod("testMethod1", null, null);
            var testMethod2 = new TestMethod("testMethod2", null, null);
            var testMethod3 = new TestMethod("testMethod3", null, null);
            var testClass = new TestableTestClass("typeName", new[] { testMethod1, testMethod2, testMethod3 });
            var testAssembly = new TestAssembly(wrapper.Object, new[] { testClass });

            var result = testClass.Run(new[] { testMethod1, testMethod2, testMethod3 }, callback.Object);

            Assert.Single(testClass.RunTests_Methods, "testMethod1");
            Assert.Single(testClass.RunTests_Methods, "testMethod2");
            Assert.Single(testClass.RunTests_Methods, "testMethod3");
            Assert.Same(callback.Object, testClass.RunTests_Callback);
            Assert.Equal(testClass.RunTests_ReturnValue, result);
        }

        [Fact]
        public void EmptiesResultListBeforeRunning()
        {
            var wrapper = new Mock<IExecutorWrapper>();
            var callback = new Mock<ITestMethodRunnerCallback>();
            var testMethod = new TestMethod("testMethod", null, null);
            var testClass = new TestableTestClass("typeName", new[] { testMethod });
            var testAssembly = new TestAssembly(wrapper.Object, new[] { testClass });
            testMethod.RunResults.Add(new TestPassedResult(1.23, "displayName", null));

            testClass.Run(new[] { testMethod }, callback.Object);

            Assert.Empty(testMethod.RunResults);
        }

        [Fact]
        public void CallsExecutorWrapperToRunTests()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<foo/>");
            var xmlNode = xmlDocument.ChildNodes[0];
            var wrapper = new Mock<IExecutorWrapper>(MockBehavior.Strict);
            var callback = new Mock<ITestMethodRunnerCallback>();
            var testMethod = new TestMethod("testMethod", null, null);
            var testClass = new TestClass("typeName", new[] { testMethod });
            var testAssembly = new TestAssembly(wrapper.Object, new[] { testClass });
            wrapper.Setup(w => w.RunTests(testClass.TypeName, new List<string> { "testMethod" }, It.IsAny<Predicate<XmlNode>>()))
                   .Returns(xmlNode);

            var result = testClass.Run(new[] { testMethod }, callback.Object);

            Assert.Equal("<foo />", result);
        }

        [Fact]
        public void CallsLoggerWhenExecutorWrapperThrows()
        {
            var ex = new Exception();
            var wrapper = new Mock<IExecutorWrapper>();
            var callback = new Mock<ITestMethodRunnerCallback>();
            var testMethod = new TestMethod("testMethod", null, null);
            var testClass = new TestClass("typeName", new[] { testMethod });
            var testAssembly = new TestAssembly(wrapper.Object, new[] { testClass });
            wrapper.Setup(w => w.RunTests(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<Predicate<XmlNode>>()))
                   .Throws(ex);

            var result = testClass.Run(new[] { testMethod }, callback.Object);

            Assert.Equal(String.Empty, result);
            callback.Verify(c => c.ExceptionThrown(testAssembly, ex));
        }
    }

    class TestableTestClass : TestClass
    {
        public List<string> RunTests_Methods;
        public ITestMethodRunnerCallback RunTests_Callback;
        public string RunTests_ReturnValue = "ReturnValue";

        public TestableTestClass(string typeName, IEnumerable<TestMethod> testMethods)
            : base(typeName, testMethods) { }

        protected override string RunTests(List<string> methods, ITestMethodRunnerCallback callback)
        {
            RunTests_Methods = methods;
            RunTests_Callback = callback;

            return RunTests_ReturnValue;
        }
    }
}