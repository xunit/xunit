using System;
using System.Collections.Generic;
using System.Reflection;
using Moq;
using TestDriven.Framework;
using Xunit.Abstractions;
using Xunit.Runner.TdNet;

public class MockTdNetRunnerHelper : Mock<TdNetRunnerHelper>
{
    public List<string> Operations = new List<string>();
    public List<ITestCase> TestsRun = new List<ITestCase>();
    public List<IMethodTestCase> TestsToDiscover = new List<IMethodTestCase> { new Mock<IMethodTestCase>().Object };

    public MockTdNetRunnerHelper()
    {
        this.Setup(h => h.Discover())
            .Callback(() => Operations.Add("Discovery()"))
            .Returns(TestsToDiscover);

        this.Setup(h => h.Run(It.IsAny<IEnumerable<ITestCase>>(), It.IsAny<TestRunState>()))
            .Callback<IEnumerable<ITestCase>, TestRunState>((tc, state) =>
            {
                Operations.Add(String.Format("Run(initialRunState: {0})", state));
                TestsRun.AddRange(tc);
            });

        this.Setup(h => h.RunClass(It.IsAny<Type>(), It.IsAny<TestRunState>()))
            .Callback<Type, TestRunState>((type, state) => Operations.Add(String.Format("RunClass(type: {0}, initialRunState: {1})", type.FullName, state)));

        this.Setup(h => h.RunMethod(It.IsAny<MethodInfo>(), It.IsAny<TestRunState>()))
            .Callback<MethodInfo, TestRunState>((method, state) => Operations.Add(String.Format("RunMethod(method: {0}.{1}, initialRunState: {2})", method.DeclaringType.FullName, method.Name, state)));
    }
}