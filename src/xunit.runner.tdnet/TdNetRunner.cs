using System;
using System.Linq;
using System.Reflection;
using TestDriven.Framework;

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
                return helper.Run();
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            using (var helper = CreateHelper(testListener, assembly))
            {
                var type = member as Type;
                if (type != null)
                    return helper.RunClass(type);

                var method = member as MethodInfo;
                if (method != null)
                    return helper.RunMethod(method);

                return TestRunState.NoTests;
            }
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            using (var helper = CreateHelper(testListener, assembly))
            {
                var testCases = helper.Discover().Where(tc => ns == null || tc.GetClass().Namespace == ns).ToList();
                return helper.Run(testCases);
            }
        }
    }
}
