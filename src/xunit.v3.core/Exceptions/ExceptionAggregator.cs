using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.v3;

#pragma warning disable IDE0250  // This struct may *look* readonly to the analyzer, but it's logically not
#pragma warning disable IDE0251  // The same goes for the members of this struct

/// <summary>
/// Aggregates exceptions. Intended to run one or more code blocks, and collect the
/// exceptions thrown by those code blocks.
/// </summary>
public struct ExceptionAggregator
{
	readonly List<Exception> exceptions;

	/// <summary>
	/// Initializes a new instance of the <see cref="ExceptionAggregator"/> value type.
	/// </summary>
	public ExceptionAggregator() =>
		exceptions = [];

	ExceptionAggregator(IEnumerable<Exception>? exceptionsToClone) =>
		exceptions = new(exceptionsToClone ?? []);

	/// <summary>
	/// Returns <c>true</c> if the aggregator has at least one exception inside it.
	/// </summary>
	public bool HasExceptions =>
		GetExceptions().Count > 0;

	/// <summary>
	/// Adds an exception to the aggregator.
	/// </summary>
	/// <param name="ex">The exception to be added.</param>
	public void Add(Exception ex) =>
		GetExceptions().Add(Guard.ArgumentNotNull(ex));

	/// <summary>
	/// Adds exceptions from another aggregator into this aggregator.
	/// </summary>
	/// <param name="aggregator">The aggregator whose exceptions should be copied.</param>
	public void Aggregate(ExceptionAggregator aggregator) =>
		GetExceptions().AddRange(aggregator.GetExceptions());

	/// <summary>
	/// Clears the aggregator.
	/// </summary>
	public void Clear() =>
		GetExceptions().Clear();

	/// <summary>
	/// Clones the aggregator with a copy of the existing exceptions.
	/// </summary>
	public ExceptionAggregator Clone() =>
		new(GetExceptions());

	/// <summary>
	/// Creates an empty aggregator.
	/// </summary>
	public static ExceptionAggregator Create() =>
		new();

	List<Exception> GetExceptions() =>
		Guard.NotNull("ExceptionAggregator is in an invalid state", exceptions);

	/// <summary>
	/// Runs the code, catching the exception that is thrown and adding it to
	/// the aggregate.
	/// </summary>
	/// <param name="code">The code to be run.</param>
	public void Run(Action code)
	{
		Guard.ArgumentNotNull(code);

		try
		{
			code();
		}
		catch (Exception ex)
		{
			GetExceptions().Add(ex.Unwrap());
		}
	}

	/// <summary>
	/// Runs the code, catching the exception that is thrown and adding it to
	/// the aggregate.
	/// </summary>
	/// <param name="code">The code to be run.</param>
	/// <param name="defaultValue">The default value to return if the lambda throws an exception</param>
	public T Run<T>(
		Func<T> code,
		T defaultValue)
	{
		Guard.ArgumentNotNull(code);

		try
		{
			return code();
		}
		catch (Exception ex)
		{
			GetExceptions().Add(ex.Unwrap());
			return defaultValue;
		}
	}

	/// <summary>
	/// Runs the code, catching the exception that is thrown and adding it to
	/// the aggregate.
	/// </summary>
	/// <param name="code">The code to be run.</param>
	public async ValueTask RunAsync(Func<ValueTask> code)
	{
		Guard.ArgumentNotNull(code);

		try
		{
			await code();
		}
		catch (Exception ex)
		{
			GetExceptions().Add(ex.Unwrap());
		}
	}

	/// <summary>
	/// Runs the code, catching the exception that is thrown and adding it to
	/// the aggregate.
	/// </summary>
	/// <param name="code">The code to be run.</param>
	/// <param name="defaultValue">The default value to return if the lambda throws an exception</param>
	public async ValueTask<T> RunAsync<T>(
		Func<ValueTask<T>> code,
		T defaultValue)
	{
		Guard.ArgumentNotNull(code);

		try
		{
			return await code();
		}
		catch (Exception ex)
		{
			GetExceptions().Add(ex.Unwrap());
			return defaultValue;
		}
	}

	/// <summary>
	/// Throws an exception if the aggregator contains any exceptions. If the aggregator contains
	/// a single exception, it will be re-thrown without losing the original stack trace; if
	/// the aggregator contains more than one exception, then the original exceptions will be
	/// wrapped up into an instance of <see cref="AggregateException"/>.
	/// </summary>
	public void ThrowIfFaulted()
	{
		var exception = ToException();
		if (exception is null)
			return;

		ExceptionDispatchInfo.Capture(exception).Throw();
	}

	/// <summary>
	/// Returns an exception that represents the exceptions thrown by the code
	/// passed to the <see cref="Run"/> or RunAsync methods.
	/// </summary>
	/// <returns>Returns <c>null</c> if no exceptions were thrown; returns the
	/// exact exception if a single exception was thrown; returns <see cref="AggregateException"/>
	/// if more than one exception was thrown.</returns>
	public Exception? ToException()
	{
		var exceptions = GetExceptions();

		return exceptions.Count switch
		{
			0 => null,
			1 => exceptions[0],
			_ => toAggregateException(exceptions)
		};

		static Exception toAggregateException(List<Exception> exceptions)
		{
			// A special case for tests which time out: we might have both the test timeout and the cancellation token being
			// cancelled in the aggregator, and we don't care about the cancellation in this case, so we just return the
			// timeout exception.
			if (exceptions.Count == 2)
			{
				var orderedExceptions = exceptions.Select(x => (ex: x, name: x.GetType().FullName)).OrderBy(x => x.name).ToArray();
				if (orderedExceptions[0].name == "System.Threading.Tasks.TaskCanceledException" && orderedExceptions[1].name == "Xunit.Sdk.TestTimeoutException")
					return orderedExceptions[1].ex;
			}

			return new AggregateException(exceptions);
		}
	}
}
