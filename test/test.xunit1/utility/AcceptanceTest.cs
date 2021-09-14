using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace TestUtility
{
    public class AcceptanceTest
    {
        protected IEnumerable<MethodResult> RunClass(Type typeUnderTest)
        {
            ITestClassCommand testClassCommand = new TestClassCommand(typeUnderTest);

            ClassResult classResult = TestClassCommandRunner.Execute(testClassCommand, testClassCommand.EnumerateTestMethods().ToList(),
                                                                     startCallback: null, resultCallback: null);

            return classResult.Results.OfType<MethodResult>();
        }
    }
}
