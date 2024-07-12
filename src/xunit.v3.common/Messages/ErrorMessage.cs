using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a catastrophic error has occurred.
/// </summary>
[JsonTypeID("error")]
public sealed class ErrorMessage : MessageSinkMessage, IErrorMetadata
{
	int[]? exceptionParentIndices;
	string?[]? exceptionTypes;
	string[]? messages;
	string?[]? stackTraces;

	/// <inheritdoc/>
	public required int[] ExceptionParentIndices
	{
		get => this.ValidateNullablePropertyValue(exceptionParentIndices, nameof(ExceptionParentIndices));
		set => exceptionParentIndices = Guard.ArgumentNotNullOrEmpty(value, nameof(ExceptionParentIndices));
	}

	/// <inheritdoc/>
	public required string?[] ExceptionTypes
	{
		get => this.ValidateNullablePropertyValue(exceptionTypes, nameof(ExceptionTypes));
		set => exceptionTypes = Guard.ArgumentNotNullOrEmpty(value, nameof(ExceptionTypes));
	}

	/// <inheritdoc/>
	public required string[] Messages
	{
		get => this.ValidateNullablePropertyValue(messages, nameof(Messages));
		set => messages = Guard.ArgumentNotNullOrEmpty(value, nameof(Messages));
	}

	/// <inheritdoc/>
	public required string?[] StackTraces
	{
		get => this.ValidateNullablePropertyValue(stackTraces, nameof(StackTraces));
		set => stackTraces = Guard.ArgumentNotNullOrEmpty(value, nameof(StackTraces));
	}

	/// <summary>
	/// Creates a new <see cref="ErrorMessage"/> constructed from an <see cref="Exception"/> object.
	/// </summary>
	/// <param name="ex">The exception to use</param>
	public static ErrorMessage FromException(Exception ex)
	{
		Guard.ArgumentNotNull(ex);

		var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

		return new ErrorMessage
		{
			ExceptionTypes = errorMetadata.ExceptionTypes,
			Messages = errorMetadata.Messages,
			StackTraces = errorMetadata.StackTraces,
			ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
		};
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		// This code is duplicated from JsonObjectSerializerExtensions.DeserializeErrorMetadata because that class
		// is not available here.

		if (JsonDeserializer.TryGetArrayOfInt(root, nameof(IErrorMetadata.ExceptionParentIndices)) is int[] expectedParentIndices)
			ExceptionParentIndices = expectedParentIndices;
		if (JsonDeserializer.TryGetArrayOfNullableString(root, nameof(IErrorMetadata.ExceptionTypes)) is string?[] exceptionTypes)
			ExceptionTypes = exceptionTypes;
		if (JsonDeserializer.TryGetArrayOfString(root, nameof(IErrorMetadata.Messages)) is string?[] messages)
			Messages = messages;
		if (JsonDeserializer.TryGetArrayOfNullableString(root, nameof(IErrorMetadata.StackTraces)) is string?[] stackTraces)
			StackTraces = stackTraces;
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		// This code is duplicated from JsonObjectSerializerExtensions.SerializeErrorMetadata because that class
		// is not available here.

		using (var indexArraySerializer = serializer.SerializeArray(nameof(IErrorMetadata.ExceptionParentIndices)))
			foreach (var index in ExceptionParentIndices)
				indexArraySerializer.Serialize(index);

		using (var typeArraySerializer = serializer.SerializeArray(nameof(IErrorMetadata.ExceptionTypes)))
			foreach (var type in ExceptionTypes)
				typeArraySerializer.Serialize(type);

		using (var messageArraySerializer = serializer.SerializeArray(nameof(IErrorMetadata.Messages)))
			foreach (var message in Messages)
				messageArraySerializer.Serialize(message);

		using (var stackTraceArraySerializer = serializer.SerializeArray(nameof(IErrorMetadata.StackTraces)))
			foreach (var stackTrace in StackTraces)
				stackTraceArraySerializer.Serialize(stackTrace);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(exceptionParentIndices, nameof(ExceptionParentIndices), invalidProperties);
		ValidatePropertyIsNotNull(exceptionTypes, nameof(ExceptionTypes), invalidProperties);
		ValidatePropertyIsNotNull(messages, nameof(Messages), invalidProperties);
		ValidatePropertyIsNotNull(stackTraces, nameof(StackTraces), invalidProperties);
	}
}
