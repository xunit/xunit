using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using TestDriven.Framework;

namespace Xunit.Runner.TdNet
{
	public class TdNetRunner : ITestRunner
	{
		public virtual TdNetRunnerHelper CreateHelper(ITestListener testListener, Assembly assembly) =>
			new TdNetRunnerHelper(assembly, testListener);

		public TestRunState RunAssembly(ITestListener testListener, Assembly assembly)
		{
			var helper = CreateHelper(testListener, assembly);
			try
			{
				return helper.Run();
			}
			finally
			{
				ThreadPool.QueueUserWorkItem(_ => helper.DisposeAsync());
			}
		}

		public TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member)
		{
			var helper = CreateHelper(testListener, assembly);
			try
			{
				var type = member as Type;
				if (type != null)
					return helper.RunClass(type);

				var method = member as MethodInfo;
				if (method != null)
					return helper.RunMethod(method);

				return TestRunState.NoTests;
			}
			finally
			{
				ThreadPool.QueueUserWorkItem(_ => helper.DisposeAsync());
			}
		}

		public TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns)
		{
			var helper = CreateHelper(testListener, assembly);
			try
			{
				var testCases = helper.Discover().Where(tc => ns == null || tc.TestNamespace == ns).ToList();
				return helper.Run(testCases);
			}
			finally
			{
				ThreadPool.QueueUserWorkItem(_ => helper.DisposeAsync());
			}
		}
	}
}
