using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Xunit.Sdk
{
    /// <summary>
    /// This command sets up the necessary trace listeners and standard
    /// output/error listeners to capture Assert/Debug.Trace failures,
    /// output to stdout/stderr, and Assert/Debug.Write text. It also
    /// captures any exceptions that are thrown and packages them as
    /// FailedResults, including the possibility that the configuration
    /// file is messed up (which is exposed when we attempt to manipulate
    /// the trace listener list).
    /// </summary>
    public class ExceptionAndOutputCaptureCommand : DelegatingTestCommand
    {
        IMethodInfo method;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionAndOutputCaptureCommand"/>
        /// class.
        /// </summary>
        /// <param name="innerCommand">The command that will be wrapped.</param>
        /// <param name="method">The test method.</param>
        public ExceptionAndOutputCaptureCommand(ITestCommand innerCommand, IMethodInfo method)
            : base(innerCommand)
        {
            this.method = method;
        }

        /// <inheritdoc/>
        public override MethodResult Execute(object testClass)
        {
            MethodResult result = null;
            List<TraceListener> oldListeners = new List<TraceListener>();
            TextWriter oldOut = Console.Out;
            TextWriter oldError = Console.Error;

            using (StringWriter outputWriter = new StringWriter())
            {
                try
                {
                    foreach (TraceListener oldListener in Trace.Listeners)
                        oldListeners.Add(oldListener);

                    Trace.Listeners.Clear();
                    Trace.Listeners.Add(new AssertTraceListener());
                    Trace.Listeners.Add(new TextWriterTraceListener(outputWriter));

                    Console.SetOut(outputWriter);
                    Console.SetError(outputWriter);

                    result = InnerCommand.Execute(testClass);
                }
                catch (Exception ex)
                {
                    result = new FailedResult(method, ex, DisplayName);
                }
                finally
                {
                    Console.SetOut(oldOut);
                    Console.SetError(oldError);

                    try
                    {
                        Trace.Listeners.Clear();
                        Trace.Listeners.AddRange(oldListeners.ToArray());
                    }
                    catch (Exception) { }
                }

                result.Output = outputWriter.ToString();
                return result;
            }
        }

        class AssertTraceListener : TraceListener
        {
            public override void Fail(string message,
                                      string detailMessage)
            {
                throw new TraceAssertException(message, detailMessage);
            }

            public override void Write(string message) { }

            public override void WriteLine(string message) { }
        }
    }
}
