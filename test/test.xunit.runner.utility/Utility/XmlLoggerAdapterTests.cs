using System.Xml;
using Moq;
using Xunit;

public class XmlLoggerAdapterTests
{
    Mock<IRunnerLogger> logger = new Mock<IRunnerLogger>();

    [Fact]
    public void StartNodeCallsTestStart()
    {
        const string xml = @"<start name='Name of the test' type='Fully.Qualified.TypeName' method='MethodName' />";

        XmlLoggerAdapter.LogStartNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.TestStart("Name of the test", "Fully.Qualified.TypeName", "MethodName"));
    }

    [Fact]
    public void TestNodeCallsTestFinished()
    {
        const string xml = @"<test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Pass' time='1.234' />";

        XmlLoggerAdapter.LogTestNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.TestFinished("This is the test name", "ThisIsTheType", "ThisIsTheMethod"));
    }

    [Fact]
    public void PassedTestNodeCallsTestPassed()
    {
        const string xml = @"<test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Pass' time='1.234' />";

        XmlLoggerAdapter.LogTestNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.TestPassed("This is the test name", "ThisIsTheType", "ThisIsTheMethod", 1.234, null));
    }

    [Fact]
    public void PassedTestNodeWithOutput()
    {
        const string xml = @"<test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Pass' time='1.234'><output><![CDATA[output message]]></output></test>";

        XmlLoggerAdapter.LogTestNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.TestPassed("This is the test name", "ThisIsTheType", "ThisIsTheMethod", 1.234, "output message"));
    }

    [Fact]
    public void FailedTestNodeCallsTestFailed()
    {
        const string xml = @"<test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Fail' time='1.234'><failure exception-type='Exception.Type'><message><![CDATA[Failure message]]></message><stack-trace><![CDATA[Stack trace]]></stack-trace></failure></test>";

        XmlLoggerAdapter.LogTestNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.TestFailed("This is the test name", 
                                              "ThisIsTheType", 
                                              "ThisIsTheMethod", 
                                              1.234, 
                                              null,
                                              "Exception.Type", 
                                              "Failure message", 
                                              "Stack trace"));
    }

    [Fact]
    public void FailedTestNodeWithOutput()
    {
        const string xml = @"<test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Fail' time='1.234'><output><![CDATA[output message]]></output><failure exception-type='Exception.Type'><message><![CDATA[Failure message]]></message><stack-trace><![CDATA[Stack trace]]></stack-trace></failure></test>";

        XmlLoggerAdapter.LogTestNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.TestFailed("This is the test name",
                                              "ThisIsTheType",
                                              "ThisIsTheMethod",
                                              1.234,
                                              "output message",
                                              "Exception.Type",
                                              "Failure message",
                                              "Stack trace"));
    }

    [Fact]
    public void FailedTestWithNoStackTrace()
    {
        const string xml = @"<test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Fail' time='1.234'><failure exception-type='Exception.Type'><message><![CDATA[Failure message]]></message></failure></test>";

        XmlLoggerAdapter.LogTestNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.TestFailed("This is the test name",
                                              "ThisIsTheType",
                                              "ThisIsTheMethod",
                                              1.234,
                                              null,
                                              "Exception.Type",
                                              "Failure message",
                                              null));
    }

    [Fact]
    public void SkippedTestNodeCallsTestSkipped()
    {
        const string xml = @"<test name='This is the test name' type='ThisIsTheType' method='ThisIsTheMethod' result='Skip'><reason><message><![CDATA[Skip reason]]></message></reason></test>";

        XmlLoggerAdapter.LogTestNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.TestSkipped("This is the test name",
                                              "ThisIsTheType",
                                              "ThisIsTheMethod",
                                              "Skip reason"));
    }

    [Fact]
    public void ClassNodeWithNoFailureYieldsNoCallback()
    {
        logger = new Mock<IRunnerLogger>(MockBehavior.Strict);
        const string xml = @"<class time='1.234' name='Class.Name' total='10' passed='7' failed='2' skipped='1' />";

        XmlLoggerAdapter.LogClassNode(CreateXmlNode(xml), logger.Object);

        logger.Verify();
    }

    [Fact]
    public void ClassNodeWithFailureCallsClassFailed_NoExceptionType()
    {
        const string xml = @"<class time='1.234' name='Class.Name' total='10' passed='7' failed='2' skipped='1'><failure><message><![CDATA[Failure message]]></message><stack-trace><![CDATA[Stack trace]]></stack-trace></failure></class>";

        XmlLoggerAdapter.LogClassNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.ClassFailed("Class.Name", null, 
            "Failure message", "Stack trace"));
    }

    [Fact]
    public void ClassNodeWithFailureCallsClassFailed_WithExceptionType()
    {
        const string xml = @"<class time='1.234' name='Class.Name' total='10' passed='7' failed='2' skipped='1'><failure exception-type='Exception.Type'><message><![CDATA[Failure message]]></message><stack-trace><![CDATA[Stack trace]]></stack-trace></failure></class>";

        XmlLoggerAdapter.LogClassNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.ClassFailed("Class.Name", "Exception.Type",
            "Failure message", "Stack trace"));
    }

    [Fact]
    public void AssemblyNodeCallsAssemblyFinished()
    {
        const string xml = @"<assembly name='C:\foo\bar.dll' run-date='2008-09-20' run-time='12:34:56' time='1.234' total='10' passed='7' failed='2' skipped='1' />";

        XmlLoggerAdapter.LogAssemblyNode(CreateXmlNode(xml), logger.Object);

        logger.Verify(l => l.AssemblyFinished(@"C:\foo\bar.dll", 10, 2, 1, 1.234));
    }

    static XmlNode CreateXmlNode(string xml)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        return doc.ChildNodes[0];
    }
}