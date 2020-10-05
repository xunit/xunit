using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that a line of output was provided for a test.
	/// </summary>
	public class _TestOutput : _TestMessage
	{
		string? output;

		/// <summary>
		/// Gets or sets the line of output.
		/// </summary>
		public string Output
		{
			get => output ?? throw new InvalidOperationException($"Attempted to get {nameof(Output)} on an uninitialized '{GetType().FullName}' object");
			set => output = Guard.ArgumentNotNull(nameof(Output), value);
		}
	}
}
