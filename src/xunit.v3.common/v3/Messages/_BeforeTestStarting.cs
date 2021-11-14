using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message is sent during execution to indicate that the Before method of a
	/// <see cref="T:Xunit.Sdk.BeforeAfterTestAttribute"/> is about to execute.
	/// </summary>
	public class _BeforeTestStarting : _TestMessage
	{
		string? attributeName;

		/// <summary>
		/// Gets or sets the fully qualified type name of the <see cref="T:Xunit.Sdk.BeforeAfterTestAttribute"/>.
		/// </summary>
		public string AttributeName
		{
			get => attributeName ?? throw new InvalidOperationException($"Attempted to get {nameof(AttributeName)} on an uninitialized '{GetType().FullName}' object");
			set => attributeName = Guard.ArgumentNotNull(value, nameof(AttributeName));
		}

		/// <inheritdoc/>
		public override string ToString() =>
			$"{base.ToString()} attr={attributeName.Quoted()}";
	}
}
