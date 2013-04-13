using System;
using System.Linq;
using System.Reflection;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class TdNetRunner : ITestRunner
    {
        public virtual TdNetRunnerHelper CreateHelper(ITestListener testListener, Assembly assembly)
        {
            return new TdNetRunnerHelper(assembly, testListener);
        }

        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            using (var helper = CreateHelper(testListener, assembly))
                return helper.Run(helper.Discover());
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            using (var helper = CreateHelper(testListener, assembly))
            {
                if (member is Type)
                    return helper.RunClass((Type)member);
                if (member is MethodInfo)
                    return helper.RunMethod((MethodInfo)member);

                return TestRunState.NoTests;
            }
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            using (var helper = CreateHelper(testListener, assembly))
            {
                var testCases = helper.Discover().Where(tc => ns == null || tc.GetClass().Namespace == ns);
                return helper.Run(testCases);
            }
        }
    }
}