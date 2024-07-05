using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// The methods on this static class can measure the time taken to execute actions (both synchronous
/// and asynchronous).
/// </summary>
public static class ExecutionTimer
{
	/// <summary>
	/// Executes an action and returns the amount of time it took to execute. Note: time cannot be
	/// measured for any action that throws an exception, so this should only be called by code that
	/// is known not to throw (f.e., using <see cref="ExceptionAggregator"/>) or when the execution
	/// time for throwing code is irrelevant.
	/// </summary>
	/// <param name="action">The action to measure.</param>
	public static TimeSpan Measure(Action action)
	{
		Guard.ArgumentNotNull(action);

		var stopwatch = Stopwatch.StartNew();
		action();
		return stopwatch.Elapsed;
	}

	/// <summary>
	/// Executes a function and returns the amount of time it took to execute. Note: time cannot be
	/// measured for any action that throws an exception, so this should only be called by code that
	/// is known not to throw (f.e., using <see cref="ExceptionAggregator"/>) or when the execution
	/// time for throwing code is irrelevant.
	/// </summary>
	/// <param name="func">The function to measure.</param>
	public static (T Result, TimeSpan Elapsed) Measure<T>(Func<T> func)
	{
		Guard.ArgumentNotNull(func);

		var stopwatch = Stopwatch.StartNew();
		var result = func();
		return (result, stopwatch.Elapsed);
	}

	/// <summary>
	/// Executes an asynchronous action and returns the amount of time it took to execute. Note: time
	/// cannot be measured for any action that throws an exception, so this should only be called by
	/// code that is known not to throw (f.e., using <see cref="ExceptionAggregator"/>) or when the
	/// execution time for throwing code is irrelevant.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to measure.</param>
	public static async ValueTask<TimeSpan> MeasureAsync(Func<ValueTask> asyncAction)
	{
		Guard.ArgumentNotNull(asyncAction);

		var stopwatch = Stopwatch.StartNew();
		await asyncAction();
		return stopwatch.Elapsed;
	}

	/// <summary>
	/// Executes an asynchronous function and returns the amount of time it took to execute. Note: time
	/// cannot be measured for any action that throws an exception, so this should only be called by
	/// code that is known not to throw (f.e., using <see cref="ExceptionAggregator"/>) or when the
	/// execution time for throwing code is irrelevant.
	/// </summary>
	/// <param name="asyncFunc">The asynchronous function to measure.</param>
	public static async ValueTask<(T Result, TimeSpan Elapsed)> MeasureAsync<T>(Func<ValueTask<T>> asyncFunc)
	{
		Guard.ArgumentNotNull(asyncFunc);

		var stopwatch = Stopwatch.StartNew();
		var result = await asyncFunc();
		return (result, stopwatch.Elapsed);
	}
}
