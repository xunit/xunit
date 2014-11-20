using System;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestOutputHelper"/>.
    /// </summary>
    public class TestOutputHelper : ITestOutputHelper
    {
        StringBuilder buffer;
        IMessageBus messageBus;
        ITest test;

        readonly object lockObject = new object();

        /// <summary>
        /// Gets the output provided by the test.
        /// </summary>
        public string Output
        {
            get
            {
                GuardInitialized();
                return buffer.ToString();
            }
        }

        /// <summary>
        /// Initialize the test output helper with information about a test case.
        /// </summary>
        public void Initialize(IMessageBus messageBus, ITest test)
        {
            Guard.ArgumentNotNull("messageBus", messageBus);
            Guard.ArgumentNotNull("test", test);

            this.messageBus = messageBus;
            this.test = test;

            buffer = new StringBuilder();
        }

        void GuardInitialized()
        {
            if (buffer == null)
                throw new InvalidOperationException("There is no currently active test case.");
        }

        void QueueTestCaseOutput(string output)
        {
            lock (lockObject)
            {
                GuardInitialized();

                buffer.Append(output);
            }

            messageBus.QueueMessage(new TestOutput(test, output));
        }

        /// <summary>
        /// Resets the test output helper to its uninitialized state.
        /// </summary>
        public void Uninitialize()
        {
            buffer = null;
            messageBus = null;
            test = null;
        }

        /// <inheritdoc/>
        public void WriteLine(string message)
        {
            Guard.ArgumentNotNull("message", message);

            QueueTestCaseOutput(message + Environment.NewLine);
        }

        /// <inheritdoc/>
        public void WriteLine(string format, params object[] args)
        {
            Guard.ArgumentNotNull("format", format);
            Guard.ArgumentNotNull("args", args);

            QueueTestCaseOutput(String.Format(format, args) + Environment.NewLine);
        }
    }
}
