#nullable enable  // This file is temporarily shared with xunit.v1.tests and xunit.v2.tests, which are not nullable-enabled

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Xunit.Internal;

/// <summary>
/// Helper class for guarding value arguments and valid state.
/// </summary>
public static class Guard
{
	/// <summary>
	/// Ensures that an enum value is valid by comparing against a list of valid values.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="validValues">The list of valid values</param>
	/// <param name="argName">The name of the argument</param>
	/// <exception cref="ArgumentException"></exception>
	public static T ArgumentEnumValid<T>(
		T argValue,
		HashSet<T> validValues,
		[CallerArgumentExpression(nameof(argValue))] string? argName = null)
			where T : Enum =>
				ArgumentNotNull(validValues).Contains(argValue)
					? argValue
					: throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Enum value {0} not in valid set: [{1}]", argValue, string.Join(", ", validValues)), argName?.TrimStart('@'));

	/// <summary>
	/// Ensures that a nullable value type argument is not null.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null value</returns>
	/// <exception cref="ArgumentNullException">Thrown when the argument is null</exception>
	public static T ArgumentNotNull<T>(
		[NotNull] T? argValue,
		[CallerArgumentExpression(nameof(argValue))] string? argName = null)
			where T : struct =>
				argValue ?? throw new ArgumentNullException(argName?.TrimStart('@'));

	/// <summary>
	/// Ensures that a nullable reference type argument is not null.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null value</returns>
	/// <exception cref="ArgumentNullException">Thrown when the argument is null</exception>
	public static T ArgumentNotNull<T>(
		[NotNull] T? argValue,
		[CallerArgumentExpression(nameof(argValue))] string? argName = null)
			where T : class =>
				argValue ?? throw new ArgumentNullException(argName?.TrimStart('@'));

	/// <summary>
	/// Ensures that a nullable reference type argument is not null.
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
			where T : class =>
				argValue ?? throw new ArgumentNullException(argName, message);

	/// <summary>
	/// Ensures that a nullable reference type argument is not null.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="messageFunc">The creator for an exception message to use when the argument is null</param>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null value</returns>
	/// <exception cref="ArgumentNullException">Thrown when the argument is null</exception>
	public static T ArgumentNotNull<T>(
		Func<string> messageFunc,
		[NotNull] T? argValue,
		string? argName = null)
		where T : class =>
			argValue ?? throw new ArgumentNullException(argName, messageFunc?.Invoke());

	/// <summary>
	/// Ensures that a nullable enumerable type argument is not null or empty.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null, non-empty value</returns>
	/// <exception cref="ArgumentException">Thrown when the argument is null or empty</exception>
	public static T ArgumentNotNullOrEmpty<T>(
		[NotNull] T? argValue,
		[CallerArgumentExpression(nameof(argValue))] string? argName = null)
			where T : class, IEnumerable =>
				ArgumentNotNull(argValue, argName).GetEnumerator().MoveNext()
					? argValue
					: throw new ArgumentException("Argument was empty", argName?.TrimStart('@'));

	/// <summary>
	/// Ensures that a nullable enumerable type argument is not null or empty.
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
			where T : class, IEnumerable =>
				ArgumentNotNull(argValue, argName).GetEnumerator().MoveNext()
					? argValue
					: throw new ArgumentException(message, argName);

	/// <summary>
	/// Ensures that a nullable enumerable type argument is not null or empty.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="messageFunc">The creator for an exception message to use when the argument is null or empty</param>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null, non-empty value</returns>
	/// <exception cref="ArgumentException">Thrown when the argument is null or empty</exception>
	public static T ArgumentNotNullOrEmpty<T>(
		Func<string> messageFunc,
		[NotNull] T? argValue,
		string? argName = null)
			where T : class, IEnumerable =>
				argValue is not null && argValue.GetEnumerator().MoveNext()
					? argValue
					: throw new ArgumentException(messageFunc?.Invoke(), argName);

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
	/// Ensures that an argument is valid.
	/// </summary>
	/// <param name="messageFunc">The creator for an exception message to use when the argument is not valid</param>
	/// <param name="test">The validity test value</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-null value</returns>
	/// <exception cref="ArgumentException">Thrown when the argument is not valid</exception>
	public static void ArgumentValid(
		Func<string> messageFunc,
		bool test,
		string? argName = null)
	{
		if (!test)
			throw new ArgumentException(messageFunc?.Invoke(), argName);
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
		[CallerArgumentExpression(nameof(fileName))] string? argName = null)
	{
		ArgumentNotNullOrEmpty(fileName, argName);
		ArgumentValid(() => string.Format(CultureInfo.CurrentCulture, "File not found: {0}", fileName), File.Exists(fileName), argName?.TrimStart('@'));

		return fileName;
	}

	/// <summary>
	/// Ensures that a value is not default value. This is used for values of generic types
	/// where nullability is not known.
	/// </summary>
	/// <typeparam name="T">The argument type</typeparam>
	/// <param name="argValue">The value of the argument</param>
	/// <param name="argName">The name of the argument</param>
	/// <returns>The argument value as a non-default value</returns>
	/// <exception cref="ArgumentNullException">Thrown when the argument is default</exception>
	public static T GenericArgumentNotNull<T>(
		[NotNull] T? argValue,
		[CallerArgumentExpression(nameof(argValue))] string? argName = null) =>
			argValue ?? throw new ArgumentNullException(argName?.TrimStart('@'));

	/// <summary>
	/// Ensure that a reference value is not null.
	/// </summary>
	/// <typeparam name="T">The value type</typeparam>
	/// <param name="message">The exception message to use when the value is not valid</param>
	/// <param name="value">The value to test for null</param>
	/// <returns>The value as a non-null value</returns>
	/// <exception cref="InvalidOperationException">Thrown when the value is not valid</exception>
	public static T NotNull<T>(
		string message,
		[NotNull] T? value)
			where T : class =>
				value ?? throw new InvalidOperationException(message);

	/// <summary>
	/// Ensure that a reference value is not null.
	/// </summary>
	/// <typeparam name="T">The value type</typeparam>
	/// <param name="messageFunc">The creator for an exception message to use when the value is not valid</param>
	/// <param name="value">The value to test for null</param>
	/// <returns>The value as a non-null value</returns>
	/// <exception cref="InvalidOperationException">Thrown when the value is not valid</exception>
	public static T NotNull<T>(
		Func<string> messageFunc,
		[NotNull] T? value)
			where T : class =>
				value ?? throw new InvalidOperationException(messageFunc?.Invoke());

	/// <summary>
	/// Ensure that a nullable struct value is not null.
	/// </summary>
	/// <typeparam name="T">The value type</typeparam>
	/// <param name="message">The exception message to use when the value is not valid</param>
	/// <param name="value">The value to test for null</param>
	/// <returns>The value as a non-null value</returns>
	/// <exception cref="InvalidOperationException">Thrown when the value is not valid</exception>
	public static T NotNull<T>(
		string message,
		[NotNull] T? value)
			where T : struct =>
				value ?? throw new InvalidOperationException(message);

	/// <summary>
	/// Ensure that a nullable struct value is not null.
	/// </summary>
	/// <typeparam name="T">The value type</typeparam>
	/// <param name="messageFunc">The creator for an exception message to use when the value is not valid</param>
	/// <param name="value">The value to test for null</param>
	/// <returns>The value as a non-null value</returns>
	/// <exception cref="InvalidOperationException">Thrown when the value is not valid</exception>
	public static T NotNull<T>(
		Func<string> messageFunc,
		[NotNull] T? value)
			where T : struct =>
				value ?? throw new InvalidOperationException(messageFunc?.Invoke());
}
