using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    public class XmlTestExecutionVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        readonly XElement assemblyElement;
        readonly XElement errorsElement;
        readonly ConcurrentDictionary<Guid, XElement> testCollectionElements = new ConcurrentDictionary<Guid, XElement>();

        public XmlTestExecutionVisitor(XElement assemblyElement, Func<bool> cancelThunk)
        {
            CancelThunk = cancelThunk ?? (() => false);

            this.assemblyElement = assemblyElement;

            if (this.assemblyElement != null)
            {
                errorsElement = new XElement("errors");
                this.assemblyElement.Add(errorsElement);
            }
        }

        public readonly Func<bool> CancelThunk;
        public int Errors;
        public int Failed;
        public int Skipped;
        public decimal Time;
        public int Total;

        XElement CreateTestResultElement(ITestResultMessage testResult, string resultText)
        {
            var collectionElement = GetTestCollectionElement(testResult.TestCase.TestMethod.TestClass.TestCollection);
            var testResultElement =
                new XElement("test",
                    new XAttribute("name", XmlEscape(testResult.Test.DisplayName)),
                    new XAttribute("type", testResult.TestCase.TestMethod.TestClass.Class.Name),
                    new XAttribute("method", testResult.TestCase.TestMethod.Method.Name),
                    new XAttribute("time", testResult.ExecutionTime.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("result", resultText)
                );

            if (!string.IsNullOrWhiteSpace(testResult.Output))
            {
                testResultElement.Add(new XElement("output", new XCData(testResult.Output)));
            }

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

                foreach (var key in testResult.TestCase.Traits.Keys)
                    foreach (var value in testResult.TestCase.Traits[key])
                        traitsElement.Add(
                            new XElement("trait",
                                new XAttribute("name", XmlEscape(key)),
                                new XAttribute("value", XmlEscape(value))
                            )
                        );

                testResultElement.Add(traitsElement);
            }

            collectionElement.Add(testResultElement);

            return testResultElement;
        }

        XElement GetTestCollectionElement(ITestCollection testCollection)
        {
            return testCollectionElements.GetOrAdd(testCollection.UniqueID, tc => new XElement("collection"));
        }

        public override bool OnMessage(IMessageSinkMessage message)
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
                    new XAttribute("time", Time.ToString("0.000", CultureInfo.InvariantCulture)),
                    new XAttribute("errors", Errors)
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
                    new XAttribute("name", assemblyStarting.TestAssembly.Assembly.AssemblyPath),
                    new XAttribute("environment", assemblyStarting.TestEnvironment),
                    new XAttribute("test-framework", assemblyStarting.TestFrameworkDisplayName),
                    new XAttribute("run-date", assemblyStarting.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    new XAttribute("run-time", assemblyStarting.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
                );

                if (assemblyStarting.TestAssembly.ConfigFileName != null)
                    assemblyElement.Add(new XAttribute("config-file", assemblyStarting.TestAssembly.ConfigFileName));
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
                    new XAttribute("time", testCollectionFinished.ExecutionTime.ToString("0.000", CultureInfo.InvariantCulture))
                );
            }

            return base.Visit(testCollectionFinished);
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            if (assemblyElement != null)
            {
                var testElement = CreateTestResultElement(testFailed, "Fail");
                testElement.Add(CreateFailureElement(testFailed));
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

        protected override bool Visit(IErrorMessage error)
        {
            AddError("fatal", null, error);

            return base.Visit(error);
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            AddError("assembly-cleanup", cleanupFailure.TestAssembly.Assembly.AssemblyPath, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            AddError("test-case-cleanup", cleanupFailure.TestCase.DisplayName, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            AddError("test-class-cleanup", cleanupFailure.TestClass.Class.Name, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            AddError("test-collection-cleanup", cleanupFailure.TestCollection.DisplayName, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            AddError("test-cleanup", cleanupFailure.Test.DisplayName, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            AddError("test-method-cleanup", cleanupFailure.TestMethod.Method.Name, cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        void AddError(string type, string name, IFailureInformation failureInfo)
        {
            Errors++;

            if (errorsElement == null)
                return;

            var errorElement = new XElement("error", new XAttribute("type", type), CreateFailureElement(failureInfo));
            if (name != null)
                errorElement.Add(new XAttribute("name", name));

            errorsElement.Add(errorElement);
        }

        static XElement CreateFailureElement(IFailureInformation failureInfo)
        {
            return new XElement("failure",
                new XAttribute("exception-type", failureInfo.ExceptionTypes[0]),
                new XElement("message", new XCData(XmlEscape(ExceptionUtility.CombineMessages(failureInfo)))),
                new XElement("stack-trace", new XCData(ExceptionUtility.CombineStackTraces(failureInfo) ?? string.Empty))
            );
        }

        protected static string Escape(string value)
        {
            if (value == null)
                return string.Empty;

            return value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0");
        }

        protected static string XmlEscape(string value)
        {
            if (value == null)
                return string.Empty;

            value = Escape(value);
            var escapedValue = new StringBuilder(value.Length);
            for (var idx = 0; idx < value.Length; ++idx)
                if (value[idx] < 32)
                    escapedValue.Append($"\\x{((byte)value[idx]).ToString("x2")}");
                else
                    escapedValue.Append(value[idx]);

            return escapedValue.ToString();
        }
    }
}