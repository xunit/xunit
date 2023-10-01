using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.Sdk;

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
	public ExceptionAggregator()
	{
		exceptions = new();
	}

	ExceptionAggregator(IEnumerable<Exception>? exceptionsToClone) =>
		exceptions = new(exceptionsToClone ?? Array.Empty<Exception>());

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
	/// Returns an exception that represents the exceptions thrown by the code
	/// passed to the <see cref="Run"/> or RunAsync methods.
	/// </summary>
	/// <returns>Returns <c>null</c> if no exceptions were thrown; returns the
	/// exact exception if a single exception was thrown; returns <see cref="AggregateException"/>
	/// if more than one exception was thrown.</returns>
	public Exception? ToException()
	{
		var exceptions = GetExceptions();

		if (exceptions.Count == 0)
			return null;
		if (exceptions.Count == 1)
			return exceptions[0];
		return new AggregateException(exceptions);
	}
}
