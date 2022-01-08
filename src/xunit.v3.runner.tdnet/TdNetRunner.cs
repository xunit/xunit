using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TestDriven.Framework;

namespace Xunit.Runner.TdNet;

public class TdNetRunner : ITestRunner
{
	public virtual TdNetRunnerHelper CreateHelper(
		ITestListener testListener,
		Assembly assembly) =>
			new(assembly, testListener);

	TestRunState Run(
		ITestListener testListener,
		Assembly assembly,
		Func<TdNetRunnerHelper, TestRunState> action)
	{
		var helper = CreateHelper(testListener, assembly);

		try
		{
			return action(helper);
		}
		finally
		{
			ExecuteOnBackgroundThread(async () => await helper.DisposeAsync());
		}
	}

	protected virtual void ExecuteOnBackgroundThread(Func<ValueTask> action) =>
		ThreadPool.QueueUserWorkItem(async _ => await action());

	public TestRunState RunAssembly(
		ITestListener testListener,
		Assembly assembly) =>
			Run(testListener, assembly, helper => helper.RunAll());

	public TestRunState RunMember(
		ITestListener testListener,
		Assembly assembly,
		MemberInfo member) =>
			Run(testListener, assembly, helper =>
			{
				if (member is Type type)
					return helper.RunClass(type);

				if (member is MethodInfo method)
					return helper.RunMethod(method);

				return TestRunState.NoTests;
			});

	public TestRunState RunNamespace(
		ITestListener testListener,
		Assembly assembly,
		string ns) =>
			Run(testListener, assembly, helper => helper.RunNamespace(ns));
}
