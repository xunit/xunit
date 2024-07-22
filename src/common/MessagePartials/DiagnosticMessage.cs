using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="IDiagnosticMessage"/>.
/// </summary>
[JsonTypeID("diagnostic")]
sealed partial class DiagnosticMessage : MessageSinkMessage, IDiagnosticMessage
{
	/// <summary>
	/// Creates a new instance of the <see cref="DiagnosticMessage"/> class.
	/// </summary>
	public DiagnosticMessage()
	{ }

	/// <summary>
	/// Creates a new instance of the <see cref="DiagnosticMessage"/> class with
	/// the provided message.
	/// </summary>
	/// <param name="message">The diagnostic message</param>
	[SetsRequiredMembers]
	public DiagnosticMessage(string message) =>
		Message = Guard.ArgumentNotNull(message);

	/// <summary>
	/// Creates a new instance of the <see cref="DiagnosticMessage"/> class with
	/// the provided message format and single argument.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	[SetsRequiredMembers]
	public DiagnosticMessage(
		string messageFormat,
		object? arg0) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0);

	/// <summary>
	/// Creates a new instance of the <see cref="DiagnosticMessage"/> class with
	/// the provided message format and two arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	[SetsRequiredMembers]
	public DiagnosticMessage(
		string messageFormat,
		object? arg0,
		object? arg1) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0, arg1);

	/// <summary>
	/// Creates a new instance of the <see cref="DiagnosticMessage"/> class with
	/// the provided message format and three arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string.</param>
	/// <param name="arg1">The value to replace {1} in the format string.</param>
	/// <param name="arg2">The value to replace {2} in the format string.</param>
	[SetsRequiredMembers]
	public DiagnosticMessage(
		string messageFormat,
		object? arg0,
		object? arg1,
		object? arg2) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), arg0, arg1, arg2);

	/// <summary>
	/// Creates a new instance of the <see cref="DiagnosticMessage"/> class with
	/// the provided message format and multiple arguments.
	/// </summary>
	/// <param name="messageFormat">The message format string</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	[SetsRequiredMembers]
	public DiagnosticMessage(
		string messageFormat,
		params object?[] args) =>
			Message = string.Format(CultureInfo.CurrentCulture, Guard.ArgumentNotNull(messageFormat), args);

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		serializer.Serialize(nameof(Message), Message);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} message={1}", GetType().Name, Message.Quoted());
}
