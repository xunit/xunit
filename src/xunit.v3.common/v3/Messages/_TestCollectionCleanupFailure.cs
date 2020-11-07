using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that an error has occurred during test collection cleanup. 
	/// </summary>
	public class _TestCollectionCleanupFailure : _TestCollectionMessage, _IErrorMetadata
	{
		int[]? exceptionParentIndices;
		string?[]? exceptionTypes;
		string[]? messages;
		string?[]? stackTraces;

		/// <inheritdoc/>
		public int[] ExceptionParentIndices
		{
			get => exceptionParentIndices ?? throw new InvalidOperationException($"Attempted to get {nameof(ExceptionParentIndices)} on an uninitialized '{GetType().FullName}' object");
			set => exceptionParentIndices = Guard.ArgumentNotNullOrEmpty(nameof(ExceptionParentIndices), value);
		}

		/// <inheritdoc/>
		public string?[] ExceptionTypes
		{
			get => exceptionTypes ?? throw new InvalidOperationException($"Attempted to get {nameof(ExceptionTypes)} on an uninitialized '{GetType().FullName}' object");
			set => exceptionTypes = Guard.ArgumentNotNullOrEmpty(nameof(ExceptionTypes), value);
		}

		/// <inheritdoc/>
		public string[] Messages
		{
			get => messages ?? throw new InvalidOperationException($"Attempted to get {nameof(Messages)} on an uninitialized '{GetType().FullName}' object");
			set => messages = Guard.ArgumentNotNullOrEmpty(nameof(Messages), value);
		}

		/// <inheritdoc/>
		public string?[] StackTraces
		{
			get => stackTraces ?? throw new InvalidOperationException($"Attempted to get {nameof(StackTraces)} on an uninitialized '{GetType().FullName}' object");
			set => stackTraces = Guard.ArgumentNotNullOrEmpty(nameof(StackTraces), value);
		}

		/// <summary>
		/// Creates a new <see cref="_TestCollectionCleanupFailure"/> constructed from an <see cref="Exception"/> object.
		/// </summary>
		/// <param name="ex">The exception to use</param>
		/// <param name="testCollectionUniqueID">The unique ID of the test collectioon</param>
		public static _TestCollectionCleanupFailure FromException(
			Exception ex,
			string testCollectionUniqueID)
		{
			Guard.ArgumentNotNull(nameof(ex), ex);
			Guard.ArgumentNotNull(nameof(testCollectionUniqueID), testCollectionUniqueID);

			var failureInfo = ExceptionUtility.ConvertExceptionToErrorMetadata(ex);

			return new _TestCollectionCleanupFailure
			{
				TestCollectionUniqueID = testCollectionUniqueID,
				ExceptionTypes = failureInfo.ExceptionTypes,
				Messages = failureInfo.Messages,
				StackTraces = failureInfo.StackTraces,
				ExceptionParentIndices = failureInfo.ExceptionParentIndices,
			};
		}
	}
}
