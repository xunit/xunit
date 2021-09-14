using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class ExceptionAndOutputCaptureCommandTests
    {
        public class ExceptionTests
        {
            [Fact]
            public void ShouldWrapExceptionDetailsWhenExceptionIsThrown()
            {
                ExceptionThrowingCommand innerCmd = new ExceptionThrowingCommand();
                MethodInfo method = typeof(ExceptionThrowingCommand).GetMethod("Execute");
                var command = new ExceptionAndOutputCaptureCommand(innerCmd, Reflector.Wrap(method));

                MethodResult result = command.Execute(null);

                FailedResult failed = Assert.IsType<FailedResult>(result);
                Assert.Equal(method.Name, failed.MethodName);
                Assert.Equal(method.DeclaringType.FullName, failed.TypeName);
                Assert.Equal(typeof(TargetInvocationException).FullName, failed.ExceptionType);
                Assert.Contains("ExceptionThrowingCommand.Execute", failed.StackTrace);
            }

            class ExceptionThrowingCommand : ITestCommand
            {
                public string DisplayName
                {
                    get { return null; }
                }

                public bool ShouldCreateInstance
                {
                    get { return true; }
                }

                public int Timeout
                {
                    get { return 0; }
                }

                public MethodResult Execute(object testClass)
                {
                    throw new TargetInvocationException(new Exception());
                }

                public XmlNode ToStartXml()
                {
                    return null;
                }
            }
        }

        public class OutputCaptureTests : IDisposable
        {
            TraceListener[] oldListeners;

            public OutputCaptureTests()
            {
                oldListeners = Trace.Listeners.OfType<TraceListener>().ToArray();
                Trace.Listeners.Clear();
            }

            public void Dispose()
            {
                Trace.Listeners.Clear();
                Trace.Listeners.AddRange(oldListeners);
            }

            [Fact]
            public void ConsoleOutAndErrorAreReplacedDuringTestExecution()
            {
                TextWriter originalOut = Console.Out;
                TextWriter originalError = Console.Error;

                try
                {
                    TextWriter newOut = new StringWriter();
                    TextWriter newError = new StringWriter();
                    Console.SetOut(newOut);
                    Console.SetError(newError);
                    StubCommand cmd = new StubCommand();
                    var outputCmd = new ExceptionAndOutputCaptureCommand(cmd, null);

                    outputCmd.Execute(null);

                    Assert.Empty(newOut.ToString());
                    Assert.Empty(newError.ToString());
                }
                finally
                {
                    Console.SetOut(originalOut);
                    Console.SetError(originalError);
                }
            }

            [Fact]
            public void ConsoleOutAndErrorAndTraceIsCapturedAndPlacedInMethodResult()
            {
                string expected = "Standard Output" + Environment.NewLine +
                                  "Standard Error" + Environment.NewLine +
                                  "Trace" + Environment.NewLine;

                StubCommand cmd = new StubCommand();
                var outputCmd = new ExceptionAndOutputCaptureCommand(cmd, null);

                MethodResult result = outputCmd.Execute(null);

                Assert.Equal(expected, result.Output);
            }

            class StubCommand : ITestCommand
            {
                public object TestClass;

                public string DisplayName
                {
                    get { return null; }
                }

                public bool ShouldCreateInstance
                {
                    get { return true; }
                }

                public int Timeout
                {
                    get { return 0; }
                }

                public MethodResult Execute(object testClass)
                {
                    TestClass = testClass;
                    Console.WriteLine("Standard Output");
                    Console.Error.WriteLine("Standard Error");
                    Trace.WriteLine("Trace");
                    MethodInfo method = typeof(StubCommand).GetMethod("Execute");
                    return new PassedResult(Reflector.Wrap(method), null);
                }

                public XmlNode ToStartXml()
                {
                    return null;
                }
            }
        }
    }
}
