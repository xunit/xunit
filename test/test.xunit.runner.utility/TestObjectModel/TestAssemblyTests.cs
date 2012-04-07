using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Moq;
using Xunit;

public class TestAssemblyTests
{
    public class Constructor
    {
        [Fact]
        public void NullExecutorWrapperThrows()
        {
            Exception ex = Record.Exception(() => new TestAssembly(null, new TestClass[0]));

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void NullTestClassListThrows()
        {
            StubExecutorWrapper wrapper = new StubExecutorWrapper();

            Exception ex = Record.Exception(() => new TestAssembly(wrapper, null));

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void SetsTestAssemblyOnTestClasses()
        {
            StubExecutorWrapper wrapper = new StubExecutorWrapper();
            TestClass testClass = new TestClass("typeName", new TestMethod[0]);

            var assembly = new TestAssembly(wrapper, new[] { testClass });

            Assert.Same(assembly, testClass.TestAssembly);
        }
    }

    public class Dispose
    {
        [Fact]
        public void DisposingAssemblyDisposesExecutorWrapper()
        {
            StubExecutorWrapper wrapper = new StubExecutorWrapper();
            var assembly = new TestAssembly(wrapper, new TestClass[0]);

            assembly.Dispose();

            Assert.True(wrapper.Dispose__Called);
        }
    }

    public class EnumerateTestMethods
    {
        [Fact]
        public void UnfilteredReturnsAllTestsFromAllClasses()
        {
            var wrapper = new StubExecutorWrapper();
            var class1Method1 = new TestMethod("method1", null, null);
            var class1Method2 = new TestMethod("method2", null, null);
            var class2Method3 = new TestMethod("method3", null, null);
            var class1 = new TestClass("foo", new[] { class1Method1, class1Method2 });
            var class2 = new TestClass("bar", new[] { class2Method3 });
            var assembly = new TestAssembly(wrapper, new[] { class1, class2 });

            var tests = assembly.EnumerateTestMethods();

            Assert.Equal(3, tests.Count());
            Assert.Contains(class1Method1, tests);
            Assert.Contains(class1Method2, tests);
            Assert.Contains(class2Method3, tests);
        }

        [Fact]
        public void NullFilterThrows()
        {
            TestAssembly testAssembly = new TestAssembly(new StubExecutorWrapper(), new TestClass[0]);

            Exception ex = Record.Exception(() => testAssembly.EnumerateTestMethods(null).ToList());

            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void FilterWithTruePredicate()
        {
            var wrapper = new StubExecutorWrapper();
            var class1Method1 = new TestMethod("method1", null, null);
            var class1Method2 = new TestMethod("method2", null, null);
            var class2Method3 = new TestMethod("method3", null, null);
            var class1 = new TestClass("foo", new[] { class1Method1, class1Method2 });
            var class2 = new TestClass("bar", new[] { class2Method3 });
            var assembly = new TestAssembly(wrapper, new[] { class1, class2 });

            var tests = assembly.EnumerateTestMethods(testMethod => true);

            Assert.Equal(3, tests.Count());
            Assert.Contains(class1Method1, tests);
            Assert.Contains(class1Method2, tests);
            Assert.Contains(class2Method3, tests);
        }

        [Fact]
        public void FilterWithFalsePredicate()
        {
            var wrapper = new StubExecutorWrapper();
            var class1Method1 = new TestMethod("method1", null, null);
            var class1Method2 = new TestMethod("method2", null, null);
            var class2Method3 = new TestMethod("method3", null, null);
            var class1 = new TestClass("foo", new[] { class1Method1, class1Method2 });
            var class2 = new TestClass("bar", new[] { class2Method3 });
            var assembly = new TestAssembly(wrapper, new[] { class1, class2 });

            var tests = assembly.EnumerateTestMethods(testMethod => false);

            Assert.Equal(0, tests.Count());
        }
    }

    public class Run
    {
        [Fact]
        public void NullTestMethodsThrows()
        {
            var testAssembly = TestableTestAssembly.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();

            Assert.Throws<ArgumentNullException>(() => testAssembly.Run(null, callback.Object));
        }

        [Fact]
        public void EmptyTestMethodsThrows()
        {
            var testAssembly = TestableTestAssembly.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();

            Assert.Throws<ArgumentException>(() => testAssembly.Run(new TestMethod[0], callback.Object));
        }

        [Fact]
        public void NullCallbackThrows()
        {
            var testAssembly = TestableTestAssembly.Create();
            var testMethod = testAssembly.Class1.Object.EnumerateTestMethods().First();

            Assert.Throws<ArgumentNullException>(() => testAssembly.Run(new[] { testMethod }, null));
        }

        [Fact]
        public void TestMethodNotForThisTestAssemblyThrows()
        {
            var testAssembly = TestableTestAssembly.Create();
            var testMethod = new TestMethod(null, null, null);
            var testClass = new TestClass(null, new[] { testMethod });
            var callback = new Mock<ITestMethodRunnerCallback>();

            Assert.Throws<ArgumentException>(() => testAssembly.Run(new[] { testMethod }, callback.Object));
        }

        [Fact]
        public void RunSortsTestsByClassAndCallsRunOnClasses()
        {
            var testAssembly = TestableTestAssembly.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();
            TestMethod class1Method1 = testAssembly.Class1.Object.EnumerateTestMethods().First();
            TestMethod class2Method2 = testAssembly.Class2.Object.EnumerateTestMethods().First();
            testAssembly.Class1.Setup(c => c.Run(new[] { class1Method1 }, It.IsAny<ITestMethodRunnerCallback>()))
                               .Returns("<class1/>")
                               .Verifiable();
            testAssembly.Class2.Setup(c => c.Run(new[] { class2Method2 }, It.IsAny<ITestMethodRunnerCallback>()))
                               .Returns("<class2/>")
                               .Verifiable();

            testAssembly.Run(testAssembly.EnumerateTestMethods(), callback.Object);

            testAssembly.Class1.Verify();
            testAssembly.Class2.Verify();
        }

        [Fact]
        public void RunOnlyCallsRunOnClassesWithMethodsToBeRun()
        {
            var testAssembly = TestableTestAssembly.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();
            TestMethod class1Method1 = testAssembly.Class1.Object.EnumerateTestMethods().First();
            TestMethod class2Method2 = testAssembly.Class2.Object.EnumerateTestMethods().First();
            testAssembly.Class1.Setup(c => c.Run(new[] { class1Method1 }, It.IsAny<ITestMethodRunnerCallback>()))
                               .Returns("<class1/>")
                               .Verifiable();

            testAssembly.Run(new[] { class1Method1 }, callback.Object);

            testAssembly.Class1.Verify();
            testAssembly.Class2.Verify(c => c.Run(It.IsAny<IEnumerable<TestMethod>>(), callback.Object), Times.Never());
        }

        [Fact]
        public void RunReturnsAssemblyXml()
        {
            var testAssembly = TestableTestAssembly.Create();
            var callback = new Mock<ITestMethodRunnerCallback>();
            var class1Method1 = testAssembly.Class1.Object.EnumerateTestMethods().First();
            class1Method1.RunResults.Add(new TestFailedResult(1.2, "DisplayName", "Output", "ExceptionType", "ExceptionMessage", "ExceptionStackTrace"));
            class1Method1.RunResults.Add(new TestSkippedResult("DisplayName", "Reason"));
            var class2Method2 = testAssembly.Class2.Object.EnumerateTestMethods().First();
            class2Method2.RunResults.Add(new TestPassedResult(2.4, "DisplayName", "Output"));
            testAssembly.Class1.Setup(c => c.Run(new[] { class1Method1 }, It.IsAny<ITestMethodRunnerCallback>()))
                               .Returns("<class1/>")
                               .Verifiable();
            testAssembly.Class2.Setup(c => c.Run(new[] { class2Method2 }, It.IsAny<ITestMethodRunnerCallback>()))
                               .Returns("<class2/>")
                               .Verifiable();

            var result = testAssembly.Run(testAssembly.EnumerateTestMethods(), callback.Object);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            XmlNode assemblyNode = doc.ChildNodes[0];
            Assert.Equal("AssemblyFilename", assemblyNode.Attributes["name"].Value);
            Assert.Equal("ConfigFilename", assemblyNode.Attributes["configFile"].Value);
            Assert.Equal("3.600", assemblyNode.Attributes["time"].Value);
            Assert.Equal("3", assemblyNode.Attributes["total"].Value);
            Assert.Equal("1", assemblyNode.Attributes["passed"].Value);
            Assert.Equal("1", assemblyNode.Attributes["failed"].Value);
            Assert.Equal("1", assemblyNode.Attributes["skipped"].Value);
            Assert.Equal("<class1 /><class2 />", assemblyNode.InnerXml);
            Assert.NotEmpty(assemblyNode.Attributes["run-date"].Value);
            Assert.NotEmpty(assemblyNode.Attributes["run-time"].Value);
            Assert.NotEmpty(assemblyNode.Attributes["environment"].Value);
            Assert.Contains("xUnit.net", assemblyNode.Attributes["test-framework"].Value);
        }
    }

    class TestableTestAssembly : TestAssembly
    {
        public Mock<TestClass> Class1 { get; set; }

        public Mock<TestClass> Class2 { get; set; }

        TestableTestAssembly(Mock<IExecutorWrapper> wrapper, Mock<TestClass> class1, Mock<TestClass> class2)
            : base(wrapper.Object, new[] { class1.Object, class2.Object })
        {
            Class1 = class1;
            Class2 = class2;
        }

        public static TestableTestAssembly Create()
        {
            var wrapper = new Mock<IExecutorWrapper>();
            wrapper.Setup(w => w.AssemblyFilename).Returns("AssemblyFilename");
            wrapper.Setup(w => w.ConfigFilename).Returns("ConfigFilename");
            var class1Method1 = new TestMethod("method1", null, null);
            var class2Method2 = new TestMethod("method2", null, null);
            var class1 = new Mock<TestClass>("foo", new[] { class1Method1 });
            var class2 = new Mock<TestClass>("bar", new[] { class2Method2 });
            return new TestableTestAssembly(wrapper, class1, class2);
        }
    }
}