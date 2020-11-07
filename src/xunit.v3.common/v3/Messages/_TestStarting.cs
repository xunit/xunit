using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that a test is about to start executing.
	/// </summary>
	public class _TestStarting : _TestMessage, _ITestMetadata
	{
		string? testDisplayName;

		/// <inheritdoc/>
		public string TestDisplayName
		{
			get => testDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestDisplayName), value);
		}
	}
}
