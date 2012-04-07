using System;
using System.Linq;
using Moq;
using Xunit;

public class TestClassCallbackDispatcherTests
{
    [Fact]
    public void AssemblyFinished()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();

        dispatcher.AssemblyFinished("assemblyFilename", 1, 2, 3, 4.5);

        dispatcher.Callback.Verify(c => c.AssemblyFinished(dispatcher.TestClass.TestAssembly, 1, 2, 3, 4.5));
    }

    [Fact]
    public void AssemblyStart()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();

        dispatcher.AssemblyStart("assemblyFilename", "configFilename", "1.2.3.4");

        dispatcher.Callback.Verify(c => c.AssemblyStart(dispatcher.TestClass.TestAssembly));
    }

    [Fact]
    public void ClassFailed()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();
        dispatcher.Callback.Setup(c => c.ClassFailed(dispatcher.TestClass, "exceptionType", "message", "stackTrace"))
                           .Returns(true)
                           .Verifiable();

        bool result = dispatcher.ClassFailed("className", "exceptionType", "message", "stackTrace");

        Assert.True(result);
        dispatcher.Callback.Verify();
    }

    [Fact]
    public void ClassFailed_Cancelled()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();
        dispatcher.Callback.Setup(c => c.ClassFailed(dispatcher.TestClass, "exceptionType", "message", "stackTrace"))
                           .Returns(false);

        bool result = dispatcher.ClassFailed("className", "exceptionType", "message", "stackTrace");

        Assert.False(result);
    }

    [Fact]
    public void ExceptionThrown()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();
        var exception = new Exception();

        dispatcher.ExceptionThrown("assemblyFilename", exception);

        dispatcher.Callback.Verify(c => c.ExceptionThrown(dispatcher.TestClass.TestAssembly, exception));
    }

    [Fact]
    public void TestFailed()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();

        dispatcher.TestFailed("displayName", "typeName", dispatcher.TestMethod.MethodName,
                              1.2, "output", "exceptionType", "message", "stackTrace");

        var result = Assert.IsType<TestFailedResult>(dispatcher.TestMethod.RunResults[0]);
        Assert.Equal(1.2, result.Duration);
        Assert.Equal("displayName", result.DisplayName);
        Assert.Equal("output", result.Output);
        Assert.Equal("exceptionType", result.ExceptionType);
        Assert.Equal("message", result.ExceptionMessage);
        Assert.Equal("stackTrace", result.ExceptionStackTrace);
    }

    [Fact]
    public void TestFinished()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();
        dispatcher.Callback.Setup(c => c.TestFinished(dispatcher.TestMethod))
                           .Returns(true)
                           .Verifiable();

        bool result = dispatcher.TestFinished("displayName", "typeName", dispatcher.TestMethod.MethodName);

        Assert.True(result);
        dispatcher.Callback.Verify();
    }

    [Fact]
    public void TestFinished_Cancelled()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();
        dispatcher.Callback.Setup(c => c.TestFinished(dispatcher.TestMethod))
                           .Returns(false);

        bool result = dispatcher.TestFinished("displayName", "typeName", dispatcher.TestMethod.MethodName);

        Assert.False(result);
    }

    [Fact]
    public void TestPassed()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();

        dispatcher.TestPassed("displayName", "typeName", dispatcher.TestMethod.MethodName, 1.2, "output");

        var result = Assert.IsType<TestPassedResult>(dispatcher.TestMethod.RunResults[0]);
        Assert.Equal(1.2, result.Duration);
        Assert.Equal("displayName", result.DisplayName);
        Assert.Equal("output", result.Output);
    }

    [Fact]
    public void TestSkipped()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();

        dispatcher.TestSkipped("displayName", "typeName", dispatcher.TestMethod.MethodName, "reason");

        var result = Assert.IsType<TestSkippedResult>(dispatcher.TestMethod.RunResults[0]);
        Assert.Equal("displayName", result.DisplayName);
        Assert.Equal("reason", result.Reason);
    }

    [Fact]
    public void TestStart()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();
        dispatcher.Callback.Setup(c => c.TestStart(dispatcher.TestMethod))
                           .Returns(true)
                           .Verifiable();

        bool result = dispatcher.TestStart("displayName", "typeName", dispatcher.TestMethod.MethodName);

        Assert.True(result);
        dispatcher.Callback.Verify();
    }

    [Fact]
    public void TestStart_Cancelled()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();
        dispatcher.Callback.Setup(c => c.TestStart(dispatcher.TestMethod))
                           .Returns(false);

        bool result = dispatcher.TestStart("displayName", "typeName", dispatcher.TestMethod.MethodName);

        Assert.False(result);
    }

    [Fact]
    public void MultipleResultsPerTestMethod()
    {
        var dispatcher = TestableTestClassCallbackDispatcher.Create();

        dispatcher.TestPassed("displayName", "typeName", dispatcher.TestMethod.MethodName, 1.2, "output");
        dispatcher.TestFailed("displayName", "typeName", dispatcher.TestMethod.MethodName,
                              1.2, "output", "exceptionType", "message", "stackTrace");
        dispatcher.TestSkipped("displayName", "typeName", dispatcher.TestMethod.MethodName, "reason");

        Assert.Equal(3, dispatcher.TestMethod.RunResults.Count);
        Assert.IsType<TestPassedResult>(dispatcher.TestMethod.RunResults[0]);
        Assert.IsType<TestFailedResult>(dispatcher.TestMethod.RunResults[1]);
        Assert.IsType<TestSkippedResult>(dispatcher.TestMethod.RunResults[2]);
    }

    class TestableTestClassCallbackDispatcher : TestClassCallbackDispatcher
    {
        public Mock<ITestMethodRunnerCallback> Callback { get; set; }
        public TestClass TestClass { get; set; }
        public TestMethod TestMethod
        {
            get { return TestClass.EnumerateTestMethods().Single(); }
        }

        TestableTestClassCallbackDispatcher(TestClass testClass, Mock<ITestMethodRunnerCallback> callback)
            : base(testClass, callback.Object)
        {
            Callback = callback;
            TestClass = testClass;
        }

        public static TestableTestClassCallbackDispatcher Create()
        {
            var testMethod = new TestMethod("testMethod", null, null);
            var testClass = new TestClass("typeName", new[] { testMethod });
            var testAssembly = new TestAssembly(new Mock<IExecutorWrapper>().Object, new[] { testClass });
            var callback = new Mock<ITestMethodRunnerCallback>();
            return new TestableTestClassCallbackDispatcher(testClass, callback);
        }
    }
}