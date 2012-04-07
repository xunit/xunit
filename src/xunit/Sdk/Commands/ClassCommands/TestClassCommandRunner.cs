using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Runner that executes an <see cref="ITestClassCommand"/> synchronously.
    /// </summary>
    public static class TestClassCommandRunner
    {
        /// <summary>
        /// Execute the <see cref="ITestClassCommand"/>.
        /// </summary>
        /// <param name="testClassCommand">The test class command to execute</param>
        /// <param name="methods">The methods to execute; if null or empty, all methods will be executed</param>
        /// <param name="startCallback">The start run callback</param>
        /// <param name="resultCallback">The end run result callback</param>
        /// <returns>A <see cref="ClassResult"/> with the results of the test run</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static ClassResult Execute(ITestClassCommand testClassCommand,
                                          List<IMethodInfo> methods,
                                          Predicate<ITestCommand> startCallback,
                                          Predicate<ITestResult> resultCallback)
        {
            if (methods == null)
                methods = new List<IMethodInfo>();

            if (methods.Count == 0)
                foreach (IMethodInfo method in testClassCommand.EnumerateTestMethods())
                    methods.Add(method);

            ClassResult classResult = new ClassResult(testClassCommand.TypeUnderTest.Type);
            Exception fixtureException = testClassCommand.ClassStart();
            bool @continue = true;

            if (fixtureException == null)
            {
                List<IMethodInfo> runList = new List<IMethodInfo>();
                foreach (IMethodInfo method in testClassCommand.EnumerateTestMethods())
                    runList.Add(method);

                while (@continue && runList.Count > 0)
                {
                    int idx = testClassCommand.ChooseNextTest(runList.AsReadOnly());
                    IMethodInfo method = runList[idx];

                    if (methods.Contains(method))
                        foreach (ITestCommand command in TestCommandFactory.Make(testClassCommand, method))
                        {
                            if (startCallback != null)
                                @continue = startCallback(command);

                            if (!@continue)
                                break;

                            MethodResult methodResult = command.Execute(testClassCommand.ObjectUnderTest);
                            classResult.Add(methodResult);

                            if (resultCallback != null)
                                @continue = resultCallback(methodResult);

                            if (!@continue)
                                break;
                        }

                    runList.RemoveAt(idx);
                }
            }

            classResult.SetException(testClassCommand.ClassFinish() ?? fixtureException);

            if (resultCallback != null)
                resultCallback(classResult);

            return classResult;
        }
    }
}