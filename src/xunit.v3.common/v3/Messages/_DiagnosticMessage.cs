using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message is sent when code (1st or 3rd party) wants to alert the user to a situation that may require
/// diagnostic investigation. This is typically not displayed unless the user has explicitly asked for diagnostic
/// messages to be displayed (see <see href="https://xunit.net/docs/configuration-files#diagnosticMessages"/> on
/// how to enable display of diagnostic messages).
/// </summary>
public class _DiagnosticMessage : _MessageSinkMessage
{
	string? message;

	/// <summary>
	/// Creats a new instance of the <see cref="_DiagnosticMessage"/> class.
	/// </summary>
	public _DiagnosticMessage()
	{ }

	/// <summary>
	/// Creats a new instance of the <see cref="_DiagnosticMessage"/> class with
	/// the provided message.
	/// </summary>
	/// <param name="message">The diagnostic message</param>
	public _DiagnosticMessage(string message) =>
		Message = Guard.ArgumentNotNull(message);

	/// <summary>
	/// Creats a new instance of the <see cref="_DiagnosticMessage"/> class with
	/// the provided message format and single argument.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	public _DiagnosticMessage(
		string messageFormat,
		object? arg0) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0);

	/// <summary>
	/// Creats a new instance of the <see cref="_DiagnosticMessage"/> class with
	/// the provided message format and two arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	public _DiagnosticMessage(
		string messageFormat,
		object? arg0,
		object? arg1) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0, arg1);

	/// <summary>
	/// Creats a new instance of the <see cref="_DiagnosticMessage"/> class with
	/// the provided message format and three arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	/// <param name="arg2">The value to replace {2} in the format string.</param>
	public _DiagnosticMessage(
		string messageFormat,
		object? arg0,
		object? arg1,
		object? arg2) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0, arg1, arg2);

	/// <summary>
	/// Creats a new instance of the <see cref="_DiagnosticMessage"/> class with
	/// the provided message format and multiple arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public _DiagnosticMessage(
		string messageFormat,
		params object?[] args) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), args);

	/// <summary>
	/// Gets or sets the diagnostic message.
	/// </summary>
	public string Message
	{
		get => this.ValidateNullablePropertyValue(message, nameof(Message));
		set => message = Guard.ArgumentNotNull(value, nameof(Message));
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} message={1}", GetType().Name, message.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(message, nameof(Message), invalidProperties);
	}
}
