using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.v3;

internal sealed class InProcessDirectLauncherProcess : ITestProcessBase
{
	readonly CancellationTokenSource cancellationTokenSource;
	readonly Task workerTask;

	InProcessDirectLauncherProcess(
		CancellationTokenSource cancellationTokenSource,
		Task workerTask)
	{
		this.cancellationTokenSource = cancellationTokenSource;
		this.workerTask = workerTask;
	}

	public bool HasExited =>
		workerTask.Status is TaskStatus.Faulted or TaskStatus.RanToCompletion;

	public void Cancel(bool forceCancellation)
	{
		if (HasExited)
			return;

		cancellationTokenSource.Cancel();
	}

	public void Dispose()
	{
		try
		{
			// We can't forcefully close an in-process test run, because Thread.Abort was removed from .NET
			// so we'll request graceful termination and give it 15 seconds to finish.
			Cancel(false);
			WaitForExit(15_000);

			cancellationTokenSource.Dispose();
		}
		catch { }
	}

	public bool WaitForExit(int milliseconds) =>
		workerTask.SpinWait(milliseconds);

	// Factory method

	public static ITestProcessBase Create(
		XunitProjectAssembly projectAssembly,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider,
		MethodInfo method)
	{
		var cancellationTokenSource = new CancellationTokenSource();
		var task = Task.Run(async () =>
		{
			IMessageSink delegatingMessageSink = new TokenSourceCancellationMessageSink(messageSink, cancellationTokenSource);
			if (projectAssembly.Configuration.IncludeSourceInformationOrDefault && sourceInformationProvider is not null)
				delegatingMessageSink = new SourceInformationMessageSink(delegatingMessageSink, sourceInformationProvider);

			var task = method.Invoke(obj: null, [delegatingMessageSink, diagnosticMessageSink, projectAssembly, cancellationTokenSource]) as Task;
			if (task is not null)
				await task;
		});

		return new InProcessDirectLauncherProcess(cancellationTokenSource, task);
	}
}
