using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test has failed.
/// </summary>
[JsonTypeID("test-failed")]
public sealed class TestFailed : TestResultMessage, IErrorMetadata
{
	FailureCause? cause;
	int[]? exceptionParentIndices;
	string?[]? exceptionTypes;
	string[]? messages;
	string?[]? stackTraces;

	/// <summary>
	/// Gets or sets the cause of the test failure.
	/// </summary>
	public required FailureCause Cause
	{
		get => this.ValidateNullablePropertyValue(cause, nameof(Cause));
		set => cause = Guard.ArgumentEnumValid(value, [FailureCause.Assertion, FailureCause.Exception, FailureCause.Other, FailureCause.Timeout], nameof(Cause));
	}

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

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		cause = JsonDeserializer.TryGetEnum<FailureCause>(root, nameof(Cause));
		exceptionParentIndices = JsonDeserializer.TryGetArrayOfInt(root, nameof(ExceptionParentIndices));
		exceptionTypes = JsonDeserializer.TryGetArrayOfNullableString(root, nameof(ExceptionTypes));
		messages = JsonDeserializer.TryGetArrayOfString(root, nameof(Messages));
		stackTraces = JsonDeserializer.TryGetArrayOfNullableString(root, nameof(StackTraces));
	}

	/// <summary>
	/// Creates a new <see cref="TestFailed"/> constructed from an <see cref="Exception"/> object.
	/// </summary>
	/// <param name="ex">The exception to use</param>
	/// <param name="assemblyUniqueID">The unique ID of the assembly</param>
	/// <param name="testCollectionUniqueID">The unique ID of the test collectioon</param>
	/// <param name="testClassUniqueID">The (optional) unique ID of the test class</param>
	/// <param name="testMethodUniqueID">The (optional) unique ID of the test method</param>
	/// <param name="testCaseUniqueID">The unique ID of the test case</param>
	/// <param name="testUniqueID">The unique ID of the test</param>
	/// <param name="executionTime">The execution time of the test (may be <c>null</c> if the test wasn't executed)</param>
	/// <param name="output">The (optional) output from the test</param>
	/// <param name="warnings">The (optional) warnings that were recorded during test execution</param>
	public static TestFailed FromException(
		Exception ex,
		string assemblyUniqueID,
		string testCollectionUniqueID,
		string? testClassUniqueID,
		string? testMethodUniqueID,
		string testCaseUniqueID,
		string testUniqueID,
		decimal executionTime,
		string? output,
		string[]? warnings)
	{
		Guard.ArgumentNotNull(ex);
		Guard.ArgumentNotNull(assemblyUniqueID);
		Guard.ArgumentNotNull(testCollectionUniqueID);
		Guard.ArgumentNotNull(testCaseUniqueID);
		Guard.ArgumentNotNull(testUniqueID);

		var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

		return new TestFailed
		{
			AssemblyUniqueID = assemblyUniqueID,
			Cause = errorMetadata.Cause,
			ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
			ExceptionTypes = errorMetadata.ExceptionTypes,
			ExecutionTime = executionTime,
			Messages = errorMetadata.Messages,
			Output = output ?? string.Empty,
			StackTraces = errorMetadata.StackTraces,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestUniqueID = testUniqueID,
			Warnings = warnings,
		};
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(Cause), Cause);
		serializer.SerializeIntArray(nameof(ExceptionParentIndices), ExceptionParentIndices);
		serializer.SerializeStringArray(nameof(ExceptionTypes), ExceptionTypes);
		serializer.SerializeStringArray(nameof(Messages), Messages);
		serializer.SerializeStringArray(nameof(StackTraces), StackTraces);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(cause, nameof(Cause), invalidProperties);
		ValidatePropertyIsNotNull(exceptionParentIndices, nameof(ExceptionParentIndices), invalidProperties);
		ValidatePropertyIsNotNull(exceptionTypes, nameof(ExceptionTypes), invalidProperties);
		ValidatePropertyIsNotNull(messages, nameof(Messages), invalidProperties);
		ValidatePropertyIsNotNull(stackTraces, nameof(StackTraces), invalidProperties);
	}
}
