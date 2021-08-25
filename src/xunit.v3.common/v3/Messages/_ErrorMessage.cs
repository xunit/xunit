using System;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that an error has occurred in the execution process.
	/// </summary>
	public class _ErrorMessage : _MessageSinkMessage, _IErrorMetadata
	{
		int[]? exceptionParentIndices;
		string?[]? exceptionTypes;
		string[]? messages;
		string? details;
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
		public string? Details
		{
			get => details ?? throw new InvalidOperationException($"Attempted to get {nameof(Details)} on an uninitialized '{GetType().FullName} object");
			set => details = Guard.ArgumentNotNull(nameof(Details), value);
		}

		/// <inheritdoc/>
		public string?[] StackTraces
		{
			get => stackTraces ?? throw new InvalidOperationException($"Attempted to get {nameof(StackTraces)} on an uninitialized '{GetType().FullName}' object");
			set => stackTraces = Guard.ArgumentNotNullOrEmpty(nameof(StackTraces), value);
		}

		/// <summary>
		/// Creates a new <see cref="_ErrorMessage"/> constructed from an <see cref="Exception"/> object.
		/// </summary>
		/// <param name="ex">The exception to use</param>
		public static _ErrorMessage FromException(Exception ex)
		{
			Guard.ArgumentNotNull(nameof(ex), ex);

			var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

			return new _ErrorMessage
			{
				ExceptionTypes = errorMetadata.ExceptionTypes,
				Messages = errorMetadata.Messages,
				Details = ex.ToString(),
				StackTraces = errorMetadata.StackTraces,
				ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
			};
		}
	}
}
