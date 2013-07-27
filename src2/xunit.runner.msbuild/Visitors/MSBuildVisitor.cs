using System;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Microsoft.Build.Utilities;
using Xunit.Abstractions;

namespace Xunit.Runner.MSBuild
{
    public class MSBuildVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        readonly XElement assemblyElement;
        readonly ConcurrentDictionary<ITestCollection, XElement> testCollectionElements = new ConcurrentDictionary<ITestCollection, XElement>();

        public MSBuildVisitor(TaskLoggingHelper log, XElement assemblyElement, Func<bool> cancelThunk)
        {
            Log = log;
            CancelThunk = cancelThunk ?? (() => false);

            this.assemblyElement = assemblyElement;
        }

        public readonly Func<bool> CancelThunk;
        public int Failed;
        public readonly TaskLoggingHelper Log;
        public int Skipped;
        public decimal Time;
        public int Total;

        XElement CreateTestResultElement(ITestResultMessage testResult, string resultText)
        {
            var collectionElement = GetTestCollectionElement(testResult.TestCase.TestCollection);
            var testResultElement =
                new XElement("test",
                    new XAttribute("name", XmlEscape(testResult.TestDisplayName)),
                    new XAttribute("type", testResult.TestCase.Class.Name),
                    new XAttribute("method", testResult.TestCase.Method.Name),
                    new XAttribute("time", testResult.ExecutionTime.ToString("0.000")),
                    new XAttribute("result", resultText)
                );

            if (testResult.TestCase.SourceInformation != null)
            {
                if (testResult.TestCase.SourceInformation.FileName != null)
                    testResultElement.Add(new XAttribute("source-file", testResult.TestCase.SourceInformation.FileName));
                if (testResult.TestCase.SourceInformation.LineNumber != null)
                    testResultElement.Add(new XAttribute("source-line", testResult.TestCase.SourceInformation.LineNumber.GetValueOrDefault()));
            }

            if (testResult.TestCase.Traits != null && testResult.TestCase.Traits.Count > 0)
            {
                var traitsElement = new XElement("traits");

                foreach (var kvp in testResult.TestCase.Traits)
                    traitsElement.Add(
                        new XElement("trait",
                            new XAttribute("name", XmlEscape(kvp.Key)),
                            new XAttribute("value", XmlEscape(kvp.Value))
                        )
                    );

                testResultElement.Add(traitsElement);
            }

            collectionElement.Add(testResultElement);

            return testResultElement;
        }

        XElement GetTestCollectionElement(ITestCollection testCollection)
        {
            return testCollectionElements.GetOrAdd(testCollection, tc => new XElement("collection"));
        }

        public override bool OnMessage(ITestMessage message)
        {
            var result = base.OnMessage(message);
            if (result)
                result = !CancelThunk();

            return result;
        }

        protected override bool Visit(ITestAssemblyFinished assemblyFinished)
        {
            Total += assemblyFinished.TestsRun;
            Failed += assemblyFinished.TestsFailed;
            Skipped += assemblyFinished.TestsSkipped;
            Time += assemblyFinished.ExecutionTime;

            if (assemblyElement != null)
            {
                assemblyElement.Add(
                    new XAttribute("total", Total),
                    new XAttribute("passed", Total - Failed - Skipped),
                    new XAttribute("failed", Failed),
                    new XAttribute("skipped", Skipped),
                    new XAttribute("time", Time.ToString("0.000"))
                );

                foreach (var element in testCollectionElements.Values)
                    assemblyElement.Add(element);
            }

            return base.Visit(assemblyFinished);
        }

        protected override bool Visit(ITestAssemblyStarting assemblyStarting)
        {
            if (assemblyElement != null)
            {
                assemblyElement.Add(
                    new XAttribute("name", assemblyStarting.AssemblyFileName),
                    new XAttribute("environment", assemblyStarting.TestEnvironment),
                    new XAttribute("test-framework", assemblyStarting.TestFrameworkDisplayName),
                    new XAttribute("run-date", assemblyStarting.StartTime.ToString("yyyy-MM-dd")),
                    new XAttribute("run-time", assemblyStarting.StartTime.ToString("HH:mm:ss"))
                );

                if (assemblyStarting.ConfigFileName != null)
                    assemblyElement.Add(new XAttribute("config-file", assemblyStarting.ConfigFileName));
            }

            return base.Visit(assemblyStarting);
        }

        protected override bool Visit(ITestCollectionFinished testCollectionFinished)
        {
            if (assemblyElement != null)
            {
                var collectionElement = GetTestCollectionElement(testCollectionFinished.TestCollection);
                collectionElement.Add(
                    new XAttribute("total", testCollectionFinished.TestsRun),
                    new XAttribute("passed", testCollectionFinished.TestsRun - testCollectionFinished.TestsFailed - testCollectionFinished.TestsSkipped),
                    new XAttribute("failed", testCollectionFinished.TestsFailed),
                    new XAttribute("skipped", testCollectionFinished.TestsSkipped),
                    new XAttribute("name", XmlEscape(testCollectionFinished.TestCollection.DisplayName)),
                    new XAttribute("time", testCollectionFinished.ExecutionTime.ToString("0.000"))
                );
            }

            return base.Visit(testCollectionFinished);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            if (assemblyElement != null)
            {
                var testElement = CreateTestResultElement(testFailed, "Fail");
                testElement.Add(
                    new XElement("failure",
                        new XAttribute("exception-type", testFailed.ExceptionType),
                        new XElement("message", new XCData(XmlEscape(testFailed.Message))),
                        new XElement("stack-trace", new XCData(testFailed.StackTrace ?? String.Empty))
                    )
                );
            }

            return base.Visit(testFailed);
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            if (assemblyElement != null)
                CreateTestResultElement(testPassed, "Pass");

            return base.Visit(testPassed);
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            if (assemblyElement != null)
            {
                var testElement = CreateTestResultElement(testSkipped, "Skip");
                testElement.Add(new XElement("reason", new XCData(XmlEscape(testSkipped.Reason))));
            }

            return base.Visit(testSkipped);
        }

        protected static string Escape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0");
        }

        protected static string XmlEscape(string value)
        {
            if (value == null)
                return String.Empty;

            return value.Replace("\0", "\\0");
        }
    }
}