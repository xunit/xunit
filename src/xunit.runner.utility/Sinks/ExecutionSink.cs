using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

#if !NET35
using System.Xml.Linq;
#endif

namespace Xunit
{
    /// <summary>
    /// This is the default implementation of <see cref="IExecutionSink"/>, which can perform several
    /// operations (including recording XML results, detecting long running tests, and failing skipped
    /// tests). This replaces the now deprecated delegating sink classes.
    /// </summary>
    public class ExecutionSink : LongLivedMarshalByRefObject, IExecutionSink
    {
        static readonly string[] FailSkipsExceptionTypes = new[] { "FAIL_SKIP" };
        static readonly int[] FailSkipsParentIndices = new[] { -1 };
        static readonly string[] FailSkipsStackTraces = new[] { "" };
        static readonly HashSet<string> TestFailedMessageTypes = new HashSet<string>(typeof(ITestFailed).GetInterfaces().Select(x => x.FullName));

        volatile int errors;
        readonly Dictionary<ITestCase, DateTime> executingTestCases;
        readonly IMessageSinkWithTypes innerSink;
        readonly ExecutionSinkOptions options;
        DateTime lastTestActivity;
        volatile int skipCount;
        ManualResetEvent stopEvent;
        bool stopRequested;

#if !NET35
        readonly XElement errorsElement;
        readonly Dictionary<Guid, XElement> testCollectionElements;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionSink"/> class.
        /// </summary>
        /// <param name="innerSink">The inner sink to forward messages to (typically the reporter
        /// message handler, retrieved by calling <see cref="IRunnerReporter.CreateMessageHandler"/>
        /// on the runner reporter, then wrapped with <see cref="MessageSinkWithTypesAdapter.Wrap"/>)</param>
        /// <param name="options">The options to use for the execution sink</param>
        public ExecutionSink(IMessageSinkWithTypes innerSink, ExecutionSinkOptions options)
        {
            Guard.ArgumentNotNull(nameof(innerSink), innerSink);
            Guard.ArgumentNotNull(nameof(options), options);

            this.innerSink = innerSink;
            this.options = options;

            NeedsFailSkips = options.FailSkips;

            if (options.LongRunningTestTime > TimeSpan.Zero)
            {
                NeedsTestTiming = true;
                executingTestCases = new Dictionary<ITestCase, DateTime>();
            }

#if !NET35
            if (options.AssemblyElement != null)
            {
                NeedsXml = true;
                testCollectionElements = new Dictionary<Guid, XElement>();
                errorsElement = new XElement("errors");
                options.AssemblyElement.Add(errorsElement);
            }
#endif
        }

        /// <inheritdoc/>
        public ExecutionSummary ExecutionSummary { get; } = new ExecutionSummary();

        /// <inheritdoc/>
        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

        bool NeedsFailSkips { get; }

        bool NeedsTestTiming { get; }

        bool NeedsXml { get; }

        /// <summary>
        /// Returns the current time in UTC. Overrideable for testing purposes.
        /// </summary>
        protected virtual DateTime UtcNow => DateTime.UtcNow;

#if !NET35
        void AddError(string type, string name, IFailureInformation failureInfo)
        {
            var errorElement = new XElement("error", new XAttribute("type", type), CreateFailureElement(failureInfo));
            if (name != null)
                errorElement.Add(new XAttribute("name", name));

            errorsElement.Add(errorElement);
        }

        static XElement CreateFailureElement(IFailureInformation failureInfo)
            => new XElement("failure",
                   new XAttribute("exception-type", failureInfo.ExceptionTypes[0]),
                   new XElement("message", new XCData(XmlEscape(ExceptionUtility.CombineMessages(failureInfo)))),
                   new XElement("stack-trace", new XCData(ExceptionUtility.CombineStackTraces(failureInfo) ?? string.Empty))
               );

        XElement CreateTestResultElement(ITestResultMessage testResult, string resultText)
        {
            ITest test = testResult.Test;
            ITestCase testCase = testResult.TestCase;
            ITestMethod testMethod = testCase.TestMethod;
            ITestClass testClass = testMethod.TestClass;

            var collectionElement = GetTestCollectionElement(testClass.TestCollection);
            var testResultElement =
                new XElement("test",
                    new XAttribute("name", XmlEscape(test.DisplayName)),
                    new XAttribute("type", testClass.Class.Name),
                    new XAttribute("method", testMethod.Method.Name),
                    new XAttribute("time", testResult.ExecutionTime.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("result", resultText)
                );

            var testOutput = testResult.Output;
            if (!string.IsNullOrWhiteSpace(testOutput))
                testResultElement.Add(new XElement("output", new XCData(testOutput)));

            ISourceInformation sourceInformation = testCase.SourceInformation;
            if (sourceInformation != null)
            {
                var fileName = sourceInformation.FileName;
                if (fileName != null)
                    testResultElement.Add(new XAttribute("source-file", fileName));

                var lineNumber = sourceInformation.LineNumber;
                if (lineNumber != null)
                    testResultElement.Add(new XAttribute("source-line", lineNumber.GetValueOrDefault()));
            }

            var traits = testCase.Traits;
            if (traits != null && traits.Count > 0)
            {
                var traitsElement = new XElement("traits");

                foreach (var keyValuePair in traits)
                    foreach (var val in keyValuePair.Value)
                        traitsElement.Add(
                            new XElement("trait",
                                new XAttribute("name", XmlEscape(keyValuePair.Key)),
                                new XAttribute("value", XmlEscape(val))
                            )
                        );

                testResultElement.Add(traitsElement);
            }

            collectionElement.Add(testResultElement);

            return testResultElement;
        }
#endif

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

            Finished.SafeDispose();
            stopEvent.SafeDispose();
        }

#if !NET35
        XElement GetTestCollectionElement(ITestCollection testCollection)
        {
            lock (testCollectionElements)
                return testCollectionElements.AddOrGet(testCollection.UniqueID, () => new XElement("collection"));
        }
#endif

        void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
        {
            Interlocked.Increment(ref errors);

#if !NET35
            if (NeedsXml)
                AddError("fatal", null, args.Message);
#endif
        }

        void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
        {
            Interlocked.Increment(ref errors);

#if !NET35
            if (NeedsXml)
                AddError("assembly-cleanup", args.Message.TestAssembly.Assembly.AssemblyPath, args.Message);
#endif
        }

        void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            ExecutionSummary.Total = args.Message.TestsRun;
            ExecutionSummary.Failed = args.Message.TestsFailed;
            ExecutionSummary.Skipped = args.Message.TestsSkipped;
            ExecutionSummary.Time = args.Message.ExecutionTime;
            ExecutionSummary.Errors = errors;

            options.FinishedCallback?.Invoke(ExecutionSummary);

#if !NET35
            if (NeedsXml)
            {
                options.AssemblyElement.Add(
                    new XAttribute("total", ExecutionSummary.Total),
                    new XAttribute("passed", ExecutionSummary.Total - ExecutionSummary.Failed - ExecutionSummary.Skipped),
                    new XAttribute("failed", ExecutionSummary.Failed),
                    new XAttribute("skipped", ExecutionSummary.Skipped),
                    new XAttribute("time", ExecutionSummary.Time.ToString("0.000", CultureInfo.InvariantCulture)),
                    new XAttribute("errors", ExecutionSummary.Errors)
                );

                foreach (var element in testCollectionElements.Values)
                    options.AssemblyElement.Add(element);
            }
#endif

            stopRequested = true;
        }

        void HandleTestAssemblyStarting(MessageHandlerArgs<ITestAssemblyStarting> args)
        {
            if (NeedsTestTiming)
            {
                stopEvent = new ManualResetEvent(initialState: false);
                lastTestActivity = UtcNow;
                XunitWorkerThread.QueueUserWorkItem(ThreadWorker);
            }

#if !NET35
            if (NeedsXml)
            {
                var assemblyStarting = args.Message;

                options.AssemblyElement.Add(
                    new XAttribute("name", assemblyStarting.TestAssembly.Assembly.AssemblyPath),
                    new XAttribute("environment", assemblyStarting.TestEnvironment),
                    new XAttribute("test-framework", assemblyStarting.TestFrameworkDisplayName),
                    new XAttribute("run-date", assemblyStarting.StartTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    new XAttribute("run-time", assemblyStarting.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
                );

                if (assemblyStarting.TestAssembly.ConfigFileName != null)
                    options.AssemblyElement.Add(new XAttribute("config-file", assemblyStarting.TestAssembly.ConfigFileName));
            }
#endif
        }

        void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
        {
            Interlocked.Increment(ref errors);

#if !NET35
            if (NeedsXml)
                AddError("test-case-cleanup", args.Message.TestCase.DisplayName, args.Message);
#endif
        }

        void HandleTestCaseFinished(MessageHandlerArgs<ITestCaseFinished> args)
        {
            if (NeedsTestTiming)
                lock (executingTestCases)
                {
                    executingTestCases.Remove(args.Message.TestCase);
                    lastTestActivity = UtcNow;
                }
        }

        void HandleTestCaseStarting(MessageHandlerArgs<ITestCaseStarting> args)
        {
            if (NeedsTestTiming)
                lock (executingTestCases)
                    executingTestCases.Add(args.Message.TestCase, UtcNow);
        }

        void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
        {
            Interlocked.Increment(ref errors);

#if !NET35
            if (NeedsXml)
                AddError("test-class-cleanup", args.Message.TestClass.Class.Name, args.Message);
#endif
        }

        void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
        {
            Interlocked.Increment(ref errors);

#if !NET35
            if (NeedsXml)
                AddError("test-cleanup", args.Message.Test.DisplayName, args.Message);
#endif
        }

        void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
        {
            Interlocked.Increment(ref errors);

#if !NET35
            if (NeedsXml)
                AddError("test-collection-cleanup", args.Message.TestCollection.DisplayName, args.Message);
#endif
        }

#if !NET35
        void HandleTestCollectionFinished(MessageHandlerArgs<ITestCollectionFinished> args)
        {
            if (NeedsXml)
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

        void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            if (NeedsXml)
            {
                var testFailed = args.Message;
                var testElement = CreateTestResultElement(testFailed, "Fail");
                testElement.Add(CreateFailureElement(testFailed));
            }
        }
#endif

        void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
        {
            Interlocked.Increment(ref errors);

#if !NET35
            if (NeedsXml)
                AddError("test-method-cleanup", args.Message.TestMethod.Method.Name, args.Message);
#endif
        }

#if !NET35
        void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            if (NeedsXml)
                CreateTestResultElement(args.Message, "Pass");
        }

        void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            if (NeedsXml)
            {
                var testSkipped = args.Message;
                var testElement = CreateTestResultElement(testSkipped, "Skip");
                testElement.Add(new XElement("reason", new XCData(XmlEscape(testSkipped.Reason))));
            }
        }
#endif

        /// <inheritdoc/>
        public bool OnMessageWithTypes(IMessageSinkMessage message, HashSet<string> messageTypes)
        {
            // Convert skips into failures
            if (NeedsFailSkips)
            {
                var testSkipped = message.Cast<ITestSkipped>(messageTypes);
                if (testSkipped != null)
                {
                    Interlocked.Increment(ref skipCount);
                    message = new TestFailed(testSkipped.Test, 0M, "", FailSkipsExceptionTypes, new[] { testSkipped.Reason }, FailSkipsStackTraces, FailSkipsParentIndices);
                    messageTypes = TestFailedMessageTypes;
                }
                else
                {
                    // Convert collection counts so skips are counted as failures in the results
                    var testCollectionFinished = message.Cast<ITestCollectionFinished>(messageTypes);
                    if (testCollectionFinished != null)
                        message = new TestCollectionFinished(testCollectionFinished.TestCases,
                                                             testCollectionFinished.TestCollection,
                                                             testCollectionFinished.ExecutionTime,
                                                             testCollectionFinished.TestsRun,
                                                             testCollectionFinished.TestsFailed + testCollectionFinished.TestsSkipped,
                                                             testsSkipped: 0);
                    else
                    {
                        var assemblyFinished = message.Cast<ITestAssemblyFinished>(messageTypes);
                        if (assemblyFinished != null)
                            message = new TestAssemblyFinished(assemblyFinished.TestCases,
                                                               assemblyFinished.TestAssembly,
                                                               assemblyFinished.ExecutionTime,
                                                               assemblyFinished.TestsRun,
                                                               assemblyFinished.TestsFailed + assemblyFinished.TestsSkipped,
                                                               testsSkipped: 0);
                    }
                }
            }

            // Record execution results
            var result =
                message.Dispatch<IErrorMessage>(messageTypes, HandleErrorMessage)
                && message.Dispatch<ITestAssemblyCleanupFailure>(messageTypes, HandleTestAssemblyCleanupFailure)
                && message.Dispatch<ITestAssemblyFinished>(messageTypes, HandleTestAssemblyFinished)
                && message.Dispatch<ITestAssemblyStarting>(messageTypes, HandleTestAssemblyStarting)
                && message.Dispatch<ITestCaseCleanupFailure>(messageTypes, HandleTestCaseCleanupFailure)
                && message.Dispatch<ITestClassCleanupFailure>(messageTypes, HandleTestClassCleanupFailure)
                && message.Dispatch<ITestCleanupFailure>(messageTypes, HandleTestCleanupFailure)
                && message.Dispatch<ITestCollectionCleanupFailure>(messageTypes, HandleTestCollectionCleanupFailure)
                && message.Dispatch<ITestMethodCleanupFailure>(messageTypes, HandleTestMethodCleanupFailure);

            // Check for long running tests
            if (NeedsTestTiming)
                result =
                    message.Dispatch<ITestCaseFinished>(messageTypes, HandleTestCaseFinished)
                    && message.Dispatch<ITestCaseStarting>(messageTypes, HandleTestCaseStarting)
                    && result;

#if !NET35
            // Record XML
            if (NeedsXml)
                result =
                    message.Dispatch<ITestCollectionFinished>(messageTypes, HandleTestCollectionFinished)
                    && message.Dispatch<ITestFailed>(messageTypes, HandleTestFailed)
                    && message.Dispatch<ITestPassed>(messageTypes, HandleTestPassed)
                    && message.Dispatch<ITestSkipped>(messageTypes, HandleTestSkipped)
                    && result;
#endif

            // Dispatch to the reporter handler
            result =
                innerSink.OnMessageWithTypes(message, messageTypes)
                && result
                && (options.CancelThunk == null || !options.CancelThunk());

            // Don't request stop until after the inner handler has had a chance to process the message
            // per https://github.com/xunit/visualstudio.xunit/issues/396
            if (stopRequested)
            {
                Finished.Set();
                stopEvent?.Set();
            }

            return result;
        }

        void SendLongRunningMessage()
        {
            Dictionary<ITestCase, TimeSpan> longRunningTestCases;
            lock (executingTestCases)
            {
                var now = UtcNow;
                longRunningTestCases = executingTestCases.Where(kvp => (now - kvp.Value) >= options.LongRunningTestTime)
                                                         .ToDictionary(k => k.Key, v => now - v.Value);
            }

            if (longRunningTestCases.Count > 0)
            {
                if (options.LongRunningTestCallback != null)
                    options.LongRunningTestCallback(new LongRunningTestsSummary(options.LongRunningTestTime, longRunningTestCases));

                if (options.DiagnosticMessageSink != null)
                    options.DiagnosticMessageSink.OnMessage(
                        new DiagnosticMessage(
                            string.Join(
                                Environment.NewLine,
                                longRunningTestCases.Select(pair => string.Format(CultureInfo.CurrentCulture, @"[Long Running Test] '{0}', Elapsed: {1:hh\:mm\:ss}", pair.Key.DisplayName, pair.Value)).ToArray()
                            )
                        )
                    );
            }
        }

        void ThreadWorker()
        {
            // Fire the loop approximately every 1/10th of our delay time, but no more frequently than every
            // second (so we don't over-fire the timer). This should give us reasonable precision for the
            // requested delay time, without going crazy to check for long-running tests.

            var delayTime = (int)Math.Max(1000, options.LongRunningTestTime.TotalMilliseconds / 10);

            while (true)
            {
                if (WaitForStopEvent(delayTime))
                    return;

                var now = UtcNow;
                if (now - lastTestActivity >= options.LongRunningTestTime)
                {
                    SendLongRunningMessage();
                    lastTestActivity = now;
                }
            }
        }

        /// <summary>
        /// Performs a Task-safe delay. Overrideable for testing purposes.
        /// </summary>
        protected virtual bool WaitForStopEvent(int millionsecondsDelay)
            => stopEvent?.WaitOne(millionsecondsDelay) ?? true;

#if !NET35
        static string XmlEscape(string value)
        {
            if (value == null)
                return string.Empty;

            value = value.Replace("\\", "\\\\")
                         .Replace("\r", "\\r")
                         .Replace("\n", "\\n")
                         .Replace("\t", "\\t")
                         .Replace("\0", "\\0")
                         .Replace("\a", "\\a")
                         .Replace("\b", "\\b")
                         .Replace("\v", "\\v")
                         .Replace("\"", "\\\"")
                         .Replace("\f", "\\f");

            var escapedValue = new StringBuilder(value.Length);
            for (var idx = 0; idx < value.Length; ++idx)
            {
                char ch = value[idx];
                if (ch < 32)
                    escapedValue.Append(string.Format(CultureInfo.InvariantCulture, @"\x{0:x2}", +ch));
                else if (char.IsSurrogatePair(value, idx)) // Takes care of the case when idx + 1 == value.Length
                {
                    escapedValue.Append(ch); // Append valid surrogate chars like normal
                    escapedValue.Append(value[++idx]);
                }
                // Check for invalid chars and append them like \x----
                else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
                    escapedValue.Append(string.Format(CultureInfo.InvariantCulture, @"\x{0:x4}", +ch));
                else
                    escapedValue.Append(ch);
            }

            return escapedValue.ToString();
        }
#endif
    }
}
