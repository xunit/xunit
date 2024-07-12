using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message is sent when xUnit.net itself wants to log internal messages and state that are typically only
/// used by the xUnit.net team to gain a deeper understanding of potential end user issues. These messages are
/// rarely useful to end users directly, and may result in very noisy logs. This is typically not displayed
/// unless the user has explicit asked for internal diagnostic messages to be displayed (see
/// <a href="https://xunit.net/docs/configuration-files#internalDiagnosticMessages"/> on how to enable
/// display of internal diagnostic messages).
/// </summary>
[JsonTypeID("internal-diagnostic")]
public sealed class InternalDiagnosticMessage : MessageSinkMessage
{
	string? message;

	/// <summary>
	/// Creates a new instance of the <see cref="InternalDiagnosticMessage"/> class.
	/// </summary>
	public InternalDiagnosticMessage()
	{ }

	/// <summary>
	/// Creates a new instance of the <see cref="InternalDiagnosticMessage"/> class with
	/// the provided message.
	/// </summary>
	/// <param name="message">The diagnostic message</param>
	[SetsRequiredMembers]
	public InternalDiagnosticMessage(string message) =>
		Message = Guard.ArgumentNotNull(message);

	/// <summary>
	/// Creates a new instance of the <see cref="InternalDiagnosticMessage"/> class with
	/// the provided message format and single argument.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	[SetsRequiredMembers]
	public InternalDiagnosticMessage(
		string messageFormat,
		object? arg0) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0);

	/// <summary>
	/// Creates a new instance of the <see cref="InternalDiagnosticMessage"/> class with
	/// the provided message format and two arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	[SetsRequiredMembers]
	public InternalDiagnosticMessage(
		string messageFormat,
		object? arg0,
		object? arg1) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0, arg1);

	/// <summary>
	/// Creates a new instance of the <see cref="InternalDiagnosticMessage"/> class with
	/// the provided message format and three arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	/// <param name="arg2">The value to replace {2} in the format string.</param>
	[SetsRequiredMembers]
	public InternalDiagnosticMessage(
		string messageFormat,
		object? arg0,
		object? arg1,
		object? arg2) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0, arg1, arg2);

	/// <summary>
	/// Creates a new instance of the <see cref="InternalDiagnosticMessage"/> class with
	/// the provided message format and multiple arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	[SetsRequiredMembers]
	public InternalDiagnosticMessage(
		string messageFormat,
		params object?[] args) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), args);

	/// <summary>
	/// Gets or sets the diagnostic message.
	/// </summary>
	public required string Message
	{
		get => this.ValidateNullablePropertyValue(message, nameof(Message));
		set => message = Guard.ArgumentNotNull(value, nameof(Message));
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		message = JsonDeserializer.TryGetString(root, nameof(Message));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(Message), Message);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} message={1}", GetType().Name, message.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(message, nameof(Message), invalidProperties);
	}
}
