﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class XmlTestExecutionVisitorTests
{
    public class OnMessage
    {
        readonly IMessageSinkMessage testMessage;

        public OnMessage()
        {
            testMessage = Substitute.For<IMessageSinkMessage>();
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var visitor = new XmlTestExecutionVisitor(null, () => true);

            var result = visitor.OnMessage(testMessage);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var visitor = new XmlTestExecutionVisitor(null, () => false);

            var result = visitor.OnMessage(testMessage);

            Assert.True(result);
        }
    }

    public class OnMessage_TestAssemblyFinished
    {
        [Fact]
        public void AddsStatisticsToRunningTotal()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);

            var visitor = new XmlTestExecutionVisitor(null, () => false) { Total = 10, Failed = 10, Skipped = 10, Time = 10M };

            visitor.OnMessage(assemblyFinished);

            Assert.Equal(2122, visitor.Total);
            Assert.Equal(52, visitor.Failed);
            Assert.Equal(16, visitor.Skipped);
            Assert.Equal(133.4567M, visitor.Time);
        }
    }

    public class Xml
    {
        [Fact]
        public void AddsAssemblyStartingInformationToXml()
        {
            var assemblyStarting = Substitute.For<ITestAssemblyStarting>();
            assemblyStarting.TestAssembly.Assembly.AssemblyPath.Returns("assembly");
            assemblyStarting.TestAssembly.ConfigFileName.Returns("config");
            assemblyStarting.StartTime.Returns(new DateTime(2013, 7, 6, 16, 24, 32));
            assemblyStarting.TestEnvironment.Returns("256-bit MentalFloss");
            assemblyStarting.TestFrameworkDisplayName.Returns("xUnit.net v14.42");

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(assemblyStarting);

            Assert.Equal("assembly", assemblyElement.Attribute("name").Value);
            Assert.Equal("256-bit MentalFloss", assemblyElement.Attribute("environment").Value);
            Assert.Equal("xUnit.net v14.42", assemblyElement.Attribute("test-framework").Value);
            Assert.Equal("config", assemblyElement.Attribute("config-file").Value);
            Assert.Equal("2013-07-06", assemblyElement.Attribute("run-date").Value);
            Assert.Equal("16:24:32", assemblyElement.Attribute("run-time").Value);
        }

        [Fact]
        public void AssemblyStartingDoesNotIncludeNullConfigFile()
        {
            var assemblyStarting = Substitute.For<ITestAssemblyStarting>();
            assemblyStarting.TestAssembly.ConfigFileName.Returns((string)null);

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(assemblyStarting);

            Assert.Null(assemblyElement.Attribute("config-file"));
        }

        [CulturedFact]
        public void AddsAssemblyFinishedInformationToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(assemblyFinished);

            Assert.Equal("2112", assemblyElement.Attribute("total").Value);
            Assert.Equal("2064", assemblyElement.Attribute("passed").Value);
            Assert.Equal("42", assemblyElement.Attribute("failed").Value);
            Assert.Equal("6", assemblyElement.Attribute("skipped").Value);
            Assert.Equal(123.457M.ToString(), assemblyElement.Attribute("time").Value);
        }

        [CulturedFact]
        public void AddsTestCollectionElementsToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCollection = Substitute.For<ITestCollection>();
            testCollection.DisplayName.Returns("Collection Name");
            var testCollectionFinished = Substitute.For<ITestCollectionFinished>();
            testCollectionFinished.TestCollection.Returns(testCollection);
            testCollectionFinished.TestsRun.Returns(2112);
            testCollectionFinished.TestsFailed.Returns(42);
            testCollectionFinished.TestsSkipped.Returns(6);
            testCollectionFinished.ExecutionTime.Returns(123.4567M);

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(testCollectionFinished);
            visitor.OnMessage(assemblyFinished);

            var collectionElement = Assert.Single(assemblyElement.Elements("collection"));
            Assert.Equal("Collection Name", collectionElement.Attribute("name").Value);
            Assert.Equal("2112", collectionElement.Attribute("total").Value);
            Assert.Equal("2064", collectionElement.Attribute("passed").Value);
            Assert.Equal("42", collectionElement.Attribute("failed").Value);
            Assert.Equal("6", collectionElement.Attribute("skipped").Value);
            Assert.Equal(123.457M.ToString(), collectionElement.Attribute("time").Value);
        }

        [CulturedFact]
        public void AddsPassingTestElementToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            testCase.SourceInformation.Returns(new SourceInformation());
            var testPassed = Substitute.For<ITestPassed>();
            testPassed.TestCase.Returns(testCase);
            testPassed.TestDisplayName.Returns("Test Display Name");
            testPassed.ExecutionTime.Returns(123.4567M);

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(testPassed);
            visitor.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("Test Display Name", testElement.Attribute("name").Value);
            Assert.Equal("XmlTestExecutionVisitorTests+Xml+ClassUnderTest", testElement.Attribute("type").Value);
            Assert.Equal("TestMethod", testElement.Attribute("method").Value);
            Assert.Equal("Pass", testElement.Attribute("result").Value);
            Assert.Equal(123.457M.ToString(), testElement.Attribute("time").Value);
            Assert.Null(testElement.Attribute("source-file"));
            Assert.Null(testElement.Attribute("source-line"));
            Assert.Empty(testElement.Elements("traits"));
            Assert.Empty(testElement.Elements("failure"));
            Assert.Empty(testElement.Elements("reason"));
        }

        [CulturedFact]
        public void AddsFailingTestElementToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestCase.Returns(testCase);
            testFailed.TestDisplayName.Returns("Test Display Name");
            testFailed.ExecutionTime.Returns(123.4567M);
            testFailed.ExceptionTypes.Returns(new[] { "Exception Type" });
            testFailed.Messages.Returns(new[] { "Exception Message" });
            testFailed.StackTraces.Returns(new[] { "Exception Stack Trace" });

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(testFailed);
            visitor.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("Test Display Name", testElement.Attribute("name").Value);
            Assert.Equal("XmlTestExecutionVisitorTests+Xml+ClassUnderTest", testElement.Attribute("type").Value);
            Assert.Equal("TestMethod", testElement.Attribute("method").Value);
            Assert.Equal("Fail", testElement.Attribute("result").Value);
            Assert.Equal(123.457M.ToString(), testElement.Attribute("time").Value);
            var failureElement = Assert.Single(testElement.Elements("failure"));
            Assert.Equal("Exception Type", failureElement.Attribute("exception-type").Value);
            Assert.Equal("Exception Type : Exception Message", failureElement.Elements("message").Single().Value);
            Assert.Equal("Exception Stack Trace", failureElement.Elements("stack-trace").Single().Value);
            Assert.Empty(testElement.Elements("reason"));
        }

        [Fact]
        public void NullStackTraceInFailedTestResultsInEmptyStackTraceXmlElement()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestCase.Returns(testCase);
            testFailed.ExceptionTypes.Returns(new[] { "ExceptionType" });
            testFailed.Messages.Returns(new[] { "Exception Message" });
            testFailed.StackTraces.Returns(new[] { (string)null });
            testFailed.ExceptionParentIndices.Returns(new[] { -1 });

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(testFailed);
            visitor.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            var failureElement = Assert.Single(testElement.Elements("failure"));
            Assert.Empty(failureElement.Elements("stack-trace").Single().Value);
        }

        [CulturedFact]
        public void AddsSkippedTestElementToXml()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            var testSkipped = Substitute.For<ITestSkipped>();
            testSkipped.TestCase.Returns(testCase);
            testSkipped.TestDisplayName.Returns("Test Display Name");
            testSkipped.ExecutionTime.Returns(0.0M);
            testSkipped.Reason.Returns("Skip Reason");

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(testSkipped);
            visitor.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("Test Display Name", testElement.Attribute("name").Value);
            Assert.Equal("XmlTestExecutionVisitorTests+Xml+ClassUnderTest", testElement.Attribute("type").Value);
            Assert.Equal("TestMethod", testElement.Attribute("method").Value);
            Assert.Equal("Skip", testElement.Attribute("result").Value);
            Assert.Equal(0.0M.ToString("0.000"), testElement.Attribute("time").Value);
            var reasonElement = Assert.Single(testElement.Elements("reason"));
            Assert.Equal("Skip Reason", reasonElement.Value);
            Assert.Empty(testElement.Elements("failure"));
        }

        [Fact]
        public void TestElementSourceInfoIsPlacedInXmlWhenPresent()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            testCase.SourceInformation.Returns(new SourceInformation { FileName = "source file", LineNumber = 42 });
            var testPassed = Substitute.For<ITestPassed>();
            testPassed.TestCase.Returns(testCase);

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(testPassed);
            visitor.OnMessage(assemblyFinished);

            var testElement = Assert.Single(assemblyElement.Elements("collection").Single().Elements("test"));
            Assert.Equal("source file", testElement.Attribute("source-file").Value);
            Assert.Equal("42", testElement.Attribute("source-line").Value);
        }

        [Fact]
        public void TestElementTraisArePlacedInXmlWhenPresent()
        {
            var traits = new Dictionary<string, List<string>>
            {
                { "name1", new List<string> { "value1" }},
                { "name2", new List<string> { "value2" }}
            };
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var passingTestCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            passingTestCase.Traits.Returns(traits);
            var testPassed = Substitute.For<ITestPassed>();
            testPassed.TestCase.Returns(passingTestCase);

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(testPassed);
            visitor.OnMessage(assemblyFinished);

            var traitsElements = assemblyElement.Elements("collection").Single().Elements("test").Single().Elements("traits").Single().Elements("trait");
            var name1Element = Assert.Single(traitsElements, e => e.Attribute("name").Value == "name1");
            Assert.Equal("value1", name1Element.Attribute("value").Value);
            var name2Element = Assert.Single(traitsElements, e => e.Attribute("name").Value == "name2");
            Assert.Equal("value2", name2Element.Attribute("value").Value);
        }

        [Fact]
        public void IllegalXmlDoesNotPreventXmlFromBeingSaved()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            var testCase = Mocks.TestCase<ClassUnderTest>("TestMethod");
            var testSkipped = Substitute.For<ITestSkipped>();
            testSkipped.TestCase.Returns(testCase);
            testSkipped.TestDisplayName.Returns("Display\0\r\nName");
            testSkipped.Reason.Returns("Bad\0\r\nString");

            var assemblyElement = new XElement("assembly");
            var visitor = new XmlTestExecutionVisitor(assemblyElement, () => false);

            visitor.OnMessage(testSkipped);
            visitor.OnMessage(assemblyFinished);

            using (var writer = new StringWriter())
                Assert.DoesNotThrow(() => assemblyElement.Save(writer));
        }

        class ClassUnderTest
        {
            [Fact]
            public void TestMethod() { }
        }
    }
}