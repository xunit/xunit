using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that an error has occurred during test cleanup. 
	/// </summary>
	public class _TestCleanupFailure : _TestMessage, _IErrorMetadata
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
		/// Creates a new <see cref="_TestCleanupFailure"/> constructed from an <see cref="Exception"/> object.
		/// </summary>
		/// <param name="ex">The exception to use</param>
		/// <param name="assemblyUniqueID">The unique ID of the assembly</param>
		/// <param name="testCollectionUniqueID">The unique ID of the test collectioon</param>
		/// <param name="testClassUniqueID">The (optional) unique ID of the test class</param>
		/// <param name="testMethodUniqueID">The (optional) unique ID of the test method</param>
		/// <param name="testCaseUniqueID">The unique ID of the test case</param>
		/// <param name="testUniqueID">The unique ID of the test</param>
		public static _TestCleanupFailure FromException(
			Exception ex,
			string assemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID,
			string? testMethodUniqueID,
			string testCaseUniqueID,
			string testUniqueID)
		{
			Guard.ArgumentNotNull(nameof(ex), ex);
			Guard.ArgumentNotNull(nameof(assemblyUniqueID), assemblyUniqueID);
			Guard.ArgumentNotNull(nameof(testCollectionUniqueID), testCollectionUniqueID);
			Guard.ArgumentNotNull(nameof(testCaseUniqueID), testCaseUniqueID);
			Guard.ArgumentNotNull(nameof(testUniqueID), testUniqueID);

			var failureInfo = ExceptionUtility.ConvertExceptionToErrorMetadata(ex);

			return new _TestCleanupFailure
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestUniqueID = testUniqueID,
				ExceptionTypes = failureInfo.ExceptionTypes,
				Messages = failureInfo.Messages,
				StackTraces = failureInfo.StackTraces,
				ExceptionParentIndices = failureInfo.ExceptionParentIndices,
			};
		}
	}
}
