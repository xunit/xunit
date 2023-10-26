using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit;

/// <summary>
/// Allows the user to record actions for a test.
/// </summary>
public static class Record
{
	/// <summary>
	/// Records any exception which is thrown by the given code.
	/// </summary>
	/// <param name="testCode">The code which may throw an exception.</param>
	/// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
	public static Exception? Exception(Action testCode)
	{
		Guard.ArgumentNotNull(testCode);

		try
		{
			testCode();
			return null;
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	/// <summary>
	/// Records any exception which is thrown by the given code that has
	/// a return value. Generally used for testing property accessors.
	/// </summary>
	/// <param name="testCode">The code which may throw an exception.</param>
	/// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
	public static Exception? Exception(Func<object?> testCode)
	{
		Guard.ArgumentNotNull(testCode);
		object? testCodeResult;

		try
		{
			testCodeResult = testCode();
		}
		catch (Exception ex)
		{
			return ex;
		}

		if (testCodeResult is Task || testCodeResult is ValueTask)
			throw new InvalidOperationException("You must call Assert.ThrowsAsync, Assert.DoesNotThrowAsync, or Record.ExceptionAsync when testing async code.");

		return null;
	}

	/// <summary/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DoesNotReturn]
	[Obsolete("You must call Record.ExceptionAsync (and await the result) when testing async code.", true)]
	public static Exception Exception(Func<Task> testCode) => throw new NotImplementedException("You must call Record.ExceptionAsync (and await the result) when testing async code.");

	/// <summary/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DoesNotReturn]
	[Obsolete("You must call Record.ExceptionAsync (and await the result) when testing async code.", true)]
	public static Exception Exception(Func<ValueTask> testCode) => throw new NotImplementedException("You must call Record.ExceptionAsync (and await the result) when testing async code.");

	/// <summary/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DoesNotReturn]
	[Obsolete("You must call Record.ExceptionAsync (and await the result) when testing async code.", true)]
	public static Exception Exception<T>(Func<ValueTask<T>> testCode) => throw new NotImplementedException("You must call Record.ExceptionAsync (and await the result) when testing async code.");

	/// <summary>
	/// Records any exception which is thrown by the given task.
	/// </summary>
	/// <param name="testCode">The task which may throw an exception.</param>
	/// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
	public static async ValueTask<Exception?> ExceptionAsync(Func<Task> testCode)
	{
		Guard.ArgumentNotNull(testCode);

		try
		{
			await testCode();
			return null;
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	/// <summary>
	/// Records any exception which is thrown by the given task.
	/// </summary>
	/// <param name="testCode">The task which may throw an exception.</param>
	/// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
	public static async ValueTask<Exception?> ExceptionAsync(Func<ValueTask> testCode)
	{
		Guard.ArgumentNotNull(testCode);

		try
		{
			await testCode();
			return null;
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	/// <summary>
	/// Records any exception which is thrown by the given task.
	/// </summary>
	/// <param name="testCode">The task which may throw an exception.</param>
	/// <typeparam name="T">The type of the value returned by the value task.</typeparam>
	/// <returns>Returns the exception that was thrown by the code; null, otherwise.</returns>
	public static async ValueTask<Exception?> ExceptionAsync<T>(Func<ValueTask<T>> testCode)
	{
		Guard.ArgumentNotNull(testCode);

		try
		{
			await testCode();
			return null;
		}
		catch (Exception ex)
		{
			return ex;
		}
	}
}
