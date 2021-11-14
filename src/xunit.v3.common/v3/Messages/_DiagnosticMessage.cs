using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message is sent when the test framework wants to report a diagnostic message
	/// to the end user.
	/// </summary>
	public class _DiagnosticMessage : _MessageSinkMessage
	{
		string? message;

		/// <summary>
		/// Gets or sets the diagnostic message.
		/// </summary>
		public string Message
		{
			get => message ?? throw new InvalidOperationException($"Attempted to get {nameof(Message)} on an uninitialized '{GetType().FullName}' object");
			set => message = Guard.ArgumentNotNull(value, nameof(Message));
		}

		/// <inheritdoc/>
		public override string ToString() =>
			$"{GetType().Name} message={message.Quoted()}";
	}
}
