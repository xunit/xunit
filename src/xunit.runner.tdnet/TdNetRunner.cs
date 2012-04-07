using System;
using System.Reflection;
using TestDriven.Framework;
using ITdNetTestRunner = TestDriven.Framework.ITestRunner;

namespace Xunit.Runner.TdNet
{
    public class TdNetRunner : ITdNetTestRunner
    {
        public static TestRunState RunAssembly(TestRunner runner)
        {
            TestRunnerResult result = runner.RunAssembly();
            return TestResultMapper.Map(result);
        }

        public static TestRunState RunClass(TestRunner runner, Type type)
        {
            var result = runner.RunClass(type.FullName);
            return TestResultMapper.Map(result);
        }

        public static TestRunState RunClassWithInnerTypes(TestRunner runner, Type type)
        {
            TestRunState result = RunClass(runner, type);

            foreach (MemberInfo memberInfo in type.GetMembers())
            {
                Type childType = memberInfo as Type;

                if (childType != null)
                    result = TestResultMapper.Merge(result, RunClassWithInnerTypes(runner, childType));
            }

            return result;
        }

        public static TestRunState RunMethod(TestRunner runner, MethodInfo method)
        {
            TestRunnerResult result = runner.RunTest(method.ReflectedType.FullName, method.Name);
            return TestResultMapper.Map(result);
        }

        // ITestRunner implementation

        TestRunState ITdNetTestRunner.RunAssembly(ITestListener listener, Assembly assembly)
        {
            string assemblyFilename = new Uri(assembly.CodeBase).LocalPath;

            try
            {
                using (ExecutorWrapper wrapper = new ExecutorWrapper(assemblyFilename, null, false))
                {
                    TdNetLogger logger = new TdNetLogger(listener, assembly);
                    TestRunner runner = new TestRunner(wrapper, logger);
                    return RunAssembly(runner);
                }
            }
            catch (ArgumentException)
            {
                return TestRunState.NoTests;
            }
        }

        TestRunState ITdNetTestRunner.RunMember(ITestListener listener, Assembly assembly, MemberInfo member)
        {
            try
            {
                using (ExecutorWrapper wrapper = new ExecutorWrapper(new Uri(assembly.CodeBase).LocalPath, null, false))
                {
                    TdNetLogger logger = new TdNetLogger(listener, assembly);
                    TestRunner runner = new TestRunner(wrapper, logger);

                    MethodInfo method = member as MethodInfo;
                    if (method != null)
                        return RunMethod(runner, method);

                    Type type = member as Type;
                    if (type != null)
                        return RunClassWithInnerTypes(runner, type);

                    return TestRunState.NoTests;
                }
            }
            catch (ArgumentException)
            {
                return TestRunState.NoTests;
            }
        }

        TestRunState ITdNetTestRunner.RunNamespace(ITestListener listener, Assembly assembly, string ns)
        {
            try
            {
                using (ExecutorWrapper wrapper = new ExecutorWrapper(new Uri(assembly.CodeBase).LocalPath, null, false))
                {
                    TdNetLogger logger = new TdNetLogger(listener, assembly);
                    TestRunner runner = new TestRunner(wrapper, logger);
                    TestRunState runState = TestRunState.NoTests;

                    foreach (Type type in assembly.GetExportedTypes())
                        if (ns == null || type.Namespace == ns)
                            runState = TestResultMapper.Merge(runState, RunClass(runner, type));

                    return runState;
                }
            }
            catch (ArgumentException)
            {
                return TestRunState.NoTests;
            }
        }
    }
}