namespace Xunit.Sdk
{
    /// <summary>
    /// A command wrapper which times the running of a command.
    /// </summary>
    public class TimedCommand : DelegatingTestCommand
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TimedCommand"/> class.
        /// </summary>
        /// <param name="innerCommand">The command that will be timed.</param>
        public TimedCommand(ITestCommand innerCommand)
            : base(innerCommand) { }

        /// <summary>
        /// Executes the inner test method, gathering the amount of time it takes to run.
        /// </summary>
        /// <returns>Returns information about the test run</returns>
        public override MethodResult Execute(object testClass)
        {
            TestTimer timer = new TestTimer();

            timer.Start();
            MethodResult methodResult = InnerCommand.Execute(testClass);
            timer.Stop();

            methodResult.ExecutionTime = timer.ElapsedMilliseconds / 1000.00;

            return methodResult;
        }
    }
}