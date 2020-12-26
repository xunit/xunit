using System;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that an error has occurred during test class cleanup. 
	/// </summary>
	public class _TestClassCleanupFailure : _TestClassMessage, _IErrorMetadata
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
		/// Creates a new <see cref="_TestClassCleanupFailure"/> constructed from an <see cref="Exception"/> object.
		/// </summary>
		/// <param name="ex">The exception to use</param>
		/// <param name="assemblyUniqueID">The unique ID of the assembly</param>
		/// <param name="testCollectionUniqueID">The unique ID of the test collectioon</param>
		/// <param name="testClassUniqueID">The (optional) unique ID of the test class</param>
		public static _TestClassCleanupFailure FromException(
			Exception ex,
			string assemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID)
		{
			Guard.ArgumentNotNull(nameof(ex), ex);
			Guard.ArgumentNotNull(nameof(assemblyUniqueID), assemblyUniqueID);
			Guard.ArgumentNotNull(nameof(testCollectionUniqueID), testCollectionUniqueID);

			var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

			return new _TestClassCleanupFailure
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				ExceptionTypes = errorMetadata.ExceptionTypes,
				Messages = errorMetadata.Messages,
				StackTraces = errorMetadata.StackTraces,
				ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
			};
		}
	}
}
