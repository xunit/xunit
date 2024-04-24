#if NETFRAMEWORK || NETCOREAPP

using System;
using System.ComponentModel;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IRunnerLogger"/> which logs messages to <see cref="Console"/>.
    /// </summary>
    public class ConsoleRunnerLogger : IRunnerLogger
    {
        readonly object lockObject;
        readonly bool useColors;

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Please use the new overload with the useAnsiColor flag")]
        public ConsoleRunnerLogger(bool useColors) : this(useColors, useAnsiColor: false, new object()) { }

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Please use the new overload with the useAnsiColor flag")]
        public ConsoleRunnerLogger(bool useColors, object lockObject) : this(useColors, useAnsiColor: false, lockObject) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleRunnerLogger"/> class.
        /// </summary>
        /// <param name="useColors">A flag to indicate whether colors should be used when
        /// logging messages.</param>
        /// <param name="useAnsiColor">A flag to indicate whether ANSI colors should be
        /// forced on Windows.</param>
        public ConsoleRunnerLogger(bool useColors, bool useAnsiColor) : this(useColors, useAnsiColor, new object()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleRunnerLogger"/> class.
        /// </summary>
        /// <param name="useColors">A flag to indicate whether colors should be used when
        /// logging messages.</param>
        /// <param name="useAnsiColor">A flag to indicate whether ANSI colors should be
        /// forced on Windows.</param>
        /// <param name="lockObject">The lock object used to prevent console clashes.</param>
        public ConsoleRunnerLogger(bool useColors, bool useAnsiColor, object lockObject)
        {
            this.useColors = useColors;
            this.lockObject = lockObject;

            if (useAnsiColor)
                ConsoleHelper.UseAnsiColor();
        }

        /// <inheritdoc/>
        public object LockObject => lockObject;

        /// <inheritdoc/>
        public void LogError(StackFrameInfo stackFrame, string message)
        {
            lock (LockObject)
                using (SetColor(ConsoleColor.Red))
                    Console.WriteLine(message);
        }

        /// <inheritdoc/>
        public void LogImportantMessage(StackFrameInfo stackFrame, string message)
        {
            lock (LockObject)
                using (SetColor(ConsoleColor.Gray))
                    Console.WriteLine(message);
        }

        /// <inheritdoc/>
        public void LogMessage(StackFrameInfo stackFrame, string message)
        {
            lock (LockObject)
                using (SetColor(ConsoleColor.DarkGray))
                    Console.WriteLine(message);
        }

        /// <inheritdoc/>
        public void LogRaw(string message)
        {
            lock (LockObject)
                Console.WriteLine(message);
        }

        /// <inheritdoc/>
        public void LogWarning(StackFrameInfo stackFrame, string message)
        {
            lock (LockObject)
                using (SetColor(ConsoleColor.Yellow))
                    Console.WriteLine(message);
        }

        IDisposable SetColor(ConsoleColor color)
            => useColors ? new ColorRestorer(color) : null;

        class ColorRestorer : IDisposable
        {
            public ColorRestorer(ConsoleColor color)
                => ConsoleHelper.SetForegroundColor(color);

            public void Dispose()
                => ConsoleHelper.ResetColor();
        }
    }
}

#endif
