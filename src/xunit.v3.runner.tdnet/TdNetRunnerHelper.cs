using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestDriven.Framework;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.TdNet
{
	public class TdNetRunnerHelper : IAsyncDisposable
	{
		bool disposed;
		readonly DisposalTracker disposalTracker = new DisposalTracker();
		readonly IFrontController? frontController;
		readonly XunitProjectAssembly projectAssembly;
		readonly ITestListener? testListener;

		/// <summary>
		/// This constructor is for unit testing purposes only.
		/// </summary>
		protected TdNetRunnerHelper()
		{
			// TODO: Where do these come from?
			var project = new XunitProject();
			projectAssembly = new(project);
		}

		public TdNetRunnerHelper(
			Assembly assembly,
			ITestListener testListener)
		{
			this.testListener = testListener;

			var assemblyFileName = assembly.GetLocalCodeBase();
			var project = new XunitProject();
			projectAssembly = new XunitProjectAssembly(project)
			{
				Assembly = assembly,
				AssemblyFilename = assemblyFileName,
				TargetFramework = AssemblyUtility.GetTargetFramework(assemblyFileName)
			};
			projectAssembly.Configuration.ShadowCopy = false;
			ConfigReader.Load(projectAssembly.Configuration, assemblyFileName);

			var diagnosticMessageSink = new DiagnosticMessageSink(testListener, Path.GetFileNameWithoutExtension(assemblyFileName), projectAssembly.Configuration.DiagnosticMessagesOrDefault);
			frontController = Xunit2.ForDiscoveryAndExecution(projectAssembly, diagnosticMessageSink: diagnosticMessageSink);
			disposalTracker.Add(frontController);
		}

		public virtual IReadOnlyList<_TestCaseDiscovered> Discover()
		{
			Guard.NotNull($"Attempted to use an uninitialized {GetType().FullName}", frontController);

			var settings = new FrontControllerFindSettings(_TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration));
			return Discover(sink => frontController.Find(sink, settings));
		}

		IReadOnlyList<_TestCaseDiscovered> Discover(Type? type)
		{
			Guard.NotNull($"Attempted to use an uninitialized {GetType().FullName}", frontController);

			if (type == null || type.FullName == null)
				return new _TestCaseDiscovered[0];

			var settings = new FrontControllerFindSettings(_TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration));
			settings.Filters.IncludedClasses.Add(type.FullName);

			return Discover(sink => frontController.Find(sink, settings));
		}

		IReadOnlyList<_TestCaseDiscovered> Discover(Action<_IMessageSink> discoveryAction)
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
				return new _TestCaseDiscovered[0];
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
			IReadOnlyList<_TestCaseDiscovered>? testCases = null,
			TestRunState initialRunState = TestRunState.NoTests)
		{
			Guard.NotNull($"Attempted to use an uninitialized {GetType().FullName}", testListener);
			Guard.NotNull($"Attempted to use an uninitialized {GetType().FullName}", frontController);

			try
			{
				// TODO: This should be able to be converted to FindAndRun, but we need test case
				// count for the results sink...?
				if (testCases == null)
					testCases = Discover();

				var resultSink = new ResultSink(testListener, testCases.Count) { TestRunState = initialRunState };
				disposalTracker.Add(resultSink);

				var executionOptions = _TestFrameworkOptions.ForExecution(projectAssembly.Configuration);
				var settings = new FrontControllerRunSettings(executionOptions, testCases.Select(tc => tc.Serialization));
				frontController.Run(resultSink, settings);

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
				if (tc.TestClassWithNamespace == null || tc.TestMethod == null)
					return false;

				var typeInfo = Type.GetType(tc.TestClassWithNamespace);
				if (typeInfo == null)
					return false;

				var methodInfo = typeInfo.GetMethod(tc.TestMethod, BindingFlags.Public);
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
