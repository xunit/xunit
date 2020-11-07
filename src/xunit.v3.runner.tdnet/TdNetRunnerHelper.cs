using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestDriven.Framework;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.v3;

namespace Xunit.Runner.TdNet
{
	public class TdNetRunnerHelper : IAsyncDisposable
	{
		readonly TestAssemblyConfiguration? configuration;
		bool disposed;
		readonly DisposalTracker disposalTracker = new DisposalTracker();
		readonly ITestListener? testListener;
		readonly Xunit2? xunit;

		/// <summary>
		/// This constructor is for unit testing purposes only.
		/// </summary>
		protected TdNetRunnerHelper()
		{ }

		public TdNetRunnerHelper(
			Assembly assembly,
			ITestListener testListener)
		{
			this.testListener = testListener;

			var assemblyFileName = assembly.GetLocalCodeBase();
			configuration = ConfigReader.Load(assemblyFileName);
			var diagnosticMessageSink = new DiagnosticMessageSink(testListener, Path.GetFileNameWithoutExtension(assemblyFileName), configuration.DiagnosticMessagesOrDefault);
			xunit = new Xunit2(diagnosticMessageSink, configuration.AppDomainOrDefault, _NullSourceInformationProvider.Instance, assemblyFileName, shadowCopy: false);
			disposalTracker.Add(xunit);
		}

		public virtual IReadOnlyList<ITestCase> Discover()
		{
			Guard.NotNull($"Attempted to use an uninitialized {GetType().FullName}", xunit);

			return Discover(sink => xunit.Find(false, sink, _TestFrameworkOptions.ForDiscovery(configuration)));
		}

		IReadOnlyList<ITestCase> Discover(Type? type)
		{
			Guard.NotNull($"Attempted to use an uninitialized {GetType().FullName}", xunit);

			if (type == null)
				return new ITestCase[0];

			return Discover(sink => xunit.Find(type.FullName!, false, sink, _TestFrameworkOptions.ForDiscovery(configuration)));
		}

		IReadOnlyList<ITestCase> Discover(Action<_IMessageSink> discoveryAction)
		{
			try
			{
				var sink = new TestDiscoverySink();
				disposalTracker.Add(sink);
				discoveryAction(sink);
				sink.Finished.WaitOne();
				return sink.TestCases.ToList();
			}
			catch (Exception ex)
			{
				testListener?.WriteLine("Error during test discovery:\r\n" + ex, Category.Error);
				return new ITestCase[0];
			}
		}

		public ValueTask DisposeAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			return disposalTracker.DisposeAsync();
		}

		public virtual TestRunState Run(
			IReadOnlyList<ITestCase>? testCases = null,
			TestRunState initialRunState = TestRunState.NoTests)
		{
			Guard.NotNull($"Attempted to use an uninitialized {GetType().FullName}", testListener);
			Guard.NotNull($"Attempted to use an uninitialized {GetType().FullName}", xunit);

			try
			{
				if (testCases == null)
					testCases = Discover();

				var resultSink = new ResultSink(testListener, testCases.Count) { TestRunState = initialRunState };
				disposalTracker.Add(resultSink);

				var executionOptions = _TestFrameworkOptions.ForExecution(configuration);
				xunit.RunTests(testCases, resultSink, executionOptions);

				resultSink.Finished.WaitOne();

				return resultSink.TestRunState;
			}
			catch (Exception ex)
			{
				testListener.WriteLine("Error during test execution:\r\n" + ex, Category.Error);
				return TestRunState.Error;
			}
		}

		public virtual TestRunState RunClass(
			Type type,
			TestRunState initialRunState = TestRunState.NoTests)
		{
			var state = Run(Discover(type), initialRunState);

			foreach (var memberInfo in type.GetMembers())
			{
				var childType = memberInfo as Type;
				if (childType != null)
					state = RunClass(childType, state);
			}

			return state;
		}

		public virtual TestRunState RunMethod(
			MethodInfo method,
			TestRunState initialRunState = TestRunState.NoTests)
		{
			var testCases = Discover(method.ReflectedType).Where(tc =>
			{
				var methodInfo = tc.GetMethod();
				if (methodInfo == null)
					return false;

				if (methodInfo == method)
					return true;

				if (methodInfo.IsGenericMethod)
					return methodInfo.GetGenericMethodDefinition() == method;

				return false;
			}).ToList();

			return Run(testCases, initialRunState);
		}
	}
}
