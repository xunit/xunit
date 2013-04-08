using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents an extension to <see cref="ITestCase"/> for xUnit.net tests. This includes
    /// the ability to run the tests, as well as support for parameterization.
    /// </summary>
    public interface IXunitTestCase : ITestCase
    {
        /// <summary>
        /// The arguments that will be passed to the test method.
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// Executes the test case, returning 0 or more result messages through the message sink.
        /// </summary>
        /// <param name="messageSink">The message sink to report results to.</param>
        /// <returns>Returns <c>true</c> if the tests should continue to run; <c>false</c> otherwise.</returns>
        // REVIEW: Returning bool from this method may be bad design, as it dictates a synchronous execution pattern.
        bool Run(IMessageSink messageSink);
    }
}