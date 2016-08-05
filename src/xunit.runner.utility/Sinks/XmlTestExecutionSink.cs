#if !NET35

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IMessageSinkWithTypes"/> which records all operations into
    /// xUnit.net v2 XML format.
    /// </summary>
    public class XmlTestExecutionSink : TestMessageSink
    {
        readonly XElement assemblyElement;
        readonly XElement errorsElement;
        readonly ConcurrentDictionary<Guid, XElement> testCollectionElements = new ConcurrentDictionary<Guid, XElement>();

        /// <summary>
        /// Initializes a new instance of <see cref="XmlTestExecutionSink"/>.
        /// </summary>
        /// <param name="assemblyElement">The root XML assembly element to collect the result XML.</param>
        /// <param name="cancelThunk">The callback used to determine when to cancel execution.</param>
        public XmlTestExecutionSink(XElement assemblyElement, Func<bool> cancelThunk)
        {
            CancelThunk = cancelThunk ?? (() => false);

            this.assemblyElement = assemblyElement;

            if (this.assemblyElement != null)
            {
                errorsElement = new XElement("errors");
                this.assemblyElement.Add(errorsElement);
            }

            TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
            TestAssemblyStartingEvent += HandleTestAssemblyStarting;
            TestCollectionFinishedEvent += HandleTestCollectionFinished;
            TestFailedEvent += HandleTestFailed;
            TestPassedEvent += HandleTestPassed;
            TestSkippedEvent += HandleTestSkipped;
            ErrorMessageEvent += HandleErrorMessage;
            TestAssemblyCleanupFailureEvent += HandleTestAssemblyCollection;
            TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
            TestClassCleanupFailureEvent += HandleTestClassCleanupFailure;
            TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
            TestCleanupFailureEvent += HandleTestCleanupFailure;
            TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
        }

        /// <summary>
        /// Gets the callback used to determine when to cancel execution.
        /// </summary>
        public Func<bool> CancelThunk { get; }

        /// <summary>
        /// Gets or sets the number of errors that have occurred (outside of actual test execution).
        /// </summary>
        public int Errors { get; set; }

        /// <summary>
        /// Gets or sets the number of tests which failed.
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// Gets or sets the number of tests which were skipped.
        /// </summary>
        public int Skipped { get; set; }

        /// <summary>
        /// Gets or sets the time spent executing tests, in seconds.
        /// </summary>
        public decimal Time { get; set; }

        /// <summary>
        /// Gets or sets the total number of tests, regardless of result.
        /// </summary>
        public int Total { get; set; }

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

        /// <inheritdoc/>
        public override bool OnMessageWithTypes(IMessageSinkMessage message, string[] messageTypes)
        {
            var result = base.OnMessageWithTypes(message, messageTypes);
            if (result)
                result = !CancelThunk();

            return result;
        }

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestAssemblyFinishedEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            var assemblyFinished = args.Message;

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
        }

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestAssemblyStartingEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
        {
            if (assemblyElement != null)
            {
                var assemblyStarting = args.Message;
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
        }

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestCollectionFinishedEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
        {
            if (assemblyElement != null)
            {
                var testCollectionFinished = args.Message;
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
        }

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestFailedEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            if (assemblyElement != null)
            {
                var testFailed = args.Message;
                var testElement = CreateTestResultElement(testFailed, "Fail");
                testElement.Add(CreateFailureElement(testFailed));
            }
        }

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestPassedEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            if (assemblyElement != null)
                CreateTestResultElement(args.Message, "Pass");
        }

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestSkippedEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            if (assemblyElement != null)
            {
                var testSkipped = args.Message;
                var testElement = CreateTestResultElement(testSkipped, "Skip");
                testElement.Add(new XElement("reason", new XCData(XmlEscape(testSkipped.Reason))));
            }
        }

        /// <summary>
        /// Called when <see cref="TestMessageSink.ErrorMessageEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
            => AddError("fatal", null, args.Message);

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestAssemblyCleanupFailureEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestAssemblyCollection(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
            => AddError("assembly-cleanup", args.Message.TestAssembly.Assembly.AssemblyPath, args.Message);

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestCaseCleanupFailureEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
            => AddError("test-case-cleanup", args.Message.TestCase.DisplayName, args.Message);

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestClassCleanupFailureEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
            => AddError("test-class-cleanup", args.Message.TestClass.Class.Name, args.Message);

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestCollectionCleanupFailureEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
            => AddError("test-collection-cleanup", args.Message.TestCollection.DisplayName, args.Message);

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestCleanupFailureEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
            => AddError("test-cleanup", args.Message.Test.DisplayName, args.Message);

        /// <summary>
        /// Called when <see cref="TestMessageSink.TestMethodCleanupFailureEvent"/> is raised.
        /// </summary>
        /// <param name="args">An object that contains the event data.</param>
        protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
            => AddError("test-method-cleanup", args.Message.TestMethod.Method.Name, args.Message);

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

        static string Escape(string value)
        {
            if (value == null)
                return string.Empty;

            return value.Replace("\\", "\\\\")
                        .Replace("\r", "\\r")
                        .Replace("\n", "\\n")
                        .Replace("\t", "\\t")
                        .Replace("\0", "\\0")
                        .Replace("\a", "\\a")
                        .Replace("\b", "\\b")
                        .Replace("\v", "\\v")
                        .Replace("\"", "\\\"")
                        .Replace("\f", "\\f");
        }

        /// <summary>
        /// Escapes a string for placing into the XML.
        /// </summary>
        /// <param name="value">The value to be escaped.</param>
        /// <returns>The escaped value.</returns>
        protected static string XmlEscape(string value)
        {
            if (value == null)
                return string.Empty;

            value = Escape(value);
            var escapedValue = new StringBuilder(value.Length);
            for (var idx = 0; idx < value.Length; ++idx)
            {
                char ch = value[idx];
                if (ch < 32)
                    escapedValue.Append($@"\x{(+ch).ToString("x2")}");
                else if (char.IsSurrogatePair(value, idx)) // Takes care of the case when idx + 1 == value.Length
                {
                    escapedValue.Append(ch); // Append valid surrogate chars like normal
                    escapedValue.Append(value[++idx]);
                }
                // Check for invalid chars and append them like \x----
                else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
                    escapedValue.Append($@"\x{(+ch).ToString("x4")}");
                else
                    escapedValue.Append(ch);
            }

            return escapedValue.ToString();
        }
    }
}

#endif
