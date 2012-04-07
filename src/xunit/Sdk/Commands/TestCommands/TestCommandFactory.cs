using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// Factory for creating <see cref="ITestCommand"/> objects.
    /// </summary>
    public static class TestCommandFactory
    {
        /// <summary>
        /// Make instances of <see cref="ITestCommand"/> objects for the given class and method.
        /// </summary>
        /// <param name="classCommand">The class command</param>
        /// <param name="method">The method under test</param>
        /// <returns>The set of <see cref="ITestCommand"/> objects</returns>
        public static IEnumerable<ITestCommand> Make(ITestClassCommand classCommand,
                                                     IMethodInfo method)
        {
            foreach (var testCommand in classCommand.EnumerateTestCommands(method))
            {
                ITestCommand wrappedCommand = testCommand;

                // Timeout (if they have one) -> Capture -> Timed -> Lifetime (if we need an instance) -> BeforeAfter

                wrappedCommand = new BeforeAfterCommand(wrappedCommand, method.MethodInfo);

                if (testCommand.ShouldCreateInstance)
                    wrappedCommand = new LifetimeCommand(wrappedCommand, method);

                wrappedCommand = new ExceptionAndOutputCaptureCommand(wrappedCommand, method);
                wrappedCommand = new TimedCommand(wrappedCommand);

                if (wrappedCommand.Timeout > 0)
                    wrappedCommand = new TimeoutCommand(wrappedCommand, wrappedCommand.Timeout, method);

                yield return wrappedCommand;
            }
        }
    }
}