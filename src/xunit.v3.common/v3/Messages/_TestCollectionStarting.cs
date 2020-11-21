using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that a test collection is about to start executing.
	/// </summary>
	public class _TestCollectionStarting : _TestCollectionMessage, _ITestCollectionMetadata
	{
		string? testCollectionDisplayName;

		/// <inheritdoc/>
		public string? TestCollectionClass { get; set; }

		/// <inheritdoc/>
		public string TestCollectionDisplayName
		{
			get => testCollectionDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestCollectionDisplayName)} on an uninitialized '{GetType().FullName}' object");
			set => testCollectionDisplayName = Guard.ArgumentNotNullOrEmpty(nameof(TestCollectionDisplayName), value);
		}
	}
}
