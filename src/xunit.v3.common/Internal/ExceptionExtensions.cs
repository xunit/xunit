using System;
using System.Reflection;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public static class ExceptionExtensions
{
	/// <summary>
	/// Unwraps an exception to remove any wrappers, like <see cref="TargetInvocationException"/>.
	/// </summary>
	/// <param name="ex">The exception to unwrap.</param>
	/// <returns>The unwrapped exception.</returns>
	public static Exception Unwrap(this Exception ex)
	{
		while (true)
		{
			if (ex is not TargetInvocationException tiex)
				return ex;

			ex = tiex.InnerException!;
		}
	}
}
