using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior to reflection invoke a test method. This includes
/// support for async test methods ("async Task" and "async ValueTask" for C#/VB, and async functions
/// in F#) as well as creation and disposal of the test class.
/// </summary>
/// <typeparam name="TContext">The context type used by the invoker</typeparam>
/// <typeparam name="TTest">The type of the test that the test framework uses. Must be derived
/// from <see cref="ITest"/>.</typeparam>
public abstract class TestInvoker<TContext, TTest>
	where TContext : TestInvokerContext<TTest>
	where TTest : class, ITest
{
	/// <summary>
	/// This method calls the test method via <see cref="MethodBase.Invoke(object, object[])"/>. This is an available
	/// override point if you need to do some other form of invocation of the actual test method.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>The return value from the test method invocation</returns>
	protected virtual object? CallTestMethod(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return ctxt.TestMethod.Invoke(ctxt.TestClassInstance, ctxt.TestMethodArguments);
	}

	/// <summary>
	/// Invokes the test method on the given test class instance. This method fast fails any test marked as "async void",
	/// ensures that the test method has the correct number of arguments, then calls <see cref="CallTestMethod"/>
	/// to do the actual method invocation. It ensure that any async test method is fully completed before returning, and
	/// returns the measured clock time that the invocation took.
	/// </summary>
	/// <remarks>
	/// Test runners will typically catch exceptions thrown by this methods, but it's still strongly
	/// recommended that you use <see cref="ExecutionTimer"/> to measure the execution, wrapped
	/// around a call to the aggregator inside <paramref name="ctxt"/>, so that you can accurately
	/// measure the time spent in execution, even when the test method throws an exception.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Returns the amount of time the test took to run, in seconds</returns>
	protected virtual async ValueTask<TimeSpan> InvokeTestMethodAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (AsyncUtility.IsAsyncVoid(ctxt.TestMethod))
		{
			ctxt.Aggregator.Add(new InvalidOperationException("Tests marked as 'async void' are no longer supported. Please convert to 'async Task' or 'async ValueTask'."));
			return TimeSpan.Zero;
		}

		return await ExecutionTimer.MeasureAsync(
			() => ctxt.Aggregator.RunAsync(
				async () =>
				{
					var parameterCount = ctxt.TestMethod.GetParameters().Length;
					var valueCount = ctxt.TestMethodArguments is null ? 0 : ctxt.TestMethodArguments.Length;
					if (parameterCount != valueCount)
					{
						ctxt.Aggregator.Add(
							new InvalidOperationException(
								string.Format(
									CultureInfo.CurrentCulture,
									"The test method expected {0} parameter value{1}, but {2} parameter value{3} {4} provided.",
									parameterCount,
									parameterCount == 1 ? "" : "s",
									valueCount,
									valueCount == 1 ? "" : "s",
									valueCount == 1 ? "was" : "were"
								)
							)
						);
					}
					else
					{
						var logEnabled = TestEventSource.Log.IsEnabled();

						if (logEnabled)
							TestEventSource.Log.TestStart(ctxt.Test.TestDisplayName);

						try
						{
							var result = CallTestMethod(ctxt);
							var valueTask = AsyncUtility.TryConvertToValueTask(result);
							if (valueTask.HasValue)
								await valueTask.Value;
						}
						catch (TaskCanceledException) { }
						finally
						{
							if (logEnabled)
								TestEventSource.Log.TestStop(ctxt.Test.TestDisplayName);
						}
					}
				}
			)
		);
	}

	/// <summary>
	/// Invokes the test method.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Returns the time (in seconds) running the test</returns>
	protected ValueTask<TimeSpan> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return ctxt.Aggregator.RunAsync(
			async () =>
				!ctxt.CancellationTokenSource.IsCancellationRequested && !ctxt.Aggregator.HasExceptions
					? await InvokeTestMethodAsync(ctxt)
					: TimeSpan.Zero,
			TimeSpan.Zero
		);
	}
}
