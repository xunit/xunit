using System;
using System.Collections.Generic;
using System.Xml;
using Moq;
using Xunit;

public class TestRunnerTests
{
    public class RunAssembly
    {
        [Fact]
        public void CallsAssemblyStart()
        {
            var runner = TestableTestRunner.CreateForAssembly();

            runner.RunAssembly();

            runner.Logger.Verify(l => l.AssemblyStart(runner.Executor.AssemblyFilename,
                runner.Executor.ConfigFilename,
                runner.Executor.XunitVersion));
        }

        [Fact]
        public void CallsAssemblyStartWithConfigFilename()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssemblyWithConfigFile(@"C:\biff\baz.config");

            runner.RunAssembly();

            runner.Logger.Verify(l => l.AssemblyStart(@"C:\foo\bar.dll",
                                      @"C:\biff\baz.config",
                                      @"abcd"));
        }

        [Fact]
        public void PassingAssemblyReturnsPassed()
        {
            const string xml = @"
<assembly name='C:\foo\bar.dll' run-date='2008-09-20' run-time='12:34:56' time='0.000' total='1' passed='1' failed='0' skipped='0'>
    <class time='1.234' name='ThisIsTheType' total='1' passed='1' failed='0' skipped='0'>
        <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Pass' time='1.234' />
    </class>
</assembly>";
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly(xml);

            TestRunnerResult result = runner.RunAssembly();

            Assert.Equal(TestRunnerResult.Passed, result);
        }

        [Fact]
        public void FailingAssemblyReturnsFailed()
        {
            const string xml = @"
<assembly name='C:\foo\bar.dll' run-date='2008-09-20' run-time='12:34:56' time='0.000' total='1' passed='0' failed='1' skipped='0'>
    <class time='1.234' name='ThisIsTheType' total='1' passed='0' failed='1' skipped='0'>
        <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Fail' time='1.234'>
            <failure exception-type='Exception.Type'>
                <message><![CDATA[Failure message]]></message>
                <stack-trace><![CDATA[Stack trace]]></stack-trace>
            </failure>
        </test>
    </class>
</assembly>";
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly(xml);

            TestRunnerResult result = runner.RunAssembly();

            Assert.Equal(TestRunnerResult.Failed, result);
        }

        [Fact]
        public void AllSkipAssemblyReturnsPassed()
        {
            const string xml = @"
<assembly name='C:\foo\bar.dll' run-date='2008-09-20' run-time='12:34:56' time='0.000' total='1' passed='0' failed='0' skipped='1'>
    <class time='1.234' name='ThisIsTheType' total='1' passed='0' failed='0' skipped='1'>
        <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Skip'>
            <reason>
                <message><![CDATA[Skip reason]]></message>
            </reason>
        </test>
    </class>
</assembly>";
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly(xml);

            TestRunnerResult result = runner.RunAssembly();

            Assert.Equal(TestRunnerResult.Passed, result);
        }

        [Fact]
        public void EmptyAssemblyNodeReturnsNoTests()
        {
            const string xml = @"<assembly name='C:\foo\bar.dll' run-date='2008-09-20' run-time='12:34:56' time='0.000' total='0' passed='0' failed='0' skipped='0'/>";
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly(xml);

            TestRunnerResult result = runner.RunAssembly();

            Assert.Equal(TestRunnerResult.NoTests, result);
        }

        [Fact]
        public void NullAssemblyReturnsNoTests()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly(null);

            TestRunnerResult result = runner.RunAssembly();

            Assert.Equal(TestRunnerResult.NoTests, result);
        }

        [Fact]
        public void ThrownExceptionSentToLoggerAndFailedIsReturned()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly();
            Exception exception = new InvalidOperationException();
            runner.Executor.RunAssembly__CallbackEvent += delegate { throw exception; };

            TestRunnerResult result = runner.RunAssembly();

            runner.Logger.Verify(l => l.ExceptionThrown(@"C:\foo\bar.dll", exception));
            Assert.Equal(TestRunnerResult.Failed, result);
        }

        [Fact]
        public void CallsTransformerWhenAssemblyIsFinished()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly();
            StubTransformer transformer = new StubTransformer();

            runner.RunAssembly(new IResultXmlTransform[] { transformer });

            Assert.True(transformer.Transform__Called);
            Assert.Equal(runner.Executor.RunAssembly__Result.OuterXml, transformer.Transform_Xml);
        }
    }

    public class RunClass
    {
        [Fact]
        public void PassingTestReturnsPassed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='1' failed='0' skipped='0'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Pass' time='1.234' />
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForClass(xml);

            TestRunnerResult result = runner.RunClass(null);

            Assert.Equal(TestRunnerResult.Passed, result);
        }

        [Fact]
        public void FailingTestReturnsFailed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='0' failed='1' skipped='0'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Fail' time='1.234'>
        <failure exception-type='Exception.Type'>
            <message><![CDATA[Failure message]]></message>
            <stack-trace><![CDATA[Stack trace]]></stack-trace>
        </failure>
    </test>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForClass(xml);

            TestRunnerResult result = runner.RunClass(null);

            Assert.Equal(TestRunnerResult.Failed, result);
        }

        [Fact]
        public void SkippedTestReturnsPassed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='0' failed='0' skipped='1'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Skip'>
        <reason>
            <message><![CDATA[Skip reason]]></message>
        </reason>
    </test>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForClass(xml);

            TestRunnerResult result = runner.RunClass(null);

            Assert.Equal(TestRunnerResult.Passed, result);
        }

        [Fact]
        public void ClassFailureReturnsFailed()
        {
            const string xml = @"
<class time='1.234' name='Class.Name' total='1' passed='0' failed='1' skipped='0'>
    <failure exception-type='Exception.Type'>
        <message><![CDATA[Failure message]]></message>
        <stack-trace><![CDATA[Stack trace]]></stack-trace>
    </failure>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForClass(xml);

            TestRunnerResult result = runner.RunClass(null);

            Assert.Equal(TestRunnerResult.Failed, result);

        }

        [Fact]
        public void ValuesArePassedToExecutorWrapper()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly();

            runner.RunClass("foo");

            Assert.Equal("foo", runner.Executor.RunClass_Type);
        }

        [Fact]
        public void ThrownExceptionSentToLoggerAndFailedIsReturned()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly();
            Exception exception = new InvalidOperationException();
            runner.Executor.RunClass__CallbackEvent += delegate { throw exception; };

            TestRunnerResult result = runner.RunClass(null);

            runner.Logger.Verify(l => l.ExceptionThrown(@"C:\foo\bar.dll", exception));
            Assert.Equal(TestRunnerResult.Failed, result);
        }
    }

    public class RunTest
    {
        [Fact]
        public void PassingTestReturnsPassed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='1' failed='0' skipped='0'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Pass' time='1.234' />
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForTest(xml);

            TestRunnerResult result = runner.RunTest(null, null);

            Assert.Equal(TestRunnerResult.Passed, result);
        }

        [Fact]
        public void FailingTestReturnsFailed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='0' failed='1' skipped='0'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Fail' time='1.234'>
        <failure exception-type='Exception.Type'>
            <message><![CDATA[Failure message]]></message>
            <stack-trace><![CDATA[Stack trace]]></stack-trace>
        </failure>
    </test>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForTest(xml);

            TestRunnerResult result = runner.RunTest(null, null);

            Assert.Equal(TestRunnerResult.Failed, result);
        }

        [Fact]
        public void SkippedTestReturnsPassed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='0' failed='0' skipped='1'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Skip'>
        <reason>
            <message><![CDATA[Skip reason]]></message>
        </reason>
    </test>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForTest(xml);

            TestRunnerResult result = runner.RunTest(null, null);

            Assert.Equal(TestRunnerResult.Passed, result);
        }

        [Fact]
        public void ClassFailureReturnsFailed()
        {
            const string xml = @"
<class time='1.234' name='Class.Name' total='1' passed='0' failed='1' skipped='0'>
    <failure exception-type='Exception.Type'>
        <message><![CDATA[Failure message]]></message>
        <stack-trace><![CDATA[Stack trace]]></stack-trace>
    </failure>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForTest(xml);

            TestRunnerResult result = runner.RunTest(null, null);

            Assert.Equal(TestRunnerResult.Failed, result);

        }

        [Fact]
        public void ValuesArePassedToExecutorWrapper()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly();

            runner.RunTest("foo", "bar");

            Assert.Equal("foo", runner.Executor.RunTest_Type);
            Assert.Equal("bar", runner.Executor.RunTest_Method);
        }

        [Fact]
        public void ThrownExceptionSentToLoggerAndFailedIsReturned()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly();
            Exception exception = new InvalidOperationException();
            runner.Executor.RunTest__CallbackEvent += delegate { throw exception; };

            TestRunnerResult result = runner.RunTest(null, null);

            runner.Logger.Verify(l => l.ExceptionThrown(@"C:\foo\bar.dll", exception));
            Assert.Equal(TestRunnerResult.Failed, result);
        }
    }

    public class RunTests
    {
        [Fact]
        public void PassingTestReturnsPassed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='1' failed='0' skipped='0'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Pass' time='1.234' />
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForTests(xml);

            TestRunnerResult result = runner.RunTests(null, null);

            Assert.Equal(TestRunnerResult.Passed, result);
        }

        [Fact]
        public void FailingTestReturnsFailed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='0' failed='1' skipped='0'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Fail' time='1.234'>
        <failure exception-type='Exception.Type'>
            <message><![CDATA[Failure message]]></message>
            <stack-trace><![CDATA[Stack trace]]></stack-trace>
        </failure>
    </test>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForTests(xml);

            TestRunnerResult result = runner.RunTests(null, null);

            Assert.Equal(TestRunnerResult.Failed, result);
        }

        [Fact]
        public void SkippedTestReturnsPassed()
        {
            const string xml = @"
<class time='1.234' name='ThisIsTheType' total='1' passed='0' failed='0' skipped='1'>
    <test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Skip'>
        <reason>
            <message><![CDATA[Skip reason]]></message>
        </reason>
    </test>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForTests(xml);

            TestRunnerResult result = runner.RunTests(null, null);

            Assert.Equal(TestRunnerResult.Passed, result);
        }

        [Fact]
        public void ClassFailureReturnsFailed()
        {
            const string xml = @"
<class time='1.234' name='Class.Name' total='1' passed='0' failed='1' skipped='0'>
    <failure exception-type='Exception.Type'>
        <message><![CDATA[Failure message]]></message>
        <stack-trace><![CDATA[Stack trace]]></stack-trace>
    </failure>
</class>";
            TestableTestRunner runner = TestableTestRunner.CreateForTests(xml);

            TestRunnerResult result = runner.RunTests(null, null);

            Assert.Equal(TestRunnerResult.Failed, result);
        }

        [Fact]
        public void ValuesArePassedToExecutorWrapper()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly();
            List<string> testList = new List<string> { "bar", "baz" };

            runner.RunTests("foo", testList);

            Assert.Equal("foo", runner.Executor.RunTests_Type);
            Assert.Equal(testList, runner.Executor.RunTests_Methods);
        }

        [Fact]
        public void ThrownExceptionSentToLoggerAndFailedIsReturned()
        {
            TestableTestRunner runner = TestableTestRunner.CreateForAssembly();
            Exception exception = new InvalidOperationException();
            runner.Executor.RunTests__CallbackEvent += delegate { throw exception; };

            TestRunnerResult result = runner.RunTests(null, null);

            runner.Logger.Verify(l => l.ExceptionThrown(@"C:\foo\bar.dll", exception));
            Assert.Equal(TestRunnerResult.Failed, result);
        }
    }

    internal class TestableTestRunner : TestRunner
    {
        public readonly StubExecutorWrapper Executor;
        public readonly Mock<IRunnerLogger> Logger;

        private TestableTestRunner(StubExecutorWrapper executor, Mock<IRunnerLogger> logger)
            : base(executor, logger.Object)
        {
            Executor = executor;
            Logger = logger;
        }


        private static TestableTestRunner Create()
        {
            Mock<IRunnerLogger> logger = new Mock<IRunnerLogger>();
            StubExecutorWrapper executor = new StubExecutorWrapper
                                                {
                                                    AssemblyFilename = @"C:\foo\bar.dll",
                                                    XunitVersion = "abcd",
                                                };

            return new TestableTestRunner(executor, logger);
        }

        public static TestableTestRunner CreateForAssembly()
        {
            return CreateForAssembly(@"<assembly name='C:\foo\bar.dll' run-date='2008-09-20' run-time='12:34:56' time='0.000' total='0' passed='0' failed='0' skipped='0'/>");
        }

        public static TestableTestRunner CreateForAssemblyWithConfigFile(string configFilename)
        {
            TestableTestRunner result = CreateForAssembly();
            result.Executor.ConfigFilename = configFilename;
            return result;
        }

        public static TestableTestRunner CreateForAssembly(string xml)
        {
            TestableTestRunner result = Create();
            XmlNode xmlNode = xml == null ? null : MakeXmlNode(xml);
            result.Executor.RunAssembly__Result = xmlNode;
            return result;
        }

        public static TestableTestRunner CreateForClass(string xml)
        {
            TestableTestRunner result = Create();
            XmlNode xmlNode = xml == null ? null : MakeXmlNode(xml);
            result.Executor.RunClass__Result = xmlNode;
            return result;
        }

        public static TestableTestRunner CreateForTest(string xml)
        {
            TestableTestRunner result = Create();
            XmlNode xmlNode = xml == null ? null : MakeXmlNode(xml);
            result.Executor.RunTest__Result = xmlNode;
            return result;
        }

        public static TestableTestRunner CreateForTests(string xml)
        {
            TestableTestRunner result = Create();
            XmlNode xmlNode = xml == null ? null : MakeXmlNode(xml);
            result.Executor.RunTests__Result = xmlNode;
            return result;
        }

        public static XmlNode MakeXmlNode(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.ChildNodes[0];
        }
    }
}