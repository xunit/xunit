using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A simple implementation of <see cref="IXunitTestCase"/> that can be used to report an error
    /// rather than running a test.
    /// </summary>
    public class ExecutionErrorTestCase : XunitTestCase
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public ExecutionErrorTestCase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionErrorTestCase"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="testMethod">The test method.</param>
        /// <param name="errorMessage">The error message to report for the test.</param>
        public ExecutionErrorTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod, string errorMessage)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod)
        {
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets the error message that will be display when the test is run.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <inheritdoc/>
        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                  IMessageBus messageBus,
                                                  object[] constructorArguments,
                                                  ExceptionAggregator aggregator,
                                                  CancellationTokenSource cancellationTokenSource)
        {
            return new ExecutionErrorTestCaseRunner(this, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }

        /// <inheritdoc/>
        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("ErrorMessage", ErrorMessage);
        }

        /// <inheritdoc/>
        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            ErrorMessage = data.GetValue<string>("ErrorMessage");
        }
    }
}