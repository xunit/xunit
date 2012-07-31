using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents an implementation of <see cref="ITestCommand"/> to be used with
    /// tests which are decorated with the <see cref="FactAttribute"/>.
    /// </summary>
    public class FactCommand : TestCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FactCommand"/> class.
        /// </summary>
        /// <param name="method">The test method.</param>
        public FactCommand(IMethodInfo method)
            : base(method, MethodUtility.GetDisplayName(method), MethodUtility.GetTimeoutParameter(method)) { }

        /// <inheritdoc/>
        public override MethodResult Execute(object testClass)
        {
            try
            {
                testMethod.Invoke(testClass, null);
            }
            catch (ParameterCountMismatchException)
            {
                throw new InvalidOperationException("Fact method " + TypeName + "." + MethodName + " cannot have parameters");
            }

            return new PassedResult(testMethod, DisplayName);
        }
    }
}
