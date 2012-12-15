using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TestDriven.Framework;
using Xunit.Abstractions;
using ITdNetTestRunner = TestDriven.Framework.ITestRunner;

namespace Xunit.Runner.TdNet
{
    public class TdNetRunner : ITdNetTestRunner
    {
        private static IEnumerable<IMethodTestCase> Discover(IXunitController controller)
        {
            return Discover(sink => controller.Find(false, sink));
        }

        private static IEnumerable<IMethodTestCase> Discover(IXunitController controller, Type type)
        {
            return Discover(sink => controller.Find(Reflector2.Wrap(type), false, sink));
        }

        protected virtual IXunitController CreateController(string assemblyFileName)
        {
            return new XunitFrontController(assemblyFileName, null, false);
        }

        private static IEnumerable<IMethodTestCase> Discover(Action<IMessageSink> discoveryAction)
        {
            var collector = new MessageCollector<IDiscoveryCompleteMessage>();
            discoveryAction(collector);
            collector.Finished.WaitOne();

            var testCases = collector.Messages
                                     .OfType<ITestCaseDiscoveryMessage>()
                                     .Select(msg => (IMethodTestCase)msg.TestCase);
            return testCases;
        }

        private static TestRunState Run(IXunitController controller, ITestListener testListener, Assembly assembly, IEnumerable<ITestCase> testCases)
        {
            var visitor = new ResultVisitor(testListener, assembly);
            controller.Run(testCases.ToList(), visitor);
            visitor.Finished.WaitOne();

            return visitor.TestRunState;
        }

        public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
        {
            string assemblyFileName = new Uri(assembly.CodeBase).LocalPath;

            using (var controller = CreateController(assemblyFileName))
                return Run(controller, testListener, assembly, Discover(controller));
        }

        private static TestRunState RunClass(IXunitController controller, ITestListener testListener, Assembly assembly, Type type, TestRunState initialRunState)
        {
            return Run(controller, testListener, assembly, Discover(controller, type));
        }

        public TestRunState RunClassWithInnerTypes(ITestListener testListener, Assembly assembly, Type type)
        {
            string assemblyFileName = new Uri(assembly.CodeBase).LocalPath;
            TestRunState state = TestRunState.NoTests;

            using (var controller = CreateController(assemblyFileName))
            {
                state = RunClass(controller, testListener, assembly, type, state);

                foreach (MemberInfo memberInfo in type.GetMembers())
                {
                    Type childType = memberInfo as Type;

                    if (childType != null)
                        state = RunClass(controller, testListener, assembly, childType, state);
                }
            }

            return state;
        }

        public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.TypeInfo)
                return RunClassWithInnerTypes(testListener, assembly, (Type)member);
            if (member.MemberType == MemberTypes.Method)
                return RunMethod(testListener, assembly, (MethodInfo)member);

            return TestRunState.NoTests;
        }

        private TestRunState RunMethod(ITestListener testListener, Assembly assembly, MethodInfo method)
        {
            string assemblyFileName = new Uri(assembly.CodeBase).LocalPath;

            using (var controller = CreateController(assemblyFileName))
            {
                var testCases = Discover(controller).Where(tc => tc.ToMethodInfo() == method);

                return Run(controller, testListener, assembly, testCases);
            }
        }

        public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
        {
            string assemblyFileName = new Uri(assembly.CodeBase).LocalPath;

            using (var controller = CreateController(assemblyFileName))
            {
                var testCases = Discover(controller).Where(tc => ns == null || tc.ToType().Namespace == ns);

                return Run(controller, testListener, assembly, testCases);
            }
        }
    }
}