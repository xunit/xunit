#nullable enable  // This file is temporarily shared with xunit.v1.tests and xunit.v2.tests, which are not nullable-enabled

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Xunit.Internal
{
	/// <summary>
	/// Helper class for guarding value arguments and valid state.
	/// </summary>
	public static class Guard
	{
		/// <summary>
		/// Ensures that an argument is not null.
		/// </summary>
		/// <typeparam name="T">The argument type</typeparam>
		/// <param name="argValue">The value of the argument</param>
		/// <param name="argName">The name of the argument</param>
		/// <returns>The argument value as a non-null value</returns>
		/// <exception cref="ArgumentNullException">Thrown when the argument is null</exception>
		public static T ArgumentNotNull<T>(
			[NotNull] T? argValue,
			[CallerArgumentExpression("argValue")] string? argName = null)
		{
			if (argValue == null)
				throw new ArgumentNullException(argName?.TrimStart('@'));

			return argValue;
		}

		/// <summary>
		/// Ensures that an argument is not null.
		/// </summary>
		/// <typeparam name="T">The argument type</typeparam>
		/// <param name="message">The exception message to use when the argument is null</param>
		/// <param name="argValue">The value of the argument</param>
		/// <param name="argName">The name of the argument</param>
		/// <returns>The argument value as a non-null value</returns>
		/// <exception cref="ArgumentNullException">Thrown when the argument is null</exception>
		public static T ArgumentNotNull<T>(
			string message,
			[NotNull] T? argValue,
			string? argName = null)
				where T : class
		{
			if (argValue == null)
				throw new ArgumentNullException(argName, message);

			return argValue;
		}

		/// <summary>
		/// Ensures that an argument is not null or empty.
		/// </summary>
		/// <typeparam name="T">The argument type</typeparam>
		/// <param name="argValue">The value of the argument</param>
		/// <param name="argName">The name of the argument</param>
		/// <returns>The argument value as a non-null, non-empty value</returns>
		/// <exception cref="ArgumentException">Thrown when the argument is null or empty</exception>
		public static T ArgumentNotNullOrEmpty<T>(
			[NotNull] T? argValue,
			[CallerArgumentExpression("argValue")] string? argName = null)
				where T : class, IEnumerable
		{
			ArgumentNotNull(argValue, argName);

			if (!argValue.GetEnumerator().MoveNext())
				throw new ArgumentException("Argument was empty", argName);

			return argValue;
		}

		/// <summary>
		/// Ensures that an argument is not null or empty.
		/// </summary>
		/// <typeparam name="T">The argument type</typeparam>
		/// <param name="message">The exception message to use when the argument is null or empty</param>
		/// <param name="argValue">The value of the argument</param>
		/// <param name="argName">The name of the argument</param>
		/// <returns>The argument value as a non-null, non-empty value</returns>
		/// <exception cref="ArgumentException">Thrown when the argument is null or empty</exception>
		public static T ArgumentNotNullOrEmpty<T>(
			string message,
			[NotNull] T? argValue,
			string? argName = null)
				where T : class, IEnumerable
		{
			if (argValue == null || !argValue.GetEnumerator().MoveNext())
				throw new ArgumentException(message, argName);

			return argValue;
		}

		/// <summary>
		/// Ensures that an argument is valid.
		/// </summary>
		/// <param name="message">The exception message to use when the argument is not valid</param>
		/// <param name="test">The validity test value</param>
		/// <param name="argName">The name of the argument</param>
		/// <returns>The argument value as a non-null value</returns>
		/// <exception cref="ArgumentException">Thrown when the argument is not valid</exception>
		public static void ArgumentValid(
			string message,
			bool test,
			string? argName = null)
		{
			if (!test)
				throw new ArgumentException(message, argName);
		}

		/// <summary>
		/// Ensures that a filename argument is not null or empty, and that the file exists on disk.
		/// </summary>
		/// <param name="fileName">The file name value</param>
		/// <param name="argName">The name of the argument</param>
		/// <returns>The file name as a non-null value</returns>
		/// <exception cref="ArgumentException">Thrown when the argument is null, empty, or not on disk</exception>
		public static string FileExists(
			[NotNull] string? fileName,
			[CallerArgumentExpression("fileName")] string? argName = null)
		{
			ArgumentNotNullOrEmpty(fileName, argName);
			ArgumentValid($"File not found: {fileName}", File.Exists(fileName), argName);

			return fileName;
		}

		/// <summary>
		/// Ensure that a value is not null.
		/// </summary>
		/// <typeparam name="T">The value type</typeparam>
		/// <param name="message">The exception message to use when the value is not valid</param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Thrown when the value is not valid</exception>
		public static T NotNull<T>(
			string message,
			[NotNull] T? value)
				where T : class
		{
			if (value == null)
				throw new InvalidOperationException(message);

			return value;
		}
	}
}

#if !NETCOREAPP3_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	internal sealed class CallerArgumentExpressionAttribute : Attribute
	{
		public CallerArgumentExpressionAttribute(string parameterName)
		{
			ParameterName = parameterName;
		}

		public string ParameterName { get; }
	}
}

#endif
