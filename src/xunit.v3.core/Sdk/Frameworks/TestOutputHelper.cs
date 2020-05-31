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
                lock (lockObject)
                {
                    GuardInitialized();

                    return buffer.ToString();
                }
            }
        }

        /// <summary>
        /// Initialize the test output helper with information about a test.
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
                throw new InvalidOperationException("There is no currently active test.");
        }

        void QueueTestOutput(string output)
        {
            output = EscapeInvalidHexChars(output);

            lock (lockObject)
            {
                GuardInitialized();

                buffer.Append(output);
            }

            messageBus.QueueMessage(new TestOutput(test, output));
        }

        private static string EscapeInvalidHexChars(string s)
        {
            var builder = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == '\0')
                    builder.Append("\\0");
                else if (ch < 32 && !char.IsWhiteSpace(ch)) // C0 control char
                    builder.AppendFormat(@"\x{0}", (+ch).ToString("x2"));
                else if (char.IsSurrogatePair(s, i))
                {
                    // For valid surrogates, append like normal
                    builder.Append(ch);
                    builder.Append(s[++i]);
                }
                // Check for stray surrogates/other invalid chars
                else if (char.IsSurrogate(ch) || ch == '\uFFFE' || ch == '\uFFFF')
                {
                    builder.AppendFormat(@"\x{0}", (+ch).ToString("x4"));
                }
                else
                    builder.Append(ch); // Append the char like normal
            }
            return builder.ToString();
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

            QueueTestOutput(message + Environment.NewLine);
        }

        /// <inheritdoc/>
        public void WriteLine(string format, params object[] args)
        {
            Guard.ArgumentNotNull("format", format);
            Guard.ArgumentNotNull("args", args);

            QueueTestOutput(string.Format(format, args) + Environment.NewLine);
        }
    }
}
